using System.Reflection.Emit;
using System.Runtime.Intrinsics;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// SIMD bitselect from two 128-bit vectors with a 128-bit mask.
/// </summary>
public class Vec128BitSelect : SimdInstruction
{
    /// <summary>
    /// Creates a new  <see cref="Vec128BitSelect"/> instance.
    /// </summary>
    public Vec128BitSelect()
    {
    }

    /// <summary>
    /// Always <see cref="WebAssembly.SimdOpCode.Vec128BitSelect"/>.
    /// </summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Vec128BitSelect;
    
    internal sealed override void Compile(CompilationContext context)
    {
        // TODO: Maybe add an override which accepts SimdOpCode too
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Vector128, WebAssemblyValueType.Vector128);
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Vector128);
        context.Stack.Push(WebAssemblyValueType.Vector128);

        context.Emit(OpCodes.Call, context[HelperMethod.Vec128BitSelect, (_, c) =>
        {
            // NOTE: the matching method only differs in the order of the arguments
            var typeVec128 = typeof(Vector128<uint>);
            var builder = c.CheckedExportsBuilder.DefineMethod(
                "☣ Vec128BitSelect",
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
