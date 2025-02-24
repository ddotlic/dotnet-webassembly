using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Provides shared functionality for instructions that read from linear memory.
/// </summary>
public abstract class MemoryReadInstruction : MemoryImmediateInstruction
{
    private protected MemoryReadInstruction()
        : base()
    {
    }

    private protected MemoryReadInstruction(Reader reader)
        : base(reader)
    {
    }

    private protected virtual System.Reflection.Emit.OpCode ConversionOpCode => OpCodes.Nop;

    
    internal sealed override void Compile(CompilationContext context)
    { 
        EmitMemoryAccessProlog(context, OpCode, Offset, Flags, Size);

        context.Emit(this.EmittedOpCode);
        var conversion = this.ConversionOpCode;
        if (conversion != OpCodes.Nop)
            context.Emit(conversion);

        var stack = context.Stack;
        stack.Push(this.Type);
    }
}
