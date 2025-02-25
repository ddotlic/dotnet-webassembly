using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Intrinsics;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Identifies an instruction that uses a single CIL method call of the <see cref="Vector128"/> to remove one SIMD value from the stack, replacing it with one value, all of a specific type.
/// </summary>
public abstract class SimdValueOneToOneCallInstruction : SimdInstruction
{
    private static MethodInfo Converter => typeof(Vector128).GetMethods(BindingFlags.Public | BindingFlags.Static)
        .First(m => m.Name == "As");
    
    private protected SimdValueOneToOneCallInstruction()
    {
    }

    private protected virtual WebAssemblyValueType OutputType => WebAssemblyValueType.Vector128;
    
    internal sealed override void Compile(CompilationContext context)
    {
        var stack = context.Stack;

        // TODO: Maybe add an override which accepts SimdOpCode too
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Vector128);
        stack.Push(OutputType);

        if (this.SimdOpCode.RequiresLaneConversion())
        {
            var conv = Converter.MakeGenericMethod(typeof(uint), this.SimdOpCode.ToLaneType());
            context.Emit(OpCodes.Call, conv);
        }
        
        context.Emit(OpCodes.Call, this.SimdOpCode.ToMethodInfo());
    }
}
