using System;
using System.Collections.Generic;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.IO
{
	internal enum TagElementStreamParseEnumResult
	{
		Success,
		FailedMemberNotFound,
	};
	internal static class TagElementStreamParseEnumUtil
	{
		public static int EnumToInt<TEnum>(TEnum value)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			// Note: Enum's convertible implementation isn't efficient. Uses 'GetValue' which returns the value in a boxed object
			//return value.ToInt32(null);

			return Reflection.EnumValue<TEnum>.ToInt32(value);
		}

		public static TagElementStreamParseEnumResult Parse<TEnum>(bool ignoreCase,
			string str, ref TEnum value)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			bool result = Enum.TryParse(str, ignoreCase, out TEnum temp);

			if (!result)
				return TagElementStreamParseEnumResult.FailedMemberNotFound;
			else
				value = temp;

			return TagElementStreamParseEnumResult.Success;
		}
		public static TagElementStreamParseEnumResult Parse<TEnum>(bool ignoreCase,
			string str, ref int intValue)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			intValue = 0;

			TEnum value = default(TEnum);
			var result = Parse(ignoreCase, str, ref value);

			if (result == TagElementStreamParseEnumResult.Success)
				intValue = EnumToInt(value);

			return result;
		}

#if false // currently unused. probably obsolete
		public static bool ParseOpt<TEnum>(bool ignoreCase,
			string str, out TEnum value)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			bool result = Enum.TryParse(str, ignoreCase, out value);

			return result;
		}
		public static bool ParseOpt<TEnum>(bool ignoreCase,
			string str, out int intValue)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			intValue = 0;

			TEnum value;
			bool result = ParseOpt(ignoreCase, str, out value);

			if (result)
				intValue = EnumToInt(value);

			return result;
		}
#endif
	};

	partial class TagElementStream<TDoc, TCursor, TName>
	{
		#region Enums
		/// <summary>Should Enums read from the steam be treated with no respect to case sensitivity?</summary>
		/// <remarks>If true, 'CanRead' would be parsed where 'canread' appears</remarks>
		public bool IgnoreCaseOnEnums { get; set; }

		public bool ExceptionOnEnumParseFail { get; set; }
		#endregion

		/// <summary>Throws a suitable exception to detail the position information of the last read</summary>
		/// <param name="detailsException">The additional details to include in the thrown exception (really, the inner exception. eg, InvalidData)</param>
		public abstract void ThrowReadException(Exception detailsException);
		// #NOTE: this is dumb, but without doing this I get CS0535
		void ICanThrowReadExceptionsWithExtraDetails.ThrowReadExeception(Exception detailsException) {
			this.ThrowReadException(detailsException); }

		// #TODO: document that 'ref value' will equal the streamed value or 'null' after returning, depending on success

		#region ReadElement impl
		/// <summary>Stream out the InnerText of element <paramref name="name"/> into the enum <paramref name="value"/></summary>
		/// <typeparam name="TEnum">Enumeration type</typeparam>
		/// <param name="n">Node element to read</param>
		/// <param name="enumValue">value to receive the data</param>
		protected abstract void ReadElementEnum<TEnum>(TCursor n, ref TEnum enumValue)
			where TEnum : struct, IComparable, IFormattable, IConvertible;
		/// <summary>Stream out the InnerText of element <paramref name="name"/> into the enum <paramref name="value"/></summary>
		/// <typeparam name="TEnum">Enumeration type</typeparam>
		/// <param name="n">Node element to read</param>
		/// <param name="enumValue">value to receive the data</param>
		protected abstract void ReadElementEnum<TEnum>(TCursor n, ref int enumValue)
			where TEnum : struct, IComparable, IFormattable, IConvertible;

		protected abstract void ReadElement(TCursor n, ref Values.KGuid value);
		#endregion

		#region ReadCursor
		/// <summary>Stream out the Value of <see cref="Cursor"/> into the enum <paramref name="enumValue"/></summary>
		/// <typeparam name="TEnum">Enumeration type</typeparam>
		/// <param name="enumValue">value to receive the data</param>
		public void ReadCursorEnum<TEnum>(ref TEnum enumValue)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			this.ReadElementEnum(this.Cursor, ref enumValue);
		}
		/// <summary>Stream out the Value of <see cref="Cursor"/> into the enum <paramref name="enumValue"/></summary>
		/// <typeparam name="TEnum">Enumeration type</typeparam>
		/// <param name="enumValue">value to receive the data</param>
		public void ReadCursorEnum<TEnum>(ref int enumValue)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			this.ReadElementEnum(this.Cursor, ref enumValue);
		}

		/// <summary>Interpret the Name of <see cref="Cursor"/> as a member of <typeparamref name="TEnum"/></summary>
		/// <typeparam name="TEnum">Enumeration type</typeparam>
		/// <param name="enumValue">value to receive the data</param>
		public abstract void ReadCursorName<TEnum>(ref TEnum enumValue)
			where TEnum : struct, IComparable, IFormattable, IConvertible;

		public void ReadCursor(ref Values.KGuid value)
		{
			this.ReadElement(this.Cursor, ref value);
		}
		#endregion

		#region ReadElement
		public abstract void ReadElementBegin(TName name, out TCursor oldCursor);
		/// <summary>Restore the cursor to what it was before the corresponding call to a <see cref="ReadElementBegin(string, TCursor&amp;)"/></summary>
		public abstract void ReadElementEnd(ref TCursor oldCursor);

		protected abstract TCursor GetElement(TName name);

		/// <summary>Stream out the InnerText of element <paramref name="name"/> into the enum <paramref name="value"/></summary>
		/// <typeparam name="TEnum">Enumeration type</typeparam>
		/// <param name="name">Element name</param>
		/// <param name="enumValue">value to receive the data</param>
		public void ReadElementEnum<TEnum>(TName name, ref TEnum enumValue)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			Contract.Requires(this.ValidateNameArg(name));

			this.ReadElementEnum(this.GetElement(name), ref enumValue);
		}
		/// <summary>Stream out the InnerText of element <paramref name="name"/> into the enum <paramref name="value"/></summary>
		/// <typeparam name="TEnum">Enumeration type</typeparam>
		/// <param name="name">Element name</param>
		/// <param name="enumValue">value to receive the data</param>
		public void ReadElementEnum<TEnum>(TName name, ref int enumValue)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			Contract.Requires(this.ValidateNameArg(name));

			this.ReadElementEnum(this.GetElement(name), ref enumValue);
		}

		public void ReadElement(TName name, ref Values.KGuid value)
		{
			Contract.Requires(this.ValidateNameArg(name));

			this.ReadElement(this.GetElement(name), ref value);
		}
		#endregion

		#region ReadAttribute
		/// <summary>Stream out the attribute data of <paramref name="name"/> into enum <paramref name="value"/></summary>
		/// <typeparam name="TEnum">Enumeration type</typeparam>
		/// <param name="name">Attribute name</param>
		/// <param name="enumValue">enum value to receive the data</param>
		public abstract void ReadAttributeEnum<TEnum>(TName name, ref TEnum enumValue)
			where TEnum : struct, IComparable, IFormattable, IConvertible;
		/// <summary>Stream out the attribute data of <paramref name="name"/> into enum <paramref name="value"/></summary>
		/// <typeparam name="TEnum">Enumeration type</typeparam>
		/// <param name="name">Attribute name</param>
		/// <param name="enumValue">enum value to receive the data</param>
		public abstract void ReadAttributeEnum<TEnum>(TName name, ref int enumValue)
			where TEnum : struct, IComparable, IFormattable, IConvertible;

		public abstract void ReadAttribute(TName name, ref Values.KGuid value);
		#endregion

		#region ReadElementOpt
		/// <summary>Stream out the InnerText of element <paramref name="name"/> into enum <paramref name="value"/></summary>
		/// <typeparam name="TEnum">Enumeration type</typeparam>
		/// <param name="name">Element name</param>
		/// <param name="enumValue">enum value to receive the data</param>
		/// <remarks>If inner text is just an empty string, the stream ignores its existence</remarks>
		/// <returns>true if the value exists</returns>
		public abstract bool ReadElementEnumOpt<TEnum>(TName name, ref TEnum enumValue)
			where TEnum : struct, IComparable, IFormattable, IConvertible;
		/// <summary>Stream out the InnerText of element <paramref name="name"/> into enum <paramref name="value"/></summary>
		/// <typeparam name="TEnum">Enumeration type</typeparam>
		/// <param name="name">Element name</param>
		/// <param name="enumValue">enum value to receive the data</param>
		/// <remarks>If inner text is just an empty string, the stream ignores its existence</remarks>
		/// <returns>true if the value exists</returns>
		public abstract bool ReadElementEnumOpt<TEnum>(TName name, ref int enumValue)
			where TEnum : struct, IComparable, IFormattable, IConvertible;

		public abstract bool ReadElementOpt(TName name, ref Values.KGuid value);
		#endregion

		#region ReadAttributeOpt
		/// <summary>Stream out the attribute data of <paramref name="name"/> into enum <paramref name="value"/></summary>
		/// <typeparam name="TEnum">Enumeration type</typeparam>
		/// <param name="name">Attribute name</param>
		/// <param name="enumValue">enum value to receive the data</param>
		/// <returns>true if the value exists</returns>
		public abstract bool ReadAttributeEnumOpt<TEnum>(TName name, ref TEnum enumValue)
			where TEnum : struct, IComparable, IFormattable, IConvertible;
		/// <summary>Stream out the attribute data of <paramref name="name"/> into enum <paramref name="value"/></summary>
		/// <typeparam name="TEnum">Enumeration type</typeparam>
		/// <param name="name">Attribute name</param>
		/// <param name="enumValue">enum value to receive the data</param>
		/// <returns>true if the value exists</returns>
		public abstract bool ReadAttributeEnumOpt<TEnum>(TName name, ref int enumValue)
			where TEnum : struct, IComparable, IFormattable, IConvertible;

		public abstract bool ReadAttributeOpt(TName name, ref Values.KGuid value);
		#endregion

		#region ReadElements (ICollection)
		void ReadElements<T, TContext>(IEnumerable<TCursor> elements,
			ICollection<T> coll, TContext ctxt, StreamAction<T, TContext> action, Func<TContext, T> ctor)
		{
			foreach (var node in elements)
				using (this.EnterCursorBookmark(node))
				{
					var value = ctor(ctxt);
					action(this, ctxt, ref value);

					coll.Add(value);
				}
		}
		public void ReadElements<T, TContext>(
			ICollection<T> coll, TContext ctxt, StreamAction<T, TContext> action, Func<TContext, T> ctor)
		{
			Contract.Requires<ArgumentNullException>(coll != null);
			Contract.Requires(action != null);
			Contract.Requires(ctor != null);

			this.ReadElements(this.Elements, coll, ctxt, action, ctor);
		}
		public void ReadElements<T, TContext>(
			ICollection<T> coll, TContext ctxt, StreamAction<T, TContext> action)
			where T : new()
		{
			Contract.Requires<ArgumentNullException>(coll != null);
			Contract.Requires(action != null);

			this.ReadElements(this.Elements, coll, ctxt, action, _ctxt => new T());
		}
		public void ReadElements<T, TContext>(TName name,
			ICollection<T> coll, TContext ctxt, StreamAction<T, TContext> action, Func<TContext, T> ctor)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(coll != null);
			Contract.Requires(action != null);
			Contract.Requires(ctor != null);

			this.ReadElements(this.ElementsByName(name), coll, ctxt, action, ctor);
		}
		public void ReadElements<T, TContext>(TName name,
			ICollection<T> coll, TContext ctxt, StreamAction<T, TContext> action)
			where T : new()
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(coll != null);
			Contract.Requires(action != null);

			this.ReadElements(this.ElementsByName(name), coll, ctxt, action, _ctxt => new T());
		}

		void ReadStreamableElements<T, TContext>(IEnumerable<TCursor> elements,
			ICollection<T> coll, TContext ctxt, Func<TContext, T> ctor)
			where T : ITagElementStreamable<TName>
		{
			foreach (var node in elements)
				using (this.EnterCursorBookmark(node))
				{
					var value = ctor(ctxt);
					value.Serialize(this);

					coll.Add(value);
				}
		}
		public void ReadStreamableElements<T, TContext>(
			ICollection<T> coll, TContext ctxt, Func<TContext, T> ctor)
			where T : ITagElementStreamable<TName>
		{
			Contract.Requires<ArgumentNullException>(coll != null);
			Contract.Requires(ctor != null);

			this.ReadStreamableElements(this.Elements, coll, ctxt, ctor);
		}
		public void ReadStreamableElements<T, TContext>(TName name,
			ICollection<T> coll, TContext ctxt, Func<TContext, T> ctor)
			where T : ITagElementStreamable<TName>
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(coll != null);
			Contract.Requires(ctor != null);

			this.ReadStreamableElements(this.ElementsByName(name), coll, ctxt, ctor);
		}
		#endregion

		#region ReadElements (IDictionary)
		void ReadElements<TKey, TValue, TContext>(IEnumerable<TCursor> elements,
			IDictionary<TKey, TValue> dic, TContext ctxt,
			StreamAction<TKey, TContext> streamKey,
			StreamAction<TValue, TContext> streamValue, Func<TContext, TValue> valueCtor)
		{
			foreach (var node in elements)
				using (this.EnterCursorBookmark(node))
				{
					var key = default(TKey);
					streamKey(this, ctxt, ref key);

					var value = valueCtor(ctxt);
					streamValue(this, ctxt, ref value);

					dic.Add(key, value);
				}
		}
		public void ReadElements<TKey, TValue, TContext>(
			IDictionary<TKey, TValue> dic, TContext ctxt,
			StreamAction<TKey, TContext> streamKey,
			StreamAction<TValue, TContext> streamValue, Func<TContext, TValue> valueCtor)
		{
			Contract.Requires<ArgumentNullException>(dic != null);
			Contract.Requires(streamKey != null);
			Contract.Requires(streamValue != null && valueCtor != null);

			this.ReadElements(this.Elements, dic, ctxt, streamKey, streamValue, valueCtor);
		}
		public void ReadElements<TKey, TValue, TContext>(
			IDictionary<TKey, TValue> dic, TContext ctxt,
			StreamAction<TKey, TContext> streamKey,
			StreamAction<TValue, TContext> streamValue)
			where TValue : new()
		{
			Contract.Requires<ArgumentNullException>(dic != null);
			Contract.Requires(streamKey != null);
			Contract.Requires(streamValue != null);

			this.ReadElements(this.Elements, dic, ctxt, streamKey, streamValue, _ctxt => new TValue());
		}
		public void ReadElements<TKey, TValue, TContext>(TName name,
			IDictionary<TKey, TValue> dic, TContext ctxt,
			StreamAction<TKey, TContext> streamKey,
			StreamAction<TValue, TContext> streamValue, Func<TContext, TValue> valueCtor)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(dic != null);
			Contract.Requires(streamKey != null);
			Contract.Requires(streamValue != null && valueCtor != null);

			this.ReadElements(this.ElementsByName(name), dic, ctxt, streamKey, streamValue, valueCtor);
		}
		public void ReadElements<TKey, TValue, TContext>(TName name,
			IDictionary<TKey, TValue> dic, TContext ctxt,
			StreamAction<TKey, TContext> streamKey,
			StreamAction<TValue, TContext> streamValue)
			where TValue : new()
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(dic != null);
			Contract.Requires(streamKey != null);
			Contract.Requires(streamValue != null);

			this.ReadElements(this.ElementsByName(name), dic, ctxt, streamKey, streamValue, _ctxt => new TValue());
		}

		void ReadStreamableElements<TKey, TValue, TContext>(IEnumerable<TCursor> elements,
			IDictionary<TKey,TValue> dic, TContext ctxt,
			StreamAction<TKey,TContext> streamKey)
			where TValue : ITagElementStreamable<TName>, new()
		{
			foreach (var node in elements)
				using (this.EnterCursorBookmark(node))
				{
					var key = default(TKey);
					streamKey(this, ctxt, ref key);

					var value = new TValue();
					value.Serialize(this);

					dic.Add(key, value);
				}
		}
		public void ReadStreamableElements<TKey, TValue, TContext>(TName name,
			IDictionary<TKey, TValue> dic, TContext ctxt,
			StreamAction<TKey, TContext> streamKey)
			where TValue : ITagElementStreamable<TName>, new()
		{
			Contract.Requires<ArgumentNullException>(dic != null);

			this.ReadStreamableElements(this.ElementsByName(name), dic, ctxt, streamKey);
		}
		#endregion

		#region Read Fixed Array
		int ReadFixedArray<T, TContext>(IEnumerable<TCursor> elements, T[] array,
			TContext ctxt, Func<TContext, T> ctor)
			where T : ITagElementStreamable<TName>
		{
			int count = 0;
			foreach (var node in elements)
			{
				using (this.EnterCursorBookmark(node))
				{
					var value = ctor(ctxt);
					value.Serialize(this);

					array[count++] = value;
				}

				if (count == array.Length) break;
			}

			return count;
		}
		public int ReadFixedArray<T, TContext>(T[] array,
			TContext ctxt, Func<TContext, T> ctor)
			where T : ITagElementStreamable<TName>
		{
			Contract.Requires<ArgumentNullException>(array != null);
			Contract.Requires(ctor != null);

			return this.ReadFixedArray(this.Elements, array, ctxt, ctor);
		}
		public int ReadFixedArray<T, TContext>(TName name, T[] array,
			TContext ctxt, Func<TContext, T> ctor)
			where T : ITagElementStreamable<TName>
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(array != null);
			Contract.Requires(ctor != null);

			return this.ReadFixedArray(this.ElementsByName(name), array, ctxt, ctor);
		}
		#endregion
	};

	partial class TagElementStreamContract<TDoc, TCursor, TName>
	{
		#region ReadElement
		public override void ReadElementBegin(TName name, out TCursor oldCursor)
		{
			Contract.Requires(this.ValidateNameArg(name));

			throw new NotImplementedException();
		}

		public override void ReadElementEnd(ref TCursor oldCursor)
		{
			Contract.Ensures(Contract.ValueAtReturn(out oldCursor) == null);

			throw new NotImplementedException();
		}
		#endregion

		#region ReadAttribute
		public override void ReadAttributeEnum<TEnum>(TName name, ref TEnum enumValue)
		{
			Contract.Requires(this.ValidateNameArg(name));

			throw new NotImplementedException();
		}
		public override void ReadAttributeEnum<TEnum>(TName name, ref int enumValue)
		{
			Contract.Requires(this.ValidateNameArg(name));

			throw new NotImplementedException();
		}

		public override void ReadAttribute(TName name, ref Values.KGuid value)
		{
			Contract.Requires(this.ValidateNameArg(name));

			throw new NotImplementedException();
		}
		#endregion

		#region ReadElementOpt
		public override bool ReadElementEnumOpt<TEnum>(TName name, ref TEnum enumValue)
		{
			Contract.Requires(this.ValidateNameArg(name));

			throw new NotImplementedException();
		}
		public override bool ReadElementEnumOpt<TEnum>(TName name, ref int enumValue)
		{
			Contract.Requires(this.ValidateNameArg(name));

			throw new NotImplementedException();
		}

		public override bool ReadElementOpt(TName name, ref Values.KGuid value)
		{
			Contract.Requires(this.ValidateNameArg(name));

			throw new NotImplementedException();
		}
		#endregion

		#region ReadAttributeOpt
		public override bool ReadAttributeEnumOpt<TEnum>(TName name, ref TEnum enumValue)
		{
			Contract.Requires(this.ValidateNameArg(name));

			throw new NotImplementedException();
		}
		public override bool ReadAttributeEnumOpt<TEnum>(TName name, ref int enumValue)
		{
			Contract.Requires(this.ValidateNameArg(name));

			throw new NotImplementedException();
		}

		public override bool ReadAttributeOpt(TName name, ref Values.KGuid value)
		{
			Contract.Requires(this.ValidateNameArg(name));

			throw new NotImplementedException();
		}
		#endregion
	};
}
