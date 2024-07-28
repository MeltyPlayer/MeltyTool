using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Contracts = System.Diagnostics.Contracts;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft
{
	// Getting matching results with radix <= 63:
	// http://www.pgregg.com/projects/php/base_conversion/base_conversion.php

	// http://bitplane.net/2010/08/java-float-fast-parser/
	// http://tinodidriksen.com/2011/05/28/cpp-convert-string-to-double-speed/

	public static partial class Numbers
	{
		enum ParseErrorType
		{
			None,
			/// <summary>Input string is null or empty</summary>
			NoInput,
			/// <summary>The input can't be parsed as-is</summary>
			InvalidValue,
			InvalidStartIndex,
		};

		public const int kBase10 = 10;
		public const int kBase36 = 36;
		public const int kBase64 = 64;
		public const string kBase64Digits = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz+/";
		public const string kBase64DigitsRfc4648 = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";

		[Contracts.Pure]
		static bool HandleParseError(ParseErrorType errorType, bool noThrow, string s, int startIndex
			, Text.IHandleTextParseError handler = null)
		{
			Exception detailsException = null;

			switch (errorType)
			{
			case ParseErrorType.NoInput:
				if (noThrow)
					return false;

				detailsException = new ArgumentException
					("Input null or empty", nameof(s));
				break;

			case ParseErrorType.InvalidValue:
				detailsException = new ArgumentException(string.Format
					(Util.InvariantCultureInfo, "Couldn't parse '{0}'", s), nameof(s));
				break;

			case ParseErrorType.InvalidStartIndex:
				detailsException = new ArgumentOutOfRangeException(nameof(s), string.Format
					(Util.InvariantCultureInfo, "'{0}' is out of range of the input length of '{1}'", startIndex, s.Length));
				break;

			default:
				return true;
			}

			if (handler == null)
				handler = Text.Util.DefaultTextParseErrorHandler;

			if (noThrow == false)
				handler.ThrowReadExeception(detailsException);

			handler.LogReadExceptionWarning(detailsException);
			return true;
		}

		[Contracts.Pure]
		public static bool IsValidLookupTable(NumeralBase radix, string digits)
		{
			return radix >= NumeralBase.Binary && (int)radix <= digits.Length;
		}
		[Contracts.Pure]
		public static bool IsValidLookupTable(NumbersRadix radix, string digits)
		{
			return radix >= NumbersRadix.Binary && (int)radix <= digits.Length;
		}


		[SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
		[SuppressMessage("Microsoft.Design", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
		public struct StringListDesc
		{
			public const char kDefaultSeparator = ',';
			public const char kDefaultTerminator = ';';
			public static StringListDesc Default { get => new StringListDesc(kDefaultSeparator); }

			public string Digits;
			public NumbersRadix Radix;
			/// <remarks><b>false</b> by default</remarks>
			public bool RequiresTerminator;

			public char Separator;
			public char Terminator;

			public StringListDesc(char separator, char terminator = kDefaultTerminator,
				NumbersRadix radix = NumbersRadix.Decimal, string digits = kBase64Digits)
			{
				Contract.Requires(!string.IsNullOrEmpty(digits));
				Contract.Requires(IsValidLookupTable(radix, digits));

				this.Digits = digits;
				this.Radix = radix;
				this.RequiresTerminator = false;

				this.Separator = separator;
				this.Terminator = terminator;
			}

			[Contracts.Pure]
			internal int PredictedCount(string values)
			{
				Contract.Assume(values != null);

				int count = 1;

				// using StringSegment and its Enumerator won't allocate any reference types
				var sseg = new Collections.StringSegment(values);
				foreach (char c in sseg)
				{
					if (c == this.Separator)
						count++;
					else if (c == this.Terminator)
						break;
				}

				return count;
			}
		};

		// #REVIEW: IsWhiteSpace can be rather expensive, and it is used in TryParseImpl. Perhaps we can make a variant
		// that can safely assume all characters are non-ws, and have TryParseList impls call it instead?
		// The TryParse() below would need to be updated to catch trailing ws

		// #REVIEW: add an option to just flat out skip unsuccessful items?

		abstract class TryParseNumberListBase<T, TListItem>
			where T : struct
		{
			protected readonly StringListDesc mDesc;
			protected readonly string mValues;
			protected List<TListItem> mList;

			protected TryParseNumberListBase(StringListDesc desc, string values)
			{
				this.mDesc = desc;
				this.mValues = values;
			}

			protected abstract IEnumerable<T?> EmptyResult { get; }

			void InitializeList()
			{
				// ReSharper disable once ImpureMethodCallOnReadonlyValueField - yes IT IS fucking Pure you POS
				int predicated_count = this.mDesc.PredictedCount(this.mValues);
				this.mList = new List<TListItem>(predicated_count);
			}

			protected abstract TListItem CreateItem(int start, int length);

			protected abstract IEnumerable<T?> CreateResult();

			public IEnumerable<T?> TryParse()
			{
				if (this.mValues == null)
					return this.EmptyResult;

				this.InitializeList();

				bool found_terminator = false;
				int value_length = this.mValues.Length;
				for (int start = 0; !found_terminator && start < value_length; )
				{
					// Skip any starting whitespace
					while (start < value_length && char.IsWhiteSpace(this.mValues[start]))
						++start;

					int end = start;
					int length = 0;
					while (end < value_length)
					{
						char c = this.mValues[end];
						found_terminator = c == this.mDesc.Terminator;
						// NOTE: TryParseImpl actually handles leading and trailing whitespace
						if (c == this.mDesc.Separator || found_terminator)
							break;

						// NOTE: we wouldn't want to update length if we hit ws before the separator and the TryParseImpl assumes no ws
						++length;
						++end;
					}

					if (length > 0)
						this.mList.Add(this.CreateItem(start, length));

					start = end + 1;
				}

				// #REVIEW: should we add support for throwing an exception or such when a terminator isn't encountered?

				return this.mList.Count == 0
					? this.EmptyResult
					: this.CreateResult();
			}
		};

		// Single.ToString(string): "if format is null or an empty string, the return value for this isntance is formatted with the general numeric format specifier ("G")
		public const string kFloatDefaultFormatSpecifier = null;
		public const string kFloatRoundTripFormatSpecifier = "G9";
		public const string kSingleRoundTripFormatSpecifier = kFloatRoundTripFormatSpecifier;
		public const string kDoubleRoundTripFormatSpecifier = "G17";

		// based on the reference source, this is what the default number styles are
		public const NumberStyles kFloatTryParseNumberStyles = 0
			| NumberStyles.Float
			| NumberStyles.AllowThousands;
		public static bool FloatTryParseInvariant(string s, out float result)
		{
			return float.TryParse(s, kFloatTryParseNumberStyles, CultureInfo.InvariantCulture, out result);
		}
		public static float FloatParseInvariant(string s)
		{
			return float.Parse(s, kFloatTryParseNumberStyles, CultureInfo.InvariantCulture);
		}

		// based on the reference source, this is what the default number styles are
		public const NumberStyles kDoubleTryParseNumberStyles = kFloatTryParseNumberStyles;
		public static bool DoubleTryParseInvariant(string s, out double result)
		{
			return double.TryParse(s, kDoubleTryParseNumberStyles, CultureInfo.InvariantCulture, out result);
		}
		public static double DoubleParseInvariant(string s)
		{
			return double.Parse(s, kDoubleTryParseNumberStyles, CultureInfo.InvariantCulture);
		}
	};
};
