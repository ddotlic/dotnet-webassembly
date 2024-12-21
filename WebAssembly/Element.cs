﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using WebAssembly.Instructions;

namespace WebAssembly;

/// <summary>
/// The elements section allows a module to initialize (at instantiation time) the elements of any imported or internally-defined table with any other definition in the module.
/// </summary>
public class Element
{
    /// <summary>
    /// The table index.
    /// </summary>
    public uint Index { get; set; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)] //Wrapped by a property
    private IList<Instruction>? initializerExpression;

    /// <summary>
    /// An initializer expression that computes the offset at which to place the elements.
    /// </summary>
    /// <exception cref="ArgumentNullException">Value cannot be set to null.</exception>
    public IList<Instruction> InitializerExpression
    {
        get => this.initializerExpression ??= [];
        set => this.initializerExpression = value ?? throw new ArgumentNullException(nameof(value));
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)] //Wrapped by a property
    private IList<uint>? elements;

    /// <summary>
    /// A sequence of function indices.
    /// </summary>
    /// <exception cref="ArgumentNullException">Value cannot be set to null.</exception>
    public IList<uint> Elements
    {
        get => this.elements ??= [];
        set => this.elements = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// Creates a new <see cref="Element"/> instance.
    /// </summary>
    public Element()
    {
    }

    /// <summary>
    /// Creates a new <see cref="Element"/> instance with the provided elements.
    /// </summary>
    /// <param name="offset">The zero-based offset from the start of the table where <paramref name="elements"/> are placed, retained as the <see cref="InitializerExpression"/>.</param>
    /// <param name="elements">The table entries.</param>
    public Element(uint offset, params uint[] elements)
        : this(offset, (IList<uint>)elements)
    {
    }

    /// <summary>
    /// Creates a new <see cref="Element"/> instance with the provided elements.
    /// </summary>
    /// <param name="offset">The zero-based offset from the start of the table where <paramref name="elements"/> are placed, retained as the <see cref="InitializerExpression"/>.</param>
    /// <param name="elements">The table entries.</param>
    public Element(uint offset, IList<uint> elements)
    {
        this.initializerExpression =
        [
            new Int32Constant(offset),
            new End(),
        ];
        this.elements = elements;
    }

    private void ReadInitializerExpression(Reader reader)
    {
        this.initializerExpression = Instruction.ParseInitializerExpression(reader).ToList();
    }

    internal Element(Reader reader)
    {
        var preKindOffset = reader.Offset;
        var kind = reader.ReadVarUInt32();
        switch (kind)
        {
            case 0: // active, implicit table index = 0
                this.Index = 0;
                this.ReadInitializerExpression(reader);
                break;
            case 2: // active, explicit table index
                var preIndexOffset = reader.Offset;
                this.Index = reader.ReadVarUInt32();
                if (this.Index != 0)
                    throw new ModuleLoadException("Table index must be 0 for now.", preIndexOffset);
                this.ReadInitializerExpression(reader);
                var preElemKindOffset = reader.Offset;
                var elemKind = reader.ReadVarUInt32();
                if (elemKind != 0)
                    throw new ModuleLoadException("Spec only recognizes an element kind of 0.", preElemKindOffset);
                break;
            default:
                throw new ModuleLoadException("Element segment kind must be 0 or 2 for now.", preKindOffset);
        }
        // this.Index = reader.ReadVarUInt32();
        // this.initializerExpression = Instruction.ParseInitializerExpression(reader).ToList();

        var count = checked((int)reader.ReadVarUInt32());
        var elements = this.elements = [];

        for (var i = 0; i < count; i++)
            elements.Add(reader.ReadVarUInt32());
    }

    /// <summary>
    /// Expresses the value of this instance as a string.
    /// </summary>
    /// <returns>A string representation of this instance.</returns>
    public override string ToString() => $"{Index}: {InitializerExpression.Count} ({Elements.Count})";

    internal void WriteTo(Writer writer)
    {
        writer.WriteVar(this.Index);
        foreach (var instruction in this.InitializerExpression)
            instruction.WriteTo(writer);

        writer.WriteVar((uint)this.Elements.Count);
        foreach (var element in this.Elements)
            writer.WriteVar(element);
    }
}
