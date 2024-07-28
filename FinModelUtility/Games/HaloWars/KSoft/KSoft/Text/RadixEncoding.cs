using System;
using System.Collections.Generic;
using System.Numerics;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Text
{
	/// <summary>Encodes/decodes bytes to/from a string</summary>
	/// <remarks>
	/// Encoded string is always in big-endian ordering
	///
	/// <p>Encode and Decode take a <b>includeProceedingZeros</b> parameter which acts as a work-around
	/// for an edge case with our BigInteger implementation.
	/// MSDN says BigInteger byte arrays are in LSB->MSB ordering. So a byte buffer with zeros at the
	/// end will have those zeros ignored in the resulting encoded radix string.
	/// If such a loss in precision absolutely cannot occur pass true to <b>includeProceedingZeros</b>
	/// and for a tiny bit of extra processing it will handle the padding of zero digits (encoding)
	/// or bytes (decoding).</p>
	/// <p>Note: doing this for decoding <b>may</b> add an extra byte more than what was originally
	/// given to Encode.</p>
	/// </remarks>
	// Based on the answers from http://codereview.stackexchange.com/questions/14084/base-36-encoding-of-a-byte-array/
	public class RadixEncoding
	{
		readonly string kDigits;
		readonly double kBitsPerDigit;
		readonly BigInteger kRadixBig;
		readonly Shell.EndianFormat kEndian;
		readonly bool kIncludeProceedingZeros;

		/// <summary>Numerial base of this encoding</summary>
		public int Radix { get { return this.kDigits.Length; } }
		/// <summary>Endian ordering of bytes input to Encode and output by Decode</summary>
		public Shell.EndianFormat Endian { get { return this.kEndian; } }
		/// <summary>True if we want ending zero bytes to be encoded</summary>
		public bool IncludeProceedingZeros { get { return this.kIncludeProceedingZeros; } }

		public override string ToString()
		{
			return string.Format(KSoft.Util.InvariantCultureInfo, "Base-{0} {1}", this.Radix.ToString(KSoft.Util.InvariantCultureInfo), this.kDigits);
		}

		/// <summary>Create a radix encoder using the given characters as the digits in the radix</summary>
		/// <param name="digits">Digits to use for the radix-encoded string</param>
		/// <param name="bytesEndian">Endian ordering of bytes input to Encode and output by Decode</param>
		/// <param name="includeProceedingZeros">True if we want ending zero bytes to be encoded</param>
		public RadixEncoding(string digits,
			Shell.EndianFormat bytesEndian = Shell.EndianFormat.Little, bool includeProceedingZeros = false)
		{
			Contract.Requires<ArgumentNullException>(digits != null);
			int radix = digits.Length;

			this.kDigits = digits;
			this.kBitsPerDigit = Math.Log(radix, 2);
			this.kRadixBig = new BigInteger(radix);
			this.kEndian = bytesEndian;
			this.kIncludeProceedingZeros = includeProceedingZeros;
		}

		// Number of characters needed for encoding the specified number of bytes
		int EncodingCharsCount(int bytesLength)
		{
			return (int)Math.Ceiling((bytesLength * Bits.kByteBitCount) / this.kBitsPerDigit);
		}
		// Number of bytes needed to decoding the specified number of characters
		int DecodingBytesCount(int charsCount)
		{
			return (int)Math.Ceiling((charsCount * this.kBitsPerDigit) / Bits.kByteBitCount);
		}

		/// <summary>Encode a byte array into a radix-encoded string</summary>
		/// <param name="bytes">byte array to encode</param>
		/// <returns>The bytes in encoded into a radix-encoded string</returns>
		/// <remarks>If <paramref name="bytes"/> is zero length, returns an empty string</remarks>
		public string Encode(byte[] bytes)
		{
			Contract.Requires<ArgumentNullException>(bytes != null);
			Contract.Ensures(Contract.Result<string>() != null);

			// Don't really have to do this, our code will build this result (empty string),
			// but why not catch the condition before doing work?
			if (bytes.Length == 0) return string.Empty;

			// if the array ends with zeros, having the capacity set to this will help us know how much
			// 'padding' we will need to add
			int result_length = this.EncodingCharsCount(bytes.Length);
			// List<> has a(n in-place) Reverse method. StringBuilder doesn't. That's why.
			var result = new List<char>(result_length);

			// HACK: BigInteger uses the last byte as the 'sign' byte. If the byte's MSB is set,
			// we need to pad the input with an extra 0 (ie, make it positive)
			if (IntegerMath.IsSigned(bytes[bytes.Length-1]))
				Array.Resize(ref bytes, bytes.Length+1);

			var dividend = new BigInteger(bytes);
			// IsZero's computation is less complex than evaluating "dividend > 0"
			// which invokes BigInteger.CompareTo(BigInteger)
			while (!dividend.IsZero)
			{
				dividend = BigInteger.DivRem(dividend, this.kRadixBig, out BigInteger remainder);
				int digit_index = Math.Abs((int)remainder);
				result.Add(this.kDigits[digit_index]);
			}

			if (this.kIncludeProceedingZeros)
				for (int x = result.Count; x < result.Capacity; x++)
					result.Add(this.kDigits[0]); // pad with the character that represents 'zero'

			// orientate the characters in big-endian ordering
			if (this.kEndian == Shell.EndianFormat.Little)
				result.Reverse();
			// If we didn't end up adding padding, ToArray will end up returning a TrimExcess'd array,
			// so nothing wasted
			return new string(result.ToArray());
		}

		void DecodeImplPadResult(ref byte[] result, int padCount)
		{
			if (padCount > 0)
			{
				int new_length = result.Length + this.DecodingBytesCount(padCount);
				Array.Resize(ref result, new_length); // new bytes will be zero, just the way we want it
			}
		}
		#region Decode (Little Endian)
		byte[] DecodeImpl(string chars, int startIndex = 0)
		{
			var bi = new BigInteger();
			for (int x = startIndex; x < chars.Length; x++)
			{
				int i = this.kDigits.IndexOf(chars[x]);
				if (i < 0) return null; // invalid character
				bi *= this.kRadixBig;
				bi += i;
			}

			return bi.ToByteArray();
		}
		byte[] DecodeImplWithPadding(string chars)
		{
			int pad_count = 0;
			for (int x = 0; x < chars.Length; x++, pad_count++)
				if (chars[x] != this.kDigits[0]) break;

			var result = this.DecodeImpl(chars, pad_count);
			this.DecodeImplPadResult(ref result, pad_count);

			return result;
		}
		#endregion
		#region Decode (Big Endian)
		byte[] DecodeImplReversed(string chars, int startIndex = 0)
		{
			var bi = new BigInteger();
			for (int x = (chars.Length-1)-startIndex; x >= 0; x--)
			{
				int i = this.kDigits.IndexOf(chars[x]);
				if (i < 0) return null; // invalid character
				bi *= this.kRadixBig;
				bi += i;
			}

			return bi.ToByteArray();
		}
		byte[] DecodeImplReversedWithPadding(string chars)
		{
			int pad_count = 0;
			for (int x = chars.Length - 1; x >= 0; x--, pad_count++)
				if (chars[x] != this.kDigits[0]) break;

			var result = this.DecodeImplReversed(chars, pad_count);
			this.DecodeImplPadResult(ref result, pad_count);

			return result;
		}
		#endregion
		/// <summary>Decode a radix-encoded string into a byte array</summary>
		/// <param name="radixChars">radix string</param>
		/// <returns>The decoded bytes, or null if an invalid character is encountered</returns>
		/// <remarks>
		/// If <paramref name="radixChars"/> is an empty string, returns a zero length array
		///
		/// Using <paramref name="IncludeProceedingZeros"/> has the potential to return a buffer with an
		/// additional zero byte that wasn't in the input. So a 4 byte buffer was encoded, this could end up
		/// returning a 5 byte buffer, with the extra byte being null.
		/// </remarks>
		public byte[] Decode(string radixChars)
		{
			Contract.Requires<ArgumentNullException>(radixChars != null);

			if (this.kEndian == Shell.EndianFormat.Big)
				return this.kIncludeProceedingZeros ? this.DecodeImplReversedWithPadding(radixChars) : this.DecodeImplReversed(radixChars);
			else
				return this.kIncludeProceedingZeros ? this.DecodeImplWithPadding(radixChars) : this.DecodeImpl(radixChars);
		}
	};
};
