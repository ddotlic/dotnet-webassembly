using System.Reflection.Emit;
using System.Runtime.Intrinsics;
using WebAssembly.Runtime.Compilation;
using static WebAssembly.SimdOpCodeExtensions;

namespace WebAssembly.Instructions;

/// <summary>
/// Implements i32x4.dot_i16x8_s: pairwise dot product of i16 lanes into i32.
/// result[i] = (a[2*i] * b[2*i]) + (a[2*i+1] * b[2*i+1])
/// </summary>
public sealed class Int32X4DotInt16X8Signed : SimdInstruction
{
    /// <summary>
    /// Creates a new <see cref="Int32X4DotInt16X8Signed"/> instance.
    /// </summary>
    public Int32X4DotInt16X8Signed() { }

    /// <summary>
    /// Always <see cref="SimdOpCode.Int32X4DotInt16X8Signed"/>.
    /// </summary>
    public override SimdOpCode SimdOpCode => SimdOpCode.Int32X4DotInt16X8Signed;

    internal override void Compile(CompilationContext context)
    {
        context.PopStackNoReturn(OpCode, WebAssemblyValueType.Vector128, WebAssemblyValueType.Vector128);
        context.Stack.Push(WebAssemblyValueType.Vector128);

        var vecI16 = typeof(Vector128<short>);
        var vecI32 = typeof(Vector128<int>);

        var widenLower = FindVector128Method("WidenLower", typeof(short), 1, true);
        var widenUpper = FindVector128Method("WidenUpper", typeof(short), 1, true);
        var multiply = FindVector128Method("Multiply", typeof(int), 2, true);
        var add = FindVector128Method("Add", typeof(int), 2, true);
        var shuffle = FindVector128Method("Shuffle", typeof(int), 2, false);
        var createFromUlong = FindVector128Method("Create", typeof(ulong), 1, false);
        var getElement = FindVector128Method("GetElement", typeof(int), 2, true);
        var createFromInts = FindVector128Method("Create", typeof(int), 4, false);

        // Store inputs to locals
        var v2 = context.DeclareLocal(vecI16);
        var v1 = context.DeclareLocal(vecI16);
        context.Emit(OpCodes.Stloc, v2.LocalIndex);
        context.Emit(OpCodes.Stloc, v1.LocalIndex);

        // Compute lower products: [p0, p1, p2, p3]
        context.Emit(OpCodes.Ldloc, v1.LocalIndex);
        context.Emit(OpCodes.Call, widenLower);
        context.Emit(OpCodes.Ldloc, v2.LocalIndex);
        context.Emit(OpCodes.Call, widenLower);
        context.Emit(OpCodes.Call, multiply);

        var lowProd = context.DeclareLocal(vecI32);
        context.Emit(OpCodes.Stloc, lowProd.LocalIndex);

        // Compute upper products: [p4, p5, p6, p7]
        context.Emit(OpCodes.Ldloc, v1.LocalIndex);
        context.Emit(OpCodes.Call, widenUpper);
        context.Emit(OpCodes.Ldloc, v2.LocalIndex);
        context.Emit(OpCodes.Call, widenUpper);
        context.Emit(OpCodes.Call, multiply);

        var highProd = context.DeclareLocal(vecI32);
        context.Emit(OpCodes.Stloc, highProd.LocalIndex);

        // Shuffle lowProd to get evens and odds, then add
        // lowEvens = Shuffle(lowProd, [0, 2, 0, 2])
        // lowOdds = Shuffle(lowProd, [1, 3, 1, 3])
        // lowSums = lowEvens + lowOdds = [p0+p1, p2+p3, ...]
        context.Emit(OpCodes.Ldloc, lowProd.LocalIndex);
        context.Emit(OpCodes.Ldc_I8, 0x00000002_00000000);
        context.Emit(OpCodes.Call, createFromUlong);
        context.Emit(OpCodes.Call, shuffle);

        context.Emit(OpCodes.Ldloc, lowProd.LocalIndex);
        context.Emit(OpCodes.Ldc_I8, 0x00000003_00000001);
        context.Emit(OpCodes.Call, createFromUlong);
        context.Emit(OpCodes.Call, shuffle);

        context.Emit(OpCodes.Call, add);
        var lowSums = context.DeclareLocal(vecI32);
        context.Emit(OpCodes.Stloc, lowSums.LocalIndex);

        // Same for highProd
        context.Emit(OpCodes.Ldloc, highProd.LocalIndex);
        context.Emit(OpCodes.Ldc_I8, 0x00000002_00000000);
        context.Emit(OpCodes.Call, createFromUlong);
        context.Emit(OpCodes.Call, shuffle);

        context.Emit(OpCodes.Ldloc, highProd.LocalIndex);
        context.Emit(OpCodes.Ldc_I8, 0x00000003_00000001);
        context.Emit(OpCodes.Call, createFromUlong);
        context.Emit(OpCodes.Call, shuffle);

        context.Emit(OpCodes.Call, add);
        var highSums = context.DeclareLocal(vecI32);
        context.Emit(OpCodes.Stloc, highSums.LocalIndex);

        // Combine: Vector128.Create(lowSums.GetLower(), highSums.GetLower())
        context.Emit(OpCodes.Ldloc, lowSums.LocalIndex);
        context.Emit(OpCodes.Ldc_I4_0);
        context.Emit(OpCodes.Call, getElement);
        context.Emit(OpCodes.Ldloc, lowSums.LocalIndex);
        context.Emit(OpCodes.Ldc_I4_1);
        context.Emit(OpCodes.Call, getElement);
        context.Emit(OpCodes.Ldloc, highSums.LocalIndex);
        context.Emit(OpCodes.Ldc_I4_0);
        context.Emit(OpCodes.Call, getElement);
        context.Emit(OpCodes.Ldloc, highSums.LocalIndex);
        context.Emit(OpCodes.Ldc_I4_1);
        context.Emit(OpCodes.Call, getElement);
        context.Emit(OpCodes.Call, createFromInts);
    }
}
