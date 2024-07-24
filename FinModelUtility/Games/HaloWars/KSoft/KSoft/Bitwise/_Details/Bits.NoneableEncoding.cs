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
		/// <summary>Calculate the traits needed for representing a bit-encoded value which can also equal NONE (-1)</summary>
		/// <param name="maxValue">An enumeration's <b>kMax</b> value</param>
		/// <param name="bitCount">Number of bits needed to represent NONE to (<paramref name="maxValue"/> - 1)</param>
		/// <param name="traceVerboseChecks">Should verbose checks be performed and traced? No side effects outside of DEBUG</param>
#if DEBUG
		/// <param name="sourceFile">Source file path of this method's caller. DEBUG only, don't manually specify</param>
		/// <param name="sourceLineNum">Source file line of this method's caller. DEBUG only, don't manually specify</param>
#endif
		/// <remarks>A <b>kMax</b> value should be unused and the last entry of an Enumeration. This is why 1 is subtracted from <paramref name="maxValue"/>.</remarks>
		[Contracts.Pure]
		public static uint GetNoneableEncodingTraits(int maxValue
			, out int bitCount
			, bool traceVerboseChecks = true // only used in DEBUG
#if DEBUG
			, [System.Runtime.CompilerServices.CallerFilePath] string sourceFile = ""
			, [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNum = -1
#endif
			)
		{
			Contract.Requires/*<ArgumentOutOfRangeException>*/(maxValue > 0 && maxValue < int.MaxValue);
			Contract.Ensures(Contract.ValueAtReturn(out bitCount) > 0);

			// Add one to the max value, as NONE encoding adds one to the value when encoding (then subtracts on decode)
			bitCount = GetMaxEnumBits(maxValue+1);
			var bitmask = BitCountToMask32(bitCount);
#if DEBUG
			if (traceVerboseChecks)
			{
				// GetMaxEnumBits asserts maxValue > 1, as why would you normally want to bit encode an enum with only 1 possible value?
				// hard set naked bit count to 1-bit to get around this case (where NONE and 0 are valid values)
				int naked_bit_count = maxValue > 1
					? GetMaxEnumBits(maxValue)
					: 1;

				if (bitCount > naked_bit_count)
				{
					Debug.Trace.KSoft.TraceInformation(
						"Noneable encoding at {0},#{1} appears inefficient. " +
						"#{2} requires {3}-bits, but encoding overflows to {4}-bits",
						sourceFile, sourceLineNum, maxValue, naked_bit_count, bitCount);
				}
			}
#endif

			return bitmask;
		}

		/// <summary>Calculate the traits needed for representing a bit-encoded value which can also equal NONE (-1)</summary>
		/// <param name="maxValue">An enumeration's <b>kMax</b> value</param>
		/// <param name="bitCount">Number of bits needed to represent NONE to (<paramref name="maxValue"/> - 1)</param>
		/// <param name="traceVerboseChecks">Should verbose checks be performed and traced? No side effects outside of DEBUG</param>
#if DEBUG
		/// <param name="sourceFile">Source file path of this method's caller. DEBUG only, don't manually specify</param>
		/// <param name="sourceLineNum">Source file line of this method's caller. DEBUG only, don't manually specify</param>
#endif
		/// <remarks>A <b>kMax</b> value should be unused and the last entry of an Enumeration. This is why 1 is subtracted from <paramref name="maxValue"/>.</remarks>
		[Contracts.Pure]
		public static ulong GetNoneableEncodingTraits(long maxValue
			, out int bitCount
			, bool traceVerboseChecks = true // only used in DEBUG
#if DEBUG
			, [System.Runtime.CompilerServices.CallerFilePath] string sourceFile = ""
			, [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNum = -1
#endif
			)
		{
			Contract.Requires/*<ArgumentOutOfRangeException>*/(maxValue > 0 && maxValue < long.MaxValue);
			Contract.Ensures(Contract.ValueAtReturn(out bitCount) > 0);

			// Add one to the max value, as NONE encoding adds one to the value when encoding (then subtracts on decode)
			bitCount = GetMaxEnumBits(maxValue+1);
			var bitmask = BitCountToMask64(bitCount);
#if DEBUG
			if (traceVerboseChecks)
			{
				// GetMaxEnumBits asserts maxValue > 1, as why would you normally want to bit encode an enum with only 1 possible value?
				// hard set naked bit count to 1-bit to get around this case (where NONE and 0 are valid values)
				int naked_bit_count = maxValue > 1
					? GetMaxEnumBits(maxValue)
					: 1;

				if (bitCount > naked_bit_count)
				{
					Debug.Trace.KSoft.TraceInformation(
						"Noneable encoding at {0},#{1} appears inefficient. " +
						"#{2} requires {3}-bits, but encoding overflows to {4}-bits",
						sourceFile, sourceLineNum, maxValue, naked_bit_count, bitCount);
				}
			}
#endif

			return bitmask;
		}

	};
}
