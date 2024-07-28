using System;
using System.Collections.Generic;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Phoenix.Xmb
{
	public sealed partial class BinaryDataTreeMemoryPool
		: IDisposable
	{
		/// <summary>Default amount of entry memory allocated for use</summary>
		const int kEntryStartCount = 16;

		Dictionary<uint, PoolEntry> mEntries;
		Dictionary<uint, uint> mDataOffsetToSizeValue;
		uint mPoolSize;

		public uint Size { get { return this.mPoolSize; } }

		IO.EndianReader mBuffer;
		uint mBufferedDataRemaining;

		internal IO.EndianReader InternalBuffer { get { return this.mBuffer; } }

		public BinaryDataTreeMemoryPool(int initialEntryCount = kEntryStartCount)
		{
			if (initialEntryCount < 0)
				initialEntryCount = kEntryStartCount;

			this.mEntries = new Dictionary<uint, PoolEntry>(kEntryStartCount);
			this.mDataOffsetToSizeValue = new Dictionary<uint, uint>();
		}
		public BinaryDataTreeMemoryPool(byte[] buffer, Shell.EndianFormat byteOrder = Shell.EndianFormat.Big)
			: this()
		{
			Contract.Requires(buffer != null);

			this.mPoolSize = (uint)buffer.Length;
			var ms = new System.IO.MemoryStream(buffer, false);
			this.mBuffer = new IO.EndianReader(ms, byteOrder, this);
			this.mBufferedDataRemaining = this.mPoolSize;
		}

		#region IDisposable Members
		void DisposeBuffer()
		{
			Util.DisposeAndNull(ref this.mBuffer);
		}
		public void Dispose()
		{
			this.DisposeBuffer();

			if (this.mEntries != null)
			{
				this.mEntries.Clear();
				this.mEntries = null;
			}
		}
		#endregion

		#region Add
		#endregion

		#region Get
		bool ValidOffset(uint offset) { return offset < this.mPoolSize; }

		public uint GetSizeValue(uint dataOffset)
		{
			if (!this.ValidOffset(dataOffset))
				throw new ArgumentOutOfRangeException("dataOffset", string.Format("{0} > {1}",
					dataOffset.ToString("X8"),
					this.mPoolSize.ToString("X6")));
			if (dataOffset < sizeof(uint))
				throw new ArgumentOutOfRangeException("dataOffset", "Offset doesn't have room for a size value");

			uint size_value;
			if (!this.mDataOffsetToSizeValue.TryGetValue(dataOffset, out size_value))
			{
				if (this.mBufferedDataRemaining == 0)
					throw new InvalidOperationException("No data left in buffer");
				else if (this.mBuffer == null)
					throw new InvalidOperationException("No underlying buffer");

				uint size_offset = dataOffset - sizeof(uint);
				// Great, now read the entry's value data
				this.mBuffer.Seek32(size_offset);
				size_value = this.mBuffer.ReadUInt32();

				// Update how much data is still remaining
				this.mBufferedDataRemaining -= sizeof(uint);

				if (this.mBufferedDataRemaining == 0)
					this.DisposeBuffer();

				this.mDataOffsetToSizeValue.Add(dataOffset, size_value);
			}

			return size_value;
		}

		internal PoolEntry DeBuffer(BinaryDataTreeNameValue nameValue)
		{
			var type_desc = nameValue.GuessTypeDesc();
			uint offset = nameValue.Offset;
			bool size_is_indirect = nameValue.SizeIsIndirect;
			return this.DeBuffer(type_desc, offset, size_is_indirect);
		}

		PoolEntry DeBuffer(BinaryDataTreeVariantTypeDesc desc, uint offset, bool sizeIsIndirect = false)
		{
			if (!this.ValidOffset(offset))
				throw new ArgumentOutOfRangeException("offset", string.Format("{0} > {1}",
					offset.ToString("X8"),
					this.mPoolSize.ToString("X6")));

			PoolEntry e;
			if (!this.mEntries.TryGetValue(offset, out e))
			{
					 if (this.mBufferedDataRemaining == 0)	throw new InvalidOperationException("No data left in buffer");
				else if (this.mBuffer == null)				throw new InvalidOperationException("No underlying buffer");

				// Create our new entry, setting any additional properties
				e = PoolEntry.New(desc);
				if (sizeIsIndirect)
				{
					uint size = this.GetSizeValue(offset);
					e.ArrayLength = (int)(size >> desc.SizeBit);
				}
				// Great, now read the entry's value data
				this.mBuffer.Seek32(offset);
				e.Read(this.mBuffer);

				// Update how much data is still remaining
				uint bytes_read = (uint)(this.mBuffer.BaseStream.Position - offset);
				this.mBufferedDataRemaining -= bytes_read;

				if (this.mBufferedDataRemaining == 0)
					this.DisposeBuffer();

				this.mEntries.Add(offset, e);
			}

			return e;
		}
		#endregion

		public void Write(IO.EndianWriter s)
		{
			foreach (var e in this.mEntries.Values)
				e.Write(s);
		}
	};
}
