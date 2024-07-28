using System.IO;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.IO
{
	partial class VersionMismatchException
	{
		#region UInt32 ctors
		public VersionMismatchException(string dataDescription, uint found)
			: base(string.Format(Util.InvariantCultureInfo, "Invalid '{0}' version '{1}'!", dataDescription, found))
		{
			Contract.Requires(!string.IsNullOrEmpty(dataDescription));
		}
		public VersionMismatchException(string dataDescription, uint expected, uint found)
			: base(string.Format(Util.InvariantCultureInfo, kDescFormat, dataDescription, expected, found, VersionCompareDesc(expected, found)))
		{
			Contract.Requires(!string.IsNullOrEmpty(dataDescription));
		}
		#endregion
		#region Int32 ctors
		public VersionMismatchException(string dataDescription, int found)
			: base(string.Format(Util.InvariantCultureInfo, "Invalid '{0}' version '{1}'!", dataDescription, found))
		{
			Contract.Requires(!string.IsNullOrEmpty(dataDescription));
		}
		public VersionMismatchException(string dataDescription, int expected, int found)
			: base(string.Format(Util.InvariantCultureInfo, kDescFormat, dataDescription, expected, found, VersionCompareDesc(expected, found)))
		{
			Contract.Requires(!string.IsNullOrEmpty(dataDescription));
		}
		#endregion

		#region Stream ctors
		public VersionMismatchException(Stream s, byte expected, byte found) :
			this(s.Position - 1, VersionCompareDesc(expected, found), expected.ToString("X2", Util.InvariantCultureInfo), found.ToString("X2", Util.InvariantCultureInfo))
		{
			Contract.Requires(s != null);
		}

		public VersionMismatchException(Stream s, ushort expected, ushort found) :
			this(s.Position - 2, VersionCompareDesc(expected, found), expected.ToString("X4", Util.InvariantCultureInfo), found.ToString("X4", Util.InvariantCultureInfo))
		{
			Contract.Requires(s != null);
		}

		public VersionMismatchException(Stream s, uint expected, uint found) :
			this(s.Position - 4, VersionCompareDesc(expected, found), expected.ToString("X8", Util.InvariantCultureInfo), found.ToString("X8", Util.InvariantCultureInfo))
		{
			Contract.Requires(s != null);
		}

		public VersionMismatchException(Stream s, ulong expected, ulong found) :
			this(s.Position - 8, VersionCompareDesc(expected, found), expected.ToString("X16", Util.InvariantCultureInfo), found.ToString("X16", Util.InvariantCultureInfo))
		{
			Contract.Requires(s != null);
		}

		#endregion

		#region EndianReader util
		public static void Assert(EndianReader s, byte expected)
		{
			Contract.Requires(s != null);

			var version = s.ReadByte();
			if (version != expected)
				throw new VersionMismatchException(s.BaseStream, expected, version);
		}

		public static void Assert(EndianReader s, ushort expected)
		{
			Contract.Requires(s != null);

			var version = s.ReadUInt16();
			if (version != expected)
				throw new VersionMismatchException(s.BaseStream, expected, version);
		}

		public static void Assert(EndianReader s, uint expected)
		{
			Contract.Requires(s != null);

			var version = s.ReadUInt32();
			if (version != expected)
				throw new VersionMismatchException(s.BaseStream, expected, version);
		}

		public static void Assert(EndianReader s, ulong expected)
		{
			Contract.Requires(s != null);

			var version = s.ReadUInt64();
			if (version != expected)
				throw new VersionMismatchException(s.BaseStream, expected, version);
		}

		#endregion
	};

	partial class VersionOutOfRangeException
	{
		#region UInt32 ctors
		public VersionOutOfRangeException(string dataDescription
			, uint expectedMin
			, uint expectedMax
			, uint found)
			: base(string.Format(Util.InvariantCultureInfo, kDescFormat, dataDescription, expectedMin, expectedMax, found, VersionCompareDesc(expectedMin, expectedMax, found)))
		{
			Contract.Requires(!string.IsNullOrEmpty(dataDescription));
		}
		#endregion
		#region Int32 ctors
		public VersionOutOfRangeException(string dataDescription
			, int expectedMin
			, int expectedMax
			, int found)
			: base(string.Format(Util.InvariantCultureInfo, kDescFormat, dataDescription, expectedMin, expectedMax, found, VersionCompareDesc(expectedMin, expectedMax, found)))
		{
			Contract.Requires(!string.IsNullOrEmpty(dataDescription));
		}
		#endregion

		#region Stream ctors
		public VersionOutOfRangeException(Stream s
			, byte expectedMin
			, byte expectedMax
			, byte found)
			: this(s.Position - 1, VersionCompareDesc(expectedMin, expectedMax, found), expectedMin.ToString("X2", Util.InvariantCultureInfo), expectedMax.ToString("X2", Util.InvariantCultureInfo), found.ToString("X2", Util.InvariantCultureInfo))
		{
			Contract.Requires(s != null);
		}

		public VersionOutOfRangeException(Stream s
			, ushort expectedMin
			, ushort expectedMax
			, ushort found)
			: this(s.Position - 2, VersionCompareDesc(expectedMin, expectedMax, found), expectedMin.ToString("X4", Util.InvariantCultureInfo), expectedMax.ToString("X4", Util.InvariantCultureInfo), found.ToString("X4", Util.InvariantCultureInfo))
		{
			Contract.Requires(s != null);
		}

		public VersionOutOfRangeException(Stream s
			, uint expectedMin
			, uint expectedMax
			, uint found)
			: this(s.Position - 4, VersionCompareDesc(expectedMin, expectedMax, found), expectedMin.ToString("X8", Util.InvariantCultureInfo), expectedMax.ToString("X8", Util.InvariantCultureInfo), found.ToString("X8", Util.InvariantCultureInfo))
		{
			Contract.Requires(s != null);
		}

		public VersionOutOfRangeException(Stream s
			, ulong expectedMin
			, ulong expectedMax
			, ulong found)
			: this(s.Position - 8, VersionCompareDesc(expectedMin, expectedMax, found), expectedMin.ToString("X16", Util.InvariantCultureInfo), expectedMax.ToString("X16", Util.InvariantCultureInfo), found.ToString("X16", Util.InvariantCultureInfo))
		{
			Contract.Requires(s != null);
		}

		#endregion

		#region EndianReader util
		public static byte Assert(EndianReader s
			, byte expectedMin
			, byte expectedMax)
		{
			Contract.Requires(s != null);

			var version = s.ReadByte();
			if (version < expectedMin || version > expectedMax)
				throw new VersionOutOfRangeException(s.BaseStream, expectedMin, expectedMax, version);

			return version;
		}

		public static ushort Assert(EndianReader s
			, ushort expectedMin
			, ushort expectedMax)
		{
			Contract.Requires(s != null);

			var version = s.ReadUInt16();
			if (version < expectedMin || version > expectedMax)
				throw new VersionOutOfRangeException(s.BaseStream, expectedMin, expectedMax, version);

			return version;
		}

		public static uint Assert(EndianReader s
			, uint expectedMin
			, uint expectedMax)
		{
			Contract.Requires(s != null);

			var version = s.ReadUInt32();
			if (version < expectedMin || version > expectedMax)
				throw new VersionOutOfRangeException(s.BaseStream, expectedMin, expectedMax, version);

			return version;
		}

		public static ulong Assert(EndianReader s
			, ulong expectedMin
			, ulong expectedMax)
		{
			Contract.Requires(s != null);

			var version = s.ReadUInt64();
			if (version < expectedMin || version > expectedMax)
				throw new VersionOutOfRangeException(s.BaseStream, expectedMin, expectedMax, version);

			return version;
		}

		#endregion
	};
}
