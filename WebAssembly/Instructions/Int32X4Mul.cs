namespace WebAssembly.Instructions;

/// <summary>
/// SIMD multiply 4 32-bit integers.
/// </summary>
public class Int32X4Mul : SimdValueTwoToOneCallInstruction
{
    /// <summary>
    /// Always <see cref="SimdOpCode.Int32X4Mul"/>.
    /// </summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32X4Mul;

    /// <summary>
    /// Creates a new  <see cref="Int32X4Mul"/> instance.
    /// </summary>
    public Int32X4Mul()
    {
    }
}
