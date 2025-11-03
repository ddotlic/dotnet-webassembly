using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Provides shared functionality for SIMD instructions that write to linear memory.
/// </summary>
public abstract class SimdMemoryWriteInstruction : SimdMemoryImmediateInstruction
{
    private protected SimdMemoryWriteInstruction()
        : base()
    {
    }

    private protected SimdMemoryWriteInstruction(Reader reader)
        : base(reader)
    {
    }

    internal sealed override void Compile(CompilationContext context)
    {
        // Stack on entry: [address:i32, value:v128]

        // Save the v128 value first
        var valueType = typeof(System.Runtime.Intrinsics.Vector128<byte>);
        var valueLocal = context.DeclareLocal(valueType);
        context.Emit(OpCodes.Stloc, valueLocal.LocalIndex);
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Vector128);

        // Stack is now: [address:i32]
        // Handle the memory address: address calculation, range check, and pointer to memory
        MemoryImmediateInstruction.EmitMemoryAccessProlog(context, OpCode, Offset, Flags, Size);

        // Stack is now: [memory_pointer]
        // We need to swap so the vector is first, then pointer
        // Store pointer temporarily
        var pointerType = typeof(byte).MakePointerType();
        var pointerLocal = context.DeclareLocal(pointerType);
        context.Emit(OpCodes.Stloc, pointerLocal.LocalIndex);

        // Load vector value, then pointer
        context.Emit(OpCodes.Ldloc, valueLocal.LocalIndex);
        context.Emit(OpCodes.Ldloc, pointerLocal.LocalIndex);

        // Stack is now: [value:v128, memory_pointer]
        // Call Vector128.Store(Vector128<byte> source, byte* destination)
        var storeMethod = SimdOpCode.ToMethodInfo();
        context.Emit(OpCodes.Call, storeMethod);

        // No value left on the stack
    }
}
