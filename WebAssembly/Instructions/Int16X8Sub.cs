using System.Reflection;

namespace WebAssembly.Instructions;

/// <summary>
/// SIMD subtract 8 16-bit integers.
/// </summary>
public class Int16X8Sub : SimdValueTwoToOneCallInstruction
{
    private static readonly MethodInfo subMethod = FindVector128Method("Subtract", typeof(ushort));

    /// <summary>
    /// Always <see cref="SimdOpCode.Int16X8Sub"/>.
    /// </summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16X8Sub;

    private protected override MethodInfo Vector128Method => subMethod;

    /// <summary>
    /// Creates a new  <see cref="Int16X8Sub"/> instance.
    /// </summary>
    public Int16X8Sub()
    {
    }
}
