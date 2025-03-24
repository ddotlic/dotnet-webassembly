using System.Reflection.Emit;

namespace WebAssembly.Instructions;

/// <summary>
/// Load a 64-bit value from memory into vector and zero pad.
/// </summary>
public class Vec128Load64Zero : SimdMemoryReadInstruction
{
    /// <summary>
    /// Creates a new <see cref="Vec128Load64Zero"/> instance.
    /// </summary>
    public Vec128Load64Zero()
    {
    }

    /// <summary>
    /// Creates a new <see cref="Vec128Load64Zero"/> instance from binary data.
    /// </summary>
    /// <param name="reader">The source of binary data.</param>
    internal Vec128Load64Zero(Reader reader) : base(reader)
    {
    }
    
    /// <summary>
    /// Always <see cref="WebAssembly.SimdOpCode.Vec128Load64Zero"/>.
    /// </summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Vec128Load64Zero;

    private protected override System.Reflection.Emit.OpCode[] LoadOpCodes =>
    [
        OpCodes.Ldind_I8,
    ];
    
    private protected override byte Size => 8;
}
