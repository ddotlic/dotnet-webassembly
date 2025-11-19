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
    /// Load an 8-bit scalar from memory and splat across lanes.
    /// </summary>
    [OpCodeCharacteristics("v128.load8_splat")]
    [SimdInstructionGenerate<Vec128LoadSplat>(includeReaderConstructor: true)]
    Vec128Load8Splat = 0x07,

    /// <summary>
    /// Load a 16-bit scalar from memory and splat across lanes.
    /// </summary>
    [OpCodeCharacteristics("v128.load16_splat")]
    [SimdInstructionGenerate<Vec128LoadSplat>(includeReaderConstructor: true)]
    Vec128Load16Splat = 0x08,

    /// <summary>
    /// Load a 32-bit scalar from memory and splat across lanes.
    /// </summary>
    [OpCodeCharacteristics("v128.load32_splat")]
    [SimdInstructionGenerate<Vec128LoadSplat>(includeReaderConstructor: true)]
    Vec128Load32Splat = 0x09,

    /// <summary>
    /// Load a 64-bit scalar from memory and splat across lanes.
    /// </summary>
    [OpCodeCharacteristics("v128.load64_splat")]
    [SimdInstructionGenerate<Vec128LoadSplat>(includeReaderConstructor: true)]
    Vec128Load64Splat = 0x0a,

    /// <summary>
    /// Store a SIMD vector to memory.
    /// </summary>
    [OpCodeCharacteristics("v128.store")]
    Vec128Store = 0x0b,

    /// <summary>
    /// Shuffle bytes from two vectors using indices from immediate values.
    /// </summary>
    [OpCodeCharacteristics("i8x16.shuffle")]
    [SimdOpTraits(hasMethodInfo: false)]
    Int8X16Shuffle = 0x0d,

    /// <summary>
    /// Shuffle bytes from the first vector using indices from the second vector.
    /// </summary>
    [OpCodeCharacteristics("i8x16.swizzle")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int8X16Swizzle = 0x0e,

    /// <summary>
    /// Splat an 8-bit value into a v128 vector.
    /// </summary>
    [OpCodeCharacteristics("i8x16.splat")]
    [SimdInstructionGenerate<Vec128Splat>()]
    Int8X16Splat = 0x0f,

    /// <summary>
    /// Splat a 16-bit value into a v128 vector.
    /// </summary>
    [OpCodeCharacteristics("i16x8.splat")]
    [SimdInstructionGenerate<Vec128Splat>()]
    Int16X8Splat = 0x10,

    /// <summary>
    /// Splat a 32-bit value into a v128 vector.
    /// </summary>
    [OpCodeCharacteristics("i32x4.splat")]
    [SimdInstructionGenerate<Vec128Splat>()]
    Int32X4Splat = 0x11,

    /// <summary>
    /// Splat a 32-bit value into a v128 vector.
    /// </summary>
    [OpCodeCharacteristics("i64x2.splat")]
    [SimdInstructionGenerate<Vec128Splat>()]
    Int64X2Splat = 0x12,

    /// <summary>
    /// Splat a 32-bit float into a v128 vector.
    /// </summary>
    [OpCodeCharacteristics("f32x4.splat")]
    [SimdInstructionGenerate<Vec128Splat>()]
    Float32X4Splat = 0x13,

    /// <summary>
    /// Splat a 64-bit float into a v128 vector.
    /// </summary>
    [OpCodeCharacteristics("f64x2.splat")]
    [SimdInstructionGenerate<Vec128Splat>()]
    Float64X2Splat = 0x14,

    /// <summary>
    /// Extract a signed 8-bit lane from a SIMD vector and sign-extend to i32.
    /// </summary>
    [OpCodeCharacteristics("i8x16.extract_lane_s")]
    [SimdInstructionGenerate<Vec128ExtractLane>(includeReaderConstructor: true)]
    Int8X16ExtractLaneS = 0x15,

    /// <summary>
    /// Extract an unsigned 8-bit lane from a SIMD vector and zero-extend to i32.
    /// </summary>
    [OpCodeCharacteristics("i8x16.extract_lane_u")]
    [SimdInstructionGenerate<Vec128ExtractLane>(includeReaderConstructor: true)]
    Int8X16ExtractLaneU = 0x16,

    /// <summary>
    /// Replace an 8-bit lane in a SIMD vector with a new value.
    /// </summary>
    [OpCodeCharacteristics("i8x16.replace_lane")]
    [SimdInstructionGenerate<Vec128ReplaceLane>(includeReaderConstructor: true)]
    Int8X16ReplaceLane = 0x17,

    /// <summary>
    /// Extract a signed 16-bit lane from a SIMD vector and sign-extend to i32.
    /// </summary>
    [OpCodeCharacteristics("i16x8.extract_lane_s")]
    [SimdInstructionGenerate<Vec128ExtractLane>(includeReaderConstructor: true)]
    Int16X8ExtractLaneS = 0x18,

    /// <summary>
    /// Extract an unsigned 16-bit lane from a SIMD vector and zero-extend to i32.
    /// </summary>
    [OpCodeCharacteristics("i16x8.extract_lane_u")]
    [SimdInstructionGenerate<Vec128ExtractLane>(includeReaderConstructor: true)]
    Int16X8ExtractLaneU = 0x19,

    /// <summary>
    /// Replace a 16-bit lane in a SIMD vector with a new value.
    /// </summary>
    [OpCodeCharacteristics("i16x8.replace_lane")]
    [SimdInstructionGenerate<Vec128ReplaceLane>(includeReaderConstructor: true)]
    Int16X8ReplaceLane = 0x1a,

    /// <summary>
    /// Extract a lane (32-bit int value) from a SIMD vector.
    /// </summary>
    [OpCodeCharacteristics("i32x4.extract_lane")]
    [SimdInstructionGenerate<Vec128ExtractLane>(includeReaderConstructor: true)]
    Int32X4ExtractLane = 0x1b,

    /// <summary>
    /// Replace a 32-bit lane in a SIMD vector with a new value.
    /// </summary>
    [OpCodeCharacteristics("i32x4.replace_lane")]
    [SimdInstructionGenerate<Vec128ReplaceLane>(includeReaderConstructor: true)]
    Int32X4ReplaceLane = 0x1c,

    /// <summary>
    /// Extract a lane (64-bit int value) from a SIMD vector.
    /// </summary>
    [OpCodeCharacteristics("i64x2.extract_lane")]
    [SimdInstructionGenerate<Vec128ExtractLane>(includeReaderConstructor: true)]
    Int64X2ExtractLane = 0x1d,

    /// <summary>
    /// Replace a 64-bit lane in a SIMD vector with a new value.
    /// </summary>
    [OpCodeCharacteristics("i64x2.replace_lane")]
    [SimdInstructionGenerate<Vec128ReplaceLane>(includeReaderConstructor: true)]
    Int64X2ReplaceLane = 0x1e,

    /// <summary>
    /// Extract a lane (32-bit float value) from a SIMD vector.
    /// </summary>
    [OpCodeCharacteristics("f32x4.extract_lane")]
    [SimdInstructionGenerate<Vec128ExtractLane>(includeReaderConstructor: true)]
    Float32X4ExtractLane = 0x1f,

    /// <summary>
    /// Replace a 32-bit float lane in a SIMD vector with a new value.
    /// </summary>
    [OpCodeCharacteristics("f32x4.replace_lane")]
    [SimdInstructionGenerate<Vec128ReplaceLane>(includeReaderConstructor: true)]
    Float32X4ReplaceLane = 0x20,

    /// <summary>
    /// Extract a lane (64-bit float value) from a SIMD vector.
    /// </summary>
    [OpCodeCharacteristics("f64x2.extract_lane")]
    [SimdInstructionGenerate<Vec128ExtractLane>(includeReaderConstructor: true)]
    Float64X2ExtractLane = 0x21,

    /// <summary>
    /// Replace a 64-bit float lane in a SIMD vector with a new value.
    /// </summary>
    [OpCodeCharacteristics("f64x2.replace_lane")]
    [SimdInstructionGenerate<Vec128ReplaceLane>(includeReaderConstructor: true)]
    Float64X2ReplaceLane = 0x22,

    /// <summary>
    /// Load a 32-bit value from memory into vector and zero pad.
    /// </summary>
    [OpCodeCharacteristics("v128.load32_zero")]
    Vec128Load32Zero = 0x5c,

    /// <summary>
    /// Load a 64-bit value from memory into vector and zero pad.
    /// </summary>
    [OpCodeCharacteristics("v128.load64_zero")]
    Vec128Load64Zero = 0x5d,

    /// <summary>
    /// Load a single 8-bit value from memory into a lane of a v128 vector.
    /// </summary>
    [OpCodeCharacteristics("v128.load8_lane")]
    [SimdInstructionGenerate<Vec128LoadLane>(includeReaderConstructor: true)]
    Vec128Load8Lane = 0x54,

    /// <summary>
    /// Load a single 16-bit value from memory into a lane of a v128 vector.
    /// </summary>
    [OpCodeCharacteristics("v128.load16_lane")]
    [SimdInstructionGenerate<Vec128LoadLane>(includeReaderConstructor: true)]
    Vec128Load16Lane = 0x55,

    /// <summary>
    /// Load a single 32-bit value from memory into a lane of a v128 vector.
    /// </summary>
    [OpCodeCharacteristics("v128.load32_lane")]
    [SimdInstructionGenerate<Vec128LoadLane>(includeReaderConstructor: true)]
    Vec128Load32Lane = 0x56,

    /// <summary>
    /// Load a single 64-bit value from memory into a lane of a v128 vector.
    /// </summary>
    [OpCodeCharacteristics("v128.load64_lane")]
    [SimdInstructionGenerate<Vec128LoadLane>(includeReaderConstructor: true)]
    Vec128Load64Lane = 0x57,

    /// <summary>
    /// Store a single 8-bit value from a lane of a v128 vector to memory.
    /// </summary>
    [OpCodeCharacteristics("v128.store8_lane")]
    [SimdInstructionGenerate<Vec128StoreLane>(includeReaderConstructor: true)]
    Vec128Store8Lane = 0x58,

    /// <summary>
    /// Store a single 16-bit value from a lane of a v128 vector to memory.
    /// </summary>
    [OpCodeCharacteristics("v128.store16_lane")]
    [SimdInstructionGenerate<Vec128StoreLane>(includeReaderConstructor: true)]
    Vec128Store16Lane = 0x59,

    /// <summary>
    /// Store a single 32-bit value from a lane of a v128 vector to memory.
    /// </summary>
    [OpCodeCharacteristics("v128.store32_lane")]
    [SimdInstructionGenerate<Vec128StoreLane>(includeReaderConstructor: true)]
    Vec128Store32Lane = 0x5a,

    /// <summary>
    /// Store a single 64-bit value from a lane of a v128 vector to memory.
    /// </summary>
    [OpCodeCharacteristics("v128.store64_lane")]
    [SimdInstructionGenerate<Vec128StoreLane>(includeReaderConstructor: true)]
    Vec128Store64Lane = 0x5b,

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
    /// Lane-wise compare 16 8-bit int lanes, equality.
    /// </summary>
    [OpCodeCharacteristics("i8x16.eq")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int8X16Equal = 0x23,
    /// <summary>
    /// Lane-wise compare 16 8-bit int lanes, non-equality.
    /// </summary>
    [OpCodeCharacteristics("i8x16.ne")]
    [SimdInstructionGenerate<Vec128NotEqual>()]
    Int8X16NotEqual = 0x24,

    /// <summary>
    /// Lane-wise compare 16 8-bit int lanes, signed less than.
    /// </summary>
    [OpCodeCharacteristics("i8x16.lt_s")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int8X16LessThanSigned = 0x25,

    /// <summary>
    /// Lane-wise compare 16 8-bit int lanes, unsigned less than.
    /// </summary>
    [OpCodeCharacteristics("i8x16.lt_u")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int8X16LessThanUnsigned = 0x26,

    /// <summary>
    /// Lane-wise compare 16 8-bit int lanes, signed greater than.
    /// </summary>
    [OpCodeCharacteristics("i8x16.gt_s")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int8X16GreaterThanSigned = 0x27,

    /// <summary>
    /// Lane-wise compare 16 8-bit int lanes, unsigned greater than.
    /// </summary>
    [OpCodeCharacteristics("i8x16.gt_u")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int8X16GreaterThanUnsigned = 0x28,

    /// <summary>
    /// Lane-wise compare 16 8-bit int lanes, signed less than or equal.
    /// </summary>
    [OpCodeCharacteristics("i8x16.le_s")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int8X16LessThanOrEqualSigned = 0x29,

    /// <summary>
    /// Lane-wise compare 16 8-bit int lanes, unsigned less than or equal.
    /// </summary>
    [OpCodeCharacteristics("i8x16.le_u")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int8X16LessThanOrEqualUnsigned = 0x2a,

    /// <summary>
    /// Lane-wise compare 16 8-bit int lanes, signed greater than or equal.
    /// </summary>
    [OpCodeCharacteristics("i8x16.ge_s")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int8X16GreaterThanOrEqualSigned = 0x2b,

    /// <summary>
    /// Lane-wise compare 16 8-bit int lanes, unsigned greater than or equal.
    /// </summary>
    [OpCodeCharacteristics("i8x16.ge_u")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int8X16GreaterThanOrEqualUnsigned = 0x2c,

    /// <summary>
    /// Return 1 if all 16 8-bit int lanes are non-zero, 0 otherwise.
    /// </summary>
    [OpCodeCharacteristics("i8x16.all_true")]
    [SimdInstructionGenerate<Vec128AllTrue>()]
    [SimdOpTraits(hasMethodInfo: false)]
    Int8X16AllTrue = 0x63,

    /// <summary>
    /// Extract the high bit for each of the 16 8-bit int lanes and produce a scalar mask with all bits concatenated.
    /// </summary>
    [OpCodeCharacteristics("i8x16.bitmask")]
    [SimdInstructionGenerate<Vec128BitMask>()]
    [SimdOpTraits(hasMethodInfo: true)]
    Int8X16BitMask = 0x64,

    /// <summary>
    /// Shift the bits in each of the 16 8-bit int lanes left by the same amout.
    /// </summary>
    [OpCodeCharacteristics("i8x16.shl")]
    [SimdInstructionGenerate<Vec128Shift>()]
    Int8X16ShiftLeft = 0x6b,

    /// <summary>
    /// Shift the bits in each of the 16 8-bit int lanes arithmetic right by the same amout.
    /// </summary>
    [OpCodeCharacteristics("i8x16.shr_s")]
    [SimdInstructionGenerate<Vec128Shift>()]
    Int8X16ShiftArithRight = 0x6c,

    /// <summary>
    /// Shift the bits in each of the 16 8-bit int lanes logical right by the same amout.
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
    /// SIMD add 16 8-bit integers with saturation (signed).
    /// </summary>
    [OpCodeCharacteristics("i8x16.add_saturate_s")]
    [SimdInstructionGenerate<Vec128AddSaturate>()]
    [SimdOpTraits(hasMethodInfo: false)]
    Int8X16AddSaturateSigned = 0x6f,

    /// <summary>
    /// SIMD add 16 8-bit integers with saturation (unsigned).
    /// </summary>
    [OpCodeCharacteristics("i8x16.add_saturate_u")]
    [SimdInstructionGenerate<Vec128AddSaturate>()]
    [SimdOpTraits(hasMethodInfo: false)]
    Int8X16AddSaturateUnsigned = 0x70,

    /// <summary>
    /// SIMD subtract 16 8-bit integers with saturation (signed).
    /// </summary>
    [OpCodeCharacteristics("i8x16.sub_saturate_s")]
    [SimdInstructionGenerate<Vec128SubSaturate>()]
    [SimdOpTraits(hasMethodInfo: false)]
    Int8X16SubSaturateSigned = 0x72,

    /// <summary>
    /// SIMD subtract 16 8-bit integers with saturation (unsigned).
    /// </summary>
    [OpCodeCharacteristics("i8x16.sub_saturate_u")]
    [SimdInstructionGenerate<Vec128SubSaturate>()]
    [SimdOpTraits(hasMethodInfo: false)]
    Int8X16SubSaturateUnsigned = 0x73,

    /// <summary>
    /// SIMD negate 8 16-bit integers. 
    /// </summary>
    [OpCodeCharacteristics("i16x8.neg")]
    [SimdInstructionGenerate<SimdValueOneToOneCallInstruction>()]
    Int16X8Neg = 0x81,

    /// <summary>
    /// Lane-wise compare 8 16-bit int lanes, equality.
    /// </summary>
    [OpCodeCharacteristics("i16x8.eq")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int16X8Equal = 0x2d,

    /// <summary>
    /// Lane-wise compare 8 16-bit int lanes, non-equality.
    /// </summary>
    [OpCodeCharacteristics("i16x8.ne")]
    [SimdInstructionGenerate<Vec128NotEqual>()]
    Int16X8NotEqual = 0x2e,

    /// <summary>
    /// Lane-wise compare 8 16-bit int lanes, signed less than.
    /// </summary>
    [OpCodeCharacteristics("i16x8.lt_s")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int16X8LessThanSigned = 0x2f,

    /// <summary>
    /// Lane-wise compare 8 16-bit int lanes, unsigned less than.
    /// </summary>
    [OpCodeCharacteristics("i16x8.lt_u")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int16X8LessThanUnsigned = 0x30,

    /// <summary>
    /// Lane-wise compare 8 16-bit int lanes, signed greater than.
    /// </summary>
    [OpCodeCharacteristics("i16x8.gt_s")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int16X8GreaterThanSigned = 0x31,

    /// <summary>
    /// Lane-wise compare 8 16-bit int lanes, unsigned greater than.
    /// </summary>
    [OpCodeCharacteristics("i16x8.gt_u")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int16X8GreaterThanUnsigned = 0x32,

    /// <summary>
    /// Lane-wise compare 8 16-bit int lanes, signed less than or equal.
    /// </summary>
    [OpCodeCharacteristics("i16x8.le_s")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int16X8LessThanOrEqualSigned = 0x33,

    /// <summary>
    /// Lane-wise compare 8 16-bit int lanes, unsigned less than or equal.
    /// </summary>
    [OpCodeCharacteristics("i16x8.le_u")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int16X8LessThanOrEqualUnsigned = 0x34,

    /// <summary>
    /// Lane-wise compare 8 16-bit int lanes, signed greater than or equal.
    /// </summary>
    [OpCodeCharacteristics("i16x8.ge_s")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int16X8GreaterThanOrEqualSigned = 0x35,

    /// <summary>
    /// Lane-wise compare 8 16-bit int lanes, unsigned greater than or equal.
    /// </summary>
    [OpCodeCharacteristics("i16x8.ge_u")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int16X8GreaterThanOrEqualUnsigned = 0x36,

    /// <summary>
    /// Return 1 if all 8 16-bit int lanes are non-zero, 0 otherwise.
    /// </summary>
    [OpCodeCharacteristics("i16x8.all_true")]
    [SimdInstructionGenerate<Vec128AllTrue>()]
    [SimdOpTraits(hasMethodInfo: false)]
    Int16X8AllTrue = 0x83,

    /// <summary>
    /// Extract the high bit for each of the 8 16-bit int lanes and produce a scalar mask with all bits concatenated.
    /// </summary>
    [OpCodeCharacteristics("i16x8.bitmask")]
    [SimdInstructionGenerate<Vec128BitMask>()]
    [SimdOpTraits(hasMethodInfo: true)]
    Int16X8BitMask = 0x84,

    /// <summary>
    /// Shift the bits in each of the 8 16-bit int lanes left by the same amout.
    /// </summary>
    [OpCodeCharacteristics("i16x8.shl")]
    [SimdInstructionGenerate<Vec128Shift>()]
    Int16X8ShiftLeft = 0x8b,

    /// <summary>
    /// Shift the bits in each of the 8 16-bit int lanes arithmetic right by the same amout.
    /// </summary>
    [OpCodeCharacteristics("i16x8.shr_s")]
    [SimdInstructionGenerate<Vec128Shift>()]
    Int16X8ShiftArithRight = 0x8c,

    /// <summary>
    /// Shift the bits in each of the 8 16-bit int lanes logical right by the same amout.
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
    /// SIMD add 8 16-bit integers with saturation (signed).
    /// </summary>
    [OpCodeCharacteristics("i16x8.add_saturate_s")]
    [SimdInstructionGenerate<Vec128AddSaturate>()]
    [SimdOpTraits(hasMethodInfo: false)]
    Int16X8AddSaturateSigned = 0x8f,

    /// <summary>
    /// SIMD add 8 16-bit integers with saturation (unsigned).
    /// </summary>
    [OpCodeCharacteristics("i16x8.add_saturate_u")]
    [SimdInstructionGenerate<Vec128AddSaturate>()]
    [SimdOpTraits(hasMethodInfo: false)]
    Int16X8AddSaturateUnsigned = 0x90,

    /// <summary>
    /// SIMD subtract 8 16-bit integers with saturation (signed).
    /// </summary>
    [OpCodeCharacteristics("i16x8.sub_saturate_s")]
    [SimdInstructionGenerate<Vec128SubSaturate>()]
    [SimdOpTraits(hasMethodInfo: false)]
    Int16X8SubSaturateSigned = 0x92,

    /// <summary>
    /// SIMD subtract 8 16-bit integers with saturation (unsigned).
    /// </summary>
    [OpCodeCharacteristics("i16x8.sub_saturate_u")]
    [SimdInstructionGenerate<Vec128SubSaturate>()]
    [SimdOpTraits(hasMethodInfo: false)]
    Int16X8SubSaturateUnsigned = 0x93,

    /// <summary>
    /// SIMD negate 4 32-bit integers. 
    /// </summary>
    [OpCodeCharacteristics("i32x4.neg")]
    [SimdInstructionGenerate<SimdValueOneToOneCallInstruction>()]
    Int32X4Neg = 0xa1,

    /// <summary>
    /// Lane-wise compare 4 32-bit int lanes, equality.
    /// </summary>
    [OpCodeCharacteristics("i32x4.eq")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int32X4Equal = 0x37,

    /// <summary>
    /// Lane-wise compare 4 32-bit int lanes, non-equality.
    /// </summary>
    [OpCodeCharacteristics("i32x4.ne")]
    [SimdInstructionGenerate<Vec128NotEqual>()]
    Int32X4NotEqual = 0x38,

    /// <summary>
    /// Lane-wise compare 4 32-bit int lanes, signed less than.
    /// </summary>
    [OpCodeCharacteristics("i32x4.lt_s")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int32X4LessThanSigned = 0x39,

    /// <summary>
    /// Lane-wise compare 4 32-bit int lanes, unsigned less than.
    /// </summary>
    [OpCodeCharacteristics("i32x4.lt_u")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int32X4LessThanUnsigned = 0x3a,

    /// <summary>
    /// Lane-wise compare 4 32-bit int lanes, signed greater than.
    /// </summary>
    [OpCodeCharacteristics("i32x4.gt_s")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int32X4GreaterThanSigned = 0x3b,

    /// <summary>
    /// Lane-wise compare 4 32-bit int lanes, unsigned greater than.
    /// </summary>
    [OpCodeCharacteristics("i32x4.gt_u")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int32X4GreaterThanUnsigned = 0x3c,

    /// <summary>
    /// Lane-wise compare 4 32-bit int lanes, signed less than or equal.
    /// </summary>
    [OpCodeCharacteristics("i32x4.le_s")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int32X4LessThanOrEqualSigned = 0x3d,

    /// <summary>
    /// Lane-wise compare 4 32-bit int lanes, unsigned less than or equal.
    /// </summary>
    [OpCodeCharacteristics("i32x4.le_u")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int32X4LessThanOrEqualUnsigned = 0x3e,

    /// <summary>
    /// Lane-wise compare 4 32-bit int lanes, signed greater than or equal.
    /// </summary>
    [OpCodeCharacteristics("i32x4.ge_s")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int32X4GreaterThanOrEqualSigned = 0x3f,

    /// <summary>
    /// Lane-wise compare 4 32-bit int lanes, unsigned greater than or equal.
    /// </summary>
    [OpCodeCharacteristics("i32x4.ge_u")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int32X4GreaterThanOrEqualUnsigned = 0x40,

    /// <summary>
    /// Return 1 if all 4 32-bit int lanes are non-zero, 0 otherwise.
    /// </summary>
    [OpCodeCharacteristics("i32x4.all_true")]
    [SimdInstructionGenerate<Vec128AllTrue>()]
    [SimdOpTraits(hasMethodInfo: false)]
    Int32X4AllTrue = 0xa3,

    /// <summary>
    /// Extract the high bit for each of the 4 32-bit int lanes and produce a scalar mask with all bits concatenated.
    /// </summary>
    [OpCodeCharacteristics("i32x4.bitmask")]
    [SimdInstructionGenerate<Vec128BitMask>()]
    [SimdOpTraits(hasMethodInfo: true)]
    Int32X4BitMask = 0xa4,

    /// <summary>
    /// Shift the bits in each of the 4 32-bit int lanes left by the same amout.
    /// </summary>
    [OpCodeCharacteristics("i32x4.shl")]
    [SimdInstructionGenerate<Vec128Shift>()]
    Int32X4ShiftLeft = 0xab,

    /// <summary>
    /// Shift the bits in each of the 4 32-bit int lanes arithmetic right by the same amout.
    /// </summary>
    [OpCodeCharacteristics("i32x4.shr_s")]
    [SimdInstructionGenerate<Vec128Shift>()]
    Int32X4ShiftArithRight = 0xac,

    /// <summary>
    /// Shift the bits in each of the 4 32-bit int lanes logical right by the same amout.
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
    /// Lane-wise compare 2 64-bit int lanes, equality.
    /// </summary>
    [OpCodeCharacteristics("i64x2.eq")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int64X2Equal = 0xd6,

    /// <summary>
    /// Lane-wise compare 2 64-bit int lanes, non-equality.
    /// </summary>
    [OpCodeCharacteristics("i64x2.ne")]
    [SimdInstructionGenerate<Vec128NotEqual>()]
    Int64X2NotEqual = 0xd7,

    /// <summary>
    /// Lane-wise compare 2 64-bit int lanes, signed less than.
    /// </summary>
    [OpCodeCharacteristics("i64x2.lt_s")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int64X2LessThanSigned = 0xd8,

    /// <summary>
    /// Lane-wise compare 2 64-bit int lanes, signed greater than.
    /// </summary>
    [OpCodeCharacteristics("i64x2.gt_s")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int64X2GreaterThanSigned = 0xd9,

    /// <summary>
    /// Lane-wise compare 2 64-bit int lanes, signed less than or equal.
    /// </summary>
    [OpCodeCharacteristics("i64x2.le_s")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int64X2LessThanOrEqualSigned = 0xda,

    /// <summary>
    /// Lane-wise compare 2 64-bit int lanes, signed greater than or equal.
    /// </summary>
    [OpCodeCharacteristics("i64x2.ge_s")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Int64X2GreaterThanOrEqualSigned = 0xdb,

    /// <summary>
    /// Return 1 if all 2 64-bit int lanes are non-zero, 0 otherwise.
    /// </summary>
    [OpCodeCharacteristics("i64x2.all_true")]
    [SimdInstructionGenerate<Vec128AllTrue>()]
    [SimdOpTraits(hasMethodInfo: false)]
    Int64X2AllTrue = 0xc3,

    /// <summary>
    /// Extract the high bit for each of the 2 64-bit int lanes and produce a scalar mask with all bits concatenated.
    /// </summary>
    [OpCodeCharacteristics("i64x2.bitmask")]
    [SimdInstructionGenerate<Vec128BitMask>()]
    [SimdOpTraits(hasMethodInfo: true)]
    Int64X2BitMask = 0xc4,

    /// <summary>
    /// Shift the bits in each of the 2 64-bit int lanes left by the same amout.
    /// </summary>
    [OpCodeCharacteristics("i64x2.shl")]
    [SimdInstructionGenerate<Vec128Shift>()]
    Int64X2ShiftLeft = 0xcb,

    /// <summary>
    /// Shift the bits in each of the 2 64-bit int lanes arithmetic right by the same amout.
    /// </summary>
    [OpCodeCharacteristics("i64x2.shr_s")]
    [SimdInstructionGenerate<Vec128Shift>()]
    Int64X2ShiftArithRight = 0xcc,

    /// <summary>
    /// Shift the bits in each of the 2 64-bit int lanes logical right by the same amout.
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
    /// Lane-wise compare 4 32-bit float lanes, equality.
    /// </summary>
    [OpCodeCharacteristics("f32x4.eq")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Float32X4Equal = 0x41,

    /// <summary>
    /// Lane-wise compare 4 32-bit float lanes, non-equality.
    /// </summary>
    [OpCodeCharacteristics("f32x4.ne")]
    [SimdInstructionGenerate<Vec128NotEqual>()]
    Float32X4NotEqual = 0x42,

    /// <summary>
    /// Lane-wise compare 4 32-bit float lanes, signed less than.
    /// </summary>
    [OpCodeCharacteristics("f32x4.lt")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Float32X4LessThan = 0x43,

    /// <summary>
    /// Lane-wise compare 4 32-bit float lanes, signed greater than.
    /// </summary>
    [OpCodeCharacteristics("f32x4.gt")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Float32X4GreaterThan = 0x44,

    /// <summary>
    /// Lane-wise compare 4 32-bit float lanes, signed less than or equal.
    /// </summary>
    [OpCodeCharacteristics("f32x4.le")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Float32X4LessThanOrEqual = 0x45,

    /// <summary>
    /// Lane-wise compare 4 32-bit float lanes, signed greater than or equal.
    /// </summary>
    [OpCodeCharacteristics("f32x4.ge")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Float32X4GreaterThanOrEqual = 0x46,

    /// <summary>
    /// SIMD absolute value 4 32-bit floats.
    /// </summary>
    [OpCodeCharacteristics("f32x4.abs")]
    [SimdInstructionGenerate<SimdValueOneToOneCallInstruction>()]
    Float32X4Abs = 0xe0,

    /// <summary>
    /// SIMD minimum 4 32-bit floats (lane-wise, WebAssembly semantics).
    /// </summary>
    [OpCodeCharacteristics("f32x4.min")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Float32X4Min = 0xe8,

    /// <summary>
    /// SIMD maximum 4 32-bit floats (lane-wise, WebAssembly semantics).
    /// </summary>
    [OpCodeCharacteristics("f32x4.max")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Float32X4Max = 0xe9,

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
    /// Lane-wise compare 2 64-bit float lanes, equality.
    /// </summary>
    [OpCodeCharacteristics("f64x2.eq")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Float64X2Equal = 0x47,

    /// <summary>
    /// Lane-wise compare 2 64-bit float lanes, non-equality.
    /// </summary>
    [OpCodeCharacteristics("f64x2.ne")]
    [SimdInstructionGenerate<Vec128NotEqual>()]
    Float64X2NotEqual = 0x48,

    /// <summary>
    /// Lane-wise compare 2 64-bit float lanes, signed less than.
    /// </summary>
    [OpCodeCharacteristics("f64x2.lt")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Float64X2LessThan = 0x49,

    /// <summary>
    /// Lane-wise compare 2 64-bit float lanes, signed greater than.
    /// </summary>
    [OpCodeCharacteristics("f64x2.gt")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Float64X2GreaterThan = 0x4a,

    /// <summary>
    /// Lane-wise compare 2 64-bit float lanes, signed less than or equal.
    /// </summary>
    [OpCodeCharacteristics("f64x2.le")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Float64X2LessThanOrEqual = 0x4b,

    /// <summary>
    /// Lane-wise compare 2 64-bit float lanes, signed greater than or equal.
    /// </summary>
    [OpCodeCharacteristics("f64x2.ge")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Float64X2GreaterThanOrEqual = 0x4c,

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
    /// SIMD absolute value 2 64-bit floats.
    /// </summary>
    [OpCodeCharacteristics("f64x2.abs")]
    [SimdInstructionGenerate<SimdValueOneToOneCallInstruction>()]
    Float64X2Abs = 0xec,

    /// <summary>
    /// SIMD minimum 2 64-bit floats (lane-wise, WebAssembly semantics).
    /// </summary>
    [OpCodeCharacteristics("f64x2.min")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Float64X2Min = 0xf4,

    /// <summary>
    /// SIMD maximum 2 64-bit floats (lane-wise, WebAssembly semantics).
    /// </summary>
    [OpCodeCharacteristics("f64x2.max")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Float64X2Max = 0xf5,

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
    /// SIMD convert signed i32x4 -> f32x4 (vectorized convert).
    /// </summary>
    [OpCodeCharacteristics("f32x4.convert_i32x4_s")]
    [SimdInstructionGenerate<SimdValueOneToOneCallInstruction>()]
    Float32X4ConvertI32X4Signed = 0xfa,

    /// <summary>
    /// SIMD convert unsigned i32x4 -> f32x4 (vectorized convert).
    /// </summary>
    [OpCodeCharacteristics("f32x4.convert_i32x4_u")]
    [SimdInstructionGenerate<SimdValueOneToOneCallInstruction>()]
    Float32X4ConvertI32X4Unsigned = 0xfb,
    
    /// <summary>
    /// SIMD ceiling of 4 32-bit floats.
    /// </summary>
    [OpCodeCharacteristics("f32x4.ceil")]
    [SimdInstructionGenerate<SimdValueOneToOneCallInstruction>()]
    Float32X4Ceil = 0x67,

    /// <summary>
    /// SIMD floor of 4 32-bit floats.
    /// </summary>
    [OpCodeCharacteristics("f32x4.floor")]
    [SimdInstructionGenerate<SimdValueOneToOneCallInstruction>()]
    Float32X4Floor = 0x68,

    /// <summary>
    /// SIMD truncate of 4 32-bit floats.
    /// </summary>
    [OpCodeCharacteristics("f32x4.trunc")]
    [SimdInstructionGenerate<SimdValueOneToOneCallInstruction>()]
    Float32X4Trunc = 0x69,

    /// <summary>
    /// SIMD round-to-nearest of 4 32-bit floats.
    /// </summary>
    [OpCodeCharacteristics("f32x4.nearest")]
    [SimdInstructionGenerate<SimdValueOneToOneCallInstruction>()]
    Float32X4Nearest = 0x6a,

    /// <summary>
    /// SIMD ceiling of 2 64-bit floats.
    /// </summary>
    [OpCodeCharacteristics("f64x2.ceil")]
    [SimdInstructionGenerate<SimdValueOneToOneCallInstruction>()]
    Float64X2Ceil = 0x74,

    /// <summary>
    /// SIMD floor of 2 64-bit floats.
    /// </summary>
    [OpCodeCharacteristics("f64x2.floor")]
    [SimdInstructionGenerate<SimdValueOneToOneCallInstruction>()]
    Float64X2Floor = 0x75,

    /// <summary>
    /// SIMD truncate of 2 64-bit floats.
    /// </summary>
    [OpCodeCharacteristics("f64x2.trunc")]
    [SimdInstructionGenerate<SimdValueOneToOneCallInstruction>()]
    Float64X2Trunc = 0x7a,

    /// <summary>
    /// SIMD round-to-nearest of 2 64-bit floats.
    /// </summary>
    [OpCodeCharacteristics("f64x2.nearest")]
    [SimdInstructionGenerate<SimdValueOneToOneCallInstruction>()]
    Float64X2Nearest = 0x94,

    /// <summary>
    /// SIMD bitwise not one 128-bit vector.
    /// </summary>
    [OpCodeCharacteristics("v128.not")]
    [SimdInstructionGenerate<SimdValueOneToOneCallInstruction>()]
    Vec128Not = 0x4d,

    /// <summary>
    /// SIMD bitwise and two 128-bit vectors.
    /// </summary>
    [OpCodeCharacteristics("v128.and")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Vec128And = 0x4e,

    /// <summary>
    /// SIMD bitwise and-not two 128-bit vectors.
    /// </summary>
    [OpCodeCharacteristics("v128.andnot")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Vec128AndNot = 0x4f,

    /// <summary>
    /// SIMD bitwise or two 128-bit vectors.
    /// </summary>
    [OpCodeCharacteristics("v128.or")]
    [SimdInstructionGenerate<SimdValueTwoToOneCallInstruction>()]
    Vec128Or = 0x50,

    /// <summary>
    /// SIMD bitwise xor two 128-bit vectors.
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
    /// Return 1 if any bit in a 128-bit vector is non-zero, 0 otherwise.
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
            if (pars.Length != parsCount) return false;
            var vectorType = typeof(Vector128<>).MakeGenericType(parType);
            return name switch
            {
                "ShiftLeft" or "ShiftRightArithmetic" or "ShiftRightLogical" =>
                    pars[0].ParameterType == vectorType
                    && pars[1].ParameterType == typeof(int),
                "GetElement" or "WithElement" => true,
                "Ceiling" or "Floor" or "Truncate" or "Round" => pars[0].ParameterType == vectorType,
                _ => pars.Select(p => p.ParameterType).All(pt =>
                    isGeneric
                        ? pt.IsPointer || pt.IsByRef
                        || (pt.IsGenericType && pt.GetGenericTypeDefinition() == typeof(Vector128<>))
                        : pt == parType),
            };
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
        { "load8_splat", ("Create", 1, false) },
        { "load16_splat", ("Create", 1, false) },
        { "load32_splat", ("Create", 1, false) },
        { "load64_splat", ("Create", 1, false) },
        { "load8_lane", ("WithElement", 3, true) },
        { "load16_lane", ("WithElement", 3, true) },
        { "load32_lane", ("WithElement", 3, true) },
        { "load64_lane", ("WithElement", 3, true) },
        { "store", ("Store", 2, true) },
        { "store8_lane", ("GetElement", 2, true) },
        { "store16_lane", ("GetElement", 2, true) },
        { "store32_lane", ("GetElement", 2, true) },
        { "store64_lane", ("GetElement", 2, true) },
        { "const", ("Create", 4, false) },
        { "neg", ("Negate", 1, true) },
        { "eq", ("Equals", 2, true) },
        { "ne", ("Equals", 2, true) },
        { "lt", ("LessThan", 2, true) },
        { "lt_s", ("LessThan", 2, true) },
        { "lt_u", ("LessThan", 2, true) },
        { "gt", ("GreaterThan", 2, true) },
        { "gt_s", ("GreaterThan", 2, true) },
        { "gt_u", ("GreaterThan", 2, true) },
        { "le", ("LessThanOrEqual", 2, true) },
        { "le_s", ("LessThanOrEqual", 2, true) },
        { "le_u", ("LessThanOrEqual", 2, true) },
        { "ge", ("GreaterThanOrEqual", 2, true) },
        { "ge_s", ("GreaterThanOrEqual", 2, true) },
        { "ge_u", ("GreaterThanOrEqual", 2, true) },
        { "add", ("Add", 2, true) },
        { "sub", ("Subtract", 2, true) },
        { "mul", ("Multiply", 2, true) },
        { "sqrt", ("Sqrt", 1, true) },
        { "abs", ("Abs", 1, true) },
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
        { "load32_zero", ("CreateScalar", 1, false) },
        { "load64_zero", ("CreateScalar", 1, false) },
        { "extract_lane", ("GetElement", 2, true) },
        { "extract_lane_s", ("GetElement", 2, true) },
        { "extract_lane_u", ("GetElement", 2, true) },
        { "replace_lane", ("WithElement", 3, true) },
        { "splat", ("Create", 1, false) },
        { "swizzle", ("Shuffle", 2, true) },
        { "min", ("Min", 2, true) },
        { "max", ("Max", 2, true) },
        { "convert_i32x4_s", ("ConvertToSingle", 1, true) },
        { "convert_i32x4_u", ("ConvertToSingle", 1, true) },
        { "ceil", ("Ceiling", 1, false) },
        { "floor", ("Floor", 1, false) },
        { "trunc", ("Truncate", 1, false) },
        { "nearest", ("Round", 1, false) },
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
        switch (opName)
        {
            case "load32_zero":
                return typeof(uint);
            case "load64_zero":
                return typeof(ulong);
            case "load8_lane":
            case "load8_splat":
                return typeof(byte);
            case "load16_lane":
            case "load16_splat":
                return typeof(ushort);
            case "load32_lane":
            case "load32_splat":
                return typeof(uint);
            case "load64_lane":
            case "load64_splat":
                return typeof(ulong);
            case "store":
                return typeof(byte);
            case "store8_lane":
                return typeof(byte);
            case "store16_lane":
                return typeof(ushort);
            case "store32_lane":
                return typeof(uint);
            case "store64_lane":
                return typeof(ulong);
            case "extract_lane_s":
                return laneType switch
                {
                    "i8x16" => typeof(sbyte),
                    "i16x8" => typeof(short),
                    _ => null,
                };
            case "extract_lane_u":
                return laneType switch
                {
                    "i8x16" => typeof(byte),
                    "i16x8" => typeof(ushort),
                    _ => null,
                };
            case "convert_i32x4_s":
                return typeof(int);
            case "convert_i32x4_u":
                return typeof(uint);
        }
        return !opName.EndsWith("_s", StringComparison.InvariantCulture)
            ? null
            : methodName switch
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
                "Ceiling" or "Floor" or "Truncate" or "Round" => laneType switch
                {
                    "f32x4" => typeof(float),
                    "f64x2" => typeof(double),
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
