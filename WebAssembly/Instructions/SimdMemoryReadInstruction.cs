using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using WebAssembly.Runtime.Compilation;
using System.Linq;

namespace WebAssembly.Instructions;

/// <summary>
/// Provides shared functionality for SIMD instructions that read from linear memory.
/// </summary>
public abstract class SimdMemoryReadInstruction : SimdMemoryImmediateInstruction
{
    private static MethodInfo ReadUnaligned => 
        typeof(Unsafe).GetMethods(BindingFlags.Public | BindingFlags.Static)
            .First(m => m.Name == nameof(Unsafe.ReadUnaligned) && m.GetParameters().Length == 1)
            .MakeGenericMethod(typeof(Vector128<uint>));
        
    private protected SimdMemoryReadInstruction()
        : base()
    {
    }

    private protected SimdMemoryReadInstruction(Reader reader)
        : base(reader)
    {
    }

    internal sealed override void Compile(CompilationContext context)
    {
        MemoryImmediateInstruction.EmitMemoryAccessProlog(context, OpCode, Offset, Flags, 16);

        context.Emit(OpCodes.Conv_U);
        context.Emit(OpCodes.Call, ReadUnaligned); 

        context.Stack.Push(WebAssemblyValueType.Vector128);
    }
}

