using System.Reflection;
using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// SIMD negate 2 64-bit integers.
/// </summary>
public class Int64X2Neg : SimdInstruction
{
    private static readonly MethodInfo negMethod = FindVector128Method("Negate", typeof(ulong), 1);

    /// <summary>
    /// Always <see cref="SimdOpCode.Int64X2Neg"/>.
    /// </summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int64X2Neg;

    internal sealed override void Compile(CompilationContext context)
    {
        var stack = context.Stack;

        // TODO: Maybe add an override which accepts SimdOpCode too
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Vector128);
        stack.Push(WebAssemblyValueType.Vector128);

        context.Emit(OpCodes.Call, negMethod);
    }

    /// <summary>
    /// Creates a new  <see cref="Int64X2Neg"/> instance.
    /// </summary>
    public Int64X2Neg()
    {
    }
}
