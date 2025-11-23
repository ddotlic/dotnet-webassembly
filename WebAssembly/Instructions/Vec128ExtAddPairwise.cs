using System;
using System.Reflection.Emit;
using System.Runtime.Intrinsics;
using WebAssembly.Runtime.Compilation;
using static WebAssembly.SimdOpCodeExtensions;

namespace WebAssembly.Instructions;

/// <summary>
/// Implements i16x8.extadd_pairwise_i8x16_{s|u} (extensible for other lanes).
/// </summary>
public abstract class Vec128ExtAddPairwise : SimdInstruction
{
    private protected Vec128ExtAddPairwise() { }

    internal override void Compile(CompilationContext context)
    {
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Vector128);
        context.Stack.Push(WebAssemblyValueType.Vector128);

        var opcodeName = this.SimdOpCode.ToNativeName();
        var isSigned = opcodeName.EndsWith("_s", StringComparison.InvariantCulture);

        var (srcLaneType, dstLaneType) = opcodeName switch
        {
            "i16x8.extadd_pairwise_i8x16_s" => (typeof(sbyte), typeof(short)),
            "i16x8.extadd_pairwise_i8x16_u" => (typeof(byte), typeof(ushort)),
            _ => throw new InvalidOperationException($"Unsupported opcode '{opcodeName}' for extadd pairwise."),
        };

        var vecSrc = typeof(Vector128<>).MakeGenericType(srcLaneType);
        var vecDst = typeof(Vector128<>).MakeGenericType(dstLaneType);

        // 1. Get MethodInfos for the required Vector128 operations.
        var createBytes = FindVector128Method("Create", typeof(byte), 16, false);
        var shuffle = FindVector128Method("Shuffle", srcLaneType, 2, true);
        var widenLower = FindVector128Method("WidenLower", srcLaneType, 1, false);
        var add = FindVector128Method("Add", dstLaneType, 2, true);

        var input = context.DeclareLocal(vecSrc);
        context.Emit(OpCodes.Stloc, input.LocalIndex);

        // 2. Create shuffle control vectors.
        var evensIndices = context.DeclareLocal(vecSrc);
        context.Emit(OpCodes.Ldc_I4_0);
        context.Emit(OpCodes.Ldc_I4_2);
        context.Emit(OpCodes.Ldc_I4_4);
        context.Emit(OpCodes.Ldc_I4_6);
        context.Emit(OpCodes.Ldc_I4_8);
        context.Emit(OpCodes.Ldc_I4, 10);
        context.Emit(OpCodes.Ldc_I4, 12);
        context.Emit(OpCodes.Ldc_I4, 14);
        context.Emit(OpCodes.Ldc_I4_0);
        context.Emit(OpCodes.Ldc_I4_0);
        context.Emit(OpCodes.Ldc_I4_0);
        context.Emit(OpCodes.Ldc_I4_0);
        context.Emit(OpCodes.Ldc_I4_0);
        context.Emit(OpCodes.Ldc_I4_0);
        context.Emit(OpCodes.Ldc_I4_0);
        context.Emit(OpCodes.Ldc_I4_0);
        context.Emit(OpCodes.Call, createBytes);
        // context.Emit(OpCodes.Call, asSrc);
        context.Emit(OpCodes.Stloc, evensIndices.LocalIndex);

        var oddsIndices = context.DeclareLocal(vecSrc);
        context.Emit(OpCodes.Ldc_I4_1);
        context.Emit(OpCodes.Ldc_I4_3);
        context.Emit(OpCodes.Ldc_I4_5);
        context.Emit(OpCodes.Ldc_I4_7);
        context.Emit(OpCodes.Ldc_I4, 9);
        context.Emit(OpCodes.Ldc_I4, 11);
        context.Emit(OpCodes.Ldc_I4, 13);
        context.Emit(OpCodes.Ldc_I4, 15);
        context.Emit(OpCodes.Ldc_I4_0);
        context.Emit(OpCodes.Ldc_I4_0);
        context.Emit(OpCodes.Ldc_I4_0);
        context.Emit(OpCodes.Ldc_I4_0);
        context.Emit(OpCodes.Ldc_I4_0);
        context.Emit(OpCodes.Ldc_I4_0);
        context.Emit(OpCodes.Ldc_I4_0);
        context.Emit(OpCodes.Ldc_I4_0);
        context.Emit(OpCodes.Call, createBytes);
        context.Emit(OpCodes.Stloc, oddsIndices.LocalIndex);

        // 3. Shuffle to isolate even and odd bytes into the lower 8 bytes of two vectors.
        var evens = context.DeclareLocal(vecSrc);
        context.Emit(OpCodes.Ldloc, input.LocalIndex);
        context.Emit(OpCodes.Ldloc, evensIndices.LocalIndex);
        context.Emit(OpCodes.Call, shuffle);
        context.Emit(OpCodes.Stloc, evens.LocalIndex);

        var odds = context.DeclareLocal(vecSrc);
        context.Emit(OpCodes.Ldloc, input.LocalIndex);
        context.Emit(OpCodes.Ldloc, oddsIndices.LocalIndex);
        context.Emit(OpCodes.Call, shuffle);
        context.Emit(OpCodes.Stloc, odds.LocalIndex);

        // 4. Widen the lower 8 bytes of each shuffled vector.
        var evensWider = context.DeclareLocal(vecDst);
        context.Emit(OpCodes.Ldloc, evens.LocalIndex);
        context.Emit(OpCodes.Call, widenLower);
        context.Emit(OpCodes.Stloc, evensWider.LocalIndex);

        var oddsWider = context.DeclareLocal(vecDst);
        context.Emit(OpCodes.Ldloc, odds.LocalIndex);
        context.Emit(OpCodes.Call, widenLower);
        context.Emit(OpCodes.Stloc, oddsWider.LocalIndex);

        // 5. Add the two widened vectors to get the final result.
        context.Emit(OpCodes.Ldloc, evensWider.LocalIndex);
        context.Emit(OpCodes.Ldloc, oddsWider.LocalIndex);
        context.Emit(OpCodes.Call, add);
    }
}
