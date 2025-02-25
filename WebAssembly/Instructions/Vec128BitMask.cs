namespace WebAssembly.Instructions;

/// <summary>
/// Base class for SIMD bitmask instructions, only overrides the output type.
/// </summary>
public abstract class Vec128BitMask : SimdValueOneToOneCallInstruction
{
    private protected Vec128BitMask() { }

    private protected override WebAssemblyValueType OutputType => WebAssemblyValueType.Int32;
}
