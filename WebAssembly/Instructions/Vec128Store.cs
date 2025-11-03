namespace WebAssembly.Instructions;

/// <summary>
/// Store 16 bytes from v128 to memory.
/// </summary>
public class Vec128Store : SimdMemoryWriteInstruction
{
    /// <summary>
    /// Creates a new <see cref="Vec128Store"/> instance.
    /// </summary>
    public Vec128Store()
    {
    }

    /// <summary>
    /// Creates a new <see cref="Vec128Store"/> instance from binary data.
    /// </summary>
    /// <param name="reader">The source of binary data.</param>
    internal Vec128Store(Reader reader) : base(reader)
    {
    }

    /// <summary>
    /// Always <see cref="WebAssembly.SimdOpCode.Vec128Store"/>.
    /// </summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Vec128Store;

    private protected override byte Size => 16;
}
