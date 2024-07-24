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
#if false // old, solid impl
		/// <summary>Count the number of 'on' bits in an unsigned integer</summary>
		/// <param name="bits">Integer whose bits to count</param>
		/// <returns></returns>
		[Contracts.Pure]
		public static int BitCount(byte bits)
		{
			uint x = bits;
			x = ((x & 0xAA) >>  1) + (x & 0x55);
			x = ((x & 0xCC) >>  2) + (x & 0x33);
			x = ((x & 0xF0) >>  4) + (x & 0x0F);

			return (int)x;
		}

		/// <summary>Count the number of 'on' bits in an unsigned integer</summary>
		/// <param name="bits">Integer whose bits to count</param>
		/// <returns></returns>
		[Contracts.Pure]
		public static int BitCount(ushort bits)
		{
			uint x = bits;
			x = ((x & 0xAAAA) >>  1) + (x & 0x5555);
			x = ((x & 0xCCCC) >>  2) + (x & 0x3333);
			x = ((x & 0xF0F0) >>  4) + (x & 0x0F0F);
			x = ((x & 0xFF00) >>  8) + (x & 0x00FF);

			return (int)x;
		}

		/// <summary>Count the number of 'on' bits in an unsigned integer</summary>
		/// <param name="bits">Integer whose bits to count</param>
		/// <returns></returns>
		[Contracts.Pure]
		public static int BitCount(uint bits)
		{
			uint x = bits;
			x = ((x & 0xAAAAAAAA) >>  1) + (x & 0x55555555);
			x = ((x & 0xCCCCCCCC) >>  2) + (x & 0x33333333);
			x = ((x & 0xF0F0F0F0) >>  4) + (x & 0x0F0F0F0F);
			x = ((x & 0xFF00FF00) >>  8) + (x & 0x00FF00FF);
			x = ((x & 0xFFFF0000) >> 16) + (x & 0x0000FFFF);

			return (int)x;
		}

		/// <summary>Count the number of 'on' bits in an unsigned integer</summary>
		/// <param name="bits">Integer whose bits to count</param>
		/// <returns></returns>
		[Contracts.Pure]
		public static int BitCount(ulong bits)
		{
			ulong x = bits;
			x = ((x & 0xAAAAAAAAAAAAAAAA) >>  1) + (x & 0x5555555555555555);
			x = ((x & 0xCCCCCCCCCCCCCCCC) >>  2) + (x & 0x3333333333333333);
			x = ((x & 0xF0F0F0F0F0F0F0F0) >>  4) + (x & 0x0F0F0F0F0F0F0F0F);
			x = ((x & 0xFF00FF00FF00FF00) >>  8) + (x & 0x00FF00FF00FF00FF);
			x = ((x & 0xFFFF0000FFFF0000) >> 16) + (x & 0x0000FFFF0000FFFF);
			x = ((x & 0xFFFFFFFF00000000) >> 32) + (x & 0x00000000FFFFFFFF);

			return (int)x;
		}

#endif

		/// <summary>Count the number of 'on' bits in an unsigned integer</summary>
		/// <param name="bits">Integer whose bits to count</param>
		/// <returns></returns>
		[Contracts.Pure]
		public static int BitCount(byte bits)
		{
			uint x = bits;
			x =  x - ((x >> 1) & 0x55);
			x = (x & 0x33) + ((x >> 2) & 0x33);
			x =  x + (x >> 4) & 0x0F;
			x = (x * 0x01) >> 0;

			return (int)x;
		}

		/// <summary>Count the number of 'on' bits in an unsigned integer</summary>
		/// <param name="bits">Integer whose bits to count</param>
		/// <returns></returns>
		[Contracts.Pure]
		public static int BitCount(uint bits)
		{
			uint x = bits;
			x =  x - ((x >> 1) & 0x55555555);
			x = (x & 0x33333333) + ((x >> 2) & 0x33333333);
			x =  x + (x >> 4) & 0x0F0F0F0F;
			x = (x * 0x01010101) >> 24;

			return (int)x;
		}

		/// <summary>Count the number of 'on' bits in an unsigned integer</summary>
		/// <param name="bits">Integer whose bits to count</param>
		/// <returns></returns>
		[Contracts.Pure]
		public static int BitCount(ulong bits)
		{
			ulong x = bits;
			x =  x - ((x >> 1) & 0x5555555555555555);
			x = (x & 0x3333333333333333) + ((x >> 2) & 0x3333333333333333);
			x =  x + (x >> 4) & 0x0F0F0F0F0F0F0F0F;
			x = (x * 0x0101010101010101) >> 56;

			return (int)x;
		}


		/// <summary>Calculate the bit-mask needed for a number of bits</summary>
		/// <param name="bitCount">Number of bits needed for the mask</param>
		/// <returns></returns>
		[Contracts.Pure]
		public static uint BitCountToMask32(int bitCount)
		{
			Contract.Requires/*<ArgumentOutOfRangeException>*/(bitCount >= 0 && bitCount <= kUInt32BitCount);

			return uint.MaxValue >> (kUInt32BitCount-bitCount);
		}

		/// <summary>Calculate the bit-mask needed for a number of bits</summary>
		/// <param name="bitCount">Number of bits needed for the mask</param>
		/// <returns></returns>
		[Contracts.Pure]
		public static ulong BitCountToMask64(int bitCount)
		{
			Contract.Requires/*<ArgumentOutOfRangeException>*/(bitCount >= 0 && bitCount <= kUInt64BitCount);

			return ulong.MaxValue >> (kUInt64BitCount-bitCount);
		}

	};
}
