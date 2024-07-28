using System;
using System.Collections.Generic;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

using Vector2f = System.Numerics.Vector2;
using Vector3f = System.Numerics.Vector3;
using Vector4f = System.Numerics.Vector4;

namespace KSoft.Phoenix.Xmb
{
	sealed partial class XmbVariantMemoryPool
		: IDisposable
	{
		/// <summary>Default amount of entry memory allocated for use</summary>
		const int kEntryStartCount = 16;

		Dictionary<uint, PoolEntry> mEntries;
		uint mPoolSize;

		public uint Size { get { return this.mPoolSize; } }

		IO.EndianReader mBuffer;
		uint mBufferedDataRemaining;

		public XmbVariantMemoryPool(int initialEntryCount = kEntryStartCount)
		{
			if (initialEntryCount < 0)
				initialEntryCount = kEntryStartCount;

			this.mEntries = new Dictionary<uint, PoolEntry>(kEntryStartCount);
		}
		public XmbVariantMemoryPool(byte[] buffer, Shell.EndianFormat byteOrder = Shell.EndianFormat.Big)
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
		uint Add(XmbFileBuilder builder, PoolEntry e)
		{
			uint size = e.CalculateSize();

			uint offset = this.mPoolSize += size;
			// In case the entry needs to be aligned
			this.mPoolSize += e.CalculatePadding(offset);

			this.mEntries.Add(offset, e);
			return offset;
		}

		public uint Add(XmbFileBuilder builder, int v)
		{
			foreach (var kv in this.mEntries)
				if (kv.Value.Equals(v))
					return kv.Key;

			var entry = PoolEntry.New(v);
			return this.Add(builder, entry);
		}
		public uint Add(XmbFileBuilder builder, uint v)
		{
			foreach (var kv in this.mEntries)
				if (kv.Value.Equals(v))
					return kv.Key;

			var entry = PoolEntry.New(v);
			return this.Add(builder, entry);
		}
		public uint Add(XmbFileBuilder builder, float v)
		{
			foreach (var kv in this.mEntries)
				if (kv.Value.Equals(v))
					return kv.Key;

			var entry = PoolEntry.New(v);
			return this.Add(builder, entry);
		}
		public uint Add(XmbFileBuilder builder, double v)
		{
			foreach (var kv in this.mEntries)
				if (kv.Value.Equals(v))
					return kv.Key;

			var entry = PoolEntry.New(v);
			return this.Add(builder, entry);
		}
		public uint Add(XmbFileBuilder builder, string v, bool isUnicode = false)
		{
			foreach (var kv in this.mEntries)
				if (kv.Value.Equals(v))
					return kv.Key;

			var entry = PoolEntry.New(v);
			entry.IsUnicode = isUnicode;
			return this.Add(builder, entry);
		}
		public uint Add(XmbFileBuilder builder, Vector2f v)
		{
			foreach (var kv in this.mEntries)
				if (kv.Value.Equals(v))
					return kv.Key;

			var entry = PoolEntry.New(v);
			return this.Add(builder, entry);
		}
		public uint Add(XmbFileBuilder builder, Vector3f v)
		{
			foreach (var kv in this.mEntries)
				if (kv.Value.Equals(v))
					return kv.Key;

			var entry = PoolEntry.New(v);
			return this.Add(builder, entry);
		}
		public uint Add(XmbFileBuilder builder, Vector4f v)
		{
			foreach (var kv in this.mEntries)
				if (kv.Value.Equals(v))
					return kv.Key;

			var entry = PoolEntry.New(v);
			return this.Add(builder, entry);
		}
		#endregion

		#region Get
		bool ValidOffset(uint offset)	{ return offset < this.mPoolSize; }

		PoolEntry DeBuffer(XmbVariantType type, uint offset, byte flags = 0)
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
				e = PoolEntry.New(type);
					 if (type == XmbVariantType.String) e.IsUnicode = flags != 0;
				else if (type == XmbVariantType.Vector) e.VectorLength = flags;
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

		public int GetInt32(uint offset)
		{
			var e = this.DeBuffer(XmbVariantType.Int, offset);

			return (int)e.Int;
		}
		public uint GetUInt32(uint offset)
		{
			var e = this.DeBuffer(XmbVariantType.Int, offset);

			return e.Int;
		}
		public float GetSingle(uint offset)
		{
			var e = this.DeBuffer(XmbVariantType.Single, offset);

			return e.Single;
		}
		public double GetDouble(uint offset)
		{
			var e = this.DeBuffer(XmbVariantType.Double, offset);

			return e.Double;
		}
		public string GetString(uint offset, bool isUnicode)
		{
			var e = this.DeBuffer(XmbVariantType.String, offset, (byte)(isUnicode ? 1 : 0));

			return e.String;
		}
		public Vector2f GetVector2D(uint offset)
		{
			var e = this.DeBuffer(XmbVariantType.Vector, offset, 2);

			return e.Vector2d;
		}
		public Vector3f GetVector3D(uint offset)
		{
			var e = this.DeBuffer(XmbVariantType.Vector, offset, 3);

			return e.Vector3d;
		}
		public Vector4f GetVector4D(uint offset)
		{
			var e = this.DeBuffer(XmbVariantType.Vector, offset, 4);

			return e.Vector4d;
		}
		#endregion

		public void Write(IO.EndianWriter s)
		{
			foreach (var e in this.mEntries.Values)
				e.Write(s);
		}

		public static bool IsInt32(string str, ref int value, bool useInt24)
		{
			if (str == "0")
			{
				value = 0;
				return true;
			}

			int v;
			try
			{
				v = Convert.ToInt32(str, (int)NumeralBase.Decimal);
			}
			catch (Exception)
			{
				return false;
			}

			// VC++'s strtol returns 0, LONG_MIN, LONG_MAX when a conversion cannot be performed or when there's overflow
			if (v == 0 || v == int.MinValue || v == int.MaxValue)
				return false;

			if (useInt24)
			{
				var int24 = v & XmbVariantSerialization.kValueBitMask;
				if (int24 != v)
					return false;
			}

			var unpackedV = v;
			if ((unpackedV & 0x800000) != 0)
				unpackedV |= unchecked( (int)0xFF000000 );

			if (unpackedV != v)
				return false;

			value = unpackedV;
			return true;
		}

		public static bool IsUInt32(string str, ref uint value, bool useInt24)
		{
			if (str == "0")
			{
				value = 0;
				return true;
			}

			uint v;
			try
			{
				v = Convert.ToUInt32(str, (int)NumeralBase.Decimal);
			}
			catch (Exception)
			{
				return false;
			}

			// VC++'s strtoul returns 0, ULONG_MAX when a conversion cannot be performed or when there's overflow
			if (v == uint.MinValue || v == uint.MaxValue)
				return false;

			if (useInt24)
			{
				var int24 = v & XmbVariantSerialization.kValueBitMask;
				if (int24 != v)
					return false;
			}

			value = v;
			return true;
		}

		public static bool IsFloat(string str, ref uint value, double epsilon, bool useInt24)
		{
			double v;
			try
			{
				v = Convert.ToDouble(str);
			}
			catch (Exception)
			{
				return false;
			}

			// VC++'s strtod returns 0, +HUGE_VAL, -HUGE_VAL when a conversion cannot be performed or when there's overflow
			if (v == 0.0f)
				return false;
			if (double.IsNegativeInfinity(v) || double.IsPositiveInfinity(v))
				return false;

			// #TODO finish this

			return false;
		}
	};
}