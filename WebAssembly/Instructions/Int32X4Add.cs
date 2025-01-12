namespace WebAssembly.Instructions;

/// <summary>
/// SIMD add 4 32-bit integers.
/// </summary>
public class Int32X4Add : SimdValueTwoToOneCallInstruction
{
    /// <summary>
    /// Always <see cref="SimdOpCode.Int32X4Add"/>.
    /// </summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32X4Add;

    /// <summary>
    /// Creates a new  <see cref="Int32X4Add"/> instance.
    /// </summary>
    public Int32X4Add()
    {
    }
}
