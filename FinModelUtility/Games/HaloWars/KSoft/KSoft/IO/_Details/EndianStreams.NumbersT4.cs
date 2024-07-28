#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.IO
{
	partial class EndianReader
	{
		/// <summary>Reads a unsigned 16-bit integer</summary>
		/// <returns></returns>
		/// <seealso cref="System.IO.BinaryReader.ReadUInt16()"/>
		public override ushort ReadUInt16()
		{
			var value = base.ReadUInt16();
			return !this.mRequiresByteSwap
				? value
				: Bitwise.ByteSwap.SwapUInt16(value);
		}

		/// <summary>Reads a signed 16-bit integer</summary>
		/// <returns></returns>
		/// <seealso cref="System.IO.BinaryReader.ReadInt16()"/>
		public override short ReadInt16()
		{
			var value = base.ReadInt16();
			return !this.mRequiresByteSwap
				? value
				: Bitwise.ByteSwap.SwapInt16(value);
		}

		/// <summary>Reads a unsigned 32-bit integer</summary>
		/// <returns></returns>
		/// <seealso cref="System.IO.BinaryReader.ReadUInt32()"/>
		public override uint ReadUInt32()
		{
			var value = base.ReadUInt32();
			return !this.mRequiresByteSwap
				? value
				: Bitwise.ByteSwap.SwapUInt32(value);
		}

		/// <summary>Reads a signed 32-bit integer</summary>
		/// <returns></returns>
		/// <seealso cref="System.IO.BinaryReader.ReadInt32()"/>
		public override int ReadInt32()
		{
			var value = base.ReadInt32();
			return !this.mRequiresByteSwap
				? value
				: Bitwise.ByteSwap.SwapInt32(value);
		}

		/// <summary>Reads a unsigned 64-bit integer</summary>
		/// <returns></returns>
		/// <seealso cref="System.IO.BinaryReader.ReadUInt64()"/>
		public override ulong ReadUInt64()
		{
			var value = base.ReadUInt64();
			return !this.mRequiresByteSwap
				? value
				: Bitwise.ByteSwap.SwapUInt64(value);
		}

		/// <summary>Reads a signed 64-bit integer</summary>
		/// <returns></returns>
		/// <seealso cref="System.IO.BinaryReader.ReadInt64()"/>
		public override long ReadInt64()
		{
			var value = base.ReadInt64();
			return !this.mRequiresByteSwap
				? value
				: Bitwise.ByteSwap.SwapInt64(value);
		}

		/// <summary>Reads a single-precision number</summary>
		/// <returns></returns>
		/// <seealso cref="System.IO.BinaryReader.ReadSingle()"/>
		public override float ReadSingle()
		{
			var value = base.ReadSingle();
			return !this.mRequiresByteSwap
				? value
				: Bitwise.ByteSwap.SwapSingle(value);
		}

		/// <summary>Reads a double-precision number</summary>
		/// <returns></returns>
		/// <seealso cref="System.IO.BinaryReader.ReadDouble()"/>
		public override double ReadDouble()
		{
			var value = base.ReadDouble();
			return !this.mRequiresByteSwap
				? value
				: Bitwise.ByteSwap.SwapDouble(value);
		}


		#region ReadFixedArray
		public byte[] ReadFixedArray(byte[] array, int startIndex, int length)
		{
			Contract.Requires(array != null);
			Contract.Requires(startIndex >= 0);
			Contract.Requires(length >= 0);
			Contract.Ensures(Contract.Result<byte[]>() != null);

			for (int x = startIndex, end = startIndex+length; x < end; x++)
				array[x] = this.ReadByte();

			return array;
		}
		public byte[] ReadFixedArray(byte[] array)
		{
			Contract.Requires(array != null);
			Contract.Ensures(Contract.Result<byte[]>() != null);

			return this.ReadFixedArray(array, 0, array.Length);
		}

		public sbyte[] ReadFixedArray(sbyte[] array, int startIndex, int length)
		{
			Contract.Requires(array != null);
			Contract.Requires(startIndex >= 0);
			Contract.Requires(length >= 0);
			Contract.Ensures(Contract.Result<sbyte[]>() != null);

			for (int x = startIndex, end = startIndex+length; x < end; x++)
				array[x] = this.ReadSByte();

			return array;
		}
		public sbyte[] ReadFixedArray(sbyte[] array)
		{
			Contract.Requires(array != null);
			Contract.Ensures(Contract.Result<sbyte[]>() != null);

			return this.ReadFixedArray(array, 0, array.Length);
		}

		public ushort[] ReadFixedArray(ushort[] array, int startIndex, int length)
		{
			Contract.Requires(array != null);
			Contract.Requires(startIndex >= 0);
			Contract.Requires(length >= 0);
			Contract.Ensures(Contract.Result<ushort[]>() != null);

			for (int x = startIndex, end = startIndex+length; x < end; x++)
				array[x] = this.ReadUInt16();

			return array;
		}
		public ushort[] ReadFixedArray(ushort[] array)
		{
			Contract.Requires(array != null);
			Contract.Ensures(Contract.Result<ushort[]>() != null);

			return this.ReadFixedArray(array, 0, array.Length);
		}

		public short[] ReadFixedArray(short[] array, int startIndex, int length)
		{
			Contract.Requires(array != null);
			Contract.Requires(startIndex >= 0);
			Contract.Requires(length >= 0);
			Contract.Ensures(Contract.Result<short[]>() != null);

			for (int x = startIndex, end = startIndex+length; x < end; x++)
				array[x] = this.ReadInt16();

			return array;
		}
		public short[] ReadFixedArray(short[] array)
		{
			Contract.Requires(array != null);
			Contract.Ensures(Contract.Result<short[]>() != null);

			return this.ReadFixedArray(array, 0, array.Length);
		}

		public uint[] ReadFixedArray(uint[] array, int startIndex, int length)
		{
			Contract.Requires(array != null);
			Contract.Requires(startIndex >= 0);
			Contract.Requires(length >= 0);
			Contract.Ensures(Contract.Result<uint[]>() != null);

			for (int x = startIndex, end = startIndex+length; x < end; x++)
				array[x] = this.ReadUInt32();

			return array;
		}
		public uint[] ReadFixedArray(uint[] array)
		{
			Contract.Requires(array != null);
			Contract.Ensures(Contract.Result<uint[]>() != null);

			return this.ReadFixedArray(array, 0, array.Length);
		}

		public int[] ReadFixedArray(int[] array, int startIndex, int length)
		{
			Contract.Requires(array != null);
			Contract.Requires(startIndex >= 0);
			Contract.Requires(length >= 0);
			Contract.Ensures(Contract.Result<int[]>() != null);

			for (int x = startIndex, end = startIndex+length; x < end; x++)
				array[x] = this.ReadInt32();

			return array;
		}
		public int[] ReadFixedArray(int[] array)
		{
			Contract.Requires(array != null);
			Contract.Ensures(Contract.Result<int[]>() != null);

			return this.ReadFixedArray(array, 0, array.Length);
		}

		public ulong[] ReadFixedArray(ulong[] array, int startIndex, int length)
		{
			Contract.Requires(array != null);
			Contract.Requires(startIndex >= 0);
			Contract.Requires(length >= 0);
			Contract.Ensures(Contract.Result<ulong[]>() != null);

			for (int x = startIndex, end = startIndex+length; x < end; x++)
				array[x] = this.ReadUInt64();

			return array;
		}
		public ulong[] ReadFixedArray(ulong[] array)
		{
			Contract.Requires(array != null);
			Contract.Ensures(Contract.Result<ulong[]>() != null);

			return this.ReadFixedArray(array, 0, array.Length);
		}

		public long[] ReadFixedArray(long[] array, int startIndex, int length)
		{
			Contract.Requires(array != null);
			Contract.Requires(startIndex >= 0);
			Contract.Requires(length >= 0);
			Contract.Ensures(Contract.Result<long[]>() != null);

			for (int x = startIndex, end = startIndex+length; x < end; x++)
				array[x] = this.ReadInt64();

			return array;
		}
		public long[] ReadFixedArray(long[] array)
		{
			Contract.Requires(array != null);
			Contract.Ensures(Contract.Result<long[]>() != null);

			return this.ReadFixedArray(array, 0, array.Length);
		}

		public float[] ReadFixedArray(float[] array, int startIndex, int length)
		{
			Contract.Requires(array != null);
			Contract.Requires(startIndex >= 0);
			Contract.Requires(length >= 0);
			Contract.Ensures(Contract.Result<float[]>() != null);

			for (int x = startIndex, end = startIndex+length; x < end; x++)
				array[x] = this.ReadSingle();

			return array;
		}
		public float[] ReadFixedArray(float[] array)
		{
			Contract.Requires(array != null);
			Contract.Ensures(Contract.Result<float[]>() != null);

			return this.ReadFixedArray(array, 0, array.Length);
		}

		public double[] ReadFixedArray(double[] array, int startIndex, int length)
		{
			Contract.Requires(array != null);
			Contract.Requires(startIndex >= 0);
			Contract.Requires(length >= 0);
			Contract.Ensures(Contract.Result<double[]>() != null);

			for (int x = startIndex, end = startIndex+length; x < end; x++)
				array[x] = this.ReadDouble();

			return array;
		}
		public double[] ReadFixedArray(double[] array)
		{
			Contract.Requires(array != null);
			Contract.Ensures(Contract.Result<double[]>() != null);

			return this.ReadFixedArray(array, 0, array.Length);
		}

		#endregion
	};

	partial class EndianWriter
	{
		/// <summary>Writes a unsigned 16-bit integer</summary>
		/// <param name="value"></param>
		/// <seealso cref="System.IO.BinaryWriter.Write(ushort)"/>
		public override void Write(ushort value)
		{
			base.Write(!this.mRequiresByteSwap
				? value
				: Bitwise.ByteSwap.SwapUInt16(value));
		}

		/// <summary>Writes a signed 16-bit integer</summary>
		/// <param name="value"></param>
		/// <seealso cref="System.IO.BinaryWriter.Write(short)"/>
		public override void Write(short value)
		{
			base.Write(!this.mRequiresByteSwap
				? value
				: Bitwise.ByteSwap.SwapInt16(value));
		}

		/// <summary>Writes a unsigned 32-bit integer</summary>
		/// <param name="value"></param>
		/// <seealso cref="System.IO.BinaryWriter.Write(uint)"/>
		public override void Write(uint value)
		{
			base.Write(!this.mRequiresByteSwap
				? value
				: Bitwise.ByteSwap.SwapUInt32(value));
		}

		/// <summary>Writes a signed 32-bit integer</summary>
		/// <param name="value"></param>
		/// <seealso cref="System.IO.BinaryWriter.Write(int)"/>
		public override void Write(int value)
		{
			base.Write(!this.mRequiresByteSwap
				? value
				: Bitwise.ByteSwap.SwapInt32(value));
		}

		/// <summary>Writes a unsigned 64-bit integer</summary>
		/// <param name="value"></param>
		/// <seealso cref="System.IO.BinaryWriter.Write(ulong)"/>
		public override void Write(ulong value)
		{
			base.Write(!this.mRequiresByteSwap
				? value
				: Bitwise.ByteSwap.SwapUInt64(value));
		}

		/// <summary>Writes a signed 64-bit integer</summary>
		/// <param name="value"></param>
		/// <seealso cref="System.IO.BinaryWriter.Write(long)"/>
		public override void Write(long value)
		{
			base.Write(!this.mRequiresByteSwap
				? value
				: Bitwise.ByteSwap.SwapInt64(value));
		}

		/// <summary>Writes a single-precision number</summary>
		/// <param name="value"></param>
		/// <seealso cref="System.IO.BinaryWriter.Write(float)"/>
		public override void Write(float value)
		{
			base.Write(!this.mRequiresByteSwap
				? value
				: Bitwise.ByteSwap.SwapSingle(value));
		}

		/// <summary>Writes a double-precision number</summary>
		/// <param name="value"></param>
		/// <seealso cref="System.IO.BinaryWriter.Write(double)"/>
		public override void Write(double value)
		{
			base.Write(!this.mRequiresByteSwap
				? value
				: Bitwise.ByteSwap.SwapDouble(value));
		}


		#region WriteFixedArray
		public byte[] WriteFixedArray(byte[] array, int startIndex, int length)
		{
			Contract.Requires(array != null);
			Contract.Requires(startIndex >= 0);
			Contract.Requires(length >= 0);
			Contract.Ensures(Contract.Result<byte[]>() != null);

			for (int x = startIndex, end = startIndex+length; x < end; x++)
				this.Write(array[x]);

			return array;
		}
		public byte[] WriteFixedArray(byte[] array)
		{
			Contract.Requires(array != null);
			Contract.Ensures(Contract.Result<byte[]>() != null);

			return this.WriteFixedArray(array, 0, array.Length);
		}

		public sbyte[] WriteFixedArray(sbyte[] array, int startIndex, int length)
		{
			Contract.Requires(array != null);
			Contract.Requires(startIndex >= 0);
			Contract.Requires(length >= 0);
			Contract.Ensures(Contract.Result<sbyte[]>() != null);

			for (int x = startIndex, end = startIndex+length; x < end; x++)
				this.Write(array[x]);

			return array;
		}
		public sbyte[] WriteFixedArray(sbyte[] array)
		{
			Contract.Requires(array != null);
			Contract.Ensures(Contract.Result<sbyte[]>() != null);

			return this.WriteFixedArray(array, 0, array.Length);
		}

		public ushort[] WriteFixedArray(ushort[] array, int startIndex, int length)
		{
			Contract.Requires(array != null);
			Contract.Requires(startIndex >= 0);
			Contract.Requires(length >= 0);
			Contract.Ensures(Contract.Result<ushort[]>() != null);

			for (int x = startIndex, end = startIndex+length; x < end; x++)
				this.Write(array[x]);

			return array;
		}
		public ushort[] WriteFixedArray(ushort[] array)
		{
			Contract.Requires(array != null);
			Contract.Ensures(Contract.Result<ushort[]>() != null);

			return this.WriteFixedArray(array, 0, array.Length);
		}

		public short[] WriteFixedArray(short[] array, int startIndex, int length)
		{
			Contract.Requires(array != null);
			Contract.Requires(startIndex >= 0);
			Contract.Requires(length >= 0);
			Contract.Ensures(Contract.Result<short[]>() != null);

			for (int x = startIndex, end = startIndex+length; x < end; x++)
				this.Write(array[x]);

			return array;
		}
		public short[] WriteFixedArray(short[] array)
		{
			Contract.Requires(array != null);
			Contract.Ensures(Contract.Result<short[]>() != null);

			return this.WriteFixedArray(array, 0, array.Length);
		}

		public uint[] WriteFixedArray(uint[] array, int startIndex, int length)
		{
			Contract.Requires(array != null);
			Contract.Requires(startIndex >= 0);
			Contract.Requires(length >= 0);
			Contract.Ensures(Contract.Result<uint[]>() != null);

			for (int x = startIndex, end = startIndex+length; x < end; x++)
				this.Write(array[x]);

			return array;
		}
		public uint[] WriteFixedArray(uint[] array)
		{
			Contract.Requires(array != null);
			Contract.Ensures(Contract.Result<uint[]>() != null);

			return this.WriteFixedArray(array, 0, array.Length);
		}

		public int[] WriteFixedArray(int[] array, int startIndex, int length)
		{
			Contract.Requires(array != null);
			Contract.Requires(startIndex >= 0);
			Contract.Requires(length >= 0);
			Contract.Ensures(Contract.Result<int[]>() != null);

			for (int x = startIndex, end = startIndex+length; x < end; x++)
				this.Write(array[x]);

			return array;
		}
		public int[] WriteFixedArray(int[] array)
		{
			Contract.Requires(array != null);
			Contract.Ensures(Contract.Result<int[]>() != null);

			return this.WriteFixedArray(array, 0, array.Length);
		}

		public ulong[] WriteFixedArray(ulong[] array, int startIndex, int length)
		{
			Contract.Requires(array != null);
			Contract.Requires(startIndex >= 0);
			Contract.Requires(length >= 0);
			Contract.Ensures(Contract.Result<ulong[]>() != null);

			for (int x = startIndex, end = startIndex+length; x < end; x++)
				this.Write(array[x]);

			return array;
		}
		public ulong[] WriteFixedArray(ulong[] array)
		{
			Contract.Requires(array != null);
			Contract.Ensures(Contract.Result<ulong[]>() != null);

			return this.WriteFixedArray(array, 0, array.Length);
		}

		public long[] WriteFixedArray(long[] array, int startIndex, int length)
		{
			Contract.Requires(array != null);
			Contract.Requires(startIndex >= 0);
			Contract.Requires(length >= 0);
			Contract.Ensures(Contract.Result<long[]>() != null);

			for (int x = startIndex, end = startIndex+length; x < end; x++)
				this.Write(array[x]);

			return array;
		}
		public long[] WriteFixedArray(long[] array)
		{
			Contract.Requires(array != null);
			Contract.Ensures(Contract.Result<long[]>() != null);

			return this.WriteFixedArray(array, 0, array.Length);
		}

		public float[] WriteFixedArray(float[] array, int startIndex, int length)
		{
			Contract.Requires(array != null);
			Contract.Requires(startIndex >= 0);
			Contract.Requires(length >= 0);
			Contract.Ensures(Contract.Result<float[]>() != null);

			for (int x = startIndex, end = startIndex+length; x < end; x++)
				this.Write(array[x]);

			return array;
		}
		public float[] WriteFixedArray(float[] array)
		{
			Contract.Requires(array != null);
			Contract.Ensures(Contract.Result<float[]>() != null);

			return this.WriteFixedArray(array, 0, array.Length);
		}

		public double[] WriteFixedArray(double[] array, int startIndex, int length)
		{
			Contract.Requires(array != null);
			Contract.Requires(startIndex >= 0);
			Contract.Requires(length >= 0);
			Contract.Ensures(Contract.Result<double[]>() != null);

			for (int x = startIndex, end = startIndex+length; x < end; x++)
				this.Write(array[x]);

			return array;
		}
		public double[] WriteFixedArray(double[] array)
		{
			Contract.Requires(array != null);
			Contract.Ensures(Contract.Result<double[]>() != null);

			return this.WriteFixedArray(array, 0, array.Length);
		}

		#endregion
	};

	partial class EndianStream
	{
		public EndianStream Stream(ref byte value)
		{
				 if (this.IsReading) value = this.Reader.ReadByte();
			else if (this.IsWriting)
				this.Writer.Write(value);

			return this;
		}

		public EndianStream Stream(ref sbyte value)
		{
				 if (this.IsReading) value = this.Reader.ReadSByte();
			else if (this.IsWriting)
				this.Writer.Write(value);

			return this;
		}

		public EndianStream Stream(ref ushort value)
		{
				 if (this.IsReading) value = this.Reader.ReadUInt16();
			else if (this.IsWriting)
				this.Writer.Write(value);

			return this;
		}

		public EndianStream Stream(ref short value)
		{
				 if (this.IsReading) value = this.Reader.ReadInt16();
			else if (this.IsWriting)
				this.Writer.Write(value);

			return this;
		}

		public EndianStream Stream(ref uint value)
		{
				 if (this.IsReading) value = this.Reader.ReadUInt32();
			else if (this.IsWriting)
				this.Writer.Write(value);

			return this;
		}

		public EndianStream Stream(ref int value)
		{
				 if (this.IsReading) value = this.Reader.ReadInt32();
			else if (this.IsWriting)
				this.Writer.Write(value);

			return this;
		}

		public EndianStream Stream(ref ulong value)
		{
				 if (this.IsReading) value = this.Reader.ReadUInt64();
			else if (this.IsWriting)
				this.Writer.Write(value);

			return this;
		}

		public EndianStream Stream(ref long value)
		{
				 if (this.IsReading) value = this.Reader.ReadInt64();
			else if (this.IsWriting)
				this.Writer.Write(value);

			return this;
		}

		public EndianStream Stream(ref float value)
		{
				 if (this.IsReading) value = this.Reader.ReadSingle();
			else if (this.IsWriting)
				this.Writer.Write(value);

			return this;
		}

		public EndianStream Stream(ref double value)
		{
				 if (this.IsReading) value = this.Reader.ReadDouble();
			else if (this.IsWriting)
				this.Writer.Write(value);

			return this;
		}


		#region StreamFixedArray
		public EndianStream StreamFixedArray(byte[] array, int startIndex, int length)
		{
			Contract.Requires(array != null);
			Contract.Requires(startIndex >= 0);
			Contract.Requires(length >= 0);

				 if (this.IsReading)
					 this.Reader.ReadFixedArray(array, startIndex, length);
			else if (this.IsWriting)
				this.Writer.WriteFixedArray(array, startIndex, length);

			return this;
		}
		public EndianStream StreamFixedArray(byte[] array)
		{
			Contract.Requires(array != null);

			return this.StreamFixedArray(array, 0, array.Length);
		}

		public EndianStream StreamFixedArray(sbyte[] array, int startIndex, int length)
		{
			Contract.Requires(array != null);
			Contract.Requires(startIndex >= 0);
			Contract.Requires(length >= 0);

				 if (this.IsReading)
					 this.Reader.ReadFixedArray(array, startIndex, length);
			else if (this.IsWriting)
				this.Writer.WriteFixedArray(array, startIndex, length);

			return this;
		}
		public EndianStream StreamFixedArray(sbyte[] array)
		{
			Contract.Requires(array != null);

			return this.StreamFixedArray(array, 0, array.Length);
		}

		public EndianStream StreamFixedArray(ushort[] array, int startIndex, int length)
		{
			Contract.Requires(array != null);
			Contract.Requires(startIndex >= 0);
			Contract.Requires(length >= 0);

				 if (this.IsReading)
					 this.Reader.ReadFixedArray(array, startIndex, length);
			else if (this.IsWriting)
				this.Writer.WriteFixedArray(array, startIndex, length);

			return this;
		}
		public EndianStream StreamFixedArray(ushort[] array)
		{
			Contract.Requires(array != null);

			return this.StreamFixedArray(array, 0, array.Length);
		}

		public EndianStream StreamFixedArray(short[] array, int startIndex, int length)
		{
			Contract.Requires(array != null);
			Contract.Requires(startIndex >= 0);
			Contract.Requires(length >= 0);

				 if (this.IsReading)
					 this.Reader.ReadFixedArray(array, startIndex, length);
			else if (this.IsWriting)
				this.Writer.WriteFixedArray(array, startIndex, length);

			return this;
		}
		public EndianStream StreamFixedArray(short[] array)
		{
			Contract.Requires(array != null);

			return this.StreamFixedArray(array, 0, array.Length);
		}

		public EndianStream StreamFixedArray(uint[] array, int startIndex, int length)
		{
			Contract.Requires(array != null);
			Contract.Requires(startIndex >= 0);
			Contract.Requires(length >= 0);

				 if (this.IsReading)
					 this.Reader.ReadFixedArray(array, startIndex, length);
			else if (this.IsWriting)
				this.Writer.WriteFixedArray(array, startIndex, length);

			return this;
		}
		public EndianStream StreamFixedArray(uint[] array)
		{
			Contract.Requires(array != null);

			return this.StreamFixedArray(array, 0, array.Length);
		}

		public EndianStream StreamFixedArray(int[] array, int startIndex, int length)
		{
			Contract.Requires(array != null);
			Contract.Requires(startIndex >= 0);
			Contract.Requires(length >= 0);

				 if (this.IsReading)
					 this.Reader.ReadFixedArray(array, startIndex, length);
			else if (this.IsWriting)
				this.Writer.WriteFixedArray(array, startIndex, length);

			return this;
		}
		public EndianStream StreamFixedArray(int[] array)
		{
			Contract.Requires(array != null);

			return this.StreamFixedArray(array, 0, array.Length);
		}

		public EndianStream StreamFixedArray(ulong[] array, int startIndex, int length)
		{
			Contract.Requires(array != null);
			Contract.Requires(startIndex >= 0);
			Contract.Requires(length >= 0);

				 if (this.IsReading)
					 this.Reader.ReadFixedArray(array, startIndex, length);
			else if (this.IsWriting)
				this.Writer.WriteFixedArray(array, startIndex, length);

			return this;
		}
		public EndianStream StreamFixedArray(ulong[] array)
		{
			Contract.Requires(array != null);

			return this.StreamFixedArray(array, 0, array.Length);
		}

		public EndianStream StreamFixedArray(long[] array, int startIndex, int length)
		{
			Contract.Requires(array != null);
			Contract.Requires(startIndex >= 0);
			Contract.Requires(length >= 0);

				 if (this.IsReading)
					 this.Reader.ReadFixedArray(array, startIndex, length);
			else if (this.IsWriting)
				this.Writer.WriteFixedArray(array, startIndex, length);

			return this;
		}
		public EndianStream StreamFixedArray(long[] array)
		{
			Contract.Requires(array != null);

			return this.StreamFixedArray(array, 0, array.Length);
		}

		public EndianStream StreamFixedArray(float[] array, int startIndex, int length)
		{
			Contract.Requires(array != null);
			Contract.Requires(startIndex >= 0);
			Contract.Requires(length >= 0);

				 if (this.IsReading)
					 this.Reader.ReadFixedArray(array, startIndex, length);
			else if (this.IsWriting)
				this.Writer.WriteFixedArray(array, startIndex, length);

			return this;
		}
		public EndianStream StreamFixedArray(float[] array)
		{
			Contract.Requires(array != null);

			return this.StreamFixedArray(array, 0, array.Length);
		}

		public EndianStream StreamFixedArray(double[] array, int startIndex, int length)
		{
			Contract.Requires(array != null);
			Contract.Requires(startIndex >= 0);
			Contract.Requires(length >= 0);

				 if (this.IsReading)
					 this.Reader.ReadFixedArray(array, startIndex, length);
			else if (this.IsWriting)
				this.Writer.WriteFixedArray(array, startIndex, length);

			return this;
		}
		public EndianStream StreamFixedArray(double[] array)
		{
			Contract.Requires(array != null);

			return this.StreamFixedArray(array, 0, array.Length);
		}

		#endregion
	};
}