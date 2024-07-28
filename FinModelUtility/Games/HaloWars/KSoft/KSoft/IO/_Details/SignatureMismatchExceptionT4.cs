using System.IO;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.IO
{
	partial class SignatureMismatchException
	{
		#region Stream ctors
		public SignatureMismatchException(Stream s, byte expected, byte found) :
			this(s.Position - 1,
				expected.ToString("X2", Util.InvariantCultureInfo),
				found.ToString("X2", Util.InvariantCultureInfo))
		{
			Contract.Requires(s != null);
		}

		public SignatureMismatchException(Stream s, ushort expected, ushort found) :
			this(s.Position - 2,
				expected.ToString("X4", Util.InvariantCultureInfo),
				found.ToString("X4", Util.InvariantCultureInfo))
		{
			Contract.Requires(s != null);
		}

		public SignatureMismatchException(Stream s, uint expected, uint found) :
			this(s.Position - 4,
				expected.ToString("X8", Util.InvariantCultureInfo),
				found.ToString("X8", Util.InvariantCultureInfo))
		{
			Contract.Requires(s != null);
		}

		public SignatureMismatchException(Stream s, ulong expected, ulong found) :
			this(s.Position - 8,
				expected.ToString("X16", Util.InvariantCultureInfo),
				found.ToString("X16", Util.InvariantCultureInfo))
		{
			Contract.Requires(s != null);
		}

		#endregion

		#region EndianReader util
		public static void Assert(EndianReader s, byte expected)
		{
			Contract.Requires(s != null);

			var version = s.ReadByte();
			if (version != expected) throw new SignatureMismatchException(s.BaseStream,
				expected, version);
		}

		public static void Assert(EndianReader s, ushort expected)
		{
			Contract.Requires(s != null);

			var version = s.ReadUInt16();
			if (version != expected) throw new SignatureMismatchException(s.BaseStream,
				expected, version);
		}

		public static void Assert(EndianReader s, uint expected)
		{
			Contract.Requires(s != null);

			var version = s.ReadUInt32();
			if (version != expected) throw new SignatureMismatchException(s.BaseStream,
				expected, version);
		}

		public static void Assert(EndianReader s, ulong expected)
		{
			Contract.Requires(s != null);

			var version = s.ReadUInt64();
			if (version != expected) throw new SignatureMismatchException(s.BaseStream,
				expected, version);
		}

		#endregion
	};
}
