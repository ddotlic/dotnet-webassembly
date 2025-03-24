using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;
namespace WebAssembly.Instructions;

/// <summary>
/// Provides shared functionality for SIMD instructions that read from linear memory.
/// </summary>
public abstract class SimdMemoryReadInstruction : SimdMemoryImmediateInstruction
{
       
    private protected SimdMemoryReadInstruction()
        : base()
    {
    }

    private protected SimdMemoryReadInstruction(Reader reader)
        : base(reader)
    {
    }

    private protected virtual System.Reflection.Emit.OpCode[] LoadOpCodes => [];
    
    internal sealed override void Compile(CompilationContext context)
    {
        MemoryImmediateInstruction.EmitMemoryAccessProlog(context, OpCode, Offset, Flags, Size);

        foreach (var opCode in LoadOpCodes)
        {
            context.Emit(opCode);
        }
        
        context.Emit(OpCodes.Call, this.SimdOpCode.ToMethodInfo()); 

        context.Stack.Push(WebAssemblyValueType.Vector128);
    }
}

