using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Text
{
	using Memory.Strings;

	/// <summary>Encapsulates an <see cref="Encoding"/> from a <see cref="StringStorage"/> definition</summary>
	/// <remarks>
	/// For <see cref="StringStorageType.CString"/> cases, the encoding does not check if there
	/// is an existing '\0' character in the user supplied strings. If you pass such strings to
	/// the encoding for streaming the results will be undefined. Oh and bad.
	/// </remarks>
	[SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes")]
	public sealed partial class StringStorageEncoding
		: Encoding
		, IEquatable<StringStorageEncoding>, IEqualityComparer<StringStorageEncoding>
		, IComparer<StringStorageEncoding>, IComparable<StringStorageEncoding>
	{
		Encoding mBaseEncoding;
		#region Storage
		StringStorage mStorage;
		/// <summary>The string storage definition for this encoding</summary>
		public StringStorage Storage { get => this.mStorage; }
		#endregion
		Options mOptions;
		bool DontAlwaysFlush { get => (this.mOptions & Options.DontAlwaysFlush) != 0; }
		/// <summary>Number of bytes a null character consumes</summary>
		int mNullCharacterSize;
		/// <summary>
		/// Number of bytes used to store a fixed length character array using
		/// the <see cref="StringStorageType"/> defined in <see cref="storage"/>
		/// </summary>
		int mFixedLengthByteLength;

		#region Ctor
		/// <summary>Initialize an encoding for this library's methods of String Storages</summary>
		/// <param name="storage">Storage definition</param>
		/// <param name="options"><see cref="System.Text.Encoding"/> options</param>
		public StringStorageEncoding(StringStorage storage, Options options = 0)
		{
			bool use_bom = (options & Options.UseByteOrderMark) != 0;
			bool big_endian = storage.ByteOrder == Shell.EndianFormat.Big;
			bool throw_on_invalid = (options & Options.ThrowOnInvalidBytes) != 0;

			this.mNullCharacterSize = sizeof(byte); // The majority of encodings only require 1 byte for the null character
			switch (storage.WidthType)
			{
				case StringStorageWidthType.Ascii:
					this.mBaseEncoding = ASCII;
					break;
				case StringStorageWidthType.Unicode:
					this.mBaseEncoding = new UnicodeEncoding(big_endian, use_bom, throw_on_invalid);
					this.mNullCharacterSize = UnicodeEncoding.CharSize; // #REVIEW: should we really do this?
					break;
				case StringStorageWidthType.UTF7:
					this.mBaseEncoding = new UTF7Encoding(!throw_on_invalid);
					break;
				case StringStorageWidthType.UTF8:
					this.mBaseEncoding = new UTF8Encoding(use_bom, throw_on_invalid);
					break;
				case StringStorageWidthType.UTF32:
					this.mBaseEncoding = new UTF32Encoding(big_endian, use_bom, throw_on_invalid);
					this.mNullCharacterSize = 4 * sizeof(byte);
					break;

				default: throw new Debug.UnreachableException();
			}

			this.mFixedLengthByteLength = !storage.IsFixedLength
				? 0
				: this.GetMaxCleanByteCount(storage.FixedLength);

			this.mStorage = storage;
			this.mOptions = options;
		}

		public override object Clone()
		{
			return new StringStorageEncoding(this.mStorage, this.mOptions);
		}
		#endregion

		#region Implementation
		/// <summary>Calculates the number of bytes produced by encoding a set of characters from the specified character array</summary>
		/// <param name="chars">The character array containing the set of characters to encode</param>
		/// <param name="index">The index of the first character to encode</param>
		/// <param name="count">The number of characters to encode</param>
		/// <returns>The number of bytes produced by encoding the specified characters</returns>
		public override int GetByteCount(char[] chars, int index, int count)
		{
			// In case someone is trying to encode a string outside of the storage's bounds
			this.ClampCharCount(ref count);

			int byte_count = this.mBaseEncoding.GetByteCount(chars, index, count);

			byte_count = this.CalculateByteCount(byte_count); // Add our String Storage calculations

			return byte_count;
		}
		/// <summary>Encodes a set of characters from the specified character array into the specified byte array</summary>
		/// <param name="chars">The character array containing the set of characters to encode</param>
		/// <param name="charIndex">The index of the first character to encode</param>
		/// <param name="charCount">The number of characters to encode</param>
		/// <param name="bytes">The byte array to contain the resulting sequence of bytes</param>
		/// <param name="byteIndex">The index at which to start writing the resulting sequence of bytes</param>
		/// <returns>The actual number of bytes written into bytes</returns>
		public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
		{
			// In case someone is trying to encode a string outside of the storage's bounds
			this.ClampCharCount(ref charCount);

			// Add our String Storage calculations
			int bytes_written = this.EncodeStringStorageTypePrefixData(chars, charIndex, charCount, bytes, byteIndex);

			bytes_written += this.mBaseEncoding.GetBytes(chars, charIndex, charCount, bytes, byteIndex + bytes_written);

			// Add our String Storage calculations
			bytes_written += this.EncodeStringStorageTypePostfixData(chars, charIndex, charCount, bytes, bytes_written);

			return bytes_written;
		}
		/// <summary>Calculates the number of characters produced by decoding a sequence of bytes from the specified byte array</summary>
		/// <param name="bytes">The byte array containing the sequence of bytes to decode</param>
		/// <param name="index">The index of the first byte to decode</param>
		/// <param name="count">The number of bytes to decode</param>
		/// <returns>The number of characters produced by decoding the specified sequence of bytes.</returns>
		public override int GetCharCount(byte[] bytes, int index, int count)
		{
			count = this.CalculateCharByteCount(bytes, ref index, count); // Remove our String Storage calculations

			int char_count = this.mBaseEncoding.GetCharCount(bytes, index, count);

			return char_count;
		}
		/// <summary>Decodes a sequence of bytes from the specified byte array into the specified character array</summary>
		/// <param name="bytes">The byte array containing the sequence of bytes to decode</param>
		/// <param name="byteIndex">The index of the first byte to decode</param>
		/// <param name="byteCount">The number of bytes to decode</param>
		/// <param name="chars">The character array to contain the resulting set of characters.</param>
		/// <param name="charIndex">The index at which to start writing the resulting set of characters</param>
		/// <returns>The actual number of characters written into chars</returns>
		public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
		{
			byteCount = this.CalculateCharByteCount(bytes, ref byteIndex, byteCount); // Remove our String Storage calculations

			int chars_written = this.mBaseEncoding.GetChars(bytes, byteIndex, byteCount, chars, charIndex);

			return chars_written;
		}
		/// <summary>Calculates the maximum number of bytes produced by encoding the specified number of characters</summary>
		/// <param name="charCount">The number of characters to encode</param>
		/// <returns>The maximum number of bytes produced by encoding the specified number of characters</returns>
		public override int GetMaxByteCount(int charCount)
		{
			int max_count = this.mBaseEncoding.GetMaxByteCount(charCount);

			max_count = this.CalculateByteCount(max_count); // Add our String Storage calculations

			return max_count;
		}
		/// <summary>
		/// Calculates the maximum number of bytes produced by encoding the specified number of characters WITHOUT
		/// THE EXTRA SURROGATE JIZZ
		/// </summary>
		/// <param name="charCount">The number of characters to encode</param>
		/// <returns></returns>
		public int GetMaxCleanByteCount(int charCount)
		{
			int max_count = this.mBaseEncoding.GetMaxByteCount(charCount);

			// NOTE: that GetMaxByteCount considers potential leftover surrogates from a previous decoder operation.
			// Because of the decoder, passing a value of 1 to the method retrieves 2 for a single-byte encoding,
			// such as ASCII. Your application should use the IsSingleByte property if this information is necessary.
			// ...That being said, it looks like they internally use a null character. So for streaming related cases,
			// we have to circumcise the fucking count.
			max_count -= this.mNullCharacterSize;

			return max_count;
		}
		/// <summary>Calculates the maximum number of characters produced by decoding the specified number of bytes</summary>
		/// <param name="byteCount">The number of bytes to decode</param>
		/// <returns>The maximum number of characters produced by decoding the specified number of bytes</returns>
		public override int GetMaxCharCount(int byteCount)
		{
			// For Pascal type strings, this will give a larger count
			// than usual, even for Max standards, since we can't
			// sneak a peak at the length prefix bytes
			byteCount = this.CalculateCharByteCount(byteCount); // Remove our String Storage calculations

			int max_count = this.mBaseEncoding.GetMaxCharCount(byteCount);

			return max_count;
		}
		// #REVIEW: override?
		//public override string ToString()		{ return mBaseEncoding.ToString(); }
		#endregion

		#region overrides to baseEncoding
		public override string BodyName			=> this.mBaseEncoding.BodyName;
		public override int CodePage			=> this.mBaseEncoding.CodePage;
		public override string EncodingName		=> this.mBaseEncoding.EncodingName;
		public override string HeaderName		=> this.mBaseEncoding.HeaderName;
		public override bool IsBrowserDisplay	=> this.mBaseEncoding.IsBrowserDisplay;
		public override bool IsBrowserSave		=> this.mBaseEncoding.IsBrowserSave;
		public override bool IsMailNewsDisplay	=> this.mBaseEncoding.IsMailNewsDisplay;
		public override bool IsMailNewsSave		=> this.mBaseEncoding.IsMailNewsSave;
		public override bool IsSingleByte		=> this.mBaseEncoding.IsSingleByte;
		public override string WebName			=> this.mBaseEncoding.WebName;
		public override int WindowsCodePage		=> this.mBaseEncoding.WindowsCodePage;

		/// <summary>Compares this to another object testing for equality</summary>
		/// <param name="obj"></param>
		/// <returns>
		/// True if both this object and <paramref name="obj"/> are equal.
		/// False if <paramref name="obj"/> is not a <see cref="StringStorageEncoding"/></returns>
		public override bool Equals(object value)
		{
			//return mBaseEncoding.Equals(value);
			if (value is StringStorageEncoding e)
				return this.Equals(e);

			return false;
		}
		public override System.Text.Decoder GetDecoder() => new Decoder(this);
		public override System.Text.Encoder GetEncoder() => new Encoder(this);
		public override int GetHashCode() => this.mBaseEncoding.GetHashCode();
		public override byte[] GetPreamble() => this.mBaseEncoding.GetPreamble();
		public override bool IsAlwaysNormalized(NormalizationForm form) => this.mBaseEncoding.IsAlwaysNormalized(form);
		#endregion

		#region IEquatable<StringStorageEncoding> Members
		/// <summary>
		/// Compares this to another <see cref="StringStorageEncoding"/> object testing
		/// their underlying fields for equality
		/// </summary>
		/// <param name="obj">other <see cref="StringStorageEncoding"/> object</param>
		/// <returns>true if both this object and <paramref name="obj"/> are equal</returns>
		public bool Equals(StringStorageEncoding other)
		{
			return this.mOptions == other.mOptions &&
			       this.mStorage.Equals(other.mStorage);
		}

		public bool Equals(StringStorageEncoding x, StringStorageEncoding y) => x.Equals(y);

		public int GetHashCode(StringStorageEncoding obj) => obj.GetHashCode();
		#endregion

		#region IComparer<StringStorageEncoding> Members
		/// <summary>Compare two <see cref="StringStorageEncoding"/> objects for similar underlying values</summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public int Compare(StringStorageEncoding x, StringStorageEncoding y) => x.CompareTo(y);
		/// <summary>Compare this with another <see cref="StringStorageEncoding"/> object for similar underlying values</summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public int CompareTo(StringStorageEncoding other)
		{
			int cmp = this.mStorage.CompareTo(other.mStorage);

			if (cmp == 0)
				return ((int) this.mOptions) - ((int)other.mOptions);

			return cmp;
		}
		#endregion


		#region Static encodings
		internal static readonly StringStorageEncoding[] kStorageEncodingList = EncodingArrayFromStorageArray(StringStorage.kStorageTypesList);
		static StringStorageEncoding[] EncodingArrayFromStorageArray(StringStorage[] storageArray)
		{
			var encodings = new StringStorageEncoding[storageArray.Length];
			for (int x = 0; x < encodings.Length; x++)
				encodings[x] = new StringStorageEncoding(storageArray[x]);
			return encodings;
		}

		/// <summary>
		/// Try and get an existing <b>static</b> <see cref="StringStorageEncoding"/> instance
		/// based on a provided definition
		/// </summary>
		/// <param name="storageDesc">Storage to base the result on</param>
		/// <returns>
		/// If an instance is found with <paramref name="storageDesc"/>, a static based
		/// object will be returned. Otherwise, a new <see cref="StringStorageEncoding"/>
		/// object will be created using the definition.
		/// </returns>
		public static StringStorageEncoding TryAndGetStaticEncoding(StringStorage storageDesc)
		{
			Contract.Ensures(Contract.Result<StringStorageEncoding>() != null);

			StringStorageEncoding sse = Array.Find(kStorageEncodingList,
				x => x.mStorage.Equals(storageDesc));

			return sse ?? new StringStorageEncoding(storageDesc);
		}
		#endregion
	};
}
