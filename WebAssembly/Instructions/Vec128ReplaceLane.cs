using System;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Replaces a lane in a vector with a new value.
/// </summary>
public abstract class Vec128ReplaceLane : SimdInstruction
{
    private protected Vec128ReplaceLane() { }

    /// <summary>
    /// The index of the lane to replace.
    /// </summary>
    public int LaneIndex { get; set; }

    /// <summary>
    /// Creates a new <see cref="Vec128ReplaceLane"/> instance from binary data.
    /// </summary>
    /// <param name="reader">The source of binary data.</param>
    private protected Vec128ReplaceLane(Reader reader)
    {
        LaneIndex = reader.ReadByte();
    }

    internal sealed override void WriteTo(Writer writer)
    {
        base.WriteTo(writer);
        writer.WriteVar((byte)this.LaneIndex);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => this.Equals(obj as Vec128ReplaceLane);

    /// <summary>
    /// Determines whether this instruction is identical to another.
    /// </summary>
    /// <param name="other">The instruction to compare against.</param>
    /// <returns>True if they have the same type and value, otherwise false.</returns>
    public override bool Equals(Instruction? other) => this.Equals(other as Vec128ReplaceLane);

    /// <summary>
    /// Determines whether this instruction is identical to another.
    /// </summary>
    /// <param name="other">The instruction to compare against.</param>
    /// <returns>True if they have the same type and value, otherwise false.</returns>
    public bool Equals(Vec128ReplaceLane? other) =>
        base.Equals(other)
        && other.LaneIndex == this.LaneIndex;

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), this.LaneIndex);

    private WebAssemblyValueType InputType => this.SimdOpCode.ToLaneKind() switch
    {
        "i8x16" or "i16x8" or "i32x4" => WebAssemblyValueType.Int32,
        "i64x2" => WebAssemblyValueType.Int64,
        "f32x4" => WebAssemblyValueType.Float32,
        "f64x2" => WebAssemblyValueType.Float64,
        _ => throw new NotSupportedException($"Unsupported lane kind: {this.SimdOpCode.ToLaneKind()}"),
    };

    private int MaxIndex
    {
        get
        {
            var native = this.SimdOpCode.ToNativeName().Split('.')[0];
            var bits = native.Split('x')[1];
            return int.Parse(bits, null) - 1;
        }
    }
    
    internal sealed override void Compile(CompilationContext context)
    {
        var maxIndex = MaxIndex;
        if (LaneIndex < 0 || LaneIndex > maxIndex)
            throw new CompilerException($"Lane index must be less than {maxIndex + 1}");

        // Stack on entry: [vector:v128, value:scalar]
        // We need to call: WithElement(vector, index, value)

        // Pop the scalar value
        var inputType = InputType;
        context.PopStackNoReturn(this.OpCode, inputType);

        // Pop the vector
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Vector128);

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
        var laneType = this.SimdOpCode.ToLaneType();
        var vecType = typeof(System.Runtime.Intrinsics.Vector128<>).MakeGenericType(laneType);
        var vectorLocal = context.DeclareLocal(vecType);
        context.Emit(OpCodes.Stloc, vectorLocal.LocalIndex);

        // Now build the call: [vector, index, value]
        context.Emit(OpCodes.Ldloc, vectorLocal.LocalIndex);
        context.Emit(OpCodes.Ldc_I4, LaneIndex);
        context.Emit(OpCodes.Ldloc, valueLocal.LocalIndex);

        // Call WithElement
        context.Emit(OpCodes.Call, this.SimdOpCode.ToMethodInfo());

        // Push the result v128 back onto the stack
        context.Stack.Push(WebAssemblyValueType.Vector128);
    }
}
