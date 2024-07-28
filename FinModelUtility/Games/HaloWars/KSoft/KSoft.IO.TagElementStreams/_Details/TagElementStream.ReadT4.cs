using System;
using System.Collections.Generic;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.IO
{
	partial class TagElementStream<TDoc, TCursor, TName>
	{
		#region ReadElement impl
		/// <summary>Stream out the InnerText of element <paramref name="name"/> into <paramref name="value"/></summary>
		/// <param name="n">Node element to read</param>
		/// <param name="value">value to receive the data</param>
		protected abstract void ReadElement(TCursor n, ref string value);
		/// <summary>Stream out the InnerText of element <paramref name="name"/> into <paramref name="value"/></summary>
		/// <param name="n">Node element to read</param>
		/// <param name="value">value to receive the data</param>
		protected abstract void ReadElement(TCursor n, ref char value);
		/// <summary>Stream out the InnerText of element <paramref name="name"/> into <paramref name="value"/></summary>
		/// <param name="n">Node element to read</param>
		/// <param name="value">value to receive the data</param>
		protected abstract void ReadElement(TCursor n, ref bool value);
		/// <summary>Stream out the InnerText of element <paramref name="name"/> into <paramref name="value"/></summary>
		/// <param name="n">Node element to read</param>
		/// <param name="value">value to receive the data</param>
		protected abstract void ReadElement(TCursor n, ref float value);
		/// <summary>Stream out the InnerText of element <paramref name="name"/> into <paramref name="value"/></summary>
		/// <param name="n">Node element to read</param>
		/// <param name="value">value to receive the data</param>
		protected abstract void ReadElement(TCursor n, ref double value);

		/// <summary>
		/// Stream out the InnerText of element <paramref name="name"/>
		/// using numerical base of <paramref name="base"/> into
		/// <paramref name="value"/>
		/// </summary>
		/// <param name="n">Node element to read</param>
		/// <param name="fromBase">numerical base to use</param>
		/// <param name="value">value to receive the data</param>
		protected abstract void ReadElement(TCursor n, ref byte value, NumeralBase fromBase);
		/// <summary>
		/// Stream out the InnerText of element <paramref name="name"/>
		/// using numerical base of <paramref name="base"/> into
		/// <paramref name="value"/>
		/// </summary>
		/// <param name="n">Node element to read</param>
		/// <param name="fromBase">numerical base to use</param>
		/// <param name="value">value to receive the data</param>
		protected abstract void ReadElement(TCursor n, ref sbyte value, NumeralBase fromBase);
		/// <summary>
		/// Stream out the InnerText of element <paramref name="name"/>
		/// using numerical base of <paramref name="base"/> into
		/// <paramref name="value"/>
		/// </summary>
		/// <param name="n">Node element to read</param>
		/// <param name="fromBase">numerical base to use</param>
		/// <param name="value">value to receive the data</param>
		protected abstract void ReadElement(TCursor n, ref ushort value, NumeralBase fromBase);
		/// <summary>
		/// Stream out the InnerText of element <paramref name="name"/>
		/// using numerical base of <paramref name="base"/> into
		/// <paramref name="value"/>
		/// </summary>
		/// <param name="n">Node element to read</param>
		/// <param name="fromBase">numerical base to use</param>
		/// <param name="value">value to receive the data</param>
		protected abstract void ReadElement(TCursor n, ref short value, NumeralBase fromBase);
		/// <summary>
		/// Stream out the InnerText of element <paramref name="name"/>
		/// using numerical base of <paramref name="base"/> into
		/// <paramref name="value"/>
		/// </summary>
		/// <param name="n">Node element to read</param>
		/// <param name="fromBase">numerical base to use</param>
		/// <param name="value">value to receive the data</param>
		protected abstract void ReadElement(TCursor n, ref uint value, NumeralBase fromBase);
		/// <summary>
		/// Stream out the InnerText of element <paramref name="name"/>
		/// using numerical base of <paramref name="base"/> into
		/// <paramref name="value"/>
		/// </summary>
		/// <param name="n">Node element to read</param>
		/// <param name="fromBase">numerical base to use</param>
		/// <param name="value">value to receive the data</param>
		protected abstract void ReadElement(TCursor n, ref int value, NumeralBase fromBase);
		/// <summary>
		/// Stream out the InnerText of element <paramref name="name"/>
		/// using numerical base of <paramref name="base"/> into
		/// <paramref name="value"/>
		/// </summary>
		/// <param name="n">Node element to read</param>
		/// <param name="fromBase">numerical base to use</param>
		/// <param name="value">value to receive the data</param>
		protected abstract void ReadElement(TCursor n, ref ulong value, NumeralBase fromBase);
		/// <summary>
		/// Stream out the InnerText of element <paramref name="name"/>
		/// using numerical base of <paramref name="base"/> into
		/// <paramref name="value"/>
		/// </summary>
		/// <param name="n">Node element to read</param>
		/// <param name="fromBase">numerical base to use</param>
		/// <param name="value">value to receive the data</param>
		protected abstract void ReadElement(TCursor n, ref long value, NumeralBase fromBase);
		#endregion


		#region ReadCursor
		/// <summary>Stream out the Value of <see cref="Cursor"/> into <paramref name="value"/></summary>
		/// <param name="value">value to receive the data</param>
		public void ReadCursor(ref string value)
		{
			this.ReadElement(this.Cursor, ref value);
		}
		/// <summary>Stream out the Value of <see cref="Cursor"/> into <paramref name="value"/></summary>
		/// <param name="value">value to receive the data</param>
		public void ReadCursor(ref char value)
		{
			this.ReadElement(this.Cursor, ref value);
		}
		/// <summary>Stream out the Value of <see cref="Cursor"/> into <paramref name="value"/></summary>
		/// <param name="value">value to receive the data</param>
		public void ReadCursor(ref bool value)
		{
			this.ReadElement(this.Cursor, ref value);
		}
		/// <summary>Stream out the Value of <see cref="Cursor"/> into <paramref name="value"/></summary>
		/// <param name="value">value to receive the data</param>
		public void ReadCursor(ref float value)
		{
			this.ReadElement(this.Cursor, ref value);
		}
		/// <summary>Stream out the Value of <see cref="Cursor"/> into <paramref name="value"/></summary>
		/// <param name="value">value to receive the data</param>
		public void ReadCursor(ref double value)
		{
			this.ReadElement(this.Cursor, ref value);
		}

		/// <summary>
		/// Stream out the Value of <see cref="Cursor"/>
		/// using numerical base of <paramref name="base"/> into <paramref name="value"/>
		/// </summary>
		/// <param name="fromBase">numerical base to use</param>
		/// <param name="value">value to receive the data</param>
		public void ReadCursor(ref byte value, NumeralBase fromBase = NumeralBase.Decimal)
		{
			this.ReadElement(this.Cursor, ref value, fromBase);
		}
		/// <summary>
		/// Stream out the Value of <see cref="Cursor"/>
		/// using numerical base of <paramref name="base"/> into <paramref name="value"/>
		/// </summary>
		/// <param name="fromBase">numerical base to use</param>
		/// <param name="value">value to receive the data</param>
		public void ReadCursor(ref sbyte value, NumeralBase fromBase = NumeralBase.Decimal)
		{
			this.ReadElement(this.Cursor, ref value, fromBase);
		}
		/// <summary>
		/// Stream out the Value of <see cref="Cursor"/>
		/// using numerical base of <paramref name="base"/> into <paramref name="value"/>
		/// </summary>
		/// <param name="fromBase">numerical base to use</param>
		/// <param name="value">value to receive the data</param>
		public void ReadCursor(ref ushort value, NumeralBase fromBase = NumeralBase.Decimal)
		{
			this.ReadElement(this.Cursor, ref value, fromBase);
		}
		/// <summary>
		/// Stream out the Value of <see cref="Cursor"/>
		/// using numerical base of <paramref name="base"/> into <paramref name="value"/>
		/// </summary>
		/// <param name="fromBase">numerical base to use</param>
		/// <param name="value">value to receive the data</param>
		public void ReadCursor(ref short value, NumeralBase fromBase = NumeralBase.Decimal)
		{
			this.ReadElement(this.Cursor, ref value, fromBase);
		}
		/// <summary>
		/// Stream out the Value of <see cref="Cursor"/>
		/// using numerical base of <paramref name="base"/> into <paramref name="value"/>
		/// </summary>
		/// <param name="fromBase">numerical base to use</param>
		/// <param name="value">value to receive the data</param>
		public void ReadCursor(ref uint value, NumeralBase fromBase = NumeralBase.Decimal)
		{
			this.ReadElement(this.Cursor, ref value, fromBase);
		}
		/// <summary>
		/// Stream out the Value of <see cref="Cursor"/>
		/// using numerical base of <paramref name="base"/> into <paramref name="value"/>
		/// </summary>
		/// <param name="fromBase">numerical base to use</param>
		/// <param name="value">value to receive the data</param>
		public void ReadCursor(ref int value, NumeralBase fromBase = NumeralBase.Decimal)
		{
			this.ReadElement(this.Cursor, ref value, fromBase);
		}
		/// <summary>
		/// Stream out the Value of <see cref="Cursor"/>
		/// using numerical base of <paramref name="base"/> into <paramref name="value"/>
		/// </summary>
		/// <param name="fromBase">numerical base to use</param>
		/// <param name="value">value to receive the data</param>
		public void ReadCursor(ref ulong value, NumeralBase fromBase = NumeralBase.Decimal)
		{
			this.ReadElement(this.Cursor, ref value, fromBase);
		}
		/// <summary>
		/// Stream out the Value of <see cref="Cursor"/>
		/// using numerical base of <paramref name="base"/> into <paramref name="value"/>
		/// </summary>
		/// <param name="fromBase">numerical base to use</param>
		/// <param name="value">value to receive the data</param>
		public void ReadCursor(ref long value, NumeralBase fromBase = NumeralBase.Decimal)
		{
			this.ReadElement(this.Cursor, ref value, fromBase);
		}
		#endregion


		#region ReadElement
		/// <summary>Stream out the InnerText of element <paramref name="name"/> into <paramref name="value"/></summary>
		/// <param name="name">Element name</param>
		/// <param name="value">value to receive the data</param>
		public void ReadElement(TName name, ref string value)
		{
			Contract.Requires(this.ValidateNameArg(name));

			this.ReadElement(this.GetElement(name), ref value);
		}
		/// <summary>Stream out the InnerText of element <paramref name="name"/> into <paramref name="value"/></summary>
		/// <param name="name">Element name</param>
		/// <param name="value">value to receive the data</param>
		public void ReadElement(TName name, ref char value)
		{
			Contract.Requires(this.ValidateNameArg(name));

			this.ReadElement(this.GetElement(name), ref value);
		}
		/// <summary>Stream out the InnerText of element <paramref name="name"/> into <paramref name="value"/></summary>
		/// <param name="name">Element name</param>
		/// <param name="value">value to receive the data</param>
		public void ReadElement(TName name, ref bool value)
		{
			Contract.Requires(this.ValidateNameArg(name));

			this.ReadElement(this.GetElement(name), ref value);
		}
		/// <summary>Stream out the InnerText of element <paramref name="name"/> into <paramref name="value"/></summary>
		/// <param name="name">Element name</param>
		/// <param name="value">value to receive the data</param>
		public void ReadElement(TName name, ref float value)
		{
			Contract.Requires(this.ValidateNameArg(name));

			this.ReadElement(this.GetElement(name), ref value);
		}
		/// <summary>Stream out the InnerText of element <paramref name="name"/> into <paramref name="value"/></summary>
		/// <param name="name">Element name</param>
		/// <param name="value">value to receive the data</param>
		public void ReadElement(TName name, ref double value)
		{
			Contract.Requires(this.ValidateNameArg(name));

			this.ReadElement(this.GetElement(name), ref value);
		}

		/// <summary>
		/// Stream out the InnerText of element <paramref name="name"/>
		/// using numerical base of <paramref name="base"/> into
		/// <paramref name="value"/>
		/// </summary>
		/// <param name="name">Element name</param>
		/// <param name="fromBase">numerical base to use</param>
		/// <param name="value">value to receive the data</param>
		public void ReadElement(TName name, ref  byte value, NumeralBase fromBase = NumeralBase.Decimal)
		{
			Contract.Requires(this.ValidateNameArg(name));

			this.ReadElement(this.GetElement(name), ref value, fromBase);
		}
		/// <summary>
		/// Stream out the InnerText of element <paramref name="name"/>
		/// using numerical base of <paramref name="base"/> into
		/// <paramref name="value"/>
		/// </summary>
		/// <param name="name">Element name</param>
		/// <param name="fromBase">numerical base to use</param>
		/// <param name="value">value to receive the data</param>
		public void ReadElement(TName name, ref  sbyte value, NumeralBase fromBase = NumeralBase.Decimal)
		{
			Contract.Requires(this.ValidateNameArg(name));

			this.ReadElement(this.GetElement(name), ref value, fromBase);
		}
		/// <summary>
		/// Stream out the InnerText of element <paramref name="name"/>
		/// using numerical base of <paramref name="base"/> into
		/// <paramref name="value"/>
		/// </summary>
		/// <param name="name">Element name</param>
		/// <param name="fromBase">numerical base to use</param>
		/// <param name="value">value to receive the data</param>
		public void ReadElement(TName name, ref  ushort value, NumeralBase fromBase = NumeralBase.Decimal)
		{
			Contract.Requires(this.ValidateNameArg(name));

			this.ReadElement(this.GetElement(name), ref value, fromBase);
		}
		/// <summary>
		/// Stream out the InnerText of element <paramref name="name"/>
		/// using numerical base of <paramref name="base"/> into
		/// <paramref name="value"/>
		/// </summary>
		/// <param name="name">Element name</param>
		/// <param name="fromBase">numerical base to use</param>
		/// <param name="value">value to receive the data</param>
		public void ReadElement(TName name, ref  short value, NumeralBase fromBase = NumeralBase.Decimal)
		{
			Contract.Requires(this.ValidateNameArg(name));

			this.ReadElement(this.GetElement(name), ref value, fromBase);
		}
		/// <summary>
		/// Stream out the InnerText of element <paramref name="name"/>
		/// using numerical base of <paramref name="base"/> into
		/// <paramref name="value"/>
		/// </summary>
		/// <param name="name">Element name</param>
		/// <param name="fromBase">numerical base to use</param>
		/// <param name="value">value to receive the data</param>
		public void ReadElement(TName name, ref  uint value, NumeralBase fromBase = NumeralBase.Decimal)
		{
			Contract.Requires(this.ValidateNameArg(name));

			this.ReadElement(this.GetElement(name), ref value, fromBase);
		}
		/// <summary>
		/// Stream out the InnerText of element <paramref name="name"/>
		/// using numerical base of <paramref name="base"/> into
		/// <paramref name="value"/>
		/// </summary>
		/// <param name="name">Element name</param>
		/// <param name="fromBase">numerical base to use</param>
		/// <param name="value">value to receive the data</param>
		public void ReadElement(TName name, ref  int value, NumeralBase fromBase = NumeralBase.Decimal)
		{
			Contract.Requires(this.ValidateNameArg(name));

			this.ReadElement(this.GetElement(name), ref value, fromBase);
		}
		/// <summary>
		/// Stream out the InnerText of element <paramref name="name"/>
		/// using numerical base of <paramref name="base"/> into
		/// <paramref name="value"/>
		/// </summary>
		/// <param name="name">Element name</param>
		/// <param name="fromBase">numerical base to use</param>
		/// <param name="value">value to receive the data</param>
		public void ReadElement(TName name, ref  ulong value, NumeralBase fromBase = NumeralBase.Decimal)
		{
			Contract.Requires(this.ValidateNameArg(name));

			this.ReadElement(this.GetElement(name), ref value, fromBase);
		}
		/// <summary>
		/// Stream out the InnerText of element <paramref name="name"/>
		/// using numerical base of <paramref name="base"/> into
		/// <paramref name="value"/>
		/// </summary>
		/// <param name="name">Element name</param>
		/// <param name="fromBase">numerical base to use</param>
		/// <param name="value">value to receive the data</param>
		public void ReadElement(TName name, ref  long value, NumeralBase fromBase = NumeralBase.Decimal)
		{
			Contract.Requires(this.ValidateNameArg(name));

			this.ReadElement(this.GetElement(name), ref value, fromBase);
		}
		#endregion


		#region ReadAttribute
		/// <summary>Stream out the attribute data of <paramref name="name"/> into <paramref name="value"/></summary>
		/// <param name="name">Attribute name</param>
		/// <param name="value">value to receive the data</param>
		public abstract void ReadAttribute(TName name, ref string value);
		/// <summary>Stream out the attribute data of <paramref name="name"/> into <paramref name="value"/></summary>
		/// <param name="name">Attribute name</param>
		/// <param name="value">value to receive the data</param>
		public abstract void ReadAttribute(TName name, ref char value);
		/// <summary>Stream out the attribute data of <paramref name="name"/> into <paramref name="value"/></summary>
		/// <param name="name">Attribute name</param>
		/// <param name="value">value to receive the data</param>
		public abstract void ReadAttribute(TName name, ref bool value);
		/// <summary>Stream out the attribute data of <paramref name="name"/> into <paramref name="value"/></summary>
		/// <param name="name">Attribute name</param>
		/// <param name="value">value to receive the data</param>
		public abstract void ReadAttribute(TName name, ref float value);
		/// <summary>Stream out the attribute data of <paramref name="name"/> into <paramref name="value"/></summary>
		/// <param name="name">Attribute name</param>
		/// <param name="value">value to receive the data</param>
		public abstract void ReadAttribute(TName name, ref double value);

		/// <summary>
		/// Stream out the attribute data of <paramref name="name"/>
		/// using numerical base of <paramref name="base"/> into
		/// <paramref name="value"/>
		/// </summary>
		/// <param name="name">Attribute name</param>
		/// <param name="fromBase">numerical base to use</param>
		/// <param name="value">value to receive the data</param>
		public abstract void ReadAttribute(TName name, ref byte value, NumeralBase fromBase = NumeralBase.Decimal);
		/// <summary>
		/// Stream out the attribute data of <paramref name="name"/>
		/// using numerical base of <paramref name="base"/> into
		/// <paramref name="value"/>
		/// </summary>
		/// <param name="name">Attribute name</param>
		/// <param name="fromBase">numerical base to use</param>
		/// <param name="value">value to receive the data</param>
		public abstract void ReadAttribute(TName name, ref sbyte value, NumeralBase fromBase = NumeralBase.Decimal);
		/// <summary>
		/// Stream out the attribute data of <paramref name="name"/>
		/// using numerical base of <paramref name="base"/> into
		/// <paramref name="value"/>
		/// </summary>
		/// <param name="name">Attribute name</param>
		/// <param name="fromBase">numerical base to use</param>
		/// <param name="value">value to receive the data</param>
		public abstract void ReadAttribute(TName name, ref ushort value, NumeralBase fromBase = NumeralBase.Decimal);
		/// <summary>
		/// Stream out the attribute data of <paramref name="name"/>
		/// using numerical base of <paramref name="base"/> into
		/// <paramref name="value"/>
		/// </summary>
		/// <param name="name">Attribute name</param>
		/// <param name="fromBase">numerical base to use</param>
		/// <param name="value">value to receive the data</param>
		public abstract void ReadAttribute(TName name, ref short value, NumeralBase fromBase = NumeralBase.Decimal);
		/// <summary>
		/// Stream out the attribute data of <paramref name="name"/>
		/// using numerical base of <paramref name="base"/> into
		/// <paramref name="value"/>
		/// </summary>
		/// <param name="name">Attribute name</param>
		/// <param name="fromBase">numerical base to use</param>
		/// <param name="value">value to receive the data</param>
		public abstract void ReadAttribute(TName name, ref uint value, NumeralBase fromBase = NumeralBase.Decimal);
		/// <summary>
		/// Stream out the attribute data of <paramref name="name"/>
		/// using numerical base of <paramref name="base"/> into
		/// <paramref name="value"/>
		/// </summary>
		/// <param name="name">Attribute name</param>
		/// <param name="fromBase">numerical base to use</param>
		/// <param name="value">value to receive the data</param>
		public abstract void ReadAttribute(TName name, ref int value, NumeralBase fromBase = NumeralBase.Decimal);
		/// <summary>
		/// Stream out the attribute data of <paramref name="name"/>
		/// using numerical base of <paramref name="base"/> into
		/// <paramref name="value"/>
		/// </summary>
		/// <param name="name">Attribute name</param>
		/// <param name="fromBase">numerical base to use</param>
		/// <param name="value">value to receive the data</param>
		public abstract void ReadAttribute(TName name, ref ulong value, NumeralBase fromBase = NumeralBase.Decimal);
		/// <summary>
		/// Stream out the attribute data of <paramref name="name"/>
		/// using numerical base of <paramref name="base"/> into
		/// <paramref name="value"/>
		/// </summary>
		/// <param name="name">Attribute name</param>
		/// <param name="fromBase">numerical base to use</param>
		/// <param name="value">value to receive the data</param>
		public abstract void ReadAttribute(TName name, ref long value, NumeralBase fromBase = NumeralBase.Decimal);
		#endregion


		#region ReadElementOpt
		/// <summary>Stream out the InnerText of element <paramref name="name"/> into <paramref name="value"/></summary>
		/// <param name="name">Element name</param>
		/// <param name="value">value to receive the data</param>
		/// <remarks>If inner text is just an empty string, the stream ignores its existence</remarks>
		/// <returns>true if the value exists</returns>
		public abstract bool ReadElementOpt(TName name, ref string value);
		/// <summary>Stream out the InnerText of element <paramref name="name"/> into <paramref name="value"/></summary>
		/// <param name="name">Element name</param>
		/// <param name="value">value to receive the data</param>
		/// <remarks>If inner text is just an empty string, the stream ignores its existence</remarks>
		/// <returns>true if the value exists</returns>
		public abstract bool ReadElementOpt(TName name, ref char value);
		/// <summary>Stream out the InnerText of element <paramref name="name"/> into <paramref name="value"/></summary>
		/// <param name="name">Element name</param>
		/// <param name="value">value to receive the data</param>
		/// <remarks>If inner text is just an empty string, the stream ignores its existence</remarks>
		/// <returns>true if the value exists</returns>
		public abstract bool ReadElementOpt(TName name, ref bool value);
		/// <summary>Stream out the InnerText of element <paramref name="name"/> into <paramref name="value"/></summary>
		/// <param name="name">Element name</param>
		/// <param name="value">value to receive the data</param>
		/// <remarks>If inner text is just an empty string, the stream ignores its existence</remarks>
		/// <returns>true if the value exists</returns>
		public abstract bool ReadElementOpt(TName name, ref float value);
		/// <summary>Stream out the InnerText of element <paramref name="name"/> into <paramref name="value"/></summary>
		/// <param name="name">Element name</param>
		/// <param name="value">value to receive the data</param>
		/// <remarks>If inner text is just an empty string, the stream ignores its existence</remarks>
		/// <returns>true if the value exists</returns>
		public abstract bool ReadElementOpt(TName name, ref double value);

		/// <summary>
		/// Stream out the InnerText of element <paramref name="name"/>
		/// using numerical base of <paramref name="base"/> into
		/// <paramref name="value"/>
		/// </summary>
		/// <param name="name">Element name</param>
		/// <param name="fromBase">numerical base to use</param>
		/// <param name="value">value to receive the data</param>
		/// <remarks>If inner text is just an empty string, the stream ignores its existence</remarks>
		/// <returns>true if the value exists</returns>
		public abstract bool ReadElementOpt(TName name, ref byte value, NumeralBase fromBase = NumeralBase.Decimal);
		/// <summary>
		/// Stream out the InnerText of element <paramref name="name"/>
		/// using numerical base of <paramref name="base"/> into
		/// <paramref name="value"/>
		/// </summary>
		/// <param name="name">Element name</param>
		/// <param name="fromBase">numerical base to use</param>
		/// <param name="value">value to receive the data</param>
		/// <remarks>If inner text is just an empty string, the stream ignores its existence</remarks>
		/// <returns>true if the value exists</returns>
		public abstract bool ReadElementOpt(TName name, ref sbyte value, NumeralBase fromBase = NumeralBase.Decimal);
		/// <summary>
		/// Stream out the InnerText of element <paramref name="name"/>
		/// using numerical base of <paramref name="base"/> into
		/// <paramref name="value"/>
		/// </summary>
		/// <param name="name">Element name</param>
		/// <param name="fromBase">numerical base to use</param>
		/// <param name="value">value to receive the data</param>
		/// <remarks>If inner text is just an empty string, the stream ignores its existence</remarks>
		/// <returns>true if the value exists</returns>
		public abstract bool ReadElementOpt(TName name, ref ushort value, NumeralBase fromBase = NumeralBase.Decimal);
		/// <summary>
		/// Stream out the InnerText of element <paramref name="name"/>
		/// using numerical base of <paramref name="base"/> into
		/// <paramref name="value"/>
		/// </summary>
		/// <param name="name">Element name</param>
		/// <param name="fromBase">numerical base to use</param>
		/// <param name="value">value to receive the data</param>
		/// <remarks>If inner text is just an empty string, the stream ignores its existence</remarks>
		/// <returns>true if the value exists</returns>
		public abstract bool ReadElementOpt(TName name, ref short value, NumeralBase fromBase = NumeralBase.Decimal);
		/// <summary>
		/// Stream out the InnerText of element <paramref name="name"/>
		/// using numerical base of <paramref name="base"/> into
		/// <paramref name="value"/>
		/// </summary>
		/// <param name="name">Element name</param>
		/// <param name="fromBase">numerical base to use</param>
		/// <param name="value">value to receive the data</param>
		/// <remarks>If inner text is just an empty string, the stream ignores its existence</remarks>
		/// <returns>true if the value exists</returns>
		public abstract bool ReadElementOpt(TName name, ref uint value, NumeralBase fromBase = NumeralBase.Decimal);
		/// <summary>
		/// Stream out the InnerText of element <paramref name="name"/>
		/// using numerical base of <paramref name="base"/> into
		/// <paramref name="value"/>
		/// </summary>
		/// <param name="name">Element name</param>
		/// <param name="fromBase">numerical base to use</param>
		/// <param name="value">value to receive the data</param>
		/// <remarks>If inner text is just an empty string, the stream ignores its existence</remarks>
		/// <returns>true if the value exists</returns>
		public abstract bool ReadElementOpt(TName name, ref int value, NumeralBase fromBase = NumeralBase.Decimal);
		/// <summary>
		/// Stream out the InnerText of element <paramref name="name"/>
		/// using numerical base of <paramref name="base"/> into
		/// <paramref name="value"/>
		/// </summary>
		/// <param name="name">Element name</param>
		/// <param name="fromBase">numerical base to use</param>
		/// <param name="value">value to receive the data</param>
		/// <remarks>If inner text is just an empty string, the stream ignores its existence</remarks>
		/// <returns>true if the value exists</returns>
		public abstract bool ReadElementOpt(TName name, ref ulong value, NumeralBase fromBase = NumeralBase.Decimal);
		/// <summary>
		/// Stream out the InnerText of element <paramref name="name"/>
		/// using numerical base of <paramref name="base"/> into
		/// <paramref name="value"/>
		/// </summary>
		/// <param name="name">Element name</param>
		/// <param name="fromBase">numerical base to use</param>
		/// <param name="value">value to receive the data</param>
		/// <remarks>If inner text is just an empty string, the stream ignores its existence</remarks>
		/// <returns>true if the value exists</returns>
		public abstract bool ReadElementOpt(TName name, ref long value, NumeralBase fromBase = NumeralBase.Decimal);
		#endregion


		#region ReadAttributeOpt
		/// <summary>Stream out the attribute data of <paramref name="name"/> into <paramref name="value"/></summary>
		/// <param name="name">Attribute name</param>
		/// <param name="value">value to receive the data</param>
		/// <returns>true if the value exists</returns>
		public abstract bool ReadAttributeOpt(TName name, ref string value);
		/// <summary>Stream out the attribute data of <paramref name="name"/> into <paramref name="value"/></summary>
		/// <param name="name">Attribute name</param>
		/// <param name="value">value to receive the data</param>
		/// <returns>true if the value exists</returns>
		public abstract bool ReadAttributeOpt(TName name, ref char value);
		/// <summary>Stream out the attribute data of <paramref name="name"/> into <paramref name="value"/></summary>
		/// <param name="name">Attribute name</param>
		/// <param name="value">value to receive the data</param>
		/// <returns>true if the value exists</returns>
		public abstract bool ReadAttributeOpt(TName name, ref bool value);
		/// <summary>Stream out the attribute data of <paramref name="name"/> into <paramref name="value"/></summary>
		/// <param name="name">Attribute name</param>
		/// <param name="value">value to receive the data</param>
		/// <returns>true if the value exists</returns>
		public abstract bool ReadAttributeOpt(TName name, ref float value);
		/// <summary>Stream out the attribute data of <paramref name="name"/> into <paramref name="value"/></summary>
		/// <param name="name">Attribute name</param>
		/// <param name="value">value to receive the data</param>
		/// <returns>true if the value exists</returns>
		public abstract bool ReadAttributeOpt(TName name, ref double value);

		/// <summary>
		/// Stream out the attribute data of <paramref name="name"/>
		/// using numerical base of <paramref name="base"/> into
		/// <paramref name="value"/>
		/// </summary>
		/// <param name="name">Attribute name</param>
		/// <param name="fromBase">numerical base to use</param>
		/// <param name="value">value to receive the data</param>
		/// <returns>true if the value exists</returns>
		public abstract bool ReadAttributeOpt(TName name, ref byte value, NumeralBase fromBase = NumeralBase.Decimal);
		/// <summary>
		/// Stream out the attribute data of <paramref name="name"/>
		/// using numerical base of <paramref name="base"/> into
		/// <paramref name="value"/>
		/// </summary>
		/// <param name="name">Attribute name</param>
		/// <param name="fromBase">numerical base to use</param>
		/// <param name="value">value to receive the data</param>
		/// <returns>true if the value exists</returns>
		public abstract bool ReadAttributeOpt(TName name, ref sbyte value, NumeralBase fromBase = NumeralBase.Decimal);
		/// <summary>
		/// Stream out the attribute data of <paramref name="name"/>
		/// using numerical base of <paramref name="base"/> into
		/// <paramref name="value"/>
		/// </summary>
		/// <param name="name">Attribute name</param>
		/// <param name="fromBase">numerical base to use</param>
		/// <param name="value">value to receive the data</param>
		/// <returns>true if the value exists</returns>
		public abstract bool ReadAttributeOpt(TName name, ref ushort value, NumeralBase fromBase = NumeralBase.Decimal);
		/// <summary>
		/// Stream out the attribute data of <paramref name="name"/>
		/// using numerical base of <paramref name="base"/> into
		/// <paramref name="value"/>
		/// </summary>
		/// <param name="name">Attribute name</param>
		/// <param name="fromBase">numerical base to use</param>
		/// <param name="value">value to receive the data</param>
		/// <returns>true if the value exists</returns>
		public abstract bool ReadAttributeOpt(TName name, ref short value, NumeralBase fromBase = NumeralBase.Decimal);
		/// <summary>
		/// Stream out the attribute data of <paramref name="name"/>
		/// using numerical base of <paramref name="base"/> into
		/// <paramref name="value"/>
		/// </summary>
		/// <param name="name">Attribute name</param>
		/// <param name="fromBase">numerical base to use</param>
		/// <param name="value">value to receive the data</param>
		/// <returns>true if the value exists</returns>
		public abstract bool ReadAttributeOpt(TName name, ref uint value, NumeralBase fromBase = NumeralBase.Decimal);
		/// <summary>
		/// Stream out the attribute data of <paramref name="name"/>
		/// using numerical base of <paramref name="base"/> into
		/// <paramref name="value"/>
		/// </summary>
		/// <param name="name">Attribute name</param>
		/// <param name="fromBase">numerical base to use</param>
		/// <param name="value">value to receive the data</param>
		/// <returns>true if the value exists</returns>
		public abstract bool ReadAttributeOpt(TName name, ref int value, NumeralBase fromBase = NumeralBase.Decimal);
		/// <summary>
		/// Stream out the attribute data of <paramref name="name"/>
		/// using numerical base of <paramref name="base"/> into
		/// <paramref name="value"/>
		/// </summary>
		/// <param name="name">Attribute name</param>
		/// <param name="fromBase">numerical base to use</param>
		/// <param name="value">value to receive the data</param>
		/// <returns>true if the value exists</returns>
		public abstract bool ReadAttributeOpt(TName name, ref ulong value, NumeralBase fromBase = NumeralBase.Decimal);
		/// <summary>
		/// Stream out the attribute data of <paramref name="name"/>
		/// using numerical base of <paramref name="base"/> into
		/// <paramref name="value"/>
		/// </summary>
		/// <param name="name">Attribute name</param>
		/// <param name="fromBase">numerical base to use</param>
		/// <param name="value">value to receive the data</param>
		/// <returns>true if the value exists</returns>
		public abstract bool ReadAttributeOpt(TName name, ref long value, NumeralBase fromBase = NumeralBase.Decimal);
		#endregion


		#region ReadElements
		void ReadElements(IEnumerable<TCursor> elements, ICollection< string > coll)
		{
			foreach (var node in elements)
				using (this.EnterCursorBookmark(node))
				{
					var value = default(string);
					this.ReadCursor(ref value);

					coll.Add(value);
				}
		}
		public void ReadElements(ICollection< string > coll)
		{
			Contract.Requires<ArgumentNullException>(coll != null);

			this.ReadElements(this.Elements, coll);
		}
		public void ReadElements(TName name, ICollection< string > coll)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(coll != null);

			this.ReadElements(this.ElementsByName(name), coll);
		}

		void ReadElements(IEnumerable<TCursor> elements, ICollection< char > coll)
		{
			foreach (var node in elements)
				using (this.EnterCursorBookmark(node))
				{
					var value = default(char);
					this.ReadCursor(ref value);

					coll.Add(value);
				}
		}
		public void ReadElements(ICollection< char > coll)
		{
			Contract.Requires<ArgumentNullException>(coll != null);

			this.ReadElements(this.Elements, coll);
		}
		public void ReadElements(TName name, ICollection< char > coll)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(coll != null);

			this.ReadElements(this.ElementsByName(name), coll);
		}

		void ReadElements(IEnumerable<TCursor> elements, ICollection< bool > coll)
		{
			foreach (var node in elements)
				using (this.EnterCursorBookmark(node))
				{
					var value = default(bool);
					this.ReadCursor(ref value);

					coll.Add(value);
				}
		}
		public void ReadElements(ICollection< bool > coll)
		{
			Contract.Requires<ArgumentNullException>(coll != null);

			this.ReadElements(this.Elements, coll);
		}
		public void ReadElements(TName name, ICollection< bool > coll)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(coll != null);

			this.ReadElements(this.ElementsByName(name), coll);
		}

		void ReadElements(IEnumerable<TCursor> elements, ICollection< float > coll)
		{
			foreach (var node in elements)
				using (this.EnterCursorBookmark(node))
				{
					var value = default(float);
					this.ReadCursor(ref value);

					coll.Add(value);
				}
		}
		public void ReadElements(ICollection< float > coll)
		{
			Contract.Requires<ArgumentNullException>(coll != null);

			this.ReadElements(this.Elements, coll);
		}
		public void ReadElements(TName name, ICollection< float > coll)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(coll != null);

			this.ReadElements(this.ElementsByName(name), coll);
		}

		void ReadElements(IEnumerable<TCursor> elements, ICollection< double > coll)
		{
			foreach (var node in elements)
				using (this.EnterCursorBookmark(node))
				{
					var value = default(double);
					this.ReadCursor(ref value);

					coll.Add(value);
				}
		}
		public void ReadElements(ICollection< double > coll)
		{
			Contract.Requires<ArgumentNullException>(coll != null);

			this.ReadElements(this.Elements, coll);
		}
		public void ReadElements(TName name, ICollection< double > coll)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(coll != null);

			this.ReadElements(this.ElementsByName(name), coll);
		}


		void ReadElements(IEnumerable<TCursor> elements, ICollection< byte > coll, NumeralBase fromBase = NumeralBase.Decimal)
		{
			foreach (var node in elements)
				using (this.EnterCursorBookmark(node))
				{
					byte value = 0;
					this.ReadCursor(ref value, fromBase);

					coll.Add(value);
				}
		}
		public void ReadElements(ICollection< byte > coll, NumeralBase fromBase = NumeralBase.Decimal)
		{
			Contract.Requires<ArgumentNullException>(coll != null);

			this.ReadElements(this.Elements, coll, fromBase);
		}
		public void ReadElements(TName name, ICollection< byte > coll, NumeralBase fromBase = NumeralBase.Decimal)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(coll != null);

			this.ReadElements(this.ElementsByName(name), coll, fromBase);
		}

		void ReadElements(IEnumerable<TCursor> elements, ICollection< sbyte > coll, NumeralBase fromBase = NumeralBase.Decimal)
		{
			foreach (var node in elements)
				using (this.EnterCursorBookmark(node))
				{
					sbyte value = 0;
					this.ReadCursor(ref value, fromBase);

					coll.Add(value);
				}
		}
		public void ReadElements(ICollection< sbyte > coll, NumeralBase fromBase = NumeralBase.Decimal)
		{
			Contract.Requires<ArgumentNullException>(coll != null);

			this.ReadElements(this.Elements, coll, fromBase);
		}
		public void ReadElements(TName name, ICollection< sbyte > coll, NumeralBase fromBase = NumeralBase.Decimal)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(coll != null);

			this.ReadElements(this.ElementsByName(name), coll, fromBase);
		}

		void ReadElements(IEnumerable<TCursor> elements, ICollection< ushort > coll, NumeralBase fromBase = NumeralBase.Decimal)
		{
			foreach (var node in elements)
				using (this.EnterCursorBookmark(node))
				{
					ushort value = 0;
					this.ReadCursor(ref value, fromBase);

					coll.Add(value);
				}
		}
		public void ReadElements(ICollection< ushort > coll, NumeralBase fromBase = NumeralBase.Decimal)
		{
			Contract.Requires<ArgumentNullException>(coll != null);

			this.ReadElements(this.Elements, coll, fromBase);
		}
		public void ReadElements(TName name, ICollection< ushort > coll, NumeralBase fromBase = NumeralBase.Decimal)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(coll != null);

			this.ReadElements(this.ElementsByName(name), coll, fromBase);
		}

		void ReadElements(IEnumerable<TCursor> elements, ICollection< short > coll, NumeralBase fromBase = NumeralBase.Decimal)
		{
			foreach (var node in elements)
				using (this.EnterCursorBookmark(node))
				{
					short value = 0;
					this.ReadCursor(ref value, fromBase);

					coll.Add(value);
				}
		}
		public void ReadElements(ICollection< short > coll, NumeralBase fromBase = NumeralBase.Decimal)
		{
			Contract.Requires<ArgumentNullException>(coll != null);

			this.ReadElements(this.Elements, coll, fromBase);
		}
		public void ReadElements(TName name, ICollection< short > coll, NumeralBase fromBase = NumeralBase.Decimal)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(coll != null);

			this.ReadElements(this.ElementsByName(name), coll, fromBase);
		}

		void ReadElements(IEnumerable<TCursor> elements, ICollection< uint > coll, NumeralBase fromBase = NumeralBase.Decimal)
		{
			foreach (var node in elements)
				using (this.EnterCursorBookmark(node))
				{
					uint value = 0;
					this.ReadCursor(ref value, fromBase);

					coll.Add(value);
				}
		}
		public void ReadElements(ICollection< uint > coll, NumeralBase fromBase = NumeralBase.Decimal)
		{
			Contract.Requires<ArgumentNullException>(coll != null);

			this.ReadElements(this.Elements, coll, fromBase);
		}
		public void ReadElements(TName name, ICollection< uint > coll, NumeralBase fromBase = NumeralBase.Decimal)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(coll != null);

			this.ReadElements(this.ElementsByName(name), coll, fromBase);
		}

		void ReadElements(IEnumerable<TCursor> elements, ICollection< int > coll, NumeralBase fromBase = NumeralBase.Decimal)
		{
			foreach (var node in elements)
				using (this.EnterCursorBookmark(node))
				{
					int value = 0;
					this.ReadCursor(ref value, fromBase);

					coll.Add(value);
				}
		}
		public void ReadElements(ICollection< int > coll, NumeralBase fromBase = NumeralBase.Decimal)
		{
			Contract.Requires<ArgumentNullException>(coll != null);

			this.ReadElements(this.Elements, coll, fromBase);
		}
		public void ReadElements(TName name, ICollection< int > coll, NumeralBase fromBase = NumeralBase.Decimal)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(coll != null);

			this.ReadElements(this.ElementsByName(name), coll, fromBase);
		}

		void ReadElements(IEnumerable<TCursor> elements, ICollection< ulong > coll, NumeralBase fromBase = NumeralBase.Decimal)
		{
			foreach (var node in elements)
				using (this.EnterCursorBookmark(node))
				{
					ulong value = 0;
					this.ReadCursor(ref value, fromBase);

					coll.Add(value);
				}
		}
		public void ReadElements(ICollection< ulong > coll, NumeralBase fromBase = NumeralBase.Decimal)
		{
			Contract.Requires<ArgumentNullException>(coll != null);

			this.ReadElements(this.Elements, coll, fromBase);
		}
		public void ReadElements(TName name, ICollection< ulong > coll, NumeralBase fromBase = NumeralBase.Decimal)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(coll != null);

			this.ReadElements(this.ElementsByName(name), coll, fromBase);
		}

		void ReadElements(IEnumerable<TCursor> elements, ICollection< long > coll, NumeralBase fromBase = NumeralBase.Decimal)
		{
			foreach (var node in elements)
				using (this.EnterCursorBookmark(node))
				{
					long value = 0;
					this.ReadCursor(ref value, fromBase);

					coll.Add(value);
				}
		}
		public void ReadElements(ICollection< long > coll, NumeralBase fromBase = NumeralBase.Decimal)
		{
			Contract.Requires<ArgumentNullException>(coll != null);

			this.ReadElements(this.Elements, coll, fromBase);
		}
		public void ReadElements(TName name, ICollection< long > coll, NumeralBase fromBase = NumeralBase.Decimal)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(coll != null);

			this.ReadElements(this.ElementsByName(name), coll, fromBase);
		}

		#endregion


		#region ReadFixedArray
		int ReadFixedArray(IEnumerable<TCursor> elements, string[] array)
		{
			int count = 0;
			foreach (var node in elements)
			{
				using (this.EnterCursorBookmark(node))
					this.ReadCursor(ref array[count++]);

				if(count == array.Length)
					break;
			}

			return count;
		}
		public int ReadFixedArray(string[] array)
		{
			Contract.Requires<ArgumentNullException>(array != null);

			return this.ReadFixedArray(this.Elements, array);
		}
		public int ReadFixedArray(TName name, string[] array)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(array != null);

			return this.ReadFixedArray(this.ElementsByName(name), array);
		}

		int ReadFixedArray(IEnumerable<TCursor> elements, char[] array)
		{
			int count = 0;
			foreach (var node in elements)
			{
				using (this.EnterCursorBookmark(node))
					this.ReadCursor(ref array[count++]);

				if(count == array.Length)
					break;
			}

			return count;
		}
		public int ReadFixedArray(char[] array)
		{
			Contract.Requires<ArgumentNullException>(array != null);

			return this.ReadFixedArray(this.Elements, array);
		}
		public int ReadFixedArray(TName name, char[] array)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(array != null);

			return this.ReadFixedArray(this.ElementsByName(name), array);
		}

		int ReadFixedArray(IEnumerable<TCursor> elements, bool[] array)
		{
			int count = 0;
			foreach (var node in elements)
			{
				using (this.EnterCursorBookmark(node))
					this.ReadCursor(ref array[count++]);

				if(count == array.Length)
					break;
			}

			return count;
		}
		public int ReadFixedArray(bool[] array)
		{
			Contract.Requires<ArgumentNullException>(array != null);

			return this.ReadFixedArray(this.Elements, array);
		}
		public int ReadFixedArray(TName name, bool[] array)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(array != null);

			return this.ReadFixedArray(this.ElementsByName(name), array);
		}

		int ReadFixedArray(IEnumerable<TCursor> elements, float[] array)
		{
			int count = 0;
			foreach (var node in elements)
			{
				using (this.EnterCursorBookmark(node))
					this.ReadCursor(ref array[count++]);

				if(count == array.Length)
					break;
			}

			return count;
		}
		public int ReadFixedArray(float[] array)
		{
			Contract.Requires<ArgumentNullException>(array != null);

			return this.ReadFixedArray(this.Elements, array);
		}
		public int ReadFixedArray(TName name, float[] array)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(array != null);

			return this.ReadFixedArray(this.ElementsByName(name), array);
		}

		int ReadFixedArray(IEnumerable<TCursor> elements, double[] array)
		{
			int count = 0;
			foreach (var node in elements)
			{
				using (this.EnterCursorBookmark(node))
					this.ReadCursor(ref array[count++]);

				if(count == array.Length)
					break;
			}

			return count;
		}
		public int ReadFixedArray(double[] array)
		{
			Contract.Requires<ArgumentNullException>(array != null);

			return this.ReadFixedArray(this.Elements, array);
		}
		public int ReadFixedArray(TName name, double[] array)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(array != null);

			return this.ReadFixedArray(this.ElementsByName(name), array);
		}


		int ReadFixedArray(IEnumerable<TCursor> elements, byte[] array, NumeralBase fromBase = NumeralBase.Decimal)
		{
			int count = 0;
			foreach (var node in elements)
			{
				using (this.EnterCursorBookmark(node))
					this.ReadCursor(ref array[count++], fromBase);

				if(count == array.Length)
					break;
			}

			return count;
		}
		public int ReadFixedArray(byte[] array, NumeralBase fromBase = NumeralBase.Decimal)
		{
			Contract.Requires<ArgumentNullException>(array != null);

			return this.ReadFixedArray(this.Elements, array, fromBase);
		}
		public int ReadFixedArray(TName name, byte[] array, NumeralBase fromBase = NumeralBase.Decimal)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(array != null);

			return this.ReadFixedArray(this.ElementsByName(name), array, fromBase);
		}

		int ReadFixedArray(IEnumerable<TCursor> elements, sbyte[] array, NumeralBase fromBase = NumeralBase.Decimal)
		{
			int count = 0;
			foreach (var node in elements)
			{
				using (this.EnterCursorBookmark(node))
					this.ReadCursor(ref array[count++], fromBase);

				if(count == array.Length)
					break;
			}

			return count;
		}
		public int ReadFixedArray(sbyte[] array, NumeralBase fromBase = NumeralBase.Decimal)
		{
			Contract.Requires<ArgumentNullException>(array != null);

			return this.ReadFixedArray(this.Elements, array, fromBase);
		}
		public int ReadFixedArray(TName name, sbyte[] array, NumeralBase fromBase = NumeralBase.Decimal)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(array != null);

			return this.ReadFixedArray(this.ElementsByName(name), array, fromBase);
		}

		int ReadFixedArray(IEnumerable<TCursor> elements, ushort[] array, NumeralBase fromBase = NumeralBase.Decimal)
		{
			int count = 0;
			foreach (var node in elements)
			{
				using (this.EnterCursorBookmark(node))
					this.ReadCursor(ref array[count++], fromBase);

				if(count == array.Length)
					break;
			}

			return count;
		}
		public int ReadFixedArray(ushort[] array, NumeralBase fromBase = NumeralBase.Decimal)
		{
			Contract.Requires<ArgumentNullException>(array != null);

			return this.ReadFixedArray(this.Elements, array, fromBase);
		}
		public int ReadFixedArray(TName name, ushort[] array, NumeralBase fromBase = NumeralBase.Decimal)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(array != null);

			return this.ReadFixedArray(this.ElementsByName(name), array, fromBase);
		}

		int ReadFixedArray(IEnumerable<TCursor> elements, short[] array, NumeralBase fromBase = NumeralBase.Decimal)
		{
			int count = 0;
			foreach (var node in elements)
			{
				using (this.EnterCursorBookmark(node))
					this.ReadCursor(ref array[count++], fromBase);

				if(count == array.Length)
					break;
			}

			return count;
		}
		public int ReadFixedArray(short[] array, NumeralBase fromBase = NumeralBase.Decimal)
		{
			Contract.Requires<ArgumentNullException>(array != null);

			return this.ReadFixedArray(this.Elements, array, fromBase);
		}
		public int ReadFixedArray(TName name, short[] array, NumeralBase fromBase = NumeralBase.Decimal)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(array != null);

			return this.ReadFixedArray(this.ElementsByName(name), array, fromBase);
		}

		int ReadFixedArray(IEnumerable<TCursor> elements, uint[] array, NumeralBase fromBase = NumeralBase.Decimal)
		{
			int count = 0;
			foreach (var node in elements)
			{
				using (this.EnterCursorBookmark(node))
					this.ReadCursor(ref array[count++], fromBase);

				if(count == array.Length)
					break;
			}

			return count;
		}
		public int ReadFixedArray(uint[] array, NumeralBase fromBase = NumeralBase.Decimal)
		{
			Contract.Requires<ArgumentNullException>(array != null);

			return this.ReadFixedArray(this.Elements, array, fromBase);
		}
		public int ReadFixedArray(TName name, uint[] array, NumeralBase fromBase = NumeralBase.Decimal)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(array != null);

			return this.ReadFixedArray(this.ElementsByName(name), array, fromBase);
		}

		int ReadFixedArray(IEnumerable<TCursor> elements, int[] array, NumeralBase fromBase = NumeralBase.Decimal)
		{
			int count = 0;
			foreach (var node in elements)
			{
				using (this.EnterCursorBookmark(node))
					this.ReadCursor(ref array[count++], fromBase);

				if(count == array.Length)
					break;
			}

			return count;
		}
		public int ReadFixedArray(int[] array, NumeralBase fromBase = NumeralBase.Decimal)
		{
			Contract.Requires<ArgumentNullException>(array != null);

			return this.ReadFixedArray(this.Elements, array, fromBase);
		}
		public int ReadFixedArray(TName name, int[] array, NumeralBase fromBase = NumeralBase.Decimal)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(array != null);

			return this.ReadFixedArray(this.ElementsByName(name), array, fromBase);
		}

		int ReadFixedArray(IEnumerable<TCursor> elements, ulong[] array, NumeralBase fromBase = NumeralBase.Decimal)
		{
			int count = 0;
			foreach (var node in elements)
			{
				using (this.EnterCursorBookmark(node))
					this.ReadCursor(ref array[count++], fromBase);

				if(count == array.Length)
					break;
			}

			return count;
		}
		public int ReadFixedArray(ulong[] array, NumeralBase fromBase = NumeralBase.Decimal)
		{
			Contract.Requires<ArgumentNullException>(array != null);

			return this.ReadFixedArray(this.Elements, array, fromBase);
		}
		public int ReadFixedArray(TName name, ulong[] array, NumeralBase fromBase = NumeralBase.Decimal)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(array != null);

			return this.ReadFixedArray(this.ElementsByName(name), array, fromBase);
		}

		int ReadFixedArray(IEnumerable<TCursor> elements, long[] array, NumeralBase fromBase = NumeralBase.Decimal)
		{
			int count = 0;
			foreach (var node in elements)
			{
				using (this.EnterCursorBookmark(node))
					this.ReadCursor(ref array[count++], fromBase);

				if(count == array.Length)
					break;
			}

			return count;
		}
		public int ReadFixedArray(long[] array, NumeralBase fromBase = NumeralBase.Decimal)
		{
			Contract.Requires<ArgumentNullException>(array != null);

			return this.ReadFixedArray(this.Elements, array, fromBase);
		}
		public int ReadFixedArray(TName name, long[] array, NumeralBase fromBase = NumeralBase.Decimal)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(array != null);

			return this.ReadFixedArray(this.ElementsByName(name), array, fromBase);
		}

		#endregion
	};


	partial class TagElementStreamContract<TDoc, TCursor, TName>
	{
		#region ReadAttribute
		public override void ReadAttribute(TName name, ref string value)
		{ Contract.Requires(this.ValidateNameArg(name)); throw new NotImplementedException(); }
		public override void ReadAttribute(TName name, ref char value)
		{ Contract.Requires(this.ValidateNameArg(name)); throw new NotImplementedException(); }
		public override void ReadAttribute(TName name, ref bool value)
		{ Contract.Requires(this.ValidateNameArg(name)); throw new NotImplementedException(); }
		public override void ReadAttribute(TName name, ref float value)
		{ Contract.Requires(this.ValidateNameArg(name)); throw new NotImplementedException(); }
		public override void ReadAttribute(TName name, ref double value)
		{ Contract.Requires(this.ValidateNameArg(name)); throw new NotImplementedException(); }

		public override void ReadAttribute(TName name, ref byte value, NumeralBase fromBase)
		{ Contract.Requires(this.ValidateNameArg(name)); throw new NotImplementedException(); }
		public override void ReadAttribute(TName name, ref sbyte value, NumeralBase fromBase)
		{ Contract.Requires(this.ValidateNameArg(name)); throw new NotImplementedException(); }
		public override void ReadAttribute(TName name, ref ushort value, NumeralBase fromBase)
		{ Contract.Requires(this.ValidateNameArg(name)); throw new NotImplementedException(); }
		public override void ReadAttribute(TName name, ref short value, NumeralBase fromBase)
		{ Contract.Requires(this.ValidateNameArg(name)); throw new NotImplementedException(); }
		public override void ReadAttribute(TName name, ref uint value, NumeralBase fromBase)
		{ Contract.Requires(this.ValidateNameArg(name)); throw new NotImplementedException(); }
		public override void ReadAttribute(TName name, ref int value, NumeralBase fromBase)
		{ Contract.Requires(this.ValidateNameArg(name)); throw new NotImplementedException(); }
		public override void ReadAttribute(TName name, ref ulong value, NumeralBase fromBase)
		{ Contract.Requires(this.ValidateNameArg(name)); throw new NotImplementedException(); }
		public override void ReadAttribute(TName name, ref long value, NumeralBase fromBase)
		{ Contract.Requires(this.ValidateNameArg(name)); throw new NotImplementedException(); }
		#endregion


		#region ReadElementOpt
		public override bool ReadElementOpt(TName name, ref string value)
		{ Contract.Requires(this.ValidateNameArg(name)); throw new NotImplementedException(); }
		public override bool ReadElementOpt(TName name, ref char value)
		{ Contract.Requires(this.ValidateNameArg(name)); throw new NotImplementedException(); }
		public override bool ReadElementOpt(TName name, ref bool value)
		{ Contract.Requires(this.ValidateNameArg(name)); throw new NotImplementedException(); }
		public override bool ReadElementOpt(TName name, ref float value)
		{ Contract.Requires(this.ValidateNameArg(name)); throw new NotImplementedException(); }
		public override bool ReadElementOpt(TName name, ref double value)
		{ Contract.Requires(this.ValidateNameArg(name)); throw new NotImplementedException(); }

		public override bool ReadElementOpt(TName name, ref byte value, NumeralBase fromBase)
		{ Contract.Requires(this.ValidateNameArg(name)); throw new NotImplementedException(); }
		public override bool ReadElementOpt(TName name, ref sbyte value, NumeralBase fromBase)
		{ Contract.Requires(this.ValidateNameArg(name)); throw new NotImplementedException(); }
		public override bool ReadElementOpt(TName name, ref ushort value, NumeralBase fromBase)
		{ Contract.Requires(this.ValidateNameArg(name)); throw new NotImplementedException(); }
		public override bool ReadElementOpt(TName name, ref short value, NumeralBase fromBase)
		{ Contract.Requires(this.ValidateNameArg(name)); throw new NotImplementedException(); }
		public override bool ReadElementOpt(TName name, ref uint value, NumeralBase fromBase)
		{ Contract.Requires(this.ValidateNameArg(name)); throw new NotImplementedException(); }
		public override bool ReadElementOpt(TName name, ref int value, NumeralBase fromBase)
		{ Contract.Requires(this.ValidateNameArg(name)); throw new NotImplementedException(); }
		public override bool ReadElementOpt(TName name, ref ulong value, NumeralBase fromBase)
		{ Contract.Requires(this.ValidateNameArg(name)); throw new NotImplementedException(); }
		public override bool ReadElementOpt(TName name, ref long value, NumeralBase fromBase)
		{ Contract.Requires(this.ValidateNameArg(name)); throw new NotImplementedException(); }
		#endregion


		#region ReadAttributeOpt
		public override bool ReadAttributeOpt(TName name, ref string value)
		{ Contract.Requires(this.ValidateNameArg(name)); throw new NotImplementedException(); }
		public override bool ReadAttributeOpt(TName name, ref char value)
		{ Contract.Requires(this.ValidateNameArg(name)); throw new NotImplementedException(); }
		public override bool ReadAttributeOpt(TName name, ref bool value)
		{ Contract.Requires(this.ValidateNameArg(name)); throw new NotImplementedException(); }
		public override bool ReadAttributeOpt(TName name, ref float value)
		{ Contract.Requires(this.ValidateNameArg(name)); throw new NotImplementedException(); }
		public override bool ReadAttributeOpt(TName name, ref double value)
		{ Contract.Requires(this.ValidateNameArg(name)); throw new NotImplementedException(); }

		public override bool ReadAttributeOpt(TName name, ref byte value, NumeralBase fromBase)
		{ Contract.Requires(this.ValidateNameArg(name)); throw new NotImplementedException(); }
		public override bool ReadAttributeOpt(TName name, ref sbyte value, NumeralBase fromBase)
		{ Contract.Requires(this.ValidateNameArg(name)); throw new NotImplementedException(); }
		public override bool ReadAttributeOpt(TName name, ref ushort value, NumeralBase fromBase)
		{ Contract.Requires(this.ValidateNameArg(name)); throw new NotImplementedException(); }
		public override bool ReadAttributeOpt(TName name, ref short value, NumeralBase fromBase)
		{ Contract.Requires(this.ValidateNameArg(name)); throw new NotImplementedException(); }
		public override bool ReadAttributeOpt(TName name, ref uint value, NumeralBase fromBase)
		{ Contract.Requires(this.ValidateNameArg(name)); throw new NotImplementedException(); }
		public override bool ReadAttributeOpt(TName name, ref int value, NumeralBase fromBase)
		{ Contract.Requires(this.ValidateNameArg(name)); throw new NotImplementedException(); }
		public override bool ReadAttributeOpt(TName name, ref ulong value, NumeralBase fromBase)
		{ Contract.Requires(this.ValidateNameArg(name)); throw new NotImplementedException(); }
		public override bool ReadAttributeOpt(TName name, ref long value, NumeralBase fromBase)
		{ Contract.Requires(this.ValidateNameArg(name)); throw new NotImplementedException(); }
		#endregion
	};
};