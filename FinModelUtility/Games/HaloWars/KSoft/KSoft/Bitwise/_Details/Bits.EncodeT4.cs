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
		#region BitEncode (Enum)
		/// <summary>Bit encode an enumeration value into an unsigned integer</summary>
		/// <param name="value">Enumeration value to encode</param>
		/// <param name="bits">Bit data as an unsigned integer</param>
		/// <param name="bitIndex">Index in <paramref name="bits"/> to start encoding at</param>
		/// <param name="bitMask">Masking value for <paramref name="value"/></param>
		/// <returns><paramref name="bits"/> with <paramref name="value"/> encoded into it</returns>
		/// <remarks>
		/// Clears the bit-space between <paramref name="bitIndex"/> + <paramref name="bitMask"/> so
		/// any possibly existing value will be zeroed before <paramref name="value"/> is added
		/// </remarks>
		[Contracts.Pure]
		public static uint BitEncodeEnum(uint value, uint bits, int bitIndex, uint bitMask)
		{
			Contract.Requires/*<ArgumentOutOfRangeException>*/(bitIndex >= 0);
			Contract.Requires/*<ArgumentOutOfRangeException>*/(bitIndex < kInt32BitCount);
			Contract.Requires/*<ArgumentException>*/(bitMask != 0);

			return Bitwise.Flags.Add(bits & ~(bitMask << bitIndex), // clear the bit-space
				(value & bitMask) << bitIndex); // add [value] to the newly cleared bit-space
		}
		/// <summary>Bit encode an enumeration value into an unsigned integer</summary>
		/// <param name="value">Enumeration value to encode</param>
		/// <param name="bits">Bit data as an unsigned integer</param>
		/// <param name="bitIndex">Index in <paramref name="bits"/> to start encoding at</param>
		/// <param name="bitMask">Masking value for <paramref name="value"/></param>
		/// <remarks>
		/// On return <paramref name="bits"/> has <paramref name="value"/> encoded into it and <paramref name="bitIndex"/>
		/// is incremented by the bit count (determined from <paramref name="bitMask"/>)
		/// </remarks>
		public static void BitEncodeEnum(uint value, ref uint bits, ref int bitIndex, uint bitMask)
		{
			Contract.Requires/*<ArgumentOutOfRangeException>*/(bitIndex >= 0);
			Contract.Requires/*<ArgumentOutOfRangeException>*/(bitIndex < kInt32BitCount);
			Contract.Requires/*<ArgumentException>*/(bitMask != 0);

			int bit_count = BitCount(bitMask);
			Contract.Assert((bitIndex + bit_count) <= kInt32BitCount);

			bits = BitEncodeEnum(value, bits, bitIndex, bitMask);
			bitIndex += bit_count;
		}

		/// <summary>Bit encode an enumeration value into an unsigned integer</summary>
		/// <param name="value">Enumeration value to encode</param>
		/// <param name="bits">Bit data as an unsigned integer</param>
		/// <param name="bitIndex">Index in <paramref name="bits"/> to start encoding at</param>
		/// <param name="bitMask">Masking value for <paramref name="value"/></param>
		/// <returns><paramref name="bits"/> with <paramref name="value"/> encoded into it</returns>
		/// <remarks>
		/// Clears the bit-space between <paramref name="bitIndex"/> + <paramref name="bitMask"/> so
		/// any possibly existing value will be zeroed before <paramref name="value"/> is added
		/// </remarks>
		[Contracts.Pure]
		public static ulong BitEncodeEnum(ulong value, ulong bits, int bitIndex, ulong bitMask)
		{
			Contract.Requires/*<ArgumentOutOfRangeException>*/(bitIndex >= 0);
			Contract.Requires/*<ArgumentOutOfRangeException>*/(bitIndex < kInt64BitCount);
			Contract.Requires/*<ArgumentException>*/(bitMask != 0);

			return Bitwise.Flags.Add(bits & ~(bitMask << bitIndex), // clear the bit-space
				(value & bitMask) << bitIndex); // add [value] to the newly cleared bit-space
		}
		/// <summary>Bit encode an enumeration value into an unsigned integer</summary>
		/// <param name="value">Enumeration value to encode</param>
		/// <param name="bits">Bit data as an unsigned integer</param>
		/// <param name="bitIndex">Index in <paramref name="bits"/> to start encoding at</param>
		/// <param name="bitMask">Masking value for <paramref name="value"/></param>
		/// <remarks>
		/// On return <paramref name="bits"/> has <paramref name="value"/> encoded into it and <paramref name="bitIndex"/>
		/// is incremented by the bit count (determined from <paramref name="bitMask"/>)
		/// </remarks>
		public static void BitEncodeEnum(ulong value, ref ulong bits, ref int bitIndex, ulong bitMask)
		{
			Contract.Requires/*<ArgumentOutOfRangeException>*/(bitIndex >= 0);
			Contract.Requires/*<ArgumentOutOfRangeException>*/(bitIndex < kInt64BitCount);
			Contract.Requires/*<ArgumentException>*/(bitMask != 0);

			int bit_count = BitCount(bitMask);
			Contract.Assert((bitIndex + bit_count) <= kInt64BitCount);

			bits = BitEncodeEnum(value, bits, bitIndex, bitMask);
			bitIndex += bit_count;
		}

		#endregion

		#region BitEncode (Flags)
		/// <summary>Bit encode a flags value into an unsigned integer</summary>
		/// <param name="value">Flags to encode</param>
		/// <param name="bits">Bit data as an unsigned integer</param>
		/// <param name="bitIndex">Index in <paramref name="bits"/> to start encoding at</param>
		/// <param name="bitMask">Masking value for <paramref name="value"/></param>
		/// <returns><paramref name="bits"/> with <paramref name="value"/> encoded into it</returns>
		/// <remarks>
		/// Doesn't clear the bit-space between <paramref name="bitIndex"/> + <paramref name="bitMask"/>
		/// so any possibly existing flags will be retained before and after <paramref name="value"/> is added
		/// </remarks>
		[Contracts.Pure]
		public static uint BitEncodeFlags(uint value, uint bits, int bitIndex, uint bitMask)
		{
			Contract.Requires/*<ArgumentOutOfRangeException>*/(bitIndex >= 0);
			Contract.Requires/*<ArgumentOutOfRangeException>*/(bitIndex < kInt32BitCount);
			Contract.Requires/*<ArgumentException>*/(bitMask != 0);

			return Bitwise.Flags.Add(bits,
				(value & bitMask) << bitIndex); // add [value] to the existing bits
		}
		/// <summary>Bit encode a flags value into an unsigned integer</summary>
		/// <param name="value">Flags to encode</param>
		/// <param name="bits">Bit data as an unsigned integer</param>
		/// <param name="bitIndex">Index in <paramref name="bits"/> to start encoding at</param>
		/// <param name="bitMask">Masking value for <paramref name="value"/></param>
		/// <remarks>
		/// On return <paramref name="bits"/> has <paramref name="value"/> encoded into it and <paramref name="bitIndex"/>
		/// is incremented by the bit count (determined from <paramref name="bitMask"/>)
		/// </remarks>
		public static void BitEncodeFlags(uint value, ref uint bits, ref int bitIndex, uint bitMask)
		{
			Contract.Requires/*<ArgumentOutOfRangeException>*/(bitIndex >= 0);
			Contract.Requires/*<ArgumentOutOfRangeException>*/(bitIndex < kInt32BitCount);
			Contract.Requires/*<ArgumentException>*/(bitMask != 0);

			int bit_count = BitCount(bitMask);
			Contract.Assert((bitIndex + bit_count) <= kInt32BitCount);

			bits = BitEncodeFlags(value, bits, bitIndex, bitMask);
			bitIndex += bit_count;
		}

		/// <summary>Bit encode a flags value into an unsigned integer</summary>
		/// <param name="value">Flags to encode</param>
		/// <param name="bits">Bit data as an unsigned integer</param>
		/// <param name="bitIndex">Index in <paramref name="bits"/> to start encoding at</param>
		/// <param name="bitMask">Masking value for <paramref name="value"/></param>
		/// <returns><paramref name="bits"/> with <paramref name="value"/> encoded into it</returns>
		/// <remarks>
		/// Doesn't clear the bit-space between <paramref name="bitIndex"/> + <paramref name="bitMask"/>
		/// so any possibly existing flags will be retained before and after <paramref name="value"/> is added
		/// </remarks>
		[Contracts.Pure]
		public static ulong BitEncodeFlags(ulong value, ulong bits, int bitIndex, ulong bitMask)
		{
			Contract.Requires/*<ArgumentOutOfRangeException>*/(bitIndex >= 0);
			Contract.Requires/*<ArgumentOutOfRangeException>*/(bitIndex < kInt64BitCount);
			Contract.Requires/*<ArgumentException>*/(bitMask != 0);

			return Bitwise.Flags.Add(bits,
				(value & bitMask) << bitIndex); // add [value] to the existing bits
		}
		/// <summary>Bit encode a flags value into an unsigned integer</summary>
		/// <param name="value">Flags to encode</param>
		/// <param name="bits">Bit data as an unsigned integer</param>
		/// <param name="bitIndex">Index in <paramref name="bits"/> to start encoding at</param>
		/// <param name="bitMask">Masking value for <paramref name="value"/></param>
		/// <remarks>
		/// On return <paramref name="bits"/> has <paramref name="value"/> encoded into it and <paramref name="bitIndex"/>
		/// is incremented by the bit count (determined from <paramref name="bitMask"/>)
		/// </remarks>
		public static void BitEncodeFlags(ulong value, ref ulong bits, ref int bitIndex, ulong bitMask)
		{
			Contract.Requires/*<ArgumentOutOfRangeException>*/(bitIndex >= 0);
			Contract.Requires/*<ArgumentOutOfRangeException>*/(bitIndex < kInt64BitCount);
			Contract.Requires/*<ArgumentException>*/(bitMask != 0);

			int bit_count = BitCount(bitMask);
			Contract.Assert((bitIndex + bit_count) <= kInt64BitCount);

			bits = BitEncodeFlags(value, bits, bitIndex, bitMask);
			bitIndex += bit_count;
		}

		#endregion

		#region BitEncode
		/// <summary>Bit encode a value into an unsigned integer, removing the original data in the value's range</summary>
		/// <param name="value">Value to encode</param>
		/// <param name="bits">Bit data as an unsigned integer</param>
		/// <param name="bitIndex">Index in <paramref name="bits"/> to start encoding at</param>
		/// <param name="bitMask">Masking value for <paramref name="value"/></param>
		/// <returns><paramref name="bits"/> with <paramref name="value"/> encoded into it</returns>
		/// <remarks>
		/// Clears the bit-space between <paramref name="bitIndex"/> + <paramref name="bitMask"/>
		/// so any existing values will be lost after <paramref name="value"/> is added
		/// </remarks>
		[Contracts.Pure]
		public static uint BitEncode(uint value, uint bits, int bitIndex, uint bitMask)
		{
			Contract.Requires/*<ArgumentOutOfRangeException>*/(bitIndex >= 0);
			Contract.Requires/*<ArgumentOutOfRangeException>*/(bitIndex < kInt32BitCount);
			Contract.Requires/*<ArgumentException>*/(bitMask != 0);

			// Use the bit mask's invert so we can get all of the non-value bits
			return BitEncodeFlags(value, bits & (~bitMask), bitIndex, bitMask);
		}
		/// <summary>Bit encode a value into an unsigned integer, removing the original data in the value's range</summary>
		/// <param name="value">Value to encode</param>
		/// <param name="bits">Bit data as an unsigned integer</param>
		/// <param name="bitIndex">Index in <paramref name="bits"/> to start encoding at</param>
		/// <param name="bitMask">Masking value for <paramref name="value"/></param>
		/// <returns><paramref name="bits"/> with <paramref name="value"/> encoded into it</returns>
		/// <remarks>
		/// Clears the bit-space between <paramref name="bitIndex"/> + <paramref name="bitMask"/>
		/// so any existing values will be lost after <paramref name="value"/> is added
		///
		/// On return <paramref name="bitIndex"/> is incremented by the bit count (determined
		/// from <paramref name="bitMask"/>)
		/// </remarks>
		public static void BitEncode(uint value, ref uint bits, ref int bitIndex, uint bitMask)
		{
			Contract.Requires/*<ArgumentOutOfRangeException>*/(bitIndex >= 0);
			Contract.Requires/*<ArgumentOutOfRangeException>*/(bitIndex < kInt32BitCount);
			Contract.Requires/*<ArgumentException>*/(bitMask != 0);

			// Use the bit mask's invert so we can get all of the non-value bits
			bits &= ~bitMask;
			BitEncodeFlags(value, ref bits, ref bitIndex, bitMask);
		}
		/// <summary>Bit encode a value into an unsigned integer, removing the original data in the value's range</summary>
		/// <param name="value">Value to encode</param>
		/// <param name="bits">Bit data as an unsigned integer</param>
		/// <param name="traits"></param>
		/// <returns><paramref name="bits"/> with <paramref name="value"/> encoded into it</returns>
		/// <remarks>
		/// Clears the bit-space between <paramref name="bitIndex"/> + <paramref name="bitMask"/>
		/// so any existing values will be lost after <paramref name="value"/> is added
		/// </remarks>
		[Contracts.Pure]
		public static uint BitEncode(uint value, uint bits, Bitwise.BitFieldTraits traits)
		{
			Contract.Requires/*<ArgumentException>*/(!traits.IsEmpty);

			var bitmask = traits.Bitmask32;
			// Use the bit mask's invert so we can get all of the non-value bits
			return BitEncodeFlags(value, bits & (~bitmask), traits.BitIndex, bitmask);
		}

		/// <summary>Bit encode a value into an unsigned integer, removing the original data in the value's range</summary>
		/// <param name="value">Value to encode</param>
		/// <param name="bits">Bit data as an unsigned integer</param>
		/// <param name="bitIndex">Index in <paramref name="bits"/> to start encoding at</param>
		/// <param name="bitMask">Masking value for <paramref name="value"/></param>
		/// <returns><paramref name="bits"/> with <paramref name="value"/> encoded into it</returns>
		/// <remarks>
		/// Clears the bit-space between <paramref name="bitIndex"/> + <paramref name="bitMask"/>
		/// so any existing values will be lost after <paramref name="value"/> is added
		/// </remarks>
		[Contracts.Pure]
		public static ulong BitEncode(ulong value, ulong bits, int bitIndex, ulong bitMask)
		{
			Contract.Requires/*<ArgumentOutOfRangeException>*/(bitIndex >= 0);
			Contract.Requires/*<ArgumentOutOfRangeException>*/(bitIndex < kInt64BitCount);
			Contract.Requires/*<ArgumentException>*/(bitMask != 0);

			// Use the bit mask's invert so we can get all of the non-value bits
			return BitEncodeFlags(value, bits & (~bitMask), bitIndex, bitMask);
		}
		/// <summary>Bit encode a value into an unsigned integer, removing the original data in the value's range</summary>
		/// <param name="value">Value to encode</param>
		/// <param name="bits">Bit data as an unsigned integer</param>
		/// <param name="bitIndex">Index in <paramref name="bits"/> to start encoding at</param>
		/// <param name="bitMask">Masking value for <paramref name="value"/></param>
		/// <returns><paramref name="bits"/> with <paramref name="value"/> encoded into it</returns>
		/// <remarks>
		/// Clears the bit-space between <paramref name="bitIndex"/> + <paramref name="bitMask"/>
		/// so any existing values will be lost after <paramref name="value"/> is added
		///
		/// On return <paramref name="bitIndex"/> is incremented by the bit count (determined
		/// from <paramref name="bitMask"/>)
		/// </remarks>
		public static void BitEncode(ulong value, ref ulong bits, ref int bitIndex, ulong bitMask)
		{
			Contract.Requires/*<ArgumentOutOfRangeException>*/(bitIndex >= 0);
			Contract.Requires/*<ArgumentOutOfRangeException>*/(bitIndex < kInt64BitCount);
			Contract.Requires/*<ArgumentException>*/(bitMask != 0);

			// Use the bit mask's invert so we can get all of the non-value bits
			bits &= ~bitMask;
			BitEncodeFlags(value, ref bits, ref bitIndex, bitMask);
		}
		/// <summary>Bit encode a value into an unsigned integer, removing the original data in the value's range</summary>
		/// <param name="value">Value to encode</param>
		/// <param name="bits">Bit data as an unsigned integer</param>
		/// <param name="traits"></param>
		/// <returns><paramref name="bits"/> with <paramref name="value"/> encoded into it</returns>
		/// <remarks>
		/// Clears the bit-space between <paramref name="bitIndex"/> + <paramref name="bitMask"/>
		/// so any existing values will be lost after <paramref name="value"/> is added
		/// </remarks>
		[Contracts.Pure]
		public static ulong BitEncode(ulong value, ulong bits, Bitwise.BitFieldTraits traits)
		{
			Contract.Requires/*<ArgumentException>*/(!traits.IsEmpty);

			var bitmask = traits.Bitmask64;
			// Use the bit mask's invert so we can get all of the non-value bits
			return BitEncodeFlags(value, bits & (~bitmask), traits.BitIndex, bitmask);
		}

		#endregion
	};
}
