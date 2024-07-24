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
		[Contracts.Pure]
		public static byte BitSwap(byte value, int startBitIndex = kByteBitCount-1)
		{
			Contract.Requires(startBitIndex > 0, kBitSwap_StartBitIndexNotGreaterThanZero);
			Contract.Requires(startBitIndex < kByteBitCount);

			if (value != 0 && value != byte.MaxValue)
			{
				uint bits = 0;
				const uint k_one = 1U;

				int bits_shift = 0;
				int value_shift = startBitIndex;
				for (var value_mask = k_one << startBitIndex;
					value_shift >= 0;
					value_mask >>= 1, value_shift--, bits_shift++)
					bits |= ((value & value_mask) >> value_shift) << bits_shift;

				value = (byte)bits;
			}
			return value;
		}

		[Contracts.Pure]
		public static ushort BitSwap(ushort value, int startBitIndex = kInt16BitCount-1)
		{
			Contract.Requires(startBitIndex > 0, kBitSwap_StartBitIndexNotGreaterThanZero);
			Contract.Requires(startBitIndex < kInt16BitCount);

			if (value != 0 && value != ushort.MaxValue)
			{
				uint bits = 0;
				const uint k_one = 1U;

				int bits_shift = 0;
				int value_shift = startBitIndex;
				for (var value_mask = k_one << startBitIndex;
					value_shift >= 0;
					value_mask >>= 1, value_shift--, bits_shift++)
					bits |= ((value & value_mask) >> value_shift) << bits_shift;

				value = (ushort)bits;
			}
			return value;
		}

		[Contracts.Pure]
		public static uint BitSwap(uint value, int startBitIndex = kInt32BitCount-1)
		{
			Contract.Requires(startBitIndex > 0, kBitSwap_StartBitIndexNotGreaterThanZero);
			Contract.Requires(startBitIndex < kInt32BitCount);

			if (value != 0 && value != uint.MaxValue)
			{
				uint bits = 0;
				const uint k_one = 1U;

				int bits_shift = 0;
				int value_shift = startBitIndex;
				for (var value_mask = k_one << startBitIndex;
					value_shift >= 0;
					value_mask >>= 1, value_shift--, bits_shift++)
					bits |= ((value & value_mask) >> value_shift) << bits_shift;

				value = (uint)bits;
			}
			return value;
		}

		[Contracts.Pure]
		public static ulong BitSwap(ulong value, int startBitIndex = kInt64BitCount-1)
		{
			Contract.Requires(startBitIndex > 0, kBitSwap_StartBitIndexNotGreaterThanZero);
			Contract.Requires(startBitIndex < kInt64BitCount);

			if (value != 0 && value != ulong.MaxValue)
			{
				ulong bits = 0;
				const ulong k_one = 1UL;

				int bits_shift = 0;
				int value_shift = startBitIndex;
				for (var value_mask = k_one << startBitIndex;
					value_shift >= 0;
					value_mask >>= 1, value_shift--, bits_shift++)
					bits |= ((value & value_mask) >> value_shift) << bits_shift;

				value = (ulong)bits;
			}
			return value;
		}

	};
}
