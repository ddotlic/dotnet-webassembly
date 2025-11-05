using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Identifies an instruction that splats a given lane into a 128 bit vector.
/// </summary>
public abstract class Vec128Splat : SimdInstruction
{
    private protected Vec128Splat()
    {
    }
    
    private WebAssemblyValueType InputType => SimdOpCode switch
    {
        SimdOpCode.Int8X16Splat or SimdOpCode.Int16X8Splat or SimdOpCode.Int32X4Splat => WebAssemblyValueType.Int32,
        SimdOpCode.Int64X2Splat => WebAssemblyValueType.Int64,
        SimdOpCode.Float32X4Splat => WebAssemblyValueType.Float32,
        SimdOpCode.Float64X2Splat => WebAssemblyValueType.Float64,
        _ => throw new CompilerException("Invalid SimdOpCode for splat instruction.")
    };
    
    internal sealed override void Compile(CompilationContext context)
    {
        // TODO: Maybe add an override which accepts SimdOpCode too
        context.PopStackNoReturn(this.OpCode, InputType);
        context.Stack.Push(WebAssemblyValueType.Vector128);
        
        context.Emit(OpCodes.Call, this.SimdOpCode.ToMethodInfo());
    }
}
