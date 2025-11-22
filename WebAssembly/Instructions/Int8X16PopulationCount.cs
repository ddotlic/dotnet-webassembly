using System;
using System.Reflection.Emit;
using System.Runtime.Intrinsics;
using WebAssembly.Runtime.Compilation;
using static WebAssembly.SimdOpCodeExtensions;

namespace WebAssembly.Instructions;

/// <summary>
/// Implements i8x16.popcnt using standard SWAR popcount algorithm.
/// </summary>
public sealed class Int8X16PopulationCount : SimdInstruction
{
    /// <summary>
    /// Always <see cref="WebAssembly.SimdOpCode.Int8X16PopulationCount"/>.
    /// </summary>
    public override SimdOpCode SimdOpCode => SimdOpCode.Int8X16PopulationCount;
    
    internal override void Compile(CompilationContext context)
    {
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Vector128);
        context.Stack.Push(WebAssemblyValueType.Vector128);

        var laneKind = this.SimdOpCode.ToLaneKind();
        var parType = laneKind switch
        {
            "i8x16" => typeof(byte),
            _ => throw new InvalidOperationException($"Unsupported lane kind '{laneKind}' for popcnt."),
        };

        var vecType = typeof(Vector128<>).MakeGenericType(parType);

        var subtract = FindVector128Method("Subtract", parType, 2, true);
        var shiftRightLogical = FindVector128Method("ShiftRightLogical", parType, 2, false);
        var bitwiseAnd = FindVector128Method("BitwiseAnd", parType, 2, true);
        var add = FindVector128Method("Add", parType, 2, true);
        var create = FindVector128Method("Create", parType, 1, false);

        var localValue = context.DeclareLocal(vecType);
        var localTmpA = context.DeclareLocal(vecType);
        var localTmpB = context.DeclareLocal(vecType);
        var mask55 = context.DeclareLocal(vecType);
        var mask33 = context.DeclareLocal(vecType);
        var mask0F = context.DeclareLocal(vecType);

        // store operand
        context.Emit(OpCodes.Stloc, localValue.LocalIndex);

        // mask55 = Create(0x55)
        context.Emit(OpCodes.Ldc_I4, 0x55);
        context.Emit(OpCodes.Call, create);
        context.Emit(OpCodes.Stloc, mask55.LocalIndex);

        // mask33 = Create(0x33)
        context.Emit(OpCodes.Ldc_I4, 0x33);
        context.Emit(OpCodes.Call, create);
        context.Emit(OpCodes.Stloc, mask33.LocalIndex);

        // mask0F = Create(0x0f)
        context.Emit(OpCodes.Ldc_I4, 0x0f);
        context.Emit(OpCodes.Call, create);
        context.Emit(OpCodes.Stloc, mask0F.LocalIndex);

        // tmpA = (value >> 1) & mask55
        context.Emit(OpCodes.Ldloc, localValue.LocalIndex);
        context.Emit(OpCodes.Ldc_I4_1);
        context.Emit(OpCodes.Call, shiftRightLogical);
        context.Emit(OpCodes.Stloc, localTmpA.LocalIndex);

        context.Emit(OpCodes.Ldloc, localTmpA.LocalIndex);
        context.Emit(OpCodes.Ldloc, mask55.LocalIndex);
        context.Emit(OpCodes.Call, bitwiseAnd);
        context.Emit(OpCodes.Stloc, localTmpA.LocalIndex);

        // value = value - tmpA (parallel count of 2-bit fields)
        context.Emit(OpCodes.Ldloc, localValue.LocalIndex);
        context.Emit(OpCodes.Ldloc, localTmpA.LocalIndex);
        context.Emit(OpCodes.Call, subtract);
        context.Emit(OpCodes.Stloc, localValue.LocalIndex);

        // tmpA = value & mask33
        context.Emit(OpCodes.Ldloc, localValue.LocalIndex);
        context.Emit(OpCodes.Ldloc, mask33.LocalIndex);
        context.Emit(OpCodes.Call, bitwiseAnd);
        context.Emit(OpCodes.Stloc, localTmpA.LocalIndex);

        // tmpB = (value >> 2) & mask33
        context.Emit(OpCodes.Ldloc, localValue.LocalIndex);
        context.Emit(OpCodes.Ldc_I4_2);
        context.Emit(OpCodes.Call, shiftRightLogical);
        context.Emit(OpCodes.Stloc, localTmpB.LocalIndex);

        context.Emit(OpCodes.Ldloc, localTmpB.LocalIndex);
        context.Emit(OpCodes.Ldloc, mask33.LocalIndex);
        context.Emit(OpCodes.Call, bitwiseAnd);
        context.Emit(OpCodes.Stloc, localTmpB.LocalIndex);

        // value = tmpA + tmpB (sum neighboring 2-bit counts)
        context.Emit(OpCodes.Ldloc, localTmpA.LocalIndex);
        context.Emit(OpCodes.Ldloc, localTmpB.LocalIndex);
        context.Emit(OpCodes.Call, add);
        context.Emit(OpCodes.Stloc, localValue.LocalIndex);

        // tmpB = value >> 4
        context.Emit(OpCodes.Ldloc, localValue.LocalIndex);
        context.Emit(OpCodes.Ldc_I4, 4);
        context.Emit(OpCodes.Call, shiftRightLogical);
        context.Emit(OpCodes.Stloc, localTmpB.LocalIndex);

        // value = value + tmpB (accumulate 4-bit counts)
        context.Emit(OpCodes.Ldloc, localValue.LocalIndex);
        context.Emit(OpCodes.Ldloc, localTmpB.LocalIndex);
        context.Emit(OpCodes.Call, add);
        context.Emit(OpCodes.Stloc, localValue.LocalIndex);

        // value &= mask0F (final 4-bit popcount per lane)
        context.Emit(OpCodes.Ldloc, localValue.LocalIndex);
        context.Emit(OpCodes.Ldloc, mask0F.LocalIndex);
        context.Emit(OpCodes.Call, bitwiseAnd);
        context.Emit(OpCodes.Stloc, localValue.LocalIndex);

        context.Emit(OpCodes.Ldloc, localValue.LocalIndex);
    }
}
