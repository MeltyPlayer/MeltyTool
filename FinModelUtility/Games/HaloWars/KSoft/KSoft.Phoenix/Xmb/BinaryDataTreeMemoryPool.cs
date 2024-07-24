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

		public uint Size { get { return mPoolSize; } }

		IO.EndianReader mBuffer;
		uint mBufferedDataRemaining;

		internal IO.EndianReader InternalBuffer { get { return mBuffer; } }

		public BinaryDataTreeMemoryPool(int initialEntryCount = kEntryStartCount)
		{
			if (initialEntryCount < 0)
				initialEntryCount = kEntryStartCount;

			mEntries = new Dictionary<uint, PoolEntry>(kEntryStartCount);
			mDataOffsetToSizeValue = new Dictionary<uint, uint>();
		}
		public BinaryDataTreeMemoryPool(byte[] buffer, Shell.EndianFormat byteOrder = Shell.EndianFormat.Big)
			: this()
		{
			Contract.Requires(buffer != null);

			mPoolSize = (uint)buffer.Length;
			var ms = new System.IO.MemoryStream(buffer, false);
			mBuffer = new IO.EndianReader(ms, byteOrder, this);
			mBufferedDataRemaining = mPoolSize;
		}

		#region IDisposable Members
		void DisposeBuffer()
		{
			Util.DisposeAndNull(ref mBuffer);
		}
		public void Dispose()
		{
			DisposeBuffer();

			if (mEntries != null)
			{
				mEntries.Clear();
				mEntries = null;
			}
		}
		#endregion

		#region Add
		#endregion

		#region Get
		bool ValidOffset(uint offset) { return offset < mPoolSize; }

		public uint GetSizeValue(uint dataOffset)
		{
			if (!ValidOffset(dataOffset))
				throw new ArgumentOutOfRangeException("dataOffset", string.Format("{0} > {1}",
					dataOffset.ToString("X8"), mPoolSize.ToString("X6")));
			if (dataOffset < sizeof(uint))
				throw new ArgumentOutOfRangeException("dataOffset", "Offset doesn't have room for a size value");

			uint size_value;
			if (!mDataOffsetToSizeValue.TryGetValue(dataOffset, out size_value))
			{
				if (mBufferedDataRemaining == 0)
					throw new InvalidOperationException("No data left in buffer");
				else if (mBuffer == null)
					throw new InvalidOperationException("No underlying buffer");

				uint size_offset = dataOffset - sizeof(uint);
				// Great, now read the entry's value data
				mBuffer.Seek32(size_offset);
				size_value = mBuffer.ReadUInt32();

				// Update how much data is still remaining
				mBufferedDataRemaining -= sizeof(uint);

				if (mBufferedDataRemaining == 0)
					DisposeBuffer();

				mDataOffsetToSizeValue.Add(dataOffset, size_value);
			}

			return size_value;
		}

		internal PoolEntry DeBuffer(BinaryDataTreeNameValue nameValue)
		{
			var type_desc = nameValue.GuessTypeDesc();
			uint offset = nameValue.Offset;
			bool size_is_indirect = nameValue.SizeIsIndirect;
			return DeBuffer(type_desc, offset, size_is_indirect);
		}

		PoolEntry DeBuffer(BinaryDataTreeVariantTypeDesc desc, uint offset, bool sizeIsIndirect = false)
		{
			if (!ValidOffset(offset))
				throw new ArgumentOutOfRangeException("offset", string.Format("{0} > {1}",
					offset.ToString("X8"), mPoolSize.ToString("X6")));

			PoolEntry e;
			if (!mEntries.TryGetValue(offset, out e))
			{
					 if (mBufferedDataRemaining == 0)	throw new InvalidOperationException("No data left in buffer");
				else if (mBuffer == null)				throw new InvalidOperationException("No underlying buffer");

				// Create our new entry, setting any additional properties
				e = PoolEntry.New(desc);
				if (sizeIsIndirect)
				{
					uint size = GetSizeValue(offset);
					e.ArrayLength = (int)(size >> desc.SizeBit);
				}
				// Great, now read the entry's value data
				mBuffer.Seek32(offset);
				e.Read(mBuffer);

				// Update how much data is still remaining
				uint bytes_read = (uint)(mBuffer.BaseStream.Position - offset);
				mBufferedDataRemaining -= bytes_read;

				if (mBufferedDataRemaining == 0)
					DisposeBuffer();

				mEntries.Add(offset, e);
			}

			return e;
		}
		#endregion

		public void Write(IO.EndianWriter s)
		{
			foreach (var e in mEntries.Values)
				e.Write(s);
		}
	};
}
