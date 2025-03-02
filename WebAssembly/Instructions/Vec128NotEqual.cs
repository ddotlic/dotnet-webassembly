using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;
using static WebAssembly.SimdOpCodeExtensions;
using static WebAssembly.SimdOpCodeExtensions.KnownMethodName;

namespace WebAssembly.Instructions;

/// <summary>
/// Check if two vectors are not equal (implemented as 'equal' followed by 'not').
/// </summary>
public abstract class Vec128NotEqual : SimdInstruction
{
    private protected Vec128NotEqual()
    {
    }
    
    internal sealed override void Compile(CompilationContext context)
    {
        // TODO: Maybe add an override which accepts SimdOpCode too
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Vector128, WebAssemblyValueType.Vector128);
        
        context.Emit(OpCodes.Call, this.SimdOpCode.ToMethodInfo());
        // NOTE: Slightly hacky, but replace f32x4/f64x2 with i32x4/i64x2 for the 'not' operation
        var laneKind = $"i{this.SimdOpCode.ToLaneKind().Substring(1)}";
        context.Emit(OpCodes.Call, GetWellKnownMethod(laneKind, OnesComplement));
        
        context.Stack.Push(WebAssemblyValueType.Vector128);
    }
}
