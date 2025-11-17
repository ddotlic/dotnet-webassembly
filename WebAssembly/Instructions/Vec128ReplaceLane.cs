using System;
using System.Reflection.Emit;
using System.Runtime.Intrinsics;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
///     Replaces a lane in a vector with a new value.
/// </summary>
public abstract class Vec128ReplaceLane : Vec128Lane
{
    private protected Vec128ReplaceLane() { }

    private protected Vec128ReplaceLane(Reader reader) : base(reader)
    {
    }

    private WebAssemblyValueType InputType => SimdOpCode.ToLaneKind() switch
    {
        "i8x16" or "i16x8" or "i32x4" => WebAssemblyValueType.Int32,
        "i64x2" => WebAssemblyValueType.Int64,
        "f32x4" => WebAssemblyValueType.Float32,
        "f64x2" => WebAssemblyValueType.Float64,
        _ => throw new NotSupportedException($"Unsupported lane kind: {SimdOpCode.ToLaneKind()}"),
    };

    internal sealed override void Compile(CompilationContext context)
    {
        ValidateLaneIndex();

        // Stack on entry: [vector:v128, value:scalar]
        // We need to call: WithElement(vector, index, value)

        // Pop the scalar value
        var inputType = InputType;
        context.PopStackNoReturn(OpCode, inputType);

        // Pop the vector
        context.PopStackNoReturn(OpCode, WebAssemblyValueType.Vector128);

        // Push arguments for WithElement: vector is already on stack from local
        // We need: [vector, index, value]
        // But they're in reverse order on the stack, so we need to reorder

        // Save value in a local
        var valueLocal = context.DeclareLocal(inputType switch
        {
            WebAssemblyValueType.Int32 => typeof(int),
            WebAssemblyValueType.Int64 => typeof(long),
            WebAssemblyValueType.Float32 => typeof(float),
            WebAssemblyValueType.Float64 => typeof(double),
            _ => throw new NotSupportedException($"Unsupported input type: {inputType}"),
        });
        context.Emit(OpCodes.Stloc, valueLocal.LocalIndex);

        // Save vector in a local
        var laneType = SimdOpCode.ToLaneType();
        var vecType = typeof(Vector128<>).MakeGenericType(laneType);
        var vectorLocal = context.DeclareLocal(vecType);
        context.Emit(OpCodes.Stloc, vectorLocal.LocalIndex);

        // Now build the call: [vector, index, value]
        context.Emit(OpCodes.Ldloc, vectorLocal.LocalIndex);
        context.Emit(OpCodes.Ldc_I4, LaneIndex);
        context.Emit(OpCodes.Ldloc, valueLocal.LocalIndex);

        // Call WithElement
        context.Emit(OpCodes.Call, SimdOpCode.ToMethodInfo());

        // Push the result v128 back onto the stack
        context.Stack.Push(WebAssemblyValueType.Vector128);
    }
}
