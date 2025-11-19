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
        // Reads a mask and two vectors from the stack (mask is on top, followed by the "true" and "false" operands).
        var vectorType = typeof(Vector128<uint>);

        var maskLocal = context.DeclareLocal(vectorType);
        var falseLocal = context.DeclareLocal(vectorType);
        var trueLocal = context.DeclareLocal(vectorType);

        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Vector128, WebAssemblyValueType.Vector128);
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Vector128);

        context.Emit(OpCodes.Stloc, maskLocal.LocalIndex);
        context.Emit(OpCodes.Stloc, falseLocal.LocalIndex);
        context.Emit(OpCodes.Stloc, trueLocal.LocalIndex);

        context.Emit(OpCodes.Ldloc, maskLocal.LocalIndex);
        context.Emit(OpCodes.Ldloc, trueLocal.LocalIndex);
        context.Emit(OpCodes.Ldloc, falseLocal.LocalIndex);

        context.Emit(OpCodes.Call, this.SimdOpCode.ToMethodInfo());
        context.Stack.Push(WebAssemblyValueType.Vector128);
    }
}
