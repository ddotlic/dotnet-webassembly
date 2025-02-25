using System;

namespace WebAssembly;

/// <summary>
/// Declares the traits of the SIMD operation mapping to the .NET code.
/// 
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public sealed class SimdOpTraitsAttribute(bool hasMethodInfo = true) : Attribute
{
    /// <summary>
    /// Indicates whether the SIMD operation has a corresponding .NET method which implements it trivially.
    /// </summary>
    public bool HasMethodInfo { get; } = hasMethodInfo;
}
