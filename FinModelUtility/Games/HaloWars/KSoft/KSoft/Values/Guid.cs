using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif
using Interop = System.Runtime.InteropServices;

namespace KSoft.Values
{
	// http://www.ietf.org/rfc/rfc4122.txt
	// useful reference: http://grepcode.com/file/repository.grepcode.com/java/root/jdk/openjdk/6-b14/java/util/UUID.java

	public enum UuidVersion
	{
		TimeBased,
		/// <summary>DCE Security, with embedded POSIX UIDs</summary>
		DCE,
		/// <summary>Name-based, with MD5</summary>
		NameBasedMd5,
		/// <summary>(Pseudo-)Randomly generated</summary>
		Random,
		/// <summary>Name-based, with SHA1</summary>
		NameBasedSha1,

		/// <remarks>4 bits</remarks>
		[Obsolete(EnumBitEncoderBase.kObsoleteMsg, true)] kNumberOf,
	};

	public enum UuidVariant
	{
		/// <summary>Network Computing System backward compatibility</summary>
		NCS,
		/// <summary>Leach-Salz</summary>
		Standard,
		/// <summary>GUID; Microsoft Component Object Model backward compatibility</summary>
		Microsoft,
		/// <summary>Reserved for future definition</summary>
		Reserved,

		/// <remarks>3 bits</remarks>
		[Obsolete(EnumBitEncoderBase.kObsoleteMsg, true)] kNumberOf,
	};

	[Interop.StructLayout(Interop.LayoutKind.Explicit, Size=kSizeOf)]
	[Interop.ComVisible(true)]
	[Serializable]
	[SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes")]
	public struct KGuid
		: IO.IEndianStreamable, IO.IEndianStreamSerializable
		, IComparable, IComparable<KGuid>, IComparable<Guid>
		, System.Collections.IComparer, IComparer<KGuid>
		, IEquatable<KGuid>, IEqualityComparer<KGuid>, IEquatable<Guid>
	{
		#region Constants
		public const int kSizeOf = sizeof(int) + (sizeof(short) * 2) + (sizeof(byte) * 8);

		const int kVersionBitCount = 4;
		const int kVersionBitShift = Bits.kInt16BitCount - kVersionBitCount;

		const int kVariantBitCount = 3;
		const int kVariantBitShift = Bits.kByteBitCount - kVariantBitCount;

		/// <summary>
		/// Guid format is 32 digits: 00000000000000000000000000000000
		/// </summary>
		public const string kFormatNoStyle = "N";
		/// <summary>
		/// Guid format is 32 digits separated by hyphens: 00000000-0000-0000-0000-000000000000
		/// </summary>
		public const string kFormatHyphenated = "D";
		#endregion

		#region Guid Accessors
		// nesting these into a static class makes them run before the struct's static ctor...
		// which, being a value type cctor, may not run when we want it
		/// <summary><see cref="System.Guid"/> internal accessors</summary>
		static class SysGuid
		{
			const string kData1Name = "_a";
			public static readonly Func<Guid, int> GetData1;
			public static readonly Reflection.Util.ValueTypeMemberSetterDelegate<Guid, int> SetData1;

			const string kData2Name = "_b";
			public static readonly Func<Guid, short> GetData2;
			public static readonly Reflection.Util.ValueTypeMemberSetterDelegate<Guid, short> SetData2;

			const string kData3Name = "_c";
			public static readonly Func<Guid, short> GetData3;
			public static readonly Reflection.Util.ValueTypeMemberSetterDelegate<Guid, short> SetData3;

			public static readonly Func<Guid, byte>[] GetData4;
			public static readonly Reflection.Util.ValueTypeMemberSetterDelegate<Guid, byte>[] SetData4;

			[SuppressMessage("Microsoft.Design", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
			static SysGuid()
			{
				GetData1 = Reflection.Util.GenerateMemberGetter			<Guid, int>		(kData1Name);
				SetData1 = Reflection.Util.GenerateValueTypeMemberSetter<Guid, int>		(kData1Name);
				GetData2 = Reflection.Util.GenerateMemberGetter			<Guid, short>	(kData2Name);
				SetData2 = Reflection.Util.GenerateValueTypeMemberSetter<Guid, short>	(kData2Name);
				GetData3 = Reflection.Util.GenerateMemberGetter			<Guid, short>	(kData3Name);
				SetData3 = Reflection.Util.GenerateValueTypeMemberSetter<Guid, short>	(kData3Name);

				string[] kData4Names = { "_d", "_e", "_f", "_g", "_h", "_i", "_j", "_k", };
				GetData4 = new Func<Guid, byte>[kData4Names.Length];
				SetData4 = new Reflection.Util.ValueTypeMemberSetterDelegate<Guid, byte>[kData4Names.Length];

				for (int x = 0; x < kData4Names.Length; x++)
				{
					GetData4[x] = Reflection.Util.GenerateMemberGetter			<Guid, byte>(kData4Names[x]);
					SetData4[x] = Reflection.Util.GenerateValueTypeMemberSetter	<Guid, byte>(kData4Names[x]);
				}
			}
		};

		public int Data1 { get { return SysGuid.GetData1(this.mData); } }
		public int Data2 { get { return SysGuid.GetData2(this.mData); } }
		public int Data3 { get { return SysGuid.GetData3(this.mData); } }
		public long Data4 { get {
			byte	d = SysGuid.GetData4[0](this.mData), e = SysGuid.GetData4[1](this.mData),
					f = SysGuid.GetData4[2](this.mData), g = SysGuid.GetData4[3](this.mData),
					h = SysGuid.GetData4[4](this.mData), i = SysGuid.GetData4[5](this.mData),
					j = SysGuid.GetData4[6](this.mData), k = SysGuid.GetData4[7](this.mData);
			long result;

			result =  d; result <<= Bits.kByteBitCount;
			result |= e; result <<= Bits.kByteBitCount;
			result |= f; result <<= Bits.kByteBitCount;
			result |= g; result <<= Bits.kByteBitCount;
			result |= h; result <<= Bits.kByteBitCount;
			result |= i; result <<= Bits.kByteBitCount;
			result |= j; result <<= Bits.kByteBitCount;
			result = k;

			return result;
		} }
		#endregion

		[Interop.FieldOffset(0)] Guid mData;
		[Interop.FieldOffset(0)] ulong mDataHi;
		[Interop.FieldOffset(8)] ulong mDataLo;

		public Guid ToGuid() => this.mData;

		public long MostSignificantBits { get {
			ulong result = (uint)SysGuid.GetData1(this.mData);
			result <<= Bits.kInt32BitCount;

			result |= (ushort)SysGuid.GetData2(this.mData);
			result <<= Bits.kInt16BitCount;

			result |= (ushort)SysGuid.GetData3(this.mData);

			return (long)result;
		} }
		public long LeastSignificantBits { get {
			long result = this.Data4;

			return result;
		} }

		#region Version and Variant
		public UuidVersion Version { get => (UuidVersion)(SysGuid.GetData3(this.mData) >> kVersionBitShift); }

		public UuidVariant Variant { get {
			int raw = SysGuid.GetData4[0](this.mData) >> kVariantBitShift;

			// Special condition due to the 'type' bits starting in the right-most (ie, MSB) bits,
			// plus for NCS and Standard the lower two bits are documented in RFC as being 'don't care'
			if ((raw >> 2) == 0)
				return UuidVariant.NCS;
			else if ((raw >> 2) == 1)
				return UuidVariant.Standard;
			else if (raw == 6)
				return UuidVariant.Microsoft;
			else // raw == 7
				return UuidVariant.Reserved;
		} }
		#endregion

		#region TimeBased properties
		public long Timestamp { get {
			Contract.Requires<InvalidOperationException>(this.Version == UuidVersion.TimeBased,
				"Tried to get the Timestamp of a non-time-based GUID");

			ulong msb = (ulong) this.MostSignificantBits;
			ulong result = (msb & 0xFFF) << 48;
			result |= ((msb >> 16) & 0xFFFF) << 32;
			result |= msb >> 32;

			return (long)result;
		} }

		public int ClockSequence { get {
			Contract.Requires<InvalidOperationException>(this.Version == UuidVersion.TimeBased,
				"Tried to get the ClockSequence of a non-time-based GUID");

			// NOTE: While the Variant field is 3-bits, both the Java and RFC implementations
			// seem to lob the two MSB off, instead of 0x1F
			int hi = SysGuid.GetData4[0](this.mData) & 0x3F;
			int lo = SysGuid.GetData4[1](this.mData);

			return (hi << 8) | lo;
		} }

		public long Node { get {
			Contract.Requires<InvalidOperationException>(this.Version == UuidVersion.TimeBased,
				"Tried to get the Node of a non-time-based GUID");

			long result = 0;

			for (int x = 2; x < SysGuid.GetData4.Length; x++, result <<= Bits.kByteBitCount)
				result |= SysGuid.GetData4[x](this.mData);

			return result;
		} }
		#endregion

		#region Ctor
		public KGuid(Guid actualGuid)	{
			this.mDataHi= this.mDataLo=0;
			this.mData = actualGuid; }
		public KGuid(byte[] b)			{
			this.mDataHi= this.mDataLo=0;
			this.mData = new Guid(b); }
		public KGuid(string g)			{
			this.mDataHi= this.mDataLo=0;
			this.mData = new Guid(g); }
		public KGuid(long msb, long lsb)
		{
			this.mData = Guid.Empty;
			this.mDataHi = (ulong)msb;
			this.mDataLo = (ulong)lsb;
		}

		public KGuid(int a, short b, short c, byte[] d)
		{
			this.mDataHi= this.mDataLo=0;
			this.mData = new Guid(a,b,c,d);
		}
		public KGuid(int a, short b, short c, byte d, byte e, byte f, byte g, byte h, byte i, byte j, byte k)
		{
			this.mDataHi= this.mDataLo=0;
			this.mData = new Guid(a,b,c,d,e,f,g,h,i,j,k);
		}
		public KGuid(uint a, ushort b, ushort c, byte d, byte e, byte f, byte g, byte h, byte i, byte j, byte k)
		{
			this.mDataHi= this.mDataLo=0;
			this.mData = new Guid(a,b,c,d,e,f,g,h,i,j,k);
		}
		#endregion

		#region ToString
		/// <see cref="Guid.ToString()"/>
		public override string ToString()								=> this.mData.ToString();
		/// <see cref="Guid.ToString(string)"/>
		[SuppressMessage("Microsoft.Design", "CA1305:SpecifyIFormatProvider")]
		public string ToString(string format)							=> this.mData.ToString(format);
		/// <see cref="Guid.ToString(string, IFormatProvider)"/>
		public string ToString(string format, IFormatProvider provider)	=> this.mData.ToString(format, provider);

		/// <summary>
		/// 32 digits: 00000000000000000000000000000000
		/// </summary>
		/// <returns></returns>
		[SuppressMessage("Microsoft.Design", "CA1305:SpecifyIFormatProvider")]
		internal string ToStringNoStyle()								=> this.mData.ToString(kFormatNoStyle);
		/// <summary>
		/// 32 digits separated by hyphens: 00000000-0000-0000-0000-000000000000
		/// </summary>
		/// <returns></returns>
		[SuppressMessage("Microsoft.Design", "CA1305:SpecifyIFormatProvider")]
		internal string ToStringHyphenated()							=> this.mData.ToString(kFormatHyphenated);
		#endregion

		#region IEndianStreamable Members
		public void Read(IO.EndianReader s)
		{
			SysGuid.SetData1(ref this.mData, s.ReadInt32());
			SysGuid.SetData2(ref this.mData, s.ReadInt16());
			SysGuid.SetData3(ref this.mData, s.ReadInt16());

			foreach (var data4 in SysGuid.SetData4)
				data4(ref this.mData, s.ReadByte());
		}

		public void Write(IO.EndianWriter s)
		{
			int data1 = SysGuid.GetData1(this.mData);
			short data2 = SysGuid.GetData2(this.mData);
			short data3 = SysGuid.GetData3(this.mData);

			s.Write(data1);
			s.Write(data2);
			s.Write(data3);

			foreach (var data4 in SysGuid.GetData4)
				s.Write(data4(this.mData));
		}

		public void Serialize(IO.EndianStream s)
		{
			if (s.IsReading)
				this.Read(s.Reader);
			else if (s.IsWriting)
				this.Write(s.Writer);
		}
		#endregion

		#region IComparable Members
		public int CompareTo(object obj)
		{
			if (obj == null)
				return 1;
			else if (obj is KGuid)
				return this.CompareTo((KGuid)obj);
			else if (obj is Guid)
				return this.CompareTo((Guid)obj);

			throw new InvalidCastException(obj.GetType().ToString());
		}
		public int CompareTo(KGuid other)	=> this.mData.CompareTo(other.mData);
		public int CompareTo(Guid other)	=> this.mData.CompareTo(other);
		#endregion

		#region IEquatable Members
		public override bool Equals(object obj)
		{
			if (obj is KGuid kg)
				return this.Equals(this, kg);
			else if (obj is Guid g)
				return this.Equals(g);

			return false;
		}

		public bool Equals(KGuid x, KGuid y)
		{
			// We don't compare using mData. System.Guid's Equals implementation compares each individual A...K field

			return
				x.mDataHi == y.mDataHi &&
				x.mDataLo == y.mDataLo;
		}
		public bool Equals(KGuid other)		=> this.Equals(this, other);
		public bool Equals(Guid other)		=> this.mData == other;

		public override int GetHashCode()	=> this.mData.GetHashCode();
		public int GetHashCode(KGuid obj)	=> obj.GetHashCode();

		public static bool operator ==(KGuid a, KGuid b)
		{
			// We don't compare using mData. System.Guid's Equals implementation compares each individual A...K field

			return
				a.mDataHi == b.mDataHi &&
				a.mDataLo == b.mDataLo;
		}
		public static bool operator !=(KGuid a, KGuid b)
		{
			return !(a == b);
		}
		#endregion

		#region IComparer<KGuid> Members
		int System.Collections.IComparer.Compare(object x, object y)
		{
			if (x == y)
				return 0;
			if (x == null)
				return -1;
			if (y == null)
				return 1;

			if (x is KGuid)
			{
				if (y is KGuid)
					return ((KGuid)x).CompareTo((KGuid)y);
				if (y is Guid)
					return ((KGuid)x).CompareTo((Guid)y);
			}
			else if (x is Guid && y is KGuid)
				return -((KGuid)y).CompareTo((Guid)x);

			throw new InvalidCastException(x.GetType().ToString());
		}

		public int Compare(KGuid x, KGuid y) => x.CompareTo(y);
		#endregion

		public static KGuid Empty { get => new KGuid(); }
		public bool IsEmpty { get => this.mDataHi == 0 && this.mDataLo == 0; }
		public bool IsNotEmpty { get => this.mDataHi != 0 || this.mDataLo != 0; }

		public static KGuid NewGuid() => new KGuid(Guid.NewGuid());

		#region Parse
		public static KGuid Parse(string input) => new KGuid(Guid.Parse(input));
		public static KGuid ParseExact(string input, string format) => new KGuid(Guid.ParseExact(input, format));
		/// <summary>
		/// 32 digits: 00000000000000000000000000000000
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		internal static KGuid ParseExactNoStyle(string input) => new KGuid(Guid.ParseExact(input, kFormatNoStyle));
		/// <summary>
		/// 32 digits separated by hyphens: 00000000-0000-0000-0000-000000000000
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		internal static KGuid ParseExactHyphenated(string input) => new KGuid(Guid.ParseExact(input, kFormatHyphenated));

		public static bool TryParse(string input, out KGuid result)
		{
			if (Guid.TryParse(input, out Guid guid))
			{
				result = new KGuid(guid);
				return true;
			}

			result = Empty;
			return false;
		}
		public static bool TryParseExact(string input, string format, out KGuid result)
		{
			if (Guid.TryParseExact(input, format, out Guid guid))
			{
				result = new KGuid(guid);
				return true;
			}

			result = Empty;
			return false;
		}
		internal static bool TryParseExactNoStyle(string input, out KGuid result) => TryParseExact(input, kFormatNoStyle, out result);
		public static bool TryParseExactHyphenated(string input, out KGuid result) => TryParseExact(input, kFormatHyphenated, out result);
		#endregion

		#region Byte Utils
		public byte[] ToByteArray() => this.mData.ToByteArray();

		public void ToByteBuffer(byte[] buffer, int index = 0)
		{
			Contract.Requires<ArgumentNullException>(buffer != null);
			Contract.Requires<ArgumentOutOfRangeException>(index >= 0);
			Contract.Requires<ArgumentOutOfRangeException>((index+kSizeOf) <= buffer.Length);

			Bitwise.ByteSwap.ReplaceBytes(buffer, index, SysGuid.GetData1(this.mData)); index += sizeof(int);
			Bitwise.ByteSwap.ReplaceBytes(buffer, index, SysGuid.GetData2(this.mData)); index += sizeof(short);
			Bitwise.ByteSwap.ReplaceBytes(buffer, index, SysGuid.GetData3(this.mData)); index += sizeof(short);
			for (int x = 0; x < 8; x++, index++)
				buffer[x] = SysGuid.GetData4[x](this.mData);
		}
		#endregion
	};
}
