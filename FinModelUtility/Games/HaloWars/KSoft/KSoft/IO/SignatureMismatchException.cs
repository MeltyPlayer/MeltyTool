using System.IO;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.IO
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA2237:MarkISerializableTypesWithSerializable")]
	public partial class SignatureMismatchException : System.Exception
	{
		const string kFormat = "Invalid signature! @{0} Expected '{1}', got '{2}'";
		const string kDescFormat = "Invalid '{0}' signature! Expected '{1}', got '{2}'";

		public SignatureMismatchException(string dataDescription, string expected, string found)
			: base(string.Format(Util.InvariantCultureInfo, kDescFormat, dataDescription, expected, found))
		{
			Contract.Requires(!string.IsNullOrEmpty(dataDescription));
		}

		SignatureMismatchException(long pos, string expected, string found) :
			base(string.Format(Util.InvariantCultureInfo, kFormat, pos.ToString("X8", Util.InvariantCultureInfo), expected, found))
		{
		}

		#region Stream ctors
		public SignatureMismatchException(Stream s, string expected, string found) :
			this(s.Position - expected.Length, expected, found)
		{
			Contract.Requires(s != null);
		}
		#endregion

		#region EndianReader utils
		public static void Assert(EndianReader s, string expected, Memory.Strings.StringStorage storage)
		{
			Contract.Requires(s != null);
			Contract.Requires(!string.IsNullOrEmpty(expected));

			string signature = s.ReadString(storage, expected.Length);
			if (signature != expected) throw new SignatureMismatchException(s.BaseStream,
				expected, signature);
		}
		public static void Assert(EndianReader s, string expected, Text.StringStorageEncoding encoding)
		{
			Contract.Requires(s != null);
			Contract.Requires(!string.IsNullOrEmpty(expected));
			Contract.Requires(encoding != null);

			string signature = s.ReadString(encoding, expected.Length);
			if (signature != expected) throw new SignatureMismatchException(s.BaseStream,
				expected, signature);
		}
		#endregion
	};
}
