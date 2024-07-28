using System;
using System.Collections.Generic;
using Contracts = System.Diagnostics.Contracts;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.IO
{
	public enum TagElementNodeType
	{
		Element=	System.Xml.XmlNodeType.Element,
		Attribute=	System.Xml.XmlNodeType.Attribute,
		Text=		System.Xml.XmlNodeType.Text,
	};

	/// <summary></summary>
	/// <typeparam name="TDoc">Backing document type (eg, XmlDocument) for this element stream</typeparam>
	/// <typeparam name="TCursor">Type used to represent Elements</typeparam>
	/// <typeparam name="TName">Type used to represent name values (eg, string)</typeparam>
	[Contracts.ContractClass(typeof(TagElementStreamContract<,,>))]
	public abstract partial class TagElementStream<TDoc, TCursor, TName>
		: IKSoftStream
		, IKSoftStreamModeable
		, ICanThrowReadExceptionsWithExtraDetails
		, IDisposable
		where TDoc : class
		where TCursor : class
	{
		protected const NumeralBase kDefaultRadix = NumeralBase.Decimal;

		#region Owner
		/// <summary>Owner of this stream</summary>
		public object Owner { get; set; }

		public object UserData { get; set; }
		#endregion

		#region IKSoftStreamModeable
		public System.IO.FileAccess StreamMode { get; set; }
		public bool IsReading { get { return this.StreamMode == System.IO.FileAccess.Read; } }
		public bool IsWriting { get { return this.StreamMode == System.IO.FileAccess.Write; } }

		/// <summary>Supported access permissions for this stream</summary>
		public System.IO.FileAccess StreamPermissions { get; protected set; }

		protected void ValidateReadPermission()
		{
			Contract.Assert(this.StreamPermissions.CanRead(), "Stream permissions do not support reading");
		}
		protected void ValidateWritePermission()
		{
			Contract.Assert(this.StreamPermissions.CanWrite(), "Stream permissions do not support writing");
		}
		#endregion

		#region StreamName
		/// <summary></summary>
		/// <remarks>If this is for a file, this is the file name this stream is handling</remarks>
		public string StreamName { get; protected set; }

		protected void SetStreamName(System.IO.Stream stream)
		{
			Contract.Assume(stream != null);

			this.StreamName = null;

			if (stream is System.IO.FileStream fs)
				this.StreamName = fs.Name;

			if (this.StreamName == null)
				stream.ToString();
		}
		#endregion

		#region Document
		/// <summary>Backing document (eg, XmlDocument) for this element stream</summary>
		public TDoc Document { get; protected set; }
		#endregion

		#region Cursor
		TCursor mCursor;
		/// <summary>Element data we are streaming data to and from</summary>
		public TCursor Cursor {
			get { return this.mCursor; }
			set {
				Contract.Requires<ArgumentNullException>(value != null);

				this.mCursor = value;
			}
		}

		/// <summary>Try to get the number of child elements within the cursor</summary>
		/// <param name="defaultCount">Return value to use if this fails to get the element count</param>
		/// <returns></returns>
		public int TryGetCursorElementCount(int defaultCount = 0)
		{
			var count = this.Cursor != null
				? this.PredictElementCount(this.Cursor)
				: TypeExtensions.kNone;

			return count < 0
				? defaultCount
				: count;
		}

		/// <summary>
		/// Initializes the <see cref="Cursor"/> to the underlying document's root
		/// (eg, <see cref="XmlDocument.DocumentElement"/>)
		/// </summary>
		public abstract void InitializeAtRootElement();

		/// <summary>
		/// Saves the current stream cursor in <paramref name="oldCursor"/> and sets <paramref name="newCursor"/> to be the new cursor for the stream
		/// </summary>
		/// <param name="newCursor">If not null, will be the new value of <see cref="Cursor"/></param>
		/// <param name="oldCursor">On return, contains the value of <see cref="Cursor"/> before the call to this method</param>
		public void SaveCursor(TCursor newCursor, out TCursor oldCursor)
		{
			oldCursor = this.Cursor;
			if(newCursor != null)
				this.Cursor = newCursor;
		}
		/// <summary>Returns the cursor to a previously saved cursor value</summary>
		/// <param name="oldCursor">Previously saved cursor. Set to null before the method returns</param>
		public void RestoreCursor(ref TCursor oldCursor)
		{
			#if !CONTRACTS_FULL_SHIM // can't do this with our shim! ValueAtReturn sets out param to default ON ENTRY
			Contract.Ensures(Contract.ValueAtReturn(out oldCursor) == null);
			#endif
			Contract.Assert(oldCursor != null, "Can't restore a cursor that wasn't saved!");

			this.Cursor = oldCursor;
			oldCursor = null;
		}

		/// <summary>Enter a new tag element <b>(for reading)</b></summary>
		/// <param name="newCursor"></param>
		/// <returns></returns>
		public /*IDisposable*/TagElementStreamReadBookmark<TDoc, TCursor, TName> EnterCursorBookmark(TCursor newCursor)
		{
			Contract.Requires<ArgumentNullException>(newCursor != null);

			return new TagElementStreamReadBookmark<TDoc, TCursor, TName>(this, newCursor);
		}
		/// <summary>(Optionally) enter an tag element</summary>
		/// <param name="elementName">If null, no bookmarking is actually performed</param>
		/// <returns></returns>
		public /*IDisposable*/TagElementStreamBookmark<TDoc, TCursor, TName> EnterCursorBookmark(TName elementName)
		{
			return new TagElementStreamBookmark<TDoc, TCursor, TName>(this, elementName);
		}
		/// <summary>Optionally enter an tag element</summary>
		/// <param name="elementName">If null, no bookmarking is actually performed</param>
		/// <returns></returns>
		/// <remarks>
		/// When reading, implicitly checks if <see cref="ElementsExists"/> with <paramref name="elementName"/> for entering a bookmark.
		/// </remarks>
		public /*IDisposable*/TagElementStreamBookmark<TDoc, TCursor, TName> EnterCursorBookmarkOpt(TName elementName)
		{
			if ((this.IsReading && this.ElementsExists(elementName)) ||
			    this.IsWriting)
				return new TagElementStreamBookmark<TDoc, TCursor, TName>(this, elementName);

			return TagElementStreamBookmark<TDoc, TCursor, TName>.Null;
		}
		/// <summary>Optionally enter an tag element</summary>
		/// <typeparam name="T">Context type for the write predicate's object</typeparam>
		/// <param name="elementName">If null, no bookmarking is actually performed</param>
		/// <param name="theObj">The context object for <paramref name="writeShouldEnterBookmark"/></param>
		/// <param name="writeShouldEnterBookmark">When writing, this predicate controls if a bookmark is entered</param>
		/// <returns></returns>
		/// <remarks>
		/// When reading, implicitly checks if <see cref="ElementsExists"/> with <paramref name="elementName"/> for entering a bookmark.
		/// When writing, works entirely on <see cref="writeShouldEnterBookmark"/> for entering a bookmark.
		/// </remarks>
		public /*IDisposable*/TagElementStreamBookmark<TDoc, TCursor, TName> EnterCursorBookmarkOpt<T>(TName elementName,
			T theObj, Predicate<T> writeShouldEnterBookmark)
		{
			if ((this.IsReading && this.ElementsExists(elementName)) ||
				(this.IsWriting && writeShouldEnterBookmark(theObj)))
				return new TagElementStreamBookmark<TDoc, TCursor, TName>(this, elementName);

			return TagElementStreamBookmark<TDoc, TCursor, TName>.Null;
		}
		#endregion

		#region Comments
		bool mCommentsEnabled;
		/// <summary>If the stream supports comments, this toggles their usage</summary>
		/// <remarks>Off by default, setting this when a stream doesn't support comments is an error</remarks>
		public bool CommentsEnabled
		{
			get { return this.mCommentsEnabled; }
			set {
				Contract.Requires(this.SupportsComments, "Stream must support comments in order to toggle their usage");

				this.mCommentsEnabled = value;
			}
		}

		/// <summary>Returns true if the element stream supports commenting</summary>
		public virtual bool SupportsComments { get { return false; } }
		#endregion

		/// <summary>Causes WriteOpt() operations to always execute, even if the predicate says not to</summary>
		/// <remarks>
		/// Use at your own risk. Can be useful to always write, even when a value is in a 'default' state. However,
		/// some write predicates are used to avoid invalid operations on data (eg, null collections)
		/// </remarks>
		public bool IgnoreWritePredicates { get; set; }

		#region Util
		/// <summary>Checks to see if the current scope has a fully defined attribute named <paramref name="name"/></summary>
		/// <param name="name"></param>
		/// <returns></returns>
		/// <remarks>Returns false if <see cref="ValidateNameArg(TName)"/> fails on <paramref name="name"/></remarks>
		public abstract bool AttributeExists(TName name);

		/// <summary>Checks to see if the current scope has attributes</summary>
		/// <returns></returns>
		public abstract bool AttributesExist { get; }

		public abstract IEnumerable<TName> AttributeNames { get; }

		/// <summary>Checks to see if the current scope has a fully defined element named <paramref name="name"/></summary>
		/// <param name="name"></param>
		/// <returns></returns>
		/// <remarks>Returns false if <see cref="ValidateNameArg(TName)"/> fails on <paramref name="name"/></remarks>
		public abstract bool ElementsExists(TName name);

		/// <summary>Checks to see if the current scope has elements</summary>
		/// <returns></returns>
		public abstract bool ElementsExist { get; }

		public abstract IEnumerable<TCursor> Elements { get; }

		public abstract IEnumerable<TCursor> ElementsByName(TName localName);

		public abstract TName GetElementName(TCursor element);

		protected abstract int PredictElementCount(TCursor cursor);
		#endregion

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				this.mCursor = null;

				this.Owner = null;
			}
		}

		[Contracts.Pure]
		public abstract bool ValidateNameArg(TName name);
		[Contracts.Pure]
		public virtual bool ValidateNameArg(TName nodeName, TagElementNodeType nodeType)
		{
			return nodeType.RequiresName() == this.ValidateNameArg(nodeName);
		}
	};

	[Contracts.ContractClassFor(typeof(TagElementStream<,,>))]
	abstract partial class TagElementStreamContract<TDoc, TCursor, TName> : TagElementStream<TDoc, TCursor, TName>
		where TDoc : class
		where TCursor : class
	{
		public override IEnumerable<TCursor> Elements { get {
			Contract.Requires(this.Cursor != null);
			Contract.Ensures(Contract.Result<IEnumerable<TCursor>>() != null);

			throw new NotImplementedException();
		} }

		public override IEnumerable<TCursor> ElementsByName(TName localName)
		{
			Contract.Requires(this.ValidateNameArg(localName));
			Contract.Ensures(Contract.Result<IEnumerable<TCursor>>() != null);

			throw new NotImplementedException();
		}

		protected override int PredictElementCount(TCursor cursor)
		{
			Contract.Requires(cursor != null);

			throw new NotImplementedException();
		}
	};
}
