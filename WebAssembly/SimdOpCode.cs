using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
    /// SIMD negate 4 32-bit integers. 
    /// </summary>
    [OpCodeCharacteristics("i32x4.neg")]
    Int32X4Neg = 0xa1,

    /// <summary>
    /// SIMD add 4 32-bit integers. 
    /// </summary>
    [OpCodeCharacteristics("i32x4.add")]
    Int32X4Add = 0xae,

    /// <summary>
    /// SIMD subtract 4 32-bit integers. 
    /// </summary>
    [OpCodeCharacteristics("i32x4.sub")]
    Int32X4Sub = 0xb1,

    /// <summary>
    /// SIMD multiply 4 32-bit integers. 
    /// </summary>
    [OpCodeCharacteristics("i32x4.mul")]
    Int32X4Mul = 0xb5,

    /// <summary>
    /// SIMD negate 2 64-bit integers.
    /// </summary>
    [OpCodeCharacteristics("i64x2.neg")]
    Int64X2Neg = 0xc1,

    /// <summary>
    /// SIMD add 2 64-bit integers.
    /// </summary>
    [OpCodeCharacteristics("i64x2.add")]
    Int64X2Add = 0xce,

    /// <summary>
    /// SIMD subtract 2 64-bit integers.
    /// </summary>
    [OpCodeCharacteristics("i64x2.sub")]
    Int64X2Sub = 0xd1,

    /// <summary>
    /// SIMD multiply 2 64-bit integers.
    /// </summary>
    [OpCodeCharacteristics("i64x2.mul")]
    Int64X2Mul = 0xd5,

}

static class SimdOpCodeExtensions
{
    private static readonly RegeneratingWeakReference<Dictionary<SimdOpCode, string>> opCodeNativeNamesByOpCode = new(
        () => typeof(SimdOpCode)
            .GetFields()
            .Where(field => field.IsStatic)
            .Select(field => new KeyValuePair<SimdOpCode, string>((SimdOpCode)field.GetValue(null)!, field.GetCustomAttribute<OpCodeCharacteristicsAttribute>()!.Name))
            .ToDictionary(kv => kv.Key, kv => kv.Value)
    );

    public static string ToNativeName(this SimdOpCode opCode)
    {
        opCodeNativeNamesByOpCode.Reference.TryGetValue(opCode, out var result);
        return result!;
    }
}
