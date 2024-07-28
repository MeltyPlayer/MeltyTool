using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif
using Exprs = System.Linq.Expressions;

namespace KSoft.IO
{
	partial class TagElementStream<TDoc, TCursor, TName>
	{
		public delegate void StreamAction<T, TContext>(TagElementStream<TDoc, TCursor, TName> s, TContext ctxt, ref T value);
		public delegate void StreamActionNoContext<T>(TagElementStream<TDoc, TCursor, TName> s, ref T value);

		#region Stream Cursor
		/// <summary>Stream the Value of <see cref="Cursor"/> to or from <paramref name="value"/></summary>
		/// <typeparam name="TEnum">Enumeration type</typeparam>
		/// <param name="value">Source or destination value</param>
		/// <param name="isFlags">Is <paramref name="value"/> a <see cref="FlagsAttribute"/> based Enum?</param>
		/// <seealso cref="ReadCursor{TEnum}(TName, ref TEnum)"/>
		/// <seealso cref="WriteCursor(TName, Enum, bool)"/>
		public void StreamCursorEnum<TEnum>(ref TEnum value, bool isFlags = false)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
				 if (this.IsReading)
					 this.ReadCursorEnum(ref value);
			else if (this.IsWriting)
				this.WriteCursorEnum(value, isFlags);
		}

		public void StreamCursor(ref Values.KGuid value)
		{
				 if (this.IsReading)
					 this.ReadCursor(ref value);
			else if (this.IsWriting)
				this.WriteCursor(value);
		}

		/// <summary>Stream the Value of attribute <paramref name="Cursor"/> and process it from a string to an id</summary>
		/// <typeparam name="TContext">Resolving context</typeparam>
		/// <typeparam name="TIdentifer">Type representing an id</typeparam>
		/// <param name="id">Source or destination of the postprocessed value</param>
		/// <param name="ctxt">Resolving context</param>
		/// <param name="idResolver">string to id resolver</param>
		/// <param name="stringResolver">id to string resolver</param>
		public void StreamCursorIdAsString<TContext, TIdentifer>(ref TIdentifer id,
			TContext ctxt,
			Func<TContext, string, TIdentifer> idResolver,
			Func<TContext, TIdentifer, string> stringResolver)
		{
			Contract.Requires(idResolver != null && stringResolver != null);

			bool reading = this.IsReading;
			string str = reading
				? null
				: stringResolver(ctxt, id);

			this.StreamCursor(ref str);

			if (reading)
				id = idResolver(ctxt, str);
		}

		public void StreamCursor(ref DateTime timestamp)
		{
			if (this.IsReading)
			{
				long time64 = 0;
				this.ReadCursor(ref time64, NumeralBase.Hex);
				timestamp = Util.ConvertDateTimeFromUnixTime(time64);
			}
			else if (this.IsWriting)
			{
				long time64 = Util.ConvertDateTimeToUnixTime(timestamp);
				this.WriteCursor(time64, NumeralBase.Hex);
			}
		}
		#endregion

		#region Stream Element
		public void StreamElementBegin(TName name, out TCursor oldCursor)
		{
			Contract.Requires(this.StreamMode != 0, "StreamMode not set! This is an error while trying to actually stream");

			oldCursor = null;

				 if (this.IsReading)
					 this.ReadElementBegin(name, out oldCursor);
			else if (this.IsWriting)
				this.WriteElementBegin(name, out oldCursor);

			Contract.Assert(oldCursor != null);
		}
		public void StreamElementEnd(ref TCursor oldCursor)
		{
				 if (this.IsReading)
					 this.ReadElementEnd(ref oldCursor);
			else if (this.IsWriting)
				this.WriteElementEnd(ref oldCursor);
		}

		/// <summary>Stream the Value of element <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <typeparam name="TEnum">Enumeration type</typeparam>
		/// <param name="name">Element name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="isFlags">Is <paramref name="value"/> a <see cref="FlagsAttribute"/> based Enum?</param>
		/// <seealso cref="ReadElement{TEnum}(TName, ref TEnum)"/>
		/// <seealso cref="WriteElement(TName, Enum, bool)"/>
		public void StreamElementEnum<TEnum>(TName name, ref TEnum value, bool isFlags = false)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			Contract.Requires(this.ValidateNameArg(name));

				 if (this.IsReading)
					 this.ReadElementEnum(name, ref value);
			else if (this.IsWriting)
				this.WriteElementEnum(name, value, isFlags);
		}
		public void StreamElementEnum<T, TEnum>(TName name, T theObj, Exprs.Expression<Func<T, TEnum>> propExpr,
			bool isFlags = false)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			Contract.Requires(this.ValidateNameArg(name));

			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (this.IsReading)
			{
				var value = default( TEnum );
				this.ReadElementEnum(name, ref value);
				property.SetValue(theObj, value, null);
			}
			else if (this.IsWriting)
				this.WriteElementEnum(name, (TEnum)property.GetValue(theObj, null), isFlags);
		}

		public void StreamElement(TName name, ref Values.KGuid value)
		{
			Contract.Requires(this.ValidateNameArg(name));

				 if (this.IsReading)
					 this.ReadElement(name, ref value);
			else if (this.IsWriting)
				this.WriteElement(name, value);
		}

		/// <summary>Stream the Value of element <paramref name="name"/> and process it from a string to an id</summary>
		/// <typeparam name="TContext">Resolving context</typeparam>
		/// <typeparam name="TIdentifer">Type representing an id</typeparam>
		/// <param name="name">Attribute name</param>
		/// <param name="id">Source or destination of the postprocessed value</param>
		/// <param name="ctxt">Resolving context</param>
		/// <param name="idResolver">string to id resolver</param>
		/// <param name="stringResolver">id to string resolver</param>
		public void StreamElementIdAsString<TContext, TIdentifer>(TName name, ref TIdentifer id,
			TContext ctxt,
			Func<TContext, string, TIdentifer> idResolver,
			Func<TContext, TIdentifer, string> stringResolver)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires(idResolver != null && stringResolver != null);

			bool reading = this.IsReading;
			string str = reading
				? null
				: stringResolver(ctxt, id);

			this.StreamElement(name, ref str);

			if (reading)
				id = idResolver(ctxt, str);
		}

		public void StreamElement(TName name, ref DateTime timestamp)
		{
			Contract.Requires(this.ValidateNameArg(name));

			if (this.IsReading)
			{
				long time64 = 0;
				this.ReadElement(name, ref time64, NumeralBase.Hex);
				timestamp = Util.ConvertDateTimeFromUnixTime(time64);
			}
			else if (this.IsWriting)
			{
				long time64 = Util.ConvertDateTimeToUnixTime(timestamp);
				this.WriteElement(name, time64, NumeralBase.Hex);
			}
		}
		#endregion

		#region StreamElementOpt
		/// <summary>Stream the Value of element <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <typeparam name="TEnum">Enumeration type</typeparam>
		/// <param name="name">Element name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <returns>True if <paramref name="value"/> was read/written from/to stream</returns>
		/// <param name="isFlags">Is <paramref name="value"/> a <see cref="FlagsElement"/> based Enum?</param>
		/// <seealso cref="ReadElementOpt{TEnum}(TName, ref TEnum)"/>
		/// <seealso cref="WriteElementOptOnTrue{TEnum}(TName, Enum, Predicate{TEnum}, bool)"/>
		public bool StreamElementEnumOpt<TEnum>(TName name, ref TEnum value, Predicate<TEnum> predicate = null, bool isFlags = false)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			Contract.Requires(this.ValidateNameArg(name));

			if (predicate == null)
				predicate = Predicates.True<TEnum>;

			bool executed = false;
				 if (this.IsReading) executed = this.ReadElementEnumOpt(name, ref value);
			else if (this.IsWriting) executed = this.WriteElementEnumOptOnTrue(name, value, predicate, isFlags);
			return executed;
		}
		public bool StreamElementEnumOpt<T, TEnum>(TName name, T theObj, Exprs.Expression<Func<T, TEnum>> propExpr,
			Predicate<TEnum> predicate = null, bool isFlags = false)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			Contract.Requires(this.ValidateNameArg(name));

			if (predicate == null)
				predicate = Predicates.True<TEnum>;

			bool executed = false;
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (this.IsReading)
			{
				var value = default( TEnum );
				executed = this.ReadElementEnumOpt(name, ref value);
				property.SetValue(theObj, value, null);
			}
			else if (this.IsWriting)
				executed = this.WriteElementEnumOptOnTrue(name, (TEnum)property.GetValue(theObj, null), predicate, isFlags);

			return executed;
		}

		public bool StreamElementOpt(TName name, ref Values.KGuid value, Predicate<Values.KGuid> predicate = null)
		{
			Contract.Requires(this.ValidateNameArg(name));

			if (predicate == null)
				predicate = Predicates.True<Values.KGuid>;

			bool executed = false;
				 if (this.IsReading) executed = this.ReadElementOpt(name, ref value);
			else if (this.IsWriting) executed = this.WriteElementOptOnTrue(name, value, predicate);
			return executed;
		}

		/// <summary>Stream the Value of element <paramref name="name"/> and process it from a string to an id</summary>
		/// <typeparam name="TContext">Resolving context</typeparam>
		/// <typeparam name="TIdentifer">Type representing an id</typeparam>
		/// <param name="name">Attribute name</param>
		/// <param name="id">Source or destination of the postprocessed value</param>
		/// <param name="ctxt">Resolving context</param>
		/// <param name="idResolver">string to id resolver</param>
		/// <param name="stringResolver">id to string resolver</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <returns>True if <paramref name="value"/> was read/written from/to stream</returns>
		public bool StreamElementOptIdAsString<TContext, TIdentifer>(TName name, ref TIdentifer id,
			TContext ctxt,
			Func<TContext, string, TIdentifer> idResolver,
			Func<TContext, TIdentifer, string> stringResolver,
			Predicate<string> predicate = null)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires(idResolver != null && stringResolver != null);

			bool reading = this.IsReading;
			string str = reading
				? null
				: stringResolver(ctxt, id);

			bool executed = this.StreamElementOpt(name, ref str, predicate);

			if (reading && executed)
				id = idResolver(ctxt, str);

			return executed;
		}

		public bool StreamElementOpt(TName name, ref DateTime timestamp, Predicate<DateTime> predicate = null)
		{
			Contract.Requires(this.ValidateNameArg(name));

			if (predicate == null)
				predicate = Predicates.True<DateTime>;

			bool executed = false;
			if (this.IsReading)
			{
				long time64 = 0;
				executed = this.ReadElementOpt(name, ref time64, NumeralBase.Hex);
				timestamp = Util.ConvertDateTimeFromUnixTime(time64);
			}
			else if (this.IsWriting)
			{
				executed = predicate(timestamp);
				if (executed)
				{
					long time64 = Util.ConvertDateTimeToUnixTime(timestamp);
					this.WriteElement(name, time64, NumeralBase.Hex);
				}
			}
			return executed;
		}
		#endregion

		#region Stream Attribute
		/// <summary>Stream the Value of attribute <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <typeparam name="TEnum">Enumeration type</typeparam>
		/// <param name="name">Attribute name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="isFlags">Is <paramref name="value"/> a <see cref="FlagsAttribute"/> based Enum?</param>
		/// <seealso cref="ReadElement{TEnum}(string, ref TEnum)"/>
		/// <seealso cref="WriteAttribute(string, Enum, bool)"/>
		public void StreamAttributeEnum<TEnum>(TName name, ref TEnum value, bool isFlags = false)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			Contract.Requires(this.ValidateNameArg(name));

				 if (this.IsReading)
					 this.ReadAttributeEnum(name, ref value);
			else if (this.IsWriting)
				this.WriteAttributeEnum(name, value, isFlags);
		}
		public void StreamAttributeEnum<T, TEnum>(TName name, T theObj, Exprs.Expression<Func<T, TEnum>> propExpr,
			bool isFlags = false)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			Contract.Requires(this.ValidateNameArg(name));

			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (this.IsReading)
			{
				var value = default( TEnum );
				this.ReadAttributeEnum(name, ref value);
				property.SetValue(theObj, value, null);
			}
			else if (this.IsWriting)
				this.WriteAttributeEnum(name, (TEnum)property.GetValue(theObj, null), isFlags);
		}

		public void StreamAttribute(TName name, ref Values.KGuid value)
		{
			Contract.Requires(this.ValidateNameArg(name));

				 if (this.IsReading)
					 this.ReadAttribute(name, ref value);
			else if (this.IsWriting)
				this.WriteAttribute(name, value);
		}

		public void StreamAttribute(TName name, ref DateTime timestamp)
		{
			Contract.Requires(this.ValidateNameArg(name));

			if (this.IsReading)
			{
				long time64=0;
				this.ReadAttribute(name, ref time64, NumeralBase.Hex);
				timestamp = Util.ConvertDateTimeFromUnixTime(time64);
			}
			else if (this.IsWriting)
			{
				long time64 = Util.ConvertDateTimeToUnixTime(timestamp);
				this.WriteAttribute(name, time64, NumeralBase.Hex);
			}
		}
		#endregion

		#region StreamAttributeOpt
		/// <summary>Stream the Value of attribute <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <typeparam name="TEnum">Enumeration type</typeparam>
		/// <param name="name">Attribute name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <returns>True if <paramref name="value"/> was read/written from/to stream</returns>
		/// <param name="isFlags">Is <paramref name="value"/> a <see cref="FlagsAttribute"/> based Enum?</param>
		/// <seealso cref="ReadAttributeOpt{TEnum}(string, ref TEnum)"/>
		/// <seealso cref="WriteAttributeOptOnTrue{TEnum}(string, Enum, Predicate{TEnum}, bool)"/>
		public bool StreamAttributeEnumOpt<TEnum>(TName name, ref TEnum value, Predicate<TEnum> predicate = null, bool isFlags = false)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			Contract.Requires(this.ValidateNameArg(name));

			if (predicate == null)
				predicate = Predicates.True<TEnum>;

			bool executed = false;
				 if (this.IsReading) executed = this.ReadAttributeEnumOpt(name, ref value);
			else if (this.IsWriting) executed = this.WriteAttributeEnumOptOnTrue(name, value, predicate, isFlags);
			return executed;
		}
		public bool StreamAttributeEnumOpt<T, TEnum>(TName name, T theObj, Exprs.Expression<Func<T, TEnum>> propExpr,
			Predicate<TEnum> predicate = null, bool isFlags = false)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			Contract.Requires(this.ValidateNameArg(name));

			if (predicate == null)
				predicate = Predicates.True<TEnum>;

			bool executed = false;
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (this.IsReading)
			{
				var value = default( TEnum );
				executed = this.ReadAttributeEnumOpt(name, ref value);
				property.SetValue(theObj, value, null);
			}
			else if (this.IsWriting)
				executed = this.WriteAttributeEnumOptOnTrue(name, (TEnum)property.GetValue(theObj, null) , predicate, isFlags);

			return executed;
		}

		public bool StreamAttributeOpt(TName name, ref Values.KGuid value, Predicate<Values.KGuid> predicate = null)
		{
			Contract.Requires(this.ValidateNameArg(name));

			if (predicate == null)
				predicate = Predicates.True<Values.KGuid>;

			bool executed = false;
				 if (this.IsReading) executed = this.ReadAttributeOpt(name, ref value);
			else if (this.IsWriting) executed = this.WriteAttributeOptOnTrue(name, value, predicate);
			return executed;
		}

		public bool StreamAttributeOpt(TName name, ref DateTime timestamp, Predicate<DateTime> predicate = null)
		{
			Contract.Requires(this.ValidateNameArg(name));

			if (predicate == null)
				predicate = Predicates.True<DateTime>;

			bool executed = false;
			if (this.IsReading)
			{
				long time64 = 0;
				executed = this.ReadAttributeOpt(name, ref time64, NumeralBase.Hex);
				timestamp = Util.ConvertDateTimeFromUnixTime(time64);
			}
			else if (this.IsWriting)
			{
				executed = predicate(timestamp);
				if (executed)
				{
					long time64 = Util.ConvertDateTimeToUnixTime(timestamp);
					this.WriteAttribute(name, time64, NumeralBase.Hex);
				}
			}
			return executed;
		}
		#endregion

		// #TODO: T4
		// #TODO: wrap idResolver calls in try/catch then throw anything via ThrowReadException
		#region StreamAttributeId
		/// <summary>Stream the Value of attribute <paramref name="name"/> and process it from a string to an id</summary>
		/// <typeparam name="TContext">Resolving context</typeparam>
		/// <typeparam name="TIdentifer">Type representing an id</typeparam>
		/// <param name="name">Attribute name</param>
		/// <param name="id">Source or destination of the postprocessed value</param>
		/// <param name="ctxt">Resolving context</param>
		/// <param name="idResolver">string to id resolver</param>
		/// <param name="integerResolver">id to string resolver</param>
		public void StreamAttributeIdAsInt32<TContext, TIdentifer>(TName name, ref TIdentifer id,
			TContext ctxt,
			Func<TContext, int, TIdentifer> idResolver,
			Func<TContext, TIdentifer, int> integerResolver)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires(idResolver != null && integerResolver != null);

			bool reading = this.IsReading;
			var integer = reading
				? TypeExtensions.kNone
				: integerResolver(ctxt, id);

			this.StreamAttribute(name, ref integer);

			if (reading)
				id = idResolver(ctxt, integer);
		}

		/// <summary>Stream the Value of attribute <paramref name="name"/> and process it from a string to an id</summary>
		/// <typeparam name="TContext">Resolving context</typeparam>
		/// <typeparam name="TIdentifer">Type representing an id</typeparam>
		/// <param name="name">Attribute name</param>
		/// <param name="id">Source or destination of the postprocessed value</param>
		/// <param name="ctxt">Resolving context</param>
		/// <param name="idResolver">string to id resolver</param>
		/// <param name="stringResolver">id to string resolver</param>
		public void StreamAttributeIdAsString<TContext, TIdentifer>(TName name, ref TIdentifer id,
			TContext ctxt,
			Func<TContext, string, TIdentifer> idResolver,
			Func<TContext, TIdentifer, string> stringResolver)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires(idResolver != null && stringResolver != null);

			bool reading = this.IsReading;
			string str = reading
				? null
				: stringResolver(ctxt, id);

			this.StreamAttribute(name, ref str);

			if (reading)
				id = idResolver(ctxt, str);
		}
		#endregion

		// #TODO: T4
		// #TODO: wrap idResolver calls in try/catch then throw anything via ThrowReadException
		#region StreamAttributeOptId
		/// <summary>Stream the Value of attribute <paramref name="name"/> and process it from a integer to an id</summary>
		/// <typeparam name="TContext">Resolving context</typeparam>
		/// <typeparam name="TIdentifer">Type representing an id</typeparam>
		/// <param name="name">Attribute name</param>
		/// <param name="id">Source or destination of the postprocessed value</param>
		/// <param name="ctxt">Resolving context</param>
		/// <param name="idResolver">integer to id resolver</param>
		/// <param name="integerResolver">id to integer resolver</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <returns>True if <paramref name="value"/> was read/written from/to stream</returns>
		public bool StreamAttributeOptIdAsInt32<TContext, TIdentifer>(TName name, ref TIdentifer id,
			TContext ctxt,
			Func<TContext, int, TIdentifer> idResolver,
			Func<TContext, TIdentifer, int> integerResolver,
			Predicate<int> predicate = null)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires(idResolver != null && integerResolver != null);

			bool reading = this.IsReading;
			var integer = reading
				? TypeExtensions.kNone
				: integerResolver(ctxt, id);

			bool executed = this.StreamAttributeOpt(name, ref integer, predicate);

			if (reading && executed)
				id = idResolver(ctxt, integer);

			return executed;
		}

		/// <summary>Stream the Value of attribute <paramref name="name"/> and process it from a string to an id</summary>
		/// <typeparam name="TContext">Resolving context</typeparam>
		/// <typeparam name="TIdentifer">Type representing an id</typeparam>
		/// <param name="name">Attribute name</param>
		/// <param name="id">Source or destination of the postprocessed value</param>
		/// <param name="ctxt">Resolving context</param>
		/// <param name="idResolver">string to id resolver</param>
		/// <param name="stringResolver">id to string resolver</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <returns>True if <paramref name="value"/> was read/written from/to stream</returns>
		public bool StreamAttributeOptIdAsString<TContext, TIdentifer>(TName name, ref TIdentifer id,
			TContext ctxt,
			Func<TContext, string, TIdentifer> idResolver,
			Func<TContext, TIdentifer, string> stringResolver,
			Predicate<string> predicate = null)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires(idResolver != null && stringResolver != null);

			bool reading = this.IsReading;
			string str = reading
				? null
				: stringResolver(ctxt, id);

			bool executed = this.StreamAttributeOpt(name, ref str, predicate);

			if (reading && executed)
				id = idResolver(ctxt, str);

			return executed;
		}
		#endregion

		#region StreamElements (ICollection)
		public void StreamElements<T, TContext>(TName elementName,
			ICollection<T> coll, TContext ctxt, StreamAction<T, TContext> action, Func<TContext, T> ctor)
		{
			Contract.Requires(this.ValidateNameArg(elementName));
			Contract.Requires<ArgumentNullException>(coll != null);
			Contract.Requires(action != null);
			Contract.Requires(ctor != null);

				 if (this.IsReading)
					 this.ReadElements(elementName, coll, ctxt, action, ctor);
			else if (this.IsWriting)
				this.WriteElements(elementName, coll, ctxt, action);
		}
		public void StreamElements<T, TContext>(TName elementName,
			ICollection<T> coll, TContext ctxt, StreamAction<T, TContext> action)
			where T : new()
		{
			Contract.Requires(this.ValidateNameArg(elementName));
			Contract.Requires<ArgumentNullException>(coll != null);
			Contract.Requires(action != null);

				 if (this.IsReading)
					 this.ReadElements(elementName, coll, ctxt, action);
			else if (this.IsWriting)
				this.WriteElements(elementName, coll, ctxt, action);
		}
		public void StreamElements<T, TContext>(TName elementName,
			ICollection<T> coll, TContext ctxt,
			StreamAction<T, TContext> read, StreamAction<T, TContext> write)
			where T : new()
		{
			Contract.Requires(this.ValidateNameArg(elementName));
			Contract.Requires<ArgumentNullException>(coll != null);
			Contract.Requires(read != null && write != null);

				 if (this.IsReading)
					 this.ReadElements(elementName, coll, ctxt, read);
			else if (this.IsWriting)
				this.WriteElements(elementName, coll, ctxt, write);
		}

		public void StreamableElements<T, TContext>(TName elementName,
			ICollection<T> coll, TContext ctxt, Func<TContext, T> ctor,
			Predicate<T> shouldWritePredicate = null)
			where T : ITagElementStreamable<TName>
		{
			Contract.Requires(this.ValidateNameArg(elementName));
			Contract.Requires<ArgumentNullException>(coll != null);
			Contract.Requires(ctor != null);

				 if (this.IsReading)
					 this.ReadStreamableElements(elementName, coll, ctxt, ctor);
			else if (this.IsWriting)
				this.WriteStreamableElements(elementName, coll, shouldWritePredicate);
		}
		public void StreamableElements<T>(TName elementName,
			ICollection<T> coll,
			Predicate<T> shouldWritePredicate = null)
			where T : ITagElementStreamable<TName>, new()
		{
			Contract.Requires(this.ValidateNameArg(elementName));
			Contract.Requires<ArgumentNullException>(coll != null);

			this.StreamableElements(elementName, coll, (object)null, (nil) => new T(), shouldWritePredicate);
		}
		#endregion

		#region StreamElements (IDictionary)
		public void StreamElements<TKey, TValue, TContext>(TName elementName,
			IDictionary<TKey, TValue> dic, TContext ctxt,
			StreamAction<TKey, TContext> streamKey,
			StreamAction<TValue, TContext> streamValue, Func<TContext, TValue> valueCtor)
		{
			Contract.Requires(this.ValidateNameArg(elementName));
			Contract.Requires<ArgumentNullException>(dic != null);
			Contract.Requires(streamKey != null);
			Contract.Requires(streamValue != null && valueCtor != null);

				 if (this.IsReading)
					 this.ReadElements(elementName, dic, ctxt, streamKey, streamValue, valueCtor);
			else if (this.IsWriting)
				this.WriteElements(elementName, dic, ctxt, streamKey, streamValue);
		}
		public void StreamElements<TKey, TValue, TContext>(TName elementName,
			IDictionary<TKey, TValue> dic, TContext ctxt,
			StreamAction<TKey, TContext> streamKey,
			StreamAction<TValue, TContext> streamValue)
			where TValue : new()
		{
			Contract.Requires(this.ValidateNameArg(elementName));
			Contract.Requires<ArgumentNullException>(dic != null);
			Contract.Requires(streamKey != null);
			Contract.Requires(streamValue != null);

				 if (this.IsReading)
					 this.ReadElements(elementName, dic, ctxt, streamKey, streamValue);
			else if (this.IsWriting)
				this.WriteElements(elementName, dic, ctxt, streamKey, streamValue);
		}

		public void StreamableElements<TKey, TValue, TContext>(TName elementName,
			IDictionary<TKey, TValue> dic, TContext ctxt,
			StreamAction<TKey, TContext> streamKey,
			Predicate<KeyValuePair<TKey, TValue>> shouldWritePredicate = null)
			where TValue : ITagElementStreamable<TName>, new()
		{
			Contract.Requires(this.ValidateNameArg(elementName));
			Contract.Requires<ArgumentNullException>(dic != null);

				 if (this.IsReading)
					 this.ReadStreamableElements(elementName, dic, ctxt, streamKey);
			else if (this.IsWriting)
				this.WriteStreamableElements(elementName, dic, ctxt, streamKey, shouldWritePredicate);
		}
		public void StreamableElements<TKey, TValue>(TName elementName,
			IDictionary<TKey, TValue> dic,
			StreamAction<TKey, object> streamKey,
			Predicate<KeyValuePair<TKey, TValue>> shouldWritePredicate = null)
			where TValue : ITagElementStreamable<TName>, new()
		{
			Contract.Requires(this.ValidateNameArg(elementName));
			Contract.Requires<ArgumentNullException>(dic != null);

			this.StreamableElements(elementName, dic, (object)null, streamKey, shouldWritePredicate);
		}
		#endregion

		#region Stream Fixed Array
		public int StreamableFixedArray<T, TContext>(TName elementName, T[] array,
			TContext ctxt, Func<TContext, T> ctor)
			where T : ITagElementStreamable<TName>
		{
			Contract.Requires(this.ValidateNameArg(elementName));
			Contract.Requires<ArgumentNullException>(array != null);
			Contract.Requires(ctor != null);

			if (this.IsReading) return this.ReadFixedArray(elementName, array, ctxt, ctor);
			else if (this.IsWriting)
				this.WriteStreamableElements(elementName, array);

			return array.Length;
		}
		public int StreamableFixedArray<T>(TName elementName, T[] array)
			where T : ITagElementStreamable<TName>, new()
		{
			Contract.Requires(this.ValidateNameArg(elementName));
			Contract.Requires<ArgumentNullException>(array != null);

			return this.StreamableFixedArray(elementName, array, (object)null, (nil) => new T());
		}
		#endregion

		#region Stream Values
		public void StreamValue<T>(ref T value)
			where T : struct, ITagElementStreamable<TName>
		{
			value.Serialize(this);
		}
		public void StreamValue<T>(ref T value, Func<T> initializer)
			where T : struct, ITagElementStreamable<TName>
		{
			Contract.Requires(initializer != null);

			if (this.IsReading)
				value = initializer();

			value.Serialize(this);
		}
		#endregion

		#region Stream Objects
		public void StreamObject<T>(T theObj)
			where T : class, ITagElementStreamable<TName>
		{
			Contract.Requires(theObj != null);

			theObj.Serialize(this);
		}
		public void StreamObject<T>(T theObj, Func<T> initializer)
			where T : class, ITagElementStreamable<TName>
		{
			Contract.Requires(this.IsReading || theObj != null);
			Contract.Requires(initializer != null);

			if (this.IsReading)
				theObj = initializer();

			theObj.Serialize(this);
		}
		#endregion

		#region Stream Version (int)
		public void StreamVersionViaCursor(int expectedVersion, string dataDescription)
		{
			Contract.Requires(!string.IsNullOrEmpty(dataDescription));

			var data = expectedVersion;
			this.StreamCursor(ref data);
			if (data != expectedVersion)
				throw new VersionMismatchException(dataDescription, expectedVersion, data);
		}
		public void StreamVersionViaElement(TName elementName, int expectedVersion, string dataDescription)
		{
			Contract.Requires(!string.IsNullOrEmpty(dataDescription));
			Contract.Requires(this.ValidateNameArg(elementName));

			var data = expectedVersion;
			this.StreamElement(elementName, ref data);
			if (data != expectedVersion)
				throw new VersionMismatchException(dataDescription, expectedVersion, data);
		}
		public void StreamVersionViaAttribute(TName attributeName, int expectedVersion, string dataDescription)
		{
			Contract.Requires(!string.IsNullOrEmpty(dataDescription));
			Contract.Requires(this.ValidateNameArg(attributeName));

			var data = expectedVersion;
			this.StreamAttribute(attributeName, ref data);
			if (data != expectedVersion)
				throw new VersionMismatchException(dataDescription, expectedVersion, data);
		}
		#endregion

		#region Stream Version (uint)
		public void StreamVersionViaCursor(uint expectedVersion, string dataDescription)
		{
			Contract.Requires(!string.IsNullOrEmpty(dataDescription));

			var data = expectedVersion;
			this.StreamCursor(ref data);
			if (data != expectedVersion)
				throw new VersionMismatchException(dataDescription, expectedVersion, data);
		}
		public void StreamVersionViaElement(TName elementName, uint expectedVersion, string dataDescription)
		{
			Contract.Requires(!string.IsNullOrEmpty(dataDescription));
			Contract.Requires(this.ValidateNameArg(elementName));

			var data = expectedVersion;
			this.StreamElement(elementName, ref data);
			if (data != expectedVersion)
				throw new VersionMismatchException(dataDescription, expectedVersion, data);
		}
		public void StreamVersionViaAttribute(TName attributeName, uint expectedVersion, string dataDescription)
		{
			Contract.Requires(!string.IsNullOrEmpty(dataDescription));
			Contract.Requires(this.ValidateNameArg(attributeName));

			var data = expectedVersion;
			this.StreamAttribute(attributeName, ref data);
			if (data != expectedVersion)
				throw new VersionMismatchException(dataDescription, expectedVersion, data);
		}
		#endregion

		#region Stream Signature (string)
		public void StreamSignatureViaCursor(string expectedSignature, string dataDescription)
		{
			Contract.Requires(!string.IsNullOrEmpty(dataDescription));

			var data = expectedSignature;
			this.StreamCursor(ref data);
			if (data != expectedSignature)
				throw new SignatureMismatchException(dataDescription, expectedSignature, data);
		}
		public void StreamSignatureViaElement(TName elementName, string expectedSignature, string dataDescription)
		{
			Contract.Requires(!string.IsNullOrEmpty(dataDescription));
			Contract.Requires(this.ValidateNameArg(elementName));

			var data = expectedSignature;
			this.StreamElement(elementName, ref data);
			if (data != expectedSignature)
				throw new SignatureMismatchException(dataDescription, expectedSignature, data);
		}
		public void StreamSignatureViaAttribute(TName attributeName, string expectedSignature, string dataDescription)
		{
			Contract.Requires(!string.IsNullOrEmpty(dataDescription));
			Contract.Requires(this.ValidateNameArg(attributeName));

			var data = expectedSignature;
			this.StreamAttribute(attributeName, ref data);
			if (data != expectedSignature)
				throw new SignatureMismatchException(dataDescription, expectedSignature, data);
		}
		#endregion

		[SuppressMessage("Microsoft.Design", "CA1308:NormalizeStringsToUppercase")]
		public void StreamString(TName name, ref string value, bool toLower,
			TagElementNodeType type = TagElementNodeType.Attribute, bool intern = false)
		{
			Contract.Requires(type.RequiresName() == (name != null));

				 if (type == TagElementNodeType.Element)
					 this.StreamElement(name, ref value);
			else if (type == TagElementNodeType.Attribute)
				this.StreamAttribute(name, ref value);
			else if (type == TagElementNodeType.Text)
				this.StreamCursor(ref value);

			if (this.IsReading)
			{
				if (toLower) value = value.ToLowerInvariant();
				if (intern) value = string.Intern(value);
			}
		}
		[SuppressMessage("Microsoft.Design", "CA1308:NormalizeStringsToUppercase")]
		public bool StreamStringOpt(TName name, ref string value, bool toLower,
			TagElementNodeType type = TagElementNodeType.Attribute, bool intern = false)
		{
			Contract.Requires(type.RequiresName() == (name != null));

			bool result = true;
				 if (type == TagElementNodeType.Element)	result = this.StreamElementOpt(name, ref value, Predicates.IsNotNullOrEmpty);
			else if (type == TagElementNodeType.Attribute)	result = this.StreamAttributeOpt(name, ref value, Predicates.IsNotNullOrEmpty);
			else if (type == TagElementNodeType.Text)
				this.StreamCursor(ref value);

			if (this.IsReading && result)
			{
				if (toLower) value = value.ToLowerInvariant();
				if (intern) value = string.Intern(value);
			}

			return result;
		}

		public bool StreamElementNamedFlag(TName name, ref bool value)
		{
			if (this.IsReading) value = this.ElementsExists(name);
			else if (this.IsWriting && value)
				this.WriteElement(name);

			return value;
		}
	};
}
