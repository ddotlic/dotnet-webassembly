using System.Reflection.Emit;
using System.Runtime.Intrinsics;
using WebAssembly.Runtime.Compilation;
using static WebAssembly.SimdOpCodeExtensions;

namespace WebAssembly.Instructions;

/// <summary>
/// Shuffle the elements of two vectors according to the control bytes.
/// </summary>
public class Int8X16Shuffle : SimdInstruction
{
    /// <summary>
    /// The control bytes.
    /// </summary>
    public byte[] Control { get; } = new byte[16];

    /// <summary>
    /// Creates a new  <see cref="Int8X16Shuffle"/> instance.   
    /// </summary>
    public Int8X16Shuffle() { }

    internal Int8X16Shuffle(Reader reader)
    {
        for (var i = 0; i < 16; i++)
            Control[i] = reader.ReadByte();
    }

    /// <summary>
    /// Always <see cref="SimdOpCode.Int8X16Shuffle"/>.   
    /// </summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8X16Shuffle;

    internal sealed override void WriteTo(Writer writer)
    {
        base.WriteTo(writer);
        for (var i = 0; i < 16; i++)
            writer.WriteVar(Control[i]);
    }

    // The following code is an IL version of this C#, except for ReadOnlySpan usage, control (local c) is read from stream:
    // static Vector128<byte> WasmShuffle(Vector128<byte> a, Vector128<byte> b, ReadOnlySpan<byte> ctrl16) {
    //     
    //     var c = Vector128.Create(
    //         ctrl16[0], ctrl16[1], ctrl16[2], ctrl16[3],
    //         ctrl16[4], ctrl16[5], ctrl16[6], ctrl16[7],
    //         ctrl16[8], ctrl16[9], ctrl16[10], ctrl16[11],
    //         ctrl16[12], ctrl16[13], ctrl16[14], ctrl16[15]);
    // 
    //     var sixteen = Vector128.Create((byte)0x10);
    //     var zero = Vector128<byte>.Zero;
    // 
    //     var maskB = Vector128.GreaterThanOrEqual(c, sixteen);
    // 
    //     var cA = Vector128.BitwiseAnd(c, Vector128.Create((byte)0x0F));
    //     var cMinus16 = Vector128.Subtract(c, sixteen);
    //     var cB = Vector128.Max(cMinus16, zero);
    // 
    //     var fromA = Vector128.Shuffle(a, cA);
    //     var fromB = Vector128.Shuffle(b, cB);
    // 
    //     var res = Vector128.BitwiseOr(
    //         Vector128.BitwiseAnd(fromA, Vector128.OnesComplement(maskB)),
    //         Vector128.BitwiseAnd(fromB, maskB));
    // 
    //     return res;
    // }
    internal sealed override void Compile(CompilationContext context)
    {
        // Stack on entry: [v1:v128, v2:v128]
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Vector128);
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Vector128);

        var laneKind = "i8x16";
        var vType = typeof(Vector128<byte>);
        
        // Cache frequently used method references
        // TODO: consider adding some of these to the 'well known' cache.
        var createByteScalar = FindVector128Method(nameof(Vector128.Create), typeof(byte), 1, isGeneric: false);
        var bitwiseAnd = FindVector128Method(nameof(Vector128.BitwiseAnd), typeof(byte), 2, isGeneric: true);
        var bitwiseOr = FindVector128Method(nameof(Vector128.BitwiseOr), typeof(byte), 2, isGeneric: true);
        var subtract = FindVector128Method(nameof(Vector128.Subtract), typeof(byte), 2, isGeneric: true);
        var max = FindVector128Method(nameof(Vector128.Max), typeof(byte), 2, isGeneric: true);
        var shuffle = FindVector128Method(nameof(Vector128.Shuffle), typeof(byte), 2, isGeneric: true);
        var greaterThanOrEqual = FindVector128Method(nameof(Vector128.GreaterThanOrEqual), typeof(byte), 2, isGeneric: true);
        var onesComplement = GetWellKnownMethod(laneKind, KnownMethodName.OnesComplement);

        var v2Local = context.DeclareLocal(vType);
        var v1Local = context.DeclareLocal(vType);

        // Store v2 then v1 (v2 is on top of stack, v1 is below)
        context.Emit(OpCodes.Stloc, v2Local.LocalIndex);
        context.Emit(OpCodes.Stloc, v1Local.LocalIndex);

        // c = Vector128.Create(c0..c15)
        for (var i = 0; i < 16; i++)
            context.Emit(OpCodes.Ldc_I4, (int)Control[i]);
        context.Emit(OpCodes.Call, FindVector128Method(nameof(Vector128.Create), typeof(byte), 16, isGeneric: false));
        var cLocal = context.DeclareLocal(vType);
        context.Emit(OpCodes.Stloc, cLocal.LocalIndex);

        // sixteen = Vector128.Create((byte)0x10)
        context.Emit(OpCodes.Ldc_I4_S, 0x10);
        context.Emit(OpCodes.Conv_U1);
        context.Emit(OpCodes.Call, createByteScalar);
        var sixteenLocal = context.DeclareLocal(vType);
        context.Emit(OpCodes.Stloc, sixteenLocal.LocalIndex);

        // zero = Vector128<byte>.Zero
        var zeroLocal = context.DeclareLocal(vType);
        context.Emit(OpCodes.Call, FindVector128Getter("Zero", typeof(byte)));
        context.Emit(OpCodes.Stloc, zeroLocal.LocalIndex);

        // maskB = GreaterThanOrEqual(c, sixteen)
        context.Emit(OpCodes.Ldloc, cLocal.LocalIndex);
        context.Emit(OpCodes.Ldloc, sixteenLocal.LocalIndex);
        context.Emit(OpCodes.Call, greaterThanOrEqual);
        var maskBLocal = context.DeclareLocal(vType);
        context.Emit(OpCodes.Stloc, maskBLocal.LocalIndex);

        // cA = c & Vector128.Create(0x0F)
        context.Emit(OpCodes.Ldloc, cLocal.LocalIndex);
        context.Emit(OpCodes.Ldc_I4_S, 0x0F);
        context.Emit(OpCodes.Conv_U1);
        context.Emit(OpCodes.Call, createByteScalar);
        context.Emit(OpCodes.Call, bitwiseAnd);
        var cALocal = context.DeclareLocal(vType);
        context.Emit(OpCodes.Stloc, cALocal.LocalIndex);

        // cMinus16 = c - sixteen
        context.Emit(OpCodes.Ldloc, cLocal.LocalIndex);
        context.Emit(OpCodes.Ldloc, sixteenLocal.LocalIndex);
        context.Emit(OpCodes.Call, subtract);
        var cMinus16Local = context.DeclareLocal(vType);
        context.Emit(OpCodes.Stloc, cMinus16Local.LocalIndex);

        // cB = Max(cMinus16, zero)  // saturate at 0
        context.Emit(OpCodes.Ldloc, cMinus16Local.LocalIndex);
        context.Emit(OpCodes.Ldloc, zeroLocal.LocalIndex);
        context.Emit(OpCodes.Call, max);
        var cBLocal = context.DeclareLocal(vType);
        context.Emit(OpCodes.Stloc, cBLocal.LocalIndex);

        // fromA = Shuffle(v1, cA)
        context.Emit(OpCodes.Ldloc, v1Local.LocalIndex);
        context.Emit(OpCodes.Ldloc, cALocal.LocalIndex);
        context.Emit(OpCodes.Call, shuffle);
        var fromALocal = context.DeclareLocal(vType);
        context.Emit(OpCodes.Stloc, fromALocal.LocalIndex);

        // fromB = Shuffle(v2, cB)
        context.Emit(OpCodes.Ldloc, v2Local.LocalIndex);
        context.Emit(OpCodes.Ldloc, cBLocal.LocalIndex);
        context.Emit(OpCodes.Call, shuffle);
        var fromBLocal = context.DeclareLocal(vType);
        context.Emit(OpCodes.Stloc, fromBLocal.LocalIndex);

        // notMaskB = ~maskB
        context.Emit(OpCodes.Ldloc, maskBLocal.LocalIndex);
        context.Emit(OpCodes.Call, onesComplement);
        var notMaskBLocal = context.DeclareLocal(vType);
        context.Emit(OpCodes.Stloc, notMaskBLocal.LocalIndex);

        // partA = fromA & notMaskB
        context.Emit(OpCodes.Ldloc, fromALocal.LocalIndex);
        context.Emit(OpCodes.Ldloc, notMaskBLocal.LocalIndex);
        context.Emit(OpCodes.Call, bitwiseAnd);
        var partALocal = context.DeclareLocal(vType);
        context.Emit(OpCodes.Stloc, partALocal.LocalIndex);

        // partB = fromB & maskB
        context.Emit(OpCodes.Ldloc, fromBLocal.LocalIndex);
        context.Emit(OpCodes.Ldloc, maskBLocal.LocalIndex);
        context.Emit(OpCodes.Call, bitwiseAnd);
        var partBLocal = context.DeclareLocal(vType);
        context.Emit(OpCodes.Stloc, partBLocal.LocalIndex);

        // result = partA | partB
        context.Emit(OpCodes.Ldloc, partALocal.LocalIndex);
        context.Emit(OpCodes.Ldloc, partBLocal.LocalIndex);
        context.Emit(OpCodes.Call, bitwiseOr);

        // Push result
        context.Stack.Push(WebAssemblyValueType.Vector128);
    }
}
