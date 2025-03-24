using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Intrinsics;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Identifies an instruction that uses a single CIL method call of the <see cref="Vector128"/> to remove one SIMD value from the stack, replacing it with one value, all of a specific type.
/// </summary>
public abstract class SimdValueOneToOneCallInstruction : SimdInstruction
{
    private protected SimdValueOneToOneCallInstruction()
    {
    }

    private protected virtual WebAssemblyValueType OutputType => WebAssemblyValueType.Vector128;
    
    internal sealed override void Compile(CompilationContext context)
    {
        // TODO: Maybe add an override which accepts SimdOpCode too
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Vector128);
        context.Stack.Push(OutputType);
        
        context.Emit(OpCodes.Call, this.SimdOpCode.ToMethodInfo());
    }
}
