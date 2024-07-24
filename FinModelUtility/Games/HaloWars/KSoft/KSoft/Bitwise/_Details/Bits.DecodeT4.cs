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
		#region BitDecode 32
		/// <summary>Bit decode an enumeration or flags from an unsigned integer</summary>
		/// <param name="bits">Unsigned integer to decode from</param>
		/// <param name="bitIndex">Index in <paramref name="bits"/> to start decoding at</param>
		/// <param name="bitMask">Masking value for the enumeration\flags type</param>
		/// <returns>The enumeration\flags value as it stood before it was ever encoded into <paramref name="bits"/></returns>
		[Contracts.Pure]
		public static uint BitDecode(uint bits, int bitIndex, uint bitMask)
		{
			Contract.Requires/*<ArgumentOutOfRangeException>*/(bitIndex >= 0);
			Contract.Requires/*<ArgumentOutOfRangeException>*/(bitIndex < kInt32BitCount);
			Contract.Requires/*<ArgumentException>*/(bitMask != 0);

			return (bits >> bitIndex) & bitMask;
		}
		/// <summary>Bit decode a none-able value from an unsigned integer</summary>
		/// <param name="bits">Unsigned integer to decode from</param>
		/// <param name="bitIndex">Index in <paramref name="bits"/> to start decoding at</param>
		/// <param name="bitMask">Masking value for the enumeration\flags type</param>
		/// <returns>The enumeration\flags value as it stood before it was ever encoded into <paramref name="bits"/></returns>
		[Contracts.Pure]
		public static int BitDecodeNoneable(uint bits, int bitIndex, uint bitMask)
		{
			Contract.Requires/*<ArgumentOutOfRangeException>*/(bitIndex >= 0);
			Contract.Requires/*<ArgumentOutOfRangeException>*/(bitIndex < kInt32BitCount);
			Contract.Requires/*<ArgumentException>*/(bitMask != 0);

			return (int)BitDecode(bits, bitIndex, bitMask) - 1;
		}
		/// <summary>Bit decode an enumeration or flags from an unsigned integer</summary>
		/// <param name="bits">Unsigned integer to decode from</param>
		/// <param name="traits"></param>
		/// <returns>The enumeration\flags value as it stood before it was ever encoded into <paramref name="bits"/></returns>
		[Contracts.Pure]
		public static uint BitDecode(uint bits, Bitwise.BitFieldTraits traits)
		{
			Contract.Requires/*<ArgumentException>*/(!traits.IsEmpty);

			return (bits >> traits.BitIndex) & traits.Bitmask32;
		}
		/// <summary>Bit decode a none-able value from an unsigned integer</summary>
		/// <param name="bits">Unsigned integer to decode from</param>
		/// <param name="traits"></param>
		/// <returns>The enumeration\flags value as it stood before it was ever encoded into <paramref name="bits"/></returns>
		[Contracts.Pure]
		public static int BitDecodeNoneable(uint bits, Bitwise.BitFieldTraits traits)
		{
			Contract.Requires/*<ArgumentException>*/(!traits.IsEmpty);

			return (int)BitDecode(bits, traits.BitIndex, traits.Bitmask32) - 1;
		}

		/// <summary>Bit decode an enumeration or flags from an unsigned integer</summary>
		/// <param name="bits">Unsigned integer to decode from</param>
		/// <param name="bitIndex">Index in <paramref name="bits"/> to start decoding at</param>
		/// <param name="bitMask">Masking value for the enumeration\flags type</param>
		/// <returns>The enumeration\flags value as it stood before it was ever encoded into <paramref name="bits"/></returns>
		/// <remarks>On return <paramref name="bitIndex"/> is incremented by the bit count (determined from <paramref name="bitMask"/>)</remarks>
		[Contracts.Pure]
		public static uint BitDecode(uint bits, ref int bitIndex, uint bitMask)
		{
			Contract.Requires/*<ArgumentOutOfRangeException>*/(bitIndex >= 0);
			Contract.Requires/*<ArgumentOutOfRangeException>*/(bitIndex < kInt32BitCount);
			Contract.Requires/*<ArgumentException>*/(bitMask != 0);

			int bit_count = BitCount(bitMask);
			Contract.Assert((bitIndex + bit_count) <= Bits.kInt32BitCount);

			var value = (bits >> bitIndex) & bitMask;
			bitIndex += bit_count;

			return value;
		}
		/// <summary>Bit decode a none-able value from an unsigned integer</summary>
		/// <param name="bits">Unsigned integer to decode from</param>
		/// <param name="bitIndex">Index in <paramref name="bits"/> to start decoding at</param>
		/// <param name="bitMask">Masking value for the enumeration\flags type</param>
		/// <returns>The enumeration\flags value as it stood before it was ever encoded into <paramref name="bits"/></returns>
		/// <remarks>On return <paramref name="bitIndex"/> is incremented by the bit count (determined from <paramref name="bitMask"/>)</remarks>
		[Contracts.Pure]
		public static int BitDecodeNoneable(uint bits, ref int bitIndex, uint bitMask)
		{
			Contract.Requires/*<ArgumentOutOfRangeException>*/(bitIndex >= 0);
			Contract.Requires/*<ArgumentOutOfRangeException>*/(bitIndex < kInt32BitCount);
			Contract.Requires/*<ArgumentException>*/(bitMask != 0);

			return (int)BitDecode(bits, ref bitIndex, bitMask) - 1;
		}
		#endregion
		#region BitDecode 64
		/// <summary>Bit decode an enumeration or flags from an unsigned integer</summary>
		/// <param name="bits">Unsigned integer to decode from</param>
		/// <param name="bitIndex">Index in <paramref name="bits"/> to start decoding at</param>
		/// <param name="bitMask">Masking value for the enumeration\flags type</param>
		/// <returns>The enumeration\flags value as it stood before it was ever encoded into <paramref name="bits"/></returns>
		[Contracts.Pure]
		public static ulong BitDecode(ulong bits, int bitIndex, ulong bitMask)
		{
			Contract.Requires/*<ArgumentOutOfRangeException>*/(bitIndex >= 0);
			Contract.Requires/*<ArgumentOutOfRangeException>*/(bitIndex < kInt64BitCount);
			Contract.Requires/*<ArgumentException>*/(bitMask != 0);

			return (bits >> bitIndex) & bitMask;
		}
		/// <summary>Bit decode a none-able value from an unsigned integer</summary>
		/// <param name="bits">Unsigned integer to decode from</param>
		/// <param name="bitIndex">Index in <paramref name="bits"/> to start decoding at</param>
		/// <param name="bitMask">Masking value for the enumeration\flags type</param>
		/// <returns>The enumeration\flags value as it stood before it was ever encoded into <paramref name="bits"/></returns>
		[Contracts.Pure]
		public static long BitDecodeNoneable(ulong bits, int bitIndex, ulong bitMask)
		{
			Contract.Requires/*<ArgumentOutOfRangeException>*/(bitIndex >= 0);
			Contract.Requires/*<ArgumentOutOfRangeException>*/(bitIndex < kInt64BitCount);
			Contract.Requires/*<ArgumentException>*/(bitMask != 0);

			return (long)BitDecode(bits, bitIndex, bitMask) - 1;
		}
		/// <summary>Bit decode an enumeration or flags from an unsigned integer</summary>
		/// <param name="bits">Unsigned integer to decode from</param>
		/// <param name="traits"></param>
		/// <returns>The enumeration\flags value as it stood before it was ever encoded into <paramref name="bits"/></returns>
		[Contracts.Pure]
		public static ulong BitDecode(ulong bits, Bitwise.BitFieldTraits traits)
		{
			Contract.Requires/*<ArgumentException>*/(!traits.IsEmpty);

			return (bits >> traits.BitIndex) & traits.Bitmask64;
		}
		/// <summary>Bit decode a none-able value from an unsigned integer</summary>
		/// <param name="bits">Unsigned integer to decode from</param>
		/// <param name="traits"></param>
		/// <returns>The enumeration\flags value as it stood before it was ever encoded into <paramref name="bits"/></returns>
		[Contracts.Pure]
		public static long BitDecodeNoneable(ulong bits, Bitwise.BitFieldTraits traits)
		{
			Contract.Requires/*<ArgumentException>*/(!traits.IsEmpty);

			return (long)BitDecode(bits, traits.BitIndex, traits.Bitmask64) - 1;
		}

		/// <summary>Bit decode an enumeration or flags from an unsigned integer</summary>
		/// <param name="bits">Unsigned integer to decode from</param>
		/// <param name="bitIndex">Index in <paramref name="bits"/> to start decoding at</param>
		/// <param name="bitMask">Masking value for the enumeration\flags type</param>
		/// <returns>The enumeration\flags value as it stood before it was ever encoded into <paramref name="bits"/></returns>
		/// <remarks>On return <paramref name="bitIndex"/> is incremented by the bit count (determined from <paramref name="bitMask"/>)</remarks>
		[Contracts.Pure]
		public static ulong BitDecode(ulong bits, ref int bitIndex, ulong bitMask)
		{
			Contract.Requires/*<ArgumentOutOfRangeException>*/(bitIndex >= 0);
			Contract.Requires/*<ArgumentOutOfRangeException>*/(bitIndex < kInt64BitCount);
			Contract.Requires/*<ArgumentException>*/(bitMask != 0);

			int bit_count = BitCount(bitMask);
			Contract.Assert((bitIndex + bit_count) <= Bits.kInt64BitCount);

			var value = (bits >> bitIndex) & bitMask;
			bitIndex += bit_count;

			return value;
		}
		/// <summary>Bit decode a none-able value from an unsigned integer</summary>
		/// <param name="bits">Unsigned integer to decode from</param>
		/// <param name="bitIndex">Index in <paramref name="bits"/> to start decoding at</param>
		/// <param name="bitMask">Masking value for the enumeration\flags type</param>
		/// <returns>The enumeration\flags value as it stood before it was ever encoded into <paramref name="bits"/></returns>
		/// <remarks>On return <paramref name="bitIndex"/> is incremented by the bit count (determined from <paramref name="bitMask"/>)</remarks>
		[Contracts.Pure]
		public static long BitDecodeNoneable(ulong bits, ref int bitIndex, ulong bitMask)
		{
			Contract.Requires/*<ArgumentOutOfRangeException>*/(bitIndex >= 0);
			Contract.Requires/*<ArgumentOutOfRangeException>*/(bitIndex < kInt64BitCount);
			Contract.Requires/*<ArgumentException>*/(bitMask != 0);

			return (long)BitDecode(bits, ref bitIndex, bitMask) - 1;
		}
		#endregion

		#region BitFieldExtract
		/// <summary>Extract a range of bits from an unsigned integer</summary>
		/// <param name="bits">Unsigned integer to extract from</param>
		/// <param name="bitIndexLow">Index in <paramref name="bits"/> to start extracting from</param>
		/// <param name="bitIndexHigh">Index in <paramref name="bits"/> to stop extracting at</param>
		/// <returns>Returns bits <paramref name="bitIndexLow"/> to <paramref name="bitIndexHigh"/></returns>
		[Contracts.Pure]
		public static uint BitFieldExtractRange(uint bits, int bitIndexLow, int bitIndexHigh)
		{
			Contract.Requires/*<ArgumentOutOfRangeException>*/(bitIndexLow >= 0 && bitIndexHigh >= 0);
			Contract.Requires/*<ArgumentOutOfRangeException>*/(bitIndexLow < kInt32BitCount && bitIndexHigh < kInt32BitCount);

			var shifted = bits >> bitIndexLow;								// Shift the bit field to start at the 0th bit
			var mask = BitCountToMask32((bitIndexHigh-bitIndexLow) + 1);	// Generate a mask of the bit range

			return shifted & mask;
		}
		/// <summary>Extract a value represented in a bit-field</summary>
		/// <param name="bits">Unsigned integer to extract from</param>
		/// <param name="bitIndex">Index in <paramref name="bits"/> to start extracting from</param>
		/// <param name="bitCount">Number of bits representing the value to extract</param>
		/// <returns></returns>
		[Contracts.Pure]
		public static uint BitFieldExtractValue(uint bits, int bitIndex, int bitCount)
		{
			Contract.Requires/*<ArgumentOutOfRangeException>*/(bitIndex >= 0 && bitCount >= 0);
			Contract.Requires/*<ArgumentOutOfRangeException>*/(bitIndex < kInt32BitCount);
			Contract.Requires/*<ArgumentOutOfRangeException>*/(bitIndex + (bitCount-1) < kInt32BitCount);

			return BitFieldExtractRange(bits, bitIndex, bitIndex + (bitCount-1));
		}

		/// <summary>Extract a range of bits from an unsigned integer</summary>
		/// <param name="bits">Unsigned integer to extract from</param>
		/// <param name="bitIndexLow">Index in <paramref name="bits"/> to start extracting from</param>
		/// <param name="bitIndexHigh">Index in <paramref name="bits"/> to stop extracting at</param>
		/// <returns>Returns bits <paramref name="bitIndexLow"/> to <paramref name="bitIndexHigh"/></returns>
		[Contracts.Pure]
		public static ulong BitFieldExtractRange(ulong bits, int bitIndexLow, int bitIndexHigh)
		{
			Contract.Requires/*<ArgumentOutOfRangeException>*/(bitIndexLow >= 0 && bitIndexHigh >= 0);
			Contract.Requires/*<ArgumentOutOfRangeException>*/(bitIndexLow < kInt64BitCount && bitIndexHigh < kInt64BitCount);

			var shifted = bits >> bitIndexLow;								// Shift the bit field to start at the 0th bit
			var mask = BitCountToMask64((bitIndexHigh-bitIndexLow) + 1);	// Generate a mask of the bit range

			return shifted & mask;
		}
		/// <summary>Extract a value represented in a bit-field</summary>
		/// <param name="bits">Unsigned integer to extract from</param>
		/// <param name="bitIndex">Index in <paramref name="bits"/> to start extracting from</param>
		/// <param name="bitCount">Number of bits representing the value to extract</param>
		/// <returns></returns>
		[Contracts.Pure]
		public static ulong BitFieldExtractValue(ulong bits, int bitIndex, int bitCount)
		{
			Contract.Requires/*<ArgumentOutOfRangeException>*/(bitIndex >= 0 && bitCount >= 0);
			Contract.Requires/*<ArgumentOutOfRangeException>*/(bitIndex < kInt64BitCount);
			Contract.Requires/*<ArgumentOutOfRangeException>*/(bitIndex + (bitCount-1) < kInt64BitCount);

			return BitFieldExtractRange(bits, bitIndex, bitIndex + (bitCount-1));
		}

		#endregion
	};
}
