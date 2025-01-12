using System.Reflection;

namespace WebAssembly.Instructions;

/// <summary>
/// SIMD subtract 2 64-bit integers.
/// </summary>
public class Int64X2Sub : SimdValueTwoToOneCallInstruction
{
    private static readonly MethodInfo subMethod = FindVector128Method("Subtract", typeof(ulong));

    /// <summary>
    /// Always <see cref="SimdOpCode.Int64X2Sub"/>.
    /// </summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int64X2Sub;

    private protected override MethodInfo Vector128Method => subMethod;

    /// <summary>
    /// Creates a new  <see cref="Int64X2Sub"/> instance.
    /// </summary>
    public Int64X2Sub()
    {
    }
}
