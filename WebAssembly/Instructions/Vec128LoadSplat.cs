using System.Reflection.Emit;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>
/// Common features of SIMD instructions that load a scalar from memory and splat it across all lanes of a vector.
/// </summary>
public abstract class Vec128LoadSplat : SimdMemoryReadInstruction
{
    private protected Vec128LoadSplat()
    {
    }

    private protected Vec128LoadSplat(Reader reader)
        : base(reader)
    {
    }

    private protected override byte Size => SimdOpCode switch
    {
        SimdOpCode.Vec128Load8Splat => 1,
        SimdOpCode.Vec128Load16Splat => 2,
        SimdOpCode.Vec128Load32Splat => 4,
        SimdOpCode.Vec128Load64Splat => 8,
        _ => throw new CompilerException("Invalid SimdOpCode for splat load instruction."),
    };

    // Emit the correct scalar load from memory prior to calling the intrinsic create/splat.
    private protected override System.Reflection.Emit.OpCode[] LoadOpCodes => Size switch
    {
        1 => new[] { OpCodes.Ldind_U1 },
        2 => new[] { OpCodes.Ldind_U2 },
        4 => new[] { OpCodes.Ldind_U4 },
        8 => new[] { OpCodes.Ldind_I8 },
        _ => throw new CompilerException($"Invalid load size {Size}."),
    };
}
