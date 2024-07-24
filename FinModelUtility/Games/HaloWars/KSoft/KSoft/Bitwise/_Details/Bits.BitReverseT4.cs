using Contracts = System.Diagnostics.Contracts;
#if CONTRACTS_FULL_SHIM

#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft
{
	partial class Bits
	{
		/// <summary>Get the bit-reversed equivalent of an unsigned integer</summary>
		/// <param name="bits">Integer to bit-reverse</param>
		/// <returns></returns>
		[Contracts.Pure]
		public static byte BitReverse(byte bits)
		{
			uint x = bits;
			x = ((x & 0xAA) >>  1) | ((x & 0x55) <<  1); // swap odd and even bits
			x = ((x & 0xCC) >>  2) | ((x & 0x33) <<  2); // swap consecutive pairs
			x = ((x & 0xF0) >>  4) | ((x & 0x0F) <<  4); // swap nibbles

			return (byte)x;
		}

		/// <summary>Get the bit-reversed equivalent of an unsigned integer</summary>
		/// <param name="bits">Integer to bit-reverse</param>
		/// <returns></returns>
		[Contracts.Pure]
		public static ushort BitReverse(ushort bits)
		{
			uint x = bits;
			x = ((x & 0xAAAA) >>  1) | ((x & 0x5555) <<  1); // swap odd and even bits
			x = ((x & 0xCCCC) >>  2) | ((x & 0x3333) <<  2); // swap consecutive pairs
			x = ((x & 0xF0F0) >>  4) | ((x & 0x0F0F) <<  4); // swap nibbles
			x = ((x & 0xFF00) >>  8) | ((x & 0x00FF) <<  8); // swap bytes

			return (ushort)x;
		}

		/// <summary>Get the bit-reversed equivalent of an unsigned integer</summary>
		/// <param name="bits">Integer to bit-reverse</param>
		/// <returns></returns>
		[Contracts.Pure]
		public static uint BitReverse(uint bits)
		{
			uint x = bits;
			x = ((x & 0xAAAAAAAA) >>  1) | ((x & 0x55555555) <<  1); // swap odd and even bits
			x = ((x & 0xCCCCCCCC) >>  2) | ((x & 0x33333333) <<  2); // swap consecutive pairs
			x = ((x & 0xF0F0F0F0) >>  4) | ((x & 0x0F0F0F0F) <<  4); // swap nibbles
			x = ((x & 0xFF00FF00) >>  8) | ((x & 0x00FF00FF) <<  8); // swap bytes
			x = ((x & 0xFFFF0000) >> 16) | ((x & 0x0000FFFF) << 16); // swap halves

			return x;
		}

		/// <summary>Get the bit-reversed equivalent of an unsigned integer</summary>
		/// <param name="bits">Integer to bit-reverse</param>
		/// <returns></returns>
		[Contracts.Pure]
		public static ulong BitReverse(ulong bits)
		{
			ulong x = bits;
			x = ((x & 0xAAAAAAAAAAAAAAAA) >>  1) | ((x & 0x5555555555555555) <<  1); // swap odd and even bits
			x = ((x & 0xCCCCCCCCCCCCCCCC) >>  2) | ((x & 0x3333333333333333) <<  2); // swap consecutive pairs
			x = ((x & 0xF0F0F0F0F0F0F0F0) >>  4) | ((x & 0x0F0F0F0F0F0F0F0F) <<  4); // swap nibbles
			x = ((x & 0xFF00FF00FF00FF00) >>  8) | ((x & 0x00FF00FF00FF00FF) <<  8); // swap bytes
			x = ((x & 0xFFFF0000FFFF0000) >> 16) | ((x & 0x0000FFFF0000FFFF) << 16); // swap halves
			x = ((x & 0xFFFFFFFF00000000) >> 32) | ((x & 0x00000000FFFFFFFF) << 32); // swap words

			return x;
		}

	};
}
