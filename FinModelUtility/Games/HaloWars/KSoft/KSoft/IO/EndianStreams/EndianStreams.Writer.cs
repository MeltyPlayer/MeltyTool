using System;
using System.IO;
using System.Text;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.IO
{
	/// <summary>A binary stream with the ability to write data in different endian formats</summary>
	/// <remarks>For stream character encoding, when no explicit encoding is provided, <see cref="System.Text.UTF8Encoding"/> is assumed</remarks>
	public sealed partial class EndianWriter : BinaryWriter, IKSoftBinaryStream, IKSoftEndianStream
	{
		public new static readonly EndianWriter Null = new EndianWriter();

		// .NET 4.5: BinaryWriter has 'bool leaveOpen' ctor
		#region Ctor
		/// <summary>Null stream constructor</summary>
		EndianWriter() : base()
		{
			this.BaseStreamOwner = true;
			this.BaseAddress = Values.PtrHandle.Null32;

			this.ByteOrder = Shell.Platform.Environment.ProcessorType.ByteOrder;
			this.Owner = null;
			this.mStringEncoding = new UTF8Encoding(false, true);

			// Satisfy ObjectInvariant
			this.StreamName = "(null)";

			this.mRequiresByteSwap = !this.ByteOrder.IsSameAsRuntime();
		}

		/// <summary>Create a new binary writer which respects the endian format of the underlying stream's bytes</summary>
		/// <param name="output">Base stream to use as output</param>
		/// <param name="encoding">Character encoding to use. If null, <see cref="System.Text.UTF8Encoding"/> is assumed</param>
		/// <param name="byteOrder">Endian format for how we interpret the stream's bytes</param>
		/// <param name="streamOwner">Owner object of this stream, or null</param>
		/// <param name="name">Special name to associate with this stream</param>
		public EndianWriter(Stream output, Encoding encoding,
			Shell.EndianFormat byteOrder, object streamOwner = null, string name = null) : base(output, encoding)
		{
			Contract.Requires<ArgumentNullException>(output != null);
			Contract.Requires<ArgumentNullException>(encoding != null);

			this.BaseStreamOwner = true;
			this.BaseAddress = Values.PtrHandle.Null32;

			this.ByteOrder = byteOrder;
			this.Owner = streamOwner;
			this.mStringEncoding = encoding;

			this.StreamName = name ?? "(unnamed)";

			// If the stream is a different endian than the runtime, data will
			// be byte swapped of course
			//this.mRequiresByteSwap = Shell.Platform.Environment.ProcessorType.ByteOrder != byteOrder;
			this.mRequiresByteSwap = !byteOrder.IsSameAsRuntime();
		}

		/// <summary>Create a new binary writer which respects the endian format of the underlying stream's bytes</summary>
		/// <param name="output">Base stream to use as output</param>
		/// <param name="byteOrder">Endian format for how we interpret the stream's bytes</param>
		/// <param name="streamOwner">Owner object of this stream, or null</param>
		/// <param name="name">Special name to associate with this stream</param>
		/// <remarks>Defaults to <see cref="System.Text.UTF8Encoding"/> for the string encoding</remarks>
		public EndianWriter(Stream output, Shell.EndianFormat byteOrder,
			object streamOwner = null, string name = null) : this(output, Encoding.UTF8, byteOrder, streamOwner, name)
		{
		}
		/// <summary>Create a new binary writer which uses the environment's endian format</summary>
		/// <param name="output">Base stream to use as output</param>
		/// <remarks>
		/// Default endian format is set from <see cref="Shell.Platform.Environment"/>.
		/// <see cref="Owner"/> is set to <c>null</c>
		/// </remarks>
		public EndianWriter(Stream output) : this(output, Encoding.UTF8,
			Shell.Platform.Environment.ProcessorType.ByteOrder)
		{
			Contract.Requires<ArgumentNullException>(output != null);
		}
		#endregion

		#region Pad
		public void Pad(int byteCount)
		{
			Contract.Requires(byteCount > 0);

			// Write 32 bit blocks, then any odd bytes
			for (; byteCount >= 4; byteCount -= 4)
													base.Write(uint.MinValue);
			for (; byteCount > 0; --byteCount)
													base.Write(byte.MinValue);
		}
		public void Pad8()	{ base.Write(byte.MinValue); }
		public void Pad16()	{ base.Write(ushort.MinValue); }
		public void Pad24()	{ base.Write(ushort.MinValue); base.Write(byte.MinValue); }
		public void Pad32()	{ base.Write(uint.MinValue); }
		public void Pad64()	{ base.Write(ulong.MinValue); }
		public void Pad128(){ base.Write(ulong.MinValue); base.Write(ulong.MinValue); }
		#endregion

		/// <summary>Writes an unsigned byte array</summary>
		/// <param name="value"></param>
		/// <param name="count"></param>
		/// <seealso cref="BinaryWriter.Write(byte[], int, int)"/>
		public void Write(byte[] value, int count)
		{
			Contract.Requires(value != null);
			Contract.Requires(count >= 0 && count <= value.Length);

			base.Write(value, 0, count);
		}

		/// <summary>Writes a character array</summary>
		/// <param name="value"></param>
		/// <param name="count"></param>
		/// <seealso cref="BinaryWriter.Write(char[], int, int)"/>
		public void Write(char[] value, int count)
		{
			Contract.Requires(value != null);
			Contract.Requires(count >= 0 && count <= value.Length);

			base.Write(value, 0, count);
		}

		#region Write group tag
		/// <summary>Writes a tag id (four character code)</summary>
		/// <param name="tag">Big-endian ordered tag id</param>
		public void WriteTag32(char[] tag)
		{
			Contract.Requires(tag != null);
			Contract.Requires(tag.Length == 4);

			// Explicitly check for Little endian since this is
			// a character array and not a primitive integer
			if (this.ByteOrder == Shell.EndianFormat.Little)
			{
				base.Write((byte)tag[3]);
				base.Write((byte)tag[2]);
				base.Write((byte)tag[1]);
				base.Write((byte)tag[0]);
			}
			else
			{
				base.Write((byte)tag[0]);
				base.Write((byte)tag[1]);
				base.Write((byte)tag[2]);
				base.Write((byte)tag[3]);
			}
		}

		/// <summary>Writes a tag id (four character code)</summary>
		/// <param name="tag"></param>
		public void WriteTag32(uint tag)
		{
			if (this.mRequiresByteSwap) Bitwise.ByteSwap.Swap(ref tag);
			base.Write(tag);
		}

		/// <summary>Writes a tag id (eight character code)</summary>
		/// <param name="tag"></param>
		public void WriteTag64(ulong tag)
		{
			if (this.mRequiresByteSwap) Bitwise.ByteSwap.Swap(ref tag);
			base.Write(tag);
		}
		#endregion

		// #TODO: generate with T4
		#region Write numerics
		/// <summary>Writes a signed 24-bit integer</summary>
		/// <param name="value"></param>
		public void WriteInt24(int value)
		{
			if (this.mRequiresByteSwap) Bitwise.ByteSwap.SwapInt24(ref value);
			base.Write((byte) value);
			base.Write((byte)(value >>  8));
			base.Write((byte)(value >> 16));
		}

		/// <summary>Writes an unsigned 24-bit integer</summary>
		/// <param name="value"></param>
		public void WriteUInt24(uint value)
		{
			if (this.mRequiresByteSwap) Bitwise.ByteSwap.SwapUInt24(ref value);
			base.Write((byte) value);
			base.Write((byte)(value >>  8));
			base.Write((byte)(value >> 16));
		}

		/// <summary>Writes an unsigned 40-bit integer</summary>
		/// <param name="value"></param>
		public void WriteUInt40(ulong value)
		{
			if (this.mRequiresByteSwap) Bitwise.ByteSwap.SwapUInt40(ref value);
			base.Write((byte) value);
			base.Write((byte)(value >>  8));
			base.Write((byte)(value >> 16));
			base.Write((byte)(value >> 24));
			base.Write((byte)(value >> 32));
		}
		#endregion

		#region Write string
		/// <summary>Writes a string based on a <see cref="Memory.Strings.StringStorage"/> definition</summary>
		/// <param name="value">String value to write. Null defaults to an empty string</param>
		/// <param name="storage">Definition for how we're streaming the string</param>
		public void Write(string value, Memory.Strings.StringStorage storage)
		{
			var sse = Text.StringStorageEncoding.TryAndGetStaticEncoding(storage);
			byte[] bytes = sse.GetBytes(value ?? string.Empty);
			base.Write(bytes);
		}

		/// <summary>Writes string using a <see cref="Text.StringStorageEncoding"/></summary>
		/// <param name="value">String value to write. Null defaults to an empty string</param>
		/// <param name="encoding">Encoding to use for character streaming</param>
		public void Write(string value, Text.StringStorageEncoding encoding)
		{
			Contract.Requires(encoding != null);

			byte[] bytes = encoding.GetBytes(value ?? string.Empty);
			base.Write(bytes);
		}
		#endregion

		#region Write Pointer
		/// <summary>Write a pointer value to the stream with no preprocessing to the value</summary>
		/// <param name="value">>Handle to stream</param>
		public void WriteRawPointer(Values.PtrHandle value)
		{
			if (!value.IsNull)
			{
				if (!value.Is64bit)
					this.Write(value.u32);
				else
					this.Write(value.u64);
			}
			else
			{
				if (!value.Is64bit)
					this.Write(uint.MinValue);
				else
					this.Write(ulong.MinValue);
			}
		}

		/// <summary>Write a pointer value to the stream</summary>
		/// <param name="value">Handle to stream</param>
		/// <remarks>
		/// <see cref="BaseAddress"/> is added to the value of <paramref name="value"/>
		/// before the final stream happens.
		///
		/// Be sure to set <see cref="BaseAddress"/> to the proper value so you write
		/// the correct virtual-address. Otherwise, set <see cref="BaseAddress"/>
		/// to zero to write the pure pointer value.
		/// </remarks>
		public void WritePointer(Values.PtrHandle value)
		{
			if (!value.IsNull)
			{
				if (!value.Is64bit)
					this.Write(value.u32 + this.BaseAddress.u32);
				else
					this.Write(value.u64 + this.BaseAddress.u64);
			}
			else
			{
				if (!value.Is64bit)
					this.Write(uint.MinValue);
				else
					this.Write(ulong.MinValue);
			}
		}
		#endregion

		/// <summary>Write out an int 7 bits at a time. The high bit of the byte, when on, tells reader to continue reading more bytes.</summary>
		/// <param name="value"></param>
		public new void Write7BitEncodedInt(int value) { base.Write7BitEncodedInt(value); }

		public void Write(DateTime value, bool isUnixTime = false)
		{
			long binary = isUnixTime
				? Util.ConvertDateTimeToUnixTime(value)
				: value.ToBinary();

			this.Write(binary);
		}

		public void Write<TEnum>(TEnum value, IEnumEndianStreamer<TEnum> implementation)
			where TEnum : struct
		{
			Contract.Requires(implementation != null);

			implementation.Write(this, value);
		}

		public bool[] WriteFixedArray(bool[] array, int startIndex, int length)
		{
			Contract.Requires(array != null);
			Contract.Requires(startIndex >= 0);
			Contract.Requires(length >= 0);
			Contract.Ensures(Contract.Result<bool[]>() != null);

			for (int x = startIndex, end = startIndex+length; x < end; x++)
				this.Write(array[x]);

			return array;
		}
		public bool[] WriteFixedArray(bool[] array)
		{
			Contract.Requires(array != null);
			Contract.Ensures(Contract.Result<bool[]>() != null);

			return this.WriteFixedArray(array, 0, array.Length);
		}
	};
}