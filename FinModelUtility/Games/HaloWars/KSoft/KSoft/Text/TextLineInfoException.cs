using System;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Text
{
	/// <summary>Exception for use as an inner exception when processing text files and there's line/column information available</summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA2237:MarkISerializableTypesWithSerializable")]
	public class TextLineInfoException
		: Exception
		, ITextLineInfo
	{
		readonly string mStreamName;
		// Use TextLineInfo, instead of ITextLineInfo, as it is a value type, thus just an explicit copy of the line info
		readonly TextLineInfo mLineInfo;

		public string StreamName { get => this.mStreamName; }

		public TextLineInfoException(Exception innerException, ITextLineInfo lineInfo, string streamName = null)
			: base("Text stream error", innerException)
		{
			Contract.Requires<ArgumentNullException>(lineInfo != null);

			if (string.IsNullOrEmpty(streamName))
				streamName = "<unknown text stream>";

			this.mStreamName = streamName;
			this.mLineInfo = new TextLineInfo(lineInfo);
		}
		public TextLineInfoException(ITextLineInfo lineInfo, string streamName = null)
			: this(null, lineInfo, streamName)
		{
			Contract.Requires<ArgumentNullException>(lineInfo != null);
		}

		public override string Message { get => string.Format(KSoft.Util.InvariantCultureInfo,
			"{0} ({1})",
			this.mStreamName,
			this.mLineInfo.ToString());
		}

		#region ITextLineInfo Members
		public bool HasLineInfo	{ get => this.mLineInfo.HasLineInfo; }
		public int LineNumber	{ get => this.mLineInfo.LineNumber; }
		public int LinePosition	{ get => this.mLineInfo.LinePosition; }
		#endregion
	};
}
