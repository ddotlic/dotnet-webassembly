using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Bit-shift the vector by a given shift count.
/// </summary>
public abstract class Vec128Shift : SimdInstruction
{
    private protected Vec128Shift()
    {
    }
    
    internal sealed override void Compile(CompilationContext context)
    {
        // TODO: Maybe add an override which accepts SimdOpCode too
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int32);
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Vector128);
        
        context.Emit(OpCodes.Call, this.SimdOpCode.ToMethodInfo());
        
        context.Stack.Push(WebAssemblyValueType.Vector128);
    }
}
