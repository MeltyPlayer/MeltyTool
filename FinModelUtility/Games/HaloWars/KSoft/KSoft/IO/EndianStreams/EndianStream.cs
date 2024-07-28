using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.IO
{
	public sealed partial class EndianStream : IKSoftBinaryStream, IKSoftStreamModeable, IKSoftStreamWithVirtualBuffer
	{
		public Stream BaseStream { get; private set; }
		public EndianReader Reader { get; private set; }
		public EndianWriter Writer { get; private set; }

		#region IKSoftStream
		/// <summary>Owner of this stream</summary>
		public object Owner
		{
			get { return this.Reader != null ? this.Reader.Owner : this.Writer.Owner; }
			set {
				if (this.Reader != null)
					this.Reader.Owner = value;
				if (this.Writer != null)
					this.Writer.Owner = value;
			}
		}

		public object UserData
		{
			get { return this.Reader != null ? this.Reader.UserData : this.Writer.UserData; }
			set {
				if (this.Reader != null)
					this.Reader.UserData = value;
				if (this.Writer != null)
					this.Writer.UserData = value;
			}
		}

		/// <summary>Name of the underlying stream this object is interfacing with</summary>
		/// <remarks>So if this endian stream is interfacing with a file, this will be it's name</remarks>
		[SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
		public string StreamName { get {
				 if (this.IsReading) return this.Reader.StreamName;
			else if (this.IsWriting) return this.Writer.StreamName;
			else throw new Debug.UnreachableException(this.StreamMode.ToString());
		} }
		#endregion

		#region IKSoftBinaryStream
		/// <summary>Base address used for simulating pointers in the stream</summary>
		/// <remarks>Default value is <see cref="Data.PtrHandle.Null32"/></remarks>
		public Values.PtrHandle BaseAddress
		{
			get { return this.Reader != null ? this.Reader.BaseAddress : this.Writer.BaseAddress; }
			set {
				if (this.Reader != null)
					this.Reader.BaseAddress = value;
				if (this.Writer != null)
					this.Writer.BaseAddress = value;
			}
		}

		#region Seek
		/// <summary>Moves the stream cursor to <paramref name="offset"/> relative to the beginning of the stream</summary>
		/// <param name="offset">Offset to seek to</param>
		public void Seek32(uint offset)						{
			this.Seek(offset, SeekOrigin.Begin); }
		/// <summary>Moves the stream cursor to <paramref name="offset"/> relative to <paramref name="origin"/></summary>
		/// <param name="offset">Offset to seek to</param>
		/// <param name="origin">Origin to base seek operation</param>
		public void Seek32(uint offset, SeekOrigin origin)	{
			this.Seek(offset, origin); }
		/// <summary>Moves the stream cursor to <paramref name="offset"/> relative to the beginning of the stream</summary>
		/// <param name="offset">Offset to seek to</param>
		public void Seek32(int offset)						{
			this.Seek(offset, SeekOrigin.Begin); }
		/// <summary>Moves the stream cursor to <paramref name="offset"/> relative to <paramref name="origin"/></summary>
		/// <param name="offset">Offset to seek to</param>
		/// <param name="origin">Origin to base seek operation</param>
		public void Seek32(int offset, SeekOrigin origin)	{
			this.Seek(offset, origin); }

		/// <summary>Moves the stream cursor to <paramref name="offset"/> relative to the beginning of the stream</summary>
		/// <param name="offset">Offset to seek to</param>
		public void Seek(long offset)						{
			this.Seek(offset, SeekOrigin.Begin); }
		/// <summary>Moves the stream cursor to <paramref name="offset"/> relative to <paramref name="origin"/></summary>
		/// <param name="offset">Offset to seek to</param>
		/// <param name="origin">Origin to base seek operation</param>
		public void Seek(long offset, SeekOrigin origin)	{
			this.BaseStream.Seek(offset, origin); }
		#endregion

		/// <summary>Align the stream's position by a certain page boundry</summary>
		/// <param name="alignmentBit">log2 size of the alignment (ie, 1&lt;&lt;bit)</param>
		/// <returns>True if any alignment had to be performed, false if otherwise</returns>
		public bool AlignToBoundry(int alignmentBit)
		{
				 if (this.IsReading) return this.Reader.AlignToBoundry(alignmentBit);
			else if (this.IsWriting)	return this.Writer.AlignToBoundry(alignmentBit);
			else throw new Debug.UnreachableException(this.StreamMode.ToString());
		}

		#region VirtualAddressTranslation
		/// <summary>Initialize the VAT with a specific handle size and initial table capacity</summary>
		/// <param name="vaSize">Handle size</param>
		/// <param name="translationCapacity">The initial table capacity</param>
		public void VirtualAddressTranslationInitialize(Shell.ProcessorSize vaSize, int translationCapacity = 0)
		{
			if (this.Reader != null)
				this.Reader.VirtualAddressTranslationInitialize(vaSize, translationCapacity);
			if (this.Writer != null)
				this.Writer.VirtualAddressTranslationInitialize(vaSize, translationCapacity);
		}
		/// <summary>Push a PA into to the VAT table, setting the current PA in the process</summary>
		/// <param name="physicalAddress">PA to push and to use as the VAT's current address</param>
		public void VirtualAddressTranslationPush(Values.PtrHandle physicalAddress)
		{
			if (this.Reader != null)
				this.Reader.VirtualAddressTranslationPush(physicalAddress);
			if (this.Writer != null)
				this.Writer.VirtualAddressTranslationPush(physicalAddress);
		}
		/// <summary>Push the stream's position (as a physical address) into the VAT table</summary>
		public void VirtualAddressTranslationPushPosition()
		{
			if (this.Reader != null)
				this.Reader.VirtualAddressTranslationPushPosition();
			if (this.Writer != null)
				this.Writer.VirtualAddressTranslationPushPosition();
		}
		/// <summary>Increase the current address (PA) by a relative offset</summary>
		/// <param name="relativeOffset">Offset, relative to the current address</param>
		public void VirtualAddressTranslationIncrease(Values.PtrHandle relativeOffset)
		{
			if (this.Reader != null)
				this.Reader.VirtualAddressTranslationIncrease(relativeOffset);
			if (this.Writer != null)
				this.Writer.VirtualAddressTranslationIncrease(relativeOffset);
		}
		/// <summary>Pop and return the current address (PA) in the VAT table</summary>
		/// <returns>The VAT's current address value before this call</returns>
		public Values.PtrHandle VirtualAddressTranslationPop()
		{
			var result = this.BaseAddress.Is64bit
				? Values.PtrHandle.InvalidHandle64
				: Values.PtrHandle.InvalidHandle32;

			if (this.Reader != null) result = this.Reader.VirtualAddressTranslationPop();
			if (this.Writer != null) result = this.Writer.VirtualAddressTranslationPop();

			return result;
		}
		#endregion

		#region PositionPtr
		/// <summary>Get the current position as a <see cref="Data.PtrHandle"/></summary>
		/// <param name="ptrSize">Pointer size to use for the result handle</param>
		/// <returns></returns>
		public Values.PtrHandle GetPositionPtrWithExplicitWidth(Shell.ProcessorSize ptrSize) =>
			new Values.PtrHandle(ptrSize, (ulong) this.BaseStream.Position);
		/// <summary>Current position as a <see cref="Data.PtrHandle"/></summary>
		/// <remarks>Pointer traits\info is inherited from <see cref="BaseAddress"/></remarks>
		public Values.PtrHandle PositionPtr =>
			new Values.PtrHandle(this.BaseAddress, (ulong) this.BaseStream.Position);
		#endregion
		#endregion

		#region IKSoftStreamModeable
		public FileAccess StreamPermissions { get; private set; }
		public FileAccess StreamMode { get; set; }
		public bool IsReading { get { return this.StreamMode == FileAccess.Read; } }
		public bool IsWriting { get { return this.StreamMode == FileAccess.Write; } }
		#endregion

		#region IKSoftStreamWithVirtualBuffer Members
		public long VirtualBufferStart { get; set; }
		public long VirtualBufferLength { get; set; }
		#endregion

		#region IKSoftEndianStream
		/// <summary>The assumed byte order of the stream</summary>
		/// <remarks>Use <see cref="ChangeByteOrder"/> to properly change this property</remarks>
		public Shell.EndianFormat ByteOrder { get {
			return this.Reader != null ? this.Reader.ByteOrder : this.Writer.ByteOrder;
		} }

		/// <summary>Change the order in which bytes are ordered to/from the stream</summary>
		/// <param name="newOrder">The new byte order to switch to</param>
		/// <remarks>If <paramref name="newOrder"/> is the same as <see cref="ByteOrder"/> nothing will happen</remarks>
		public void ChangeByteOrder(Shell.EndianFormat newOrder)
		{
			if (this.Reader != null)
				this.Reader.ChangeByteOrder(newOrder);
			if (this.Writer != null)
				this.Writer.ChangeByteOrder(newOrder);
		}

		/// <summary>Convenience class for C# "using" statements where we want to temporarily inverse the current byte order</summary>
		class EndianFormatSwitchBlock : IDisposable
		{
			readonly IDisposable mReaderSwitch, mWriterSwitch;

			/// <summary></summary>
			/// <param name="s"></param>
			public EndianFormatSwitchBlock(EndianStream s)
			{
				if (s.Reader != null)
					this.mReaderSwitch = s.Reader.BeginEndianSwitch();
				if (s.Writer != null)
					this.mWriterSwitch = s.Writer.BeginEndianSwitch();
			}

			#region IDisposable Members
			public void Dispose()
			{
				if (this.mReaderSwitch != null)
					this.mReaderSwitch.Dispose();
				if (this.mWriterSwitch != null)
					this.mWriterSwitch.Dispose();
			}
			#endregion
		};

		/// <summary>Convenience method for C# "using" statements. Temporarily inverts the current byte order which is used for read/writes.</summary>
		/// <returns>Object which when Disposed will return this stream to its original <see cref="Shell.EndianFormat"/> state</returns>
		public IDisposable BeginEndianSwitch()
		{
			return new EndianFormatSwitchBlock(this);
		}
		/// <summary>Convenience method for C# "using" statements. Temporarily inverts the current byte order which is used for read/writes.</summary>
		/// <param name="switchTo">Byte order to switch to</param>
		/// <returns>Object which when Disposed will return this stream to its original <see cref="Shell.EndianFormat"/> state</returns>
		/// <remarks>
		/// If <paramref name="switchTo"/> is the same as <see cref="EndianStream.ByteOrder"/>
		/// then no actual object state changes will happen. However, this construct
		/// will continue to be usable and will Dispose of properly with no error
		/// </remarks>
		public IDisposable BeginEndianSwitch(Shell.EndianFormat switchTo)
		{
			if (switchTo == this.ByteOrder)
				return Util.NullDisposable;

			return new EndianFormatSwitchBlock(this);
		}
		#endregion

		/// <summary>Do we own the base stream?</summary>
		/// <remarks>If we don't own the stream, when this object is disposed, the <see cref="BaseStream"/> won't be closed\disposed</remarks>
		public bool BaseStreamOwner
		{
			get { return this.Reader != null ? this.Reader.BaseStreamOwner : this.Writer.BaseStreamOwner; }
			set {
				if (this.Reader != null)
					this.Reader.BaseStreamOwner = value;
				if (this.Writer != null)
					this.Writer.BaseStreamOwner = value;
			}
		}

		public bool CanRead		{ get { return this.Reader != null; } }
		public bool CanWrite	{ get { return this.Writer != null; } }

		#region Ctor
		private EndianStream()
		{
		}

		public EndianStream(Stream baseStream, FileAccess permissions = FileAccess.ReadWrite)
		{
			Contract.Requires<ArgumentNullException>(baseStream != null);

			this.BaseStream = baseStream;
			this.StreamPermissions = permissions;
			this.StreamMode = 0;

			if (baseStream.CanRead && permissions.CanRead())
				this.Reader = new EndianReader(baseStream);
			if (baseStream.CanWrite && permissions.CanWrite())
				this.Writer = new EndianWriter(baseStream);
		}

		public EndianStream(Stream baseStream, Encoding encoding,
			Shell.EndianFormat byteOrder,
			object streamOwner = null, string name = null, FileAccess permissions = FileAccess.ReadWrite)
		{
			Contract.Requires<ArgumentNullException>(baseStream != null);
			Contract.Requires<ArgumentNullException>(encoding != null);

			this.BaseStream = baseStream;
			this.StreamPermissions = permissions;
			this.StreamMode = 0;

			if (baseStream.CanRead && permissions.CanRead())
				this.Reader = new EndianReader(baseStream, encoding, byteOrder, streamOwner, name);
			if (baseStream.CanWrite && permissions.CanWrite())
				this.Writer = new EndianWriter(baseStream, encoding, byteOrder, streamOwner, name);
		}

		public EndianStream(Stream baseStream,
			Shell.EndianFormat byteOrder,
			object streamOwner = null, string name = null, FileAccess permissions = FileAccess.ReadWrite)
		{
			Contract.Requires<ArgumentNullException>(baseStream != null);

			this.BaseStream = baseStream;
			this.StreamPermissions = permissions;
			this.StreamMode = 0;

			if (baseStream.CanRead && permissions.CanRead())
				this.Reader = new EndianReader(baseStream, byteOrder, streamOwner, name);
			if (baseStream.CanWrite && permissions.CanWrite())
				this.Writer = new EndianWriter(baseStream, byteOrder, streamOwner, name);
		}

		public static EndianStream UsingReader(EndianReader reader)
		{
			Contract.Requires<ArgumentNullException>(reader != null);
			Contract.Ensures(Contract.Result<EndianStream>() != null);

			var s = new EndianStream
			{
				BaseStream = reader.BaseStream,
				StreamPermissions = FileAccess.Read,
				StreamMode = FileAccess.Read,
				Reader = reader
			};

			return s;
		}
		public static EndianStream UsingWriter(EndianWriter writer)
		{
			Contract.Requires<ArgumentNullException>(writer != null);
			Contract.Ensures(Contract.Result<EndianStream>() != null);

			var s = new EndianStream
			{
				BaseStream = writer.BaseStream,
				StreamPermissions = FileAccess.Write,
				StreamMode = FileAccess.Write,
				Writer = writer
			};

			return s;
		}
		#endregion

		#region IDisposable Members
		public void Dispose()
		{
			// NOTE: we intentionally don't call BaseStream's Dispose
			// Reader/Writer should call it, if needed

			if (this.Reader != null)
			{
				this.Reader.Dispose();
				this.Reader = null;
			}
			if (this.Writer != null)
			{
				this.Writer.Dispose();
				this.Writer = null;
			}
		}
		#endregion

		public void Close()
		{
			this.Dispose();
		}

		#region Pad
		public EndianStream Pad(int byteCount)
		{
			Contract.Requires(byteCount > 0);

				 if (this.IsReading)
					 this.Reader.Pad(byteCount);
			else if (this.IsWriting)
				this.Writer.Pad(byteCount);

			return this;
		}
		public EndianStream Pad8()
		{
				 if (this.IsReading)
					 this.Reader.Pad8();
			else if (this.IsWriting)
				this.Writer.Pad8();

			return this;
		}
		public EndianStream Pad16()
		{
				 if (this.IsReading)
					 this.Reader.Pad16();
			else if (this.IsWriting)
				this.Writer.Pad16();

			return this;
		}
		public EndianStream Pad24()
		{
				 if (this.IsReading)
					 this.Reader.Pad24();
			else if (this.IsWriting)
				this.Writer.Pad24();

			return this;
		}
		public EndianStream Pad32()
		{
				 if (this.IsReading)
					 this.Reader.Pad32();
			else if (this.IsWriting)
				this.Writer.Pad32();

			return this;
		}
		public EndianStream Pad64()
		{
				 if (this.IsReading)
					 this.Reader.Pad64();
			else if (this.IsWriting)
				this.Writer.Pad64();

			return this;
		}
		public EndianStream Pad128()
		{
				 if (this.IsReading)
					 this.Reader.Pad128();
			else if (this.IsWriting)
				this.Writer.Pad128();

			return this;
		}
		#endregion

		public EndianStream Stream(ref bool value)
		{
				 if (this.IsReading) value = this.Reader.ReadBoolean();
			else if (this.IsWriting)
				this.Writer.Write(value);

			return this;
		}

		#region Stream (buffers)
		public EndianStream Stream(byte[] value, int index, int count)
		{
			Contract.Requires(value != null);
			Contract.Requires(index >= 0 && index < value.Length);
			Contract.Requires(count >= 0 && count <= value.Length);
			Contract.Requires((index+count) <= value.Length);

				 if (this.IsReading)
					 this.Reader.Read(value, index, count);
			else if (this.IsWriting)
				this.Writer.Write(value, index, count);

			return this;
		}
		public EndianStream Stream(byte[] value, int count)
		{
			Contract.Requires(value != null);
			Contract.Requires(count >= 0 && count <= value.Length);

				 if (this.IsReading)
					 this.Reader.Read(value, count);
			else if (this.IsWriting)
				this.Writer.Write(value, count);

			return this;
		}
		public EndianStream Stream(byte[] value)
		{
			Contract.Requires(value != null);

				 if (this.IsReading)
					 this.Reader.Read(value, value.Length);
			else if (this.IsWriting)
				this.Writer.Write(value, value.Length);

			return this;
		}
		public EndianStream Stream(char[] value, int index, int count)
		{
			Contract.Requires(value != null);
			Contract.Requires(index >= 0 && index < value.Length);
			Contract.Requires(count >= 0 && count <= value.Length);
			Contract.Requires((index+count) <= value.Length);

				 if (this.IsReading)
					 this.Reader.Read(value, index, count);
			else if (this.IsWriting)
				this.Writer.Write(value, index, count);

			return this;
		}
		public EndianStream Stream(char[] value, int count)
		{
			Contract.Requires(value != null);
			Contract.Requires(count >= 0 && count <= value.Length);

				 if (this.IsReading)
					 this.Reader.Read(value, count);
			else if (this.IsWriting)
				this.Writer.Write(value, count);

			return this;
		}
		public EndianStream Stream(char[] value)
		{
			Contract.Requires(value != null);

				 if (this.IsReading)
					 this.Reader.Read(value, value.Length);
			else if (this.IsWriting)
				this.Writer.Write(value, value.Length);

			return this;
		}
		#endregion

		#region Stream group tag
		char[] mTagScratchBuffer = new char[8];

		public EndianStream StreamTag(ref uint value)
		{
				 if (this.IsReading) value = this.Reader.ReadTagUInt32();
			else if (this.IsWriting)
				this.Writer.WriteTag32(value);

			return this;
		}
		// Tag always appears in big-endian order in the stream
		public EndianStream StreamTagBigEndian(ref uint value)
		{
			if (this.IsReading)
			{
				this.Reader.ReadTag32(this.mTagScratchBuffer);
				value = Values.GroupTagData32.ToUInt(this.mTagScratchBuffer);
			}
			else if (this.IsWriting)
			{
				Values.GroupTagData32.FromUInt(value, this.mTagScratchBuffer);
				this.Writer.WriteTag32(this.mTagScratchBuffer);
			}

			return this;
		}

		public EndianStream StreamTag(ref ulong value)
		{
				 if (this.IsReading) value = this.Reader.ReadTagUInt64();
			else if (this.IsWriting)
				this.Writer.WriteTag64(value);

			return this;
		}
		#endregion

		// #TODO: generate with T4
		#region Stream numerics
		public EndianStream StreamInt24(ref int value)
		{
				 if (this.IsReading) value = this.Reader.ReadInt24();
			else if (this.IsWriting)
				this.Writer.WriteInt24(value);

			return this;
		}
		public EndianStream StreamUInt24(ref uint value)
		{
				 if (this.IsReading) value = this.Reader.ReadUInt24();
			else if (this.IsWriting)
				this.Writer.WriteUInt24(value);

			return this;
		}
		public EndianStream StreamUInt40(ref ulong value)
		{
				 if (this.IsReading) value = this.Reader.ReadUInt40();
			else if (this.IsWriting)
				this.Writer.WriteUInt40(value);

			return this;
		}
		#endregion

		#region Stream strings
		public EndianStream Stream(ref string value)
		{
				 if (this.IsReading) value = this.Reader.ReadString();
			else if (this.IsWriting)
				this.Writer.Write(value);

			return this;
		}

		public EndianStream Stream(ref string value, Memory.Strings.StringStorage storage)
		{
				 if (this.IsReading) value = this.Reader.ReadString(storage);
			else if (this.IsWriting)
				this.Writer.Write(value, storage);

			return this;
		}
		public EndianStream Stream(ref string value, Memory.Strings.StringStorage storage, int length)
		{
				 if (this.IsReading) value = this.Reader.ReadString(storage, length);
			else if (this.IsWriting)
				this.Writer.Write(value, storage);

			return this;
		}

		public EndianStream Stream(ref string value, Text.StringStorageEncoding encoding)
		{
			Contract.Requires(encoding != null);

				 if (this.IsReading) value = this.Reader.ReadString(encoding);
			else if (this.IsWriting)
				this.Writer.Write(value, encoding);

			return this;
		}
		public EndianStream Stream(ref string value, Text.StringStorageEncoding encoding, int length)
		{
			Contract.Requires(encoding != null);

				 if (this.IsReading) value = this.Reader.ReadString(encoding, length);
			else if (this.IsWriting)
				this.Writer.Write(value, encoding);

			return this;
		}
		#endregion

		#region Stream Pointer
		public EndianStream StreamRawPointer(ref Values.PtrHandle value)
		{
				 if (this.IsReading)
					 this.Reader.ReadRawPointer(ref value);
			else if (this.IsWriting)
				this.Writer.WriteRawPointer(value);

			return this;
		}
		public EndianStream StreamRawPointer(ref Values.PtrHandle value, Shell.ProcessorSize addressSize)
		{
				 if (this.IsReading) value = this.Reader.ReadRawPointer(addressSize);
			else if (this.IsWriting)
				this.Writer.WriteRawPointer(value);

			return this;
		}

		public EndianStream StreamPointer(ref Values.PtrHandle value)
		{
				 if (this.IsReading)
					 this.Reader.ReadPointer(ref value);
			else if (this.IsWriting)
				this.Writer.WritePointer(value);

			return this;
		}
		public EndianStream StreamPointer(ref Values.PtrHandle value, Shell.ProcessorSize addressSize)
		{
				 if (this.IsReading) value = this.Reader.ReadPointer(addressSize);
			else if (this.IsWriting)
				this.Writer.WritePointer(value);

			return this;
		}

		public EndianStream StreamPointerViaBaseAddress(ref Values.PtrHandle value)
		{
				 if (this.IsReading) value = this.Reader.ReadPointerViaBaseAddress();
			else if (this.IsWriting)
				this.Writer.WritePointer(value);

			return this;
		}
		#endregion

		public EndianStream Stream7BitEncodedInt(ref int value)
		{
				 if (this.IsReading) value = this.Reader.Read7BitEncodedInt();
			else if (this.IsWriting)
				this.Writer.Write7BitEncodedInt(value);

			return this;
		}

		public EndianStream Stream(ref DateTime value, bool isUnixTime = false)
		{
				 if (this.IsReading) value = this.Reader.ReadDateTime(isUnixTime);
			else if (this.IsWriting)
				this.Writer.Write(value, isUnixTime);

			return this;
		}

		public EndianStream Stream<TEnum>(ref TEnum value, IEnumEndianStreamer<TEnum> implementation)
			where TEnum : struct
		{
			Contract.Requires(implementation != null);

			implementation.Stream(this, ref value);

			return this;
		}

		#region Stream Values
		public EndianStream StreamValue<T>(ref T value)
			where T : struct, IEndianStreamable
		{
			if (this.IsReading)
			{
				value = new T();
				value.Read(this.Reader);
			}
			else if (this.IsWriting) value.Write(this.Writer);

			return this;
		}
		public EndianStream StreamValue<T>(ref T value, Func<T> initializer)
			where T : struct, IEndianStreamable
		{
			Contract.Requires(initializer != null);

			if (this.IsReading)
			{
				value = initializer();
				value.Read(this.Reader);
			}
			else if (this.IsWriting) value.Write(this.Writer);

			return this;
		}

		public EndianStream Stream<T>(ref T value)
			where T : struct, IEndianStreamSerializable
		{
			if (this.IsReading)
				value = new T();

			value.Serialize(this);

			return this;
		}
		#endregion

		#region Stream Objects
		public EndianStream StreamObject<T>(T value)
			where T : class, IEndianStreamable
		{
			Contract.Requires(value != null);

				 if (this.IsReading) value.Read(this.Reader);
			else if (this.IsWriting) value.Write(this.Writer);

			return this;
		}
		public EndianStream StreamObject<T>(ref T value, Func<T> initializer)
			where T : class, IEndianStreamable
		{
			Contract.Requires(this.IsReading || value != null);
			Contract.Requires(initializer != null);

			if (this.IsReading)
			{
				value = initializer();
				value.Read(this.Reader);
			}
			else if (this.IsWriting) value.Write(this.Writer);

			return this;
		}

		public EndianStream Stream<T>(T value)
			where T : class, IEndianStreamSerializable
		{
			Contract.Requires(value != null);

			value.Serialize(this);

			return this;
		}
		public EndianStream Stream<T>(ref T value, Func<T> initializer)
			where T : class, IEndianStreamSerializable
		{
			Contract.Requires(this.IsReading || value != null);
			Contract.Requires(initializer != null);

			if (this.IsReading)
				value = initializer();

			value.Serialize(this);

			return this;
		}
		#endregion

		#region Stream Value Methods
		public delegate void ReadValueDelegate<T>(EndianReader r, out T value);

		public EndianStream StreamValueMethods<T>(ref T value, ReadValueDelegate<T> read, Action<EndianWriter, T> write)
		{
			Contract.Requires(read != null);
			Contract.Requires(write != null);

				 if (this.IsReading) read (this.Reader, out value);
			else if (this.IsWriting) write(this.Writer, value);

			return this;
		}
		#endregion

		public EndianStream StreamObjectMethods<T>(T theObj, Action<EndianReader, T> read, Action<EndianWriter, T> write)
			where T : class
		{
			Contract.Requires(theObj != null);
			Contract.Requires(read != null);
			Contract.Requires(write != null);

				 if (this.IsReading) read (this.Reader, theObj);
			else if (this.IsWriting) write(this.Writer, theObj);

			return this;
		}

		#region Stream Methods
		public EndianStream StreamMethods(Action<EndianReader> read, Action<EndianWriter> write)
		{
			Contract.Requires(read != null);
			Contract.Requires(write != null);

				 if (this.IsReading) read(this.Reader);
			else if (this.IsWriting) write(this.Writer);

			return this;
		}
		public EndianStream StreamMethods<T>(T context, Action<T, EndianReader> read, Action<T, EndianWriter> write)
			where T : class
		{
			Contract.Requires(context != null);
			Contract.Requires(read != null);
			Contract.Requires(write != null);

				 if (this.IsReading) read(context, this.Reader);
			else if (this.IsWriting) write(context, this.Writer);

			return this;
		}
		#endregion

		#region Stream Fixed Array
		public EndianStream StreamFixedArray(bool[] array, int startIndex, int length)
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
		public EndianStream StreamFixedArray(bool[] array)
		{
			Contract.Requires(array != null);

			return this.StreamFixedArray(array, 0, array.Length);
		}
		#endregion

		#region Stream Array Values
		public delegate EndianStream StreamArrayValueDelegate<T>(ref T value);

		public EndianStream StreamArray<T>(T[] values)
			where T : struct, IEndianStreamSerializable
		{
			Contract.Requires(values != null);

			for (int x = 0; x < values.Length; x++)
				this.Stream(ref values[x]);

			return this;
		}

		public EndianStream StreamArrayInt32<T>(ref T[] values)
			where T : struct, IEndianStreamSerializable
		{
			Contract.Requires(this.IsReading || values != null);

			bool reading = this.IsReading;

			int count = reading ? 0 : values.Length;
			this.Stream(ref count);
			if (reading)
				values = new T[count];

			for (int x = 0; x < count; x++)
				this.Stream(ref values[x]);

			return this;
		}
		public EndianStream StreamArrayInt32<T>(ref T[] values, StreamArrayValueDelegate<T> streamFunc)
			where T : struct
		{
			Contract.Requires(this.IsReading || values != null);
			Contract.Requires(streamFunc != null);

			bool reading = this.IsReading;

			int count = reading ? 0 : values.Length;
			this.Stream(ref count);
			if (reading)
				values = new T[count];

			for (int x = 0; x < count; x++)
				streamFunc(ref values[x]);

			return this;
		}
		#endregion

		#region Stream Array Objects
		public EndianStream StreamArray<T>(T[] values, Func<T> initializer)
			where T : class, IEndianStreamSerializable
		{
			Contract.Requires(values != null);
			Contract.Requires(initializer != null);

			for (int x = 0; x < values.Length; x++)
				this.Stream(ref values[x], initializer);

			return this;
		}

		public EndianStream StreamArrayInt32<T>(ref T[] values, Func<T> initializer)
			where T : class, IEndianStreamSerializable
		{
			Contract.Requires(this.IsReading || values != null);
			Contract.Requires(initializer != null);

			bool reading = this.IsReading;

			int count = reading ? 0 : values.Length;
			this.Stream(ref count);
			if (reading)
				values = new T[count];

			for (int x = 0; x < count; x++)
				this.Stream(ref values[x], initializer);

			return this;
		}
		#endregion

		#region Stream Array Methods
		public delegate void ReadArrayDelegate<T>(EndianReader r, ref T[] array);
		public delegate void WriteArrayDelegate<T>(EndianWriter r, T[] array);

		public EndianStream StreamArrayMethods<T>(ref T[] array, ReadArrayDelegate<T> read, WriteArrayDelegate<T> write)
		{
			Contract.Requires(array != null);
			Contract.Requires(read != null);
			Contract.Requires(write != null);

				 if (this.IsReading) read (this.Reader, ref array);
			else if (this.IsWriting) write(this.Writer, array);

			return this;
		}
		#endregion

		#region Stream List
		public EndianStream StreamListElementsWithClear<T>(IList<T> values, int readCount, Func<T> initializer)
			where T : class, IEndianStreamSerializable
		{
			Contract.Requires(values != null);
			Contract.Requires(initializer != null);
			Contract.Ensures(values.Count == readCount);

			bool reading = this.IsReading;

			if (reading)
				values.Clear();
			else
				readCount = values.Count;

			for (int x = 0; x < readCount; x++)
			{
				T v = reading
					? initializer()
					: values[x];

				this.Stream(v);

				if (reading)
					values.Add(v);
			}

			return this;
		}
		#endregion

		#region Stream signature
		public EndianStream StreamSignature(byte signature)
		{
				 if (this.IsReading) SignatureMismatchException.Assert(this.Reader, signature);
			else if (this.IsWriting)
				this.Writer.Write(signature);

			return this;
		}
		public EndianStream StreamSignature(ushort signature)
		{
				 if (this.IsReading) SignatureMismatchException.Assert(this.Reader, signature);
			else if (this.IsWriting)
				this.Writer.Write(signature);

			return this;
		}
		public EndianStream StreamSignature(uint signature)
		{
				 if (this.IsReading) SignatureMismatchException.Assert(this.Reader, signature);
			else if (this.IsWriting)
				this.Writer.Write(signature);

			return this;
		}
		public EndianStream StreamSignature(ulong signature)
		{
				 if (this.IsReading) SignatureMismatchException.Assert(this.Reader, signature);
			else if (this.IsWriting)
				this.Writer.Write(signature);

			return this;
		}
		public EndianStream StreamSignature(string signature, Memory.Strings.StringStorage storage)
		{
			Contract.Requires(!string.IsNullOrEmpty(signature));

				 if (this.IsReading) SignatureMismatchException.Assert(this.Reader, signature, storage);
			else if (this.IsWriting)
				this.Writer.Write(signature, storage);

			return this;
		}
		public EndianStream StreamSignature(string signature, Text.StringStorageEncoding encoding)
		{
			Contract.Requires(!string.IsNullOrEmpty(signature));
			Contract.Requires(encoding != null);

				 if (this.IsReading) SignatureMismatchException.Assert(this.Reader, signature, encoding);
			else if (this.IsWriting)
				this.Writer.Write(signature, encoding);

			return this;
		}
		#endregion

		#region Stream version
		public EndianStream StreamVersion(byte version)
		{
				 if (this.IsReading) VersionMismatchException.Assert(this.Reader, version);
			else if (this.IsWriting)
				this.Writer.Write(version);

			return this;
		}
		public EndianStream StreamVersion(ushort version)
		{
				 if (this.IsReading) VersionMismatchException.Assert(this.Reader, version);
			else if (this.IsWriting)
				this.Writer.Write(version);

			return this;
		}
		public EndianStream StreamVersion(uint version)
		{
				 if (this.IsReading) VersionMismatchException.Assert(this.Reader, version);
			else if (this.IsWriting)
				this.Writer.Write(version);

			return this;
		}
		public EndianStream StreamVersion(ulong version)
		{
				 if (this.IsReading) VersionMismatchException.Assert(this.Reader, version);
			else if (this.IsWriting)
				this.Writer.Write(version);

			return this;
		}

		public EndianStream StreamVersion(ref byte version
			, byte versionMin
			, byte versionMax)
		{
				 if (this.IsReading) version = VersionOutOfRangeException.Assert(this.Reader, versionMin, versionMax);
			else if (this.IsWriting)
				this.Writer.Write(version);

			return this;
		}
		public EndianStream StreamVersion(ref ushort version
			, ushort versionMin
			, ushort versionMax)
		{
				 if (this.IsReading) version = VersionOutOfRangeException.Assert(this.Reader, versionMin, versionMax);
			else if (this.IsWriting)
				this.Writer.Write(version);

			return this;
		}
		public EndianStream StreamVersion(ref uint version
			, uint versionMin
			, uint versionMax)
		{
				 if (this.IsReading) version = VersionOutOfRangeException.Assert(this.Reader, versionMin, versionMax);
			else if (this.IsWriting)
				this.Writer.Write(version);

			return this;
		}
		public EndianStream StreamVersion(ref ulong version
			, ulong versionMin
			, ulong versionMax)
		{
				 if (this.IsReading) version = VersionOutOfRangeException.Assert(this.Reader, versionMin, versionMax);
			else if (this.IsWriting)
				this.Writer.Write(version);

			return this;
		}

		/// <summary>
		/// Stream a zero-based, positive enum as a version value.
		/// Uses underlying type for bit width.
		/// </summary>
		/// <typeparam name="TEnum"></typeparam>
		/// <param name="version"></param>
		/// <param name="maxCount"></param>
		/// <returns></returns>
		public EndianStream StreamVersionEnum<TEnum>(ref TEnum version
			, TEnum maxCount)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			if (this.IsReading)
				version = VersionOutOfRangeException.AssertZeroBasedEnum(this.Reader, maxCount);
			else if (this.IsWriting)
			{
				var type_code = Reflection.EnumUtil<TEnum>.UnderlyingTypeCode;

				switch (type_code)
				{
					case TypeCode.SByte:
					case TypeCode.Byte:
					{
						var integer = Reflection.EnumValue<TEnum>.ToByte(maxCount);
						this.Writer.Write(integer);
					} break;

					case TypeCode.Int16:
					case TypeCode.UInt16:
					{
						var integer = Reflection.EnumValue<TEnum>.ToUInt16(maxCount);
						this.Writer.Write(integer);
					} break;

					case TypeCode.Int32:
					case TypeCode.UInt32:
					{
						var integer = Reflection.EnumValue<TEnum>.ToUInt32(maxCount);
						this.Writer.Write(integer);
					} break;

					case TypeCode.Int64:
					case TypeCode.UInt64:
					{
						var integer = Reflection.EnumValue<TEnum>.ToUInt64(maxCount);
						this.Writer.Write(integer);
					} break;

					default:
						throw new Debug.UnreachableException(type_code.ToString());
				}
			}

			return this;
		}
		#endregion

		[System.Diagnostics.Conditional("TRACE")]
		public void TraceAndDebugPosition(ref long position)
		{
			if (this.IsReading)
				position = this.BaseStream.Position;
			else if (this.IsWriting)
				Contract.Assert(position == this.BaseStream.Position);
		}
	};
}
