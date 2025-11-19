using System;
using System.Reflection.Emit;
using System.Runtime.Intrinsics;
using WebAssembly.Runtime.Compilation;
using static WebAssembly.SimdOpCodeExtensions;
using static WebAssembly.SimdOpCodeExtensions.KnownMethodName;

namespace WebAssembly.Instructions;

/// <summary>
/// Return 1 if all lanes are non-zero, 0 otherwise.
/// </summary>
public abstract class Vec128AllTrue : SimdInstruction
{
    private protected Vec128AllTrue()
    {
    }

    private string LaneKind => this.SimdOpCode.ToLaneKind(); 
    
    private uint Mask => 
        LaneKind switch {
            "i8x16" => 0b1111_1111_1111_1111u,
            "i16x8" => 0b1111_1111u,
            "i32x4" => 0b1111u,
            "i64x2" => 0b11u,
            _ => throw new InvalidOperationException($"Unexpected lane type: {LaneKind}.")
        };
    
    internal sealed override void Compile(CompilationContext context)
    {
        var laneKind = this.SimdOpCode.ToLaneKind();

        // TODO: Maybe add an override which accepts SimdOpCode too
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Vector128);
        context.Stack.Push(WebAssemblyValueType.Int32);

        var zeroMethod = GetWellKnownMethod(laneKind, Zero);
        var equalsMethod = GetWellKnownMethod(laneKind, VecEquals);
        var onesComplementMethod = GetWellKnownMethod(laneKind, OnesComplement);
        var extractMsbMethod = GetWellKnownMethod(laneKind, ExtractMostSignificantBits);

        context.Emit(OpCodes.Call, zeroMethod);
        context.Emit(OpCodes.Call, equalsMethod);
        context.Emit(OpCodes.Call, onesComplementMethod);
        context.Emit(OpCodes.Call, extractMsbMethod);
        context.Emit(OpCodes.Ldc_I4, (int)Mask);
        context.Emit(OpCodes.Ceq);
    }
}
