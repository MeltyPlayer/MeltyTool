using System;
using Contracts = System.Diagnostics.Contracts;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft
{
	partial class Bits
	{
		#region BitmaskLookUpTable
		/// <summary>Generate an 8-bit bit count to bitmask table</summary>
		/// <param name="wordBitSize">Number of bits to generate a table for</param>
		/// <param name="lut">Bitmask look up table</param>
		/// <remarks>Treat <paramref name="lut"/> as <b>read-only</b></remarks>
		public static void BitmaskLookUpTableGenerate(int wordBitSize, out byte[] lut)
		{
			Contract.Requires/*<ArgumentOutOfRangeException>*/(wordBitSize > 0 && wordBitSize <= kByteBitCount);
			Contract.Ensures(Contract.ValueAtReturn(out lut) != null);

			if (wordBitSize == kByteBitCount && kBitmaskLookup8 != null)
			{
				lut = kBitmaskLookup8;
			}
			else
			{
				lut = new byte[BitmaskLookUpTableGetLength(wordBitSize)];
				for (int x = 1, shift = lut.Length-2; x < lut.Length; x++, shift--)
					lut[x] = (byte)(byte.MaxValue >> shift);
			}
		}
		/// <summary>Generate an 8-bit bit count to bitmask table</summary>
		/// <param name="wordBitSize">Number of bits to generate a table for</param>
		/// <param name="lut">Bitmask look up table</param>
		/// <remarks>Treat <paramref name="lut"/> as <b>read-only</b></remarks>
		public static byte[] BitmaskByteLookUpTableGenerate(int wordBitSize)
		{
			Contract.Requires/*<ArgumentOutOfRangeException>*/(wordBitSize > 0 && wordBitSize <= kByteBitCount);

			BitmaskLookUpTableGenerate(wordBitSize, out byte[] lut);

			return lut;
		}

		/// <summary>Generate an 16-bit bit count to bitmask table</summary>
		/// <param name="wordBitSize">Number of bits to generate a table for</param>
		/// <param name="lut">Bitmask look up table</param>
		/// <remarks>Treat <paramref name="lut"/> as <b>read-only</b></remarks>
		public static void BitmaskLookUpTableGenerate(int wordBitSize, out ushort[] lut)
		{
			Contract.Requires/*<ArgumentOutOfRangeException>*/(wordBitSize > 0 && wordBitSize <= kUInt16BitCount);
			Contract.Ensures(Contract.ValueAtReturn(out lut) != null);

			if (wordBitSize == kUInt16BitCount && kBitmaskLookup16 != null)
			{
				lut = kBitmaskLookup16;
			}
			else
			{
				lut = new ushort[BitmaskLookUpTableGetLength(wordBitSize)];
				for (int x = 1, shift = lut.Length-2; x < lut.Length; x++, shift--)
					lut[x] = (ushort)(ushort.MaxValue >> shift);
			}
		}
		/// <summary>Generate an 16-bit bit count to bitmask table</summary>
		/// <param name="wordBitSize">Number of bits to generate a table for</param>
		/// <param name="lut">Bitmask look up table</param>
		/// <remarks>Treat <paramref name="lut"/> as <b>read-only</b></remarks>
		public static ushort[] BitmaskUInt16LookUpTableGenerate(int wordBitSize)
		{
			Contract.Requires/*<ArgumentOutOfRangeException>*/(wordBitSize > 0 && wordBitSize <= kUInt16BitCount);

			BitmaskLookUpTableGenerate(wordBitSize, out ushort[] lut);

			return lut;
		}

		/// <summary>Generate an 32-bit bit count to bitmask table</summary>
		/// <param name="wordBitSize">Number of bits to generate a table for</param>
		/// <param name="lut">Bitmask look up table</param>
		/// <remarks>Treat <paramref name="lut"/> as <b>read-only</b></remarks>
		public static void BitmaskLookUpTableGenerate(int wordBitSize, out uint[] lut)
		{
			Contract.Requires/*<ArgumentOutOfRangeException>*/(wordBitSize > 0 && wordBitSize <= kUInt32BitCount);
			Contract.Ensures(Contract.ValueAtReturn(out lut) != null);

			if (wordBitSize == kUInt32BitCount && kBitmaskLookup32 != null)
			{
				lut = kBitmaskLookup32;
			}
			else
			{
				lut = new uint[BitmaskLookUpTableGetLength(wordBitSize)];
				for (int x = 1, shift = lut.Length-2; x < lut.Length; x++, shift--)
					lut[x] = (uint)(uint.MaxValue >> shift);
			}
		}
		/// <summary>Generate an 32-bit bit count to bitmask table</summary>
		/// <param name="wordBitSize">Number of bits to generate a table for</param>
		/// <param name="lut">Bitmask look up table</param>
		/// <remarks>Treat <paramref name="lut"/> as <b>read-only</b></remarks>
		public static uint[] BitmaskUInt32LookUpTableGenerate(int wordBitSize)
		{
			Contract.Requires/*<ArgumentOutOfRangeException>*/(wordBitSize > 0 && wordBitSize <= kUInt32BitCount);

			BitmaskLookUpTableGenerate(wordBitSize, out uint[] lut);

			return lut;
		}

		/// <summary>Generate an 64-bit bit count to bitmask table</summary>
		/// <param name="wordBitSize">Number of bits to generate a table for</param>
		/// <param name="lut">Bitmask look up table</param>
		/// <remarks>Treat <paramref name="lut"/> as <b>read-only</b></remarks>
		public static void BitmaskLookUpTableGenerate(int wordBitSize, out ulong[] lut)
		{
			Contract.Requires/*<ArgumentOutOfRangeException>*/(wordBitSize > 0 && wordBitSize <= kUInt64BitCount);
			Contract.Ensures(Contract.ValueAtReturn(out lut) != null);

			if (wordBitSize == kUInt64BitCount && kBitmaskLookup64 != null)
			{
				lut = kBitmaskLookup64;
			}
			else
			{
				lut = new ulong[BitmaskLookUpTableGetLength(wordBitSize)];
				for (int x = 1, shift = lut.Length-2; x < lut.Length; x++, shift--)
					lut[x] = (ulong)(ulong.MaxValue >> shift);
			}
		}
		/// <summary>Generate an 64-bit bit count to bitmask table</summary>
		/// <param name="wordBitSize">Number of bits to generate a table for</param>
		/// <param name="lut">Bitmask look up table</param>
		/// <remarks>Treat <paramref name="lut"/> as <b>read-only</b></remarks>
		public static ulong[] BitmaskUInt64LookUpTableGenerate(int wordBitSize)
		{
			Contract.Requires/*<ArgumentOutOfRangeException>*/(wordBitSize > 0 && wordBitSize <= kUInt64BitCount);

			BitmaskLookUpTableGenerate(wordBitSize, out ulong[] lut);

			return lut;
		}

		#endregion

		#region ArrayCopy
		#region UInt16
		public static void ArrayCopy(byte[] src, int srcOffset, ushort[] dst, int dstOffset, int count)
		{
			Contract.Requires<ArgumentNullException>(src != null);
			Contract.Requires<ArgumentOutOfRangeException>(srcOffset >= 0);
			Contract.Requires<ArgumentNullException>(dst != null);
			Contract.Requires<ArgumentOutOfRangeException>(dstOffset >= 0);

			Contract.Requires<ArgumentOutOfRangeException>(ArrayCopyFromBytesBoundsValidate(
				src, srcOffset, dst, dstOffset, count, sizeof(ushort)));

			var memcpy = new MemoryCopier<ushort, byte>(dummy: false);
			memcpy.CopyInternal(dst, dstOffset, src, srcOffset, count);
		}
		public static void ArrayCopy(ushort[] src, int srcOffset, byte[] dst, int dstOffset, int count)
		{
			Contract.Requires<ArgumentNullException>(src != null);
			Contract.Requires<ArgumentOutOfRangeException>(srcOffset >= 0);
			Contract.Requires<ArgumentNullException>(dst != null);
			Contract.Requires<ArgumentOutOfRangeException>(dstOffset >= 0);

			Contract.Requires<ArgumentOutOfRangeException>(ArrayCopyToBytesBoundsValidate(
				src, srcOffset, dst, dstOffset, count, sizeof(ushort)));

			var memcpy = new MemoryCopier<byte, ushort>(dummy: false);
			memcpy.CopyInternal(dst, dstOffset, src, srcOffset, count);
		}
		#endregion

		#region UInt32
		public static void ArrayCopy(byte[] src, int srcOffset, uint[] dst, int dstOffset, int count)
		{
			Contract.Requires<ArgumentNullException>(src != null);
			Contract.Requires<ArgumentOutOfRangeException>(srcOffset >= 0);
			Contract.Requires<ArgumentNullException>(dst != null);
			Contract.Requires<ArgumentOutOfRangeException>(dstOffset >= 0);

			Contract.Requires<ArgumentOutOfRangeException>(ArrayCopyFromBytesBoundsValidate(
				src, srcOffset, dst, dstOffset, count, sizeof(uint)));

			var memcpy = new MemoryCopier<uint, byte>(dummy: false);
			memcpy.CopyInternal(dst, dstOffset, src, srcOffset, count);
		}
		public static void ArrayCopy(uint[] src, int srcOffset, byte[] dst, int dstOffset, int count)
		{
			Contract.Requires<ArgumentNullException>(src != null);
			Contract.Requires<ArgumentOutOfRangeException>(srcOffset >= 0);
			Contract.Requires<ArgumentNullException>(dst != null);
			Contract.Requires<ArgumentOutOfRangeException>(dstOffset >= 0);

			Contract.Requires<ArgumentOutOfRangeException>(ArrayCopyToBytesBoundsValidate(
				src, srcOffset, dst, dstOffset, count, sizeof(uint)));

			var memcpy = new MemoryCopier<byte, uint>(dummy: false);
			memcpy.CopyInternal(dst, dstOffset, src, srcOffset, count);
		}
		#endregion

		#region UInt64
		public static void ArrayCopy(byte[] src, int srcOffset, ulong[] dst, int dstOffset, int count)
		{
			Contract.Requires<ArgumentNullException>(src != null);
			Contract.Requires<ArgumentOutOfRangeException>(srcOffset >= 0);
			Contract.Requires<ArgumentNullException>(dst != null);
			Contract.Requires<ArgumentOutOfRangeException>(dstOffset >= 0);

			Contract.Requires<ArgumentOutOfRangeException>(ArrayCopyFromBytesBoundsValidate(
				src, srcOffset, dst, dstOffset, count, sizeof(ulong)));

			var memcpy = new MemoryCopier<ulong, byte>(dummy: false);
			memcpy.CopyInternal(dst, dstOffset, src, srcOffset, count);
		}
		public static void ArrayCopy(ulong[] src, int srcOffset, byte[] dst, int dstOffset, int count)
		{
			Contract.Requires<ArgumentNullException>(src != null);
			Contract.Requires<ArgumentOutOfRangeException>(srcOffset >= 0);
			Contract.Requires<ArgumentNullException>(dst != null);
			Contract.Requires<ArgumentOutOfRangeException>(dstOffset >= 0);

			Contract.Requires<ArgumentOutOfRangeException>(ArrayCopyToBytesBoundsValidate(
				src, srcOffset, dst, dstOffset, count, sizeof(ulong)));

			var memcpy = new MemoryCopier<byte, ulong>(dummy: false);
			memcpy.CopyInternal(dst, dstOffset, src, srcOffset, count);
		}
		#endregion

		#endregion

		#region BitReverse (by-ref)
		/// <summary>Get the bit-reversed equivalent of an unsigned integer</summary>
		/// <param name="x">Integer to bit-reverse</param>
		public static void BitReverse(ref byte x)
		{
			x = BitReverse(x);
		}

		/// <summary>Get the bit-reversed equivalent of an unsigned integer</summary>
		/// <param name="x">Integer to bit-reverse</param>
		public static void BitReverse(ref ushort x)
		{
			x = BitReverse(x);
		}

		/// <summary>Get the bit-reversed equivalent of an unsigned integer</summary>
		/// <param name="x">Integer to bit-reverse</param>
		public static void BitReverse(ref uint x)
		{
			x = BitReverse(x);
		}

		/// <summary>Get the bit-reversed equivalent of an unsigned integer</summary>
		/// <param name="x">Integer to bit-reverse</param>
		public static void BitReverse(ref ulong x)
		{
			x = BitReverse(x);
		}

		#endregion

		#region GetMaxEnumBits
		/// <summary>Calculate how many bits are needed to represent the provided value</summary>
		/// <param name="maxValue">An enumeration's <b>kMax</b> value</param>
		/// <returns>Number of bits needed to represent (<paramref name="maxValue"/> - 1)</returns>
		/// <remarks>A <b>kMax</b> value should be unused and the last entry of an Enumeration. This is why 1 is subtracted from <paramref name="maxValue"/>.</remarks>
		[Contracts.Pure]
		public static int GetMaxEnumBits(uint maxValue)
		{
			Contract.Requires/*<ArgumentOutOfRangeException>*/(maxValue > 1, kGetMaxEnumBits_MaxValueOutOfRangeMessage);
			Contract.Ensures(Contract.Result<int>() > 0);

			return IndexOfHighestBitSet(maxValue - 1) + 1;
		}
		/// <summary>Calculate how many bits are needed to represent the provided value</summary>
		/// <param name="maxValue">An enumeration's <b>kMax</b> value</param>
		/// <returns>Number of bits needed to represent (<paramref name="maxValue"/> - 1)</returns>
		/// <remarks>A <b>kMax</b> value should be unused and the last entry of an Enumeration. This is why 1 is subtracted from <paramref name="maxValue"/>.</remarks>
		[Contracts.Pure]
		public static int GetMaxEnumBits(int maxValue)
		{
			Contract.Requires/*<ArgumentOutOfRangeException>*/(maxValue > 1, kGetMaxEnumBits_MaxValueOutOfRangeMessage);
			Contract.Ensures(Contract.Result<int>() > 0);

			return IndexOfHighestBitSet((uint)maxValue - 1) + 1;
		}

		/// <summary>Calculate how many bits are needed to represent the provided value</summary>
		/// <param name="maxValue">An enumeration's <b>kMax</b> value</param>
		/// <returns>Number of bits needed to represent (<paramref name="maxValue"/> - 1)</returns>
		/// <remarks>A <b>kMax</b> value should be unused and the last entry of an Enumeration. This is why 1 is subtracted from <paramref name="maxValue"/>.</remarks>
		[Contracts.Pure]
		public static int GetMaxEnumBits(ulong maxValue)
		{
			Contract.Requires/*<ArgumentOutOfRangeException>*/(maxValue > 1, kGetMaxEnumBits_MaxValueOutOfRangeMessage);
			Contract.Ensures(Contract.Result<int>() > 0);

			return IndexOfHighestBitSet(maxValue - 1) + 1;
		}
		/// <summary>Calculate how many bits are needed to represent the provided value</summary>
		/// <param name="maxValue">An enumeration's <b>kMax</b> value</param>
		/// <returns>Number of bits needed to represent (<paramref name="maxValue"/> - 1)</returns>
		/// <remarks>A <b>kMax</b> value should be unused and the last entry of an Enumeration. This is why 1 is subtracted from <paramref name="maxValue"/>.</remarks>
		[Contracts.Pure]
		public static int GetMaxEnumBits(long maxValue)
		{
			Contract.Requires/*<ArgumentOutOfRangeException>*/(maxValue > 1, kGetMaxEnumBits_MaxValueOutOfRangeMessage);
			Contract.Ensures(Contract.Result<int>() > 0);

			return IndexOfHighestBitSet((ulong)maxValue - 1) + 1;
		}

		#endregion

		#region GetBitmask
		/// <summary>Calculate the masking value for an enumeration</summary>
		/// <param name="maxValue">An enumeration's <b>kMax</b> value</param>
		/// <returns>The smallest bit mask value for (<paramref name="maxValue"/> - 1)</returns>
		/// <remarks>A <b>kMax</b> value should be unused and the last entry of an Enumeration. This is why 1 is subtracted from <paramref name="maxValue"/>.</remarks>
		[Contracts.Pure]
		public static uint GetBitmaskEnum(uint maxValue)
		{
			Contract.Requires/*<ArgumentOutOfRangeException>*/(maxValue > 1, kGetBitmaskEnum_MaxValueOutOfRangeMessage);
			Contract.Ensures(Contract.Result<uint>() > 0);

			int bit_count = GetMaxEnumBits(maxValue);

			return BitCountToMask32(bit_count);
		}
		/// <summary>Calculate the masking value for a series of flags</summary>
		/// <param name="maxValue">A bit enumeration's <b>kMax</b> value. IE, the 'highest bit' plus one</param>
		/// <returns>The smallest bit mask value for (<paramref name="maxValue"/> - 1)</returns>
		/// <remarks>A <b>kMax</b> value should be unused and the last entry of an Enumeration. This is why 1 is subtracted from <paramref name="maxValue"/>.</remarks>
		[Contracts.Pure]
		public static uint GetBitmaskFlags(uint maxValue)
		{
			Contract.Requires/*<ArgumentOutOfRangeException>*/(maxValue > 0, kGetBitmaskFlag_MaxValueOutOfRangeMessage);
			Contract.Requires/*<ArgumentOutOfRangeException>*/(maxValue <= kUInt32BitCount);
			Contract.Ensures(Contract.Result<uint>() > 0);

			return BitCountToMask32((int)--maxValue);
		}

		/// <summary>Calculate the masking value for an enumeration</summary>
		/// <param name="maxValue">An enumeration's <b>kMax</b> value</param>
		/// <returns>The smallest bit mask value for (<paramref name="maxValue"/> - 1)</returns>
		/// <remarks>A <b>kMax</b> value should be unused and the last entry of an Enumeration. This is why 1 is subtracted from <paramref name="maxValue"/>.</remarks>
		[Contracts.Pure]
		public static ulong GetBitmaskEnum(ulong maxValue)
		{
			Contract.Requires/*<ArgumentOutOfRangeException>*/(maxValue > 1, kGetBitmaskEnum_MaxValueOutOfRangeMessage);
			Contract.Ensures(Contract.Result<ulong>() > 0);

			int bit_count = GetMaxEnumBits(maxValue);

			return BitCountToMask64(bit_count);
		}
		/// <summary>Calculate the masking value for a series of flags</summary>
		/// <param name="maxValue">A bit enumeration's <b>kMax</b> value. IE, the 'highest bit' plus one</param>
		/// <returns>The smallest bit mask value for (<paramref name="maxValue"/> - 1)</returns>
		/// <remarks>A <b>kMax</b> value should be unused and the last entry of an Enumeration. This is why 1 is subtracted from <paramref name="maxValue"/>.</remarks>
		[Contracts.Pure]
		public static ulong GetBitmaskFlags(ulong maxValue)
		{
			Contract.Requires/*<ArgumentOutOfRangeException>*/(maxValue > 0, kGetBitmaskFlag_MaxValueOutOfRangeMessage);
			Contract.Requires/*<ArgumentOutOfRangeException>*/(maxValue <= kUInt64BitCount);
			Contract.Ensures(Contract.Result<ulong>() > 0);

			return BitCountToMask64((int)--maxValue);
		}

		#endregion

		#region SignExtend
		// http://graphics.stanford.edu/~seander/bithacks.html#VariableSignExtend

		[Contracts.Pure]
		public static int SignExtend(int value, int bitCount)
		{
			Contract.Requires/*<ArgumentOutOfRangeException>*/(bitCount > 0 && bitCount <= kUInt32BitCount);
			const uint k_one = 1;

			var bit_mask = (int)BitCountToMask32(bitCount);
			var ext_mask = (int)(k_one << (bitCount - 1));

			// clear the bits outside of our bit count range
			value &= bit_mask;

			// if the clear operation above isn't needed, we could do the following instead:
			// (value << ext_mask) >> ext_mask
			return (value ^ ext_mask) - ext_mask;
		}
		[Contracts.Pure]
		public static int SignExtendWithoutClear(int value, int bitCount)
		{
			Contract.Requires/*<ArgumentOutOfRangeException>*/(bitCount > 0 && bitCount <= kUInt32BitCount);

			int ext_shift = kUInt32BitCount - bitCount;

			return (value << ext_shift) >> ext_shift;
		}

		[Contracts.Pure]
		public static long SignExtend(long value, int bitCount)
		{
			Contract.Requires/*<ArgumentOutOfRangeException>*/(bitCount > 0 && bitCount <= kUInt64BitCount);
			const ulong k_one = 1;

			var bit_mask = (long)BitCountToMask64(bitCount);
			var ext_mask = (long)(k_one << (bitCount - 1));

			// clear the bits outside of our bit count range
			value &= bit_mask;

			// if the clear operation above isn't needed, we could do the following instead:
			// (value << ext_mask) >> ext_mask
			return (value ^ ext_mask) - ext_mask;
		}
		[Contracts.Pure]
		public static long SignExtendWithoutClear(long value, int bitCount)
		{
			Contract.Requires/*<ArgumentOutOfRangeException>*/(bitCount > 0 && bitCount <= kUInt64BitCount);

			int ext_shift = kUInt64BitCount - bitCount;

			return (value << ext_shift) >> ext_shift;
		}

		#endregion
	};
}
