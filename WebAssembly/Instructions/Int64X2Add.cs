namespace WebAssembly.Instructions;

/// <summary>
/// SIMD add 2 64-bit integers.
/// </summary>
public class Int64X2Add : SimdValueTwoToOneCallInstruction
{
    /// <summary>
    /// Always <see cref="SimdOpCode.Int64X2Add"/>.
    /// </summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int64X2Add;

    /// <summary>
    /// Creates a new  <see cref="Int64X2Add"/> instance.
    /// </summary>
    public Int64X2Add()
    {
    }
}
