using System;
using static WebAssembly.Instructions.MemoryImmediateInstruction;

namespace WebAssembly.Instructions;

/// <summary>
/// Common features of SIMD instructions that access linear memory and operate on specific lanes.
/// </summary>
public abstract class SimdMemoryLaneInstruction : SimdInstruction, IEquatable<SimdMemoryLaneInstruction>
{
    /// <summary>
    /// A bitfield which currently contains the alignment in the least significant bits, encoded as log2(alignment).
    /// </summary>
    public Options Flags { get; set; }

    /// <summary>
    /// The index within linear memory for the access operation.
    /// </summary>
    public uint Offset { get; set; }

    /// <summary>
    /// The index of the lane to operate on.
    /// </summary>
    public byte LaneIndex { get; set; }

    private protected SimdMemoryLaneInstruction()
    {
    }

    private protected SimdMemoryLaneInstruction(Reader reader)
    {
        Flags = (Options)reader.ReadVarUInt32();
        Offset = reader.ReadVarUInt32();
        LaneIndex = reader.ReadByte();
    }

    internal sealed override void WriteTo(Writer writer)
    {
        base.WriteTo(writer);
        writer.WriteVar((uint)this.Flags);
        writer.WriteVar(this.Offset);
        writer.Write(this.LaneIndex);
    }

    private protected abstract byte Size { get; }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => this.Equals(obj as SimdMemoryLaneInstruction);

    /// <summary>
    /// Determines whether this instruction is identical to another.
    /// </summary>
    /// <param name="other">The instruction to compare against.</param>
    /// <returns>True if they have the same type and value, otherwise false.</returns>
    public override bool Equals(Instruction? other) => this.Equals(other as SimdMemoryLaneInstruction);

    /// <summary>
    /// Determines whether this instruction is identical to another.
    /// </summary>
    /// <param name="other">The instruction to compare against.</param>
    /// <returns>True if they have the same type and value, otherwise false.</returns>
    public bool Equals(SimdMemoryLaneInstruction? other) =>
        base.Equals(other)
        && other.Flags == this.Flags
        && other.Offset == this.Offset
        && other.LaneIndex == this.LaneIndex;

    /// <summary>
    /// Returns a simple hash code based on the value of the instruction.
    /// </summary>
    /// <returns>The hash code.</returns>
    public override int GetHashCode()
    => HashCode.Combine(HashCode.Combine(base.GetHashCode(), (int)this.Flags, (int)this.Offset), this.LaneIndex);
}
