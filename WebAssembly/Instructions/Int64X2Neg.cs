using System.Reflection;

namespace WebAssembly.Instructions;

/// <summary>
/// SIMD negate 2 64-bit integers.
/// </summary>
public class Int64X2Neg : SimdValueOneToOneCallInstruction
{
    private static readonly MethodInfo negMethod = FindVector128Method("Negate", typeof(ulong), 1);

    /// <summary>
    /// Always <see cref="SimdOpCode.Int64X2Neg"/>.
    /// </summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int64X2Neg;

    private protected override MethodInfo Vector128Method => negMethod;

    /// <summary>
    /// Creates a new  <see cref="Int64X2Neg"/> instance.
    /// </summary>
    public Int64X2Neg()
    {
    }
}
