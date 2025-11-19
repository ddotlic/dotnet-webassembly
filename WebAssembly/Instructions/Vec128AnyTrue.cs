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

        const string uintLane = "i32x4";
        var zeroMethod = GetWellKnownMethod(uintLane, Zero);
        var equalsMethod = GetWellKnownMethod(uintLane, VecEquals);
        var onesComplementMethod = GetWellKnownMethod(uintLane, OnesComplement);
        var extractMsbMethod = GetWellKnownMethod(uintLane, ExtractMostSignificantBits);

        context.Emit(OpCodes.Call, zeroMethod);
        context.Emit(OpCodes.Call, equalsMethod);
        context.Emit(OpCodes.Call, onesComplementMethod);
        context.Emit(OpCodes.Call, extractMsbMethod);
        context.Emit(OpCodes.Ldc_I4_0);
        context.Emit(OpCodes.Cgt_Un);
    }
}
