using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.IO
{
	partial class TagElementStream<TDoc, TCursor, TName>
	{
		protected abstract void AppendElement(TCursor e);

		protected abstract void NestElement(TCursor e, out TCursor oldCursor);

		#region WriteElement impl
		/// <summary></summary>
		/// <param name="n">Node element to write</param>
		/// <param name="value">Data to set the element's <see cref="TCursor.InnerText"/> to</param>
		/// <param name="isFlags">Is <paramref name="enum_value"/> a <see cref="FlagsAttribute"/> based Enum?</param>
		protected abstract void WriteElementEnum<TEnum>(TCursor n, TEnum value, bool isFlags)
			where TEnum : struct, IComparable, IFormattable, IConvertible;

		protected abstract void WriteElement(TCursor n, Values.KGuid value);
		#endregion

		#region WriteCursor
		/// <summary>Set <see cref="Cursor"/>'s value to <paramref name="value"/></summary>
		/// <param name="value">Data to set the <see cref="Cursor"/> to</param>
		/// <param name="isFlags">Is <paramref name="enum_value"/> a <see cref="FlagsAttribute"/> based Enum?</param>
		public void WriteCursorEnum<TEnum>(TEnum value, bool isFlags = false)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			this.WriteElementEnum(this.Cursor, value, isFlags);
		}

		public void WriteCursor(Values.KGuid value)
		{
			this.WriteElement(this.Cursor, value);
		}
		#endregion

		#region WriteElement
		protected abstract TCursor WriteElementAppend(TName name);

		protected abstract TCursor WriteElementNest(TName name, out TCursor oldCursor);

		/// <summary>Create a new element under <see cref="Cursor"/> or the root element in the underlying <see cref="XmlDocument"/></summary>
		/// <param name="name">The <see cref="XmlElement"/>'s name</param>
		/// <param name="oldCursor">On return, contains the previous <see cref="Cursor"/> value</param>
		public void WriteElementBegin(TName name, out TCursor oldCursor)
		{
			Contract.Requires(this.ValidateNameArg(name));

			this.WriteElementNest(name, out oldCursor);
		}
		/// <summary>Restore the cursor to what it was before the corresponding call to a <see cref="WriteElementBegin(string, XmlElement&amp;)"/></summary>
		public void WriteElementEnd(ref TCursor oldCursor)
		{
			#if !CONTRACTS_FULL_SHIM // can't do this with our shim! ValueAtReturn sets out param to default ON ENTRY
			Contract.Ensures(Contract.ValueAtReturn(out oldCursor) == null);
			#endif

			this.RestoreCursor(ref oldCursor);
		}

		/// <summary>Create a new element in the underlying <see cref="XmlDocument"/>, relative to <see cref="Cursor"/></summary>
		/// <param name="name">The <see cref="XmlElement"/>'s name</param>
		/// <remarks>Does not change <see cref="Cursor"/></remarks>
		public void WriteElement(TName name)
		{
			Contract.Requires(this.ValidateNameArg(name));

			this.WriteElementAppend(name);
		}

		/// <summary>Create a new element in the underlying <see cref="XmlDocument"/>, relative to <see cref="Cursor"/></summary>
		/// <param name="name">The <see cref="XmlElement"/>'s name</param>
		/// <param name="value">Data to set the element's <see cref="XmlElement.InnerText"/> to</param>
		/// <param name="isFlags">Is <paramref name="enum_value"/> a <see cref="FlagsAttribute"/> based Enum?</param>
		/// <remarks>Does not change <see cref="Cursor"/></remarks>
		public void WriteElementEnum<TEnum>(TName name, TEnum value, bool isFlags = false)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			Contract.Requires(this.ValidateNameArg(name));

			this.WriteElementEnum(this.WriteElementAppend(name), value, isFlags);
		}

		public void WriteElement(TName name, Values.KGuid value)
		{
			Contract.Requires(this.ValidateNameArg(name));

			this.WriteElement(this.WriteElementAppend(name), value);
		}
		#endregion

		#region WriteAttribute
		/// <summary>Create a new attribute for <see cref="Cursor"/></summary>
		/// <param name="name">Name of the <see cref="XmlAttribute"/></param>
		/// <param name="value">Data to set the attribute text to</param>
		/// <param name="isFlags">Is <paramref name="enum_value"/> a <see cref="FlagsAttribute"/> based Enum?</param>
		public abstract void WriteAttributeEnum<TEnum>(TName name, TEnum value, bool isFlags = false)
			where TEnum : struct, IFormattable;

		public abstract void WriteAttribute(TName name, Values.KGuid value);
		#endregion

		#region WriteElementOpt
		/// <summary>Create a new element in the underlying <see cref="XmlDocument"/>, relative to <see cref="Cursor"/></summary>
		/// <param name="name">The <see cref="XmlElement"/>'s name</param>
		/// <param name="value">Data to set the element's <see cref="XmlElement.InnerText"/> to</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <param name="isFlags">Is <paramref name="value"/> a <see cref="FlagsAttribute"/> based Enum?</param>
		/// <returns>True if <paramref name="value"/> was written</returns>
		public bool WriteElementEnumOptOnTrue<TEnum>(TName name, TEnum value, Predicate<TEnum> predicate, bool isFlags = false)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires(this.Cursor != null, TagElementStreamContract<TDoc,TCursor,TName>.kCursorNullMsg);
			Contract.Requires(predicate != null);

			bool result = this.IgnoreWritePredicates || predicate(value);

			if (result)
				this.WriteElementEnum(name, value, isFlags);

			return result;
		}

		/// <summary>Create a new element in the underlying <see cref="XmlDocument"/>, relative to <see cref="Cursor"/></summary>
		/// <param name="name">The <see cref="XmlElement"/>'s name</param>
		/// <param name="value">Data to set the element's <see cref="XmlElement.InnerText"/> to</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>isn't</b> written</param>
		/// <param name="isFlags">Is <paramref name="value"/> a <see cref="FlagsAttribute"/> based Enum?</param>
		/// <returns>True if <paramref name="value"/> was written</returns>
		public bool WriteElementEnumOptOnFalse<TEnum>(TName name, TEnum value, Predicate<TEnum> predicate, bool isFlags = false)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires(this.Cursor != null, TagElementStreamContract<TDoc,TCursor,TName>.kCursorNullMsg);
			Contract.Requires(predicate != null);

			bool result = this.IgnoreWritePredicates || !predicate(value);

			if (result)
				this.WriteElementEnum(name, value, isFlags);

			return result;
		}

		public bool WriteElementOptOnTrue(TName name, Values.KGuid value, Predicate<Values.KGuid> predicate)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires(this.Cursor != null, TagElementStreamContract<TDoc, TCursor, TName>.kCursorNullMsg);
			Contract.Requires(predicate != null);

			bool result = this.IgnoreWritePredicates || predicate(value);

			if (result)
				this.WriteElement(name, value);

			return result;
		}
		#endregion

		#region WriteAttributeOpt
		/// <summary>Create a new attribute for <see cref="Cursor"/></summary>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <param name="name">Name of the <see cref="XmlAttribute"/></param>
		/// <param name="value">Data to set the attribute text to</param>
		/// <param name="isFlags">Is <paramref name="value"/> a <see cref="FlagsAttribute"/> based Enum?</param>
		/// <returns>True if <paramref name="value"/> was written</returns>
		public bool WriteAttributeEnumOptOnTrue<TEnum>(TName name, TEnum value, Predicate<TEnum> predicate, bool isFlags = false)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires(this.Cursor != null, TagElementStreamContract<TDoc,TCursor,TName>.kCursorNullMsg);
			Contract.Requires(predicate != null);

			bool result = this.IgnoreWritePredicates || predicate(value);

			if (result)
				this.WriteAttributeEnum(name, value, isFlags);

			return result;
		}

		/// <summary>Create a new attribute for <see cref="Cursor"/></summary>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>isn't</b> written</param>
		/// <param name="name">Name of the <see cref="XmlAttribute"/></param>
		/// <param name="value">Data to set the attribute text to</param>
		/// <param name="isFlags">Is <paramref name="value"/> a <see cref="FlagsAttribute"/> based Enum?</param>
		/// <returns>True if <paramref name="value"/> was written</returns>
		public bool WriteAttributeEnumOptOnFalse<TEnum>(TName name, TEnum value, Predicate<TEnum> predicate, bool isFlags = false)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires(this.Cursor != null, TagElementStreamContract<TDoc,TCursor,TName>.kCursorNullMsg);
			Contract.Requires(predicate != null);

			bool result = this.IgnoreWritePredicates || !predicate(value);

			if (result)
				this.WriteAttributeEnum(name, value, isFlags);

			return result;
		}

		public bool WriteAttributeOptOnTrue(TName name, Values.KGuid value, Predicate<Values.KGuid> predicate)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires(this.Cursor != null, TagElementStreamContract<TDoc, TCursor, TName>.kCursorNullMsg);
			Contract.Requires(predicate != null);

			bool result = this.IgnoreWritePredicates || predicate(value);

			if (result)
				this.WriteAttribute(name, value);

			return result;
		}
		#endregion

		#region WriteElements (ICollection)
		public void WriteElements<T, TContext>(TName elementName,
			IEnumerable<T> coll, TContext ctxt, StreamAction<T, TContext> action)
		{
			Contract.Requires(this.ValidateNameArg(elementName));
			Contract.Requires<ArgumentNullException>(coll != null);
			Contract.Requires(action != null);

			foreach (var value in coll)
				using (this.EnterCursorBookmark(elementName))
				{
					var v = value; // can't pass a foreach value by ref
					action(this, ctxt, ref v);
				}
		}

		public void WriteStreamableElements<T>(TName elementName,
			IEnumerable<T> coll,
			Predicate<T> shouldWritePredicate = null)
			where T : ITagElementStreamable<TName>
		{
			Contract.Requires(this.ValidateNameArg(elementName));
			Contract.Requires<ArgumentNullException>(coll != null);

			foreach (var value in coll)
				if (shouldWritePredicate == null || shouldWritePredicate(value))
					using (this.EnterCursorBookmark(elementName))
						value.Serialize(this);
		}
		#endregion

		#region WriteElements (IDictionary)
		public void WriteElements<TKey, TValue, TContext>(TName elementName,
			IDictionary<TKey, TValue> dic, TContext ctxt,
			StreamAction<TKey, TContext> streamKey,
			StreamAction<TValue, TContext> streamValue)
		{
			Contract.Requires(this.ValidateNameArg(elementName));
			Contract.Requires<ArgumentNullException>(dic != null);
			Contract.Requires(streamKey != null);
			Contract.Requires(streamValue != null);

			foreach (var kv in dic)
				using (this.EnterCursorBookmark(elementName))
				{
					var key = kv.Key;
					streamKey(this, ctxt, ref key);

					var v = kv.Value;
					streamValue(this, ctxt, ref v);
				}
		}

		public void WriteStreamableElements<TKey, TValue, TContext>(TName elementName,
			IDictionary<TKey, TValue> dic, TContext ctxt,
			StreamAction<TKey, TContext> streamKey,
			Predicate<KeyValuePair<TKey, TValue>> shouldWritePredicate = null)
			where TValue : ITagElementStreamable<TName>
		{
			Contract.Requires(this.ValidateNameArg(elementName));
			Contract.Requires<ArgumentNullException>(dic != null);
			Contract.Requires(streamKey != null);

			foreach (var kv in dic)
				if (shouldWritePredicate == null || shouldWritePredicate(kv))
					using (this.EnterCursorBookmark(elementName))
					{
						TKey key = kv.Key;
						streamKey(this, ctxt, ref key);

						kv.Value.Serialize(this);
					}
		}
		#endregion

		#region WriteComment
		/// <remarks>Will throw a contract error if implementation's <see cref="SupportsComments"/> returns true but doesn't override this method</remarks>
		protected virtual void WriteCommentImpl(string comment)
		{
			Contract.Assert(!this.SupportsComments, "Stream supports comments, but implementation class doesn't provide an API for it. Remove support or add the API");
		}

		/// <summary>
		/// If the stream <see cref="IsWriting"/> and <see cref="SupportsComments"/> and <see cref="CommentsEnabled"/>, writes <paramref name="comment"/> to the stream
		/// </summary>
		/// <param name="comment"></param>
		public void WriteComment(string comment)
		{
			if (this.IsWriting && this.SupportsComments && this.CommentsEnabled)
				this.WriteCommentImpl(comment);
		}
		public virtual void WriteComment<TContext>(TContext ctxt, Func<TContext, string> commentMaker)
		{
			Contract.Requires(commentMaker != null);

			if (this.IsWriting && this.SupportsComments && this.CommentsEnabled)
			{
				string comment = commentMaker(ctxt);
				this.WriteCommentImpl(comment);
			}
		}
		#endregion
	};
	partial class TagElementStreamContract<TDoc, TCursor, TName>
	{
		internal const string kCursorNullMsg = "Element cursor must not be null when writing an attribute.";

		#region WriteAttribute
		public override void WriteAttributeEnum<TEnum>(TName name, TEnum value, bool isFlags)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires(this.Cursor != null, kCursorNullMsg);

			throw new NotImplementedException();
		}

		public override void WriteAttribute(TName name, Values.KGuid value)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires(this.Cursor != null, kCursorNullMsg);

			throw new NotImplementedException();
		}
		#endregion
	};

	/// <summary>
	/// Helper type for exposing the <see cref="XmlElementStream.WriteElementBegin(string)">WriteElementBegin</see> and
	/// <see cref="XmlElementStream.WriteElementEnd()">WriteElementEnd</see> in a way which works with the C# "using" statements
	/// </summary>
	[SuppressMessage("Microsoft.Design", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
	public struct TagElementStreamWriteBookmark<TDoc, TCursor, TName> : IDisposable
		where TDoc : class
		where TCursor : class
	{
		TagElementStream<TDoc, TCursor, TName> mStream;
		TCursor mOldCursor;

		/// <summary>Saves the stream's cursor so a new one can be specified, but then later restored to the saved cursor, via <see cref="Dispose()"/></summary>
		/// <param name="stream">The underlying stream for this bookmark</param>
		/// <param name="elementName"></param>
		public TagElementStreamWriteBookmark(TagElementStream<TDoc, TCursor, TName> stream, TName elementName)
		{
			Contract.Requires<ArgumentNullException>(stream != null);
			Contract.Requires<ArgumentNullException>(elementName != null);

			this.mStream = null;
			this.mOldCursor = null;
			(this.mStream = stream).WriteElementBegin(elementName, out this.mOldCursor);
		}

		/// <summary>Returns the cursor of the underlying stream to the last saved cursor value</summary>
		public void Dispose()
		{
			if (this.mStream != null)
			{
				this.mStream.WriteElementEnd(ref this.mOldCursor);
				this.mStream = null;
			}
		}
	};
}
