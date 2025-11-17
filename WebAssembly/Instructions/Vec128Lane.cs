using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>
/// Common base for extract and replace lane instructions.
/// </summary>
public abstract class Vec128Lane: SimdInstruction
{
    private protected Vec128Lane() { }

    /// <summary>
    /// The index of the lane to extract.
    /// </summary>
    public int LaneIndex { get; set; }

    private protected Vec128Lane(Reader reader)
    {
        LaneIndex = reader.ReadByte();
    }

    internal sealed override void WriteTo(Writer writer)
    {
        base.WriteTo(writer);
        writer.WriteVar((byte)this.LaneIndex);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => this.Equals(obj as Vec128Lane);

    /// <summary>
    /// Determines whether this instruction is identical to another.
    /// </summary>
    /// <param name="other">The instruction to compare against.</param>
    /// <returns>True if they have the same type and value, otherwise false.</returns>
    public override bool Equals(Instruction? other) => this.Equals(other as Vec128Lane);

    /// <summary>
    /// Determines whether this instruction is identical to another.
    /// </summary>
    /// <param name="other">The instruction to compare against.</param>
    /// <returns>True if they have the same type and value, otherwise false.</returns>
    public bool Equals(Vec128Lane? other) =>
        base.Equals(other)
        && other.LaneIndex == this.LaneIndex;

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), this.LaneIndex);

    private protected int MaxIndex
    {
        get
        {
            var native = this.SimdOpCode.ToNativeName().Split('.')[0];
            var bits = native.Split('x')[1];
            return int.Parse(bits, null) - 1;
        }
    }

    private protected void ValidateLaneIndex()
    {
        var maxIndex = MaxIndex;
        if (LaneIndex < 0 || LaneIndex > maxIndex)
            throw new CompilerException($"Lane index must be less than {maxIndex + 1}");
    }
}
