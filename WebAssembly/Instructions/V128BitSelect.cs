using System.Reflection.Emit;
using System.Runtime.Intrinsics;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// SIMD bitselect from two 128-bit vectors with a 128-bit mask.
/// </summary>
public class V128BitSelect : SimdInstruction
{
    /// <summary>
    /// Creates a new  <see cref="V128BitSelect"/> instance.
    /// </summary>
    public V128BitSelect()
    {
    }

    /// <summary>
    /// Always <see cref="SimdOpCode.V128BitSelect"/>.
    /// </summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.V128BitSelect;
    
    internal sealed override void Compile(CompilationContext context)
    {
        var stack = context.Stack;

        // TODO: Maybe add an override which accepts SimdOpCode too
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Vector128, WebAssemblyValueType.Vector128);
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Vector128);
        stack.Push(WebAssemblyValueType.Vector128);

        context.Emit(OpCodes.Call, context[HelperMethod.V128BitSelect, (_, c) =>
        {
            // NOTE: the matching method only differs in the order of the arguments
            var typeVec128 = typeof(Vector128<uint>);
            var builder = c.CheckedExportsBuilder.DefineMethod(
                "☣ V128BitSelect",
                CompilationContext.HelperMethodAttributes,
                typeVec128,
                [
                    typeVec128,
                    typeVec128,
                    typeVec128
                ]
                );

            var il = builder.GetILGenerator();
            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Call, this.SimdOpCode.ToMethodInfo());
            il.Emit(OpCodes.Ret);
            return builder;
        }
        ]);
    }
}
