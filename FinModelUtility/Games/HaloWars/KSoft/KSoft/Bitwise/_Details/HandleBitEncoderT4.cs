using System;
using System.Diagnostics.CodeAnalysis;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Bitwise
{
	partial struct HandleBitEncoder
	{
		public HandleBitEncoder(uint initialBits)
		{
			this.mBits.u64 = 0;
			this.mBits.u32 = 0;
			this.mBitIndex = 0;

			this.mBits.u32 = initialBits;
		}
		public HandleBitEncoder(ulong initialBits)
		{
			this.mBits.u64 = 0;
			this.mBits.u32 = 0;
			this.mBitIndex = 0;

			this.mBits.u64 = initialBits;
		}

		/// <summary>Get the 32-bit handle value</summary>
		/// <returns></returns>
		public uint GetHandle32()
		{
			return this.mBits.u32;
		}
		/// <summary>Get the 64-bit handle value</summary>
		/// <returns></returns>
		public ulong GetHandle64()
		{
			return this.mBits.u64;
		}

		#region Encode
		/// <summary>Encode an enumeration value using an enumeration encoder object</summary>
		/// <typeparam name="TEnum">Enumeration type to encode</typeparam>
		/// <param name="value">Enumeration value to encode</param>
		/// <param name="encoder">Encoder for <typeparamref name="TEnum"/> objects</param>
		[SuppressMessage("Microsoft.Design", "CA1806:DoNotIgnoreMethodResults",
			Justification ="Pretty sure this is a CA bug",
			Scope = "method", Target = "BitEncode")]
		public void Encode32<TEnum>(TEnum value, EnumBitEncoder32<TEnum> encoder)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			Contract.Requires<ArgumentNullException>(encoder != null);

			encoder.BitEncode(value, ref this.mBits.u64, ref this.mBitIndex);
		}
		/// <summary>Encode an enumeration value using an enumeration encoder object</summary>
		/// <typeparam name="TEnum">Enumeration type to encode</typeparam>
		/// <param name="value">Enumeration value to encode</param>
		/// <param name="encoder">Encoder for <typeparamref name="TEnum"/> objects</param>
		[SuppressMessage("Microsoft.Design", "CA1806:DoNotIgnoreMethodResults",
			Justification ="Pretty sure this is a CA bug",
			Scope = "method", Target = "BitEncode")]
		public void Encode64<TEnum>(TEnum value, EnumBitEncoder64<TEnum> encoder)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			Contract.Requires<ArgumentNullException>(encoder != null);

			encoder.BitEncode(value, ref this.mBits.u64, ref this.mBitIndex);
		}

		/// <summary>Bit encode a value into this handle</summary>
		/// <param name="value">Value to encode</param>
		/// <param name="bitMask">Masking value for <paramref name="value"/></param>
		[SuppressMessage("Microsoft.Design", "CA1806:DoNotIgnoreMethodResults",
			Justification ="Pretty sure this is a CA bug",
			Scope = "method", Target = "BitEncode")]
		public void Encode32(uint value, uint bitMask)
		{
			Contract.Requires<ArgumentException>(bitMask != 0);

			Bits.BitEncodeEnum(value, ref this.mBits.u64, ref this.mBitIndex, bitMask);
		}

		/// <summary>Bit encode a none-able value into this handle</summary>
		/// <param name="value">Value to encode</param>
		/// <param name="bitMask">Masking value for <paramref name="value"/></param>
		public void EncodeNoneable32(int value, uint bitMask)
		{
			Contract.Requires<ArgumentException>(bitMask != 0);
			Contract.Requires<ArgumentOutOfRangeException>(value.IsNoneOrPositive());

			Bits.BitEncodeEnum((ulong)(value+1), ref this.mBits.u64, ref this.mBitIndex, bitMask);
		}

		/// <summary>Bit encode a value into this handle</summary>
		/// <param name="value">Value to encode</param>
		/// <param name="traits"></param>
		public void Encode32(uint value, BitFieldTraits traits)
		{
			Contract.Requires<ArgumentException>(!traits.IsEmpty);

			Bits.BitEncodeEnum(value, ref this.mBits.u64, ref this.mBitIndex, traits.Bitmask32);
		}
		/// <summary>Bit encode a none-able value into this handle</summary>
		/// <param name="value">Value to encode</param>
		/// <param name="traits"></param>
		public void EncodeNoneable32(int value, BitFieldTraits traits)
		{
			Contract.Requires<ArgumentException>(!traits.IsEmpty);
			Contract.Requires<ArgumentOutOfRangeException>(value.IsNoneOrPositive());

			Bits.BitEncodeEnum((ulong)(value+1), ref this.mBits.u64, ref this.mBitIndex, traits.Bitmask32);
		}

		/// <summary>Bit encode a value into this handle</summary>
		/// <param name="value">Value to encode</param>
		/// <param name="bitMask">Masking value for <paramref name="value"/></param>
		[SuppressMessage("Microsoft.Design", "CA1806:DoNotIgnoreMethodResults",
			Justification ="Pretty sure this is a CA bug",
			Scope = "method", Target = "BitEncode")]
		public void Encode64(ulong value, ulong bitMask)
		{
			Contract.Requires<ArgumentException>(bitMask != 0);

			Bits.BitEncodeEnum(value, ref this.mBits.u64, ref this.mBitIndex, bitMask);
		}

		/// <summary>Bit encode a none-able value into this handle</summary>
		/// <param name="value">Value to encode</param>
		/// <param name="bitMask">Masking value for <paramref name="value"/></param>
		public void EncodeNoneable64(long value, ulong bitMask)
		{
			Contract.Requires<ArgumentException>(bitMask != 0);
			Contract.Requires<ArgumentOutOfRangeException>(value.IsNoneOrPositive());

			Bits.BitEncodeEnum((ulong)(value+1), ref this.mBits.u64, ref this.mBitIndex, bitMask);
		}

		/// <summary>Bit encode a value into this handle</summary>
		/// <param name="value">Value to encode</param>
		/// <param name="traits"></param>
		public void Encode64(ulong value, BitFieldTraits traits)
		{
			Contract.Requires<ArgumentException>(!traits.IsEmpty);

			Bits.BitEncodeEnum(value, ref this.mBits.u64, ref this.mBitIndex, traits.Bitmask64);
		}
		/// <summary>Bit encode a none-able value into this handle</summary>
		/// <param name="value">Value to encode</param>
		/// <param name="traits"></param>
		public void EncodeNoneable64(long value, BitFieldTraits traits)
		{
			Contract.Requires<ArgumentException>(!traits.IsEmpty);
			Contract.Requires<ArgumentOutOfRangeException>(value.IsNoneOrPositive());

			Bits.BitEncodeEnum((ulong)(value+1), ref this.mBits.u64, ref this.mBitIndex, traits.Bitmask64);
		}

		#endregion

		#region Decode
		/// <summary>Decode an enumeration value using an enumeration encoder object</summary>
		/// <typeparam name="TEnum">Enumeration type to decode</typeparam>
		/// <param name="value">Enumeration value decoded from this handle</param>
		/// <param name="decoder">Encoder for <typeparamref name="TEnum"/> objects</param>
		public void Decode32<TEnum>(out TEnum value, EnumBitEncoder32<TEnum> decoder)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			Contract.Requires<ArgumentNullException>(decoder != null);

			value = decoder.BitDecode(this.mBits.u64, ref this.mBitIndex);
		}
		/// <summary>Decode an enumeration value using an enumeration encoder object</summary>
		/// <typeparam name="TEnum">Enumeration type to decode</typeparam>
		/// <param name="value">Enumeration value decoded from this handle</param>
		/// <param name="decoder">Encoder for <typeparamref name="TEnum"/> objects</param>
		public void Decode64<TEnum>(out TEnum value, EnumBitEncoder64<TEnum> decoder)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			Contract.Requires<ArgumentNullException>(decoder != null);

			value = decoder.BitDecode(this.mBits.u64, ref this.mBitIndex);
		}

		/// <summary>Bit decode a value from this handle</summary>
		/// <param name="value">Value decoded from this handle</param>
		/// <param name="bitMask">Masking value for <paramref name="value"/></param>
		public void Decode32(out uint value, uint bitMask)
		{
			Contract.Requires<ArgumentException>(bitMask != 0);

			value = (uint)Bits.BitDecode(this.mBits.u64, ref this.mBitIndex, bitMask);
		}
		/// <summary>Bit decode a value from this handle</summary>
		/// <param name="value">Value decoded from this handle</param>
		/// <param name="bitMask">Masking value for <paramref name="value"/></param>
		public void DecodeNoneable32(out int value, uint bitMask)
		{
			Contract.Requires<ArgumentException>(bitMask != 0);

			value = (int)Bits.BitDecodeNoneable(this.mBits.u64, ref this.mBitIndex, bitMask);
		}

		/// <summary>Bit decode a value from this handle</summary>
		/// <param name="value">Value decoded from this handle</param>
		/// <param name="traits"></param>
		public void Decode32(out uint value, BitFieldTraits traits)
		{
			Contract.Requires<ArgumentException>(!traits.IsEmpty);

			value = (uint)Bits.BitDecode(this.mBits.u64, ref this.mBitIndex, traits.Bitmask32);
		}
		/// <summary>Bit decode a value from this handle</summary>
		/// <param name="value">Value decoded from this handle</param>
		/// <param name="traits"></param>
		public void DecodeNoneable32(out int value, BitFieldTraits traits)
		{
			Contract.Requires<ArgumentException>(!traits.IsEmpty);

			value = (int)Bits.BitDecodeNoneable(this.mBits.u64, ref this.mBitIndex, traits.Bitmask32);
		}

		/// <summary>Bit decode a value from this handle</summary>
		/// <param name="value">Value decoded from this handle</param>
		/// <param name="bitMask">Masking value for <paramref name="value"/></param>
		public void Decode64(out ulong value, ulong bitMask)
		{
			Contract.Requires<ArgumentException>(bitMask != 0);

			value = (ulong)Bits.BitDecode(this.mBits.u64, ref this.mBitIndex, bitMask);
		}
		/// <summary>Bit decode a value from this handle</summary>
		/// <param name="value">Value decoded from this handle</param>
		/// <param name="bitMask">Masking value for <paramref name="value"/></param>
		public void DecodeNoneable64(out long value, ulong bitMask)
		{
			Contract.Requires<ArgumentException>(bitMask != 0);

			value = (long)Bits.BitDecodeNoneable(this.mBits.u64, ref this.mBitIndex, bitMask);
		}

		/// <summary>Bit decode a value from this handle</summary>
		/// <param name="value">Value decoded from this handle</param>
		/// <param name="traits"></param>
		public void Decode64(out ulong value, BitFieldTraits traits)
		{
			Contract.Requires<ArgumentException>(!traits.IsEmpty);

			value = (ulong)Bits.BitDecode(this.mBits.u64, ref this.mBitIndex, traits.Bitmask64);
		}
		/// <summary>Bit decode a value from this handle</summary>
		/// <param name="value">Value decoded from this handle</param>
		/// <param name="traits"></param>
		public void DecodeNoneable64(out long value, BitFieldTraits traits)
		{
			Contract.Requires<ArgumentException>(!traits.IsEmpty);

			value = (long)Bits.BitDecodeNoneable(this.mBits.u64, ref this.mBitIndex, traits.Bitmask64);
		}

		#endregion
	};
}
