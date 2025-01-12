using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;


/// <summary>
/// Simd instructions have a prefix byte 0xfd; they are defined by the combination of their <see cref="OpCode"/> and <see cref="SimdOpCode"/>.
/// </summary>
public abstract class SimdInstruction : Instruction
{
    private protected SimdInstruction()
    {
    }

    /// <summary>
    /// Always <see cref="OpCode.SimdOperationPrefix"/>.
    /// </summary>
    public sealed override OpCode OpCode => OpCode.SimdOperationPrefix;

    /// <summary>
    /// Gets the <see cref="SimdOpCode"/> associated with this instruction.
    /// </summary>
    public abstract SimdOpCode SimdOpCode { get; }

    internal override void WriteTo(Writer writer)
    {
        writer.Write((byte)this.OpCode);
        writer.Write((byte)this.SimdOpCode);
    }

    /// <summary>
    /// Determines whether this instruction is identical to another.
    /// </summary>
    /// <param name="other">The instruction to compare against.</param>
    /// <returns>True if they have the same type and value, otherwise false.</returns>
    public override bool Equals(Instruction? other) =>
        other is SimdInstruction instruction
        && instruction.OpCode == this.OpCode
        && instruction.SimdOpCode == this.SimdOpCode
    ;

    /// <summary>
    /// Returns the integer representation of <see cref="Instruction.OpCode"/> as a hash code.
    /// </summary>
    /// <returns>The hash code.</returns>
    public override int GetHashCode() => HashCode.Combine((int)this.OpCode, (int)this.SimdOpCode);

    /// <summary>
    /// Provides a native representation of the instruction.
    /// </summary>
    /// <returns>A string representation of this instance.</returns>
    public sealed override string ToString() => this.SimdOpCode.ToNativeName();

    private static protected MethodInfo FindVector128Method(string name, Type parType, int parsCount = 2, bool isGeneric = true)
    {
        var methods = typeof(Vector128).GetMethods(BindingFlags.Public | BindingFlags.Static);
        var genericMethodInfo = methods.Where(m => m.Name == name).First(m =>
        {
            var pars = m.GetParameters();
            return pars.Length == parsCount && pars.All(p =>
                isGeneric ? p.ParameterType.IsGenericType && p.ParameterType.GetGenericTypeDefinition() == typeof(Vector128<>)
                    : p.ParameterType == parType);
        });
        return isGeneric ? genericMethodInfo.MakeGenericMethod(parType) : genericMethodInfo;
    }
}
