#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

using TWord = System.UInt32;

namespace KSoft.IO
{
	partial class BitStream
	{
		#region Read\Write uint Impl
		internal void ReadWord(out uint word, int bitCount)
		{
			Contract.Requires(bitCount <= kWordBitCount);

			int bits_remaining = this.CacheBitsRemaining;

			// if the requested bits are contained entirely in the cache...
			if (bitCount <= bits_remaining)
			{
				word = (uint) this.ExtractWordFromCache(bitCount);
				this.mCacheBitIndex += bitCount;

				// If we consumed the rest of the cache after that last extraction
				if (this.mCacheBitIndex == kWordBitCount && !this.IsEndOfStream)
					this.FillCache();
			}
			else // else the cache only has a portion of the bits (or needs to be re-filled)
			{
				int word_bits_remaining = bitCount;

				// will always be negative, so abs it
				int msb_shift = -(bits_remaining - word_bits_remaining);
				// get the word bits (MSB) that are left in the cache
				word = (uint) this.ExtractWordFromCache(bits_remaining);
				word_bits_remaining -= bits_remaining;
				// adjust the bits to the MSB
				word <<= msb_shift;

				this.FillCache(); // fill the cache with the next round of bits

				// get the 'rest' of the bits that weren't initially in our cache
				TWord more_bits = this.ExtractWordFromCache(word_bits_remaining);

				word |= (uint)more_bits;
				this.mCacheBitIndex = word_bits_remaining;
			}
		}
		internal void WriteWord(uint word, int bitCount)
		{
			Contract.Requires(bitCount <= kWordBitCount);

			int bits_remaining = this.CacheBitsRemaining;

			// if the bits to write can be held entirely in the cache...
			if (bitCount <= bits_remaining)
			{
				this.PutWordInCache((TWord)word, bitCount);
				this.mCacheBitIndex += bitCount;

				if (this.mCacheBitIndex == kWordBitCount)
					this.FlushCache();
			}
			else // else we have to split the cache writes between a flush
			{
				int word_bits_remaining = bitCount;

				// will always be negative, so abs it
				int msb_shift = -(bits_remaining - word_bits_remaining);
				// write the upper (MSB) word bits to the remaining cache bits
				this.PutWordInCache((TWord)(word >> msb_shift), bits_remaining);
				word_bits_remaining -= bits_remaining;

				// Flush determines the amount of bytes to write based on the current
				// bit index. This causes it to write all the bytes of the TWord
				this.mCacheBitIndex += bits_remaining;
				this.FlushCache(); // flush the MSB results and reset the cache

				this.PutWordInCache((TWord)word, word_bits_remaining);
				this.mCacheBitIndex = word_bits_remaining;
			}
		}
		#endregion

		#region Read\Write ulong Impl
		internal void ReadWord(out ulong word, int bitCount)
		{
			Contract.Requires(bitCount <= kWordBitCount);

			int bits_remaining = this.CacheBitsRemaining;

			// if the requested bits are contained entirely in the cache...
			if (bitCount <= bits_remaining)
			{
				word = (ulong) this.ExtractWordFromCache(bitCount);
				this.mCacheBitIndex += bitCount;

				// If we consumed the rest of the cache after that last extraction
				if (this.mCacheBitIndex == kWordBitCount && !this.IsEndOfStream)
					this.FillCache();
			}
			else // else the cache only has a portion of the bits (or needs to be re-filled)
			{
				int word_bits_remaining = bitCount;

				// will always be negative, so abs it
				int msb_shift = -(bits_remaining - word_bits_remaining);
				// get the word bits (MSB) that are left in the cache
				word = (ulong) this.ExtractWordFromCache(bits_remaining);
				word_bits_remaining -= bits_remaining;
				// adjust the bits to the MSB
				word <<= msb_shift;

				this.FillCache(); // fill the cache with the next round of bits

				// get the 'rest' of the bits that weren't initially in our cache
				TWord more_bits = this.ExtractWordFromCache(word_bits_remaining);

				word |= (ulong)more_bits;
				this.mCacheBitIndex = word_bits_remaining;
			}
		}
		internal void WriteWord(ulong word, int bitCount)
		{
			Contract.Requires(bitCount <= kWordBitCount);

			int bits_remaining = this.CacheBitsRemaining;

			// if the bits to write can be held entirely in the cache...
			if (bitCount <= bits_remaining)
			{
				this.PutWordInCache((TWord)word, bitCount);
				this.mCacheBitIndex += bitCount;

				if (this.mCacheBitIndex == kWordBitCount)
					this.FlushCache();
			}
			else // else we have to split the cache writes between a flush
			{
				int word_bits_remaining = bitCount;

				// will always be negative, so abs it
				int msb_shift = -(bits_remaining - word_bits_remaining);
				// write the upper (MSB) word bits to the remaining cache bits
				this.PutWordInCache((TWord)(word >> msb_shift), bits_remaining);
				word_bits_remaining -= bits_remaining;

				// Flush determines the amount of bytes to write based on the current
				// bit index. This causes it to write all the bytes of the TWord
				this.mCacheBitIndex += bits_remaining;
				this.FlushCache(); // flush the MSB results and reset the cache

				this.PutWordInCache((TWord)word, word_bits_remaining);
				this.mCacheBitIndex = word_bits_remaining;
			}
		}
		#endregion

	};
}