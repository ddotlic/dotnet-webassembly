using System.Linq;
using System.Reflection;
using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

// TODO: This is a SIMD instruction, implementation is incomplete.

/// <summary>
/// SIMD add 4 32-bit integers.
/// </summary>
public class Int32X4Add : SimdValueTwoToOneInstruction
{
    private static readonly MethodInfo addMethod;

    static Int32X4Add()
    {
        var methods = typeof(Vector128).GetMethods(BindingFlags.Public | BindingFlags.Static);
        var genericMethodInfo = methods.Where(m => m.Name == "Add").First(m =>
        {
            var pars = m.GetParameters();
            return pars.Length == 2 && pars.All(p => p.ParameterType.IsGenericType &&
                p.ParameterType.GetGenericTypeDefinition() == typeof(Vector128<>));
        });
        addMethod = genericMethodInfo.MakeGenericMethod(typeof(uint));
    }

    /// <summary>
    /// Always <see cref="SimdOpCode.Int32X4Add"/>.
    /// </summary>
    public override SimdOpCode SimdOpCode => SimdOpCode.Int32X4Add;

    private protected sealed override WebAssemblyValueType ValueType => WebAssemblyValueType.Vector128;
    private protected override MethodInfo Vector128Method => addMethod;

    /// <summary>
    /// Creates a new  <see cref="Int32X4Add"/> instance.
    /// </summary>
    public Int32X4Add()
    {
    }
}
