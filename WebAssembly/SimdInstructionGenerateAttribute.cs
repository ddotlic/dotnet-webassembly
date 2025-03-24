using System;
using WebAssembly.Instructions;

namespace WebAssembly;

/// <summary>
/// Declares that for the annotated <see cref="SimdOpCode" /> we can generate the source code for the instruction.
/// </summary>
/// <typeparam name="T">A base class of the generated class, itself derived from <see cref="SimdInstruction"/></typeparam>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public sealed class SimdInstructionGenerateAttribute<T>(bool includeReaderConstructor = false) : Attribute where T : SimdInstruction
{
    /// <summary>
    /// Indicates whether the generated class should include a constructor that takes a <see cref="Reader"/> as an argument.
    /// </summary>
    public bool IncludeReaderConstructor { get; } = includeReaderConstructor;
}
