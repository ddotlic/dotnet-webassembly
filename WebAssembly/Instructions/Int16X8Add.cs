namespace WebAssembly.Instructions;

/// <summary>
/// SIMD add 8 16-bit integers.
/// </summary>
public class Int16X8Add : SimdValueTwoToOneCallInstruction
{
    /// <summary>
    /// Always <see cref="SimdOpCode.Int16X8Add"/>.
    /// </summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16X8Add;

    /// <summary>
    /// Creates a new  <see cref="Int16X8Add"/> instance.
    /// </summary>
    public Int16X8Add()
    {
    }
}
