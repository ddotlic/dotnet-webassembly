namespace WebAssembly.Instructions;

/// <summary>
/// SIMD subtract 8 16-bit integers.
/// </summary>
public class Int16X8Sub : SimdValueTwoToOneCallInstruction
{
    /// <summary>
    /// Always <see cref="SimdOpCode.Int16X8Sub"/>.
    /// </summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16X8Sub;

    /// <summary>
    /// Creates a new  <see cref="Int16X8Sub"/> instance.
    /// </summary>
    public Int16X8Sub()
    {
    }
}
