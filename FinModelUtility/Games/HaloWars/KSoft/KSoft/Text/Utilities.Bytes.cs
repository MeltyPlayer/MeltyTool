using System;
using System.IO;
using System.Text;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Text
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1724:TypeNamesShouldNotMatchNamespaces",
		Justification="I don't care about System.Web.Util")]
	partial class Util
	{
		/// <summary>Determine the string <see cref="Encoding"/> based on the byte-order-marks in a buffer</summary>
		/// <param name="buffer">Buffer containing the BOMs</param>
		/// <param name="index">Start index of the BOMs</param>
		/// <returns>The respected <see cref="Encoding"/> to use or <see cref="Encoding.Default"/> if this was unable to determine</returns>
		public static Encoding DetermineStringEncoding(byte[] buffer
			, int index = 0)
		{
			Contract.Requires<ArgumentNullException>(buffer != null);
			Contract.Requires<ArgumentOutOfRangeException>(index >= 0);

			Contract.Ensures(Contract.Result<Encoding>() != null);

			Encoding enc = null;
			int length = buffer.Length - index;

			/*
				FF FE			UTF-16, little-endian
				FE FF			UTF-16, big-endian
				2B 2F 76		UTF-7
				EF BB BF		UTF-8
				FF FE 00 00		UTF-32, little-endian
				00 00 FE FF		UTF-32, big-endian
			 */

			if (length >= 4)
			{
				byte b0 = buffer[index+0], b1 = buffer[index+1], b2 = buffer[index+2], b3 = buffer[index+3];

				if (b0 == 0x00 && b1 == 0x00 && b2 == 0xFE && b3 == 0xFF)
					enc = Encoding.UTF32;
			}
			if (enc == null && length >= 3)
			{
				byte b0 = buffer[index+0], b1 = buffer[index+1], b2 = buffer[index+2];

				// NOTE: Our UTF-7 detection doesn't test a 4th byte for 0x38,0x39,0x2B or 0x2F.
				// One of those are suppose to follow in UTF7 BOMs
				if (b0 == 0x2B && b1 == 0x2F && b2 == 0x76)
					enc = Encoding.UTF7;
				else if (b0 == 0xEF && b1 == 0xBB && b2 == 0xBF)
					enc = Encoding.UTF8;
			}
			if (enc == null && length >= 2)
			{
				byte b0 = buffer[index+0], b1 = buffer[index+1];

				if		(b0 == 0xFF && b1 == 0xFE)
						 enc = Encoding.Unicode;
				else if	(b0 == 0xFE && b1 == 0xFF)
						 enc = Encoding.BigEndianUnicode;
			}
			if (enc == null)
				enc = Encoding.Default;

			return enc;
		}

		#region Byte arrays
		public const int kDefaultHexDigitsPerLine = 16;

		// #REVIEW: Instead of doing byte.ToString("X2") we could just have a lookup table...

		#region ByteArrayToString (byte[] to string)
		/// <summary>Converts an array of bytes to a hex string</summary>
		/// <param name="data">Buffer of bytes to convert</param>
		/// <param name="startIndex">Index in <paramref name="data"/> to start the conversion</param>
		/// <param name="count">Number of bytes to convert</param>
		/// <example>"1337BEEF"</example>
		/// <returns></returns>
		public static string ByteArrayToString(byte[] data, int startIndex, int count)
		{
			Contract.Requires<ArgumentNullException>(data != null);
			Contract.Requires(startIndex >= 0);
			Contract.Requires(startIndex < data.Length);
			Contract.Requires(count > 0);
			Contract.Requires((startIndex+count) <= data.Length);

			Contract.Ensures(Contract.Result<string>() != null);

			StringBuilder sb = new StringBuilder(count * 2);
			for (int x = startIndex; x < (startIndex+count); x++)
				sb.Append(data[x].ToString("X2", KSoft.Util.InvariantCultureInfo));

			return sb.ToString();
		}
		/// <summary>Converts an array of bytes to a hex string and outputs it to the stream</summary>
		/// <param name="data">Buffer of bytes to convert</param>
		/// <param name="stream">Stream to output the hex string to</param>
		/// <param name="startIndex">Index in <paramref name="data"/> to start the conversion</param>
		/// <param name="count">Number of bytes to convert</param>
		/// <example>"1337BEEF"</example>
		public static void ByteArrayToStream(byte[] data, TextWriter stream, int startIndex, int count)
		{
			Contract.Requires<ArgumentNullException>(data != null);
			Contract.Requires<ArgumentNullException>(stream != null);
			Contract.Requires(startIndex >= 0);
			Contract.Requires(startIndex < data.Length);
			Contract.Requires(count > 0);
			Contract.Requires((startIndex+count) <= data.Length);

			for (int x = startIndex; x < (startIndex+count); x++)
				stream.Write(data[x].ToString("X2", KSoft.Util.InvariantCultureInfo));
		}
		/// <summary>Converts an array of bytes to a hex string</summary>
		/// <param name="data">Buffer of bytes to convert</param>
		/// <param name="startIndex">Index in <paramref name="data"/> to start the conversion</param>
		/// <example>"1337BEEF"</example>
		/// <returns></returns>
		public static string ByteArrayToString(byte[] data
			, int startIndex = 0)
		{
			Contract.Requires<ArgumentNullException>(data != null);
			Contract.Requires(startIndex >= 0);
			Contract.Requires(startIndex < data.Length);

			Contract.Ensures(Contract.Result<string>() != null);

			return ByteArrayToString(data, startIndex, data.Length-startIndex);
		}
		/// <summary>Converts an array of bytes to a hex string and outputs it to the stream</summary>
		/// <param name="data">Buffer of bytes to convert</param>
		/// <param name="startIndex">Index in <paramref name="data"/> to start the conversion</param>
		/// <example>"1337BEEF"</example>
		/// <returns></returns>
		public static void ByteArrayToStream(byte[] data, TextWriter stream
			, int startIndex = 0)
		{
			Contract.Requires<ArgumentNullException>(data != null);
			Contract.Requires<ArgumentNullException>(stream != null);
			Contract.Requires(startIndex >= 0);
			Contract.Requires(startIndex < data.Length);

			ByteArrayToStream(data, stream, startIndex, data.Length-startIndex);
		}
		#endregion

		#region ByteStringToArray (string to byte[])
		public static byte[] ByteStringToArray(byte[] bytes, string data, int startIndex, int count)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(data));
			Contract.Requires(startIndex >= 0);
			Contract.Requires(startIndex < data.Length);
			Contract.Requires(count > 0);
			Contract.Requires((startIndex+count) <= data.Length);
			Contract.Requires(
				( ((data.Length-startIndex)-count) % 2) == 0,
				"Can't byte-ify a string that's not even!"
			);
			Contract.Requires<ArgumentNullException>(bytes != null);
			Contract.Requires(bytes.Length >= (count/2));

			Contract.Ensures(Contract.Result<byte[]>() != null);

			Array.Clear(bytes, 0, bytes.Length);

			for ( int x = startIndex, index = 0
				; x < (startIndex+count)
				; x+=2, index++)
			{
				bytes[index] = (byte)CharsToByte(NumeralBase.Hex, data, x);
			}

			return bytes;
		}

		public static byte[] ByteStringToArray(byte[] bytes, string data
			, int startIndex = 0)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(data));
			Contract.Requires(startIndex >= 0);
			Contract.Requires(startIndex < data.Length);
			Contract.Requires(
				((data.Length-startIndex) % 2) == 0,
				"Can't byte-ify a string that's not even!"
			);
			Contract.Requires<ArgumentNullException>(bytes != null);
			Contract.Requires(bytes.Length >= (data.Length/2));

			Contract.Ensures(Contract.Result<byte[]>() != null);

			return ByteStringToArray(bytes, data, startIndex, data.Length-startIndex);
		}

		/// <summary>Converts a string containing hex values into a byte array</summary>
		/// <param name="data">String of hex digits to convert</param>
		/// <param name="startIndex">Character index in <paramref name="data"/> to start the conversion at</param>
		/// <param name="count">Number of characters to convert</param>
		/// <returns></returns>
		public static byte[] ByteStringToArray(string data, int startIndex, int count)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(data));
			Contract.Requires(startIndex >= 0);
			Contract.Requires(startIndex < data.Length);
			Contract.Requires(count > 0);
			Contract.Requires((startIndex+count) <= data.Length);
			Contract.Requires(
				( ((data.Length-startIndex)-count) % 2) == 0,
				"Can't byte-ify a string that's not even!"
			);

			Contract.Ensures(Contract.Result<byte[]>() != null);

			byte[] bytes = new byte[count / 2];
			return ByteStringToArray(bytes, data, startIndex, count);
		}
		/// <summary>Converts a string containing hex values into a byte array</summary>
		/// <param name="data">String of hex digits to convert</param>
		/// <param name="startIndex">Character index in <paramref name="data"/> to start the conversion at</param>
		/// <returns></returns>
		public static byte[] ByteStringToArray(string data
			, int startIndex = 0)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(data));
			Contract.Requires(startIndex >= 0);
			Contract.Requires(startIndex < data.Length);
			Contract.Requires(
				((data.Length-startIndex) % 2) == 0,
				"Can't byte-ify a string that's not even!"
			);

			Contract.Ensures(Contract.Result<byte[]>() != null);

			return ByteStringToArray(data, startIndex, data.Length-startIndex);
		}
		#endregion

		/// <summary>Convert an array of bytes into a formatted hex string</summary>
		/// <param name="data">Buffer of bytes to convert</param>
		/// <param name="padding">Padding string to appear before each line of hex characters</param>
		/// <param name="digitsPerLine">Number of hex characters per line</param>
		/// <returns></returns>
		/// <remarks>Uses <see cref="System.Environment.NewLine"/> for line termination</remarks>
		public static string ByteArrayToAlignedString(byte[] data
			, string padding = ""
			, int digitsPerLine = kDefaultHexDigitsPerLine)
		{
			Contract.Requires<ArgumentNullException>(data != null);
			Contract.Requires<ArgumentNullException>(padding != null);
			Contract.Requires(digitsPerLine >= 2);
			Contract.Requires((digitsPerLine % 2) == 0);

			Contract.Ensures(Contract.Result<string>() != null);

			string new_line = Environment.NewLine;

			int blocks = data.Length / digitsPerLine;
			int leftovers = data.Length % digitsPerLine;

			StringBuilder sb = new StringBuilder(
				(data.Length * 2) +
				(new_line.Length * blocks) + // calculate how many new line characters we'll need
				(padding.Length * (leftovers == 0 ? blocks : blocks + 1)) // calculate how many characters the padding on each line will take
			);

			int index = 0;
			for (int b = 0; b < blocks; b++, index+=digitsPerLine)
				sb.AppendFormat(KSoft.Util.InvariantCultureInfo, "{0}{1}{2}", padding, ByteArrayToString(data, index, digitsPerLine), new_line);

			if (leftovers > 0)
				sb.AppendFormat(KSoft.Util.InvariantCultureInfo, "{0}{1}{2}", padding, ByteArrayToString(data, index), new_line);

			return sb.ToString();
		}
		/// <summary>Convert an array of bytes into a formatted hex string and output it to the stream</summary>
		/// <param name="data">Buffer of bytes to convert</param>
		/// <param name="output">Stream to output the hex strings to</param>
		/// <param name="padding">Padding string to appear before each line of hex characters. Can be null.</param>
		/// <param name="digitsPerLine">Number of hex characters per line</param>
		public static void ByteArrayToAlignedOutput(byte[] data, TextWriter output
			, string padding = null
			, int digitsPerLine = kDefaultHexDigitsPerLine)
		{
			Contract.Requires<ArgumentNullException>(data != null);
			Contract.Requires(digitsPerLine >= 2);
			Contract.Requires((digitsPerLine % 2) == 0);

			int blocks = data.Length / digitsPerLine;
			int leftovers = data.Length % digitsPerLine;

			int index = 0;
			for (int b = 0; b < blocks; b++, index += digitsPerLine)
			{
				if(!string.IsNullOrEmpty(padding))
					output.Write(padding);
				output.WriteLine(ByteArrayToString(data, index, digitsPerLine));
			}

			if (leftovers > 0)
			{
				if (!string.IsNullOrEmpty(padding))
					output.Write(padding);
				output.WriteLine(ByteArrayToString(data, index));
			}
		}
		#endregion

		#region Char arrays
		/// <summary>Convert a radix into an acceptable NumberBase usable with the CharToX converters</summary>
		/// <param name="radix">Base we're converting from. Can be up to 36.</param>
		/// <returns>0 if <paramref name="radix"/> can't be converted</returns>
		public static NumeralBase ToAcceptableNumberBase(int radix) =>
			radix > 36 || radix < 0 ? 0 : (NumeralBase)radix;

		/// <summary>Convert the byte digit character to the byte value it represents</summary>
		/// <param name="c">Byte digit char up to base-36</param>
		/// <returns></returns>
		/// <remarks>Upper ('A') and lower ('a') case char digits map to the same int values</remarks>
		public static int CharToAnyDigit(char c)	=> kCharToByteLookup36[(byte)c];
		/// <summary>Checks if the byte digit character is any valid representable value</summary>
		/// <param name="c">Byte digit char up to base-36</param>
		/// <returns></returns>
		/// <remarks>Upper ('A') and lower ('a') case char digits map to the same int values</remarks>
		public static bool CharIsAnyDigit(char c)	=> kCharIsDigitLookup62[(byte)c];

		/// <summary>Convert the byte digit character to the byte value it represents</summary>
		/// <param name="c">Byte digit char up to base-62</param>
		/// <returns></returns>
		/// <remarks>"Extended" char digits map different int values for upper ('A') and lower ('a') case</remarks>
		public static int CharToAnyDigitExtended(char c)	=> kCharToByteLookup62[(byte)c];
		/// <summary>Checks if the byte digit character is any valid representable value</summary>
		/// <param name="c">Byte digit char up to base-62</param>
		/// <returns></returns>
		/// <remarks>"Extended" char digits map different int values for upper ('A') and lower ('a') case</remarks>
		public static bool CharIsAnyDigitExtended(char c)	=> kCharIsDigitLookup62[(byte)c];

		/// <summary>Convert the byte digit character to the byte value it represents</summary>
		/// <param name="c">Byte digit char up to base-16</param>
		/// <returns></returns>
		public static int CharToDigit(char c)		=> kCharToByteLookup16[(byte)c];
		/// <summary>Checks if the byte digit character is any valid representable value</summary>
		/// <param name="c">Byte digit char up to base-16</param>
		/// <returns></returns>
		public static bool CharIsDigit(char c)		=> kCharIsDigitLookup16[(byte)c];

		/// <summary>Convert a character into the byte digit it represents</summary>
		/// <param name="c">Character representing the byte digit</param>
		/// <param name="radix">The base we're converting from</param>
		/// <param name="place">base-0 position of the digit in the number string</param>
		/// <returns></returns>
		/// <remarks>Upper ('A') and lower ('a') case char digits map to the same int values</remarks>
		public static int CharToInt(char c, NumeralBase radix, int place)
		{
			int digit = CharToAnyDigit(c);
			int from_base = (int)radix;
			int multiplier = (int)Math.Pow(from_base, place);

			return digit * multiplier;
		}
		/// <summary>Convert a character into the byte digit it represents</summary>
		/// <param name="c">Character representing the byte digit</param>
		/// <param name="radix">The base we're converting from</param>
		/// <param name="place">base-0 position of the digit in the number string</param>
		/// <returns></returns>
		/// <remarks>
		/// "Extended" char digits map different int values for upper ('A') and lower ('a') case.
		/// Does not validate that <paramref name="c"/> is within <paramref name="radix"/>'s range
		/// </remarks>
		public static int CharToIntExtended(char c, NumeralBase radix, int place)
		{
			int digit = CharToAnyDigitExtended(c);
			int from_base = (int)radix;
			int multiplier = (int)Math.Pow(from_base, place);

			return digit * multiplier;
		}

		/// <summary>Convert a byte digit character pair to the byte they represent</summary>
		/// <param name="radix">The base we're converting from</param>
		/// <param name="c2">Character in the 2nd position (when reading right to left)</param>
		/// <param name="c1">Character in the 1st position (when reading right to left)</param>
		/// <returns></returns>
		/// <remarks>Upper ('A') and lower ('a') case char digits map to the same int values</remarks>
		/// <example>
		/// int b = CharsToByte(NumeralBase.Hex, '3', 'F');
		/// b == 63;
		/// </example>
		public static int CharsToByte(NumeralBase radix, char c2, char c1)
		{
			Contract.Ensures(Contract.Result<int>() >= byte.MinValue);
			Contract.Ensures(Contract.Result<int>() <= byte.MaxValue);

			int value = 0;

			if (CharIsAnyDigit(c2) && CharIsAnyDigit(c1))
				value = CharToInt(c2, radix, 1) + CharToInt(c1, radix, 0);

			// Someone could supply a radix value that isn't technically a member of NumeralBase (eg, 36)
			// So we clamp it to a byte here
			return value > byte.MaxValue ? 0 : value;
		}

		/// <summary>Convert a byte digit character pair to the byte they represent</summary>
		/// <param name="radix">The base we're converting from</param>
		/// <param name="data">Buffer that holds the byte digit character pair</param>
		/// <param name="index">Index to start processing in <paramref name="data"/></param>
		/// <returns></returns>
		/// <remarks>Upper ('A') and lower ('a') case char digits map to the same int values</remarks>
		public static int CharsToByte(NumeralBase radix, char[] data, int index = 0)
		{
			Contract.Requires<ArgumentNullException>(data != null);
			Contract.Requires(index >= 0);
			Contract.Requires(index < data.Length);

			Contract.Ensures(Contract.Result<int>() >= byte.MinValue);
			Contract.Ensures(Contract.Result<int>() <= byte.MaxValue);

			return CharsToByte(radix, data[index], data[index+1]);
		}
		/// <summary>Convert a byte digit character pair to the byte they represent</summary>
		/// <param name="radix">The base we're converting from</param>
		/// <param name="data">Buffer that holds the byte digit character pair</param>
		/// <param name="index">Index to start processing in <paramref name="data"/></param>
		/// <returns></returns>
		/// <remarks>Upper ('A') and lower ('a') case char digits map to the same int values</remarks>
		/// <example>
		/// int b = CharsToByte(NumeralBase.Hex, "3F");
		/// b == 63;
		/// </example>
		public static int CharsToByte(NumeralBase radix, string data, int index = 0)
		{
			Contract.Requires<ArgumentNullException>(data != null);
			Contract.Requires(index >= 0);
			Contract.Requires(index < data.Length);

			Contract.Ensures(Contract.Result<int>() >= byte.MinValue);
			Contract.Ensures(Contract.Result<int>() <= byte.MaxValue);

			return CharsToByte(radix, data[index], data[index+1]);
		}
		#endregion
	};
}
