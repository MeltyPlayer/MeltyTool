using System;
using System.Security.Cryptography;
using Interop = System.Runtime.InteropServices;

namespace KSoft.Phoenix
{
	using PhxHash = Security.Cryptography.PhxHash;

	[Interop.StructLayout(Interop.LayoutKind.Explicit, Size=kSizeOf)]
	public struct PhxSYSTEMTIME
		: IO.IEndianStreamSerializable
	{
		public const int kSizeOf = sizeof(ulong);
		public static PhxSYSTEMTIME MinValue { get { return new PhxSYSTEMTIME(1601, 1, 1); } }
		public static PhxSYSTEMTIME MaxValue { get { return new PhxSYSTEMTIME(30827, 12, 31, 23, 59, 59, 999); } }

		[Interop.FieldOffset(0)] public ulong Bits;

		[Interop.FieldOffset( 0)] public ushort Year;
		[Interop.FieldOffset( 2)] public ushort Month;
		[Interop.FieldOffset( 4)] public ushort DayOfWeek;
		[Interop.FieldOffset( 6)] public ushort Day;
		[Interop.FieldOffset( 8)] public ushort Hour;
		[Interop.FieldOffset(10)] public ushort Minute;
		[Interop.FieldOffset(12)] public ushort Second;
		[Interop.FieldOffset(14)] public ushort Milliseconds;

		public PhxSYSTEMTIME(ulong bits) : this()
		{
			this.Bits = bits;
		}

		public PhxSYSTEMTIME(ushort year, ushort month, ushort day
			, ushort hour = 0, ushort minute = 0, ushort second = 0, ushort ms = 0)
		{
			this.Bits = 0;
			this.Year = year;
			this.Month = month;
			this.Day = day;
			this.Hour = hour;
			this.Minute = minute;
			this.Second = second;
			this.Milliseconds = ms;
			this.DayOfWeek = 0;
		}
		public PhxSYSTEMTIME(DateTime dt)
		{
			this.Bits = 0;
			dt = dt.ToUniversalTime();
			this.Year = (ushort)(dt.Year);
			this.Month = (ushort)(dt.Month);
			this.DayOfWeek = (ushort)(dt.DayOfWeek);
			this.Day = (ushort)(dt.Day);
			this.Hour = (ushort)(dt.Hour);
			this.Minute = (ushort)(dt.Minute);
			this.Second = (ushort)(dt.Second);
			this.Milliseconds = (ushort)(dt.Millisecond);
		}

		public void UpdateHash(SHA1CryptoServiceProvider sha)
		{
			PhxHash.UInt16(sha, (uint) this.Year);
			PhxHash.UInt16(sha, (uint) this.Month);
			PhxHash.UInt16(sha, (uint) this.DayOfWeek);
			PhxHash.UInt16(sha, (uint) this.Day);
			PhxHash.UInt16(sha, (uint) this.Hour);
			PhxHash.UInt16(sha, (uint) this.Minute);
			PhxHash.UInt16(sha, (uint) this.Second);
			PhxHash.UInt16(sha, (uint) this.Milliseconds);
		}

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref this.Year);
			s.Stream(ref this.Month);
			s.Stream(ref this.DayOfWeek);
			s.Stream(ref this.Day);
			s.Stream(ref this.Hour);
			s.Stream(ref this.Minute);
			s.Stream(ref this.Second);
			s.Stream(ref this.Milliseconds);
		}
		#endregion

		public override bool Equals(object obj)
		{
			if (obj is PhxSYSTEMTIME)
				return ((PhxSYSTEMTIME)obj) == this;

			return base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public static bool operator ==(PhxSYSTEMTIME s1, PhxSYSTEMTIME s2)
		{
#if false
			return s1.Year == s2.Year
				&& s1.Month == s2.Month
				&& s1.Day == s2.Day
				&& s1.Hour == s2.Hour
				&& s1.Minute == s2.Minute
				&& s1.Second == s2.Second
				&& s1.Milliseconds == s2.Milliseconds;
#else
			return s1.Bits == s2.Bits;
#endif
		}

		public static bool operator !=(PhxSYSTEMTIME s1, PhxSYSTEMTIME s2)
		{
			return !(s1 == s2);
		}

		public DateTime ToDateTime()
		{
			if (this.Year == 0 || this == MinValue)
				return DateTime.MinValue;
			if (this == MaxValue)
				return DateTime.MaxValue;

			return new DateTime(this.Year,
			                    this.Month,
			                    this.Day,
			                    this.Hour,
			                    this.Minute,
			                    this.Second,
			                    this.Milliseconds,
				DateTimeKind.Unspecified);
		}

		public DateTime ToLocalTime()
		{
			if (this.Year == 0 || this == MinValue)
				return DateTime.MinValue;
			if (this == MaxValue)
				return DateTime.MaxValue;

			return new DateTime(this.Year,
			                    this.Month,
			                    this.Day,
			                    this.Hour,
			                    this.Minute,
			                    this.Second,
			                    this.Milliseconds,
				DateTimeKind.Local);
		}

		public DateTime ToUniversalTime()
		{
			if (this.Year == 0 || this == MinValue)
				return DateTime.MinValue;
			if (this == MaxValue)
				return DateTime.MaxValue;

			return new DateTime(this.Year,
			                    this.Month,
			                    this.Day,
			                    this.Hour,
			                    this.Minute,
			                    this.Second,
			                    this.Milliseconds,
				DateTimeKind.Utc);
		}
	};
}