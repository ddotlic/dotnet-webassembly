using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;
using static WebAssembly.SimdOpCodeExtensions;

namespace WebAssembly.Instructions;

/// <summary>
/// Implements f32x4.pmin, f32x4.pmax, f64x2.pmin, f64x2.pmax.
/// pmin(a, b) = b &lt; a ? b : a
/// pmax(a, b) = a &lt; b ? b : a
/// </summary>
public abstract class Vec128PseudoMinMax : SimdInstruction
{
    private protected Vec128PseudoMinMax() { }

    internal override void Compile(CompilationContext context)
    {
        context.PopStackNoReturn(OpCode, WebAssemblyValueType.Vector128, WebAssemblyValueType.Vector128);
        context.Stack.Push(WebAssemblyValueType.Vector128);

        var isPmin = SimdOpCode is SimdOpCode.Float32X4PMin or SimdOpCode.Float64X2PMin;
        var laneType = SimdOpCode is SimdOpCode.Float32X4PMin or SimdOpCode.Float32X4PMax
            ? typeof(float)
            : typeof(double);

        var lessThan = FindVector128Method("LessThan", laneType, 2, true);
        var conditionalSelect = FindVector128Method("ConditionalSelect", laneType, 3, true);

        // Stack: [a, b]
        // For pmin: mask = LessThan(b, a); result = ConditionalSelect(mask, b, a)
        // For pmax: mask = LessThan(a, b); result = ConditionalSelect(mask, b, a)

        var vecType = typeof(System.Runtime.Intrinsics.Vector128<>).MakeGenericType(laneType);
        var b = context.DeclareLocal(vecType);
        var a = context.DeclareLocal(vecType);
        context.Emit(OpCodes.Stloc, b.LocalIndex);
        context.Emit(OpCodes.Stloc, a.LocalIndex);

        if (isPmin)
        {
            // LessThan(b, a)
            context.Emit(OpCodes.Ldloc, b.LocalIndex);
            context.Emit(OpCodes.Ldloc, a.LocalIndex);
        }
        else
        {
            // LessThan(a, b)
            context.Emit(OpCodes.Ldloc, a.LocalIndex);
            context.Emit(OpCodes.Ldloc, b.LocalIndex);
        }
        context.Emit(OpCodes.Call, lessThan);

        // ConditionalSelect(mask, b, a)
        context.Emit(OpCodes.Ldloc, b.LocalIndex);
        context.Emit(OpCodes.Ldloc, a.LocalIndex);
        context.Emit(OpCodes.Call, conditionalSelect);
    }
}
