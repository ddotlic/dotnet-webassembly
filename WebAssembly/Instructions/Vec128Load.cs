namespace WebAssembly.Instructions;

/// <summary>
/// Load 16 bytes as v128.
/// </summary>
public class Vec128Load : SimdMemoryReadInstruction
{
    /// <summary>
    /// Creates a new <see cref="Vec128Load"/> instance.
    /// </summary>
    public Vec128Load()
    {
    }

    /// <summary>
    /// Creates a new <see cref="Vec128Load"/> instance from binary data.
    /// </summary>
    /// <param name="reader">The source of binary data.</param>
    internal Vec128Load(Reader reader) : base(reader)
    {
    }
    
    /// <summary>
    /// Always <see cref="WebAssembly.SimdOpCode.Vec128Load"/>.
    /// </summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Vec128Load;

    private protected override byte Size => 16;
}
