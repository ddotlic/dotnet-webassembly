using System.Reflection;

namespace WebAssembly.Instructions;

/// <summary>
/// SIMD mutiply 8 16-bit integers.
/// </summary>
public class Int16X8Mul : SimdValueTwoToOneCallInstruction
{
    private static readonly MethodInfo mulMethod = FindVector128Method("Multiply", typeof(ushort));

    /// <summary>
    /// Always <see cref="SimdOpCode.Int16X8Mul"/>.
    /// </summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16X8Mul;

    private protected override MethodInfo Vector128Method => mulMethod;

    /// <summary>
    /// Creates a new  <see cref="Int16X8Mul"/> instance.
    /// </summary>
    public Int16X8Mul()
    {
    }
}
