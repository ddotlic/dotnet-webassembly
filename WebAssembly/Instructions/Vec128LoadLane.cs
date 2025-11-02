using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Common features of SIMD instructions that load a value into a lane of a vector.
/// </summary>
public abstract class Vec128LoadLane : SimdMemoryLaneInstruction
{
    private protected Vec128LoadLane()
    {
    }

    private protected Vec128LoadLane(Reader reader)
        : base(reader)
    {
    }

    private protected override byte Size => SimdOpCode switch
    {
        SimdOpCode.Vec128Load8Lane => 1,
        SimdOpCode.Vec128Load16Lane => 2,
        SimdOpCode.Vec128Load32Lane => 4,
        SimdOpCode.Vec128Load64Lane => 8,
        _ => throw new CompilerException("Invalid SimdOpCode for lane load instruction."),
    };

    private MemoryImmediateInstruction.Options NaturalAlignment => Size switch
    {
        1 => MemoryImmediateInstruction.Options.Align1,
        2 => MemoryImmediateInstruction.Options.Align2,
        4 => MemoryImmediateInstruction.Options.Align4,
        8 => MemoryImmediateInstruction.Options.Align8,
        _ => throw new CompilerException("Lane size out of range."),
    };

    private void ValidateLaneAndAlignment()
    {
        var sizeBits = 8 * Size;
        var maxLanes = (128 / sizeBits) - 1;
        if (LaneIndex > maxLanes)
            throw new CompilerException($"Lane index must be less than {maxLanes + 1}");
        var alignment = NaturalAlignment;
        if (Flags > alignment)
            throw new CompilerException($"Alignment must be less than {alignment}.");
    }

    private System.Reflection.Emit.OpCode LoadOpCode => Size switch
    {
        1 => OpCodes.Ldind_U1,
        2 => OpCodes.Ldind_U2,
        4 => OpCodes.Ldind_U4,
        8 => OpCodes.Ldind_I8,
        _ => throw new CompilerException($"Invalid load size {Size}."),
    };

    private System.Type LaneType => Size switch
    {
        1 => typeof(byte),
        2 => typeof(ushort),
        4 => typeof(uint),
        8 => typeof(ulong),
        _ => throw new CompilerException($"Invalid load lane size of {Size}."),
    };

    internal override void Compile(CompilationContext context)
    {
        ValidateLaneAndAlignment();

        // Stack on entry: [address:i32, vector:v128]
        // We need to: load byte from memory at address, then call WithElement on the vector

        // Save the vector in a local variable and update stack tracking
        var vecType = typeof(System.Runtime.Intrinsics.Vector128<>).MakeGenericType(LaneType);
        var vectorLocal = context.DeclareLocal(vecType);
        context.Emit(OpCodes.Stloc, vectorLocal.LocalIndex);
        // TODO: Maybe add an override which accepts SimdOpCode too
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Vector128);

        // Stack is now: [address:i32]
        // Handle the memory address: address calculation, range check, and pointer to memory
        MemoryImmediateInstruction.EmitMemoryAccessProlog(context, OpCode, Offset, Flags, Size);

        // Stack is now: [memory_pointer]
        // Load the byte from memory
        context.Emit(LoadOpCode);

        // Stack is now: [lane_value]
        // Save lane value temporarily
        var laneLocal = context.DeclareLocal(LaneType);
        context.Emit(OpCodes.Stloc, laneLocal.LocalIndex);

        // Now build the call to WithElement: [vector, lane_index, lane_value]
        context.Emit(OpCodes.Ldloc, vectorLocal.LocalIndex);
        context.Emit(OpCodes.Ldc_I4, (int)LaneIndex);
        context.Emit(OpCodes.Ldloc, laneLocal.LocalIndex);

        // Stack: [vector, lane_index, lane_value]
        var withElementMethod = SimdOpCode.ToMethodInfo();

        context.Emit(OpCodes.Call, withElementMethod);

        // Push the result v128 back onto the stack
        context.Stack.Push(WebAssemblyValueType.Vector128);
    }
}
