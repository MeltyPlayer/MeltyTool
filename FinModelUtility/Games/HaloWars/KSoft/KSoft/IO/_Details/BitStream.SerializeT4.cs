using System.Collections.Generic;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.IO
{
	partial class BitStream
	{
		/// <summary>Serialize an <see cref="System.Char"/> to/from the stream</summary>
		/// <param name="value">value to serialize</param>
		/// <param name="bitCount">Number of bits to use</param>
		/// <returns>Returns this instance</returns>
		public BitStream Stream(ref char value, int bitCount = Bits.kCharBitCount
			)
		{
			Contract.Requires(bitCount <= Bits.kCharBitCount);

				 if (IsReading) value = ReadChar(bitCount);
			else if (IsWriting) Write(value, bitCount);

			return this;
		}
		/// <summary>Serialize an <see cref="System.Byte"/> to/from the stream</summary>
		/// <param name="value">value to serialize</param>
		/// <param name="bitCount">Number of bits to use</param>
		/// <returns>Returns this instance</returns>
		public BitStream Stream(ref byte value, int bitCount = Bits.kByteBitCount
			)
		{
			Contract.Requires(bitCount <= Bits.kByteBitCount);

				 if (IsReading) value = ReadByte(bitCount);
			else if (IsWriting) Write(value, bitCount);

			return this;
		}
		/// <summary>Serialize an <see cref="System.SByte"/> to/from the stream</summary>
		/// <param name="value">value to serialize</param>
		/// <param name="bitCount">Number of bits to use</param>
		/// <param name="signExtend">If true, the result will have the MSB extended</param>
		/// <returns>Returns this instance</returns>
		public BitStream Stream(ref sbyte value, int bitCount = Bits.kSByteBitCount
			, bool signExtend = false
			)
		{
			Contract.Requires(bitCount <= Bits.kSByteBitCount);

				 if (IsReading) value = ReadSByte(bitCount, signExtend);
			else if (IsWriting) Write(value, bitCount);

			return this;
		}
		/// <summary>Serialize an <see cref="System.UInt16"/> to/from the stream</summary>
		/// <param name="value">value to serialize</param>
		/// <param name="bitCount">Number of bits to use</param>
		/// <returns>Returns this instance</returns>
		public BitStream Stream(ref ushort value, int bitCount = Bits.kUInt16BitCount
			)
		{
			Contract.Requires(bitCount <= Bits.kUInt16BitCount);

				 if (IsReading) value = ReadUInt16(bitCount);
			else if (IsWriting) Write(value, bitCount);

			return this;
		}
		/// <summary>Serialize an <see cref="System.Int16"/> to/from the stream</summary>
		/// <param name="value">value to serialize</param>
		/// <param name="bitCount">Number of bits to use</param>
		/// <param name="signExtend">If true, the result will have the MSB extended</param>
		/// <returns>Returns this instance</returns>
		public BitStream Stream(ref short value, int bitCount = Bits.kInt16BitCount
			, bool signExtend = false
			)
		{
			Contract.Requires(bitCount <= Bits.kInt16BitCount);

				 if (IsReading) value = ReadInt16(bitCount, signExtend);
			else if (IsWriting) Write(value, bitCount);

			return this;
		}
		/// <summary>Serialize an <see cref="System.UInt32"/> to/from the stream</summary>
		/// <param name="value">value to serialize</param>
		/// <param name="bitCount">Number of bits to use</param>
		/// <returns>Returns this instance</returns>
		public BitStream Stream(ref uint value, int bitCount = Bits.kUInt32BitCount
			)
		{
			Contract.Requires(bitCount <= Bits.kUInt32BitCount);

				 if (IsReading) value = ReadUInt32(bitCount);
			else if (IsWriting) Write(value, bitCount);

			return this;
		}
		/// <summary>Serialize an <see cref="System.Int32"/> to/from the stream</summary>
		/// <param name="value">value to serialize</param>
		/// <param name="bitCount">Number of bits to use</param>
		/// <param name="signExtend">If true, the result will have the MSB extended</param>
		/// <returns>Returns this instance</returns>
		public BitStream Stream(ref int value, int bitCount = Bits.kInt32BitCount
			, bool signExtend = false
			)
		{
			Contract.Requires(bitCount <= Bits.kInt32BitCount);

				 if (IsReading) value = ReadInt32(bitCount, signExtend);
			else if (IsWriting) Write(value, bitCount);

			return this;
		}
		/// <summary>Serialize an <see cref="System.UInt64"/> to/from the stream</summary>
		/// <param name="value">value to serialize</param>
		/// <param name="bitCount">Number of bits to use</param>
		/// <returns>Returns this instance</returns>
		public BitStream Stream(ref ulong value, int bitCount = Bits.kUInt64BitCount
			)
		{
			Contract.Requires(bitCount <= Bits.kUInt64BitCount);

				 if (IsReading) value = ReadUInt64(bitCount);
			else if (IsWriting) Write(value, bitCount);

			return this;
		}
		/// <summary>Serialize an <see cref="System.Int64"/> to/from the stream</summary>
		/// <param name="value">value to serialize</param>
		/// <param name="bitCount">Number of bits to use</param>
		/// <param name="signExtend">If true, the result will have the MSB extended</param>
		/// <returns>Returns this instance</returns>
		public BitStream Stream(ref long value, int bitCount = Bits.kInt64BitCount
			, bool signExtend = false
			)
		{
			Contract.Requires(bitCount <= Bits.kInt64BitCount);

				 if (IsReading) value = ReadInt64(bitCount, signExtend);
			else if (IsWriting) Write(value, bitCount);

			return this;
		}

		/// <summary>Serialize an <see cref="System.Boolean"/> to/from the stream</summary>
		/// <param name="value">value to serialize</param>
		/// <returns>Returns this instance</returns>
		public BitStream Stream(ref bool value)
		{
				 if (IsReading) value = ReadBoolean();
			else if (IsWriting) Write(value);

			return this;
		}
		/// <summary>Serialize an <see cref="System.Single"/> to/from the stream</summary>
		/// <param name="value">value to serialize</param>
		/// <returns>Returns this instance</returns>
		public BitStream Stream(ref float value)
		{
				 if (IsReading) value = ReadSingle();
			else if (IsWriting) Write(value);

			return this;
		}
		/// <summary>Serialize an <see cref="System.Double"/> to/from the stream</summary>
		/// <param name="value">value to serialize</param>
		/// <returns>Returns this instance</returns>
		public BitStream Stream(ref double value)
		{
				 if (IsReading) value = ReadDouble();
			else if (IsWriting) Write(value);

			return this;
		}


		#region StreamFixedArray
		public BitStream StreamFixedArray(char[] array,
			int elementBitSize = Bits.kCharBitCount
			)
		{
			Contract.Requires(array != null);
			Contract.Requires(elementBitSize <= Bits.kCharBitCount);

			for (int x = 0; x < array.Length; x++) Stream(ref array[x], elementBitSize);

			return this;
		}
		public BitStream StreamFixedArray(byte[] array,
			int elementBitSize = Bits.kByteBitCount
			)
		{
			Contract.Requires(array != null);
			Contract.Requires(elementBitSize <= Bits.kByteBitCount);

			for (int x = 0; x < array.Length; x++) Stream(ref array[x], elementBitSize);

			return this;
		}
		public BitStream StreamFixedArray(sbyte[] array,
			int elementBitSize = Bits.kSByteBitCount
			, bool signExtend = false
			)
		{
			Contract.Requires(array != null);
			Contract.Requires(elementBitSize <= Bits.kSByteBitCount);

			for (int x = 0; x < array.Length; x++) Stream(ref array[x], elementBitSize, signExtend);

			return this;
		}
		public BitStream StreamFixedArray(ushort[] array,
			int elementBitSize = Bits.kUInt16BitCount
			)
		{
			Contract.Requires(array != null);
			Contract.Requires(elementBitSize <= Bits.kUInt16BitCount);

			for (int x = 0; x < array.Length; x++) Stream(ref array[x], elementBitSize);

			return this;
		}
		public BitStream StreamFixedArray(short[] array,
			int elementBitSize = Bits.kInt16BitCount
			, bool signExtend = false
			)
		{
			Contract.Requires(array != null);
			Contract.Requires(elementBitSize <= Bits.kInt16BitCount);

			for (int x = 0; x < array.Length; x++) Stream(ref array[x], elementBitSize, signExtend);

			return this;
		}
		public BitStream StreamFixedArray(uint[] array,
			int elementBitSize = Bits.kUInt32BitCount
			)
		{
			Contract.Requires(array != null);
			Contract.Requires(elementBitSize <= Bits.kUInt32BitCount);

			for (int x = 0; x < array.Length; x++) Stream(ref array[x], elementBitSize);

			return this;
		}
		public BitStream StreamFixedArray(int[] array,
			int elementBitSize = Bits.kInt32BitCount
			, bool signExtend = false
			)
		{
			Contract.Requires(array != null);
			Contract.Requires(elementBitSize <= Bits.kInt32BitCount);

			for (int x = 0; x < array.Length; x++) Stream(ref array[x], elementBitSize, signExtend);

			return this;
		}
		public BitStream StreamFixedArray(ulong[] array,
			int elementBitSize = Bits.kUInt64BitCount
			)
		{
			Contract.Requires(array != null);
			Contract.Requires(elementBitSize <= Bits.kUInt64BitCount);

			for (int x = 0; x < array.Length; x++) Stream(ref array[x], elementBitSize);

			return this;
		}
		public BitStream StreamFixedArray(long[] array,
			int elementBitSize = Bits.kInt64BitCount
			, bool signExtend = false
			)
		{
			Contract.Requires(array != null);
			Contract.Requires(elementBitSize <= Bits.kInt64BitCount);

			for (int x = 0; x < array.Length; x++) Stream(ref array[x], elementBitSize, signExtend);

			return this;
		}

		public BitStream StreamFixedArray(bool[] array)
		{
			Contract.Requires(array != null);

			for (int x = 0; x < array.Length; x++) Stream(ref array[x]);

			return this;
		}
		public BitStream StreamFixedArray(float[] array)
		{
			Contract.Requires(array != null);

			for (int x = 0; x < array.Length; x++) Stream(ref array[x]);

			return this;
		}
		public BitStream StreamFixedArray(double[] array)
		{
			Contract.Requires(array != null);

			for (int x = 0; x < array.Length; x++) Stream(ref array[x]);

			return this;
		}
		#endregion


		#region StreamArray
		public BitStream StreamArray(ref char[] array,
			int lengthBitSize, int elementBitSize = Bits.kCharBitCount
			)
		{
			Contract.Requires(IsReading || array != null);
			Contract.Requires(lengthBitSize <= Bits.kInt32BitCount);
			Contract.Requires(elementBitSize <= Bits.kCharBitCount);

			int count = IsReading ? 0 : array.Length;
			Stream(ref count, lengthBitSize);

			if (IsReading)
				array = new char[count];

			for (int x = 0; x < count; x++) Stream(ref array[x], elementBitSize);

			return this;
		}
		public BitStream StreamArray(ref byte[] array,
			int lengthBitSize, int elementBitSize = Bits.kByteBitCount
			)
		{
			Contract.Requires(IsReading || array != null);
			Contract.Requires(lengthBitSize <= Bits.kInt32BitCount);
			Contract.Requires(elementBitSize <= Bits.kByteBitCount);

			int count = IsReading ? 0 : array.Length;
			Stream(ref count, lengthBitSize);

			if (IsReading)
				array = new byte[count];

			for (int x = 0; x < count; x++) Stream(ref array[x], elementBitSize);

			return this;
		}
		public BitStream StreamArray(ref sbyte[] array,
			int lengthBitSize, int elementBitSize = Bits.kSByteBitCount
			, bool signExtend = false
			)
		{
			Contract.Requires(IsReading || array != null);
			Contract.Requires(lengthBitSize <= Bits.kInt32BitCount);
			Contract.Requires(elementBitSize <= Bits.kSByteBitCount);

			int count = IsReading ? 0 : array.Length;
			Stream(ref count, lengthBitSize);

			if (IsReading)
				array = new sbyte[count];

			for (int x = 0; x < count; x++) Stream(ref array[x], elementBitSize, signExtend);

			return this;
		}
		public BitStream StreamArray(ref ushort[] array,
			int lengthBitSize, int elementBitSize = Bits.kUInt16BitCount
			)
		{
			Contract.Requires(IsReading || array != null);
			Contract.Requires(lengthBitSize <= Bits.kInt32BitCount);
			Contract.Requires(elementBitSize <= Bits.kUInt16BitCount);

			int count = IsReading ? 0 : array.Length;
			Stream(ref count, lengthBitSize);

			if (IsReading)
				array = new ushort[count];

			for (int x = 0; x < count; x++) Stream(ref array[x], elementBitSize);

			return this;
		}
		public BitStream StreamArray(ref short[] array,
			int lengthBitSize, int elementBitSize = Bits.kInt16BitCount
			, bool signExtend = false
			)
		{
			Contract.Requires(IsReading || array != null);
			Contract.Requires(lengthBitSize <= Bits.kInt32BitCount);
			Contract.Requires(elementBitSize <= Bits.kInt16BitCount);

			int count = IsReading ? 0 : array.Length;
			Stream(ref count, lengthBitSize);

			if (IsReading)
				array = new short[count];

			for (int x = 0; x < count; x++) Stream(ref array[x], elementBitSize, signExtend);

			return this;
		}
		public BitStream StreamArray(ref uint[] array,
			int lengthBitSize, int elementBitSize = Bits.kUInt32BitCount
			)
		{
			Contract.Requires(IsReading || array != null);
			Contract.Requires(lengthBitSize <= Bits.kInt32BitCount);
			Contract.Requires(elementBitSize <= Bits.kUInt32BitCount);

			int count = IsReading ? 0 : array.Length;
			Stream(ref count, lengthBitSize);

			if (IsReading)
				array = new uint[count];

			for (int x = 0; x < count; x++) Stream(ref array[x], elementBitSize);

			return this;
		}
		public BitStream StreamArray(ref int[] array,
			int lengthBitSize, int elementBitSize = Bits.kInt32BitCount
			, bool signExtend = false
			)
		{
			Contract.Requires(IsReading || array != null);
			Contract.Requires(lengthBitSize <= Bits.kInt32BitCount);
			Contract.Requires(elementBitSize <= Bits.kInt32BitCount);

			int count = IsReading ? 0 : array.Length;
			Stream(ref count, lengthBitSize);

			if (IsReading)
				array = new int[count];

			for (int x = 0; x < count; x++) Stream(ref array[x], elementBitSize, signExtend);

			return this;
		}
		public BitStream StreamArray(ref ulong[] array,
			int lengthBitSize, int elementBitSize = Bits.kUInt64BitCount
			)
		{
			Contract.Requires(IsReading || array != null);
			Contract.Requires(lengthBitSize <= Bits.kInt32BitCount);
			Contract.Requires(elementBitSize <= Bits.kUInt64BitCount);

			int count = IsReading ? 0 : array.Length;
			Stream(ref count, lengthBitSize);

			if (IsReading)
				array = new ulong[count];

			for (int x = 0; x < count; x++) Stream(ref array[x], elementBitSize);

			return this;
		}
		public BitStream StreamArray(ref long[] array,
			int lengthBitSize, int elementBitSize = Bits.kInt64BitCount
			, bool signExtend = false
			)
		{
			Contract.Requires(IsReading || array != null);
			Contract.Requires(lengthBitSize <= Bits.kInt32BitCount);
			Contract.Requires(elementBitSize <= Bits.kInt64BitCount);

			int count = IsReading ? 0 : array.Length;
			Stream(ref count, lengthBitSize);

			if (IsReading)
				array = new long[count];

			for (int x = 0; x < count; x++) Stream(ref array[x], elementBitSize, signExtend);

			return this;
		}

		public BitStream StreamArray(ref bool[] array,
			int lengthBitSize)
		{
			Contract.Requires(IsReading || array != null);
			Contract.Requires(lengthBitSize <= Bits.kInt32BitCount);

			int count = IsReading ? 0 : array.Length;
			Stream(ref count, lengthBitSize);

			if (IsReading)
				array = new bool[count];

			for (int x = 0; x < count; x++) Stream(ref array[x]);

			return this;
		}
		public BitStream StreamArray(ref float[] array,
			int lengthBitSize)
		{
			Contract.Requires(IsReading || array != null);
			Contract.Requires(lengthBitSize <= Bits.kInt32BitCount);

			int count = IsReading ? 0 : array.Length;
			Stream(ref count, lengthBitSize);

			if (IsReading)
				array = new float[count];

			for (int x = 0; x < count; x++) Stream(ref array[x]);

			return this;
		}
		public BitStream StreamArray(ref double[] array,
			int lengthBitSize)
		{
			Contract.Requires(IsReading || array != null);
			Contract.Requires(lengthBitSize <= Bits.kInt32BitCount);

			int count = IsReading ? 0 : array.Length;
			Stream(ref count, lengthBitSize);

			if (IsReading)
				array = new double[count];

			for (int x = 0; x < count; x++) Stream(ref array[x]);

			return this;
		}
		#endregion


		#region StreamList
		public BitStream StreamElements(ICollection< char > list,
			int countBitSize, int elementBitSize = Bits.kCharBitCount
			)
		{
			Contract.Requires(list != null);
			Contract.Requires(countBitSize <= Bits.kInt32BitCount);
			Contract.Requires(elementBitSize <= Bits.kCharBitCount);

			int count = list.Count;
			Stream(ref count, countBitSize);

			if (IsReading)
			{
				for (int x = 0; x < count; x++)
				{
					var value = ReadChar(elementBitSize);
					list.Add(value);
				}
			}
			else if (IsWriting)
			{
				foreach (var value in list)
					Write(value, elementBitSize);
			}

			return this;
		}
		public BitStream StreamElements(ICollection< byte > list,
			int countBitSize, int elementBitSize = Bits.kByteBitCount
			)
		{
			Contract.Requires(list != null);
			Contract.Requires(countBitSize <= Bits.kInt32BitCount);
			Contract.Requires(elementBitSize <= Bits.kByteBitCount);

			int count = list.Count;
			Stream(ref count, countBitSize);

			if (IsReading)
			{
				for (int x = 0; x < count; x++)
				{
					var value = ReadByte(elementBitSize);
					list.Add(value);
				}
			}
			else if (IsWriting)
			{
				foreach (var value in list)
					Write(value, elementBitSize);
			}

			return this;
		}
		public BitStream StreamElements(ICollection< sbyte > list,
			int countBitSize, int elementBitSize = Bits.kSByteBitCount
			, bool signExtend = false
			)
		{
			Contract.Requires(list != null);
			Contract.Requires(countBitSize <= Bits.kInt32BitCount);
			Contract.Requires(elementBitSize <= Bits.kSByteBitCount);

			int count = list.Count;
			Stream(ref count, countBitSize);

			if (IsReading)
			{
				for (int x = 0; x < count; x++)
				{
					var value = ReadSByte(elementBitSize, signExtend);
					list.Add(value);
				}
			}
			else if (IsWriting)
			{
				foreach (var value in list)
					Write(value, elementBitSize);
			}

			return this;
		}
		public BitStream StreamElements(ICollection< ushort > list,
			int countBitSize, int elementBitSize = Bits.kUInt16BitCount
			)
		{
			Contract.Requires(list != null);
			Contract.Requires(countBitSize <= Bits.kInt32BitCount);
			Contract.Requires(elementBitSize <= Bits.kUInt16BitCount);

			int count = list.Count;
			Stream(ref count, countBitSize);

			if (IsReading)
			{
				for (int x = 0; x < count; x++)
				{
					var value = ReadUInt16(elementBitSize);
					list.Add(value);
				}
			}
			else if (IsWriting)
			{
				foreach (var value in list)
					Write(value, elementBitSize);
			}

			return this;
		}
		public BitStream StreamElements(ICollection< short > list,
			int countBitSize, int elementBitSize = Bits.kInt16BitCount
			, bool signExtend = false
			)
		{
			Contract.Requires(list != null);
			Contract.Requires(countBitSize <= Bits.kInt32BitCount);
			Contract.Requires(elementBitSize <= Bits.kInt16BitCount);

			int count = list.Count;
			Stream(ref count, countBitSize);

			if (IsReading)
			{
				for (int x = 0; x < count; x++)
				{
					var value = ReadInt16(elementBitSize, signExtend);
					list.Add(value);
				}
			}
			else if (IsWriting)
			{
				foreach (var value in list)
					Write(value, elementBitSize);
			}

			return this;
		}
		public BitStream StreamElements(ICollection< uint > list,
			int countBitSize, int elementBitSize = Bits.kUInt32BitCount
			)
		{
			Contract.Requires(list != null);
			Contract.Requires(countBitSize <= Bits.kInt32BitCount);
			Contract.Requires(elementBitSize <= Bits.kUInt32BitCount);

			int count = list.Count;
			Stream(ref count, countBitSize);

			if (IsReading)
			{
				for (int x = 0; x < count; x++)
				{
					var value = ReadUInt32(elementBitSize);
					list.Add(value);
				}
			}
			else if (IsWriting)
			{
				foreach (var value in list)
					Write(value, elementBitSize);
			}

			return this;
		}
		public BitStream StreamElements(ICollection< int > list,
			int countBitSize, int elementBitSize = Bits.kInt32BitCount
			, bool signExtend = false
			)
		{
			Contract.Requires(list != null);
			Contract.Requires(countBitSize <= Bits.kInt32BitCount);
			Contract.Requires(elementBitSize <= Bits.kInt32BitCount);

			int count = list.Count;
			Stream(ref count, countBitSize);

			if (IsReading)
			{
				for (int x = 0; x < count; x++)
				{
					var value = ReadInt32(elementBitSize, signExtend);
					list.Add(value);
				}
			}
			else if (IsWriting)
			{
				foreach (var value in list)
					Write(value, elementBitSize);
			}

			return this;
		}
		public BitStream StreamElements(ICollection< ulong > list,
			int countBitSize, int elementBitSize = Bits.kUInt64BitCount
			)
		{
			Contract.Requires(list != null);
			Contract.Requires(countBitSize <= Bits.kInt32BitCount);
			Contract.Requires(elementBitSize <= Bits.kUInt64BitCount);

			int count = list.Count;
			Stream(ref count, countBitSize);

			if (IsReading)
			{
				for (int x = 0; x < count; x++)
				{
					var value = ReadUInt64(elementBitSize);
					list.Add(value);
				}
			}
			else if (IsWriting)
			{
				foreach (var value in list)
					Write(value, elementBitSize);
			}

			return this;
		}
		public BitStream StreamElements(ICollection< long > list,
			int countBitSize, int elementBitSize = Bits.kInt64BitCount
			, bool signExtend = false
			)
		{
			Contract.Requires(list != null);
			Contract.Requires(countBitSize <= Bits.kInt32BitCount);
			Contract.Requires(elementBitSize <= Bits.kInt64BitCount);

			int count = list.Count;
			Stream(ref count, countBitSize);

			if (IsReading)
			{
				for (int x = 0; x < count; x++)
				{
					var value = ReadInt64(elementBitSize, signExtend);
					list.Add(value);
				}
			}
			else if (IsWriting)
			{
				foreach (var value in list)
					Write(value, elementBitSize);
			}

			return this;
		}

		public BitStream StreamElements(ICollection< bool > list,
			int countBitSize)
		{
			Contract.Requires(list != null);
			Contract.Requires(countBitSize <= Bits.kInt32BitCount);

			int count = list.Count;
			Stream(ref count, countBitSize);

			if (IsReading)
			{
				for (int x = 0; x < count; x++)
				{
					var value = ReadBoolean();
					list.Add(value);
				}
			}
			else if (IsWriting)
			{
				foreach (var value in list)
					Write(value);
			}

			return this;
		}
		public BitStream StreamElements(ICollection< float > list,
			int countBitSize)
		{
			Contract.Requires(list != null);
			Contract.Requires(countBitSize <= Bits.kInt32BitCount);

			int count = list.Count;
			Stream(ref count, countBitSize);

			if (IsReading)
			{
				for (int x = 0; x < count; x++)
				{
					var value = ReadSingle();
					list.Add(value);
				}
			}
			else if (IsWriting)
			{
				foreach (var value in list)
					Write(value);
			}

			return this;
		}
		public BitStream StreamElements(ICollection< double > list,
			int countBitSize)
		{
			Contract.Requires(list != null);
			Contract.Requires(countBitSize <= Bits.kInt32BitCount);

			int count = list.Count;
			Stream(ref count, countBitSize);

			if (IsReading)
			{
				for (int x = 0; x < count; x++)
				{
					var value = ReadDouble();
					list.Add(value);
				}
			}
			else if (IsWriting)
			{
				foreach (var value in list)
					Write(value);
			}

			return this;
		}
		#endregion
	};
}
