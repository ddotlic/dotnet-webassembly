using System;
using System.Reflection.Emit;
using System.Runtime.Intrinsics;
using WebAssembly.Runtime.Compilation;
using static WebAssembly.SimdOpCodeExtensions;
using static WebAssembly.SimdOpCodeExtensions.KnownMethodName;

namespace WebAssembly.Instructions;

/// <summary>
/// Return 1 if all lanes are non-zero, 0 otherwise.
/// </summary>
public abstract class Vec128AllTrue : SimdInstruction
{
    private protected Vec128AllTrue()
    {
    }

    private string LaneKind => this.SimdOpCode.ToNativeName().Split('.')[0]; 
    
    private HelperMethod AllTrueHelper => 
        LaneKind switch {
            "i8x16" => HelperMethod.Int8X16AllTrue,
            "i16x8" => HelperMethod.Int16X8AllTrue,
            "i32x4" => HelperMethod.Int32X4AllTrue,
            "i64x2" => HelperMethod.Int64X2AllTrue,
            _ => throw new InvalidOperationException($"Unexpected lane type: {LaneKind}.")
            };
    
    private uint Mask => 
        LaneKind switch {
            "i8x16" => 0b1111_1111_1111_1111u,
            "i16x8" => 0b1111_1111u,
            "i32x4" => 0b1111u,
            "i64x2" => 0b11u,
            _ => throw new InvalidOperationException($"Unexpected lane type: {LaneKind}.")
        };
    
    internal sealed override void Compile(CompilationContext context)
    {
        // TODO: Maybe add an override which accepts SimdOpCode too
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Vector128);
        context.Stack.Push(WebAssemblyValueType.Int32);
        
        context.Emit(OpCodes.Call, context[AllTrueHelper, (_, c) =>
        {
            var builder = c.CheckedExportsBuilder.DefineMethod(
                $"☣ Int{LaneKind.Substring(1)}AllTrue",
                CompilationContext.HelperMethodAttributes,
                typeof(int),
                [
                    typeof(Vector128<uint>),
                ]
                );
            // TODO: all of the vector calls must be typed to the correct lane type
            // AND the vector must first be reinterpreted to the correct lane type
            var il = builder.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            var laneKind = LaneKind;
            il.Emit(OpCodes.Call, GetWellKnownMethod(laneKind, ConvertToLaneType));
            il.Emit(OpCodes.Call, GetWellKnownMethod(laneKind, Zero));
            il.Emit(OpCodes.Call, GetWellKnownMethod(laneKind, VecEquals));
            il.Emit(OpCodes.Call, GetWellKnownMethod(laneKind, OnesComplement));
            il.Emit(OpCodes.Call, GetWellKnownMethod(laneKind, ExtractMostSignificantBits));
            il.Emit(OpCodes.Ldc_I4, Mask);
            il.Emit(OpCodes.Ceq);
            il.Emit(OpCodes.Ret);
            return builder;
        }
        ]);
    }
}
