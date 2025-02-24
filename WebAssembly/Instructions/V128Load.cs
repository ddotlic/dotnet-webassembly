namespace WebAssembly.Instructions;

/// <summary>
/// Load 16 bytes as v128.
/// </summary>
public class V128Load : SimdMemoryReadInstruction
{
    /// <summary>
    /// Creates a new <see cref="V128Load"/> instance.
    /// </summary>
    public V128Load()
    {
    }

    /// <summary>
    /// Creates a new <see cref="V128Load"/> instance from binary data.
    /// </summary>
    /// <param name="reader">The source of binary data.</param>
    internal V128Load(Reader reader) : base(reader)
    {
    }
    
    /// <summary>
    /// Always <see cref="SimdOpCode.V128Load"/>.
    /// </summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.V128Load;

}
