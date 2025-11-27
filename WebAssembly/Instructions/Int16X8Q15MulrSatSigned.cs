using System.Reflection.Emit;
using System.Runtime.Intrinsics;
using WebAssembly.Runtime.Compilation;
using static WebAssembly.SimdOpCodeExtensions;

namespace WebAssembly.Instructions;

/// <summary>
/// Implements i16x8.q15mulr_sat_s: Q15 fixed-point multiply with rounding and saturation.
/// Formula: saturate((a * b + 16384) >> 15)
/// </summary>
public sealed class Int16X8Q15MulrSatSigned : SimdInstruction
{
    /// <summary>
    /// Creates a new <see cref="Int16X8Q15MulrSatSigned"/> instance. 
    /// </summary>
    public Int16X8Q15MulrSatSigned() { }

    /// <summary>
    /// Always <see cref="SimdOpCode.Int16X8Q15MulrSatSigned"/>.
    /// </summary>
    public override SimdOpCode SimdOpCode => SimdOpCode.Int16X8Q15MulrSatSigned;

    internal override void Compile(CompilationContext context)
    {
        context.PopStackNoReturn(OpCode, WebAssemblyValueType.Vector128, WebAssemblyValueType.Vector128);
        context.Stack.Push(WebAssemblyValueType.Vector128);

        var vecI16 = typeof(Vector128<short>);
        var vecI32 = typeof(Vector128<int>);

        var widenLower = FindVector128Method("WidenLower", typeof(short), 1, true);
        var widenUpper = FindVector128Method("WidenUpper", typeof(short), 1, true);
        var multiply = FindVector128Method("Multiply", typeof(int), 2, true);
        var add = FindVector128Method("Add", typeof(int), 2, true);
        var shiftRight = FindVector128Method("ShiftRightArithmetic", typeof(int), 2, true);
        var max = FindVector128Method("Max", typeof(int), 2, true);
        var min = FindVector128Method("Min", typeof(int), 2, true);
        var narrow = FindVector128Method("Narrow", typeof(int), 2, false);
        var createInt = FindVector128Method("Create", typeof(int), 1, false);

        // Store inputs to locals
        var v2 = context.DeclareLocal(vecI16);
        var v1 = context.DeclareLocal(vecI16);
        context.Emit(OpCodes.Stloc, v2.LocalIndex);
        context.Emit(OpCodes.Stloc, v1.LocalIndex);

        // Process lower 4 lanes
        // WidenLower(v1) * WidenLower(v2) + 16384 >> 15, then clamp to i16 range
        context.Emit(OpCodes.Ldloc, v1.LocalIndex);
        context.Emit(OpCodes.Call, widenLower);
        context.Emit(OpCodes.Ldloc, v2.LocalIndex);
        context.Emit(OpCodes.Call, widenLower);
        context.Emit(OpCodes.Call, multiply);
        context.Emit(OpCodes.Ldc_I4, 16384);
        context.Emit(OpCodes.Call, createInt);
        context.Emit(OpCodes.Call, add);
        context.Emit(OpCodes.Ldc_I4, 15);
        context.Emit(OpCodes.Call, shiftRight);
        // Clamp to [short.MinValue, short.MaxValue]
        context.Emit(OpCodes.Ldc_I4, (int)short.MinValue);
        context.Emit(OpCodes.Call, createInt);
        context.Emit(OpCodes.Call, max);
        context.Emit(OpCodes.Ldc_I4, (int)short.MaxValue);
        context.Emit(OpCodes.Call, createInt);
        context.Emit(OpCodes.Call, min);

        // Store lower result
        var lowResult = context.DeclareLocal(vecI32);
        context.Emit(OpCodes.Stloc, lowResult.LocalIndex);

        // Process upper 4 lanes
        // WidenUpper(v1) * WidenUpper(v2) + 16384 >> 15, then clamp to i16 range
        context.Emit(OpCodes.Ldloc, v1.LocalIndex);
        context.Emit(OpCodes.Call, widenUpper);
        context.Emit(OpCodes.Ldloc, v2.LocalIndex);
        context.Emit(OpCodes.Call, widenUpper);
        context.Emit(OpCodes.Call, multiply);
        context.Emit(OpCodes.Ldc_I4, 16384);
        context.Emit(OpCodes.Call, createInt);
        context.Emit(OpCodes.Call, add);
        context.Emit(OpCodes.Ldc_I4, 15);
        context.Emit(OpCodes.Call, shiftRight);
        // Clamp to [short.MinValue, short.MaxValue]
        context.Emit(OpCodes.Ldc_I4, (int)short.MinValue);
        context.Emit(OpCodes.Call, createInt);
        context.Emit(OpCodes.Call, max);
        context.Emit(OpCodes.Ldc_I4, (int)short.MaxValue);
        context.Emit(OpCodes.Call, createInt);
        context.Emit(OpCodes.Call, min);

        // Stack: [highResult]
        // Load lowResult, then narrow both
        var highResult = context.DeclareLocal(vecI32);
        context.Emit(OpCodes.Stloc, highResult.LocalIndex);

        context.Emit(OpCodes.Ldloc, lowResult.LocalIndex);
        context.Emit(OpCodes.Ldloc, highResult.LocalIndex);
        context.Emit(OpCodes.Call, narrow);
    }
}
