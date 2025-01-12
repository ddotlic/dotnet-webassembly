namespace WebAssembly.Instructions;

/// <summary>
/// SIMD subtract 2 64-bit integers.
/// </summary>
public class Int64X2Sub : SimdValueTwoToOneCallInstruction
{
    /// <summary>
    /// Always <see cref="SimdOpCode.Int64X2Sub"/>.
    /// </summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int64X2Sub;

    /// <summary>
    /// Creates a new  <see cref="Int64X2Sub"/> instance.
    /// </summary>
    public Int64X2Sub()
    {
    }
}
