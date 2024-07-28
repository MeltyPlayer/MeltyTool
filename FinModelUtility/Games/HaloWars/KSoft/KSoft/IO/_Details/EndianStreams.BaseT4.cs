using System;
using System.IO;

namespace KSoft.IO
{
	partial class EndianReader
	{
		#region EndianStream
		/// <summary>Owner of this stream</summary>
		public object Owner { get; set; }

		public object UserData { get; set; }

		/// <summary>Do we own the base stream?</summary>
		/// <remarks>If we don't own the stream, when this object is disposed, the <see cref="BaseStream"/> won't be closed\disposed</remarks>
		public bool BaseStreamOwner { get; set; }

		/// <summary>Name of the underlying stream this object is interfacing with</summary>
		/// <remarks>So if this endian stream is interfacing with a file, this will be it's name</remarks>
		public string StreamName { get; private set; }

		/// <summary>Base address used for simulating pointers in the stream</summary>
		/// <remarks>Default value is <see cref="Data.PtrHandle.Null32"/></remarks>
		public Values.PtrHandle BaseAddress { get; set; }

		#region IKSoftEndianStream
		/// <summary>The assumed byte order of the stream</summary>
		/// <remarks>Use <see cref="ChangeByteOrder"/> to properly change this property</remarks>
		public Shell.EndianFormat ByteOrder { get; private set; }

		/// <summary>Change the order in which bytes are ordered to/from the stream</summary>
		/// <param name="newOrder">The new byte order to switch to</param>
		/// <remarks>If <paramref name="newOrder"/> is the same as <see cref="ByteOrder"/> nothing will happen</remarks>
		public void ChangeByteOrder(Shell.EndianFormat newOrder)
		{
			if (newOrder != this.ByteOrder)
			{
				this.ByteOrder = newOrder;
				this.mRequiresByteSwap = !this.mRequiresByteSwap;
			}
		}

		/// <summary>This will be true when the stream's byte order is not the same as the <see cref="Shell.Platform.Environment"/>'s byte order</summary>
		/*readonly*/ bool mRequiresByteSwap;

		/// <summary>Convenience class for C# "using" statements where we want to temporarily inverse the current byte order</summary>
		class EndianFormatSwitchBlock : IDisposable
		{
			readonly EndianReader mStream;
			readonly Shell.EndianFormat mOldByteOrder;
			readonly bool mOldRequiresByteSwap;
			/// <summary></summary>
			/// <param name="s"></param>
			/// <param name="requiresSwitch">Is there an actual order switch even occurring?</param>
			public EndianFormatSwitchBlock(EndianReader s, bool requiresSwitch)
			{
				this.mStream = requiresSwitch
					? s
					: null;

				if (requiresSwitch) // if not, don't do anything but keep the IDisposable wheel turning
				{
					this.mOldByteOrder = s.ByteOrder;
					this.mOldRequiresByteSwap = s.mRequiresByteSwap;

					s.ChangeByteOrder(this.mOldByteOrder.Invert());
				}
			}

			#region IDisposable Members
			public void Dispose()
			{
				if (this.mStream != null)
					this.mStream.ChangeByteOrder(this.mOldByteOrder);
			}
			#endregion
		};

		/// <summary>Convenience method for C# "using" statements. Temporarily inverts the current byte order which is used for read/writes.</summary>
		/// <returns>Object which when Disposed will return this stream to its original <see cref="Shell.EndianFormat"/> state</returns>
		public IDisposable BeginEndianSwitch()
		{
			return new EndianFormatSwitchBlock(this, true);
		}
		/// <summary>Convenience method for C# "using" statements. Temporarily inverts the current byte order which is used for read/writes.</summary>
		/// <param name="switchTo">Byte order to switch to</param>
		/// <returns>Object which when Disposed will return this stream to its original <see cref="Shell.EndianFormat"/> state</returns>
		/// <remarks>
		/// If <paramref name="switchTo"/> is the same as <see cref="EndianStream.State"/>
		/// then no actual object state changes will happen. However, this construct
		/// will continue to be usable and will Dispose of properly with no error
		/// </remarks>
		public IDisposable BeginEndianSwitch(Shell.EndianFormat switchTo)
		{
			if (switchTo == this.ByteOrder)
				return Util.NullDisposable;

			return new EndianFormatSwitchBlock(this, true);
		}
		#endregion

		#region StringEncoding
		System.Text.Encoding mStringEncoding;
		#endregion

		#region Seek
		/// <summary>Moves the stream cursor to <paramref name="offset"/> relative to the beginning of the stream</summary>
		/// <param name="offset">Offset to seek to</param>
		public void Seek32(uint offset)						=> this.Seek(offset, SeekOrigin.Begin);
		/// <summary>Moves the stream cursor to <paramref name="offset"/> relative to <paramref name="origin"/></summary>
		/// <param name="offset">Offset to seek to</param>
		/// <param name="origin">Origin to base seek operation</param>
		public void Seek32(uint offset, SeekOrigin origin)	=> this.Seek(offset, origin);
		/// <summary>Moves the stream cursor to <paramref name="offset"/> relative to the beginning of the stream</summary>
		/// <param name="offset">Offset to seek to</param>
		public void Seek32(int offset)						=> this.Seek(offset, SeekOrigin.Begin);
		/// <summary>Moves the stream cursor to <paramref name="offset"/> relative to <paramref name="origin"/></summary>
		/// <param name="offset">Offset to seek to</param>
		/// <param name="origin">Origin to base seek operation</param>
		public void Seek32(int offset, SeekOrigin origin)	=> this.Seek(offset, origin);

		/// <summary>Moves the stream cursor to <paramref name="offset"/> relative to the beginning of the stream</summary>
		/// <param name="offset">Offset to seek to</param>
		public void Seek(long offset)						=> this.Seek(offset, SeekOrigin.Begin);
		/// <summary>Moves the stream cursor to <paramref name="offset"/> relative to <paramref name="origin"/></summary>
		/// <param name="offset">Offset to seek to</param>
		/// <param name="origin">Origin to base seek operation</param>
		public void Seek(long offset, SeekOrigin origin)	=> base.BaseStream.Seek(offset, origin);
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
	};

	partial class EndianWriter
	{
		#region EndianStream
		/// <summary>Owner of this stream</summary>
		public object Owner { get; set; }

		public object UserData { get; set; }

		/// <summary>Do we own the base stream?</summary>
		/// <remarks>If we don't own the stream, when this object is disposed, the <see cref="BaseStream"/> won't be closed\disposed</remarks>
		public bool BaseStreamOwner { get; set; }

		/// <summary>Name of the underlying stream this object is interfacing with</summary>
		/// <remarks>So if this endian stream is interfacing with a file, this will be it's name</remarks>
		public string StreamName { get; private set; }

		/// <summary>Base address used for simulating pointers in the stream</summary>
		/// <remarks>Default value is <see cref="Data.PtrHandle.Null32"/></remarks>
		public Values.PtrHandle BaseAddress { get; set; }

		#region IKSoftEndianStream
		/// <summary>The assumed byte order of the stream</summary>
		/// <remarks>Use <see cref="ChangeByteOrder"/> to properly change this property</remarks>
		public Shell.EndianFormat ByteOrder { get; private set; }

		/// <summary>Change the order in which bytes are ordered to/from the stream</summary>
		/// <param name="newOrder">The new byte order to switch to</param>
		/// <remarks>If <paramref name="newOrder"/> is the same as <see cref="ByteOrder"/> nothing will happen</remarks>
		public void ChangeByteOrder(Shell.EndianFormat newOrder)
		{
			if (newOrder != this.ByteOrder)
			{
				this.ByteOrder = newOrder;
				this.mRequiresByteSwap = !this.mRequiresByteSwap;
			}
		}

		/// <summary>This will be true when the stream's byte order is not the same as the <see cref="Shell.Platform.Environment"/>'s byte order</summary>
		/*readonly*/ bool mRequiresByteSwap;

		/// <summary>Convenience class for C# "using" statements where we want to temporarily inverse the current byte order</summary>
		class EndianFormatSwitchBlock : IDisposable
		{
			readonly EndianWriter mStream;
			readonly Shell.EndianFormat mOldByteOrder;
			readonly bool mOldRequiresByteSwap;
			/// <summary></summary>
			/// <param name="s"></param>
			/// <param name="requiresSwitch">Is there an actual order switch even occurring?</param>
			public EndianFormatSwitchBlock(EndianWriter s, bool requiresSwitch)
			{
				this.mStream = requiresSwitch
					? s
					: null;

				if (requiresSwitch) // if not, don't do anything but keep the IDisposable wheel turning
				{
					this.mOldByteOrder = s.ByteOrder;
					this.mOldRequiresByteSwap = s.mRequiresByteSwap;

					s.ChangeByteOrder(this.mOldByteOrder.Invert());
				}
			}

			#region IDisposable Members
			public void Dispose()
			{
				if (this.mStream != null)
					this.mStream.ChangeByteOrder(this.mOldByteOrder);
			}
			#endregion
		};

		/// <summary>Convenience method for C# "using" statements. Temporarily inverts the current byte order which is used for read/writes.</summary>
		/// <returns>Object which when Disposed will return this stream to its original <see cref="Shell.EndianFormat"/> state</returns>
		public IDisposable BeginEndianSwitch()
		{
			return new EndianFormatSwitchBlock(this, true);
		}
		/// <summary>Convenience method for C# "using" statements. Temporarily inverts the current byte order which is used for read/writes.</summary>
		/// <param name="switchTo">Byte order to switch to</param>
		/// <returns>Object which when Disposed will return this stream to its original <see cref="Shell.EndianFormat"/> state</returns>
		/// <remarks>
		/// If <paramref name="switchTo"/> is the same as <see cref="EndianStream.State"/>
		/// then no actual object state changes will happen. However, this construct
		/// will continue to be usable and will Dispose of properly with no error
		/// </remarks>
		public IDisposable BeginEndianSwitch(Shell.EndianFormat switchTo)
		{
			if (switchTo == this.ByteOrder)
				return Util.NullDisposable;

			return new EndianFormatSwitchBlock(this, true);
		}
		#endregion

		#region StringEncoding
		System.Text.Encoding mStringEncoding;
		#endregion

		#region Seek
		/// <summary>Moves the stream cursor to <paramref name="offset"/> relative to the beginning of the stream</summary>
		/// <param name="offset">Offset to seek to</param>
		public void Seek32(uint offset)						=> this.Seek(offset, SeekOrigin.Begin);
		/// <summary>Moves the stream cursor to <paramref name="offset"/> relative to <paramref name="origin"/></summary>
		/// <param name="offset">Offset to seek to</param>
		/// <param name="origin">Origin to base seek operation</param>
		public void Seek32(uint offset, SeekOrigin origin)	=> this.Seek(offset, origin);
		/// <summary>Moves the stream cursor to <paramref name="offset"/> relative to the beginning of the stream</summary>
		/// <param name="offset">Offset to seek to</param>
		public void Seek32(int offset)						=> this.Seek(offset, SeekOrigin.Begin);
		/// <summary>Moves the stream cursor to <paramref name="offset"/> relative to <paramref name="origin"/></summary>
		/// <param name="offset">Offset to seek to</param>
		/// <param name="origin">Origin to base seek operation</param>
		public void Seek32(int offset, SeekOrigin origin)	=> this.Seek(offset, origin);

		/// <summary>Moves the stream cursor to <paramref name="offset"/> relative to the beginning of the stream</summary>
		/// <param name="offset">Offset to seek to</param>
		public void Seek(long offset)						=> this.Seek(offset, SeekOrigin.Begin);
		/// <summary>Moves the stream cursor to <paramref name="offset"/> relative to <paramref name="origin"/></summary>
		/// <param name="offset">Offset to seek to</param>
		/// <param name="origin">Origin to base seek operation</param>
		public void Seek(long offset, SeekOrigin origin)	=> base.BaseStream.Seek(offset, origin);
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
	};

}

namespace KSoft
{
	partial class TypeExtensions
	{
		public static void Read(this IO.EndianReader s, out bool value)	=> value = s.ReadBoolean();
		public static void Write(this bool value, IO.EndianWriter s)		=> s.Write(value);

		public static void Read(this IO.EndianReader s, out char value)	=> value = s.ReadChar();
		public static void Write(this char value, IO.EndianWriter s)		=> s.Write(value);

		public static void Read(this IO.EndianReader s, out byte value)	=> value = s.ReadByte();
		public static void Write(this byte value, IO.EndianWriter s)		=> s.Write(value);

		public static void Read(this IO.EndianReader s, out sbyte value)	=> value = s.ReadSByte();
		public static void Write(this sbyte value, IO.EndianWriter s)		=> s.Write(value);

		public static void Read(this IO.EndianReader s, out ushort value)	=> value = s.ReadUInt16();
		public static void Write(this ushort value, IO.EndianWriter s)		=> s.Write(value);

		public static void Read(this IO.EndianReader s, out short value)	=> value = s.ReadInt16();
		public static void Write(this short value, IO.EndianWriter s)		=> s.Write(value);

		public static void Read(this IO.EndianReader s, out uint value)	=> value = s.ReadUInt32();
		public static void Write(this uint value, IO.EndianWriter s)		=> s.Write(value);

		public static void Read(this IO.EndianReader s, out int value)	=> value = s.ReadInt32();
		public static void Write(this int value, IO.EndianWriter s)		=> s.Write(value);

		public static void Read(this IO.EndianReader s, out ulong value)	=> value = s.ReadUInt64();
		public static void Write(this ulong value, IO.EndianWriter s)		=> s.Write(value);

		public static void Read(this IO.EndianReader s, out long value)	=> value = s.ReadInt64();
		public static void Write(this long value, IO.EndianWriter s)		=> s.Write(value);

		public static void Read(this IO.EndianReader s, out float value)	=> value = s.ReadSingle();
		public static void Write(this float value, IO.EndianWriter s)		=> s.Write(value);

		public static void Read(this IO.EndianReader s, out double value)	=> value = s.ReadDouble();
		public static void Write(this double value, IO.EndianWriter s)		=> s.Write(value);

	};
}
