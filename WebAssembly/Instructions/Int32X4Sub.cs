namespace WebAssembly.Instructions;

/// <summary>
/// SIMD subtract 4 32-bit integers.
/// </summary>
public class Int32X4Sub : SimdValueTwoToOneCallInstruction
{
    /// <summary>
    /// Always <see cref="SimdOpCode.Int32X4Sub"/>.
    /// </summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32X4Sub;

    /// <summary>
    /// Creates a new  <see cref="Int32X4Sub"/> instance.
    /// </summary>
    public Int32X4Sub()
    {
    }
}
