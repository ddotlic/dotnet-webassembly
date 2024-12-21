using System.Linq;
using System.Reflection;
using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>
/// SIMD add 2 64-bit integers.
/// </summary>
public class Int64X2Add : SimdValueTwoToOneInstruction
{
    private static readonly MethodInfo addMethod;

    static Int64X2Add()
    {
        var methods = typeof(Vector128).GetMethods(BindingFlags.Public | BindingFlags.Static);
        var genericMethodInfo = methods.Where(m => m.Name == "Add").First(m =>
        {
            var pars = m.GetParameters();
            return pars.Length == 2 && pars.All(p => p.ParameterType.IsGenericType &&
                p.ParameterType.GetGenericTypeDefinition() == typeof(Vector128<>));
        });
        addMethod = genericMethodInfo.MakeGenericMethod(typeof(ulong));
    }

    /// <summary>
    /// Always <see cref="SimdOpCode.Int64X2Add"/>.
    /// </summary>
    public override SimdOpCode SimdOpCode => SimdOpCode.Int64X2Add;

    private protected sealed override WebAssemblyValueType ValueType => WebAssemblyValueType.Vector128;
    private protected override MethodInfo Vector128Method => addMethod;


    /// <summary>
    /// Creates a new  <see cref="Int64X2Add"/> instance.
    /// </summary>
    public Int64X2Add()
    {
    }
}
