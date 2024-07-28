using System;
using System.Diagnostics.CodeAnalysis;

namespace KSoft.Text
{
	using Memory.Strings;

	partial class StringStorageEncoding
	{
		#region CalculateByteCount
		/// <summary>Calculate how many additional bytes are needed to encode a raw <see cref="StringStorageType.CString"/> string</summary>
		/// <param name="byteCount">Base characters byte count</param>
		/// <returns>Total byte count needed for encoding a <see cref="StringStorageType.CString"/> string</returns>
		int CalcByteCountCString(int byteCount)
		{
			return byteCount + mNullCharacterSize;
		}
		/// <summary>Calculate how many additional bytes are needed to encode a raw <see cref="StringStorageType.Pascal"/> string</summary>
		/// <param name="byteCount">Base characters byte count</param>
		/// <returns>Total byte count needed for encoding a <see cref="StringStorageType.Pascal"/> string</returns>
		int CalcByteCountPascal(int byteCount)
		{
			switch (mStorage.LengthPrefix)
			{
				case StringStorageLengthPrefix.Int7: return byteCount + Bitwise.Encoded7BitInt.CalculateSize(byteCount);
				case StringStorageLengthPrefix.Int8: return byteCount + sizeof(byte);
				case StringStorageLengthPrefix.Int16:return byteCount + sizeof(short);
				case StringStorageLengthPrefix.Int32:return byteCount + sizeof(int);
				default:
					throw new Debug.UnreachableException(mStorage.LengthPrefix.ToString());
			}
		}

	/// <summary>Calculate how many additional bytes are needed for encoding a raw string</summary>
	/// <param name="byteCount">Base characters byte count</param>
	/// <returns>Total byte count needed for encoding a string</returns>
	int CalculateByteCount(int byteCount)
		{
			if (mStorage.IsFixedLength)
				return mFixedLengthByteLength;

			switch (mStorage.Type)
			{
				case StringStorageType.CString:		byteCount = CalcByteCountCString(byteCount); break;
				case StringStorageType.Pascal:		byteCount = CalcByteCountPascal(byteCount); break;
				// CharArray doesn't do anything anyway
				case StringStorageType.CharArray:	/*byteCount = CalcByteCountCharArray(byteCount);*/ break;
				default:
					throw new Debug.UnreachableException(mStorage.Type.ToString());
			}

			return byteCount;
		}
		#endregion

		/// <summary>If the storage requires a fixed length, this will clamp the count to be within that length</summary>
		/// <param name="charCount"></param>
		void ClampCharCount(ref int charCount)
		{
			if (!mStorage.IsFixedLength)
				return;

			switch (mStorage.Type)
			{
				case StringStorageType.CString:
					int fixed_length = mStorage.FixedLength - 1; // don't include null char

					if (charCount > fixed_length) charCount = fixed_length;
					break;
				case StringStorageType.CharArray:
					if (charCount > mStorage.FixedLength) charCount = mStorage.FixedLength;
					break;
				default:
					throw new Debug.UnreachableException(mStorage.Type.ToString());
			}
		}

		#region Encode StringStorageType Data Prefix
		int EncStringStorageTypePrefixPascalData(int charCount, byte[] bytes, int byteIndex)
		{
			int prefix_bytes;
			switch (mStorage.LengthPrefix)
			{
				case StringStorageLengthPrefix.Int7:	Bitwise.Encoded7BitInt.Write(bytes, byteIndex, charCount);
					prefix_bytes = Bitwise.Encoded7BitInt.CalculateSize(charCount); break;
				case StringStorageLengthPrefix.Int8:	bytes[byteIndex] = (byte)charCount;
					prefix_bytes = sizeof(byte); break;
				case StringStorageLengthPrefix.Int16:	Bitwise.ByteSwap.ReplaceBytes(bytes, byteIndex, (short)charCount);
														Bitwise.ByteSwap.SwapInt16(bytes, byteIndex);
					prefix_bytes = sizeof(short); break;
				case StringStorageLengthPrefix.Int32:	Bitwise.ByteSwap.ReplaceBytes(bytes, byteIndex, charCount);
														Bitwise.ByteSwap.SwapInt32(bytes, byteIndex);
					prefix_bytes = sizeof(int); break;
				default:
					throw new Debug.UnreachableException(mStorage.LengthPrefix.ToString());
			}

			return prefix_bytes;
		}
		/// <summary>Encode any prefix related data for the <see cref="StringStorageType"/> into a byte array</summary>
		/// <param name="chars">The character array containing the set of characters to encode</param>
		/// <param name="charIndex">The index of the first character to encode</param>
		/// <param name="charCount">The number of characters to encode</param>
		/// <param name="bytes">The byte array to contain the resulting sequence of bytes</param>
		/// <param name="byteIndex">The index at which to start writing the resulting sequence of bytes</param>
		/// <returns>Number of prefix bytes written into <paramref name="bytes"/></returns>
		int EncodeStringStorageTypePrefixData(
			[SuppressMessage("Microsoft.Design", "CA1801:ReviewUnusedParameters")]
			char[] chars,
			[SuppressMessage("Microsoft.Design", "CA1801:ReviewUnusedParameters")]
			int charIndex,
			int charCount, byte[] bytes, int byteIndex)
		{
			switch (mStorage.Type)
			{
				// No prefix for CString
				case StringStorageType.CString:		return 0;
				case StringStorageType.Pascal:		return EncStringStorageTypePrefixPascalData(charCount, bytes, byteIndex);
				// CharArray doesn't do anything anyway
				case StringStorageType.CharArray:	return 0;
				default:
					throw new Debug.UnreachableException(mStorage.Type.ToString());
			}
		}
		#endregion

		#region Encode StringStorageType Data Postfix
		int EncStringStoragePostfixCStringData(byte[] bytes, int byteIndex)
		{
			for (int x = byteIndex; x < mNullCharacterSize; x++)
				bytes[x] = 0;

			return mNullCharacterSize; // number of bytes written into [bytes]
		}
		/// <summary>Encode any additional <see cref="StringStorageType"/> related data into a byte array</summary>
		/// <param name="chars">The character array containing the set of characters to encode</param>
		/// <param name="charIndex">The index of the first character to encode</param>
		/// <param name="charCount">The number of characters to encode</param>
		/// <param name="bytes">The byte array to contain the resulting sequence of bytes</param>
		/// <param name="byteIndex">The index at which to start writing the resulting sequence of postfix bytes</param>
		/// <returns>The actual number of bytes written into <paramref name="bytes"/></returns>
		int EncodeStringStorageTypePostfixData(
			[SuppressMessage("Microsoft.Design", "CA1801:ReviewUnusedParameters")]
			char[] chars,
			[SuppressMessage("Microsoft.Design", "CA1801:ReviewUnusedParameters")]
			int charIndex,
			[SuppressMessage("Microsoft.Design", "CA1801:ReviewUnusedParameters")]
			int charCount,
			byte[] bytes, int byteIndex)
		{
			switch (mStorage.Type)
			{
				case StringStorageType.CString:		return EncStringStoragePostfixCStringData(bytes, byteIndex);
				// No postfix for Pascal
				case StringStorageType.Pascal:		return 0;
				// CharArray doesn't do anything anyway
				case StringStorageType.CharArray:	return 0;
				default:
					throw new Debug.UnreachableException(mStorage.Type.ToString());
			}
		}
		#endregion

		/// <summary>Converts a set of characters into a sequence of bytes.</summary>
		class Encoder : System.Text.Encoder
		{
			StringStorageEncoding mEncoding;
			System.Text.Encoder mEnc;
			public Encoder(StringStorageEncoding enc) { mEncoding = enc; mEnc = enc.mBaseEncoding.GetEncoder(); }

			/// <summary>
			/// Calculates the number of bytes produced by encoding a set of characters from the specified character array.
			/// A parameter indicates whether to clear the internal state of the encoder after the calculation.
			/// </summary>
			/// <param name="chars">The character array containing the set of characters to encode</param>
			/// <param name="index">The index of the first character to encode</param>
			/// <param name="count">The number of characters to encode</param>
			/// <param name="flush"><b>true</b> to simulate clearing the internal state of the encoder after the calculation; otherwise, <b>false</b></param>
			/// <returns>The number of bytes produced by encoding the specified characters and any characters in the internal buffer</returns>
			/// <seealso cref="System.Text.Encoder.GetByteCount(Char[], Int32, Int32, Boolean) "/>
			public override int GetByteCount(char[] chars, int index, int count, bool flush)
			{
				int byte_count = mEnc.GetByteCount(chars, index, count, mEncoding.DontAlwaysFlush ? flush : true);

				byte_count = mEncoding.CalculateByteCount(byte_count); // Add our String Storage calculations

				return byte_count;
			}

			/// <summary>
			/// Encodes a set of characters from the specified character array and any characters in the internal buffer into the specified byte array.
			/// A parameter indicates whether to clear the internal state of the encoder after the conversion.
			/// </summary>
			/// <param name="chars">The character array containing the set of characters to encode</param>
			/// <param name="charIndex">The index of the first character to encode</param>
			/// <param name="charCount">The number of characters to encode</param>
			/// <param name="bytes">The byte array to contain the resulting sequence of bytes</param>
			/// <param name="byteIndex">The index at which to start writing the resulting sequence of bytes</param>
			/// <param name="flush">true to clear the internal state of the encoder after the conversion; otherwise, false</param>
			/// <returns>The actual number of bytes written into <paramref name="bytes"/></returns>
			/// <seealso cref="System.Text.Encoder.GetBytes(Char[], Int32, Int32, Byte[], Int32, Boolean) "/>
			public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex, bool flush)
			{
				// Add our String Storage calculations
				int bytes_written = mEncoding.EncodeStringStorageTypePrefixData(chars, charCount, charCount, bytes, byteIndex);

				bytes_written += mEnc.GetBytes(chars, charIndex, charCount, bytes, byteIndex + bytes_written, mEncoding.DontAlwaysFlush ? flush : true);

				// Add our String Storage calculations
				bytes_written += mEncoding.EncodeStringStorageTypePostfixData(chars, charIndex, charCount, bytes, bytes_written);

				return bytes_written;
			}

			/// <summary>Sets the encoder back to its initial state</summary>
			public override void Reset()	{ mEnc.Reset(); }
		};

		#region WriteString
		internal void WriteString(IO.BitStream s, string value, int maxLength = -1, int prefixBitLength = -1)
		{
			if (prefixBitLength > 0)
				throw new NotSupportedException("Currently don't support unnatural bit lengths for prefixes on writes");

			int length = value.Length;
			if (maxLength > 0)
				length = System.Math.Min(maxLength, length);

			char[] chars = value.ToCharArray(0, length);
			byte[] bytes = GetBytes(chars);
			s.Write(bytes);
		}
		#endregion
	};
}
