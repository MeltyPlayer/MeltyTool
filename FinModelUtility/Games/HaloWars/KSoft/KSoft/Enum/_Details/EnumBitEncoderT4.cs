using System;
using Contracts = System.Diagnostics.Contracts;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft
{
	/// <summary>Utility class for encoding Enumerations into an integer's bits.</summary>
	/// <typeparam name="TEnum"></typeparam>
	/// <remarks>
	/// Regular Enumerations should have a member called <b>kMax</b>. This value
	/// must be the highest value and shouldn't actually be used.
	/// If <b>kMax</b> doesn't exist, the highest value found, plus 1, is used as
	/// the assumed <b>kMax</b>
	///
	/// <see cref="FlagsAttribute"/> Enumerations should have a member called
	/// <b>kAll</b>. This value must be equal to all the usable bits in the type.
	/// If you want to leave a certain bit or bits out of the encoder, don't include
	/// them in <b>kAll</b>'s value.
	/// If <b>kAll</b> doesn't exist, ALL members are OR'd together to create the
	/// assumed <b>kAll</b> value.
	/// </remarks>
	[System.Diagnostics.DebuggerDisplay("MaxValue = {MaxValueTrait}, Bitmask = {BitmaskTrait}, BitCount = {BitCountTrait}")]
	public sealed partial class EnumBitEncoder32<TEnum> : EnumBitEncoderBase, IEnumBitEncoder<uint>
		where TEnum : struct, IComparable, IFormattable, IConvertible
	{
		/// <remarks>Only made public for some Contracts in <see cref="Collections.EnumBitSet"/></remarks>
		public static readonly bool kHasNone;
		/// <summary>
		/// The <see cref="kEnumMaxMemberName"/>\<see cref="kFlagsMaxMemberName"/>
		/// value or the member value whom this class assumed would be the max
		/// </summary>
		static readonly uint kMaxValue;
		/// <summary>Masking value that can be used to single out this enumeration's value(s)</summary>
		public static readonly uint kBitmask;
		/// <summary>How many bits the enumeration consumes</summary>
		public static readonly int kBitCount;

		#region Static Initialize
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA2208:InstantiateArgumentExceptionsCorrectly")]
		static void ProcessMembers(Type t, out uint maxValue, out bool hasNone)
		{
			maxValue = uint.MaxValue;
			hasNone = false;
			var mvalues = Reflection.EnumUtil<TEnum>.Values;
			var mnames = Reflection.EnumUtil<TEnum>.Names;

			#region is_type_signed
			bool func_is_type_signed()
			{
				switch (Reflection.EnumUtil<TEnum>.UnderlyingTypeCode)
				{
					case TypeCode.SByte:
					case TypeCode.Int16:
					case TypeCode.Int32:
						return true;
					default: return false;
				}
			}
			bool is_type_signed = func_is_type_signed();
			#endregion

			uint greatest = 0, temp;
			for (int x = 0; x < mvalues.Length; x++)
			{
				bool mvalue_is_none = false;

				// Validate members when the underlying type is signed
				if (!Reflection.EnumUtil<TEnum>.IsFlags && is_type_signed)
				{
					int int_value = Convert.ToInt32(mvalues.GetValue(x), Util.InvariantCultureInfo);

					if (int_value < TypeExtensions.kNoneInt32)
					{
						// CA2208
						throw new ArgumentOutOfRangeException(nameof(TEnum),
							string.Format(Util.InvariantCultureInfo,
								"{0}:{1} is invalid (negative, less than NONE)!", t.FullName, mnames[x]));
					}
					else if (int_value.IsNone())
					{
						hasNone = mvalue_is_none = true;
					}
				}

				ProcessMembers_DebugCheckMemberName(t, Reflection.EnumUtil<TEnum>.IsFlags, mnames[x]);

				if (mvalue_is_none) // don't perform greatest value checking on NONE values
					continue;

				temp = Convert.ToUInt32(mvalues.GetValue(x), Util.InvariantCultureInfo);
				// Base max_value off the predetermined member name first
				if (IsMaxMemberName(Reflection.EnumUtil<TEnum>.IsFlags, mnames[x]))
				{
					maxValue = greatest = temp;
					// we don't stop processing even after we hit the 'max' member
					// just to be safe that we're sanity checking all members, and in the event
					// the 'none' member is defined after the 'max' member
					//break;
				}
				// Record the greatest value thus far in case the above doesn't exist
				else
				{
					if (!Reflection.EnumUtil<TEnum>.IsFlags)
						greatest = Math.Max(greatest, temp);
					else
						greatest |= temp; // just add all the flag values together
				}
			}

			// If the Enum doesn't have a member named k*MaxMemberName, use the assumed max value
			if (maxValue == uint.MaxValue && greatest != uint.MaxValue) // just in case k*MaxMemberName actually equaled uint.MaxValue
			{
				maxValue = greatest;

				// NOTE: we add +1 because the [Bits.GetBitmaskEnum32] method assumes the parameter
				// isn't a real member of the enumeration. We didn't find a k*MaxMemberName so we
				// fake it
				if (!Reflection.EnumUtil<TEnum>.IsFlags)
					maxValue += 1;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
		static EnumBitEncoder32()
		{
			Type t = typeof(TEnum);
			InitializeBase(t);

			ProcessMembers(t, out kMaxValue, out kHasNone);
			if (Reflection.EnumUtil<TEnum>.IsFlags)
				kBitmask = kMaxValue;
			else
				kBitmask = Bits.GetBitmaskEnum(kHasNone ? kMaxValue+1 : kMaxValue);
			kBitCount = Bits.BitCount(kBitmask);
		}
		#endregion

		#region IEnumBitEncoder<TUInt>
		public bool IsFlags { get { return Reflection.EnumUtil<TEnum>.IsFlags; } }
		public bool HasNone { get { return kHasNone; } }
		public uint MaxValueTrait { get { return kMaxValue; } }
		/// <see cref="kBitmask"/>
		public uint BitmaskTrait { get { return kBitmask; } }
		/// <see cref="kBitCount"/>
		public override int BitCountTrait { get { return kBitCount; } }
		#endregion

		#region DefaultBitIndex
		readonly int mDefaultBitIndex;
		/// <summary>The bit index assumed when one isn't provided</summary>
		public int DefaultBitIndex { get { return this.mDefaultBitIndex; } }
		#endregion

		public EnumBitEncoder32() : this(0) {}
		public EnumBitEncoder32(int defaultBitIndex)
		{
			Contract.Requires(defaultBitIndex >= 0);

			this.mDefaultBitIndex = defaultBitIndex;
		}

		#region Encode
		/// <summary>Bit encode an enumeration value into an unsigned integer</summary>
		/// <param name="value">Enumeration value to encode</param>
		/// <param name="bits">Bit data as an unsigned integer</param>
		/// <returns><paramref name="bits"/> with <paramref name="value"/> encoded into it</returns>
		/// <remarks>Uses <see cref="DefaultBitIndex"/> as the bit index to start encoding at</remarks>
		[Contracts.Pure]
		public uint BitEncode(TEnum value, uint bits)
		{
			return this.BitEncode(value, bits, this.mDefaultBitIndex);
		}
		/// <summary>Bit encode an enumeration value into an unsigned integer</summary>
		/// <param name="value">Enumeration value to encode</param>
		/// <param name="bits">Bit data as an unsigned integer</param>
		/// <param name="bitIndex">Index in <paramref name="bits"/> to start encoding at</param>
		/// <returns><paramref name="bits"/> with <paramref name="value"/> encoded into it</returns>
		[Contracts.Pure]
		public uint BitEncode(TEnum value, uint bits, int bitIndex)
		{
			Contract.Requires(bitIndex >= 0);
			Contract.Requires(bitIndex < Bits.kInt32BitCount);

			uint v = Reflection.EnumValue<TEnum>.ToUInt32(value);
			if (kHasNone)
				v++;

			Contract.Assert(v <= kMaxValue);
			return Reflection.EnumUtil<TEnum>.IsFlags ?
				Bits.BitEncodeFlags(v, bits, bitIndex, kBitmask) :
				Bits.BitEncodeEnum (v, bits, bitIndex, kBitmask);
		}
		/// <summary>Bit encode an enumeration value into an unsigned integer</summary>
		/// <param name="value">Enumeration value to encode</param>
		/// <param name="bits">Bit data as an unsigned integer</param>
		/// <param name="traits">Index in <paramref name="bits"/> to start encoding at</param>
		/// <returns><paramref name="bits"/> with <paramref name="value"/> encoded into it</returns>
		[Contracts.Pure]
		public uint BitEncode(TEnum value, uint bits, Bitwise.BitFieldTraits traits)
		{
			Contract.Requires(!traits.IsEmpty);

			return this.BitEncode(value, bits, traits.BitIndex);
		}
		/// <summary>Bit encode an enumeration value into an unsigned integer</summary>
		/// <param name="value">Enumeration value to encode</param>
		/// <param name="bits">Bit data as an unsigned integer</param>
		/// <param name="bitIndex">Index in <paramref name="bits"/> to start encoding at</param>
		/// <remarks>
		/// On return <paramref name="bits"/> has <paramref name="value"/> encoded into it and
		/// <paramref name="bitIndex"/> is incremented by the bit count of the underlying enumeration
		/// </remarks>
		[Contracts.Pure]
		public void BitEncode(TEnum value, ref uint bits, ref int bitIndex)
		{
			Contract.Requires(bitIndex >= 0);
			Contract.Requires(bitIndex < Bits.kInt32BitCount);
			Contract.Requires((bitIndex+kBitCount) < Bits.kInt32BitCount);

			uint v = Reflection.EnumValue<TEnum>.ToUInt32(value);
			if (kHasNone)
				v++;

			Contract.Assert(v <= kMaxValue);
			bits = Reflection.EnumUtil<TEnum>.IsFlags ?
				Bits.BitEncodeFlags(v, bits, bitIndex, kBitmask) :
				Bits.BitEncodeEnum (v, bits, bitIndex, kBitmask);

			bitIndex += kBitCount;
		}
		#endregion

		#region Decode
		/// <summary>Bit decode an enumeration value from an unsigned integer</summary>
		/// <param name="bits">Unsigned integer to decode from<</param>
		/// <returns>The enumeration value as it stood before it was ever encoded into <paramref name="bits"/></returns>
		/// <remarks>Uses <see cref="DefaultBitIndex"/> as the bit index to start decoding at</remarks>
		[Contracts.Pure]
		public TEnum BitDecode(uint bits)
		{
			return this.BitDecode(bits, this.mDefaultBitIndex);
		}
		/// <summary>Bit decode an enumeration value from an unsigned integer</summary>
		/// <param name="bits">Unsigned integer to decode from</param>
		/// <param name="bitIndex">Index in <paramref name="bits"/> to start decoding at</param>
		/// <returns>The enumeration value as it stood before it was ever encoded into <paramref name="bits"/></returns>
		[Contracts.Pure]
		public TEnum BitDecode(uint bits, int bitIndex)
		{
			Contract.Requires(bitIndex >= 0);
			Contract.Requires(bitIndex < Bits.kInt32BitCount);

			uint v = Bits.BitDecode(bits, bitIndex, kBitmask);
			if (kHasNone)
				v--;

			Contract.Assert(v <= kMaxValue || (kHasNone && v == uint.MaxValue));
			return Reflection.EnumValue<TEnum>.FromUInt32(v);
		}
		/// <summary>Bit decode an enumeration value from an unsigned integer</summary>
		/// <param name="bits">Unsigned integer to decode from</param>
		/// <param name="traits">Index in <paramref name="bits"/> to start decoding at</param>
		/// <returns>The enumeration value as it stood before it was ever encoded into <paramref name="bits"/></returns>
		[Contracts.Pure]
		public TEnum BitDecode(uint bits, Bitwise.BitFieldTraits traits)
		{
			Contract.Requires(!traits.IsEmpty);

			return this.BitDecode(bits, traits.BitIndex);
		}
		/// <summary>Bit decode an enumeration value from an unsigned integer</summary>
		/// <param name="bits">Unsigned integer to decode from</param>
		/// <param name="bitIndex">Index in <paramref name="bits"/> to start decoding at</param>
		/// <returns>The enumeration value as it stood before it was ever encoded into <paramref name="bits"/></returns>
		/// <remarks>
		/// <paramref name="bitIndex"/> is incremented by the bit count of the underlying enumeration
		/// </remarks>
		[Contracts.Pure]
		public TEnum BitDecode(uint bits, ref int bitIndex)
		{
			Contract.Requires(bitIndex >= 0);
			Contract.Requires(bitIndex < Bits.kInt32BitCount);
			Contract.Requires((bitIndex+kBitCount) < Bits.kInt32BitCount);

			uint v = Bits.BitDecode(bits, bitIndex, kBitmask);
			if (kHasNone)
				v--;

			bitIndex += kBitCount;

			Contract.Assert(v <= kMaxValue || (kHasNone && v == uint.MaxValue));
			return Reflection.EnumValue<TEnum>.FromUInt32(v);
		}
		#endregion

		#region Endian Streaming
		/// <summary>Read a <typeparamref name="TEnum"/> value from a stream</summary>
		/// <param name="s">Stream to read from</param>
		/// <param name="value">Enum value read from the stream</param>
		/// <remarks>
		/// Uses <typeparamref name="TEnum"/>'s underlying <see cref="TypeCode"/> to
		/// decide how big of a numeric type to read from the stream.
		/// </remarks>
		public static void Read(IO.EndianReader s, out TEnum value)
		{
			Contract.Requires(s != null);

			uint stream_value;
			switch (Reflection.EnumUtil<TEnum>.UnderlyingTypeCode)
			{
				case TypeCode.Byte:
				case TypeCode.SByte: stream_value = s.ReadByte();
					break;
				case TypeCode.Int16:
				case TypeCode.UInt16: stream_value = s.ReadUInt16();
					break;
				case TypeCode.Int32:
				case TypeCode.UInt32: stream_value = s.ReadUInt32();
					break;

				default:
					throw new Debug.UnreachableException();
			}

			value = Reflection.EnumValue<TEnum>.FromUInt64(stream_value);
		}
		/// <summary>Write a <typeparamref name="TEnum"/> value to a stream</summary>
		/// <param name="s">Stream to write to</param>
		/// <param name="value">Value to write to the stream</param>
		/// <remarks>
		/// Uses <typeparamref name="TEnum"/>'s underlying <see cref="TypeCode"/> to
		/// decide how big of a numeric type to write to the stream.
		/// </remarks>
		public static void Write(IO.EndianWriter s, TEnum value)
		{
			Contract.Requires(s != null);

			uint stream_value = Reflection.EnumValue<TEnum>.ToUInt32(value);
			switch (Reflection.EnumUtil<TEnum>.UnderlyingTypeCode)
			{
				case TypeCode.Byte:
				case TypeCode.SByte: s.Write((byte)stream_value);
					break;
				case TypeCode.Int16:
				case TypeCode.UInt16: s.Write((ushort)stream_value);
					break;
				case TypeCode.Int32:
				case TypeCode.UInt32: s.Write((uint)stream_value);
					break;

				default:
					throw new Debug.UnreachableException();
			}
		}
		#endregion
	};

	/// <summary>Utility class for encoding Enumerations into an integer's bits.</summary>
	/// <typeparam name="TEnum"></typeparam>
	/// <remarks>
	/// Regular Enumerations should have a member called <b>kMax</b>. This value
	/// must be the highest value and shouldn't actually be used.
	/// If <b>kMax</b> doesn't exist, the highest value found, plus 1, is used as
	/// the assumed <b>kMax</b>
	///
	/// <see cref="FlagsAttribute"/> Enumerations should have a member called
	/// <b>kAll</b>. This value must be equal to all the usable bits in the type.
	/// If you want to leave a certain bit or bits out of the encoder, don't include
	/// them in <b>kAll</b>'s value.
	/// If <b>kAll</b> doesn't exist, ALL members are OR'd together to create the
	/// assumed <b>kAll</b> value.
	/// </remarks>
	[System.Diagnostics.DebuggerDisplay("MaxValue = {MaxValueTrait}, Bitmask = {BitmaskTrait}, BitCount = {BitCountTrait}")]
	public sealed partial class EnumBitEncoder64<TEnum> : EnumBitEncoderBase, IEnumBitEncoder<ulong>
		where TEnum : struct, IComparable, IFormattable, IConvertible
	{
		/// <remarks>Only made public for some Contracts in <see cref="Collections.EnumBitSet"/></remarks>
		public static readonly bool kHasNone;
		/// <summary>
		/// The <see cref="kEnumMaxMemberName"/>\<see cref="kFlagsMaxMemberName"/>
		/// value or the member value whom this class assumed would be the max
		/// </summary>
		static readonly ulong kMaxValue;
		/// <summary>Masking value that can be used to single out this enumeration's value(s)</summary>
		public static readonly ulong kBitmask;
		/// <summary>How many bits the enumeration consumes</summary>
		public static readonly int kBitCount;

		#region Static Initialize
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA2208:InstantiateArgumentExceptionsCorrectly")]
		static void ProcessMembers(Type t, out ulong maxValue, out bool hasNone)
		{
			maxValue = ulong.MaxValue;
			hasNone = false;
			var mvalues = Reflection.EnumUtil<TEnum>.Values;
			var mnames = Reflection.EnumUtil<TEnum>.Names;

			#region is_type_signed
			bool func_is_type_signed()
			{
				switch (Reflection.EnumUtil<TEnum>.UnderlyingTypeCode)
				{
					case TypeCode.SByte:
					case TypeCode.Int16:
					case TypeCode.Int32:
					case TypeCode.Int64:
						return true;
					default: return false;
				}
			}
			bool is_type_signed = func_is_type_signed();
			#endregion

			ulong greatest = 0, temp;
			for (int x = 0; x < mvalues.Length; x++)
			{
				bool mvalue_is_none = false;

				// Validate members when the underlying type is signed
				if (!Reflection.EnumUtil<TEnum>.IsFlags && is_type_signed)
				{
					long int_value = Convert.ToInt64(mvalues.GetValue(x), Util.InvariantCultureInfo);

					if (int_value < TypeExtensions.kNoneInt64)
					{
						// CA2208
						throw new ArgumentOutOfRangeException(nameof(TEnum),
							string.Format(Util.InvariantCultureInfo,
								"{0}:{1} is invalid (negative, less than NONE)!", t.FullName, mnames[x]));
					}
					else if (int_value.IsNone())
					{
						hasNone = mvalue_is_none = true;
					}
				}

				ProcessMembers_DebugCheckMemberName(t, Reflection.EnumUtil<TEnum>.IsFlags, mnames[x]);

				if (mvalue_is_none) // don't perform greatest value checking on NONE values
					continue;

				temp = Convert.ToUInt64(mvalues.GetValue(x), Util.InvariantCultureInfo);
				// Base max_value off the predetermined member name first
				if (IsMaxMemberName(Reflection.EnumUtil<TEnum>.IsFlags, mnames[x]))
				{
					maxValue = greatest = temp;
					// we don't stop processing even after we hit the 'max' member
					// just to be safe that we're sanity checking all members, and in the event
					// the 'none' member is defined after the 'max' member
					//break;
				}
				// Record the greatest value thus far in case the above doesn't exist
				else
				{
					if (!Reflection.EnumUtil<TEnum>.IsFlags)
						greatest = Math.Max(greatest, temp);
					else
						greatest |= temp; // just add all the flag values together
				}
			}

			// If the Enum doesn't have a member named k*MaxMemberName, use the assumed max value
			if (maxValue == ulong.MaxValue && greatest != ulong.MaxValue) // just in case k*MaxMemberName actually equaled uint.MaxValue
			{
				maxValue = greatest;

				// NOTE: we add +1 because the [Bits.GetBitmaskEnum64] method assumes the parameter
				// isn't a real member of the enumeration. We didn't find a k*MaxMemberName so we
				// fake it
				if (!Reflection.EnumUtil<TEnum>.IsFlags)
					maxValue += 1;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
		static EnumBitEncoder64()
		{
			Type t = typeof(TEnum);
			InitializeBase(t);

			ProcessMembers(t, out kMaxValue, out kHasNone);
			if (Reflection.EnumUtil<TEnum>.IsFlags)
				kBitmask = kMaxValue;
			else
				kBitmask = Bits.GetBitmaskEnum(kHasNone ? kMaxValue+1 : kMaxValue);
			kBitCount = Bits.BitCount(kBitmask);
		}
		#endregion

		#region IEnumBitEncoder<TUInt>
		public bool IsFlags { get { return Reflection.EnumUtil<TEnum>.IsFlags; } }
		public bool HasNone { get { return kHasNone; } }
		public ulong MaxValueTrait { get { return kMaxValue; } }
		/// <see cref="kBitmask"/>
		public ulong BitmaskTrait { get { return kBitmask; } }
		/// <see cref="kBitCount"/>
		public override int BitCountTrait { get { return kBitCount; } }
		#endregion

		#region DefaultBitIndex
		readonly int mDefaultBitIndex;
		/// <summary>The bit index assumed when one isn't provided</summary>
		public int DefaultBitIndex { get { return this.mDefaultBitIndex; } }
		#endregion

		public EnumBitEncoder64() : this(0) {}
		public EnumBitEncoder64(int defaultBitIndex)
		{
			Contract.Requires(defaultBitIndex >= 0);

			this.mDefaultBitIndex = defaultBitIndex;
		}

		#region Encode
		/// <summary>Bit encode an enumeration value into an unsigned integer</summary>
		/// <param name="value">Enumeration value to encode</param>
		/// <param name="bits">Bit data as an unsigned integer</param>
		/// <returns><paramref name="bits"/> with <paramref name="value"/> encoded into it</returns>
		/// <remarks>Uses <see cref="DefaultBitIndex"/> as the bit index to start encoding at</remarks>
		[Contracts.Pure]
		public ulong BitEncode(TEnum value, ulong bits)
		{
			return this.BitEncode(value, bits, this.mDefaultBitIndex);
		}
		/// <summary>Bit encode an enumeration value into an unsigned integer</summary>
		/// <param name="value">Enumeration value to encode</param>
		/// <param name="bits">Bit data as an unsigned integer</param>
		/// <param name="bitIndex">Index in <paramref name="bits"/> to start encoding at</param>
		/// <returns><paramref name="bits"/> with <paramref name="value"/> encoded into it</returns>
		[Contracts.Pure]
		public ulong BitEncode(TEnum value, ulong bits, int bitIndex)
		{
			Contract.Requires(bitIndex >= 0);
			Contract.Requires(bitIndex < Bits.kInt64BitCount);

			ulong v = Reflection.EnumValue<TEnum>.ToUInt64(value);
			if (kHasNone)
				v++;

			Contract.Assert(v <= kMaxValue);
			return Reflection.EnumUtil<TEnum>.IsFlags ?
				Bits.BitEncodeFlags(v, bits, bitIndex, kBitmask) :
				Bits.BitEncodeEnum (v, bits, bitIndex, kBitmask);
		}
		/// <summary>Bit encode an enumeration value into an unsigned integer</summary>
		/// <param name="value">Enumeration value to encode</param>
		/// <param name="bits">Bit data as an unsigned integer</param>
		/// <param name="traits">Index in <paramref name="bits"/> to start encoding at</param>
		/// <returns><paramref name="bits"/> with <paramref name="value"/> encoded into it</returns>
		[Contracts.Pure]
		public ulong BitEncode(TEnum value, ulong bits, Bitwise.BitFieldTraits traits)
		{
			Contract.Requires(!traits.IsEmpty);

			return this.BitEncode(value, bits, traits.BitIndex);
		}
		/// <summary>Bit encode an enumeration value into an unsigned integer</summary>
		/// <param name="value">Enumeration value to encode</param>
		/// <param name="bits">Bit data as an unsigned integer</param>
		/// <param name="bitIndex">Index in <paramref name="bits"/> to start encoding at</param>
		/// <remarks>
		/// On return <paramref name="bits"/> has <paramref name="value"/> encoded into it and
		/// <paramref name="bitIndex"/> is incremented by the bit count of the underlying enumeration
		/// </remarks>
		[Contracts.Pure]
		public void BitEncode(TEnum value, ref ulong bits, ref int bitIndex)
		{
			Contract.Requires(bitIndex >= 0);
			Contract.Requires(bitIndex < Bits.kInt64BitCount);
			Contract.Requires((bitIndex+kBitCount) < Bits.kInt64BitCount);

			ulong v = Reflection.EnumValue<TEnum>.ToUInt64(value);
			if (kHasNone)
				v++;

			Contract.Assert(v <= kMaxValue);
			bits = Reflection.EnumUtil<TEnum>.IsFlags ?
				Bits.BitEncodeFlags(v, bits, bitIndex, kBitmask) :
				Bits.BitEncodeEnum (v, bits, bitIndex, kBitmask);

			bitIndex += kBitCount;
		}
		#endregion

		#region Decode
		/// <summary>Bit decode an enumeration value from an unsigned integer</summary>
		/// <param name="bits">Unsigned integer to decode from<</param>
		/// <returns>The enumeration value as it stood before it was ever encoded into <paramref name="bits"/></returns>
		/// <remarks>Uses <see cref="DefaultBitIndex"/> as the bit index to start decoding at</remarks>
		[Contracts.Pure]
		public TEnum BitDecode(ulong bits)
		{
			return this.BitDecode(bits, this.mDefaultBitIndex);
		}
		/// <summary>Bit decode an enumeration value from an unsigned integer</summary>
		/// <param name="bits">Unsigned integer to decode from</param>
		/// <param name="bitIndex">Index in <paramref name="bits"/> to start decoding at</param>
		/// <returns>The enumeration value as it stood before it was ever encoded into <paramref name="bits"/></returns>
		[Contracts.Pure]
		public TEnum BitDecode(ulong bits, int bitIndex)
		{
			Contract.Requires(bitIndex >= 0);
			Contract.Requires(bitIndex < Bits.kInt64BitCount);

			ulong v = Bits.BitDecode(bits, bitIndex, kBitmask);
			if (kHasNone)
				v--;

			Contract.Assert(v <= kMaxValue || (kHasNone && v == ulong.MaxValue));
			return Reflection.EnumValue<TEnum>.FromUInt64(v);
		}
		/// <summary>Bit decode an enumeration value from an unsigned integer</summary>
		/// <param name="bits">Unsigned integer to decode from</param>
		/// <param name="traits">Index in <paramref name="bits"/> to start decoding at</param>
		/// <returns>The enumeration value as it stood before it was ever encoded into <paramref name="bits"/></returns>
		[Contracts.Pure]
		public TEnum BitDecode(ulong bits, Bitwise.BitFieldTraits traits)
		{
			Contract.Requires(!traits.IsEmpty);

			return this.BitDecode(bits, traits.BitIndex);
		}
		/// <summary>Bit decode an enumeration value from an unsigned integer</summary>
		/// <param name="bits">Unsigned integer to decode from</param>
		/// <param name="bitIndex">Index in <paramref name="bits"/> to start decoding at</param>
		/// <returns>The enumeration value as it stood before it was ever encoded into <paramref name="bits"/></returns>
		/// <remarks>
		/// <paramref name="bitIndex"/> is incremented by the bit count of the underlying enumeration
		/// </remarks>
		[Contracts.Pure]
		public TEnum BitDecode(ulong bits, ref int bitIndex)
		{
			Contract.Requires(bitIndex >= 0);
			Contract.Requires(bitIndex < Bits.kInt64BitCount);
			Contract.Requires((bitIndex+kBitCount) < Bits.kInt64BitCount);

			ulong v = Bits.BitDecode(bits, bitIndex, kBitmask);
			if (kHasNone)
				v--;

			bitIndex += kBitCount;

			Contract.Assert(v <= kMaxValue || (kHasNone && v == ulong.MaxValue));
			return Reflection.EnumValue<TEnum>.FromUInt64(v);
		}
		#endregion

		#region Endian Streaming
		/// <summary>Read a <typeparamref name="TEnum"/> value from a stream</summary>
		/// <param name="s">Stream to read from</param>
		/// <param name="value">Enum value read from the stream</param>
		/// <remarks>
		/// Uses <typeparamref name="TEnum"/>'s underlying <see cref="TypeCode"/> to
		/// decide how big of a numeric type to read from the stream.
		/// </remarks>
		public static void Read(IO.EndianReader s, out TEnum value)
		{
			Contract.Requires(s != null);

			ulong stream_value;
			switch (Reflection.EnumUtil<TEnum>.UnderlyingTypeCode)
			{
				case TypeCode.Byte:
				case TypeCode.SByte: stream_value = s.ReadByte();
					break;
				case TypeCode.Int16:
				case TypeCode.UInt16: stream_value = s.ReadUInt16();
					break;
				case TypeCode.Int32:
				case TypeCode.UInt32: stream_value = s.ReadUInt32();
					break;
				case TypeCode.Int64:
				case TypeCode.UInt64: stream_value = s.ReadUInt64();
					break;

				default:
					throw new Debug.UnreachableException();
			}

			value = Reflection.EnumValue<TEnum>.FromUInt64(stream_value);
		}
		/// <summary>Write a <typeparamref name="TEnum"/> value to a stream</summary>
		/// <param name="s">Stream to write to</param>
		/// <param name="value">Value to write to the stream</param>
		/// <remarks>
		/// Uses <typeparamref name="TEnum"/>'s underlying <see cref="TypeCode"/> to
		/// decide how big of a numeric type to write to the stream.
		/// </remarks>
		public static void Write(IO.EndianWriter s, TEnum value)
		{
			Contract.Requires(s != null);

			ulong stream_value = Reflection.EnumValue<TEnum>.ToUInt64(value);
			switch (Reflection.EnumUtil<TEnum>.UnderlyingTypeCode)
			{
				case TypeCode.Byte:
				case TypeCode.SByte: s.Write((byte)stream_value);
					break;
				case TypeCode.Int16:
				case TypeCode.UInt16: s.Write((ushort)stream_value);
					break;
				case TypeCode.Int32:
				case TypeCode.UInt32: s.Write((uint)stream_value);
					break;
				case TypeCode.Int64:
				case TypeCode.UInt64: s.Write(stream_value);
					break;

				default:
					throw new Debug.UnreachableException();
			}
		}
		#endregion
	};

}
