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
    /// Load a SIMD vector from memory.
    /// </summary>
    [OpCodeCharacteristics("v128.load")]
    Vec128Load = 0x00,

    /// <summary>
    /// Instantiate a new SIMD vector with 16 8-bit elements.
    /// </summary>
    [OpCodeCharacteristics("v128.const")]
    Vec128Const = 0x0c,

    /// <summary>
    /// SIMD negate 16 8-bit integers.
    /// </summary>
    [OpCodeCharacteristics("i8x16.neg")]
    [SimdInstructionGenerate<SimdValueOneToOneCallInstruction>()]
    Int8X16Neg = 0x61,

    /// <summary>
    /// Return 1 if all 16 8-bit lanes are non-zero, 0 otherwise.
    /// </summary>
    [OpCodeCharacteristics("i8x16.all_true")]
    [SimdInstructionGenerate<Vec128AllTrue>()]
    [SimdOpTraits(hasMethodInfo: false, requiresLaneConversion: true)]
    Int8X16AllTrue = 0x63,

    /// <summary>
    /// Extract the high bit for each of the 16 8-bit lanes and produce a scalar mask with all bits concatenated.
    /// </summary>
    [OpCodeCharacteristics("i8x16.bitmask")]
    [SimdInstructionGenerate<Vec128BitMask>()]
    [SimdOpTraits(hasMethodInfo: true, requiresLaneConversion: true)]
    Int8X16BitMask = 0x64,

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
    /// Return 1 if all 8 16-bit lanes are non-zero, 0 otherwise.
    /// </summary>
    [OpCodeCharacteristics("i16x8.all_true")]
    [SimdInstructionGenerate<Vec128AllTrue>()]
    [SimdOpTraits(hasMethodInfo: false, requiresLaneConversion: true)]
    Int16X8AllTrue = 0x83,

    /// <summary>
    /// Extract the high bit for each of the 8 16-bit lanes and produce a scalar mask with all bits concatenated.
    /// </summary>
    [OpCodeCharacteristics("i16x8.bitmask")]
    [SimdInstructionGenerate<Vec128BitMask>()]
    [SimdOpTraits(hasMethodInfo: true, requiresLaneConversion: true)]
    Int16X8BitMask = 0x84,

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
    /// Return 1 if all 4 32-bit lanes are non-zero, 0 otherwise.
    /// </summary>
    [OpCodeCharacteristics("i32x4.all_true")]
    [SimdInstructionGenerate<Vec128AllTrue>()]
    [SimdOpTraits(hasMethodInfo: false, requiresLaneConversion: true)]
    Int32X4AllTrue = 0xa3,

    /// <summary>
    /// Extract the high bit for each of the 4 32-bit lanes and produce a scalar mask with all bits concatenated.
    /// </summary>
    [OpCodeCharacteristics("i32x4.bitmask")]
    [SimdInstructionGenerate<Vec128BitMask>()]
    [SimdOpTraits(hasMethodInfo: true, requiresLaneConversion: true)]
    Int32X4BitMask = 0xa4,

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
    /// Return 1 if all 2 64-bit lanes are non-zero, 0 otherwise.
    /// </summary>
    [OpCodeCharacteristics("i64x2.all_true")]
    [SimdInstructionGenerate<Vec128AllTrue>()]
    [SimdOpTraits(hasMethodInfo: false, requiresLaneConversion: true)]
    Int64X2AllTrue = 0xc3,

    /// <summary>
    /// Extract the high bit for each of the 2 64-bit lanes and produce a scalar mask with all bits concatenated.
    /// </summary>
    [OpCodeCharacteristics("i64x2.bitmask")]
    [SimdInstructionGenerate<Vec128BitMask>()]
    [SimdOpTraits(hasMethodInfo: true, requiresLaneConversion: true)]
    Int64X2BitMask = 0xc4,

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

    /// <summary>
    /// SIMD negate 2 64-bit floats.
    /// </summary>
    [OpCodeCharacteristics("f64x2.neg")]
    [SimdInstructionGenerate<SimdValueOneToOneCallInstruction>()]
    Float64X2Neg = 0xed,

    /// <summary>
    /// SIMD square root 2 64-bit floats.
    /// </summary>
    [OpCodeCharacteristics("f64x2.sqrt")]
    [SimdInstructionGenerate<SimdValueOneToOneCallInstruction>()]
    Float64X2Sqrt = 0xef,

    /// <summary>
    /// SIMD add 2 64-bit floats.
    /// </summary>
    [OpCodeCharacteristics("f64x2.add")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Float64X2Add = 0xf0,

    /// <summary>
    /// SIMD subtract 2 64-bit floats.
    /// </summary>
    [OpCodeCharacteristics("f64x2.sub")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Float64X2Sub = 0xf1,

    /// <summary>
    /// SIMD multiply 2 64-bit floats.
    /// </summary>
    [OpCodeCharacteristics("f64x2.mul")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Float64X2Mul = 0xf2,

    /// <summary>
    /// SIMD divide 2 64-bit floats.
    /// </summary>
    [OpCodeCharacteristics("f64x2.div")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Float64X2Div = 0xf3,

    /// <summary>
    /// SIMD bitwise not one 128-bit vector.
    /// </summary>
    [OpCodeCharacteristics("v128.not")]
    [SimdInstructionGenerate<SimdValueOneToOneCallInstruction>()]
    Vec128Not = 0x4d,

    /// <summary>
    /// SIMD bitwise and two 128-bit vectorc.
    /// </summary>
    [OpCodeCharacteristics("v128.and")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Vec128And = 0x4e,

    /// <summary>
    /// SIMD bitwise and-not two 128-bit vectorc.
    /// </summary>
    [OpCodeCharacteristics("v128.andnot")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Vec128AndNot = 0x4f,

    /// <summary>
    /// SIMD bitwise or two 128-bit vectorc.
    /// </summary>
    [OpCodeCharacteristics("v128.or")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Vec128Or = 0x50,

    /// <summary>
    /// SIMD bitwise xor two 128-bit vectorc.
    /// </summary>
    [OpCodeCharacteristics("v128.xor")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Vec128Xor = 0x51,

    /// <summary>
    /// SIMD bitselect from two 128-bit vectors, using a 128-bit mask.
    /// </summary>
    [OpCodeCharacteristics("v128.bitselect")]
    Vec128BitSelect = 0x52,

    /// <summary>
    /// Return 1 if any bit is non-zero, 0 otherwise.
    /// </summary>
    [OpCodeCharacteristics("v128.any_true")]
    [SimdOpTraits(hasMethodInfo: false, requiresLaneConversion: false)]
    Vec128AnyTrue = 0x53,
}

internal sealed record OpCodeInfo(SimdOpCode OpCode, string NativeName, bool HasMethodInfo, bool RequiresLaneConversion);

internal static class SimdOpCodeExtensions
{
    private static readonly List<OpCodeInfo> opCodeInfos = typeof(SimdOpCode)
        .GetFields()
        .Where(field => field.IsStatic)
        .Select(field =>
        {
            var opCode = (SimdOpCode)field.GetValue(null)!;
            var name = field.GetCustomAttribute<OpCodeCharacteristicsAttribute>()!.Name;
            var traits = field.GetCustomAttribute<SimdOpTraitsAttribute>() ?? new SimdOpTraitsAttribute();
            return new OpCodeInfo(opCode, name, traits.HasMethodInfo, traits.RequiresLaneConversion);
        })
        .ToList();

    private static readonly RegeneratingWeakReference<Dictionary<SimdOpCode, OpCodeInfo>> opCodeInfoByOpCode =
        new(() => opCodeInfos.ToDictionary(oci => oci.OpCode, oci => oci));

    public static string ToNativeName(this SimdOpCode opCode)
    {
        opCodeInfoByOpCode.Reference.TryGetValue(opCode, out var result);
        return result!.NativeName;
    }

    internal static MethodInfo FindVector128Getter(string name, Type laneType)
    {
        return typeof(Vector128<>).MakeGenericType(laneType)
            .GetProperty(name, BindingFlags.Public | BindingFlags.Static)!.GetGetMethod()!;
    }

    internal static MethodInfo FindVector128Method(string name, Type parType, int parsCount, bool isGeneric)
    {
        var methods = typeof(Vector128).GetMethods(BindingFlags.Public | BindingFlags.Static);
        var genericMethodInfo = methods.Where(m => m.Name == name).First(m =>
        {
            var pars = m.GetParameters();
            return pars.Length == parsCount && pars.Select(p => p.ParameterType).All(pt =>
                isGeneric
                    ? pt.IsPointer || pt.IsByRef
                    || (pt.IsGenericType && pt.GetGenericTypeDefinition() == typeof(Vector128<>))
                    : pt == parType);
        });
        return isGeneric ? genericMethodInfo.MakeGenericMethod(parType) : genericMethodInfo;
    }

    internal static void Deconstruct<T>(this IList<T> list, out T first, out T second)
    {
        first = list[0]!;
        second = list[1];
    }

    private static readonly Dictionary<string, (string, int, bool)> opNameToMethodTuple = new()
    {
        { "load", ("Load", 1, true) },
        { "const", ("Create", 4, false) },
        { "neg", ("Negate", 1, true) },
        { "add", ("Add", 2, true) },
        { "sub", ("Subtract", 2, true) },
        { "mul", ("Multiply", 2, true) },
        { "sqrt", ("Sqrt", 1, true) },
        { "div", ("Divide", 2, true) },
        { "not", ("OnesComplement", 1, true) },
        { "and", ("BitwiseAnd", 2, true) },
        { "andnot", ("AndNot", 2, true) },
        { "or", ("BitwiseOr", 2, true) },
        { "xor", ("Xor", 2, true) },
        { "bitselect", ("ConditionalSelect", 3, true) },
        { "bitmask", ("ExtractMostSignificantBits", 1, true) },
    };

    private static readonly Dictionary<string, Type> laneTypeToType = new()
    {
        { "v128", typeof(uint) },
        { "i8x16", typeof(byte) },
        { "i16x8", typeof(ushort) },
        { "i32x4", typeof(uint) },
        { "i64x2", typeof(ulong) },
        { "f32x4", typeof(float) },
        { "f64x2", typeof(double) },
    };

    private static readonly RegeneratingWeakReference<Dictionary<SimdOpCode, MethodInfo>> opCodeMethodInfoByOpCode =
        new(() => opCodeInfos.Where(oci => oci.HasMethodInfo).ToDictionary(oci => oci.OpCode, oci =>
        {
            var (laneType, opName) = oci.NativeName.Split('.');
            var (methodName, parCount, isGeneric) = opNameToMethodTuple[opName];
            return FindVector128Method(methodName, laneTypeToType[laneType], parCount, isGeneric);
        }));

    public static MethodInfo ToMethodInfo(this SimdOpCode opCode)
    {
        opCodeMethodInfoByOpCode.Reference.TryGetValue(opCode, out var result);
        return result!;
    }

    public static Type ToLaneType(this SimdOpCode opCode)
    {
        var (laneType, _) = opCode.ToNativeName().Split('.');
        return laneTypeToType[laneType];
    }

    public static bool RequiresLaneConversion(this SimdOpCode opCode)
    {
        opCodeInfoByOpCode.Reference.TryGetValue(opCode, out var result);
        return result!.RequiresLaneConversion;
    }
    
    public enum KnownMethodName { Zero, VecEquals, OnesComplement, ExtractMostSignificantBits, ConvertToLaneType }
    private sealed record KnownMethods(MethodInfo Zero, MethodInfo VecEquals, 
        MethodInfo OnesComplement, MethodInfo ExtractMostSignificantBits,
        MethodInfo ConvertToLaneType);
    
    private static readonly RegeneratingWeakReference<Dictionary<string, KnownMethods>> wellKnownMethodsByLane =
        new(() => laneTypeToType.Where(kv => kv.Key.StartsWith('i')).ToDictionary(kv => kv.Key, kv =>
        {
            var laneType = kv.Value;
            var zero = FindVector128Getter("Zero", laneType);
            var equals = FindVector128Method("Equals", laneType, 2, true);
            var onesComplement = FindVector128Method("OnesComplement", laneType, 1, true);
            var extractMsb = FindVector128Method("ExtractMostSignificantBits", laneType, 1, true);
            var convert = typeof(Vector128).GetMethods(BindingFlags.Public | BindingFlags.Static)
                .First(m => m.Name == "As").MakeGenericMethod(typeof(uint), laneType);
            return new KnownMethods(zero, equals, onesComplement, extractMsb, convert);
        }));
        
    public static MethodInfo GetWellKnownMethod(string laneKind, KnownMethodName method)
    {
        wellKnownMethodsByLane.Reference.TryGetValue(laneKind, out var result);
        return method switch
        {
            KnownMethodName.Zero => result!.Zero,
            KnownMethodName.VecEquals => result!.VecEquals,
            KnownMethodName.OnesComplement => result!.OnesComplement,
            KnownMethodName.ExtractMostSignificantBits => result!.ExtractMostSignificantBits,
            KnownMethodName.ConvertToLaneType => result!.ConvertToLaneType,
            _ => throw new InvalidOperationException($"Unexpected known method name: {method}."),
        };
    }
}
