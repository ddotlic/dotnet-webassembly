using System;
using System.Reflection.Emit;
using WebAssembly;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Extracts a lane from a vector.
/// </summary>
public abstract class Vec128ExtractLane : SimdInstruction
{
    private protected Vec128ExtractLane() { }

    /// <summary>
    /// The index of the lane to extract.
    /// </summary>
    public int LaneIndex { get; set; }

    /// <summary>
    /// Creates a new <see cref="Vec128ExtractLane"/> instance from binary data.
    /// </summary>
    /// <param name="reader">The source of binary data.</param>
    private protected Vec128ExtractLane(Reader reader)
    {
        LaneIndex = reader.ReadByte();
    }

    internal sealed override void WriteTo(Writer writer)
    {
        base.WriteTo(writer);
        writer.WriteVar((byte)this.LaneIndex);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => this.Equals(obj as Vec128ExtractLane);

    /// <summary>
    /// Determines whether this instruction is identical to another.
    /// </summary>
    /// <param name="other">The instruction to compare against.</param>
    /// <returns>True if they have the same type and value, otherwise false.</returns>
    public override bool Equals(Instruction? other) => this.Equals(other as Vec128ExtractLane);

    /// <summary>
    /// Determines whether this instruction is identical to another.
    /// </summary>
    /// <param name="other">The instruction to compare against.</param>
    /// <returns>True if they have the same type and value, otherwise false.</returns>
    public bool Equals(Vec128ExtractLane? other) =>
        base.Equals(other)
        && other.LaneIndex == this.LaneIndex;

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), this.LaneIndex);

    private WebAssemblyValueType OutputType => this.SimdOpCode.ToLaneKind() switch
    {
        "i8x16" => WebAssemblyValueType.Int32,
        "i16x8" => WebAssemblyValueType.Int32,
        "i32x4" => WebAssemblyValueType.Int32,
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
        // TODO: Maybe add an override which accepts SimdOpCode too
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Vector128);
        context.Stack.Push(OutputType);

        context.Emit(OpCodes.Ldc_I4, LaneIndex);
        context.Emit(OpCodes.Call, this.SimdOpCode.ToMethodInfo());
    }
}
