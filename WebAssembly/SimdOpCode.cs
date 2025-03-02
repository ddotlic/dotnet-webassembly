using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Intrinsics;
using WebAssembly.Instructions;

namespace WebAssembly;

/// <summary>
/// SIMD operation values (always preceded by a <see cref="OpCode.SimdOperationPrefix"/> value).
/// </summary>
public enum SimdOpCode : byte
{
    /// <summary>
    /// Load a SIMD vector from memory.
    /// </summary>
    [OpCodeCharacteristics("v128.load")]
    Vec128Load = 0x00,

    /// <summary>
    /// Instantiate a new SIMD vector with 16 8-bit elements.
    /// </summary>
    [OpCodeCharacteristics("v128.const")]
    Vec128Const = 0x0c,

    /// <summary>
    /// SIMD negate 16 8-bit integers.
    /// </summary>
    [OpCodeCharacteristics("i8x16.neg")]
    [SimdInstructionGenerate<SimdValueOneToOneCallInstruction>()]
    Int8X16Neg = 0x61,

    /// <summary>
    /// Lane-wise compare 16 8-bit lanes, equality.
    /// </summary>
    [OpCodeCharacteristics("i8x16.eq")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int8X16Equal = 0x23,
    /// <summary>
    /// Lane-wise compare 16 8-bit lanes, non-equality.
    /// </summary>
    [OpCodeCharacteristics("i8x16.ne")]
    [SimdInstructionGenerate<Vec128NotEqual>()]
    Int8X16NotEqual = 0x24,
    
    /// <summary>
    /// Lane-wise compare 16 8-bit lanes, signed less than.
    /// </summary>
    [OpCodeCharacteristics("i8x16.lt_s")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int8X16LessThanSigned = 0x25,
    
    /// <summary>
    /// Lane-wise compare 16 8-bit lanes, unsigned less than.
    /// </summary>
    [OpCodeCharacteristics("i8x16.lt_u")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int8X16LessThanUnsigned = 0x26,
    
    /// <summary>
    /// Lane-wise compare 16 8-bit lanes, signed greater than.
    /// </summary>
    [OpCodeCharacteristics("i8x16.gt_s")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int8X16GreaterThanSigned = 0x27,
    
    /// <summary>
    /// Lane-wise compare 16 8-bit lanes, unsigned greater than.
    /// </summary>
    [OpCodeCharacteristics("i8x16.gt_u")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int8X16GreaterThanUnsigned = 0x28,
    
    /// <summary>
    /// Lane-wise compare 16 8-bit lanes, signed less than or equal.
    /// </summary>
    [OpCodeCharacteristics("i8x16.le_s")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int8X16LessThanOrEqualSigned = 0x29,
    
    /// <summary>
    /// Lane-wise compare 16 8-bit lanes, unsigned less than or equal.
    /// </summary>
    [OpCodeCharacteristics("i8x16.le_u")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int8X16LessThanOrEqualUnsigned = 0x2a,
    
    /// <summary>
    /// Lane-wise compare 16 8-bit lanes, signed greater than or equal.
    /// </summary>
    [OpCodeCharacteristics("i8x16.ge_s")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int8X16GreaterThanOrEqualSigned = 0x2b,
    
    /// <summary>
    /// Lane-wise compare 16 8-bit lanes, unsigned greater than or equal.
    /// </summary>
    [OpCodeCharacteristics("i8x16.ge_u")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int8X16GreaterThanOrEqualUnsigned = 0x2c,
    
    /// <summary>
    /// Return 1 if all 16 8-bit lanes are non-zero, 0 otherwise.
    /// </summary>
    [OpCodeCharacteristics("i8x16.all_true")]
    [SimdInstructionGenerate<Vec128AllTrue>()]
    [SimdOpTraits(hasMethodInfo: false)]
    Int8X16AllTrue = 0x63,

    /// <summary>
    /// Extract the high bit for each of the 16 8-bit lanes and produce a scalar mask with all bits concatenated.
    /// </summary>
    [OpCodeCharacteristics("i8x16.bitmask")]
    [SimdInstructionGenerate<Vec128BitMask>()]
    [SimdOpTraits(hasMethodInfo: true)]
    Int8X16BitMask = 0x64,

    /// <summary>
    /// Shift the bits in each of the 16 8-bit lanes left by the same amout.
    /// </summary>
    [OpCodeCharacteristics("i8x16.shl")]
    [SimdInstructionGenerate<Vec128Shift>()]
    Int8X16ShiftLeft = 0x6b,
    
    /// <summary>
    /// Shift the bits in each of the 16 8-bit lanes arithmetic right by the same amout.
    /// </summary>
    [OpCodeCharacteristics("i8x16.shr_s")]
    [SimdInstructionGenerate<Vec128Shift>()]
    Int8X16ShiftArithRight = 0x6c,
    
    /// <summary>
    /// Shift the bits in each of the 16 8-bit lanes logical right by the same amout.
    /// </summary>
    [OpCodeCharacteristics("i8x16.shr_u")]
    [SimdInstructionGenerate<Vec128Shift>()]
    Int8X16ShiftLogicRight = 0x6d,
    
    /// <summary>
    /// SIMD add 16 8-bit integers.
    /// </summary>
    [OpCodeCharacteristics("i8x16.add")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int8X16Add = 0x6e,

    /// <summary>
    /// SIMD subtract 16 8-bit integers.
    /// </summary>
    [OpCodeCharacteristics("i8x16.sub")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int8X16Sub = 0x71,

    /// <summary>
    /// SIMD negate 8 16-bit integers. 
    /// </summary>
    [OpCodeCharacteristics("i16x8.neg")]
    [SimdInstructionGenerate<SimdValueOneToOneCallInstruction>()]
    Int16X8Neg = 0x81,

    /// <summary>
    /// Lane-wise compare 8 16-bit lanes, equality.
    /// </summary>
    [OpCodeCharacteristics("i16x8.eq")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int16X8Equal = 0x2d,
    
    /// <summary>
    /// Lane-wise compare 8 16-bit lanes, non-equality.
    /// </summary>
    [OpCodeCharacteristics("i16x8.ne")]
    [SimdInstructionGenerate<Vec128NotEqual>()]
    Int16X8NotEqual = 0x2e,
    
    /// <summary>
    /// Lane-wise compare 8 16-bit lanes, signed less than.
    /// </summary>
    [OpCodeCharacteristics("i16x8.lt_s")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int16X8LessThanSigned = 0x2f,
    
    /// <summary>
    /// Lane-wise compare 8 16-bit lanes, unsigned less than.
    /// </summary>
    [OpCodeCharacteristics("i16x8.lt_u")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int16X8LessThanUnsigned = 0x30,
    
    /// <summary>
    /// Lane-wise compare 8 16-bit lanes, signed greater than.
    /// </summary>
    [OpCodeCharacteristics("i16x8.gt_s")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int16X8GreaterThanSigned = 0x31,
    
    /// <summary>
    /// Lane-wise compare 8 16-bit lanes, unsigned greater than.
    /// </summary>
    [OpCodeCharacteristics("i16x8.gt_u")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int16X8GreaterThanUnsigned = 0x32,
    
    /// <summary>
    /// Lane-wise compare 8 16-bit lanes, signed less than or equal.
    /// </summary>
    [OpCodeCharacteristics("i16x8.le_s")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int16X8LessThanOrEqualSigned = 0x33,
    
    /// <summary>
    /// Lane-wise compare 8 16-bit lanes, unsigned less than or equal.
    /// </summary>
    [OpCodeCharacteristics("i16x8.le_u")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int16X8LessThanOrEqualUnsigned = 0x34,
    
    /// <summary>
    /// Lane-wise compare 8 16-bit lanes, signed greater than or equal.
    /// </summary>
    [OpCodeCharacteristics("i16x8.ge_s")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int16X8GreaterThanOrEqualSigned = 0x35,
    
    /// <summary>
    /// Lane-wise compare 8 16-bit lanes, unsigned greater than or equal.
    /// </summary>
    [OpCodeCharacteristics("i16x8.ge_u")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int16X8GreaterThanOrEqualUnsigned = 0x36,
    
    /// <summary>
    /// Return 1 if all 8 16-bit lanes are non-zero, 0 otherwise.
    /// </summary>
    [OpCodeCharacteristics("i16x8.all_true")]
    [SimdInstructionGenerate<Vec128AllTrue>()]
    [SimdOpTraits(hasMethodInfo: false)]
    Int16X8AllTrue = 0x83,

    /// <summary>
    /// Extract the high bit for each of the 8 16-bit lanes and produce a scalar mask with all bits concatenated.
    /// </summary>
    [OpCodeCharacteristics("i16x8.bitmask")]
    [SimdInstructionGenerate<Vec128BitMask>()]
    [SimdOpTraits(hasMethodInfo: true)]
    Int16X8BitMask = 0x84,

    /// <summary>
    /// Shift the bits in each of the 8 16-bit lanes left by the same amout.
    /// </summary>
    [OpCodeCharacteristics("i16x8.shl")]
    [SimdInstructionGenerate<Vec128Shift>()]
    Int16X8ShiftLeft = 0x8b,
    
    /// <summary>
    /// Shift the bits in each of the 8 16-bit lanes arithmetic right by the same amout.
    /// </summary>
    [OpCodeCharacteristics("i16x8.shr_s")]
    [SimdInstructionGenerate<Vec128Shift>()]
    Int16X8ShiftArithRight = 0x8c,
    
    /// <summary>
    /// Shift the bits in each of the 8 16-bit lanes logical right by the same amout.
    /// </summary>
    [OpCodeCharacteristics("i16x8.shr_u")]
    [SimdInstructionGenerate<Vec128Shift>()]
    Int16X8ShiftLogicRight = 0x8d,
    
    /// <summary>
    /// SIMD add 8 16-bit integers. 
    /// </summary>
    [OpCodeCharacteristics("i16x8.add")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int16X8Add = 0x8e,

    /// <summary>
    /// SIMD subtract 8 16-bit integers. 
    /// </summary>
    [OpCodeCharacteristics("i16x8.sub")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int16X8Sub = 0x91,

    /// <summary>
    /// SIMD multiply 8 16-bit integers. 
    /// </summary>
    [OpCodeCharacteristics("i16x8.mul")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int16X8Mul = 0x95,

    /// <summary>
    /// SIMD negate 4 32-bit integers. 
    /// </summary>
    [OpCodeCharacteristics("i32x4.neg")]
    [SimdInstructionGenerate<SimdValueOneToOneCallInstruction>()]
    Int32X4Neg = 0xa1,

    /// <summary>
    /// Lane-wise compare 4 32-bit lanes, equality.
    /// </summary>
    [OpCodeCharacteristics("i32x4.eq")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int32X4Equal = 0x37,
    
    /// <summary>
    /// Lane-wise compare 4 32-bit lanes, non-equality.
    /// </summary>
    [OpCodeCharacteristics("i32x4.ne")]
    [SimdInstructionGenerate<Vec128NotEqual>()]
    Int32X4NotEqual = 0x38,
    
    /// <summary>
    /// Lane-wise compare 4 32-bit lanes, signed less than.
    /// </summary>
    [OpCodeCharacteristics("i32x4.lt_s")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int32X4LessThanSigned = 0x39,
    
    /// <summary>
    /// Lane-wise compare 4 32-bit lanes, unsigned less than.
    /// </summary>
    [OpCodeCharacteristics("i32x4.lt_u")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int32X4LessThanUnsigned = 0x3a,
    
    /// <summary>
    /// Lane-wise compare 4 32-bit lanes, signed greater than.
    /// </summary>
    [OpCodeCharacteristics("i32x4.gt_s")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int32X4GreaterThanSigned = 0x3b,
    
    /// <summary>
    /// Lane-wise compare 4 32-bit lanes, unsigned greater than.
    /// </summary>
    [OpCodeCharacteristics("i32x4.gt_u")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int32X4GreaterThanUnsigned = 0x3c,
    
    /// <summary>
    /// Lane-wise compare 4 32-bit lanes, signed less than or equal.
    /// </summary>
    [OpCodeCharacteristics("i32x4.le_s")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int32X4LessThanOrEqualSigned = 0x3d,
    
    /// <summary>
    /// Lane-wise compare 4 32-bit lanes, unsigned less than or equal.
    /// </summary>
    [OpCodeCharacteristics("i32x4.le_u")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int32X4LessThanOrEqualUnsigned = 0x3e,
    
    /// <summary>
    /// Lane-wise compare 4 32-bit lanes, signed greater than or equal.
    /// </summary>
    [OpCodeCharacteristics("i32x4.ge_s")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int32X4GreaterThanOrEqualSigned = 0x3f,
    
    /// <summary>
    /// Lane-wise compare 4 32-bit lanes, unsigned greater than or equal.
    /// </summary>
    [OpCodeCharacteristics("i32x4.ge_u")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int32X4GreaterThanOrEqualUnsigned = 0x40,
    
    /// <summary>
    /// Return 1 if all 4 32-bit lanes are non-zero, 0 otherwise.
    /// </summary>
    [OpCodeCharacteristics("i32x4.all_true")]
    [SimdInstructionGenerate<Vec128AllTrue>()]
    [SimdOpTraits(hasMethodInfo: false)]
    Int32X4AllTrue = 0xa3,

    /// <summary>
    /// Extract the high bit for each of the 4 32-bit lanes and produce a scalar mask with all bits concatenated.
    /// </summary>
    [OpCodeCharacteristics("i32x4.bitmask")]
    [SimdInstructionGenerate<Vec128BitMask>()]
    [SimdOpTraits(hasMethodInfo: true)]
    Int32X4BitMask = 0xa4,

    /// <summary>
    /// Shift the bits in each of the 4 32-bit lanes left by the same amout.
    /// </summary>
    [OpCodeCharacteristics("i32x4.shl")]
    [SimdInstructionGenerate<Vec128Shift>()]
    Int32X4ShiftLeft = 0xab,
    
    /// <summary>
    /// Shift the bits in each of the 4 32-bit lanes arithmetic right by the same amout.
    /// </summary>
    [OpCodeCharacteristics("i32x4.shr_s")]
    [SimdInstructionGenerate<Vec128Shift>()]
    Int32X4ShiftArithRight = 0xac,
    
    /// <summary>
    /// Shift the bits in each of the 4 32-bit lanes logical right by the same amout.
    /// </summary>
    [OpCodeCharacteristics("i32x4.shr_u")]
    [SimdInstructionGenerate<Vec128Shift>()]
    Int32X4ShiftLogicRight = 0xad,
    
    /// <summary>
    /// SIMD add 4 32-bit integers. 
    /// </summary>
    [OpCodeCharacteristics("i32x4.add")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int32X4Add = 0xae,

    /// <summary>
    /// SIMD subtract 4 32-bit integers. 
    /// </summary>
    [OpCodeCharacteristics("i32x4.sub")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int32X4Sub = 0xb1,

    /// <summary>
    /// SIMD multiply 4 32-bit integers. 
    /// </summary>
    [OpCodeCharacteristics("i32x4.mul")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int32X4Mul = 0xb5,

    /// <summary>
    /// SIMD negate 2 64-bit integers.
    /// </summary>
    [OpCodeCharacteristics("i64x2.neg")]
    [SimdInstructionGenerate<SimdValueOneToOneCallInstruction>()]
    Int64X2Neg = 0xc1,

    /// <summary>
    /// Lane-wise compare 2 64-bit lanes, equality.
    /// </summary>
    [OpCodeCharacteristics("i64x2.eq")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int64X2Equal = 0xd6,
    
    /// <summary>
    /// Lane-wise compare 2 64-bit lanes, non-equality.
    /// </summary>
    [OpCodeCharacteristics("i64x2.ne")]
    [SimdInstructionGenerate<Vec128NotEqual>()]
    Int64X2NotEqual = 0xd7,
    
    /// <summary>
    /// Lane-wise compare 2 64-bit lanes, signed less than.
    /// </summary>
    [OpCodeCharacteristics("i64x2.lt_s")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int64X2LessThanSigned = 0xd8,
    
    /// <summary>
    /// Lane-wise compare 2 64-bit lanes, signed greater than.
    /// </summary>
    [OpCodeCharacteristics("i64x2.gt_s")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int64X2GreaterThanSigned = 0xd9,
    
    /// <summary>
    /// Lane-wise compare 2 64-bit lanes, signed less than or equal.
    /// </summary>
    [OpCodeCharacteristics("i64x2.le_s")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int64X2LessThanOrEqualSigned = 0xda,
    
    /// <summary>
    /// Lane-wise compare 2 64-bit lanes, signed greater than or equal.
    /// </summary>
    [OpCodeCharacteristics("i64x2.ge_s")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int64X2GreaterThanOrEqualSigned = 0xdb,
    
    /// <summary>
    /// Return 1 if all 2 64-bit lanes are non-zero, 0 otherwise.
    /// </summary>
    [OpCodeCharacteristics("i64x2.all_true")]
    [SimdInstructionGenerate<Vec128AllTrue>()]
    [SimdOpTraits(hasMethodInfo: false)]
    Int64X2AllTrue = 0xc3,

    /// <summary>
    /// Extract the high bit for each of the 2 64-bit lanes and produce a scalar mask with all bits concatenated.
    /// </summary>
    [OpCodeCharacteristics("i64x2.bitmask")]
    [SimdInstructionGenerate<Vec128BitMask>()]
    [SimdOpTraits(hasMethodInfo: true)]
    Int64X2BitMask = 0xc4,

    /// <summary>
    /// Shift the bits in each of the 2 64-bit lanes left by the same amout.
    /// </summary>
    [OpCodeCharacteristics("i64x2.shl")]
    [SimdInstructionGenerate<Vec128Shift>()]
    Int64X2ShiftLeft = 0xcb,
    
    /// <summary>
    /// Shift the bits in each of the 2 64-bit lanes arithmetic right by the same amout.
    /// </summary>
    [OpCodeCharacteristics("i64x2.shr_s")]
    [SimdInstructionGenerate<Vec128Shift>()]
    Int64X2ShiftArithRight = 0xcc,
    
    /// <summary>
    /// Shift the bits in each of the 2 64-bit lanes logical right by the same amout.
    /// </summary>
    [OpCodeCharacteristics("i64x2.shr_u")]
    [SimdInstructionGenerate<Vec128Shift>()]
    Int64X2ShiftLogicRight = 0xcd,
    
    /// <summary>
    /// SIMD add 2 64-bit integers.
    /// </summary>
    [OpCodeCharacteristics("i64x2.add")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int64X2Add = 0xce,

    /// <summary>
    /// SIMD subtract 2 64-bit integers.
    /// </summary>
    [OpCodeCharacteristics("i64x2.sub")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int64X2Sub = 0xd1,

    /// <summary>
    /// SIMD multiply 2 64-bit integers.
    /// </summary>
    [OpCodeCharacteristics("i64x2.mul")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int64X2Mul = 0xd5,

    /// <summary>
    /// SIMD negate 4 32-bit floats.
    /// </summary>
    [OpCodeCharacteristics("f32x4.neg")]
    [SimdInstructionGenerate<SimdValueOneToOneCallInstruction>()]
    Float32X4Neg = 0xe1,

    /// <summary>
    /// SIMD square root 4 32-bit floats.
    /// </summary>
    [OpCodeCharacteristics("f32x4.sqrt")]
    [SimdInstructionGenerate<SimdValueOneToOneCallInstruction>()]
    Float32X4Sqrt = 0xe3,

    /// <summary>
    /// SIMD add 4 32-bit floats.
    /// </summary>
    [OpCodeCharacteristics("f32x4.add")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Float32X4Add = 0xe4,

    /// <summary>
    /// SIMD subtract 4 32-bit floats.
    /// </summary>
    [OpCodeCharacteristics("f32x4.sub")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Float32X4Sub = 0xe5,

    /// <summary>
    /// SIMD multiply 4 32-bit floats.
    /// </summary>
    [OpCodeCharacteristics("f32x4.mul")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Float32X4Mul = 0xe6,

    /// <summary>
    /// SIMD divide 4 32-bit floats.
    /// </summary>
    [OpCodeCharacteristics("f32x4.div")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Float32X4Div = 0xe7,

    /// <summary>
    /// SIMD negate 2 64-bit floats.
    /// </summary>
    [OpCodeCharacteristics("f64x2.neg")]
    [SimdInstructionGenerate<SimdValueOneToOneCallInstruction>()]
    Float64X2Neg = 0xed,

    /// <summary>
    /// SIMD square root 2 64-bit floats.
    /// </summary>
    [OpCodeCharacteristics("f64x2.sqrt")]
    [SimdInstructionGenerate<SimdValueOneToOneCallInstruction>()]
    Float64X2Sqrt = 0xef,

    /// <summary>
    /// SIMD add 2 64-bit floats.
    /// </summary>
    [OpCodeCharacteristics("f64x2.add")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Float64X2Add = 0xf0,

    /// <summary>
    /// SIMD subtract 2 64-bit floats.
    /// </summary>
    [OpCodeCharacteristics("f64x2.sub")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Float64X2Sub = 0xf1,

    /// <summary>
    /// SIMD multiply 2 64-bit floats.
    /// </summary>
    [OpCodeCharacteristics("f64x2.mul")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Float64X2Mul = 0xf2,

    /// <summary>
    /// SIMD divide 2 64-bit floats.
    /// </summary>
    [OpCodeCharacteristics("f64x2.div")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Float64X2Div = 0xf3,

    /// <summary>
    /// SIMD bitwise not one 128-bit vector.
    /// </summary>
    [OpCodeCharacteristics("v128.not")]
    [SimdInstructionGenerate<SimdValueOneToOneCallInstruction>()]
    Vec128Not = 0x4d,

    /// <summary>
    /// SIMD bitwise and two 128-bit vectorc.
    /// </summary>
    [OpCodeCharacteristics("v128.and")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Vec128And = 0x4e,

    /// <summary>
    /// SIMD bitwise and-not two 128-bit vectorc.
    /// </summary>
    [OpCodeCharacteristics("v128.andnot")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Vec128AndNot = 0x4f,

    /// <summary>
    /// SIMD bitwise or two 128-bit vectorc.
    /// </summary>
    [OpCodeCharacteristics("v128.or")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Vec128Or = 0x50,

    /// <summary>
    /// SIMD bitwise xor two 128-bit vectorc.
    /// </summary>
    [OpCodeCharacteristics("v128.xor")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Vec128Xor = 0x51,

    /// <summary>
    /// SIMD bitselect from two 128-bit vectors, using a 128-bit mask.
    /// </summary>
    [OpCodeCharacteristics("v128.bitselect")]
    Vec128BitSelect = 0x52,

    /// <summary>
    /// Return 1 if any bit is non-zero, 0 otherwise.
    /// </summary>
    [OpCodeCharacteristics("v128.any_true")]
    [SimdOpTraits(hasMethodInfo: false)]
    Vec128AnyTrue = 0x53,
}

internal sealed record OpCodeInfo(SimdOpCode OpCode, string NativeName, bool HasMethodInfo);

internal static class SimdOpCodeExtensions
{
    private static readonly List<OpCodeInfo> opCodeInfos = typeof(SimdOpCode)
        .GetFields()
        .Where(field => field.IsStatic)
        .Select(field =>
        {
            var opCode = (SimdOpCode)field.GetValue(null)!;
            var name = field.GetCustomAttribute<OpCodeCharacteristicsAttribute>()!.Name;
            var traits = field.GetCustomAttribute<SimdOpTraitsAttribute>() ?? new SimdOpTraitsAttribute();
            return new OpCodeInfo(opCode, name, traits.HasMethodInfo);
        })
        .ToList();

    private static readonly RegeneratingWeakReference<Dictionary<SimdOpCode, OpCodeInfo>> opCodeInfoByOpCode =
        new(() => opCodeInfos.ToDictionary(oci => oci.OpCode, oci => oci));

    public static string ToNativeName(this SimdOpCode opCode)
    {
        opCodeInfoByOpCode.Reference.TryGetValue(opCode, out var result);
        return result!.NativeName;
    }

    internal static MethodInfo FindVector128Getter(string name, Type laneType)
    {
        return typeof(Vector128<>).MakeGenericType(laneType)
            .GetProperty(name, BindingFlags.Public | BindingFlags.Static)!.GetGetMethod()!;
    }

    internal static MethodInfo FindVector128Method(string name, Type parType, int parsCount, bool isGeneric)
    {
        var methods = typeof(Vector128).GetMethods(BindingFlags.Public | BindingFlags.Static);
        var methodInfo = methods.Where(m => m.Name == name).First(m =>
        {
            var pars = m.GetParameters();
            return pars.Length == parsCount && pars.Select(p => p.ParameterType).All(pt =>
                isGeneric
                    ? pt.IsPointer || pt.IsByRef
                    || (pt.IsGenericType && pt.GetGenericTypeDefinition() == typeof(Vector128<>))
                    : pt == parType || pt == typeof(Vector128<>).MakeGenericType(parType) || pt.IsPrimitive);
        });
        return isGeneric && methodInfo.IsGenericMethodDefinition ? methodInfo.MakeGenericMethod(parType) : methodInfo;
    }

    internal static void Deconstruct<T>(this IList<T> list, out T first, out T second)
    {
        first = list[0]!;
        second = list[1];
    }

    private static readonly Dictionary<string, (string, int, bool)> opNameToMethodTuple = new()
    {
        { "load", ("Load", 1, true) },
        { "const", ("Create", 4, false) },
        { "neg", ("Negate", 1, true) },
        { "eq", ("Equals", 2, true) },
        { "ne", ("Equals", 2, true) },
        { "lt_s", ("LessThan", 2, true) },
        { "lt_u", ("LessThan", 2, true) },
        { "gt_s", ("GreaterThan", 2, true) },
        { "gt_u", ("GreaterThan", 2, true) },
        { "le_s", ("LessThanOrEqual", 2, true) },
        { "le_u", ("LessThanOrEqual", 2, true) },
        { "ge_s", ("GreaterThanOrEqual", 2, true) },
        { "ge_u", ("GreaterThanOrEqual", 2, true) },
        { "add", ("Add", 2, true) },
        { "sub", ("Subtract", 2, true) },
        { "mul", ("Multiply", 2, true) },
        { "sqrt", ("Sqrt", 1, true) },
        { "div", ("Divide", 2, true) },
        { "not", ("OnesComplement", 1, true) },
        { "and", ("BitwiseAnd", 2, true) },
        { "andnot", ("AndNot", 2, true) },
        { "or", ("BitwiseOr", 2, true) },
        { "xor", ("Xor", 2, true) },
        { "bitselect", ("ConditionalSelect", 3, true) },
        { "bitmask", ("ExtractMostSignificantBits", 1, true) },
        { "shl", ("ShiftLeft", 2, false) },
        { "shr_s", ("ShiftRightArithmetic", 2, false) },
        { "shr_u", ("ShiftRightLogical", 2, false) },
    };

    private static readonly Dictionary<string, Type> laneTypeToType = new()
    {
        { "v128", typeof(uint) },
        { "i8x16", typeof(byte) },
        { "i16x8", typeof(ushort) },
        { "i32x4", typeof(uint) },
        { "i64x2", typeof(ulong) },
        { "f32x4", typeof(float) },
        { "f64x2", typeof(double) },
    };

    private static Type? SpecialCaseLaneType(string methodName, string opName, string laneType)
    {
        if (!opName.EndsWith("_s", StringComparison.InvariantCulture)) return null; 
        return methodName switch
        {
            "LessThan" or "GreaterThan" or "LessThanOrEqual" 
            or "GreaterThanOrEqual" or "ShiftRightArithmetic" => laneType switch
            {
                "i8x16" => typeof(sbyte),
                "i16x8" => typeof(short),
                "i32x4" => typeof(int),
                "i64x2" => typeof(long),
                _ => null,
            },
            _ => null,
        };
    }
    
    private static readonly RegeneratingWeakReference<Dictionary<SimdOpCode, MethodInfo>> opCodeMethodInfoByOpCode =
        new(() => opCodeInfos.Where(oci => oci.HasMethodInfo).ToDictionary(oci => oci.OpCode, oci =>
        {
            var (laneType, opName) = oci.NativeName.Split('.');
            var (methodName, parCount, isGeneric) = opNameToMethodTuple[opName];
            var parType = SpecialCaseLaneType(methodName, opName, laneType) ?? laneTypeToType[laneType];
            return FindVector128Method(methodName, parType, parCount, isGeneric);
        }));

    public static MethodInfo ToMethodInfo(this SimdOpCode opCode)
    {
        opCodeMethodInfoByOpCode.Reference.TryGetValue(opCode, out var result);
        return result!;
    }

    public static string ToLaneKind(this SimdOpCode opCode) => opCode.ToNativeName().Split('.')[0];
    
    public static Type ToLaneType(this SimdOpCode opCode) => laneTypeToType[opCode.ToLaneKind()];
    

    public enum KnownMethodName { Zero, VecEquals, OnesComplement, ExtractMostSignificantBits }
    
    private sealed record KnownMethods(MethodInfo Zero, MethodInfo VecEquals, 
        MethodInfo OnesComplement, MethodInfo ExtractMostSignificantBits);
    
    private static readonly RegeneratingWeakReference<Dictionary<string, KnownMethods>> wellKnownMethodsByLane =
        new(() => laneTypeToType.Where(kv => kv.Key.StartsWith('i')).ToDictionary(kv => kv.Key, kv =>
        {
            var laneType = kv.Value;
            var zero = FindVector128Getter("Zero", laneType);
            var equals = FindVector128Method("Equals", laneType, 2, true);
            var onesComplement = FindVector128Method("OnesComplement", laneType, 1, true);
            var extractMsb = FindVector128Method("ExtractMostSignificantBits", laneType, 1, true);
            return new KnownMethods(zero, equals, onesComplement, extractMsb);
        }));
        
    public static MethodInfo GetWellKnownMethod(string laneKind, KnownMethodName method)
    {
        wellKnownMethodsByLane.Reference.TryGetValue(laneKind, out var result);
        return method switch
        {
            KnownMethodName.Zero => result!.Zero,
            KnownMethodName.VecEquals => result!.VecEquals,
            KnownMethodName.OnesComplement => result!.OnesComplement,
            KnownMethodName.ExtractMostSignificantBits => result!.ExtractMostSignificantBits,
            _ => throw new InvalidOperationException($"Unexpected known method name: {method}."),
        };
    }
}
