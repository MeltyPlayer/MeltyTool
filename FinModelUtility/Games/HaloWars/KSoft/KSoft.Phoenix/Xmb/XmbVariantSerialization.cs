using System;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Phoenix.Xmb
{
	static class XmbVariantSerialization
	{
		const int kInfoBitIndex = 24;
		public const uint kInfoBitMask = 0xFF000000;
		public const uint kValueBitMask = 0x00FFFFFF;

		#region Type coding
		public enum RawVariantType : byte
		{
			Null			= 0, // i.e., empty string
			Single24		= 1, // S1E6M17
			Single			= 2, // Indirect
			Int24			= 3,
			Int				= 4, // Indirect
			FixedPoint		= 5,
			Double			= 6,
			Bool			= 7,
			StringAnsi		= 8, // if string is 3 characters or less, it gets put in the data field
			StringUnicode	= 9,
			Vector			= 10,
		};

		const int kInfoTypeBitIndex = kInfoBitIndex + 0;
		const int kInfoTypeBitCount = 4;
		const uint kInfoTypeBitMask = 0x0F000000;
		static RawVariantType GetType(uint data)
		{
			data &= kInfoTypeBitMask;
			data >>= kInfoTypeBitIndex;

			return (RawVariantType)data;
		}
		static void SetType(RawVariantType type, ref uint data)
		{
			uint t = (uint)type;
			t <<= kInfoTypeBitIndex;
			t &= kInfoTypeBitMask;

			data |= t;
		}

		#region Int24
		public static void Int24FromVariant(XmbVariant v, out uint data)
		{
			data = Bitwise.Int24.GetNumber(v.Int);

			if (!v.IsUnsigned)
				data = Bitwise.Int24.SetSigned(data, true);
		}
		public static void Int24ToVariant(ref XmbVariant v, RawVariantFlags f, uint data)
		{
			v.Type = XmbVariantType.Int;
			v.IsUnsigned = (f & RawVariantFlags.Unsigned) != 0;

			if (!v.IsUnsigned && Bitwise.Int24.IsSigned(data))
				data |= 0xFF000000;

			v.Int = data;
		}
		#endregion

		static void StringFromVariant(XmbVariant v, ref uint data)
		{
			if (v.IsIndirect)
			{
				data |= v.Offset & kValueBitMask;
			}
			else if (v.IsUnicode)
			{
				throw new System.IO.InvalidDataException("Unicode should always be indirect");//data |= (ushort)v.Char0;
			}
			else
			{
				data |= (uint)(v.Char0 << 0);
				data |= (uint)(v.Char1 << 8);
				data |= (uint)(v.Char2 << 16);
			}
		}
		static void StringToVariant(ref XmbVariant v, uint data)
		{
			if (v.IsIndirect)
			{
				v.Offset = data;
			}
			else if (v.IsUnicode)
			{
				throw new System.IO.InvalidDataException("Unicode should always be indirect");//v.Char0 = (char)data;
			}
			else
			{
				v.Char0 = (byte)((data >> 0)  & 0xFF);
				v.Char1 = (byte)((data >> 8)  & 0xFF);
				v.Char2 = (byte)((data >> 16) & 0xFF);
			}
		}
		#endregion

		#region Length coding
		enum RawVariantLength : byte
		{
			_1D = 0,
			_2D = 1,
			_3D = 2,
			_4D = 3,
		};

		const int kInfoLengthBitIndex = kInfoTypeBitIndex + kInfoTypeBitCount;
		const int kInfoLengthBitCount = 2;
		const uint kInfoLengthBitMask = 0x30000000;
		static RawVariantLength GetLength(uint data)
		{
			data &= kInfoLengthBitMask;
			data >>= kInfoLengthBitIndex;

			return (RawVariantLength)data;
		}
		static void SetLength(RawVariantLength length, ref uint data)
		{
			uint l = (uint)length;
			l <<= kInfoLengthBitIndex;
			l &= kInfoLengthBitMask;

			data |= l;
		}

		static RawVariantLength RawLengthFromByte(byte length)
		{
			Contract.Requires(length > 0 && length <= 4);

			RawVariantLength l = (RawVariantLength)(--length);

			return l;
		}
		static byte RawLengthToByte(RawVariantLength length)
		{
			const byte k_rebase = 1;

			byte l = (byte)length;
			l += k_rebase;

			return l;
		}
		#endregion

		#region Flags coding
		[Flags]
		public enum RawVariantFlags : byte
		{
			Unsigned = 1 << 0,
			Offset = 1 << 1,
		};

		const int kInfoFlagsBitIndex = kInfoLengthBitIndex + kInfoLengthBitCount;
	const uint kInfoFlagsBitMask = 0xC0000000;
		static RawVariantFlags GetFlags(uint data)
		{
			data &= kInfoFlagsBitMask;
			data >>= kInfoFlagsBitIndex;

			return (RawVariantFlags)data;
		}
		static void SetFlags(RawVariantFlags flags, ref uint data)
		{
			uint f = (uint)flags;
			f <<= kInfoTypeBitIndex;
			f &= kInfoTypeBitMask;

			data |= f;
		}
		#endregion

		#region RequiresIndirectStorage
		public static bool SingleRequiresIndirectStorage(float single)
		{
			return !SingleFixedPoint.InRange(single) && !Bitwise.Single24.InRange(single);
		}
		public static bool IntRequiresIndirectStorage(uint value)
		{
			return !Bitwise.Int24.InRange(value);
		}
		public static bool StringRequiresIndirectStorage(string s, bool isUnicode)
		{
			return s.Length > 3 || isUnicode;
		}
		#endregion

		#region Read
		static void Compose(out XmbVariant v, uint data)
		{
			v = XmbVariant.Empty;

			RawVariantType type = GetType(data);
			RawVariantLength length = GetLength(data);
			RawVariantFlags flags = GetFlags(data);
			// Get the actual data value
			data &= kValueBitMask;

			switch (type)
			{
				#region Single
				case RawVariantType.Single24:
					v.Type = XmbVariantType.Single;
					v.Single = Bitwise.Single24.ToSingle(data);
					break;
				case RawVariantType.Single:
					v.Type = XmbVariantType.Single;
					v.IsIndirect = (flags & RawVariantFlags.Offset) != 0; // should always be true
					v.Offset = data;
					break;

				case RawVariantType.FixedPoint:
					v.Type = XmbVariantType.Single;
					v.Single = SingleFixedPoint.ToSingle(data);
					break;
				#endregion

				#region Int
				case RawVariantType.Int24:
					Int24ToVariant(ref v, flags, data);
					break;
				case RawVariantType.Int:
					v.Type = XmbVariantType.Int;
					v.IsUnsigned = (flags & RawVariantFlags.Unsigned) != 0;
					v.IsIndirect = (flags & RawVariantFlags.Offset) != 0; // should always be true
					v.Offset = data;
					break;
				#endregion

				#region Double
				case RawVariantType.Double:
					v.Type = XmbVariantType.Double;
					v.IsIndirect = (flags & RawVariantFlags.Offset) != 0; // should always be true
					v.Offset = data;
					break;
				#endregion

				#region Bool
				case RawVariantType.Bool:
					v.Type = XmbVariantType.Bool;
					v.Bool = data != 0;
					break;
				#endregion

				#region String
				case RawVariantType.StringAnsi:
					v.Type = XmbVariantType.String;
					v.IsIndirect = (flags & RawVariantFlags.Offset) != 0;
					v.IsUnicode = false;
					break;
				case RawVariantType.StringUnicode:
					v.Type = XmbVariantType.String;
					v.IsIndirect = (flags & RawVariantFlags.Offset) != 0;
					v.IsUnicode = true;
					break;
				#endregion

				#region Vector
				case RawVariantType.Vector:
					v.Type = XmbVariantType.Vector;
					v.VectorLength = RawLengthToByte(length);
					v.IsIndirect = (flags & RawVariantFlags.Offset) != 0; // should always be true
					v.Offset = data;
					break;
				#endregion
			}

			if (v.Type == XmbVariantType.String)
				StringToVariant(ref v, data);
		}
		public static void Read(IO.EndianReader s, out XmbVariant v)
		{
			uint data = s.ReadUInt32();

			Compose(out v, data);
		}
		#endregion

		#region Write
		static void DecomposeSingle(XmbVariant v, out RawVariantType t, out uint data)
		{
			t = RawVariantType.Single;
			float single = v.Single;

			if (SingleFixedPoint.InRange(single))
			{
				t = RawVariantType.FixedPoint;
				data = SingleFixedPoint.FromSingle(single);
			}
			else if (Bitwise.Single24.InRange(single))
			{
				t = RawVariantType.Single24;
				data = Bitwise.Single24.FromSingle(single);
			}
			else
				data = v.Offset;
		}
		static void DecomposeInt(XmbVariant v, out RawVariantType t, ref RawVariantFlags f, out uint data)
		{
			t = RawVariantType.Int;
			if (v.IsUnsigned) f |= RawVariantFlags.Unsigned;
			data = v.Int;

			if (Bitwise.Int24.InRange(v.Int))
			{
				t = RawVariantType.Int24;
				Int24FromVariant(v, out data);
			}
		}
		static void Decompose(XmbVariant v,
			out RawVariantType t, out RawVariantLength l, out RawVariantFlags f, out uint data)
		{
			t = RawVariantType.Null;
			l = (RawVariantLength)byte.MinValue;
			f = (RawVariantFlags)byte.MinValue;
			data = 0;

			bool is_indirect = v.IsIndirect;
			bool is_unsigned = v.Type == XmbVariantType.Int && v.IsUnsigned;

			switch (v.Type)
			{
				case XmbVariantType.Single:
					DecomposeSingle(v, out t, out data);
					break;

				case XmbVariantType.Int:
					DecomposeInt(v, out t, ref f, out data);
					break;

				case XmbVariantType.Double: // double is always indirect
					t = RawVariantType.Double;
					break;

				case XmbVariantType.Bool:
					t = RawVariantType.Bool;
					data = v.Bool ? 1U : 0U;
					break;

				case XmbVariantType.String:
					t = v.IsUnicode ? RawVariantType.StringUnicode : RawVariantType.StringAnsi;
					StringFromVariant(v, ref data);
					break;

				case XmbVariantType.Vector: // Vector is always indirect
					t = RawVariantType.Vector;
					l = RawLengthFromByte(v.VectorLength);
					break;
			}

			if (is_indirect)
				data = v.Offset & kValueBitMask;
		}
		public static void Write(IO.EndianWriter s, XmbVariant v)
		{
			uint data = 0;

			RawVariantType t;
			RawVariantLength l;
			RawVariantFlags f;
			Decompose(v, out t, out l, out f, out data);

			SetType(t, ref data);
			SetLength(l, ref data);
			SetFlags(f, ref data);

			s.Write(data);
		}
		#endregion
	};
}