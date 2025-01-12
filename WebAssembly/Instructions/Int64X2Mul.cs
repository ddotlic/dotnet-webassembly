using System.Reflection;

namespace WebAssembly.Instructions;

/// <summary>
/// SIMD multiply 2 64-bit integers.
/// </summary>
public class Int64X2Mul : SimdValueTwoToOneCallInstruction
{
    private static readonly MethodInfo mulMethod = FindVector128Method("Multiply", typeof(ulong));

    /// <summary>
    /// Always <see cref="SimdOpCode.Int64X2Mul"/>.
    /// </summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int64X2Mul;

    private protected override MethodInfo Vector128Method => mulMethod;

    /// <summary>
    /// Creates a new  <see cref="Int64X2Mul"/> instance.
    /// </summary>
    public Int64X2Mul()
    {
    }
}
