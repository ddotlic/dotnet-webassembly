using System;
using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
///     Extracts a lane from a vector.
/// </summary>
public abstract class Vec128ExtractLane : Vec128Lane
{
    private protected Vec128ExtractLane() { }

    private protected Vec128ExtractLane(Reader reader) : base(reader)
    {
    }

    private WebAssemblyValueType OutputType => SimdOpCode.ToLaneKind() switch
    {
        "i8x16" => WebAssemblyValueType.Int32,
        "i16x8" => WebAssemblyValueType.Int32,
        "i32x4" => WebAssemblyValueType.Int32,
        "i64x2" => WebAssemblyValueType.Int64,
        "f32x4" => WebAssemblyValueType.Float32,
        "f64x2" => WebAssemblyValueType.Float64,
        _ => throw new NotSupportedException($"Unsupported lane kind: {SimdOpCode.ToLaneKind()}"),
    };

    internal sealed override void Compile(CompilationContext context)
    {
        ValidateLaneIndex();
        // TODO: Maybe add an override which accepts SimdOpCode too
        context.PopStackNoReturn(OpCode, WebAssemblyValueType.Vector128);
        context.Stack.Push(OutputType);

        context.Emit(OpCodes.Ldc_I4, LaneIndex);
        context.Emit(OpCodes.Call, SimdOpCode.ToMethodInfo());
    }
}
