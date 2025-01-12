namespace WebAssembly.Instructions;

/// <summary>
/// SIMD negate 4 32-bit integers.
/// </summary>
public class Int32X4Neg : SimdValueOneToOneCallInstruction
{
    /// <summary>
    /// Always <see cref="SimdOpCode.Int32X4Neg"/>.
    /// </summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32X4Neg;

    /// <summary>
    /// Creates a new  <see cref="Int32X4Neg"/> instance.
    /// </summary>
    public Int32X4Neg()
    {
    }
}
