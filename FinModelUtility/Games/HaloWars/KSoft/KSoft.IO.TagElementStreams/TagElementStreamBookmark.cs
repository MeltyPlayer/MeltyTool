using System;
using System.Diagnostics.CodeAnalysis;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.IO
{
	/// <summary>
	/// Helper type for exposing the <see cref="TagElementStream.StreamElementBegin(string)">StreamElementBegin</see> and
	/// <see cref="TagElementStream.StreamElementEnd()">StreamElementEnd</see> in a way which works with the C# "using" statements
	/// </summary>
	/// <remarks>If a null element name is given, skips the bookmarking process entirely</remarks>
	[SuppressMessage("Microsoft.Design", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
	public struct TagElementStreamBookmark<TDoc, TCursor, TName> : IDisposable
		where TDoc : class
		where TCursor : class
	{
		TagElementStream<TDoc, TCursor, TName> mStream;
		TCursor mOldCursor;

		#region Null
		TagElementStreamBookmark(
			[SuppressMessage("Microsoft.Design", "CA1801:ReviewUnusedParameters")]
			bool dummy)
		{
			this.mStream = null;
			this.mOldCursor = null;
		}

		[SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
		public static  TagElementStreamBookmark<TDoc, TCursor, TName> Null { get {
			return new TagElementStreamBookmark<TDoc, TCursor, TName>(dummy:true);
		} }
		#endregion

		/// <summary>Is this bookmark active?</summary>
		/// <remarks>The bookmark can start out 'null' or become null after disposal</remarks>
		public bool IsNotNull { get { return this.mStream != null; } }

		/// <summary>Saves the stream's cursor so a new one can be specified, but then later restored to the saved cursor, via <see cref="Dispose()"/></summary>
		/// <param name="stream">The underlying stream for this bookmark</param>
		/// <param name="elementName">If null, no bookmarking is actually performed</param>
		public TagElementStreamBookmark(TagElementStream<TDoc, TCursor, TName> stream, TName elementName)
		{
			Contract.Requires<ArgumentNullException>(stream != null);

			this.mStream = null;
			this.mOldCursor = null;

			if(elementName != null)
				(this.mStream = stream).StreamElementBegin(elementName, out this.mOldCursor);
		}

		/// <summary>Returns the cursor of the underlying stream to the last saved cursor value</summary>
		public void Dispose()
		{
			if (this.mStream != null)
			{
				this.mStream.StreamElementEnd(ref this.mOldCursor);
				this.mStream = null;
			}
		}
	};

	/// <summary>
	/// Helper type for exposing the <see cref="TagElementStream.SaveCursor()">SaveCursor</see> and
	/// <see cref="TagElementStream.RestoreCursor()">RestoreCursor</see> in a way which works with the C# "using" statements
	/// </summary>
	[SuppressMessage("Microsoft.Design", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
	public struct TagElementStreamReadBookmark<TDoc, TCursor, TName> : IDisposable
		where TDoc : class
		where TCursor : class
	{
		TagElementStream<TDoc, TCursor, TName> mStream;
		TCursor mOldCursor;

		/// <summary>Saves the stream's cursor so a new one can be specified, but then later restored to the saved cursor, via <see cref="Dispose()"/></summary>
		/// <param name="stream">The underlying stream for this bookmark</param>
		public TagElementStreamReadBookmark(TagElementStream<TDoc, TCursor, TName> stream)
		{
			Contract.Requires<ArgumentNullException>(stream != null);

			(this.mStream = stream).SaveCursor(null, out this.mOldCursor);
		}
		/// <summary>Saves the stream's cursor and sets <paramref name="newCursor"/> to be the new cursor for the stream</summary>
		/// <param name="stream">The underlying stream for this bookmark</param>
		/// <param name="newCursor">The new cursor for the stream</param>
		public TagElementStreamReadBookmark(TagElementStream<TDoc, TCursor, TName> stream, TCursor newCursor)
		{
			Contract.Requires<ArgumentNullException>(stream != null);
			Contract.Requires<ArgumentNullException>(newCursor != null);

			(this.mStream = stream).SaveCursor(newCursor, out this.mOldCursor);
		}

		/// <summary>Returns the cursor of the underlying stream to the last saved cursor value</summary>
		public void Dispose()
		{
			if (this.mStream != null)
			{
				this.mStream.RestoreCursor(ref this.mOldCursor);
				this.mStream = null;
			}
		}
	};
}
