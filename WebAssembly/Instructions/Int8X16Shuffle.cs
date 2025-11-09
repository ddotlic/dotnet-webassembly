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

    internal sealed override void Compile(CompilationContext context)
    {
        // Stack on entry: [v1:v128, v2:v128]
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Vector128);
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Vector128);

        var laneKind = "i8x16";
        var vType = typeof(Vector128<byte>);

        var v2Local = context.DeclareLocal(vType);
        var v1Local = context.DeclareLocal(vType);

        // Store v1 then v2 (v1 is on top)
        context.Emit(OpCodes.Stloc, v1Local.LocalIndex);
        context.Emit(OpCodes.Stloc, v2Local.LocalIndex);

        // c = Vector128.Create(c0..c15)
        for (int i = 0; i < 16; i++)
            context.Emit(OpCodes.Ldc_I4, (int)Control[i]);
        context.Emit(OpCodes.Call, FindVector128Method(nameof(Vector128.Create), typeof(byte), 16, isGeneric: false));
        var cLocal = context.DeclareLocal(vType);
        context.Emit(OpCodes.Stloc, cLocal.LocalIndex);

        // sixteen = Vector128.Create((byte)0x10)
        context.Emit(OpCodes.Ldc_I4_S, 0x10);
        context.Emit(OpCodes.Conv_U1);
        context.Emit(OpCodes.Call, FindVector128Method(nameof(Vector128.Create), typeof(byte), 1, isGeneric: false));
        var sixteenLocal = context.DeclareLocal(vType);
        context.Emit(OpCodes.Stloc, sixteenLocal.LocalIndex);

        // zero = Vector128<byte>.Zero
        var zeroLocal = context.DeclareLocal(vType);
        context.Emit(OpCodes.Call, FindVector128Getter("Zero", typeof(byte)));
        context.Emit(OpCodes.Stloc, zeroLocal.LocalIndex);

        // maskB = GreaterThanOrEqual(c, sixteen)
        context.Emit(OpCodes.Ldloc, cLocal.LocalIndex);
        context.Emit(OpCodes.Ldloc, sixteenLocal.LocalIndex);
        context.Emit(OpCodes.Call, FindVector128Method(nameof(Vector128.GreaterThanOrEqual), typeof(byte), 2, isGeneric: true));
        var maskBLocal = context.DeclareLocal(vType);
        context.Emit(OpCodes.Stloc, maskBLocal.LocalIndex);

        // cA = c & Vector128.Create(0x0F)
        context.Emit(OpCodes.Ldloc, cLocal.LocalIndex);
        context.Emit(OpCodes.Ldc_I4_S, 0x0F);
        context.Emit(OpCodes.Conv_U1);
        context.Emit(OpCodes.Call, FindVector128Method(nameof(Vector128.Create), typeof(byte), 1, isGeneric: false));
        context.Emit(OpCodes.Call, FindVector128Method(nameof(Vector128.BitwiseAnd), typeof(byte), 2, isGeneric: true));
        var cALocal = context.DeclareLocal(vType);
        context.Emit(OpCodes.Stloc, cALocal.LocalIndex);

        // cMinus16 = c - sixteen
        context.Emit(OpCodes.Ldloc, cLocal.LocalIndex);
        context.Emit(OpCodes.Ldloc, sixteenLocal.LocalIndex);
        context.Emit(OpCodes.Call, FindVector128Method(nameof(Vector128.Subtract), typeof(byte), 2, isGeneric: true));
        var cMinus16Local = context.DeclareLocal(vType);
        context.Emit(OpCodes.Stloc, cMinus16Local.LocalIndex);

        // cB = Max(cMinus16, zero)  // saturate at 0
        context.Emit(OpCodes.Ldloc, cMinus16Local.LocalIndex);
        context.Emit(OpCodes.Ldloc, zeroLocal.LocalIndex);
        context.Emit(OpCodes.Call, FindVector128Method(nameof(Vector128.Max), typeof(byte), 2, isGeneric: true));
        var cBLocal = context.DeclareLocal(vType);
        context.Emit(OpCodes.Stloc, cBLocal.LocalIndex);

        // fromA = Shuffle(v1, cA)
        context.Emit(OpCodes.Ldloc, v1Local.LocalIndex);
        context.Emit(OpCodes.Ldloc, cALocal.LocalIndex);
        context.Emit(OpCodes.Call, FindVector128Method(nameof(Vector128.Shuffle), typeof(byte), 2, isGeneric: true));
        var fromALocal = context.DeclareLocal(vType);
        context.Emit(OpCodes.Stloc, fromALocal.LocalIndex);

        // fromB = Shuffle(v2, cB)
        context.Emit(OpCodes.Ldloc, v2Local.LocalIndex);
        context.Emit(OpCodes.Ldloc, cBLocal.LocalIndex);
        context.Emit(OpCodes.Call, FindVector128Method(nameof(Vector128.Shuffle), typeof(byte), 2, isGeneric: true));
        var fromBLocal = context.DeclareLocal(vType);
        context.Emit(OpCodes.Stloc, fromBLocal.LocalIndex);

        // notMaskB = ~maskB
        context.Emit(OpCodes.Ldloc, maskBLocal.LocalIndex);
        context.Emit(OpCodes.Call, GetWellKnownMethod(laneKind, KnownMethodName.OnesComplement));
        var notMaskBLocal = context.DeclareLocal(vType);
        context.Emit(OpCodes.Stloc, notMaskBLocal.LocalIndex);

        // partA = fromA & notMaskB
        context.Emit(OpCodes.Ldloc, fromALocal.LocalIndex);
        context.Emit(OpCodes.Ldloc, notMaskBLocal.LocalIndex);
        context.Emit(OpCodes.Call, FindVector128Method(nameof(Vector128.BitwiseAnd), typeof(byte), 2, isGeneric: true));
        var partALocal = context.DeclareLocal(vType);
        context.Emit(OpCodes.Stloc, partALocal.LocalIndex);

        // partB = fromB & maskB
        context.Emit(OpCodes.Ldloc, fromBLocal.LocalIndex);
        context.Emit(OpCodes.Ldloc, maskBLocal.LocalIndex);
        context.Emit(OpCodes.Call, FindVector128Method(nameof(Vector128.BitwiseAnd), typeof(byte), 2, isGeneric: true));
        var partBLocal = context.DeclareLocal(vType);
        context.Emit(OpCodes.Stloc, partBLocal.LocalIndex);

        // result = partA | partB
        context.Emit(OpCodes.Ldloc, partALocal.LocalIndex);
        context.Emit(OpCodes.Ldloc, partBLocal.LocalIndex);
        context.Emit(OpCodes.Call, FindVector128Method(nameof(Vector128.BitwiseOr), typeof(byte), 2, isGeneric: true));

        // Push result
        context.Stack.Push(WebAssemblyValueType.Vector128);
    }
}
