using System;

namespace WebAssembly;

/// <summary>
/// Indicates that we should skip the mapping of the WebAssembly SIMD instruction to a .NET method of the Vector128 type.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public sealed class SimdSkipMethodMappingAttribute : Attribute
{

}
