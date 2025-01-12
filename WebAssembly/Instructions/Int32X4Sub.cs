using System.Reflection;

namespace WebAssembly.Instructions;

/// <summary>
/// SIMD subtract 4 32-bit integers.
/// </summary>
public class Int32X4Sub : SimdValueTwoToOneInstruction
{
    private static readonly MethodInfo subMethod = FindVector128Method("Subtract", typeof(uint));

    /// <summary>
    /// Always <see cref="SimdOpCode.Int32X4Sub"/>.
    /// </summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int32X4Sub;

    private protected override MethodInfo Vector128Method => subMethod;

    /// <summary>
    /// Creates a new  <see cref="Int32X4Sub"/> instance.
    /// </summary>
    public Int32X4Sub()
    {
    }
}
