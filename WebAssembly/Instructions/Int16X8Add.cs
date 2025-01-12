using System.Reflection;

namespace WebAssembly.Instructions;

/// <summary>
/// SIMD add 8 16-bit integers.
/// </summary>
public class Int16X8Add : SimdValueTwoToOneCallInstruction
{
    private static readonly MethodInfo addMethod = FindVector128Method("Add", typeof(ushort));

    /// <summary>
    /// Always <see cref="SimdOpCode.Int16X8Add"/>.
    /// </summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int16X8Add;

    private protected override MethodInfo Vector128Method => addMethod;

    /// <summary>
    /// Creates a new  <see cref="Int16X8Add"/> instance.
    /// </summary>
    public Int16X8Add()
    {
    }
}
