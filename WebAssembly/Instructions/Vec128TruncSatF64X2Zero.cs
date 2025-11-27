using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;
using static WebAssembly.SimdOpCodeExtensions;

namespace WebAssembly.Instructions;

/// <summary>
/// Implements i32x4.trunc_sat_f64x2_s_zero and i32x4.trunc_sat_f64x2_u_zero.
/// Converts f64x2 to i32x4 with saturation at i32 bounds, zero-extending upper lanes.
/// </summary>
public abstract class Vec128TruncSatF64X2Zero : SimdInstruction
{
    private protected Vec128TruncSatF64X2Zero() { }

    internal override void Compile(CompilationContext context)
    {
        context.PopStackNoReturn(OpCode, WebAssemblyValueType.Vector128);
        context.Stack.Push(WebAssemblyValueType.Vector128);

        var isSigned = SimdOpCode == SimdOpCode.Int32X4TruncSatF64X2SignedZero;

        if (isSigned)
            CompileSigned(context);
        else
            CompileUnsigned(context);
    }

    private static void CompileSigned(CompilationContext context)
    {
        // Stack: [f64x2]
        // 1. ConvertToInt64 -> [i64x2]
        // 2. Clamp to i32 bounds with Max/Min
        // 3. Narrow with zero -> [i32x4]

        var convertToInt64 = FindVector128Method("ConvertToInt64", typeof(double), 1, true);
        var maxLong = FindVector128Method("Max", typeof(long), 2, true);
        var minLong = FindVector128Method("Min", typeof(long), 2, true);
        var narrowInt = FindVector128Method("Narrow", typeof(long), 2, false);
        var createLong = FindVector128Method("Create", typeof(long), 1, false);
        var zeroLong = FindVector128Getter("Zero", typeof(long));

        // Stack: [f64x2] -> [i64x2]
        context.Emit(OpCodes.Call, convertToInt64);

        // Stack: [i64x2] -> [i64x2, minVec] -> [clamped_min]
        context.Emit(OpCodes.Ldc_I8, (long)int.MinValue);
        context.Emit(OpCodes.Call, createLong);
        context.Emit(OpCodes.Call, maxLong);

        // Stack: [clamped_min] -> [clamped_min, maxVec] -> [clamped]
        context.Emit(OpCodes.Ldc_I8, (long)int.MaxValue);
        context.Emit(OpCodes.Call, createLong);
        context.Emit(OpCodes.Call, minLong);

        // Stack: [clamped] -> [clamped, zero] -> [i32x4]
        context.Emit(OpCodes.Call, zeroLong);
        context.Emit(OpCodes.Call, narrowInt);
    }

    private static void CompileUnsigned(CompilationContext context)
    {
        // Stack: [f64x2]
        // 1. ConvertToUInt64 -> [u64x2]
        // 2. Clamp to u32 max with Min
        // 3. Narrow with zero -> [u32x4]

        var convertToUInt64 = FindVector128Method("ConvertToUInt64", typeof(double), 1, true);
        var minULong = FindVector128Method("Min", typeof(ulong), 2, true);
        var narrowUInt = FindVector128Method("Narrow", typeof(ulong), 2, false);
        var createULong = FindVector128Method("Create", typeof(ulong), 1, false);
        var zeroULong = FindVector128Getter("Zero", typeof(ulong));

        // Stack: [f64x2] -> [u64x2]
        context.Emit(OpCodes.Call, convertToUInt64);

        // Stack: [u64x2] -> [u64x2, maxVec] -> [clamped]
        context.Emit(OpCodes.Ldc_I8, unchecked((long)uint.MaxValue));
        context.Emit(OpCodes.Call, createULong);
        context.Emit(OpCodes.Call, minULong);

        // Stack: [clamped] -> [clamped, zero] -> [u32x4]
        context.Emit(OpCodes.Call, zeroULong);
        context.Emit(OpCodes.Call, narrowUInt);
    }
}
