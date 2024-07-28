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
		/// <summary>Read an <see cref="System.Char"/> to the stream</summary>
		/// <param name="value">value to write to the stream</param>
		/// <param name="bitCount">Number of bits to write</param>
		public void Write(char value, int bitCount = Bits.kCharBitCount)
		{
			Contract.Requires(bitCount <= Bits.kCharBitCount);

			TWord word = (TWord)value;
			this.WriteWord(word, bitCount);
		}
		/// <summary>Read an <see cref="System.Byte"/> to the stream</summary>
		/// <param name="value">value to write to the stream</param>
		/// <param name="bitCount">Number of bits to write</param>
		public void Write(byte value, int bitCount = Bits.kByteBitCount)
		{
			Contract.Requires(bitCount <= Bits.kByteBitCount);

			TWord word = (TWord)value;
			this.WriteWord(word, bitCount);
		}
		/// <summary>Read an <see cref="System.SByte"/> to the stream</summary>
		/// <param name="value">value to write to the stream</param>
		/// <param name="bitCount">Number of bits to write</param>
		public void Write(sbyte value, int bitCount = Bits.kSByteBitCount)
		{
			Contract.Requires(bitCount <= Bits.kSByteBitCount);

			TWord word = (TWord)value;
			this.WriteWord(word, bitCount);
		}
		/// <summary>Read an <see cref="System.UInt16"/> to the stream</summary>
		/// <param name="value">value to write to the stream</param>
		/// <param name="bitCount">Number of bits to write</param>
		public void Write(ushort value, int bitCount = Bits.kUInt16BitCount)
		{
			Contract.Requires(bitCount <= Bits.kUInt16BitCount);

			TWord word = (TWord)value;
			this.WriteWord(word, bitCount);
		}
		/// <summary>Read an <see cref="System.Int16"/> to the stream</summary>
		/// <param name="value">value to write to the stream</param>
		/// <param name="bitCount">Number of bits to write</param>
		public void Write(short value, int bitCount = Bits.kInt16BitCount)
		{
			Contract.Requires(bitCount <= Bits.kInt16BitCount);

			TWord word = (TWord)value;
			this.WriteWord(word, bitCount);
		}
		/// <summary>Read an <see cref="System.UInt32"/> to the stream</summary>
		/// <param name="value">value to write to the stream</param>
		/// <param name="bitCount">Number of bits to write</param>
		public void Write(uint value, int bitCount = Bits.kUInt32BitCount)
		{
			Contract.Requires(bitCount <= Bits.kUInt32BitCount);

			TWord word = (TWord)value;
			this.WriteWord(word, bitCount);
		}
		/// <summary>Read an <see cref="System.Int32"/> to the stream</summary>
		/// <param name="value">value to write to the stream</param>
		/// <param name="bitCount">Number of bits to write</param>
		public void Write(int value, int bitCount = Bits.kInt32BitCount)
		{
			Contract.Requires(bitCount <= Bits.kInt32BitCount);

			TWord word = (TWord)value;
			this.WriteWord(word, bitCount);
		}
		/// <summary>Read an <see cref="System.UInt64"/> to the stream</summary>
		/// <param name="value">value to write to the stream</param>
		/// <param name="bitCount">Number of bits to write</param>
		public void Write(ulong value, int bitCount = Bits.kUInt64BitCount)
		{
			Contract.Requires(bitCount <= Bits.kUInt64BitCount);

			uint msb_word = (uint)(value >> Bits.kInt32BitCount);
			uint lsb_word = (uint)value;
			int msb_bit_count = bitCount > Bits.kInt32BitCount ? bitCount - Bits.kInt32BitCount : 0;
			int lsb_bit_count = bitCount > Bits.kInt32BitCount ? bitCount - msb_bit_count : bitCount;

			if(msb_bit_count > 0)
				this.WriteWord(msb_word, msb_bit_count);
			this.WriteWord(lsb_word, lsb_bit_count);
		}
		/// <summary>Read an <see cref="System.Int64"/> to the stream</summary>
		/// <param name="value">value to write to the stream</param>
		/// <param name="bitCount">Number of bits to write</param>
		public void Write(long value, int bitCount = Bits.kInt64BitCount)
		{
			Contract.Requires(bitCount <= Bits.kInt64BitCount);

			uint msb_word = (uint)(value >> Bits.kInt32BitCount);
			uint lsb_word = (uint)value;
			int msb_bit_count = bitCount > Bits.kInt32BitCount ? bitCount - Bits.kInt32BitCount : 0;
			int lsb_bit_count = bitCount > Bits.kInt32BitCount ? bitCount - msb_bit_count : bitCount;

			if(msb_bit_count > 0)
				this.WriteWord(msb_word, msb_bit_count);
			this.WriteWord(lsb_word, lsb_bit_count);
		}
	};
}