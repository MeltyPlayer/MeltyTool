using Interop = System.Runtime.InteropServices;

using Vector2f = System.Numerics.Vector2;
using Vector3f = System.Numerics.Vector3;
using Vector4f = System.Numerics.Vector4;

namespace KSoft.Phoenix.Xmb
{
	partial class XmbVariantMemoryPool
	{
		[Interop.StructLayout(Interop.LayoutKind.Explicit)]
		sealed class PoolEntry
			: IO.IEndianStreamable
		{
			static readonly Text.StringStorageEncoding kAnsiEncoding =
				Text.StringStorageEncoding.TryAndGetStaticEncoding(Memory.Strings.StringStorage.CStringAscii);
			static readonly Text.StringStorageEncoding kUnicodeEncoding =
				Text.StringStorageEncoding.TryAndGetStaticEncoding(Memory.Strings.StringStorage.CStringUnicode);

			[Interop.FieldOffset(0)]
			public uint Int;
			[Interop.FieldOffset(0)]
			public float Single;
			[Interop.FieldOffset(0)]
			public double Double;
			[Interop.FieldOffset(0)]
			public Vector2f Vector2d;
			[Interop.FieldOffset(0)]
			public Vector3f Vector3d;
			[Interop.FieldOffset(0)]
			public Vector4f Vector4d = new Vector4f();

			// we don't know how big a .NET reference really is (we could be compiling for x64!) so always give it 8 bytes
			[Interop.FieldOffset(16)]
			public string String;

			[Interop.FieldOffset(24)]
			public XmbVariantType Type;

			// These are all type dependent so we reuse the memory space
			[Interop.FieldOffset(24 + 1)]
			public byte VectorLength;
//			[System.Runtime.InteropServices.FieldOffset(24 + 1)]
//			public bool IsUnsigned;
			[Interop.FieldOffset(24 + 1)]
			public bool IsUnicode;

			// Amount of padding to prefix this entry with when written
			[Interop.FieldOffset(24 + 2)]
			public byte PrePadSize;

			#region Equals
			public bool Equals(uint v)		{ return this.Type == XmbVariantType.Int && this.Int == v; }
			public bool Equals(int v)		{ return this.Type == XmbVariantType.Int && this.Int == (uint)v; }
			public bool Equals(float v)		{ return this.Type == XmbVariantType.Single && this.Single == v; }
			public bool Equals(double v)	{ return this.Type == XmbVariantType.Double && this.Double == v; }
			public bool Equals(string v)	{ return this.Type == XmbVariantType.String && this.String == v; }
			public bool Equals(Vector2f v)	{ return this.Type == XmbVariantType.Vector && this.VectorLength == 2 && this.Vector2d == v; }
			public bool Equals(Vector3f v)	{ return this.Type == XmbVariantType.Vector && this.VectorLength == 3 && this.Vector3d == v; }
			public bool Equals(Vector4f v)	{ return this.Type == XmbVariantType.Vector && this.VectorLength == 4 && this.Vector4d == v; }
			#endregion
			#region New
			public static PoolEntry New(uint v)		{ return new PoolEntry() { Type = XmbVariantType.Int, Int = v }; }
			public static PoolEntry New(int v)		{ return new PoolEntry() { Type = XmbVariantType.Int, Int = (uint)v }; }
			public static PoolEntry New(float v)	{ return new PoolEntry() { Type = XmbVariantType.Single, Single = v }; }
			public static PoolEntry New(double v)	{ return new PoolEntry() { Type = XmbVariantType.Double, Double = v }; }
			public static PoolEntry New(string v)	{ return new PoolEntry() { Type = XmbVariantType.String, String = v }; }
			public static PoolEntry New(Vector2f v)	{ return new PoolEntry() { Type = XmbVariantType.Vector, VectorLength = 2, Vector2d = v }; }
			public static PoolEntry New(Vector3f v)	{ return new PoolEntry() { Type = XmbVariantType.Vector, VectorLength = 3, Vector3d = v }; }
			public static PoolEntry New(Vector4f v)	{ return new PoolEntry() { Type = XmbVariantType.Vector, VectorLength = 4, Vector4d = v }; }
			public static PoolEntry New(XmbVariantType t)	{ return new PoolEntry() { Type = t }; }
			#endregion

			public uint CalculateSize()
			{
				switch (this.Type)
				{
				case XmbVariantType.Int:
				case XmbVariantType.Single:
					return sizeof(uint);
				case XmbVariantType.Double:
					return sizeof(ulong);
				case XmbVariantType.String:
					var sse = this.IsUnicode ? kUnicodeEncoding : kAnsiEncoding;
					return (uint)sse.GetByteCount(this.String);
				case XmbVariantType.Vector:
					return (uint)(sizeof(uint) * this.VectorLength);

				default: throw new KSoft.Debug.UnreachableException(this.Type.ToString());
				}
			}

			public uint CalculatePadding(uint offset)
			{
				const int kAlignmentBit = IntegerMath.kInt32AlignmentBit;
				this.PrePadSize = 0;

				if (this.Type != XmbVariantType.String)
					this.PrePadSize = (byte)IntegerMath.PaddingRequired(kAlignmentBit, offset);

				return this.PrePadSize;
			}

			#region IEndianStreamable Members
			public void Read(IO.EndianReader s)
			{
				switch (this.Type)
				{
				case XmbVariantType.Int:    s.Read(out this.Int); break;
				case XmbVariantType.Single: s.Read(out this.Single); break;
				case XmbVariantType.Double: s.Read(out this.Double); break;
				case XmbVariantType.String:
					this.String = s.ReadString(this.IsUnicode ? kUnicodeEncoding : kAnsiEncoding);
					break;
				case XmbVariantType.Vector:
					if (this.VectorLength >= 1) s.Read(out this.Vector4d.X);
					if (this.VectorLength >= 2) s.Read(out this.Vector4d.Y);
					if (this.VectorLength >= 3) s.Read(out this.Vector4d.Z);
					if (this.VectorLength >= 4) s.Read(out this.Vector4d.W);
					break;

				default: throw new KSoft.Debug.UnreachableException(this.Type.ToString());
				}
			}

			public void Write(IO.EndianWriter s)
			{
				if (this.PrePadSize > 0)
					for (int x = 0; x < this.PrePadSize; x++)
						s.Write(byte.MinValue);

				switch (this.Type)
				{
				case XmbVariantType.Int:    s.Write(this.Int); break;
				case XmbVariantType.Single: s.Write(this.Single); break;
				case XmbVariantType.Double: s.Write(this.Double); break;
				case XmbVariantType.String:
					s.Write(this.String, this.IsUnicode ? kUnicodeEncoding : kAnsiEncoding);
					break;
				case XmbVariantType.Vector:
					if (this.VectorLength >= 1) s.Write(this.Vector4d.X);
					if (this.VectorLength >= 2) s.Write(this.Vector4d.Y);
					if (this.VectorLength >= 3) s.Write(this.Vector4d.Z);
					if (this.VectorLength >= 4) s.Write(this.Vector4d.W);
					break;

				default: throw new KSoft.Debug.UnreachableException(this.Type.ToString());
				}
			}
			#endregion
		};
	};
}