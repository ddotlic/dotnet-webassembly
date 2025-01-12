namespace WebAssembly.Instructions;

/// <summary>
/// SIMD negate 8 16-bit integers.
/// </summary>
public class Int16X8Neg : SimdValueOneToOneCallInstruction
{
    /// <summary>
    /// Always <see cref="SimdOpCode.Int16X8Neg"/>.
    /// </summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16X8Neg;

    /// <summary>
    /// Creates a new  <see cref="Int16X8Neg"/> instance.
    /// </summary>
    public Int16X8Neg()
    {
    }

}
