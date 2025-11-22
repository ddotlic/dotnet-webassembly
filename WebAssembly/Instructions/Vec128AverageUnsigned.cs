using System.Reflection.Emit;
using System.Runtime.Intrinsics;
using WebAssembly.Runtime.Compilation;
using static WebAssembly.SimdOpCodeExtensions;

namespace WebAssembly.Instructions;

/// <summary>
/// Base implementation for lane-wise unsigned rounding average instructions.
/// </summary>
public abstract class Vec128AverageUnsigned : SimdInstruction
{
    private protected Vec128AverageUnsigned() { }

    internal override void Compile(CompilationContext context)
    {
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Vector128, WebAssemblyValueType.Vector128);
        context.Stack.Push(WebAssemblyValueType.Vector128);

        var laneKind = this.SimdOpCode.ToLaneKind();
        var parType = laneKind switch
        {
            "i8x16" => typeof(byte),
            _ => typeof(byte), // TODO: extend support to i16x8 when opcode is enabled.
        };

        var vecType = typeof(Vector128<>).MakeGenericType(parType);

        var add = FindVector128Method("Add", parType, 2, true);
        var xor = FindVector128Method("Xor", parType, 2, true);
        var bitwiseAnd = FindVector128Method("BitwiseAnd", parType, 2, true);
        var shiftRightLogical = FindVector128Method("ShiftRightLogical", parType, 2, false);
        var create = FindVector128Method("Create", parType, 1, false);

        var localA = context.DeclareLocal(vecType);
        var localB = context.DeclareLocal(vecType);
        var localXor = context.DeclareLocal(vecType);
        var localAnd = context.DeclareLocal(vecType);
        var localOddMask = context.DeclareLocal(vecType);
        var localHalfXor = context.DeclareLocal(vecType);
        var localTemp = context.DeclareLocal(vecType);
        var localResult = context.DeclareLocal(vecType);
        var localOne = context.DeclareLocal(vecType);

        // store operands (stack top is b, then a)
        context.Emit(OpCodes.Stloc, localB.LocalIndex);
        context.Emit(OpCodes.Stloc, localA.LocalIndex);

        // one = Vector128<byte>.Create(1)
        context.Emit(OpCodes.Ldc_I4_1);
        context.Emit(OpCodes.Call, create);
        context.Emit(OpCodes.Stloc, localOne.LocalIndex);

        // xor = Vector128.Xor(a, b)
        context.Emit(OpCodes.Ldloc, localA.LocalIndex);
        context.Emit(OpCodes.Ldloc, localB.LocalIndex);
        context.Emit(OpCodes.Call, xor);
        context.Emit(OpCodes.Stloc, localXor.LocalIndex);

        // and = Vector128.BitwiseAnd(a, b)
        context.Emit(OpCodes.Ldloc, localA.LocalIndex);
        context.Emit(OpCodes.Ldloc, localB.LocalIndex);
        context.Emit(OpCodes.Call, bitwiseAnd);
        context.Emit(OpCodes.Stloc, localAnd.LocalIndex);

        // oddMask = Vector128.BitwiseAnd(xor, one)
        context.Emit(OpCodes.Ldloc, localXor.LocalIndex);
        context.Emit(OpCodes.Ldloc, localOne.LocalIndex);
        context.Emit(OpCodes.Call, bitwiseAnd);
        context.Emit(OpCodes.Stloc, localOddMask.LocalIndex);

        // halfXor = Vector128.ShiftRightLogical(xor, 1)
        context.Emit(OpCodes.Ldloc, localXor.LocalIndex);
        context.Emit(OpCodes.Ldc_I4_1);
        context.Emit(OpCodes.Call, shiftRightLogical);
        context.Emit(OpCodes.Stloc, localHalfXor.LocalIndex);

        // temp = Vector128.Add(and, oddMask)
        context.Emit(OpCodes.Ldloc, localAnd.LocalIndex);
        context.Emit(OpCodes.Ldloc, localOddMask.LocalIndex);
        context.Emit(OpCodes.Call, add);
        context.Emit(OpCodes.Stloc, localTemp.LocalIndex);

        // result = Vector128.Add(temp, halfXor)
        context.Emit(OpCodes.Ldloc, localTemp.LocalIndex);
        context.Emit(OpCodes.Ldloc, localHalfXor.LocalIndex);
        context.Emit(OpCodes.Call, add);
        context.Emit(OpCodes.Stloc, localResult.LocalIndex);

        context.Emit(OpCodes.Ldloc, localResult.LocalIndex);
    }
}
