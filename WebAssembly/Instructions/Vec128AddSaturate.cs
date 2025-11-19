using System;
using System.Reflection.Emit;
using System.Runtime.Intrinsics;
using WebAssembly.Runtime.Compilation;
using static WebAssembly.SimdOpCodeExtensions;
using static WebAssembly.SimdOpCodeExtensions.KnownMethodName;

namespace WebAssembly.Instructions;

/// <summary>
/// Implements i8x16.add_saturate_s/u and i16x8.add_saturate_s/u
/// as a sequence of Vector128 operations.
/// </summary>
public abstract class Vec128AddSaturate : SimdInstruction
{
    private protected Vec128AddSaturate() { }

    internal override void Compile(CompilationContext context)
    {
        var stack = context.Stack;
        // Validate and adjust validation stack
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Vector128, WebAssemblyValueType.Vector128);
        stack.Push(WebAssemblyValueType.Vector128);

        var laneKind = this.SimdOpCode.ToLaneKind(); // e.g. "i8x16" or "i16x8"
        var isSigned = this.SimdOpCode.ToNativeName().EndsWith("_s", StringComparison.InvariantCulture);

        // choose CLR lane type based on signedness
        var parType = laneKind switch
        {
            "i8x16" => isSigned ? typeof(sbyte) : typeof(byte),
            "i16x8" => isSigned ? typeof(short) : typeof(ushort),
            _ => throw new InvalidOperationException($"Unsupported lane kind: {laneKind}"),
        };

        // Cache method infos
        var add = FindVector128Method("Add", parType, 2, true);
        var xor = FindVector128Method("Xor", parType, 2, true);
        var bitwiseAnd = FindVector128Method("BitwiseAnd", parType, 2, true);
        var onesComplement = FindVector128Method("OnesComplement", parType, 1, true);
        var lessThan = FindVector128Method("LessThan", parType, 2, true);
        var greaterThan = FindVector128Method("GreaterThan", parType, 2, true);
        var conditionalSelect = FindVector128Method("ConditionalSelect", parType, 3, true);
        var create = FindVector128Method("Create", parType, 1, false);
        var zero = GetWellKnownMethod(laneKind, Zero);

        // Compute integer constants to feed to Vector128.Create
        var maxInt = laneKind switch
        {
            "i8x16" when isSigned => 127,
            "i8x16" when !isSigned => 255,
            "i16x8" when isSigned => 32767,
            "i16x8" when !isSigned => 65535,
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

        // Store incoming operands (stack top is b, then a)
        context.Emit(OpCodes.Stloc, localB.LocalIndex);
        context.Emit(OpCodes.Stloc, localA.LocalIndex);

        // t = Add(a, b)
        context.Emit(OpCodes.Ldloc, localA.LocalIndex);
        context.Emit(OpCodes.Ldloc, localB.LocalIndex);
        context.Emit(OpCodes.Call, add);
        context.Emit(OpCodes.Stloc, localT.LocalIndex);

        if (!isSigned)
        {
            // unsigned: overflowMask = t < a
            var localOverflow = context.DeclareLocal(vecType);

            context.Emit(OpCodes.Ldloc, localT.LocalIndex);
            context.Emit(OpCodes.Ldloc, localA.LocalIndex);
            context.Emit(OpCodes.Call, lessThan);
            context.Emit(OpCodes.Stloc, localOverflow.LocalIndex);

            // maxVector = Create(maxInt)
            var localMax = context.DeclareLocal(vecType);
            context.Emit(OpCodes.Ldc_I4, maxInt);
            context.Emit(OpCodes.Call, create);
            context.Emit(OpCodes.Stloc, localMax.LocalIndex);

            // result = ConditionalSelect(overflowMask, maxVector, t)
            context.Emit(OpCodes.Ldloc, localOverflow.LocalIndex);
            context.Emit(OpCodes.Ldloc, localMax.LocalIndex);
            context.Emit(OpCodes.Ldloc, localT.LocalIndex);
            context.Emit(OpCodes.Call, conditionalSelect);
            context.Emit(OpCodes.Stloc, localT.LocalIndex);

            // push result
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

        // notXorAB = OnesComplement(xorAB)
        var localNotXorAB = context.DeclareLocal(vecType);
        context.Emit(OpCodes.Ldloc, localXorAB.LocalIndex);
        context.Emit(OpCodes.Call, onesComplement);
        context.Emit(OpCodes.Stloc, localNotXorAB.LocalIndex);

        // xorAT = Xor(a, t)
        var localXorAT = context.DeclareLocal(vecType);
        context.Emit(OpCodes.Ldloc, localA.LocalIndex);
        context.Emit(OpCodes.Ldloc, localT.LocalIndex);
        context.Emit(OpCodes.Call, xor);
        context.Emit(OpCodes.Stloc, localXorAT.LocalIndex);

        // overflowTest = BitwiseAnd(notXorAB, xorAT)
        var localOverflowTest = context.DeclareLocal(vecType);
        context.Emit(OpCodes.Ldloc, localNotXorAB.LocalIndex);
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
