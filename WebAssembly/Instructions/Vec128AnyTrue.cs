using System.Reflection.Emit;
using System.Runtime.Intrinsics;
using WebAssembly.Runtime.Compilation;
using static WebAssembly.SimdOpCodeExtensions;
using static WebAssembly.SimdOpCodeExtensions.KnownMethodName;
namespace WebAssembly.Instructions;

/// <summary>
/// Return 1 if any bit is non-zero, 0 otherwise.
/// </summary>
public class Vec128AnyTrue : SimdInstruction
{
    /// <summary>
    /// Creates a new  <see cref="Vec128AnyTrue"/> instance.
    /// </summary>
    public Vec128AnyTrue()
    {
    }

    /// <summary>
    /// Always <see cref="WebAssembly.SimdOpCode.Vec128AnyTrue"/>.
    /// </summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Vec128AnyTrue;
    
    internal sealed override void Compile(CompilationContext context)
    {
        // TODO: Maybe add an override which accepts SimdOpCode too
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Vector128);
        context.Stack.Push(WebAssemblyValueType.Int32);

        context.Emit(OpCodes.Call, context[HelperMethod.Vec128AnyTrue, (_, c) =>
        {
            var builder = c.CheckedExportsBuilder.DefineMethod(
                "☣ Vec128AnyTrue",
                CompilationContext.HelperMethodAttributes,
                typeof(int),
                [
                    typeof(Vector128<uint>),
                ]
                );
            
            const string uintLane = "i32x4";
            
            var il = builder.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, GetWellKnownMethod(uintLane, Zero));
            il.Emit(OpCodes.Call, GetWellKnownMethod(uintLane, VecEquals));
            il.Emit(OpCodes.Call, GetWellKnownMethod(uintLane, OnesComplement));
            il.Emit(OpCodes.Call, GetWellKnownMethod(uintLane, ExtractMostSignificantBits));
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Cgt_Un);
            il.Emit(OpCodes.Ret);
            return builder;
        }
        ]);
    }
}
