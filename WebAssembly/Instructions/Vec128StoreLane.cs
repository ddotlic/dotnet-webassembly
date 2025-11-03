using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Common features of SIMD instructions that store a value from a lane of a vector to memory.
/// </summary>
public abstract class Vec128StoreLane : SimdMemoryLaneInstruction
{
    private protected Vec128StoreLane()
    {
    }

    private protected Vec128StoreLane(Reader reader)
        : base(reader)
    {
    }

    private protected override byte Size => SimdOpCode switch
    {
        SimdOpCode.Vec128Store8Lane => 1,
        SimdOpCode.Vec128Store16Lane => 2,
        SimdOpCode.Vec128Store32Lane => 4,
        SimdOpCode.Vec128Store64Lane => 8,
        _ => throw new CompilerException("Invalid SimdOpCode for lane store instruction."),
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

    private System.Reflection.Emit.OpCode StoreOpCode => Size switch
    {
        1 => OpCodes.Stind_I1,
        2 => OpCodes.Stind_I2,
        4 => OpCodes.Stind_I4,
        8 => OpCodes.Stind_I8,
        _ => throw new CompilerException($"Invalid store size {Size}."),
    };

    private System.Type LaneType => Size switch
    {
        1 => typeof(byte),
        2 => typeof(ushort),
        4 => typeof(uint),
        8 => typeof(ulong),
        _ => throw new CompilerException($"Invalid store lane size of {Size}."),
    };

    internal override void Compile(CompilationContext context)
    {
        ValidateLaneAndAlignment();

        // Stack on entry: [address:i32, vector:v128]
        // Strategy: Extract lane into local, process address, then store
        
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Vector128);
        
        // Extract lane value directly from stack
        context.Emit(OpCodes.Ldc_I4, (int)LaneIndex);
        var getElementMethod = SimdOpCode.ToMethodInfo();
        context.Emit(OpCodes.Call, getElementMethod);

        // Stack is now: [address:i32, lane_value]
        // Save lane value in local
        var laneLocal = context.DeclareLocal(LaneType);
        context.Emit(OpCodes.Stloc, laneLocal.LocalIndex);

        // Stack is now: [address:i32]
        // Process address to get memory pointer, this consumes (pops) address from WASM stack
        // NOTE: do NOT emit Unaligned since we CANNOT follow with the Stind_* instruction immediately.
        //  turns out Unaligned must be IMMEDIATELY followed by Ldind_* or Stind_*, otherwise 
        //  the IL compiler will throw the `InvalidProgramException`
        MemoryImmediateInstruction.EmitMemoryAccessProlog(context, OpCode, Offset, Flags, Size, false);

        // Stack is now: [memory_pointer]
        // Load lane value and store
        context.Emit(OpCodes.Ldloc, laneLocal.LocalIndex);
        MemoryImmediateInstruction.EmitAlignment(context, Flags);
        context.Emit(StoreOpCode);
        
        // No value left on the stack
    }
}
