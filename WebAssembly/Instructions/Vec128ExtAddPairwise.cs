using System;
using System.Reflection.Emit;
using System.Runtime.Intrinsics;
using WebAssembly.Runtime.Compilation;
using static WebAssembly.SimdOpCodeExtensions;

namespace WebAssembly.Instructions;

/// <summary>
/// Implements pairwise addition of two vectors of the same type.
/// </summary>
public abstract class Vec128ExtAddPairwise : SimdInstruction
{
    private protected Vec128ExtAddPairwise() { }

    internal override void Compile(CompilationContext context)
    {
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Vector128);
        context.Stack.Push(WebAssemblyValueType.Vector128);

        var opcodeName = this.SimdOpCode.ToNativeName();

        var (srcLaneType, dstLaneType) = opcodeName switch
        {
            "i16x8.extadd_pairwise_i8x16_s" => (typeof(sbyte), typeof(short)),
            "i16x8.extadd_pairwise_i8x16_u" => (typeof(byte), typeof(ushort)),
            _ => throw new InvalidOperationException($"Unsupported opcode '{opcodeName}' for extadd pairwise."),
        };

        var vecSrc = typeof(Vector128<>).MakeGenericType(srcLaneType);

        // 1. Get MethodInfos for the required Vector128 operations.
        var createFromUlongs = FindVector128Method("Create", typeof(ulong), 1, false);
        var shuffle = FindVector128Method("Shuffle", srcLaneType, 2, true);
        var widenLower = FindVector128Method("WidenLower", srcLaneType, 1, true);
        var add = FindVector128Method("Add", dstLaneType, 2, true);

        var input = context.DeclareLocal(vecSrc);
        context.Emit(OpCodes.Stloc, input.LocalIndex);

        // 1. Load input vector for the "evens" shuffle.
        context.Emit(OpCodes.Ldloc, input.LocalIndex);

        // 2. Create and load the shuffle control vector for evens.
        context.Emit(OpCodes.Ldc_I8, 0x0E0C0A0806040200);
        context.Emit(OpCodes.Call, createFromUlongs);

        // 3. Shuffle and widen.
        context.Emit(OpCodes.Call, shuffle);
        context.Emit(OpCodes.Call, widenLower);

        // 4. Repeat for "odds": Load input, create indices, shuffle, widen.
        context.Emit(OpCodes.Ldloc, input.LocalIndex);

        context.Emit(OpCodes.Ldc_I8, 0x0F0D0B0907050301);
        context.Emit(OpCodes.Call, createFromUlongs);

        context.Emit(OpCodes.Call, shuffle);
        context.Emit(OpCodes.Call, widenLower);

        // 5. Add the two results from the stack.
        context.Emit(OpCodes.Call, add);
    }
}
