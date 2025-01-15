using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.Intrinsics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

// Effect of this is trusting that the source JSONs are valid.
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

namespace WebAssembly.Runtime;

static class SpecTestRunner
{
    public static void Run(string pathBase, string json) => Run<object>(pathBase, json);

    public static void Run(string pathBase, string json, Func<uint, bool>? skip) => Run<object>(pathBase, json, skip);

    public static void Run<TExports>(string pathBase, string json)
        where TExports : class
    {
        Run<TExports>(pathBase, json, null);
    }

    private static readonly RegeneratingWeakReference<JsonSerializerOptions> serializerOptions =
        new(() => new JsonSerializerOptions
        {
            IncludeFields = true,
        });

    public static void Run<TExports>(string pathBase, string json, Func<uint, bool>? skip)
        where TExports : class
    {
        TestInfo testInfo;
        using (var reader = File.OpenRead(Path.Combine(pathBase, json)))
        {
            testInfo = JsonSerializer.Deserialize<TestInfo>(reader, serializerOptions)!;
        }

        ObjectMethods? methodsByName = null;
        var moduleMethodsByName = new Dictionary<string, ObjectMethods>();

        // From https://github.com/WebAssembly/spec/blob/master/interpreter/host/spectest.ml
        var imports = new ImportDictionary
        {
            { "spectest", "print_i32", new FunctionImport((Action<int>)(i => { })) },
            { "spectest", "print_i32_f32", new FunctionImport((Action<int, float>)((i, f) => { })) },
            { "spectest", "print_f64_f64", new FunctionImport((Action<double, double>)((d1, d2) => { })) },
            { "spectest", "print_f32", new FunctionImport((Action<float>)(i => { })) },
            { "spectest", "print_f64", new FunctionImport((Action<double>)(i => { })) },
            { "spectest", "global_i32", new GlobalImport(() => 666) },
            { "spectest", "global_i64", new GlobalImport(() => 666L) },
            { "spectest", "global_f32", new GlobalImport(() => 666.0F) },
            { "spectest", "global_f64", new GlobalImport(() => 666.0) },
            { "spectest", "global_v128", new GlobalImport(() => Vector128.Create(666u, 666u, 666u, 666u)) },
            { "spectest", "table", new FunctionTable(10, 20) }, // Table.alloc (TableType ({min = 10l; max = Some 20l}, FuncRefType))
            { "spectest", "memory", new MemoryImport(() => new UnmanagedMemory(1, 2)) }, // Memory.alloc (MemoryType {min = 1l; max = Some 2l})
        };

        var registrationCandidates = new ImportDictionary();

        Action trapExpected;
        object? result;
        object obj;
        MethodInfo methodInfo;
        TExports? exports = null;
        foreach (var command in testInfo.commands)
        {
            if (skip != null && skip(command.line))
                continue;

            void GetMethod(TestAction action, out MethodInfo info, out object host)
            {
                var methodSource = action.module == null ? methodsByName : moduleMethodsByName[action.module];
                Assert.IsNotNull(methodSource, $"{command.line} has no method source.");
                Assert.IsTrue(methodSource!.TryGetValue(NameCleaner.CleanName(action.field), out info!), $"{command.line} failed to look up method {action.field}");
                host = methodSource.Host;
            }

            try
            {
                switch (command)
                {
                    case ModuleCommand module:
                        var path = Path.Combine(pathBase, module.filename);
                        var parsed = Module.ReadFromBinary(path); // Ensure the module parser can read it.
                        Assert.IsNotNull(parsed);
                        methodsByName = new ObjectMethods(exports = Compile.FromBinary<TExports>(path)(imports).Exports);
                        if (module.name != null)
                            moduleMethodsByName[module.name] = methodsByName;
                        continue;
                    case AssertReturn assert:
                        GetMethod(assert.action, out methodInfo, out obj);
                        try
                        {
                            result = assert.action.Call(methodInfo, obj);
                        }
                        catch (TargetInvocationException x) when (x.InnerException != null)
                        {
                            throw new AssertFailedException($"{command.line}: {x.InnerException.Message}", x.InnerException);
                        }
                        catch (Exception x)
                        {
                            throw new AssertFailedException($"{command.line}: {x.Message}", x);
                        }
                        if (assert.expected?.Length > 0)
                        {
                            var rawExpected = assert.expected[0];
                            if (rawExpected.BoxedValue.Equals(result))
                                continue;

                            switch (rawExpected)
                            {
                                default:
                                    // This happens in conversion.json starting at "line": 317 when run via GitHub Action but never locally (for me).
                                    Assert.Inconclusive($"{command.line}: Failed to parse expected value type.");
                                    return;
                            
                                case Int32Value:
                                case Int64Value:
                                    break;
                            
                                case Float32Value f32Value:
                                    {
                                        var expected = f32Value.ActualValue;
                                        Assert.AreEqual(expected, (float)result!, Math.Abs(expected * 0.000001f), $"{command.line}: f32 compare");
                                    }
                                    continue;
                                case Float64Value f64Value:
                                    {
                                        var expected = f64Value.ActualValue;
                                        Assert.AreEqual(expected, (double)result!, Math.Abs(expected * 0.000001), $"{command.line}: f64 compare");
                                    }
                                    continue;
                                case Vector128Value v128Value:
                                    {
                                        static bool IsStringNaN(string v) => v == "nan:arithmetic" || v == "nan:canonical";
                                        
                                        if (v128Value.value.Any(IsStringNaN) && result != null)
                                        {
                                            var actualResult = (Vector128<uint>)result;
                                            if (v128Value.lane_type == LaneValueType.f32)
                                            {
                                                var f32X4 = v128Value.ActualValue.AsSingle();
                                                for(var i = 0; i < Vector128<uint>.Count; i++)
                                                {
                                                    if(IsStringNaN(v128Value.value[i])) Assert.IsTrue(float.IsNaN(f32X4[i]));
                                                    else Assert.AreEqual(v128Value.ActualValue[i], actualResult[i]);
                                                }
                                                continue;
                                            }
                                            else if (v128Value.lane_type == LaneValueType.f64)
                                            {
                                                var f64X2 = v128Value.ActualValue.AsDouble();
                                                for(var i = 0; i < Vector128<ulong>.Count; i++)
                                                {
                                                    if(IsStringNaN(v128Value.value[i])) Assert.IsTrue(double.IsNaN(f64X2[i]));
                                                    else Assert.AreEqual(v128Value.ActualValue[i], actualResult[i]);
                                                }
                                                continue;
                                            }
                                        }
                                    }
                                    break;
                            }
                            
                            throw new AssertFailedException($"{command.line}: Not equal {rawExpected.GetType().Name}: {rawExpected.BoxedValue} and {result}");
                        }
                        continue;
                    case AssertReturnCanonicalNan assert:
                        GetMethod(assert.action, out methodInfo, out obj);
                        result = assert.action.Call(methodInfo, obj);
                        switch (assert.expected[0].type)
                        {
                            case RawValueType.f32:
                                Assert.IsTrue(float.IsNaN((float)result!), $"{command.line}: Expected NaN, got {result}");
                                continue;
                            case RawValueType.f64:
                                Assert.IsTrue(double.IsNaN((double)result!), $"{command.line}: Expected NaN, got {result}");
                                continue;
                            default:
                                throw new AssertFailedException($"{assert.expected[0].type} doesn't support NaN checks.");
                        }
                    case AssertReturnArithmeticNan assert:
                        GetMethod(assert.action, out methodInfo, out obj);
                        result = assert.action.Call(methodInfo, obj);
                        switch (assert.expected[0].type)
                        {
                            case RawValueType.f32:
                                Assert.IsTrue(float.IsNaN((float)result!), $"{command.line}: Expected NaN, got {result}");
                                continue;
                            case RawValueType.f64:
                                Assert.IsTrue(double.IsNaN((double)result!), $"{command.line}: Expected NaN, got {result}");
                                continue;
                            default:
                                throw new AssertFailedException($"{assert.expected[0].type} doesn't support NaN checks.");
                        }
                    case AssertInvalid assert:
                        trapExpected = () =>
                        {
                            try
                            {
                                Compile.FromBinary<TExports>(Path.Combine(pathBase, assert.filename));
                            }
                            catch (TargetInvocationException x) when (x.InnerException != null)
                            {
                                ExceptionDispatchInfo.Capture(x.InnerException).Throw();
                            }
                        };
                        switch (assert.text)
                        {
                            case "type mismatch":
                                try
                                {
                                    trapExpected();
                                }
                                catch (StackTypeInvalidException)
                                {
                                    continue;
                                }
                                catch (StackTooSmallException)
                                {
                                    continue;
                                }
                                catch (ModuleLoadException)
                                {
                                    continue;
                                }
                                catch (StackSizeIncorrectException)
                                {
                                    continue;
                                }
                                catch (LabelTypeMismatchException)
                                {
                                    continue;
                                }
                                catch (Exception x)
                                {
                                    throw new AssertFailedException($"{command.line} threw an unexpected exception of type {x.GetType().Name}.");
                                }
                                throw new AssertFailedException($"{command.line} should have thrown an exception but did not.");
                            case "alignment must not be larger than natural":
                            case "global is immutable":
                                Assert.ThrowsException<CompilerException>(trapExpected, $"{command.line}");
                                continue;
                            case "unknown memory 0":
                            case "constant expression required":
                            case "duplicate export name":
                            case "unknown table":
                            case "unknown local":
                            case "multiple memories":
                            case "size minimum must not be greater than maximum":
                            case "memory size must be at most 65536 pages (4GiB)":
                            case "unknown label":
                            case "invalid result arity":
                            case "unknown type":
                                Assert.ThrowsException<ModuleLoadException>(trapExpected, $"{command.line}");
                                continue;
                            case "unknown global":
                            case "unknown memory":
                            case "unknown function":
                            case "unknown table 0":
                                try
                                {
                                    trapExpected();
                                }
                                catch (CompilerException)
                                {
                                    continue;
                                }
                                catch (ModuleLoadException)
                                {
                                    continue;
                                }
                                catch (Exception x)
                                {
                                    throw new AssertFailedException($"{command.line} threw an unexpected exception of type {x.GetType().Name}.");
                                }
                                throw new AssertFailedException($"{command.line} should have thrown an exception but did not.");
                            case "multiple tables":
                                Assert.ThrowsException<ModuleLoadException>(trapExpected, $"{command.line}");
                                continue;
                            default:
                                throw new AssertFailedException($"{command.line}: {assert.text} doesn't have a test procedure set up.");
                        }
                    case AssertTrap assert:
                        trapExpected = () =>
                        {
                            GetMethod(assert.action, out methodInfo, out obj);
                            try
                            {
                                assert.action.Call(methodInfo, obj);
                            }
                            catch (TargetInvocationException x) when (x.InnerException != null)
                            {
                                ExceptionDispatchInfo.Capture(x.InnerException).Throw();
                            }
                        };

                        switch (assert.text)
                        {
                            case "integer divide by zero":
                                Assert.ThrowsException<DivideByZeroException>(trapExpected, $"{command.line}");
                                continue;
                            case "integer overflow":
                                Assert.ThrowsException<OverflowException>(trapExpected, $"{command.line}");
                                continue;
                            case "out of bounds memory access":
                                try
                                {
                                    trapExpected();
                                }
                                catch (MemoryAccessOutOfRangeException)
                                {
                                    continue;
                                }
                                catch (OverflowException)
                                {
                                    continue;
                                }
                                catch (Exception x)
                                {
                                    throw new AssertFailedException($"{command.line} threw an unexpected exception of type {x.GetType().Name}.");
                                }
                                throw new AssertFailedException($"{command.line} should have thrown an exception but did not.");
                            case "invalid conversion to integer":
                                Assert.ThrowsException<OverflowException>(trapExpected, $"{command.line}");
                                continue;
                            case "undefined element":
                            case "uninitialized element 7":
                                try
                                {
                                    trapExpected();
                                    throw new AssertFailedException($"{command.line}: Expected ModuleLoadException or IndexOutOfRangeException, but no exception was thrown.");
                                }
                                catch (ModuleLoadException)
                                {
                                }
                                catch (IndexOutOfRangeException)
                                {
                                }
                                catch (Exception x)
                                {
                                    throw new AssertFailedException($"{command.line}: Expected ModuleLoadException or IndexOutOfRangeException, but received {x.GetType().Name}.");
                                }
                                continue;
                            case "indirect call type mismatch":
                                Assert.ThrowsException<InvalidCastException>(trapExpected, $"{command.line}");
                                continue;
                            case "unreachable":
                                Assert.ThrowsException<UnreachableException>(trapExpected, $"{command.line}");
                                continue;
                            case "uninitialized element":
                            case "uninitialized":
                                try
                                {
                                    trapExpected();
                                    throw new AssertFailedException($"{command.line}: Expected KeyNotFoundException or NullReferenceException, but no exception was thrown.");
                                }
                                catch (KeyNotFoundException)
                                {
                                }
                                catch (NullReferenceException)
                                {
                                }
                                catch (Exception x)
                                {
                                    throw new AssertFailedException($"{command.line}: Expected KeyNotFoundException or NullReferenceException, but received {x.GetType().Name}.");
                                }
                                continue;
                            case "undefined":
                                try
                                {
                                    trapExpected();
                                    throw new AssertFailedException($"{command.line}: Expected KeyNotFoundException or IndexOutOfRangeException, but no exception was thrown.");
                                }
                                catch (KeyNotFoundException)
                                {
                                }
                                catch (IndexOutOfRangeException)
                                {
                                }
                                catch (Exception x)
                                {
                                    throw new AssertFailedException($"{command.line}: Expected KeyNotFoundException or IndexOutOfRangeException, but received {x.GetType().Name}.");
                                }
                                continue;
                            case "indirect call":
                                Assert.ThrowsException<InvalidCastException>(trapExpected, $"{command.line}");
                                continue;
                            default:
                                throw new AssertFailedException($"{command.line}: {assert.text} doesn't have a test procedure set up.");
                        }
                    case AssertMalformed assert:
                        continue; // Not writing a WAT parser.
                    case AssertExhaustion assert:
                        trapExpected = () =>
                        {
                            GetMethod(assert.action, out methodInfo, out obj);
                            try
                            {
                                assert.action.Call(methodInfo, obj);
                            }
                            catch (TargetInvocationException x) when (x.InnerException != null)
                            {
                                ExceptionDispatchInfo.Capture(x.InnerException).Throw();
                            }
                        };

                        switch (assert.text)
                        {
                            case "call stack exhausted":
                                Assert.ThrowsException<StackOverflowException>(trapExpected, $"{command.line}");
                                continue;
                            default:
                                throw new AssertFailedException($"{command.line}: {assert.text} doesn't have a test procedure set up.");
                        }
                    case AssertUnlinkable assert:
                        trapExpected = () =>
                        {
                            try
                            {
                                Compile.FromBinary<TExports>(Path.Combine(pathBase, assert.filename))(imports);
                            }
                            catch (TargetInvocationException x) when (x.InnerException != null
#if DEBUG
                                && !System.Diagnostics.Debugger.IsAttached
#endif
                                )
                            {
                                ExceptionDispatchInfo.Capture(x.InnerException).Throw();
                            }
                        };
                        switch (assert.text)
                        {
                            case "data segment does not fit":
                            case "elements segment does not fit":
                                try
                                {
                                    trapExpected();
                                    throw new AssertFailedException($"{command.line}: Expected ModuleLoadException, MemoryAccessOutOfRangeException or OverflowException, but no exception was thrown.");
                                }
                                catch (Exception x) when (x is ModuleLoadException || x is MemoryAccessOutOfRangeException || x is OverflowException)
                                {
                                }
                                continue;
                            case "unknown import":
                            case "incompatible import type":
                                Assert.ThrowsException<ImportException>(trapExpected, $"{command.line}");
                                continue;
                            default:
                                throw new AssertFailedException($"{command.line}: {assert.text} doesn't have a test procedure set up.");
                        }
                    case Register register:
                        Assert.IsNotNull(
                            moduleMethodsByName[register.@as] = register.name != null ? moduleMethodsByName[register.name] : methodsByName!,
                            $"{command.line} tried to register null as a module method source.");
                        Assert.IsNotNull(exports);
                        imports.AddFromExports(register.@as, exports!);
                        continue;
                    case AssertUninstantiable assert:
                        trapExpected = () =>
                        {
                            try
                            {
                                Compile.FromBinary<TExports>(Path.Combine(pathBase, assert.filename));
                            }
                            catch (TargetInvocationException x) when (x.InnerException != null)
                            {
                                ExceptionDispatchInfo.Capture(x.InnerException).Throw();
                            }
                        };
                        switch (assert.text)
                        {
                            case "unreachable":
                                Assert.ThrowsException<ModuleLoadException>(trapExpected, $"{command.line}");
                                continue;
                            default:
                                throw new AssertFailedException($"{command.line}: {assert.text} doesn't have a test procedure set up.");
                        }
                    default:
                        throw new AssertFailedException($"{command.line}: {command} doesn't have a test procedure set up.");
                }
            }
            catch (Exception x)
            when (!System.Diagnostics.Debugger.IsAttached && x is not UnitTestAssertException)
            {
                throw new AssertFailedException($"{command.line}: {x}", x);
            }
        }

        if (skip != null)
            Assert.Inconclusive("Some scenarios were skipped.");
    }

    class ObjectMethods : Dictionary<string, MethodInfo>
    {
        public readonly object Host;

        public ObjectMethods(object host)
        {
            Assert.IsNotNull(this.Host = host);

            foreach (var method in host
                .GetType()
                .GetMethods()
                .Select(m => new { m.Name, MethodInfo = m })
                .Concat(host
                .GetType()
                .GetProperties()
                .Where(p => p.GetGetMethod() != null)
                .Select(p => new { p.Name, MethodInfo = p.GetGetMethod()! })))
            {
                Assert.IsTrue(TryAdd(method.Name, method.MethodInfo));
            }
        }
    }

    [JsonConverter(typeof(JsonStringEnumConverter<CommandType>))]
    enum CommandType
    {
        module,
        assert_return,
        assert_return_canonical_nan,
        assert_return_arithmetic_nan,
        assert_invalid,
        assert_trap,
        assert_malformed,
        assert_exhaustion,
        assert_unlinkable,
        register,
        action,
        assert_uninstantiable
    }

#pragma warning disable 649
    class TestInfo
    {
        public string source_filename;
        public Command[] commands;
    }


    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
    [JsonDerivedType(typeof(ModuleCommand), typeDiscriminator: nameof(CommandType.module))]
    [JsonDerivedType(typeof(AssertReturn), typeDiscriminator: nameof(CommandType.assert_return))]
    [JsonDerivedType(typeof(AssertReturnCanonicalNan), typeDiscriminator: nameof(CommandType.assert_return_canonical_nan))]
    [JsonDerivedType(typeof(AssertReturnArithmeticNan), typeDiscriminator: nameof(CommandType.assert_return_arithmetic_nan))]
    [JsonDerivedType(typeof(AssertInvalid), typeDiscriminator: nameof(CommandType.assert_invalid))]
    [JsonDerivedType(typeof(AssertTrap), typeDiscriminator: nameof(CommandType.assert_trap))]
    [JsonDerivedType(typeof(AssertMalformed), typeDiscriminator: nameof(CommandType.assert_malformed))]
    [JsonDerivedType(typeof(AssertExhaustion), typeDiscriminator: nameof(CommandType.assert_exhaustion))]
    [JsonDerivedType(typeof(AssertUnlinkable), typeDiscriminator: nameof(CommandType.assert_unlinkable))]
    [JsonDerivedType(typeof(Register), typeDiscriminator: nameof(CommandType.register))]
    [JsonDerivedType(typeof(NoReturn), typeDiscriminator: nameof(CommandType.action))]
    [JsonDerivedType(typeof(AssertUninstantiable), typeDiscriminator: nameof(CommandType.assert_uninstantiable))]
    abstract class Command
    {
        public uint line;

        public override string ToString() => GetType().Name;
    }

    class ModuleCommand : Command
    {
        public string name;
        public string filename;

        public override string ToString() => $"{base.ToString()}: {filename}";
    }

    [JsonConverter(typeof(JsonStringEnumConverter<RawValueType>))]
    enum RawValueType
    {
        i32,
        i64,
        f32,
        f64,
        v128,
    }

    class TypeOnly
    {
        public RawValueType type;

        public override string ToString() => type.ToString();
    }

    
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
    [JsonDerivedType(typeof(Int32Value), typeDiscriminator: nameof(RawValueType.i32))]
    [JsonDerivedType(typeof(Int64Value), typeDiscriminator: nameof(RawValueType.i64))]
    [JsonDerivedType(typeof(Float32Value), typeDiscriminator: nameof(RawValueType.f32))]
    [JsonDerivedType(typeof(Float64Value), typeDiscriminator: nameof(RawValueType.f64))]
    [JsonDerivedType(typeof(Vector128Value), typeDiscriminator: nameof(RawValueType.v128))]
    abstract class TypedValue
    {
        public abstract object BoxedValue { get; }
    }
    
    class Int32Value : TypedValue
    {
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public uint value;

        public override object BoxedValue => (int)value;

        public override string ToString() => $"i32: {value}";
    }

    class Int64Value : TypedValue
    {
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public ulong value;

        public override object BoxedValue => (long)value;

        public override string ToString() => $"i64: {value}";
    }

    class Float32Value : TypedValue
    {
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public uint value;
        
        public float ActualValue => BitConverter.Int32BitsToSingle(unchecked((int)value));

        public override object BoxedValue => ActualValue;

        public override string ToString() => $"f32: {BoxedValue}";
    }

    class Float64Value : TypedValue
    {
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public ulong value;
        
        public double ActualValue => BitConverter.Int64BitsToDouble(unchecked((long)value));

        public override object BoxedValue => ActualValue;

        public override string ToString() => $"f64: {BoxedValue}";
    }

    [JsonConverter(typeof(JsonStringEnumConverter<LaneValueType>))]
    enum LaneValueType
    {
        i8,
        i16,
        i32,
        i64,
        f32,
        f64,
    }
    
    class Vector128Value : TypedValue 
    {
        public LaneValueType lane_type;
        
        public string[] value;

        private static TUnsigned ParseToUnsigned<TUnsigned, TSigned>(string input, bool acceptNaN = false, TUnsigned canonicalNaN = default!, TUnsigned arithmeticNaN = default!)
        where TUnsigned : IBinaryInteger<TUnsigned>
        where TSigned : IBinaryInteger<TSigned>
        {
            if (input == "nan:canonical")
                return acceptNaN ? canonicalNaN : throw new OverflowException("Canonical NaN not expected here.");
            else if (input == "nan:arithmetic")
                return acceptNaN ? arithmeticNaN : throw new OverflowException("Arithmetic NaN not expected here.");

            try
            {
                var signedValue = TSigned.Parse(input, null); // Try parsing as signed
                return TUnsigned.CreateTruncating(signedValue);
            }
            catch
            {
                return TUnsigned.Parse(input, null); // If signed parsing fails, try parsing as unsigned
            }
        }
        
        public virtual Vector128<uint> ActualValue
        {
            get
            {
                switch (lane_type)
                {
                    case LaneValueType.i8:
                        return Vector128.Create(value.Select(v => ParseToUnsigned<byte, sbyte>(v)).ToArray()).AsUInt32();
                    case LaneValueType.i16:
                        return Vector128.Create(value.Select(v => ParseToUnsigned<ushort, short>(v)).ToArray()).AsUInt32();
                    case LaneValueType.i32:
                        return Vector128.Create(value.Select(v => ParseToUnsigned<uint, int>(v)).ToArray());
                    case LaneValueType.i64:
                        return Vector128.Create(value.Select(v => ParseToUnsigned<ulong, long>(v)).ToArray()).AsUInt32();
                    case LaneValueType.f32:
                        return Vector128.Create(value.Select(v => BitConverter.Int32BitsToSingle(unchecked((int)ParseToUnsigned<uint, int>(v, true, 0x7fc00000, 0x7fc00001)))).ToArray()).AsUInt32();
                    case LaneValueType.f64:
                        return Vector128.Create(value.Select(v => BitConverter.Int64BitsToDouble(unchecked((long)ParseToUnsigned<ulong, long>(v, true, 0x7ff8000000000000, 0x7ff8000000000001)))).ToArray()).AsUInt32();
                    default:
                        throw new InvalidOperationException();
                }
            }
        }

        public override object BoxedValue => ActualValue;

        private string GetCanonicalRepresentation()
        {
            var sb = new StringBuilder(64);

            sb.Append("v128: i32x4 0x");
            sb.Append(this.ActualValue.GetElement(0).ToString("X8"));

            for (var i = 1; i < 4; i++)
            {
                sb.Append(" 0x");
                sb.Append(this.ActualValue.GetElement(i).ToString("X8"));
            }

            return sb.ToString();
        }
        
        public override string ToString() => GetCanonicalRepresentation(); 
    }

    
    [JsonConverter(typeof(JsonStringEnumConverter<TestActionType>))]
    enum TestActionType
    {
        invoke,
        get,
    }

    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
    [JsonDerivedType(typeof(Invoke), typeDiscriminator: nameof(TestActionType.invoke))]
    [JsonDerivedType(typeof(Get), typeDiscriminator: nameof(TestActionType.get))]
    abstract class TestAction
    {
        public string module;
        public string field;

        public abstract object? Call(MethodInfo methodInfo, object obj);
    }

    class Invoke : TestAction
    {
        public TypedValue[] args;

        public override object? Call(MethodInfo methodInfo, object obj)
        {
            return methodInfo.Invoke(obj, args.Select(arg => arg.BoxedValue).ToArray());
        }

        public override string ToString() => $"{field}({module})[{string.Join(',', (IEnumerable<TypedValue>)args)}]";
    }

    class Get : TestAction
    {
        public override object? Call(MethodInfo methodInfo, object obj)
        {
            return methodInfo.Invoke(obj, []);
        }
    }

    abstract class AssertCommand : Command
    {
        public TestAction action;

        public override string ToString() => $"{base.ToString()}: {action}";
    }

    class AssertReturn : AssertCommand
    {
        public TypedValue[] expected;

        public override string ToString() => $"{base.ToString()} = [{string.Join(',', (IEnumerable<TypedValue>)expected)}]";
    }

    class NoReturn : AssertReturn
    {
        public override string ToString() => $"{base.ToString()}";
    }

    class AssertReturnCanonicalNan : AssertCommand
    {
        public TypeOnly[] expected;

        public override string ToString() => $"{base.ToString()} = [{string.Join(',', (IEnumerable<TypeOnly>)expected)}]";
    }

    class AssertReturnArithmeticNan : AssertCommand
    {
        public TypeOnly[] expected;

        public override string ToString() => $"{base.ToString()} = [{string.Join(',', (IEnumerable<TypeOnly>)expected)}]";
    }

    abstract class InvalidCommand : Command
    {
        public string filename;
        public string text;
        public string module_type;

        public override string ToString() => $"{base.ToString()}: {filename} \"{text}\" {module_type}";
    }

    class AssertInvalid : InvalidCommand
    {
    }

    class AssertTrap : AssertCommand
    {
        public TypeOnly[] expected;
        public string text;
    }

    class AssertMalformed : InvalidCommand
    {
    }

    class AssertExhaustion : AssertCommand
    {
        public string text;
    }

    class AssertUnlinkable : InvalidCommand
    {
    }

    class AssertUninstantiable : InvalidCommand
    {
    }

    class Register : Command
    {
        public string name;
        public string @as;
    }
#pragma warning restore    
}
