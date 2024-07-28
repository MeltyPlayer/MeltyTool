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
		/// <summary>Read an <see cref="System.Char"/> from the stream</summary>
		/// <param name="bitCount">Number of bits to read</param>
		/// <returns></returns>
		public char ReadChar(int bitCount = Bits.kCharBitCount
			)
		{
			Contract.Requires(bitCount <= Bits.kCharBitCount);

			this.ReadWord(out TWord word, bitCount);

			return (char)word;
		}
		/// <summary>Read an <see cref="System.Byte"/> from the stream</summary>
		/// <param name="bitCount">Number of bits to read</param>
		/// <returns></returns>
		public byte ReadByte(int bitCount = Bits.kByteBitCount
			)
		{
			Contract.Requires(bitCount <= Bits.kByteBitCount);

			this.ReadWord(out TWord word, bitCount);

			return (byte)word;
		}
		/// <summary>Read an <see cref="System.SByte"/> from the stream</summary>
		/// <param name="bitCount">Number of bits to read</param>
		/// <param name="signExtend">If true, the result will have the MSB extended</param>
		/// <returns></returns>
		public sbyte ReadSByte(int bitCount = Bits.kSByteBitCount
			, bool signExtend = false
			)
		{
			Contract.Requires(bitCount <= Bits.kSByteBitCount);

			this.ReadWord(out TWord word, bitCount);
			if (signExtend && bitCount != Bits.kSByteBitCount)
				return (sbyte)Bits.SignExtend( (sbyte)word, bitCount );

			return (sbyte)word;
		}
		/// <summary>Read an <see cref="System.UInt16"/> from the stream</summary>
		/// <param name="bitCount">Number of bits to read</param>
		/// <returns></returns>
		public ushort ReadUInt16(int bitCount = Bits.kUInt16BitCount
			)
		{
			Contract.Requires(bitCount <= Bits.kUInt16BitCount);

			this.ReadWord(out TWord word, bitCount);

			return (ushort)word;
		}
		/// <summary>Read an <see cref="System.Int16"/> from the stream</summary>
		/// <param name="bitCount">Number of bits to read</param>
		/// <param name="signExtend">If true, the result will have the MSB extended</param>
		/// <returns></returns>
		public short ReadInt16(int bitCount = Bits.kInt16BitCount
			, bool signExtend = false
			)
		{
			Contract.Requires(bitCount <= Bits.kInt16BitCount);

			this.ReadWord(out TWord word, bitCount);
			if (signExtend && bitCount != Bits.kInt16BitCount)
				return (short)Bits.SignExtend( (short)word, bitCount );

			return (short)word;
		}
		/// <summary>Read an <see cref="System.UInt32"/> from the stream</summary>
		/// <param name="bitCount">Number of bits to read</param>
		/// <returns></returns>
		public uint ReadUInt32(int bitCount = Bits.kUInt32BitCount
			)
		{
			Contract.Requires(bitCount <= Bits.kUInt32BitCount);

			this.ReadWord(out TWord word, bitCount);

			return (uint)word;
		}
		/// <summary>Read an <see cref="System.Int32"/> from the stream</summary>
		/// <param name="bitCount">Number of bits to read</param>
		/// <param name="signExtend">If true, the result will have the MSB extended</param>
		/// <returns></returns>
		public int ReadInt32(int bitCount = Bits.kInt32BitCount
			, bool signExtend = false
			)
		{
			Contract.Requires(bitCount <= Bits.kInt32BitCount);

			this.ReadWord(out TWord word, bitCount);
			if (signExtend && bitCount != Bits.kInt32BitCount)
				return (int)Bits.SignExtend( (int)word, bitCount );

			return (int)word;
		}
		/// <summary>Read an <see cref="System.UInt64"/> from the stream</summary>
		/// <param name="bitCount">Number of bits to read</param>
		/// <returns></returns>
		public ulong ReadUInt64(int bitCount = Bits.kUInt64BitCount
			)
		{
			Contract.Requires(bitCount <= Bits.kUInt64BitCount);

			uint msb_word = 0;
			int msb_bit_count = bitCount > Bits.kInt32BitCount ? bitCount - Bits.kInt32BitCount : 0;
			int lsb_bit_count = bitCount > Bits.kInt32BitCount ? bitCount - msb_bit_count : bitCount;

			if (msb_bit_count > 0)
				this.ReadWord(out msb_word, msb_bit_count);
			this.ReadWord(out uint lsb_word, lsb_bit_count);

			ulong word = (ulong)msb_word << lsb_bit_count;
			word |= (ulong)lsb_word;

			return (ulong)word;
		}
		/// <summary>Read an <see cref="System.Int64"/> from the stream</summary>
		/// <param name="bitCount">Number of bits to read</param>
		/// <param name="signExtend">If true, the result will have the MSB extended</param>
		/// <returns></returns>
		public long ReadInt64(int bitCount = Bits.kInt64BitCount
			, bool signExtend = false
			)
		{
			Contract.Requires(bitCount <= Bits.kInt64BitCount);

			uint msb_word = 0;
			int msb_bit_count = bitCount > Bits.kInt32BitCount ? bitCount - Bits.kInt32BitCount : 0;
			int lsb_bit_count = bitCount > Bits.kInt32BitCount ? bitCount - msb_bit_count : bitCount;

			if (msb_bit_count > 0)
				this.ReadWord(out msb_word, msb_bit_count);
			this.ReadWord(out uint lsb_word, lsb_bit_count);

			ulong word = (ulong)msb_word << lsb_bit_count;
			word |= (ulong)lsb_word;
			if (signExtend && bitCount != Bits.kInt64BitCount)
				return (long)Bits.SignExtend( (long)word, bitCount );

			return (long)word;
		}

		/// <summary>Read an <see cref="System.Char"/> from the stream</summary>
		/// <param name="value">value read from the stream</param>
		/// <param name="bitCount">Number of bits to read</param>
		public void Read(out char value, int bitCount = Bits.kCharBitCount
			)
		{
			Contract.Requires(bitCount <= Bits.kCharBitCount);

			value = this.ReadChar(bitCount);
		}
		/// <summary>Read an <see cref="System.Byte"/> from the stream</summary>
		/// <param name="value">value read from the stream</param>
		/// <param name="bitCount">Number of bits to read</param>
		public void Read(out byte value, int bitCount = Bits.kByteBitCount
			)
		{
			Contract.Requires(bitCount <= Bits.kByteBitCount);

			value = this.ReadByte(bitCount);
		}
		/// <summary>Read an <see cref="System.SByte"/> from the stream</summary>
		/// <param name="value">value read from the stream</param>
		/// <param name="bitCount">Number of bits to read</param>
		/// <param name="signExtend">If true, the result will have the MSB extended</param>
		public void Read(out sbyte value, int bitCount = Bits.kSByteBitCount
			, bool signExtend = false
			)
		{
			Contract.Requires(bitCount <= Bits.kSByteBitCount);

			value = this.ReadSByte(bitCount, signExtend);
		}
		/// <summary>Read an <see cref="System.UInt16"/> from the stream</summary>
		/// <param name="value">value read from the stream</param>
		/// <param name="bitCount">Number of bits to read</param>
		public void Read(out ushort value, int bitCount = Bits.kUInt16BitCount
			)
		{
			Contract.Requires(bitCount <= Bits.kUInt16BitCount);

			value = this.ReadUInt16(bitCount);
		}
		/// <summary>Read an <see cref="System.Int16"/> from the stream</summary>
		/// <param name="value">value read from the stream</param>
		/// <param name="bitCount">Number of bits to read</param>
		/// <param name="signExtend">If true, the result will have the MSB extended</param>
		public void Read(out short value, int bitCount = Bits.kInt16BitCount
			, bool signExtend = false
			)
		{
			Contract.Requires(bitCount <= Bits.kInt16BitCount);

			value = this.ReadInt16(bitCount, signExtend);
		}
		/// <summary>Read an <see cref="System.UInt32"/> from the stream</summary>
		/// <param name="value">value read from the stream</param>
		/// <param name="bitCount">Number of bits to read</param>
		public void Read(out uint value, int bitCount = Bits.kUInt32BitCount
			)
		{
			Contract.Requires(bitCount <= Bits.kUInt32BitCount);

			value = this.ReadUInt32(bitCount);
		}
		/// <summary>Read an <see cref="System.Int32"/> from the stream</summary>
		/// <param name="value">value read from the stream</param>
		/// <param name="bitCount">Number of bits to read</param>
		/// <param name="signExtend">If true, the result will have the MSB extended</param>
		public void Read(out int value, int bitCount = Bits.kInt32BitCount
			, bool signExtend = false
			)
		{
			Contract.Requires(bitCount <= Bits.kInt32BitCount);

			value = this.ReadInt32(bitCount, signExtend);
		}
		/// <summary>Read an <see cref="System.UInt64"/> from the stream</summary>
		/// <param name="value">value read from the stream</param>
		/// <param name="bitCount">Number of bits to read</param>
		public void Read(out ulong value, int bitCount = Bits.kUInt64BitCount
			)
		{
			Contract.Requires(bitCount <= Bits.kUInt64BitCount);

			value = this.ReadUInt64(bitCount);
		}
		/// <summary>Read an <see cref="System.Int64"/> from the stream</summary>
		/// <param name="value">value read from the stream</param>
		/// <param name="bitCount">Number of bits to read</param>
		/// <param name="signExtend">If true, the result will have the MSB extended</param>
		public void Read(out long value, int bitCount = Bits.kInt64BitCount
			, bool signExtend = false
			)
		{
			Contract.Requires(bitCount <= Bits.kInt64BitCount);

			value = this.ReadInt64(bitCount, signExtend);
		}
	};
}
