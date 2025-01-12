using System.Reflection;

namespace WebAssembly.Instructions;

/// <summary>
/// SIMD add 2 64-bit integers.
/// </summary>
public class Int64X2Add : SimdValueTwoToOneInstruction
{
    private static readonly MethodInfo addMethod = FindVector128Method("Add", typeof(uint));

    /// <summary>
    /// Always <see cref="SimdOpCode.Int64X2Add"/>.
    /// </summary>
    public override SimdOpCode SimdOpCode => SimdOpCode.Int64X2Add;

    private protected override MethodInfo Vector128Method => addMethod;

    /// <summary>
    /// Creates a new  <see cref="Int64X2Add"/> instance.
    /// </summary>
    public Int64X2Add()
    {
    }
}
