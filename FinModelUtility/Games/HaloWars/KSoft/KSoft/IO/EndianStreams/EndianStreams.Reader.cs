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
	/// <summary>A binary stream with the ability to read data in different endian formats</summary>
	/// <remarks>For stream character encoding, when no explicit encoding is provided, <see cref="System.Text.UTF8Encoding"/> is assumed</remarks>
	public sealed partial class EndianReader : BinaryReader, IKSoftBinaryStream, IKSoftEndianStream
	{
		#region BinaryReader Accessors
		static readonly Reflection.Util.ReferenceTypeMemberSetterDelegate<BinaryReader, Stream> kSetBaseStream =
			Reflection.Util.GenerateReferenceTypeMemberSetter<BinaryReader, Stream>("m_stream");
		#endregion

		// #REVIEW: .NET 4.5: BinaryReader has 'bool leaveOpen' ctor
		#region Ctor
		/// <summary>Create a new binary reader which respects the endian format of the underlying stream's bytes</summary>
		/// <param name="input">Base stream to use as input</param>
		/// <param name="encoding">Character encoding to use. If null, <see cref="System.Text.UTF8Encoding"/> is assumed</param>
		/// <param name="byteOrder">Endian format for how we interpret the stream's bytes</param>
		/// <param name="streamOwner">Owner object of this stream, or null</param>
		/// <param name="name">Special name to associate with this stream</param>
		public EndianReader(Stream input, Encoding encoding,
			Shell.EndianFormat byteOrder, object streamOwner = null, string name = null) : base(input, encoding)
		{
			Contract.Requires<ArgumentNullException>(input != null);
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

		/// <summary>Create a new binary reader which respects the endian format of the underlying stream's bytes</summary>
		/// <param name="input">Base stream to use as input</param>
		/// <param name="byteOrder">Endian format for how we interpret the stream's bytes</param>
		/// <param name="streamOwner">Owner object of this stream, or null</param>
		/// <param name="name">Special name to associate with this stream</param>
		/// <remarks>Defaults to <see cref="System.Text.UTF8Encoding"/> for the string encoding</remarks>
		public EndianReader(Stream input, Shell.EndianFormat byteOrder,
			object streamOwner = null, string name = null) : this(input, new UTF8Encoding(), byteOrder, streamOwner, name)
		{
		}
		/// <summary>Create a new binary reader which uses the environment's endian format</summary>
		/// <param name="input">Base stream to use as input</param>
		/// <remarks>
		/// Default endian format is set from <see cref="Shell.Platform.Environment"/>.
		/// <see cref="Owner"/> is set to <c>null</c>
		/// </remarks>
		public EndianReader(Stream input) : this(input, new UTF8Encoding(),
			Shell.Platform.Environment.ProcessorType.ByteOrder)
		{
			Contract.Requires<ArgumentNullException>(input != null);
		}
		#endregion

		public IDisposable ReadSignatureWithByteSwapSupport(uint expectedSignature, out uint actualSignature)
		{
			var signature = this.ReadUInt32();
			if (signature != expectedSignature)
			{
				var signatureInverted = Bitwise.ByteSwap.SwapUInt32(signature);
				if (signatureInverted == expectedSignature)
				{
					actualSignature = signatureInverted;
					return this.BeginEndianSwitch();
				}

				throw new SignatureMismatchException(this.BaseStream, expectedSignature, signature);
			}

			actualSignature = signature;
			return Util.NullDisposable;
		}

		public IDisposable ReadSignatureWithByteSwapSupport(uint expectedSignature)
		{
			return this.ReadSignatureWithByteSwapSupport(expectedSignature, out uint actualSignature);
		}

		#region Pad
		public void Pad(int byteCount)
		{
			Contract.Requires(byteCount > 0);

			this.BaseStream.Seek(sizeof(byte) * byteCount, SeekOrigin.Current);
		}

		public void Pad8()	{
			this.Pad(sizeof(byte)); }
		public void Pad16()	{
			this.Pad(sizeof(ushort)); }
		public void Pad24()	{
			this.Pad(sizeof(ushort)+sizeof(byte)); }
		public void Pad32()	{
			this.Pad(sizeof(uint)); }
		public void Pad64()	{
			this.Pad(sizeof(ulong)); }
		public void Pad128(){
			this.Pad(sizeof(ulong)*2); }
		#endregion

		/// <summary>Reads an unsigned byte array</summary>
		/// <param name="value"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		/// <seealso cref="BinaryReader.Read(byte[], int, int)"/>
		public byte[] Read(byte[] buffer, int count)
		{
			Contract.Requires(buffer != null);
			Contract.Requires(count >= 0 && count <= buffer.Length);
			Contract.Ensures(Contract.Result<byte[]>() != null);

			base.Read(buffer, 0, count);

			return buffer;
		}
		/// <summary>Reads an unsigned byte array</summary>
		/// <param name="value"></param>
		/// <returns></returns>
		/// <seealso cref="BinaryReader.Read(byte[], int, int)"/>
		public byte[] Read(byte[] buffer)
		{
			Contract.Requires(buffer != null);
			Contract.Ensures(Contract.Result<byte[]>() != null);

			base.Read(buffer, 0, buffer.Length);

			return buffer;
		}

		/// <summary>Reads a character array</summary>
		/// <param name="value"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		/// <seealso cref="BinaryReader.Read(char[], int, int)"/>
		public char[] Read(char[] buffer, int count)
		{
			Contract.Requires(buffer != null);
			Contract.Requires(count >= 0 && count <= buffer.Length);
			Contract.Ensures(Contract.Result<char[]>() != null);

			base.Read(buffer, 0, count);

			return buffer;
		}
		/// <summary>Reads a character array</summary>
		/// <param name="value"></param>
		/// <returns></returns>
		/// <seealso cref="BinaryReader.Read(char[], int, int)"/>
		public char[] Read(char[] buffer)
		{
			Contract.Requires(buffer != null);
			Contract.Ensures(Contract.Result<char[]>() != null);

			base.Read(buffer, 0, buffer.Length);

			return buffer;
		}

		#region Read group tag
		/// <summary>Reads a tag id (four character code)</summary>
		/// <param name="tag">Array to populate</param>
		/// <returns>Big-endian ordered tag id</returns>
		public char[] ReadTag32(char[] tag)
		{
			Contract.Requires(tag != null);
			Contract.Requires(tag.Length >= 4);
			Contract.Ensures(Contract.Result<char[]>() != null);
			Contract.Ensures(Contract.Result<char[]>().Length >= 4);

			tag[0] = (char)this.ReadByte();
			tag[1] = (char)this.ReadByte();
			tag[2] = (char)this.ReadByte();
			tag[3] = (char)this.ReadByte();

			// Explicitly check for Little endian since this is
			// a character array and not a primitive integer
			if (this.ByteOrder == Shell.EndianFormat.Little)
			{
				Array.Reverse(tag, 0, 4);
				return tag;
			}

			return tag;
		}
		/// <summary>Reads a tag id (four character code)</summary>
		/// <returns>Big-endian ordered tag id</returns>
		public char[] ReadTag32()
		{
			Contract.Ensures(Contract.Result<char[]>() != null);
			Contract.Ensures(Contract.Result<char[]>().Length == 4);

			return this.ReadTag32(new char[4]);
		}

		/// <summary>Reads a tag id (eight character code)</summary>
		/// <param name="tag">Array to populate</param>
		/// <returns>Big-endian ordered tag id</returns>
		public char[] ReadTag64(char[] tag)
		{
			Contract.Requires(tag != null);
			Contract.Requires(tag.Length >= 8);
			Contract.Ensures(Contract.Result<char[]>() != null);
			Contract.Ensures(Contract.Result<char[]>().Length >= 8);

			tag[0] = (char)this.ReadByte();
			tag[1] = (char)this.ReadByte();
			tag[2] = (char)this.ReadByte();
			tag[3] = (char)this.ReadByte();
			tag[4+0] = (char)this.ReadByte();
			tag[4+1] = (char)this.ReadByte();
			tag[4+2] = (char)this.ReadByte();
			tag[4+3] = (char)this.ReadByte();

			// Explicitly check for Little endian since this is
			// a character array and not a primitive integer
			if (this.ByteOrder == Shell.EndianFormat.Little)
			{
				Array.Reverse(tag, 0, 4);
				Array.Reverse(tag, 4, 4);
			}

			return tag;
		}
		/// <summary>Reads a tag id (eight character code)</summary>
		/// <returns>Big-endian ordered tag id</returns>
		public char[] ReadTag64()
		{
			Contract.Ensures(Contract.Result<char[]>() != null);
			Contract.Ensures(Contract.Result<char[]>().Length == 8);

			return this.ReadTag64(new char[8]);
		}

		/// <summary>Reads a tag id (four character code)</summary>
		/// <returns></returns>
		/// <seealso cref="BinaryReader.ReadUInt32()"/>
		public uint ReadTagUInt32()
		{
			uint value = base.ReadUInt32();
			if (this.mRequiresByteSwap)	Bitwise.ByteSwap.Swap(ref value);

			return value;
		}

		/// <summary>Reads a tag id (eight character code)</summary>
		/// <returns></returns>
		/// <seealso cref="BinaryReader.ReadUInt64()"/>
		public ulong ReadTagUInt64()
		{
			ulong value = base.ReadUInt64();
			if (this.mRequiresByteSwap)	Bitwise.ByteSwap.Swap(ref value);

			return value;
		}
		#endregion

		// #TODO: generate with T4
		#region Read numerics
		/// <summary>Reads a signed 24-bit integer</summary>
		/// <returns></returns>
		/// <remarks>Sign extends the value read from the stream</remarks>
		public int ReadInt24()
		{
			int value = this.ReadByte() | (this.ReadByte() << 8) | (this.ReadByte() << 16);
			if(this.mRequiresByteSwap) Bitwise.ByteSwap.SwapInt24(ref value);

			value = Bits.SignExtend(value, 24);
			return value;
		}

		/// <summary>Reads an unsigned 24-bit integer</summary>
		/// <returns></returns>
		public uint ReadUInt24()
		{
			uint value = (uint)(this.ReadByte() | (this.ReadByte() << 8) | (this.ReadByte() << 16));
			if(this.mRequiresByteSwap) Bitwise.ByteSwap.SwapUInt24(ref value);

			return value;
		}

		/// <summary>Reads an unsigned 24-bit integer</summary>
		/// <returns></returns>
		public ulong ReadUInt40()
		{
			var value =
				((ulong)this.ReadByte()) |
				((ulong)this.ReadByte() << 8) |
				((ulong)this.ReadByte() << 16) |
				((ulong)this.ReadByte() << 24) |
				((ulong)this.ReadByte() << 32)
				;
			if(this.mRequiresByteSwap) Bitwise.ByteSwap.SwapUInt40(ref value);

			return value;
		}
		#endregion

		#region Read string
		// Verify that we have enough information to correctly stream a string
		static void ValidateStringStorageForStreaming(Memory.Strings.StringStorage s, int length)
		{
			// There are going to be issues if we try to read back a willy nilly char array string
			if (s.Type == Memory.Strings.StringStorageType.CharArray && !s.IsFixedLength && length <= 0)
			{
				throw new InvalidDataException(string.Format(Util.InvariantCultureInfo,
					"Provided string storage and length is invalid for Endian streaming: {0}, {1}",
					s.ToString(), length.ToString(Util.InvariantCultureInfo)));
			}
		}
		/// <summary>Read a string using a <see cref="Memory.Strings.StringStorage"/> definition and a provided character length</summary>
		/// <param name="storage">Definition for the string's characteristics</param>
		/// <param name="length">Length, in characters, of the string.</param>
		/// <returns></returns>
		/// <remarks>
		/// Length can be non-positive if <paramref name="storage"/> defines or
		/// doesn't require an explicit character length. If you do provide the
		/// length, this operation will perform faster in some cases.
		/// </remarks>
		public string ReadString(Memory.Strings.StringStorage storage, int length)
		{
			ValidateStringStorageForStreaming(storage, length);

			var sse = Text.StringStorageEncoding.TryAndGetStaticEncoding(storage);

			return sse.ReadString(this, length);
		}
		/// <summary>
		/// Read a string using a <see cref="Memory.Strings.StringStorage"/> definition.
		/// String length defaults to <see cref="Memory.Strings.StringStorage.FixedLegnth"/>
		/// </summary>
		/// <param name="storage">Definition for the string's characteristics</param>
		/// <returns></returns>
		public string ReadString(Memory.Strings.StringStorage storage)
		{
			return this.ReadString(storage, storage.FixedLength);
		}

		/// <summary>Read a string using a <see cref="Memory.Strings.StringStorage"/> encoding and a provided character length</summary>
		/// <param name="encoding">The encoding to use for character streaming</param>
		/// <param name="length">Length, in characters, of the string.</param>
		/// <returns></returns>
		/// <remarks>
		/// Length can be non-positive if <paramref name="encoding"/>'s storage defines
		/// or doesn't require an explicit character length. If you do provide the
		/// length, this operation will perform faster in some cases.
		/// </remarks>
		public string ReadString(Text.StringStorageEncoding encoding, int length)
		{
			Contract.Requires(encoding != null);
			ValidateStringStorageForStreaming(encoding.Storage, length);

			return encoding.ReadString(this, length);
		}
		/// <summary>
		/// Read a string using a <see cref="Memory.Strings.StringStorage"/> encoding.
		/// String length defaults to <see cref="Memory.Strings.StringStorage.FixedLegnth"/>
		/// </summary>
		/// <param name="encoding">The encoding to use for character streaming</param>
		/// <returns></returns>
		public string ReadString(Text.StringStorageEncoding encoding)
		{
			Contract.Requires(encoding != null);

			return this.ReadString(encoding, encoding.Storage.FixedLength);
		}
		#endregion

		#region Read Pointer
		/// <summary>Read a pointer value from the stream with no postprocessing to the result</summary>
		/// <param name="addressSize">Size of the pointer we're to read</param>
		/// <returns></returns>
		public Values.PtrHandle ReadRawPointer(Shell.ProcessorSize addressSize)
		{
			var ptr = new Values.PtrHandle(addressSize);

			if (!ptr.Is64bit)	ptr.u32 = this.ReadUInt32();
			else				ptr.u64 = this.ReadUInt64();

			return ptr;
		}
		/// <summary>Read a pointer value from the stream with no postprocessing to the result</summary>
		/// <param name="ptrHandle">Handle to stream the value into</param>
		public void ReadRawPointer(ref Values.PtrHandle ptrHandle)
		{
			if (!ptrHandle.Is64bit)	ptrHandle.u32 = this.ReadUInt32();
			else					ptrHandle.u64 = this.ReadUInt64();
		}

		/// <summary>Read a pointer value from the stream</summary>
		/// <param name="addressSize">Size of the pointer we're to read</param>
		/// <returns>
		/// A handle that encapsulates the pointer value and its size. After the value
		/// is read from the stream, <see cref="BaseAddress"/> is subtracted.
		/// </returns>
		/// <remarks>
		/// Be sure to set <see cref="BaseAddress"/> to the proper value so you get
		/// a correct relative-virtual-address. Otherwise, set <see cref="BaseAddress"/>
		/// to zero to get the pure pointer value.
		/// </remarks>
		public Values.PtrHandle ReadPointer(Shell.ProcessorSize addressSize)
		{
			var ptr = new Values.PtrHandle(addressSize);

			if (!ptr.Is64bit)
			{
				ptr.u32 = this.ReadUInt32();

				if (ptr.u32 != 0)
					ptr.u32 -= this.BaseAddress.u32;
			}
			else
			{
				ptr.u64 = this.ReadUInt64();

				if (ptr.u64 != 0)
					ptr.u64 -= this.BaseAddress.u64;
			}

			return ptr;
		}
		/// <summary>Read a pointer value from the stream</summary>
		/// <param name="ptrHandle">Handle to stream the value into</param>
		/// <remarks>
		/// After the value is read from the stream, <see cref="BaseAddress"/> is subtracted.
		///
		/// Be sure to set <see cref="BaseAddress"/> to the proper value so you get
		/// a correct relative-virtual-address. Otherwise, set <see cref="BaseAddress"/>
		/// to zero to get the pure pointer value.
		/// </remarks>
		public void ReadPointer(ref Values.PtrHandle ptrHandle)
		{
			if (!ptrHandle.Is64bit)
			{
				ptrHandle.u32 = this.ReadUInt32();

				if (ptrHandle.u32 != 0)
					ptrHandle.u32 -= this.BaseAddress.u32;
			}
			else
			{
				ptrHandle.u64 = this.ReadUInt64();

				if (ptrHandle.u64 != 0)
					ptrHandle.u64 -= this.BaseAddress.u64;
			}
		}

		/// <summary>Read a pointer value from the stream (size inherited from <see cref="BaseAddress"/>)</summary>
		/// <returns>
		/// A handle that encapsulates the pointer value and its size. After the value
		/// is read from the stream, <see cref="BaseAddress"/> is subtracted.
		/// </returns>
		/// <remarks>
		/// Be sure to set <see cref="BaseAddress"/> to the proper value so you get
		/// a correct relative-virtual-address. Otherwise, set <see cref="BaseAddress"/>
		/// to zero to get the pure pointer value.
		/// </remarks>
		public Values.PtrHandle ReadPointerViaBaseAddress()
		{
			return this.ReadPointer(this.BaseAddress.Size);
		}
		/// <summary>Read a pointer value from the stream (size inherited from <see cref="BaseAddress"/>)</summary>
		/// <param name="ptrHandle">Handle to initialize and stream the value into</param>
		/// <remarks>
		/// After the value is read from the stream, <see cref="BaseAddress"/> is subtracted.
		///
		/// Be sure to set <see cref="BaseAddress"/> to the proper value so you get
		/// a correct relative-virtual-address. Otherwise, set <see cref="BaseAddress"/>
		/// to zero to get the pure pointer value.
		/// </remarks>
		public void ReadPointerViaBaseAddress(out Values.PtrHandle ptrHandle)
		{
			ptrHandle = new Values.PtrHandle(this.BaseAddress.Size);

			this.ReadPointer(ref ptrHandle);
		}
		#endregion

		/// <summary>Read out an Int32 7 bits at a time. The high bit of the byte when on means to continue reading more bytes.</summary>
		/// <returns></returns>
		public new int Read7BitEncodedInt() { return base.Read7BitEncodedInt(); }

		public DateTime ReadDateTime(bool isUnixTime = false)
		{
			long binary = this.ReadInt64();

			return isUnixTime
				? Util.ConvertDateTimeFromUnixTime(binary)
				: DateTime.FromBinary(binary);
		}

		public TEnum Read<TEnum>(IEnumEndianStreamer<TEnum> implementation)
			where TEnum : struct
		{
			Contract.Requires(implementation != null);

			return implementation.Read(this);
		}

		public bool[] ReadFixedArray(bool[] array, int startIndex, int length)
		{
			Contract.Requires(array != null);
			Contract.Requires(startIndex >= 0);
			Contract.Requires(length >= 0);
			Contract.Ensures(Contract.Result<bool[]>() != null);

			for (int x = startIndex, end = startIndex+length; x < end; x++)
				array[x] = this.ReadBoolean();

			return array;
		}
		public bool[] ReadFixedArray(bool[] array)
		{
			Contract.Requires(array != null);
			Contract.Ensures(Contract.Result<bool[]>() != null);

			return this.ReadFixedArray(array, 0, array.Length);
		}
	};
}
