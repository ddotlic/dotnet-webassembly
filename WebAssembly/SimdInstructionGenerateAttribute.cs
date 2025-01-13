using System;
using WebAssembly.Instructions;

namespace WebAssembly;

/// <summary>
/// Declares that for the annotated <see cref="SimdOpCode" /> we can generate the source code for the instruction.
/// </summary>
/// <typeparam name="T">A base class of the generated class, itself derived from <see cref="SimdInstruction"/></typeparam>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public sealed class SimdInstructionGenerateAttribute<T>() : Attribute where T : SimdInstruction
{
}
