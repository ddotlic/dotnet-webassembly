using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Intrinsics;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Identifies an instruction that uses a single CIL method call of the <see cref="Vector128"/> to remove two SIMD values from the stack, replacing it with one value, all of a specific type.
/// </summary>
public abstract class SimdValueTwoToOneInstruction : SimdInstruction
{
    private protected SimdValueTwoToOneInstruction()
    {
    }

    private protected abstract MethodInfo Vector128Method { get; }

    internal sealed override void Compile(CompilationContext context)
    {
        var stack = context.Stack;

        // TODO: Maybe add an override which accepts SimdOpCode too
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Vector128, WebAssemblyValueType.Vector128);
        stack.Push(WebAssemblyValueType.Vector128);

        context.Emit(OpCodes.Call, Vector128Method);
    }
}
