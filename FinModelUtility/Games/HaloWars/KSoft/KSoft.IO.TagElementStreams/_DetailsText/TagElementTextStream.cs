using Contracts = System.Diagnostics.Contracts;

namespace KSoft.IO
{
	/// <remarks><typeparamref name="TCursor"/> needs to implement <see cref="Text.ITextLineInfo">LineInfo</see></remarks>
	public abstract partial class TagElementTextStream<TDoc, TCursor> : TagElementStream<TDoc, TCursor, string>
		where TDoc : class
		where TCursor : class
	{
		/// <summary>Element's qualified name, or null if <see cref="Cursor"/> is null</summary>
		public abstract string CursorName { get; }

		#region GuidFormatString
		string mGuidFormatString = Values.KGuid.kFormatHyphenated;
		/// <summary>
		/// The formatting string for read/writing Guid values.
		/// Hyphenated is the default format. Setting this to NULL reverts the format to default
		/// </summary>
		public string GuidFormatString
		{
			get { return this.mGuidFormatString; }
			set { this.mGuidFormatString = value ?? Values.KGuid.kFormatHyphenated; }
		}
		#endregion

		#region SingleFormatSpecifier
		string mSingleFormatSpecifier = Numbers.kSingleRoundTripFormatSpecifier;
		/// <summary>RoundTrip by default</summary>
		public string SingleFormatSpecifier
		{
			get { return this.mSingleFormatSpecifier; }
			set { this.mSingleFormatSpecifier = value; }
		}
		#endregion

		#region DoubleFormatSpecifier
		string mDoubleFormatSpecifier = Numbers.kDoubleRoundTripFormatSpecifier;
		/// <summary>RoundTrip by default</summary>
		public string DoubleFormatSpecifier
		{
			get { return this.mDoubleFormatSpecifier; }
			set { this.mDoubleFormatSpecifier = value; }
		}
		#endregion

		[Contracts.Pure]
		public override bool ValidateNameArg(string name) { return !string.IsNullOrEmpty(name); }

		protected TagElementTextStream()
		{
			this.mReadErrorState = new TextStreamReadErrorState(this);
		}

		public void UseDefaultFloatFormatSpecifiers()
		{
			this.SingleFormatSpecifier = Numbers.kFloatDefaultFormatSpecifier;
			this.DoubleFormatSpecifier = Numbers.kFloatDefaultFormatSpecifier;
		}
		public void UseRoundTripFloatFormatSpecifiers()
		{
			this.SingleFormatSpecifier = Numbers.kSingleRoundTripFormatSpecifier;
			this.DoubleFormatSpecifier = Numbers.kDoubleRoundTripFormatSpecifier;
		}
	};

	static partial class TagElementTextStreamUtils
	{
	};
}