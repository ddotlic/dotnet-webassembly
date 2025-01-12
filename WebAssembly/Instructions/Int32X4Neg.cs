using System.Reflection;
using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// SIMD negate 4 32-bit integers.
/// </summary>
public class Int32X4Neg : SimdInstruction
{
    private static readonly MethodInfo negMethod = FindVector128Method("Negate", typeof(uint), 1);

    /// <summary>
    /// Always <see cref="SimdOpCode.Int32X4Neg"/>.
    /// </summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32X4Neg;

    internal sealed override void Compile(CompilationContext context)
    {
        var stack = context.Stack;

        // TODO: Maybe add an override which accepts SimdOpCode too
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Vector128);
        stack.Push(WebAssemblyValueType.Vector128);

        context.Emit(OpCodes.Call, negMethod);
    }

    /// <summary>
    /// Creates a new  <see cref="Int32X4Neg"/> instance.
    /// </summary>
    public Int32X4Neg()
    {
    }
}
