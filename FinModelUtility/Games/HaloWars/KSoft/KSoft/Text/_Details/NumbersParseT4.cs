using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft
{
	partial class Numbers
	{
		#region UInt32
		static bool TryParseImpl(string s, ref uint result, int radix, int startIndex, int length, string digits)
		{
			int pos = startIndex;
			int end = startIndex+length;
			bool success = true;

			if (radix == 16)
				if ((pos+2)<end && s[pos+0]=='0' && s[pos+1]=='x')
					pos += 2;

			// Skip any starting whitespace, avoids s.Trim() allocations
			while(pos < end && char.IsWhiteSpace(s[pos]))
				++pos;



			for(var radix_in_word = (uint)radix; pos < end && !char.IsWhiteSpace(s[pos]); ++pos)
			{
				char digit = s[pos];


				int x = digits.IndexOf(digit);
				if (x >= 0 && x < radix)
				{
					result *= radix_in_word;
					result += (uint)x;
				}
				else // Character wasn't found in the look-up table, it is invalid
				{
					success = false;
					break;
				}
			}


			return success;
		}
		public static bool TryParse(string s, out uint result, int radix = kBase10, int startIndex = 0, string digits = kBase64Digits)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(radix >= 2 && radix <= digits.Length);
			Contract.Requires<ArgumentOutOfRangeException>(startIndex >= 0);
			result = 0;

			return s != null && startIndex < s.Length &&
				TryParseImpl(s, ref result, radix, startIndex, s.Length, digits);
		}
		public static bool TryParse(string s, out uint result, NumeralBase radix = NumeralBase.Decimal, int startIndex = 0, string digits = kBase64Digits)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(IsValidLookupTable(radix, digits));
			Contract.Requires<ArgumentOutOfRangeException>(startIndex >= 0);
			result = 0;

			return s != null && startIndex < s.Length &&
				TryParseImpl(s, ref result, (int)radix, startIndex, s.Length, digits);
		}
		public static bool TryParseRange(string s, out uint result, int startIndex, int length, NumeralBase radix = NumeralBase.Decimal, string digits = kBase64Digits)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(IsValidLookupTable(radix, digits));
			Contract.Requires<ArgumentOutOfRangeException>(startIndex >= 0);
			Contract.Requires<ArgumentOutOfRangeException>(length >= 0);
			result = 0;

			return s != null && startIndex+length <= s.Length &&
				TryParseImpl(s, ref result, (int)radix, startIndex, length, digits);
		}
		#endregion
		#region Int32
		static bool TryParseImpl(string s, ref int result, int radix, int startIndex, int length, string digits)
		{
			int pos = startIndex;
			int end = startIndex+length;
			bool success = true;

			if (radix == 16)
				if ((pos+2)<end && s[pos+0]=='0' && s[pos+1]=='x')
					pos += 2;

			// Skip any starting whitespace, avoids s.Trim() allocations
			while(pos < end && char.IsWhiteSpace(s[pos]))
				++pos;


			bool negate = false;
			// Sign support only exist for decimal and lower bases
			if (radix <= kBase10 && pos < end)
			{
				char sign = s[pos];

				negate = sign == '-';
				// Skip the sign character
				if (negate || sign == '+')
					++pos;
			}

			for(var radix_in_word = (int)radix; pos < end && !char.IsWhiteSpace(s[pos]); ++pos)
			{
				char digit = s[pos];


				int x = digits.IndexOf(digit);
				if (x >= 0 && x < radix)
				{
					result *= radix_in_word;
					result += (int)x;
				}
				else // Character wasn't found in the look-up table, it is invalid
				{
					success = false;
					break;
				}
			}

			// Negate the result if anything was processed
			if (negate)
				result = -result;

			return success;
		}
		public static bool TryParse(string s, out int result, int radix = kBase10, int startIndex = 0, string digits = kBase64Digits)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(radix >= 2 && radix <= digits.Length);
			Contract.Requires<ArgumentOutOfRangeException>(startIndex >= 0);
			result = 0;

			return s != null && startIndex < s.Length &&
				TryParseImpl(s, ref result, radix, startIndex, s.Length, digits);
		}
		public static bool TryParse(string s, out int result, NumeralBase radix = NumeralBase.Decimal, int startIndex = 0, string digits = kBase64Digits)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(IsValidLookupTable(radix, digits));
			Contract.Requires<ArgumentOutOfRangeException>(startIndex >= 0);
			result = 0;

			return s != null && startIndex < s.Length &&
				TryParseImpl(s, ref result, (int)radix, startIndex, s.Length, digits);
		}
		public static bool TryParseRange(string s, out int result, int startIndex, int length, NumeralBase radix = NumeralBase.Decimal, string digits = kBase64Digits)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(IsValidLookupTable(radix, digits));
			Contract.Requires<ArgumentOutOfRangeException>(startIndex >= 0);
			Contract.Requires<ArgumentOutOfRangeException>(length >= 0);
			result = 0;

			return s != null && startIndex+length <= s.Length &&
				TryParseImpl(s, ref result, (int)radix, startIndex, length, digits);
		}
		#endregion
		#region UInt64
		static bool TryParseImpl(string s, ref ulong result, int radix, int startIndex, int length, string digits)
		{
			int pos = startIndex;
			int end = startIndex+length;
			bool success = true;

			if (radix == 16)
				if ((pos+2)<end && s[pos+0]=='0' && s[pos+1]=='x')
					pos += 2;

			// Skip any starting whitespace, avoids s.Trim() allocations
			while(pos < end && char.IsWhiteSpace(s[pos]))
				++pos;



			for(var radix_in_word = (ulong)radix; pos < end && !char.IsWhiteSpace(s[pos]); ++pos)
			{
				char digit = s[pos];


				int x = digits.IndexOf(digit);
				if (x >= 0 && x < radix)
				{
					result *= radix_in_word;
					result += (ulong)x;
				}
				else // Character wasn't found in the look-up table, it is invalid
				{
					success = false;
					break;
				}
			}


			return success;
		}
		public static bool TryParse(string s, out ulong result, int radix = kBase10, int startIndex = 0, string digits = kBase64Digits)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(radix >= 2 && radix <= digits.Length);
			Contract.Requires<ArgumentOutOfRangeException>(startIndex >= 0);
			result = 0;

			return s != null && startIndex < s.Length &&
				TryParseImpl(s, ref result, radix, startIndex, s.Length, digits);
		}
		public static bool TryParse(string s, out ulong result, NumeralBase radix = NumeralBase.Decimal, int startIndex = 0, string digits = kBase64Digits)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(IsValidLookupTable(radix, digits));
			Contract.Requires<ArgumentOutOfRangeException>(startIndex >= 0);
			result = 0;

			return s != null && startIndex < s.Length &&
				TryParseImpl(s, ref result, (int)radix, startIndex, s.Length, digits);
		}
		public static bool TryParseRange(string s, out ulong result, int startIndex, int length, NumeralBase radix = NumeralBase.Decimal, string digits = kBase64Digits)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(IsValidLookupTable(radix, digits));
			Contract.Requires<ArgumentOutOfRangeException>(startIndex >= 0);
			Contract.Requires<ArgumentOutOfRangeException>(length >= 0);
			result = 0;

			return s != null && startIndex+length <= s.Length &&
				TryParseImpl(s, ref result, (int)radix, startIndex, length, digits);
		}
		#endregion
		#region Int64
		static bool TryParseImpl(string s, ref long result, int radix, int startIndex, int length, string digits)
		{
			int pos = startIndex;
			int end = startIndex+length;
			bool success = true;

			if (radix == 16)
				if ((pos+2)<end && s[pos+0]=='0' && s[pos+1]=='x')
					pos += 2;

			// Skip any starting whitespace, avoids s.Trim() allocations
			while(pos < end && char.IsWhiteSpace(s[pos]))
				++pos;


			bool negate = false;
			// Sign support only exist for decimal and lower bases
			if (radix <= kBase10 && pos < end)
			{
				char sign = s[pos];

				negate = sign == '-';
				// Skip the sign character
				if (negate || sign == '+')
					++pos;
			}

			for(var radix_in_word = (long)radix; pos < end && !char.IsWhiteSpace(s[pos]); ++pos)
			{
				char digit = s[pos];


				int x = digits.IndexOf(digit);
				if (x >= 0 && x < radix)
				{
					result *= radix_in_word;
					result += (long)x;
				}
				else // Character wasn't found in the look-up table, it is invalid
				{
					success = false;
					break;
				}
			}

			// Negate the result if anything was processed
			if (negate)
				result = -result;

			return success;
		}
		public static bool TryParse(string s, out long result, int radix = kBase10, int startIndex = 0, string digits = kBase64Digits)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(radix >= 2 && radix <= digits.Length);
			Contract.Requires<ArgumentOutOfRangeException>(startIndex >= 0);
			result = 0;

			return s != null && startIndex < s.Length &&
				TryParseImpl(s, ref result, radix, startIndex, s.Length, digits);
		}
		public static bool TryParse(string s, out long result, NumeralBase radix = NumeralBase.Decimal, int startIndex = 0, string digits = kBase64Digits)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(IsValidLookupTable(radix, digits));
			Contract.Requires<ArgumentOutOfRangeException>(startIndex >= 0);
			result = 0;

			return s != null && startIndex < s.Length &&
				TryParseImpl(s, ref result, (int)radix, startIndex, s.Length, digits);
		}
		public static bool TryParseRange(string s, out long result, int startIndex, int length, NumeralBase radix = NumeralBase.Decimal, string digits = kBase64Digits)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(IsValidLookupTable(radix, digits));
			Contract.Requires<ArgumentOutOfRangeException>(startIndex >= 0);
			Contract.Requires<ArgumentOutOfRangeException>(length >= 0);
			result = 0;

			return s != null && startIndex+length <= s.Length &&
				TryParseImpl(s, ref result, (int)radix, startIndex, length, digits);
		}
		#endregion

		#region Byte
		static bool TryParseImpl(string s, ref byte result, int radix, int startIndex, int length, string digits)
		{
			uint word = 0;
			bool success = false;
			if (TryParseImpl(s, ref word, radix, startIndex, length, digits) &&
				word >= byte.MinValue && word <= byte.MaxValue)
			{
				result = (byte)word;
				success = true;
			}

			return success;
		}
		public static bool TryParse(string s, out byte result, int radix = kBase10, int startIndex = 0, string digits = kBase64Digits)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(radix >= 2 && radix <= digits.Length);
			Contract.Requires<ArgumentOutOfRangeException>(startIndex >= 0);
			result = 0;

			return  s != null && startIndex < s.Length &&
				TryParseImpl(s, ref result, radix, startIndex, s.Length, digits);
		}
		public static bool TryParse(string s, out byte result, NumeralBase radix = NumeralBase.Decimal, int startIndex = 0, string digits = kBase64Digits)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(IsValidLookupTable(radix, digits));
			Contract.Requires<ArgumentOutOfRangeException>(startIndex >= 0);
			result = 0;

			return  s != null && startIndex < s.Length &&
				TryParseImpl(s, ref result, (int)radix, startIndex, s.Length, digits);
		}
		public static bool TryParseRange(string s, out byte result, int startIndex, int length, NumeralBase radix = NumeralBase.Decimal, string digits = kBase64Digits)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(IsValidLookupTable(radix, digits));
			Contract.Requires<ArgumentOutOfRangeException>(startIndex >= 0);
			Contract.Requires<ArgumentOutOfRangeException>(length >= 0);
			result = 0;

			return s != null && startIndex+length <= s.Length &&
				TryParseImpl(s, ref result, (int)radix, startIndex, length, digits);
		}
		#endregion
		#region SByte
		static bool TryParseImpl(string s, ref sbyte result, int radix, int startIndex, int length, string digits)
		{
			int word = 0;
			bool success = false;
			if (TryParseImpl(s, ref word, radix, startIndex, length, digits) &&
				word >= sbyte.MinValue && word <= sbyte.MaxValue)
			{
				result = (sbyte)word;
				success = true;
			}

			return success;
		}
		public static bool TryParse(string s, out sbyte result, int radix = kBase10, int startIndex = 0, string digits = kBase64Digits)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(radix >= 2 && radix <= digits.Length);
			Contract.Requires<ArgumentOutOfRangeException>(startIndex >= 0);
			result = 0;

			return  s != null && startIndex < s.Length &&
				TryParseImpl(s, ref result, radix, startIndex, s.Length, digits);
		}
		public static bool TryParse(string s, out sbyte result, NumeralBase radix = NumeralBase.Decimal, int startIndex = 0, string digits = kBase64Digits)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(IsValidLookupTable(radix, digits));
			Contract.Requires<ArgumentOutOfRangeException>(startIndex >= 0);
			result = 0;

			return  s != null && startIndex < s.Length &&
				TryParseImpl(s, ref result, (int)radix, startIndex, s.Length, digits);
		}
		public static bool TryParseRange(string s, out sbyte result, int startIndex, int length, NumeralBase radix = NumeralBase.Decimal, string digits = kBase64Digits)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(IsValidLookupTable(radix, digits));
			Contract.Requires<ArgumentOutOfRangeException>(startIndex >= 0);
			Contract.Requires<ArgumentOutOfRangeException>(length >= 0);
			result = 0;

			return s != null && startIndex+length <= s.Length &&
				TryParseImpl(s, ref result, (int)radix, startIndex, length, digits);
		}
		#endregion
		#region UInt16
		static bool TryParseImpl(string s, ref ushort result, int radix, int startIndex, int length, string digits)
		{
			uint word = 0;
			bool success = false;
			if (TryParseImpl(s, ref word, radix, startIndex, length, digits) &&
				word >= ushort.MinValue && word <= ushort.MaxValue)
			{
				result = (ushort)word;
				success = true;
			}

			return success;
		}
		public static bool TryParse(string s, out ushort result, int radix = kBase10, int startIndex = 0, string digits = kBase64Digits)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(radix >= 2 && radix <= digits.Length);
			Contract.Requires<ArgumentOutOfRangeException>(startIndex >= 0);
			result = 0;

			return  s != null && startIndex < s.Length &&
				TryParseImpl(s, ref result, radix, startIndex, s.Length, digits);
		}
		public static bool TryParse(string s, out ushort result, NumeralBase radix = NumeralBase.Decimal, int startIndex = 0, string digits = kBase64Digits)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(IsValidLookupTable(radix, digits));
			Contract.Requires<ArgumentOutOfRangeException>(startIndex >= 0);
			result = 0;

			return  s != null && startIndex < s.Length &&
				TryParseImpl(s, ref result, (int)radix, startIndex, s.Length, digits);
		}
		public static bool TryParseRange(string s, out ushort result, int startIndex, int length, NumeralBase radix = NumeralBase.Decimal, string digits = kBase64Digits)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(IsValidLookupTable(radix, digits));
			Contract.Requires<ArgumentOutOfRangeException>(startIndex >= 0);
			Contract.Requires<ArgumentOutOfRangeException>(length >= 0);
			result = 0;

			return s != null && startIndex+length <= s.Length &&
				TryParseImpl(s, ref result, (int)radix, startIndex, length, digits);
		}
		#endregion
		#region Int16
		static bool TryParseImpl(string s, ref short result, int radix, int startIndex, int length, string digits)
		{
			int word = 0;
			bool success = false;
			if (TryParseImpl(s, ref word, radix, startIndex, length, digits) &&
				word >= short.MinValue && word <= short.MaxValue)
			{
				result = (short)word;
				success = true;
			}

			return success;
		}
		public static bool TryParse(string s, out short result, int radix = kBase10, int startIndex = 0, string digits = kBase64Digits)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(radix >= 2 && radix <= digits.Length);
			Contract.Requires<ArgumentOutOfRangeException>(startIndex >= 0);
			result = 0;

			return  s != null && startIndex < s.Length &&
				TryParseImpl(s, ref result, radix, startIndex, s.Length, digits);
		}
		public static bool TryParse(string s, out short result, NumeralBase radix = NumeralBase.Decimal, int startIndex = 0, string digits = kBase64Digits)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(IsValidLookupTable(radix, digits));
			Contract.Requires<ArgumentOutOfRangeException>(startIndex >= 0);
			result = 0;

			return  s != null && startIndex < s.Length &&
				TryParseImpl(s, ref result, (int)radix, startIndex, s.Length, digits);
		}
		public static bool TryParseRange(string s, out short result, int startIndex, int length, NumeralBase radix = NumeralBase.Decimal, string digits = kBase64Digits)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(IsValidLookupTable(radix, digits));
			Contract.Requires<ArgumentOutOfRangeException>(startIndex >= 0);
			Contract.Requires<ArgumentOutOfRangeException>(length >= 0);
			result = 0;

			return s != null && startIndex+length <= s.Length &&
				TryParseImpl(s, ref result, (int)radix, startIndex, length, digits);
		}
		#endregion

		#region ParseString Byte
		static bool ParseStringImpl(string s, ref byte value, bool noThrow, int radix, int startIndex
			, Text.IHandleTextParseError parseErrorHandler)
		{
			var result = string.IsNullOrEmpty(s)
				? ParseErrorType.NoInput
				: ParseErrorType.None;

			if (result != ParseErrorType.NoInput && (startIndex < 0 || startIndex >= s.Length))
				result = ParseErrorType.InvalidStartIndex;

			if (result == ParseErrorType.None)
				result = TryParse(s, out value, radix, startIndex, kBase64Digits)
					? ParseErrorType.None
					: ParseErrorType.InvalidValue;

			return HandleParseError(result, noThrow, s, startIndex, parseErrorHandler);
		}
		public static bool ParseString(string s, ref byte result, bool noThrow
			, Text.IHandleTextParseError parseErrorHandler = null, NumeralBase radix = NumeralBase.Decimal, int startIndex = 0)
		{
			Contract.Requires(IsValidLookupTable(radix, kBase64Digits));

			return ParseStringImpl(s, ref result, noThrow, (int)radix, startIndex, parseErrorHandler);
		}
		#endregion
		#region ParseString SByte
		static bool ParseStringImpl(string s, ref sbyte value, bool noThrow, int radix, int startIndex
			, Text.IHandleTextParseError parseErrorHandler)
		{
			var result = string.IsNullOrEmpty(s)
				? ParseErrorType.NoInput
				: ParseErrorType.None;

			if (result != ParseErrorType.NoInput && (startIndex < 0 || startIndex >= s.Length))
				result = ParseErrorType.InvalidStartIndex;

			if (result == ParseErrorType.None)
				result = TryParse(s, out value, radix, startIndex, kBase64Digits)
					? ParseErrorType.None
					: ParseErrorType.InvalidValue;

			return HandleParseError(result, noThrow, s, startIndex, parseErrorHandler);
		}
		public static bool ParseString(string s, ref sbyte result, bool noThrow
			, Text.IHandleTextParseError parseErrorHandler = null, NumeralBase radix = NumeralBase.Decimal, int startIndex = 0)
		{
			Contract.Requires(IsValidLookupTable(radix, kBase64Digits));

			return ParseStringImpl(s, ref result, noThrow, (int)radix, startIndex, parseErrorHandler);
		}
		#endregion
		#region ParseString UInt16
		static bool ParseStringImpl(string s, ref ushort value, bool noThrow, int radix, int startIndex
			, Text.IHandleTextParseError parseErrorHandler)
		{
			var result = string.IsNullOrEmpty(s)
				? ParseErrorType.NoInput
				: ParseErrorType.None;

			if (result != ParseErrorType.NoInput && (startIndex < 0 || startIndex >= s.Length))
				result = ParseErrorType.InvalidStartIndex;

			if (result == ParseErrorType.None)
				result = TryParse(s, out value, radix, startIndex, kBase64Digits)
					? ParseErrorType.None
					: ParseErrorType.InvalidValue;

			return HandleParseError(result, noThrow, s, startIndex, parseErrorHandler);
		}
		public static bool ParseString(string s, ref ushort result, bool noThrow
			, Text.IHandleTextParseError parseErrorHandler = null, NumeralBase radix = NumeralBase.Decimal, int startIndex = 0)
		{
			Contract.Requires(IsValidLookupTable(radix, kBase64Digits));

			return ParseStringImpl(s, ref result, noThrow, (int)radix, startIndex, parseErrorHandler);
		}
		#endregion
		#region ParseString Int16
		static bool ParseStringImpl(string s, ref short value, bool noThrow, int radix, int startIndex
			, Text.IHandleTextParseError parseErrorHandler)
		{
			var result = string.IsNullOrEmpty(s)
				? ParseErrorType.NoInput
				: ParseErrorType.None;

			if (result != ParseErrorType.NoInput && (startIndex < 0 || startIndex >= s.Length))
				result = ParseErrorType.InvalidStartIndex;

			if (result == ParseErrorType.None)
				result = TryParse(s, out value, radix, startIndex, kBase64Digits)
					? ParseErrorType.None
					: ParseErrorType.InvalidValue;

			return HandleParseError(result, noThrow, s, startIndex, parseErrorHandler);
		}
		public static bool ParseString(string s, ref short result, bool noThrow
			, Text.IHandleTextParseError parseErrorHandler = null, NumeralBase radix = NumeralBase.Decimal, int startIndex = 0)
		{
			Contract.Requires(IsValidLookupTable(radix, kBase64Digits));

			return ParseStringImpl(s, ref result, noThrow, (int)radix, startIndex, parseErrorHandler);
		}
		#endregion
		#region ParseString UInt32
		static bool ParseStringImpl(string s, ref uint value, bool noThrow, int radix, int startIndex
			, Text.IHandleTextParseError parseErrorHandler)
		{
			var result = string.IsNullOrEmpty(s)
				? ParseErrorType.NoInput
				: ParseErrorType.None;

			if (result != ParseErrorType.NoInput && (startIndex < 0 || startIndex >= s.Length))
				result = ParseErrorType.InvalidStartIndex;

			if (result == ParseErrorType.None)
				result = TryParse(s, out value, radix, startIndex, kBase64Digits)
					? ParseErrorType.None
					: ParseErrorType.InvalidValue;

			return HandleParseError(result, noThrow, s, startIndex, parseErrorHandler);
		}
		public static bool ParseString(string s, ref uint result, bool noThrow
			, Text.IHandleTextParseError parseErrorHandler = null, NumeralBase radix = NumeralBase.Decimal, int startIndex = 0)
		{
			Contract.Requires(IsValidLookupTable(radix, kBase64Digits));

			return ParseStringImpl(s, ref result, noThrow, (int)radix, startIndex, parseErrorHandler);
		}
		#endregion
		#region ParseString Int32
		static bool ParseStringImpl(string s, ref int value, bool noThrow, int radix, int startIndex
			, Text.IHandleTextParseError parseErrorHandler)
		{
			var result = string.IsNullOrEmpty(s)
				? ParseErrorType.NoInput
				: ParseErrorType.None;

			if (result != ParseErrorType.NoInput && (startIndex < 0 || startIndex >= s.Length))
				result = ParseErrorType.InvalidStartIndex;

			if (result == ParseErrorType.None)
				result = TryParse(s, out value, radix, startIndex, kBase64Digits)
					? ParseErrorType.None
					: ParseErrorType.InvalidValue;

			return HandleParseError(result, noThrow, s, startIndex, parseErrorHandler);
		}
		public static bool ParseString(string s, ref int result, bool noThrow
			, Text.IHandleTextParseError parseErrorHandler = null, NumeralBase radix = NumeralBase.Decimal, int startIndex = 0)
		{
			Contract.Requires(IsValidLookupTable(radix, kBase64Digits));

			return ParseStringImpl(s, ref result, noThrow, (int)radix, startIndex, parseErrorHandler);
		}
		#endregion
		#region ParseString UInt64
		static bool ParseStringImpl(string s, ref ulong value, bool noThrow, int radix, int startIndex
			, Text.IHandleTextParseError parseErrorHandler)
		{
			var result = string.IsNullOrEmpty(s)
				? ParseErrorType.NoInput
				: ParseErrorType.None;

			if (result != ParseErrorType.NoInput && (startIndex < 0 || startIndex >= s.Length))
				result = ParseErrorType.InvalidStartIndex;

			if (result == ParseErrorType.None)
				result = TryParse(s, out value, radix, startIndex, kBase64Digits)
					? ParseErrorType.None
					: ParseErrorType.InvalidValue;

			return HandleParseError(result, noThrow, s, startIndex, parseErrorHandler);
		}
		public static bool ParseString(string s, ref ulong result, bool noThrow
			, Text.IHandleTextParseError parseErrorHandler = null, NumeralBase radix = NumeralBase.Decimal, int startIndex = 0)
		{
			Contract.Requires(IsValidLookupTable(radix, kBase64Digits));

			return ParseStringImpl(s, ref result, noThrow, (int)radix, startIndex, parseErrorHandler);
		}
		#endregion
		#region ParseString Int64
		static bool ParseStringImpl(string s, ref long value, bool noThrow, int radix, int startIndex
			, Text.IHandleTextParseError parseErrorHandler)
		{
			var result = string.IsNullOrEmpty(s)
				? ParseErrorType.NoInput
				: ParseErrorType.None;

			if (result != ParseErrorType.NoInput && (startIndex < 0 || startIndex >= s.Length))
				result = ParseErrorType.InvalidStartIndex;

			if (result == ParseErrorType.None)
				result = TryParse(s, out value, radix, startIndex, kBase64Digits)
					? ParseErrorType.None
					: ParseErrorType.InvalidValue;

			return HandleParseError(result, noThrow, s, startIndex, parseErrorHandler);
		}
		public static bool ParseString(string s, ref long result, bool noThrow
			, Text.IHandleTextParseError parseErrorHandler = null, NumeralBase radix = NumeralBase.Decimal, int startIndex = 0)
		{
			Contract.Requires(IsValidLookupTable(radix, kBase64Digits));

			return ParseStringImpl(s, ref result, noThrow, (int)radix, startIndex, parseErrorHandler);
		}
		#endregion

		#region TryParse list Byte
		abstract class TryParseByteListBase<TListItem>
			: TryParseNumberListBase<
					byte,
					TListItem
				>
		{
			protected TryParseByteListBase(StringListDesc desc, string values)
				: base(desc, values)
			{
			}

			protected override IEnumerable<byte?> EmptyResult => Array.Empty<byte?>();

			protected byte? ProcessItem(int start, int length)
			{
				var result = (byte)0;
				bool success = TryParseImpl(this.mValues, ref result, (int) this.mDesc.Radix, start, length, this.mDesc.Digits);
				return success
					? result
					: (byte?)null;
			}
		};

		sealed class TryParseByteListAsync
			: TryParseByteListBase< Task<byte?> >
		{
			public TryParseByteListAsync(StringListDesc desc, string values)
				: base(desc, values)
			{
			}

			static byte? ProcessItemAsync(object state)
			{
				var args = (Tuple<TryParseByteListAsync, int, int>)state;
				var me = args.Item1;
				return me.ProcessItem(args.Item2, args.Item3);
			}
			protected override Task<byte?> CreateItem(int start, int length)
			{
				return Task<byte?>.Factory.StartNew(
						ProcessItemAsync,
						new Tuple<TryParseByteListAsync, int, int>(this, start, length)
					);
			}

			protected override IEnumerable<byte?> CreateResult()
			{
				return
					from task in this.mList
					select task.Result;
			}
		};
		public static IEnumerable<byte?> TryParseByteAsync(StringListDesc desc, string values)
		{
			return new TryParseByteListAsync(desc, values).TryParse();
		}

		sealed class TryParseByteList
			: TryParseByteListBase< byte? >
		{
			public TryParseByteList(StringListDesc desc, string values)
				: base(desc, values)
			{
			}

			protected override byte? CreateItem(int start, int length)
			{
				return this.ProcessItem(start, length);
			}

			protected override IEnumerable<byte?> CreateResult()
			{
				return this.mList;
			}
		};
		public static IEnumerable<byte?> TryParseByte(StringListDesc desc, string values)
		{
			return new TryParseByteList(desc, values).TryParse();
		}
		#endregion
		#region TryParse list SByte
		abstract class TryParseSByteListBase<TListItem>
			: TryParseNumberListBase<
					sbyte,
					TListItem
				>
		{
			protected TryParseSByteListBase(StringListDesc desc, string values)
				: base(desc, values)
			{
			}

			protected override IEnumerable<sbyte?> EmptyResult => Array.Empty<sbyte?>();

			protected sbyte? ProcessItem(int start, int length)
			{
				var result = (sbyte)0;
				bool success = TryParseImpl(this.mValues, ref result, (int) this.mDesc.Radix, start, length, this.mDesc.Digits);
				return success
					? result
					: (sbyte?)null;
			}
		};

		sealed class TryParseSByteListAsync
			: TryParseSByteListBase< Task<sbyte?> >
		{
			public TryParseSByteListAsync(StringListDesc desc, string values)
				: base(desc, values)
			{
			}

			static sbyte? ProcessItemAsync(object state)
			{
				var args = (Tuple<TryParseSByteListAsync, int, int>)state;
				var me = args.Item1;
				return me.ProcessItem(args.Item2, args.Item3);
			}
			protected override Task<sbyte?> CreateItem(int start, int length)
			{
				return Task<sbyte?>.Factory.StartNew(
						ProcessItemAsync,
						new Tuple<TryParseSByteListAsync, int, int>(this, start, length)
					);
			}

			protected override IEnumerable<sbyte?> CreateResult()
			{
				return
					from task in this.mList
					select task.Result;
			}
		};
		public static IEnumerable<sbyte?> TryParseSByteAsync(StringListDesc desc, string values)
		{
			return new TryParseSByteListAsync(desc, values).TryParse();
		}

		sealed class TryParseSByteList
			: TryParseSByteListBase< sbyte? >
		{
			public TryParseSByteList(StringListDesc desc, string values)
				: base(desc, values)
			{
			}

			protected override sbyte? CreateItem(int start, int length)
			{
				return this.ProcessItem(start, length);
			}

			protected override IEnumerable<sbyte?> CreateResult()
			{
				return this.mList;
			}
		};
		public static IEnumerable<sbyte?> TryParseSByte(StringListDesc desc, string values)
		{
			return new TryParseSByteList(desc, values).TryParse();
		}
		#endregion
		#region TryParse list UInt16
		abstract class TryParseUInt16ListBase<TListItem>
			: TryParseNumberListBase<
					ushort,
					TListItem
				>
		{
			protected TryParseUInt16ListBase(StringListDesc desc, string values)
				: base(desc, values)
			{
			}

			protected override IEnumerable<ushort?> EmptyResult => Array.Empty<ushort?>();

			protected ushort? ProcessItem(int start, int length)
			{
				var result = (ushort)0;
				bool success = TryParseImpl(this.mValues, ref result, (int) this.mDesc.Radix, start, length, this.mDesc.Digits);
				return success
					? result
					: (ushort?)null;
			}
		};

		sealed class TryParseUInt16ListAsync
			: TryParseUInt16ListBase< Task<ushort?> >
		{
			public TryParseUInt16ListAsync(StringListDesc desc, string values)
				: base(desc, values)
			{
			}

			static ushort? ProcessItemAsync(object state)
			{
				var args = (Tuple<TryParseUInt16ListAsync, int, int>)state;
				var me = args.Item1;
				return me.ProcessItem(args.Item2, args.Item3);
			}
			protected override Task<ushort?> CreateItem(int start, int length)
			{
				return Task<ushort?>.Factory.StartNew(
						ProcessItemAsync,
						new Tuple<TryParseUInt16ListAsync, int, int>(this, start, length)
					);
			}

			protected override IEnumerable<ushort?> CreateResult()
			{
				return
					from task in this.mList
					select task.Result;
			}
		};
		public static IEnumerable<ushort?> TryParseUInt16Async(StringListDesc desc, string values)
		{
			return new TryParseUInt16ListAsync(desc, values).TryParse();
		}

		sealed class TryParseUInt16List
			: TryParseUInt16ListBase< ushort? >
		{
			public TryParseUInt16List(StringListDesc desc, string values)
				: base(desc, values)
			{
			}

			protected override ushort? CreateItem(int start, int length)
			{
				return this.ProcessItem(start, length);
			}

			protected override IEnumerable<ushort?> CreateResult()
			{
				return this.mList;
			}
		};
		public static IEnumerable<ushort?> TryParseUInt16(StringListDesc desc, string values)
		{
			return new TryParseUInt16List(desc, values).TryParse();
		}
		#endregion
		#region TryParse list Int16
		abstract class TryParseInt16ListBase<TListItem>
			: TryParseNumberListBase<
					short,
					TListItem
				>
		{
			protected TryParseInt16ListBase(StringListDesc desc, string values)
				: base(desc, values)
			{
			}

			protected override IEnumerable<short?> EmptyResult => Array.Empty<short?>();

			protected short? ProcessItem(int start, int length)
			{
				var result = (short)0;
				bool success = TryParseImpl(this.mValues, ref result, (int) this.mDesc.Radix, start, length, this.mDesc.Digits);
				return success
					? result
					: (short?)null;
			}
		};

		sealed class TryParseInt16ListAsync
			: TryParseInt16ListBase< Task<short?> >
		{
			public TryParseInt16ListAsync(StringListDesc desc, string values)
				: base(desc, values)
			{
			}

			static short? ProcessItemAsync(object state)
			{
				var args = (Tuple<TryParseInt16ListAsync, int, int>)state;
				var me = args.Item1;
				return me.ProcessItem(args.Item2, args.Item3);
			}
			protected override Task<short?> CreateItem(int start, int length)
			{
				return Task<short?>.Factory.StartNew(
						ProcessItemAsync,
						new Tuple<TryParseInt16ListAsync, int, int>(this, start, length)
					);
			}

			protected override IEnumerable<short?> CreateResult()
			{
				return
					from task in this.mList
					select task.Result;
			}
		};
		public static IEnumerable<short?> TryParseInt16Async(StringListDesc desc, string values)
		{
			return new TryParseInt16ListAsync(desc, values).TryParse();
		}

		sealed class TryParseInt16List
			: TryParseInt16ListBase< short? >
		{
			public TryParseInt16List(StringListDesc desc, string values)
				: base(desc, values)
			{
			}

			protected override short? CreateItem(int start, int length)
			{
				return this.ProcessItem(start, length);
			}

			protected override IEnumerable<short?> CreateResult()
			{
				return this.mList;
			}
		};
		public static IEnumerable<short?> TryParseInt16(StringListDesc desc, string values)
		{
			return new TryParseInt16List(desc, values).TryParse();
		}
		#endregion
		#region TryParse list UInt32
		abstract class TryParseUInt32ListBase<TListItem>
			: TryParseNumberListBase<
					uint,
					TListItem
				>
		{
			protected TryParseUInt32ListBase(StringListDesc desc, string values)
				: base(desc, values)
			{
			}

			protected override IEnumerable<uint?> EmptyResult => Array.Empty<uint?>();

			protected uint? ProcessItem(int start, int length)
			{
				var result = (uint)0;
				bool success = TryParseImpl(this.mValues, ref result, (int) this.mDesc.Radix, start, length, this.mDesc.Digits);
				return success
					? result
					: (uint?)null;
			}
		};

		sealed class TryParseUInt32ListAsync
			: TryParseUInt32ListBase< Task<uint?> >
		{
			public TryParseUInt32ListAsync(StringListDesc desc, string values)
				: base(desc, values)
			{
			}

			static uint? ProcessItemAsync(object state)
			{
				var args = (Tuple<TryParseUInt32ListAsync, int, int>)state;
				var me = args.Item1;
				return me.ProcessItem(args.Item2, args.Item3);
			}
			protected override Task<uint?> CreateItem(int start, int length)
			{
				return Task<uint?>.Factory.StartNew(
						ProcessItemAsync,
						new Tuple<TryParseUInt32ListAsync, int, int>(this, start, length)
					);
			}

			protected override IEnumerable<uint?> CreateResult()
			{
				return
					from task in this.mList
					select task.Result;
			}
		};
		public static IEnumerable<uint?> TryParseUInt32Async(StringListDesc desc, string values)
		{
			return new TryParseUInt32ListAsync(desc, values).TryParse();
		}

		sealed class TryParseUInt32List
			: TryParseUInt32ListBase< uint? >
		{
			public TryParseUInt32List(StringListDesc desc, string values)
				: base(desc, values)
			{
			}

			protected override uint? CreateItem(int start, int length)
			{
				return this.ProcessItem(start, length);
			}

			protected override IEnumerable<uint?> CreateResult()
			{
				return this.mList;
			}
		};
		public static IEnumerable<uint?> TryParseUInt32(StringListDesc desc, string values)
		{
			return new TryParseUInt32List(desc, values).TryParse();
		}
		#endregion
		#region TryParse list Int32
		abstract class TryParseInt32ListBase<TListItem>
			: TryParseNumberListBase<
					int,
					TListItem
				>
		{
			protected TryParseInt32ListBase(StringListDesc desc, string values)
				: base(desc, values)
			{
			}

			protected override IEnumerable<int?> EmptyResult => Array.Empty<int?>();

			protected int? ProcessItem(int start, int length)
			{
				var result = (int)0;
				bool success = TryParseImpl(this.mValues, ref result, (int) this.mDesc.Radix, start, length, this.mDesc.Digits);
				return success
					? result
					: (int?)null;
			}
		};

		sealed class TryParseInt32ListAsync
			: TryParseInt32ListBase< Task<int?> >
		{
			public TryParseInt32ListAsync(StringListDesc desc, string values)
				: base(desc, values)
			{
			}

			static int? ProcessItemAsync(object state)
			{
				var args = (Tuple<TryParseInt32ListAsync, int, int>)state;
				var me = args.Item1;
				return me.ProcessItem(args.Item2, args.Item3);
			}
			protected override Task<int?> CreateItem(int start, int length)
			{
				return Task<int?>.Factory.StartNew(
						ProcessItemAsync,
						new Tuple<TryParseInt32ListAsync, int, int>(this, start, length)
					);
			}

			protected override IEnumerable<int?> CreateResult()
			{
				return
					from task in this.mList
					select task.Result;
			}
		};
		public static IEnumerable<int?> TryParseInt32Async(StringListDesc desc, string values)
		{
			return new TryParseInt32ListAsync(desc, values).TryParse();
		}

		sealed class TryParseInt32List
			: TryParseInt32ListBase< int? >
		{
			public TryParseInt32List(StringListDesc desc, string values)
				: base(desc, values)
			{
			}

			protected override int? CreateItem(int start, int length)
			{
				return this.ProcessItem(start, length);
			}

			protected override IEnumerable<int?> CreateResult()
			{
				return this.mList;
			}
		};
		public static IEnumerable<int?> TryParseInt32(StringListDesc desc, string values)
		{
			return new TryParseInt32List(desc, values).TryParse();
		}
		#endregion
		#region TryParse list UInt64
		abstract class TryParseUInt64ListBase<TListItem>
			: TryParseNumberListBase<
					ulong,
					TListItem
				>
		{
			protected TryParseUInt64ListBase(StringListDesc desc, string values)
				: base(desc, values)
			{
			}

			protected override IEnumerable<ulong?> EmptyResult => Array.Empty<ulong?>();

			protected ulong? ProcessItem(int start, int length)
			{
				var result = (ulong)0;
				bool success = TryParseImpl(this.mValues, ref result, (int) this.mDesc.Radix, start, length, this.mDesc.Digits);
				return success
					? result
					: (ulong?)null;
			}
		};

		sealed class TryParseUInt64ListAsync
			: TryParseUInt64ListBase< Task<ulong?> >
		{
			public TryParseUInt64ListAsync(StringListDesc desc, string values)
				: base(desc, values)
			{
			}

			static ulong? ProcessItemAsync(object state)
			{
				var args = (Tuple<TryParseUInt64ListAsync, int, int>)state;
				var me = args.Item1;
				return me.ProcessItem(args.Item2, args.Item3);
			}
			protected override Task<ulong?> CreateItem(int start, int length)
			{
				return Task<ulong?>.Factory.StartNew(
						ProcessItemAsync,
						new Tuple<TryParseUInt64ListAsync, int, int>(this, start, length)
					);
			}

			protected override IEnumerable<ulong?> CreateResult()
			{
				return
					from task in this.mList
					select task.Result;
			}
		};
		public static IEnumerable<ulong?> TryParseUInt64Async(StringListDesc desc, string values)
		{
			return new TryParseUInt64ListAsync(desc, values).TryParse();
		}

		sealed class TryParseUInt64List
			: TryParseUInt64ListBase< ulong? >
		{
			public TryParseUInt64List(StringListDesc desc, string values)
				: base(desc, values)
			{
			}

			protected override ulong? CreateItem(int start, int length)
			{
				return this.ProcessItem(start, length);
			}

			protected override IEnumerable<ulong?> CreateResult()
			{
				return this.mList;
			}
		};
		public static IEnumerable<ulong?> TryParseUInt64(StringListDesc desc, string values)
		{
			return new TryParseUInt64List(desc, values).TryParse();
		}
		#endregion
		#region TryParse list Int64
		abstract class TryParseInt64ListBase<TListItem>
			: TryParseNumberListBase<
					long,
					TListItem
				>
		{
			protected TryParseInt64ListBase(StringListDesc desc, string values)
				: base(desc, values)
			{
			}

			protected override IEnumerable<long?> EmptyResult => Array.Empty<long?>();

			protected long? ProcessItem(int start, int length)
			{
				var result = (long)0;
				bool success = TryParseImpl(this.mValues, ref result, (int) this.mDesc.Radix, start, length, this.mDesc.Digits);
				return success
					? result
					: (long?)null;
			}
		};

		sealed class TryParseInt64ListAsync
			: TryParseInt64ListBase< Task<long?> >
		{
			public TryParseInt64ListAsync(StringListDesc desc, string values)
				: base(desc, values)
			{
			}

			static long? ProcessItemAsync(object state)
			{
				var args = (Tuple<TryParseInt64ListAsync, int, int>)state;
				var me = args.Item1;
				return me.ProcessItem(args.Item2, args.Item3);
			}
			protected override Task<long?> CreateItem(int start, int length)
			{
				return Task<long?>.Factory.StartNew(
						ProcessItemAsync,
						new Tuple<TryParseInt64ListAsync, int, int>(this, start, length)
					);
			}

			protected override IEnumerable<long?> CreateResult()
			{
				return
					from task in this.mList
					select task.Result;
			}
		};
		public static IEnumerable<long?> TryParseInt64Async(StringListDesc desc, string values)
		{
			return new TryParseInt64ListAsync(desc, values).TryParse();
		}

		sealed class TryParseInt64List
			: TryParseInt64ListBase< long? >
		{
			public TryParseInt64List(StringListDesc desc, string values)
				: base(desc, values)
			{
			}

			protected override long? CreateItem(int start, int length)
			{
				return this.ProcessItem(start, length);
			}

			protected override IEnumerable<long?> CreateResult()
			{
				return this.mList;
			}
		};
		public static IEnumerable<long?> TryParseInt64(StringListDesc desc, string values)
		{
			return new TryParseInt64List(desc, values).TryParse();
		}
		#endregion
	};
}
