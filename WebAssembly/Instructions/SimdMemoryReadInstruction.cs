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

    internal sealed override void Compile(CompilationContext context)
    {
        MemoryImmediateInstruction.EmitMemoryAccessProlog(context, OpCode, Offset, Flags, 16);

        context.Emit(OpCodes.Conv_U);
        context.Emit(OpCodes.Call, SimdOpCode.Vec128Load.ToMethodInfo()); 

        context.Stack.Push(WebAssemblyValueType.Vector128);
    }
}

