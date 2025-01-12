using System.Reflection;

namespace WebAssembly.Instructions;

/// <summary>
/// SIMD multiply 4 32-bit integers.
/// </summary>
public class Int32X4Mul : SimdValueTwoToOneInstruction
{
    private static readonly MethodInfo mulMethod = FindVector128Method("Multiply", typeof(uint));

    /// <summary>
    /// Always <see cref="SimdOpCode.Int32X4Mul"/>.
    /// </summary>
    public override SimdOpCode SimdOpCode => SimdOpCode.Int32X4Mul;

    private protected override MethodInfo Vector128Method => mulMethod;

    /// <summary>
    /// Creates a new  <see cref="Int32X4Mul"/> instance.
    /// </summary>
    public Int32X4Mul()
    {
    }
}
