using System;
using System.Reflection.Emit;
using System.Runtime.Intrinsics;
using WebAssembly.Runtime.Compilation;
using static WebAssembly.SimdOpCodeExtensions;

namespace WebAssembly.Instructions;

/// <summary>
/// Implements extended multiplication instructions that widen lanes before multiplying.
/// </summary>
public abstract class Vec128ExtMul : SimdInstruction
{
    private protected Vec128ExtMul() { }

    internal override void Compile(CompilationContext context)
    {
        context.PopStackNoReturn(OpCode, WebAssemblyValueType.Vector128, WebAssemblyValueType.Vector128);
        context.Stack.Push(WebAssemblyValueType.Vector128);

        var opcodeName = SimdOpCode.ToNativeName();

        var (srcLaneType, dstLaneType, isLow) = opcodeName switch
        {
            "i16x8.extmul_low_i8x16_s" => (typeof(sbyte), typeof(short), true),
            "i16x8.extmul_high_i8x16_s" => (typeof(sbyte), typeof(short), false),
            "i16x8.extmul_low_i8x16_u" => (typeof(byte), typeof(ushort), true),
            "i16x8.extmul_high_i8x16_u" => (typeof(byte), typeof(ushort), false),
            "i32x4.extmul_low_i16x8_s" => (typeof(short), typeof(int), true),
            "i32x4.extmul_high_i16x8_s" => (typeof(short), typeof(int), false),
            "i32x4.extmul_low_i16x8_u" => (typeof(ushort), typeof(uint), true),
            "i32x4.extmul_high_i16x8_u" => (typeof(ushort), typeof(uint), false),
            "i64x2.extmul_low_i32x4_s" => (typeof(int), typeof(long), true),
            "i64x2.extmul_high_i32x4_s" => (typeof(int), typeof(long), false),
            "i64x2.extmul_low_i32x4_u" => (typeof(uint), typeof(ulong), true),
            "i64x2.extmul_high_i32x4_u" => (typeof(uint), typeof(ulong), false),
            _ => throw new InvalidOperationException($"Unsupported opcode '{opcodeName}' for extmul."),
        };

        var vecDst = typeof(Vector128<>).MakeGenericType(dstLaneType);

        var widen = isLow
            ? FindVector128Method("WidenLower", srcLaneType, 1, true)
            : FindVector128Method("WidenUpper", srcLaneType, 1, true);
        var multiply = FindVector128Method("Multiply", dstLaneType, 2, true);

        // Stack: [v1, v2]
        // Widen v2, store it
        context.Emit(OpCodes.Call, widen);
        var widenedV2 = context.DeclareLocal(vecDst);
        context.Emit(OpCodes.Stloc, widenedV2.LocalIndex);

        // Stack: [v1] -> widen -> [widened_v1]
        context.Emit(OpCodes.Call, widen);

        // Stack: [widened_v1] -> load widened_v2 -> [widened_v1, widened_v2]
        context.Emit(OpCodes.Ldloc, widenedV2.LocalIndex);

        // Multiply
        context.Emit(OpCodes.Call, multiply);
    }
}
