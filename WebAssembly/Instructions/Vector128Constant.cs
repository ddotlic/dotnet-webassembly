using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Produce the value of a v128.const.
/// </summary>
public class Vector128Constant : SimdInstruction, IEquatable<Vector128Constant>
{
    /// <summary>
    /// Gets or sets the value of the constant.
    /// </summary>
    public Vector128<uint> Value { get; set; }

    private static readonly MethodInfo createVector128Method;

    static Vector128Constant()
    {
        var methods = typeof(Vector128).GetMethods(BindingFlags.Public | BindingFlags.Static);
        createVector128Method = methods.Where(m => m.Name == "Create").First(m =>
        {
            var pars = m.GetParameters();
            return pars.Length == 4 && pars.All(p => p.ParameterType == typeof(uint));
        });
    }

    /// <summary>
    /// Creates a new <see cref="Vector128Constant"/> instance.
    /// </summary>
    public Vector128Constant()
    {
    }

    // TODO: add more constructors if needed

    /// <inheritdoc/>
    public override bool Equals(object? obj) => this.Equals(obj as Vector128Constant);

    /// <summary>
    /// Returns a simple hash code based on <see cref="Value"/> and the <see cref="OpCode"/> of the instruction.
    /// </summary>
    /// <returns>The hash code.</returns>
    /// <remarks><see cref="Value"/> should not be changed while this instance is used as a hash key.</remarks>
    public override int GetHashCode() => HashCode.Combine((int)this.OpCode, (int)this.SimdOpCode, this.Value.GetHashCode());

    /// <summary>
    /// Determines whether this instruction is identical to another.
    /// </summary>
    /// <param name="other">The instruction to compare against.</param>
    /// <returns>True if they have the same type and value, otherwise false.</returns>
    public bool Equals(Vector128Constant? other) => base.Equals(other) && this.Value.Equals(other.Value);

    /// <summary>
    /// Creates a new <see cref="Vector128Constant"/> instance from binary data.
    /// </summary>
    /// <param name="reader">The source of binary data.</param>
    internal Vector128Constant(Reader reader) => Value = Vector128.Create(reader.ReadBytes(16)).AsUInt32();

    /// <summary>
    /// Always <see cref="SimdOpCode.V128Const"/>.
    /// </summary>
    public override SimdOpCode SimdOpCode => SimdOpCode.V128Const;

    internal sealed override void WriteTo(Writer writer)
    {
        base.WriteTo(writer);
        var bytes = new byte[Vector128<byte>.Count];
        Unsafe.WriteUnaligned(ref Unsafe.As<byte, byte>(ref bytes[0]), this.Value);

        writer.Write(bytes);
    }

    internal sealed override void Compile(CompilationContext context)
    {
        context.Stack.Push(WebAssemblyValueType.Vector128);
        var uints = new uint[Vector128<uint>.Count];
        Unsafe.WriteUnaligned(ref Unsafe.As<uint, byte>(ref uints[0]), this.Value);
        context.Emit(OpCodes.Ldc_I4, unchecked((int)uints[0]));
        context.Emit(OpCodes.Ldc_I4, unchecked((int)uints[1]));
        context.Emit(OpCodes.Ldc_I4, unchecked((int)uints[2]));
        context.Emit(OpCodes.Ldc_I4, unchecked((int)uints[3]));
        context.Emit(OpCodes.Call, createVector128Method);
    }

}
