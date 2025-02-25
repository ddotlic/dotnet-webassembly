﻿namespace WebAssembly.Runtime.Compilation;

enum HelperMethod
{
    RangeCheck8,
    RangeCheck16,
    RangeCheck32,
    RangeCheck64,
    RangeCheck128,
    SelectFloat32,
    SelectFloat64,
    SelectInt32,
    SelectInt64,
    Float32ReinterpretInt32,
    Float64ReinterpretInt64,
    Int32ReinterpretFloat32,
    Int64ReinterpretFloat64,
    StoreInt8FromInt32,
    StoreInt16FromInt32,
    StoreInt32FromInt32,
    StoreInt8FromInt64,
    StoreInt16FromInt64,
    StoreInt32FromInt64,
    StoreInt64FromInt64,
    StoreFloat32,
    StoreFloat64,
    Float32CopySign,
    Float64CopySign,
    GrowMemory,
#if !NETCOREAPP3_0_OR_GREATER
    Int32CountOneBits,
    Int64CountOneBits,
    Int32CountLeadingZeroes,
    Int64CountLeadingZeroes,
    Int32CountTrailingZeroes,
    Int64CountTrailingZeroes,
    Int32RotateLeft,
    Int32RotateRight,
    Int64RotateLeft,
    Int64RotateRight,
#endif
    Int32TruncateSaturateFloat32Signed,
    Int32TruncateSaturateFloat32Unsigned,
    Int32TruncateSaturateFloat64Signed,
    Int32TruncateSaturateFloat64Unsigned,
    Int64TruncateSaturateFloat32Signed,
    Int64TruncateSaturateFloat32Unsigned,
    Int64TruncateSaturateFloat64Signed,
    Int64TruncateSaturateFloat64Unsigned,
    Vec128BitSelect,
    Vec128AnyTrue,
    Int8X16AllTrue,
    Int16X8AllTrue,
    Int32X4AllTrue,
    Int64X2AllTrue,
}
