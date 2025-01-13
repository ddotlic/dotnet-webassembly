using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Intrinsics;
using WebAssembly.Instructions;

namespace WebAssembly;

/// <summary>
/// SIMD operation values (always preceded by a <see cref="OpCode.SimdOperationPrefix"/> value).
/// </summary>
public enum SimdOpCode : byte
{
    /// <summary>
    /// Instantiate a new SIMD vector with 16 8-bit elements.
    /// </summary>
    [OpCodeCharacteristics("v128.const")]
    V128Const = 0x0c,

    /// <summary>
    /// SIMD negate 16 8-bit integers.
    /// </summary>
    [OpCodeCharacteristics("i8x16.neg")]
    [SimdInstructionGenerate<SimdValueOneToOneCallInstruction>()]
    Int8X16Neg = 0x61,

    /// <summary>
    /// SIMD add 16 8-bit integers.
    /// </summary>
    [OpCodeCharacteristics("i8x16.add")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int8X16Add = 0x6e,

    /// <summary>
    /// SIMD subtract 16 8-bit integers.
    /// </summary>
    [OpCodeCharacteristics("i8x16.sub")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int8X16Sub = 0x71,

    /// <summary>
    /// SIMD negate 8 16-bit integers. 
    /// </summary>
    [OpCodeCharacteristics("i16x8.neg")]
    [SimdInstructionGenerate<SimdValueOneToOneCallInstruction>()]
    Int16X8Neg = 0x81,

    /// <summary>
    /// SIMD add 8 16-bit integers. 
    /// </summary>
    [OpCodeCharacteristics("i16x8.add")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int16X8Add = 0x8e,

    /// <summary>
    /// SIMD subtract 8 16-bit integers. 
    /// </summary>
    [OpCodeCharacteristics("i16x8.sub")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int16X8Sub = 0x91,

    /// <summary>
    /// SIMD multiply 8 16-bit integers. 
    /// </summary>
    [OpCodeCharacteristics("i16x8.mul")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int16X8Mul = 0x95,

    /// <summary>
    /// SIMD negate 4 32-bit integers. 
    /// </summary>
    [OpCodeCharacteristics("i32x4.neg")]
    [SimdInstructionGenerate<SimdValueOneToOneCallInstruction>()]
    Int32X4Neg = 0xa1,

    /// <summary>
    /// SIMD add 4 32-bit integers. 
    /// </summary>
    [OpCodeCharacteristics("i32x4.add")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int32X4Add = 0xae,

    /// <summary>
    /// SIMD subtract 4 32-bit integers. 
    /// </summary>
    [OpCodeCharacteristics("i32x4.sub")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int32X4Sub = 0xb1,

    /// <summary>
    /// SIMD multiply 4 32-bit integers. 
    /// </summary>
    [OpCodeCharacteristics("i32x4.mul")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int32X4Mul = 0xb5,

    /// <summary>
    /// SIMD negate 2 64-bit integers.
    /// </summary>
    [OpCodeCharacteristics("i64x2.neg")]
    [SimdInstructionGenerate<SimdValueOneToOneCallInstruction>()]
    Int64X2Neg = 0xc1,

    /// <summary>
    /// SIMD add 2 64-bit integers.
    /// </summary>
    [OpCodeCharacteristics("i64x2.add")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int64X2Add = 0xce,

    /// <summary>
    /// SIMD subtract 2 64-bit integers.
    /// </summary>
    [OpCodeCharacteristics("i64x2.sub")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int64X2Sub = 0xd1,

    /// <summary>
    /// SIMD multiply 2 64-bit integers.
    /// </summary>
    [OpCodeCharacteristics("i64x2.mul")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int64X2Mul = 0xd5,

    /// <summary>
    /// SIMD negate 4 32-bit floats.
    /// </summary>
    [OpCodeCharacteristics("f32x4.neg")]
    [SimdInstructionGenerate<SimdValueOneToOneCallInstruction>()]
    Float32X4Neg = 0xe1,

    /// <summary>
    /// SIMD square root 4 32-bit floats.
    /// </summary>
    [OpCodeCharacteristics("f32x4.sqrt")]
    [SimdInstructionGenerate<SimdValueOneToOneCallInstruction>()]
    Float32X4Sqrt = 0xe3,

    /// <summary>
    /// SIMD add 4 32-bit floats.
    /// </summary>
    [OpCodeCharacteristics("f32x4.add")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Float32X4Add = 0xe4,

    /// <summary>
    /// SIMD subtract 4 32-bit floats.
    /// </summary>
    [OpCodeCharacteristics("f32x4.sub")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Float32X4Sub = 0xe5,

    /// <summary>
    /// SIMD multiply 4 32-bit floats.
    /// </summary>
    [OpCodeCharacteristics("f32x4.mul")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Float32X4Mul = 0xe6,

    /// <summary>
    /// SIMD divide 4 32-bit floats.
    /// </summary>
    [OpCodeCharacteristics("f32x4.div")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Float32X4Div = 0xe7,
}

static class SimdOpCodeExtensions
{
    private static readonly List<KeyValuePair<SimdOpCode, string>> opCodeNativeNames = typeof(SimdOpCode)
        .GetFields()
        .Where(field => field.IsStatic)
        .Select(field => new KeyValuePair<SimdOpCode, string>((SimdOpCode)field.GetValue(null)!, field.GetCustomAttribute<OpCodeCharacteristicsAttribute>()!.Name))
        .ToList();

    private static readonly RegeneratingWeakReference<Dictionary<SimdOpCode, string>> opCodeNativeNamesByOpCode =
        new(() => opCodeNativeNames.ToDictionary(kv => kv.Key, kv => kv.Value));

    public static string ToNativeName(this SimdOpCode opCode)
    {
        opCodeNativeNamesByOpCode.Reference.TryGetValue(opCode, out var result);
        return result!;
    }

    private static MethodInfo FindVector128Method(string name, Type parType, int parsCount, bool isGeneric)
    {
        var methods = typeof(Vector128).GetMethods(BindingFlags.Public | BindingFlags.Static);
        var genericMethodInfo = methods.Where(m => m.Name == name).First(m =>
        {
            var pars = m.GetParameters();
            return pars.Length == parsCount && pars.All(p =>
                isGeneric ? p.ParameterType.IsGenericType && p.ParameterType.GetGenericTypeDefinition() == typeof(Vector128<>)
                    : p.ParameterType == parType);
        });
        return isGeneric ? genericMethodInfo.MakeGenericMethod(parType) : genericMethodInfo;
    }

    internal static void Deconstruct<T>(this IList<T> list, out T first, out T second)
    {
        first = list[0]!;
        second = list[1];
    }

    private static readonly Dictionary<string, (string, int, bool)> opNameToMethodTuple = new() {
        { "const", ("Create", 4, false) },
        { "neg", ("Negate", 1, true) },
        { "add", ("Add", 2, true) },
        { "sub", ("Subtract", 2, true) },
        { "mul", ("Multiply", 2, true) },
        { "sqrt", ("Sqrt", 1, true) },
        { "div", ("Divide", 2, true) },
    };

    private static readonly Dictionary<string, Type> laneTypeToType = new() {
        { "v128", typeof(uint) },
        { "i8x16", typeof(byte) },
        { "i16x8", typeof(ushort) },
        { "i32x4", typeof(uint) },
        { "i64x2", typeof(ulong) },
        { "f32x4", typeof(float) },
    };

    private static readonly RegeneratingWeakReference<Dictionary<SimdOpCode, MethodInfo>> opCodeMethodInfoByOpCode =
        new(() => opCodeNativeNames.ToDictionary(kv => kv.Key, kv =>
        {
            var (laneType, opName) = kv.Value.Split('.');
            var (methodName, parCount, isGeneric) = opNameToMethodTuple[opName];
            return FindVector128Method(methodName, laneTypeToType[laneType], parCount, isGeneric);
        }));

    public static MethodInfo ToMethodInfo(this SimdOpCode opCode)
    {
        opCodeMethodInfoByOpCode.Reference.TryGetValue(opCode, out var result);
        return result!;
    }

}
