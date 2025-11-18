using System;
using System.Reflection.Emit;
using System.Runtime.Intrinsics;
using WebAssembly.Runtime.Compilation;
using static WebAssembly.SimdOpCodeExtensions;
using static WebAssembly.SimdOpCodeExtensions.KnownMethodName;

namespace WebAssembly.Instructions;

/// <summary>
/// Implements i8x16.sub_saturate_s/u and i16x8.sub_saturate_s/u
/// as a sequence of Vector128 operations.
/// </summary>
public abstract class Vec128SubSaturate : SimdInstruction
{
    private protected Vec128SubSaturate() { }

    internal override void Compile(CompilationContext context)
    {
        var stack = context.Stack;
        // validation stack
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Vector128, WebAssemblyValueType.Vector128);
        stack.Push(WebAssemblyValueType.Vector128);

        var laneKind = this.SimdOpCode.ToLaneKind(); // e.g. "i8x16" or "i16x8"
        var isSigned = this.SimdOpCode.ToNativeName().EndsWith("_s", StringComparison.InvariantCulture);

        var parType = laneKind switch
        {
            "i8x16" => isSigned ? typeof(sbyte) : typeof(byte),
            "i16x8" => isSigned ? typeof(short) : typeof(ushort),
            _ => throw new InvalidOperationException($"Unsupported lane kind: {laneKind}"),
        };

        // Cache method infos
        var sub = FindVector128Method("Subtract", parType, 2, true);
        var xor = FindVector128Method("Xor", parType, 2, true);
        var bitwiseAnd = FindVector128Method("BitwiseAnd", parType, 2, true);
        var lessThan = FindVector128Method("LessThan", parType, 2, true);
        var greaterThan = FindVector128Method("GreaterThan", parType, 2, true);
        var conditionalSelect = FindVector128Method("ConditionalSelect", parType, 3, true);
        var create = FindVector128Method("Create", parType, 1, false);
        var zero = GetWellKnownMethod(laneKind, Zero);

        // Compute integer constants for signed lanes
        var maxInt = laneKind switch
        {
            "i8x16" when isSigned => 127,
            "i8x16" when !isSigned => 0xFF,
            "i16x8" when isSigned => 32767,
            "i16x8" when !isSigned => 0xFFFF,
            _ => throw new InvalidOperationException("Unsupported lane kind for constants."),
        };
        var minInt = laneKind switch
        {
            "i8x16" when isSigned => -128,
            "i8x16" when !isSigned => 0,
            "i16x8" when isSigned => -32768,
            "i16x8" when !isSigned => 0,
            _ => throw new InvalidOperationException("Unsupported lane kind for constants."),
        };

        // Reserve locals for A, B and T
        var vecType = typeof(Vector128<>).MakeGenericType(parType);
        var localA = context.DeclareLocal(vecType);
        var localB = context.DeclareLocal(vecType);
        var localT = context.DeclareLocal(vecType);

        // store incoming operands (stack top b, then a)
        context.Emit(OpCodes.Stloc, localB.LocalIndex);
        context.Emit(OpCodes.Stloc, localA.LocalIndex);

        // t = Subtract(a, b)
        context.Emit(OpCodes.Ldloc, localA.LocalIndex);
        context.Emit(OpCodes.Ldloc, localB.LocalIndex);
        context.Emit(OpCodes.Call, sub);
        context.Emit(OpCodes.Stloc, localT.LocalIndex);

        if (!isSigned)
        {
            // underflowMask = LessThan(a, b)  (unsigned)
            var localUnderflow = context.DeclareLocal(vecType);
            context.Emit(OpCodes.Ldloc, localA.LocalIndex);
            context.Emit(OpCodes.Ldloc, localB.LocalIndex);
            context.Emit(OpCodes.Call, lessThan);
            context.Emit(OpCodes.Stloc, localUnderflow.LocalIndex);

            // result = ConditionalSelect(underflowMask, zero, t)
            context.Emit(OpCodes.Ldloc, localUnderflow.LocalIndex);
            context.Emit(OpCodes.Call, zero);
            context.Emit(OpCodes.Ldloc, localT.LocalIndex);
            context.Emit(OpCodes.Call, conditionalSelect);
            context.Emit(OpCodes.Stloc, localT.LocalIndex);

            context.Emit(OpCodes.Ldloc, localT.LocalIndex);
            return;
        }

        // signed path
        // xorAB = Xor(a, b)
        var localXorAB = context.DeclareLocal(vecType);
        context.Emit(OpCodes.Ldloc, localA.LocalIndex);
        context.Emit(OpCodes.Ldloc, localB.LocalIndex);
        context.Emit(OpCodes.Call, xor);
        context.Emit(OpCodes.Stloc, localXorAB.LocalIndex);

        // xorAT = Xor(a, t)
        var localXorAT = context.DeclareLocal(vecType);
        context.Emit(OpCodes.Ldloc, localA.LocalIndex);
        context.Emit(OpCodes.Ldloc, localT.LocalIndex);
        context.Emit(OpCodes.Call, xor);
        context.Emit(OpCodes.Stloc, localXorAT.LocalIndex);

        // overflowTest = BitwiseAnd(xorAB, xorAT)
        var localOverflowTest = context.DeclareLocal(vecType);
        context.Emit(OpCodes.Ldloc, localXorAB.LocalIndex);
        context.Emit(OpCodes.Ldloc, localXorAT.LocalIndex);
        context.Emit(OpCodes.Call, bitwiseAnd);
        context.Emit(OpCodes.Stloc, localOverflowTest.LocalIndex);

        // overflowMask = LessThan(overflowTest, zero)
        var localOverflowMask = context.DeclareLocal(vecType);
        context.Emit(OpCodes.Ldloc, localOverflowTest.LocalIndex);
        context.Emit(OpCodes.Call, zero);
        context.Emit(OpCodes.Call, lessThan);
        context.Emit(OpCodes.Stloc, localOverflowMask.LocalIndex);

        // aPositiveMask = GreaterThan(a, zero)
        var localAPos = context.DeclareLocal(vecType);
        context.Emit(OpCodes.Ldloc, localA.LocalIndex);
        context.Emit(OpCodes.Call, zero);
        context.Emit(OpCodes.Call, greaterThan);
        context.Emit(OpCodes.Stloc, localAPos.LocalIndex);

        // aNegativeMask = LessThan(a, zero)
        var localANeg = context.DeclareLocal(vecType);
        context.Emit(OpCodes.Ldloc, localA.LocalIndex);
        context.Emit(OpCodes.Call, zero);
        context.Emit(OpCodes.Call, lessThan);
        context.Emit(OpCodes.Stloc, localANeg.LocalIndex);

        // posOverflowMask = BitwiseAnd(overflowMask, aPositiveMask)
        var localPosOverflow = context.DeclareLocal(vecType);
        context.Emit(OpCodes.Ldloc, localOverflowMask.LocalIndex);
        context.Emit(OpCodes.Ldloc, localAPos.LocalIndex);
        context.Emit(OpCodes.Call, bitwiseAnd);
        context.Emit(OpCodes.Stloc, localPosOverflow.LocalIndex);

        // negOverflowMask = BitwiseAnd(overflowMask, aNegativeMask)
        var localNegOverflow = context.DeclareLocal(vecType);
        context.Emit(OpCodes.Ldloc, localOverflowMask.LocalIndex);
        context.Emit(OpCodes.Ldloc, localANeg.LocalIndex);
        context.Emit(OpCodes.Call, bitwiseAnd);
        context.Emit(OpCodes.Stloc, localNegOverflow.LocalIndex);

        // maxVector = Create(maxInt)
        var localMaxVec = context.DeclareLocal(vecType);
        context.Emit(OpCodes.Ldc_I4, maxInt);
        context.Emit(OpCodes.Call, create);
        context.Emit(OpCodes.Stloc, localMaxVec.LocalIndex);

        // minVector = Create(minInt)
        var localMinVec = context.DeclareLocal(vecType);
        context.Emit(OpCodes.Ldc_I4, minInt);
        context.Emit(OpCodes.Call, create);
        context.Emit(OpCodes.Stloc, localMinVec.LocalIndex);

        // result = ConditionalSelect(negOverflowMask, minVector, t)
        context.Emit(OpCodes.Ldloc, localNegOverflow.LocalIndex);
        context.Emit(OpCodes.Ldloc, localMinVec.LocalIndex);
        context.Emit(OpCodes.Ldloc, localT.LocalIndex);
        context.Emit(OpCodes.Call, conditionalSelect);
        context.Emit(OpCodes.Stloc, localT.LocalIndex);

        // result = ConditionalSelect(posOverflowMask, maxVector, t)
        context.Emit(OpCodes.Ldloc, localPosOverflow.LocalIndex);
        context.Emit(OpCodes.Ldloc, localMaxVec.LocalIndex);
        context.Emit(OpCodes.Ldloc, localT.LocalIndex);
        context.Emit(OpCodes.Call, conditionalSelect);
        context.Emit(OpCodes.Stloc, localT.LocalIndex);

        // push result
        context.Emit(OpCodes.Ldloc, localT.LocalIndex);
    }
}
