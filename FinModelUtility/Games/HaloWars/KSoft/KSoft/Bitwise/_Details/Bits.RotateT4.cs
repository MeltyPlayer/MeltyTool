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
		public static byte RotateLeft(byte x, int shift)
		{
			Contract.Requires/*<ArgumentOutOfRangeException>*/(shift >= 0);
			Contract.Requires/*<ArgumentOutOfRangeException>*/(shift < kByteBitCount);

			return (byte)( (x << shift) | (x >> (kByteBitCount - shift)) );
		}
		[Contracts.Pure]
		public static byte RotateRight(byte x, int shift)
		{
			Contract.Requires/*<ArgumentOutOfRangeException>*/(shift >= 0);
			Contract.Requires/*<ArgumentOutOfRangeException>*/(shift < kByteBitCount);

			return (byte)( (x >> shift) | (x << (kByteBitCount - shift)) );
		}

		[Contracts.Pure]
		public static ushort RotateLeft(ushort x, int shift)
		{
			Contract.Requires/*<ArgumentOutOfRangeException>*/(shift >= 0);
			Contract.Requires/*<ArgumentOutOfRangeException>*/(shift < kInt16BitCount);

			return (ushort)( (x << shift) | (x >> (kInt16BitCount - shift)) );
		}
		[Contracts.Pure]
		public static ushort RotateRight(ushort x, int shift)
		{
			Contract.Requires/*<ArgumentOutOfRangeException>*/(shift >= 0);
			Contract.Requires/*<ArgumentOutOfRangeException>*/(shift < kInt16BitCount);

			return (ushort)( (x >> shift) | (x << (kInt16BitCount - shift)) );
		}

		[Contracts.Pure]
		public static uint RotateLeft(uint x, int shift)
		{
			Contract.Requires/*<ArgumentOutOfRangeException>*/(shift >= 0);
			Contract.Requires/*<ArgumentOutOfRangeException>*/(shift < kInt32BitCount);

			return (uint)( (x << shift) | (x >> (kInt32BitCount - shift)) );
		}
		[Contracts.Pure]
		public static uint RotateRight(uint x, int shift)
		{
			Contract.Requires/*<ArgumentOutOfRangeException>*/(shift >= 0);
			Contract.Requires/*<ArgumentOutOfRangeException>*/(shift < kInt32BitCount);

			return (uint)( (x >> shift) | (x << (kInt32BitCount - shift)) );
		}

		[Contracts.Pure]
		public static ulong RotateLeft(ulong x, int shift)
		{
			Contract.Requires/*<ArgumentOutOfRangeException>*/(shift >= 0);
			Contract.Requires/*<ArgumentOutOfRangeException>*/(shift < kInt64BitCount);

			return (ulong)( (x << shift) | (x >> (kInt64BitCount - shift)) );
		}
		[Contracts.Pure]
		public static ulong RotateRight(ulong x, int shift)
		{
			Contract.Requires/*<ArgumentOutOfRangeException>*/(shift >= 0);
			Contract.Requires/*<ArgumentOutOfRangeException>*/(shift < kInt64BitCount);

			return (ulong)( (x >> shift) | (x << (kInt64BitCount - shift)) );
		}

	};
}
