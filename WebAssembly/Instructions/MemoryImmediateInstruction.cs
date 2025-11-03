using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Common features of instructions that access linear memory.
/// </summary>
public abstract class MemoryImmediateInstruction : Instruction, IEquatable<MemoryImmediateInstruction>
{
    /// <summary>
    /// Indicates options for the instruction.
    /// </summary>
    [Flags]
    public enum Options : uint
    {
        /// <summary>
        /// The access uses 8-bit alignment.
        /// </summary>
        Align1 = 0b00,

        /// <summary>
        /// The access uses 16-bit alignment.
        /// </summary>
        Align2 = 0b01,

        /// <summary>
        /// The access uses 32-bit alignment.
        /// </summary>
        Align4 = 0b10,

        /// <summary>
        /// The access uses 64-bit alignment.
        /// </summary>
        Align8 = 0b11,
        
        /// <summary>
        /// The access uses 128-bit alignment.
        /// </summary>
        Align16 = 0b100,
    }
    
    /// <summary>
    /// A bitfield which currently contains the alignment in the least significant bits, encoded as log2(alignment).
    /// </summary>
    public Options Flags { get; set; }

    /// <summary>
    /// The index within linear memory for the access operation.
    /// </summary>
    public uint Offset { get; set; }

    private protected MemoryImmediateInstruction()
    {
    }

    private protected MemoryImmediateInstruction(Reader reader)
    {
        Flags = (Options)reader.ReadVarUInt32();
        Offset = reader.ReadVarUInt32();
    }

    internal sealed override void WriteTo(Writer writer)
    {
        writer.Write((byte)this.OpCode);
        writer.WriteVar((uint)this.Flags);
        writer.WriteVar(this.Offset);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => this.Equals(obj as MemoryImmediateInstruction);

    /// <summary>
    /// Determines whether this instruction is identical to another.
    /// </summary>
    /// <param name="other">The instruction to compare against.</param>
    /// <returns>True if they have the same type and value, otherwise false.</returns>
    public override bool Equals(Instruction? other) => this.Equals(other as MemoryImmediateInstruction);

    /// <summary>
    /// Determines whether this instruction is identical to another.
    /// </summary>
    /// <param name="other">The instruction to compare against.</param>
    /// <returns>True if they have the same type and value, otherwise false.</returns>
    public bool Equals(MemoryImmediateInstruction? other) =>
        other != null
        && other.OpCode == this.OpCode
        && other.Flags == this.Flags
        && other.Offset == this.Offset
        ;

    /// <summary>
    /// Returns a simple hash code based on the value of the instruction.
    /// </summary>
    /// <returns>The hash code.</returns>
    public override int GetHashCode() => HashCode.Combine((int)this.OpCode, (int)this.Flags, (int)this.Offset);

    private protected abstract WebAssemblyValueType Type { get; }

    private protected abstract byte Size { get; }

    private protected abstract System.Reflection.Emit.OpCode EmittedOpCode { get; }

    internal static HelperMethod RangeCheckHelper(byte size) => size switch
    {
        1 => HelperMethod.RangeCheck8,
        2 => HelperMethod.RangeCheck16,
        4 => HelperMethod.RangeCheck32,
        8 => HelperMethod.RangeCheck64,
        16 => HelperMethod.RangeCheck128,
        _ => throw new InvalidOperationException(),// Shouldn't be possible.
    };

    internal static void EmitRangeCheck(CompilationContext context, byte size)
    {
        context.EmitLoadThis();
        context.Emit(OpCodes.Call, context[RangeCheckHelper(size), CreateRangeCheck]);
    }

    internal static void EmitAlignment(CompilationContext context, Options flags)
    {
        byte alignment;
        switch (flags) // TODO: why was it `flags & Options.Align8` (why bitmasking)?
        {
            default: //Impossible to hit, but needed to avoid compiler error the about alignment variable.
            case Options.Align1: alignment = 1; break;
            case Options.Align2: alignment = 2; break;
            case Options.Align4: alignment = 4; break;
            case Options.Align8: alignment = 8; break;
            case Options.Align16: alignment = 16; break;
        }

        //8-byte alignment is not available in IL.
        //See: https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.unaligned?view=net-5.0
        //However, because 8-byte alignment is subset of 4-byte alignment,
        //We don't have to consider it.
        if (alignment != 4 && alignment != 8 && alignment != 16)
            context.Emit(OpCodes.Unaligned, alignment);
    }
    
    internal static void EmitMemoryAccessProlog(CompilationContext context, OpCode opCode, uint offset, Options flags, byte size, bool emitAlignment = true)
    {
        context.PopStackNoReturn(opCode, WebAssemblyValueType.Int32);

        if (offset != 0)
        {
            Int32Constant.Emit(context, (int)offset);
            context.Emit(OpCodes.Add_Ovf_Un);
        }

        EmitRangeCheck(context, size);

        context.EmitLoadThis();
        context.Emit(OpCodes.Ldfld, context.CheckedMemory);
        context.Emit(OpCodes.Call, UnmanagedMemory.StartGetter);
        context.Emit(OpCodes.Add);
        
        if(emitAlignment) EmitAlignment(context, flags);
    }
    
    internal static MethodBuilder CreateRangeCheck(HelperMethod helper, CompilationContext context)
    {
        if (context.Memory == null)
            throw new CompilerException("Cannot use instructions that depend on linear memory when linear memory is not defined.");

        byte size;
        switch (helper)
        {
            default: throw new InvalidOperationException(); // Shouldn't be possible.
            case HelperMethod.RangeCheck8:
                size = 1;
                break;
            case HelperMethod.RangeCheck16:
                size = 2;
                break;
            case HelperMethod.RangeCheck32:
                size = 4;
                break;
            case HelperMethod.RangeCheck64:
                size = 8;
                break;
            case HelperMethod.RangeCheck128:
                size = 16;
                break;
        }

        var builder = context.CheckedExportsBuilder.DefineMethod(
            $"☣ Range Check {size}",
            CompilationContext.HelperMethodAttributes,
            typeof(uint),
            [typeof(uint), context.CheckedExportsBuilder]
            );
        var il = builder.GetILGenerator();
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Ldfld, context.Memory);
        il.Emit(OpCodes.Call, UnmanagedMemory.SizeGetter);
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldc_I4_S, size);
        il.Emit(OpCodes.Add_Ovf_Un);
        var outOfRange = il.DefineLabel();
        il.Emit(OpCodes.Blt_Un_S, outOfRange);
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ret);
        il.MarkLabel(outOfRange);
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldc_I4_S, size);
        il.Emit(OpCodes.Newobj, typeof(MemoryAccessOutOfRangeException)
            .GetTypeInfo()
            .DeclaredConstructors
            .First(c =>
            {
                var parms = c.GetParameters();
                return parms.Length == 2
                && parms[0].ParameterType == typeof(uint)
                && parms[1].ParameterType == typeof(uint)
                ;
            }));
        il.Emit(OpCodes.Throw);
        return builder;
    }
}
