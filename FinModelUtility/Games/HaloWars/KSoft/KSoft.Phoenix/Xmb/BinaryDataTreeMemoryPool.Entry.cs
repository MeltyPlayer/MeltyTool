using Interop = System.Runtime.InteropServices;

namespace KSoft.Phoenix.Xmb
{
	using BDTypeDesc = BinaryDataTreeVariantTypeDesc;

	partial class BinaryDataTreeMemoryPool
	{
		[Interop.StructLayout(Interop.LayoutKind.Explicit)]
		public sealed class PoolEntry
			: IO.IEndianStreamable
		{
			static readonly Text.StringStorageEncoding kAnsiEncoding =
				Text.StringStorageEncoding.TryAndGetStaticEncoding(Memory.Strings.StringStorage.CStringAscii);
			static readonly Text.StringStorageEncoding kUnicodeEncoding =
				Text.StringStorageEncoding.TryAndGetStaticEncoding(Memory.Strings.StringStorage.CStringUnicode);

			[Interop.FieldOffset(0)]
			public uint Int;
			[Interop.FieldOffset(0)]
			public ulong Int64;
			[Interop.FieldOffset(0)]
			public float Single;
			[Interop.FieldOffset(0)]
			public double Double;

			[Interop.FieldOffset(8)]
			public BDTypeDesc TypeDesc;
			[Interop.FieldOffset(12)]
			public int ArrayLength;

			// String must come last, because we don't know how big a .NET reference really is (we could be compiling for x64!)
			[Interop.FieldOffset(16)]
			public string String;
			[Interop.FieldOffset(16)]
			public object OpaqueArrayRef;

			// Amount of padding to prefix this entry with when written
			[Interop.FieldOffset(16 + 8)]
			public byte PrePadSize;

			#region Equals
			public bool Equals(uint v)		{ return this.TypeDesc == BDTypeDesc.UInt32 && this.Int == v; }
			public bool Equals(int v)		{ return this.TypeDesc == BDTypeDesc.Int32  && this.Int == (uint)v; }
#if false
			public bool Equals(float v)		{ return Type == BinaryDataTreeVariantType.Single && Single == v; }
			public bool Equals(double v)	{ return Type == BinaryDataTreeVariantType.Double && Double == v; }
			public bool Equals(string v)	{ return Type == BinaryDataTreeVariantType.String && String == v; }
			public bool Equals(Vector2f v)	{ return Type == BinaryDataTreeVariantType.Vector && VectorLength == 2 && Vector2d == v; }
			public bool Equals(Vector3f v)	{ return Type == BinaryDataTreeVariantType.Vector && VectorLength == 3 && Vector3d == v; }
			public bool Equals(Vector4f v)	{ return Type == BinaryDataTreeVariantType.Vector && VectorLength == 4 && Vector4d == v; }
#endif
			#endregion
			#region New
			public static PoolEntry New(uint v)
			{
				return new PoolEntry()
				{
					TypeDesc = BDTypeDesc.UInt32,
					ArrayLength = 1,
					Int = v,
				};
			}
			public static PoolEntry New(int v)
			{
				return new PoolEntry()
				{
					TypeDesc = BDTypeDesc.UInt32,
					ArrayLength = 1,
					Int = (uint)v,
				};
			}
#if false
			public static PoolEntry New(float v)	{ return new PoolEntry() { Type = BinaryDataTreeVariantType.Single, Single = v }; }
			public static PoolEntry New(double v)	{ return new PoolEntry() { Type = BinaryDataTreeVariantType.Double, Double = v }; }
			public static PoolEntry New(string v)	{ return new PoolEntry() { Type = BinaryDataTreeVariantType.String, String = v }; }
			public static PoolEntry New(Vector2f v)	{ return new PoolEntry() { Type = BinaryDataTreeVariantType.Vector, VectorLength = 2, Vector2d = v }; }
			public static PoolEntry New(Vector3f v)	{ return new PoolEntry() { Type = BinaryDataTreeVariantType.Vector, VectorLength = 3, Vector3d = v }; }
			public static PoolEntry New(Vector4f v)	{ return new PoolEntry() { Type = BinaryDataTreeVariantType.Vector, VectorLength = 4, Vector4d = v }; }
#endif
			public static PoolEntry New(BDTypeDesc desc)
			{
				return new PoolEntry()
				{
					TypeDesc = desc,
					ArrayLength = 1,
				};
			}
			#endregion

			public BinaryDataTreeVariantType Type { get { return this.TypeDesc.Type; } }
			public bool IsUnicode { get { return this.TypeDesc.IsUnicode; } }
			public bool UseDirectEncoding { get { return this.TypeDesc.SizeOf <= sizeof(uint) && this.ArrayLength <= 1; } }

			public uint CalculateSize()
			{
				switch (this.Type)
				{
				case BinaryDataTreeVariantType.Null:
					return 0;

				case BinaryDataTreeVariantType.Bool:
				case BinaryDataTreeVariantType.Int:
				case BinaryDataTreeVariantType.Float:
				case BinaryDataTreeVariantType.String:
					int array_length = this.ArrayLength > 1
						? this.ArrayLength
						: 1;
					int size = this.TypeDesc.SizeOf;
					return (uint)(size * array_length);

				default: throw new KSoft.Debug.UnreachableException(this.Type.ToString());
				}
			}

			public uint CalculatePadding(uint offset, bool willWriteSizeToo)
			{
				this.PrePadSize = 0;

				int alignment_bit = System.Math.Max(this.TypeDesc.AlignmentBit, IntegerMath.kInt32AlignmentBit);

				if (this.Type != BinaryDataTreeVariantType.String)
					this.PrePadSize = (byte)IntegerMath.PaddingRequired(alignment_bit, offset);

				if (willWriteSizeToo)
					this.PrePadSize += sizeof(uint);

				return this.PrePadSize;
			}

			public uint GetSuperFastHashCode()
			{
				var hash = this.TypeDesc.GetSuperFastHashCode();

				var buffer = PhxUtil.GetBufferForSuperFastHash(sizeof(uint));

				Bitwise.ByteSwap.ReplaceBytes(buffer, 0, (uint) this.ArrayLength);
				hash = PhxUtil.SuperFastHash(buffer, 0, sizeof(uint), hash);

				// #TODO hash the value's bytes

				return hash;
			}

			#region IEndianStreamable Members
			public void Read(IO.EndianReader s)
			{
				switch (this.Type)
				{
				case BinaryDataTreeVariantType.Bool:
					this.ReadBool(s); break;
				case BinaryDataTreeVariantType.Int:
					this.ReadInt(s); break;
				case BinaryDataTreeVariantType.Float:
					this.ReadFloat(s); break;
				case BinaryDataTreeVariantType.String:
					this.ReadString(s); break;

				default: throw new KSoft.Debug.UnreachableException(this.Type.ToString());
				}
			}
			void ReadBool(IO.EndianReader s)
			{
				if (this.ArrayLength == 1)
				{
					this.Int = s.ReadBoolean().ToUInt32();
				}
				else
				{
					var array = new bool[this.ArrayLength];
					for (int x = 0; x < array.Length; x++)
						array[x] = s.ReadBoolean();

					this.OpaqueArrayRef = array;
				}
			}
			void ReadInt(IO.EndianReader s)
			{
				switch (this.TypeDesc.SizeOf)
				{
					case sizeof(byte):
					{
						if (this.ArrayLength == 1)
						{
							this.Int = this.TypeDesc.IsUnsigned
								? (uint)s.ReadByte()
								: (uint)s.ReadSByte();
						}
						else
						{
							if (this.TypeDesc.IsUnsigned)
							{
								var array = s.ReadBytes(this.ArrayLength);

								this.OpaqueArrayRef = array;
							}
							else
							{
								var array = new sbyte[this.ArrayLength];
								for (int x = 0; x < array.Length; x++)
									array[x] = s.ReadSByte();

								this.OpaqueArrayRef = array;
							}
						}
					} break;

					case sizeof(ushort):
					{
						if (this.ArrayLength == 1)
						{
							this.Int = this.TypeDesc.IsUnsigned
								? (uint)s.ReadUInt16()
								: (uint)s.ReadInt16();
						}
						else
						{
							if (this.TypeDesc.IsUnsigned)
							{
								var array = new ushort[this.ArrayLength];
								for (int x = 0; x < array.Length; x++)
									array[x] = s.ReadUInt16();

								this.OpaqueArrayRef = array;
							}
							else
							{
								var array = new short[this.ArrayLength];
								for (int x = 0; x < array.Length; x++)
									array[x] = s.ReadInt16();

								this.OpaqueArrayRef = array;
							}
						}
					} break;

					case sizeof(uint):
					{
						if (this.ArrayLength == 1)
						{
							this.Int = this.TypeDesc.IsUnsigned
								? (uint)s.ReadUInt32()
								: (uint)s.ReadInt32();
						}
						else
						{
							if (this.TypeDesc.IsUnsigned)
							{
								var array = new uint[this.ArrayLength];
								for (int x = 0; x < array.Length; x++)
									array[x] = s.ReadUInt32();

								this.OpaqueArrayRef = array;
							}
							else
							{
								var array = new int[this.ArrayLength];
								for (int x = 0; x < array.Length; x++)
									array[x] = s.ReadInt32();

								this.OpaqueArrayRef = array;
							}
						}
					} break;

					case sizeof(ulong):
					{
						if (this.ArrayLength == 1)
						{
							this.Int64 = this.TypeDesc.IsUnsigned
								? (ulong)s.ReadUInt64()
								: (ulong)s.ReadInt64();
						}
						else
						{
							if (this.TypeDesc.IsUnsigned)
							{
								var array = new ulong[this.ArrayLength];
								for (int x = 0; x < array.Length; x++)
									array[x] = s.ReadUInt64();

								this.OpaqueArrayRef = array;
							}
							else
							{
								var array = new long[this.ArrayLength];
								for (int x = 0; x < array.Length; x++)
									array[x] = s.ReadInt64();

								this.OpaqueArrayRef = array;
							}
						}
					} break;

					default:
						throw new KSoft.Debug.UnreachableException(this.TypeDesc.SizeOf.ToString());
				}
			}
			void ReadFloat(IO.EndianReader s)
			{
				switch (this.TypeDesc.SizeOf)
				{
					case sizeof(float):
					{
						if (this.ArrayLength == 1)
						{
							this.Single = s.ReadSingle();
						}
						else
						{
							var array = new float[this.ArrayLength];
							for (int x = 0; x < array.Length; x++)
								array[x] = s.ReadSingle();

							this.OpaqueArrayRef = array;
						}
					} break;

					case sizeof(double):
					{
						if (this.ArrayLength == 1)
						{
							this.Double = s.ReadDouble();
						}
						else
						{
							var array = new double[this.ArrayLength];
							for (int x = 0; x < array.Length; x++)
								array[x] = s.ReadDouble();

							this.OpaqueArrayRef = array;
						}
					} break;

					default:
						throw new KSoft.Debug.UnreachableException(this.TypeDesc.SizeOf.ToString());
				}
			}
			void ReadString(IO.EndianReader s)
			{
				if (this.UseDirectEncoding)
				{

				}
				else
				{
					this.String = s.ReadString(this.IsUnicode ? kUnicodeEncoding : kAnsiEncoding);
				}
			}

			public void Write(IO.EndianWriter s)
			{
				if (this.PrePadSize > 0)
					for (int x = 0; x < this.PrePadSize; x++)
						s.Write(byte.MinValue);

				switch (this.Type)
				{
				case BinaryDataTreeVariantType.Int:	s.Write(this.Int); break;
#if false
				case BinaryDataTreeVariantType.Single: s.Write(Single); break;
				case BinaryDataTreeVariantType.Double: s.Write(Double); break;
				case BinaryDataTreeVariantType.String:
					s.Write(String, IsUnicode ? kUnicodeEncoding : kAnsiEncoding);
					break;
				case BinaryDataTreeVariantType.Vector:
					if (VectorLength >= 1) s.Write(Vector4d.X);
					if (VectorLength >= 2) s.Write(Vector4d.Y);
					if (VectorLength >= 3) s.Write(Vector4d.Z);
					if (VectorLength >= 4) s.Write(Vector4d.W);
					break;
#endif

				default: throw new KSoft.Debug.UnreachableException(this.Type.ToString());
				}
			}
			void WriteBool(IO.EndianWriter s)
			{
			}
			void WriteInt(IO.EndianWriter s)
			{
			}
			void WriteFloat(IO.EndianWriter s)
			{
			}
			void WriteString(IO.EndianWriter s)
			{
			}
			#endregion
		};
	};
}
