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
		#region WriteElement impl
		/// <summary></summary>
		/// <param name="n">Node element to write</param>
		/// <param name="value">Data to set the element's <see cref="TCursor.InnerText"/> to</param>
		protected abstract void WriteElement(TCursor n, string value);
		/// <summary></summary>
		/// <param name="n">Node element to write</param>
		/// <param name="value">Data to set the element's <see cref="TCursor.InnerText"/> to</param>
		protected abstract void WriteElement(TCursor n, char value);
		/// <summary></summary>
		/// <param name="n">Node element to write</param>
		/// <param name="value">Data to set the element's <see cref="TCursor.InnerText"/> to</param>
		protected abstract void WriteElement(TCursor n, bool value);
		/// <summary></summary>
		/// <param name="n">Node element to write</param>
		/// <param name="value">Data to set the element's <see cref="TCursor.InnerText"/> to</param>
		protected abstract void WriteElement(TCursor n, float value);
		/// <summary></summary>
		/// <param name="n">Node element to write</param>
		/// <param name="value">Data to set the element's <see cref="TCursor.InnerText"/> to</param>
		protected abstract void WriteElement(TCursor n, double value);

		/// <summary></summary>
		/// <param name="n">Node element to write</param>
		/// <param name="value">Data to set the element's <see cref="TCursor.InnerText"/> to</param>
		/// <param name="toBase"></param>
		protected abstract void WriteElement(TCursor n, byte value, NumeralBase toBase);
		/// <summary></summary>
		/// <param name="n">Node element to write</param>
		/// <param name="value">Data to set the element's <see cref="TCursor.InnerText"/> to</param>
		/// <param name="toBase"></param>
		protected abstract void WriteElement(TCursor n, sbyte value, NumeralBase toBase);
		/// <summary></summary>
		/// <param name="n">Node element to write</param>
		/// <param name="value">Data to set the element's <see cref="TCursor.InnerText"/> to</param>
		/// <param name="toBase"></param>
		protected abstract void WriteElement(TCursor n, ushort value, NumeralBase toBase);
		/// <summary></summary>
		/// <param name="n">Node element to write</param>
		/// <param name="value">Data to set the element's <see cref="TCursor.InnerText"/> to</param>
		/// <param name="toBase"></param>
		protected abstract void WriteElement(TCursor n, short value, NumeralBase toBase);
		/// <summary></summary>
		/// <param name="n">Node element to write</param>
		/// <param name="value">Data to set the element's <see cref="TCursor.InnerText"/> to</param>
		/// <param name="toBase"></param>
		protected abstract void WriteElement(TCursor n, uint value, NumeralBase toBase);
		/// <summary></summary>
		/// <param name="n">Node element to write</param>
		/// <param name="value">Data to set the element's <see cref="TCursor.InnerText"/> to</param>
		/// <param name="toBase"></param>
		protected abstract void WriteElement(TCursor n, int value, NumeralBase toBase);
		/// <summary></summary>
		/// <param name="n">Node element to write</param>
		/// <param name="value">Data to set the element's <see cref="TCursor.InnerText"/> to</param>
		/// <param name="toBase"></param>
		protected abstract void WriteElement(TCursor n, ulong value, NumeralBase toBase);
		/// <summary></summary>
		/// <param name="n">Node element to write</param>
		/// <param name="value">Data to set the element's <see cref="TCursor.InnerText"/> to</param>
		/// <param name="toBase"></param>
		protected abstract void WriteElement(TCursor n, long value, NumeralBase toBase);
		#endregion


		#region WriteCursor
		/// <summary>Set <see cref="Cursor"/>'s value to <paramref name="value"/></summary>
		/// <param name="value">Data to set the <see cref="Cursor"/> to</param>
		public void WriteCursor(string value)
		{
			Contract.Requires<ArgumentNullException>(value != null);
			this.WriteElement(this.Cursor, value);
		}
		/// <summary>Set <see cref="Cursor"/>'s value to <paramref name="value"/></summary>
		/// <param name="value">Data to set the <see cref="Cursor"/> to</param>
		public void WriteCursor(char value)
		{
			this.WriteElement(this.Cursor, value);
		}
		/// <summary>Set <see cref="Cursor"/>'s value to <paramref name="value"/></summary>
		/// <param name="value">Data to set the <see cref="Cursor"/> to</param>
		public void WriteCursor(bool value)
		{
			this.WriteElement(this.Cursor, value);
		}
		/// <summary>Set <see cref="Cursor"/>'s value to <paramref name="value"/></summary>
		/// <param name="value">Data to set the <see cref="Cursor"/> to</param>
		public void WriteCursor(float value)
		{
			this.WriteElement(this.Cursor, value);
		}
		/// <summary>Set <see cref="Cursor"/>'s value to <paramref name="value"/></summary>
		/// <param name="value">Data to set the <see cref="Cursor"/> to</param>
		public void WriteCursor(double value)
		{
			this.WriteElement(this.Cursor, value);
		}

		/// <summary>Set <see cref="Cursor"/>'s value to <paramref name="value"/></summary>
		/// <param name="value">Data to set the <see cref="Cursor"/> to</param>
		/// <param name="toBase">Numerical base to use</param>
		public void WriteCursor(byte value, NumeralBase toBase=kDefaultRadix)
		{
			this.WriteElement(this.Cursor, value, toBase);
		}
		/// <summary>Set <see cref="Cursor"/>'s value to <paramref name="value"/></summary>
		/// <param name="value">Data to set the <see cref="Cursor"/> to</param>
		/// <param name="toBase">Numerical base to use</param>
		public void WriteCursor(sbyte value, NumeralBase toBase=kDefaultRadix)
		{
			this.WriteElement(this.Cursor, value, toBase);
		}
		/// <summary>Set <see cref="Cursor"/>'s value to <paramref name="value"/></summary>
		/// <param name="value">Data to set the <see cref="Cursor"/> to</param>
		/// <param name="toBase">Numerical base to use</param>
		public void WriteCursor(ushort value, NumeralBase toBase=kDefaultRadix)
		{
			this.WriteElement(this.Cursor, value, toBase);
		}
		/// <summary>Set <see cref="Cursor"/>'s value to <paramref name="value"/></summary>
		/// <param name="value">Data to set the <see cref="Cursor"/> to</param>
		/// <param name="toBase">Numerical base to use</param>
		public void WriteCursor(short value, NumeralBase toBase=kDefaultRadix)
		{
			this.WriteElement(this.Cursor, value, toBase);
		}
		/// <summary>Set <see cref="Cursor"/>'s value to <paramref name="value"/></summary>
		/// <param name="value">Data to set the <see cref="Cursor"/> to</param>
		/// <param name="toBase">Numerical base to use</param>
		public void WriteCursor(uint value, NumeralBase toBase=kDefaultRadix)
		{
			this.WriteElement(this.Cursor, value, toBase);
		}
		/// <summary>Set <see cref="Cursor"/>'s value to <paramref name="value"/></summary>
		/// <param name="value">Data to set the <see cref="Cursor"/> to</param>
		/// <param name="toBase">Numerical base to use</param>
		public void WriteCursor(int value, NumeralBase toBase=kDefaultRadix)
		{
			this.WriteElement(this.Cursor, value, toBase);
		}
		/// <summary>Set <see cref="Cursor"/>'s value to <paramref name="value"/></summary>
		/// <param name="value">Data to set the <see cref="Cursor"/> to</param>
		/// <param name="toBase">Numerical base to use</param>
		public void WriteCursor(ulong value, NumeralBase toBase=kDefaultRadix)
		{
			this.WriteElement(this.Cursor, value, toBase);
		}
		/// <summary>Set <see cref="Cursor"/>'s value to <paramref name="value"/></summary>
		/// <param name="value">Data to set the <see cref="Cursor"/> to</param>
		/// <param name="toBase">Numerical base to use</param>
		public void WriteCursor(long value, NumeralBase toBase=kDefaultRadix)
		{
			this.WriteElement(this.Cursor, value, toBase);
		}
		#endregion


		#region WriteElement
		/// <summary>Create a new element in the underlying <see cref="XmlDocument"/>, relative to <see cref="Cursor"/></summary>
		/// <param name="name">The <see cref="XmlElement"/>'s name</param>
		/// <param name="value">Data to set the element's <see cref="XmlElement.InnerText"/> to</param>
		/// <remarks>Does not change <see cref="Cursor"/></remarks>
		public void WriteElement(TName name, string value)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(value != null);

			this.WriteElement(this.WriteElementAppend(name), value);
		}
		/// <summary>Create a new element in the underlying <see cref="XmlDocument"/>, relative to <see cref="Cursor"/></summary>
		/// <param name="name">The <see cref="XmlElement"/>'s name</param>
		/// <param name="value">Data to set the element's <see cref="XmlElement.InnerText"/> to</param>
		/// <remarks>Does not change <see cref="Cursor"/></remarks>
		public void WriteElement(TName name, char value)
		{
			Contract.Requires(this.ValidateNameArg(name));

			this.WriteElement(this.WriteElementAppend(name), value);
		}
		/// <summary>Create a new element in the underlying <see cref="XmlDocument"/>, relative to <see cref="Cursor"/></summary>
		/// <param name="name">The <see cref="XmlElement"/>'s name</param>
		/// <param name="value">Data to set the element's <see cref="XmlElement.InnerText"/> to</param>
		/// <remarks>Does not change <see cref="Cursor"/></remarks>
		public void WriteElement(TName name, bool value)
		{
			Contract.Requires(this.ValidateNameArg(name));

			this.WriteElement(this.WriteElementAppend(name), value);
		}
		/// <summary>Create a new element in the underlying <see cref="XmlDocument"/>, relative to <see cref="Cursor"/></summary>
		/// <param name="name">The <see cref="XmlElement"/>'s name</param>
		/// <param name="value">Data to set the element's <see cref="XmlElement.InnerText"/> to</param>
		/// <remarks>Does not change <see cref="Cursor"/></remarks>
		public void WriteElement(TName name, float value)
		{
			Contract.Requires(this.ValidateNameArg(name));

			this.WriteElement(this.WriteElementAppend(name), value);
		}
		/// <summary>Create a new element in the underlying <see cref="XmlDocument"/>, relative to <see cref="Cursor"/></summary>
		/// <param name="name">The <see cref="XmlElement"/>'s name</param>
		/// <param name="value">Data to set the element's <see cref="XmlElement.InnerText"/> to</param>
		/// <remarks>Does not change <see cref="Cursor"/></remarks>
		public void WriteElement(TName name, double value)
		{
			Contract.Requires(this.ValidateNameArg(name));

			this.WriteElement(this.WriteElementAppend(name), value);
		}

		/// <summary>Create a new element in the underlying <see cref="XmlDocument"/>, relative to <see cref="Cursor"/></summary>
		/// <param name="name">The <see cref="XmlElement"/>'s name</param>
		/// <param name="toBase">Numerical base to use</param>
		/// <param name="value">Data to set the element's <see cref="XmlElement.InnerText"/> to</param>
		/// <remarks>Does not change <see cref="Cursor"/></remarks>
		public void WriteElement(TName name, byte value, NumeralBase toBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));

			this.WriteElement(this.WriteElementAppend(name), value, toBase);
		}
		/// <summary>Create a new element in the underlying <see cref="XmlDocument"/>, relative to <see cref="Cursor"/></summary>
		/// <param name="name">The <see cref="XmlElement"/>'s name</param>
		/// <param name="toBase">Numerical base to use</param>
		/// <param name="value">Data to set the element's <see cref="XmlElement.InnerText"/> to</param>
		/// <remarks>Does not change <see cref="Cursor"/></remarks>
		public void WriteElement(TName name, sbyte value, NumeralBase toBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));

			this.WriteElement(this.WriteElementAppend(name), value, toBase);
		}
		/// <summary>Create a new element in the underlying <see cref="XmlDocument"/>, relative to <see cref="Cursor"/></summary>
		/// <param name="name">The <see cref="XmlElement"/>'s name</param>
		/// <param name="toBase">Numerical base to use</param>
		/// <param name="value">Data to set the element's <see cref="XmlElement.InnerText"/> to</param>
		/// <remarks>Does not change <see cref="Cursor"/></remarks>
		public void WriteElement(TName name, ushort value, NumeralBase toBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));

			this.WriteElement(this.WriteElementAppend(name), value, toBase);
		}
		/// <summary>Create a new element in the underlying <see cref="XmlDocument"/>, relative to <see cref="Cursor"/></summary>
		/// <param name="name">The <see cref="XmlElement"/>'s name</param>
		/// <param name="toBase">Numerical base to use</param>
		/// <param name="value">Data to set the element's <see cref="XmlElement.InnerText"/> to</param>
		/// <remarks>Does not change <see cref="Cursor"/></remarks>
		public void WriteElement(TName name, short value, NumeralBase toBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));

			this.WriteElement(this.WriteElementAppend(name), value, toBase);
		}
		/// <summary>Create a new element in the underlying <see cref="XmlDocument"/>, relative to <see cref="Cursor"/></summary>
		/// <param name="name">The <see cref="XmlElement"/>'s name</param>
		/// <param name="toBase">Numerical base to use</param>
		/// <param name="value">Data to set the element's <see cref="XmlElement.InnerText"/> to</param>
		/// <remarks>Does not change <see cref="Cursor"/></remarks>
		public void WriteElement(TName name, uint value, NumeralBase toBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));

			this.WriteElement(this.WriteElementAppend(name), value, toBase);
		}
		/// <summary>Create a new element in the underlying <see cref="XmlDocument"/>, relative to <see cref="Cursor"/></summary>
		/// <param name="name">The <see cref="XmlElement"/>'s name</param>
		/// <param name="toBase">Numerical base to use</param>
		/// <param name="value">Data to set the element's <see cref="XmlElement.InnerText"/> to</param>
		/// <remarks>Does not change <see cref="Cursor"/></remarks>
		public void WriteElement(TName name, int value, NumeralBase toBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));

			this.WriteElement(this.WriteElementAppend(name), value, toBase);
		}
		/// <summary>Create a new element in the underlying <see cref="XmlDocument"/>, relative to <see cref="Cursor"/></summary>
		/// <param name="name">The <see cref="XmlElement"/>'s name</param>
		/// <param name="toBase">Numerical base to use</param>
		/// <param name="value">Data to set the element's <see cref="XmlElement.InnerText"/> to</param>
		/// <remarks>Does not change <see cref="Cursor"/></remarks>
		public void WriteElement(TName name, ulong value, NumeralBase toBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));

			this.WriteElement(this.WriteElementAppend(name), value, toBase);
		}
		/// <summary>Create a new element in the underlying <see cref="XmlDocument"/>, relative to <see cref="Cursor"/></summary>
		/// <param name="name">The <see cref="XmlElement"/>'s name</param>
		/// <param name="toBase">Numerical base to use</param>
		/// <param name="value">Data to set the element's <see cref="XmlElement.InnerText"/> to</param>
		/// <remarks>Does not change <see cref="Cursor"/></remarks>
		public void WriteElement(TName name, long value, NumeralBase toBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));

			this.WriteElement(this.WriteElementAppend(name), value, toBase);
		}
		#endregion


		#region WriteAttribute
		/// <summary>Create a new attribute for <see cref="Cursor"/></summary>
		/// <param name="name">Name of the <see cref="XmlAttribute"/></param>
		/// <param name="value">Data to set the attribute text to</param>
		public abstract void WriteAttribute(TName name, string value);
		/// <summary>Create a new attribute for <see cref="Cursor"/></summary>
		/// <param name="name">Name of the <see cref="XmlAttribute"/></param>
		/// <param name="value">Data to set the attribute text to</param>
		public abstract void WriteAttribute(TName name, char value);
		/// <summary>Create a new attribute for <see cref="Cursor"/></summary>
		/// <param name="name">Name of the <see cref="XmlAttribute"/></param>
		/// <param name="value">Data to set the attribute text to</param>
		public abstract void WriteAttribute(TName name, bool value);
		/// <summary>Create a new attribute for <see cref="Cursor"/></summary>
		/// <param name="name">Name of the <see cref="XmlAttribute"/></param>
		/// <param name="value">Data to set the attribute text to</param>
		public abstract void WriteAttribute(TName name, float value);
		/// <summary>Create a new attribute for <see cref="Cursor"/></summary>
		/// <param name="name">Name of the <see cref="XmlAttribute"/></param>
		/// <param name="value">Data to set the attribute text to</param>
		public abstract void WriteAttribute(TName name, double value);

		/// <summary>Create a new attribute for <see cref="Cursor"/></summary>
		/// <param name="name">Name of the <see cref="XmlAttribute"/></param>
		/// <param name="toBase">Numerical base to use</param>
		/// <param name="value">Data to set the attribute text to</param>
		public abstract void WriteAttribute(TName name, byte value, NumeralBase toBase=kDefaultRadix);
		/// <summary>Create a new attribute for <see cref="Cursor"/></summary>
		/// <param name="name">Name of the <see cref="XmlAttribute"/></param>
		/// <param name="toBase">Numerical base to use</param>
		/// <param name="value">Data to set the attribute text to</param>
		public abstract void WriteAttribute(TName name, sbyte value, NumeralBase toBase=kDefaultRadix);
		/// <summary>Create a new attribute for <see cref="Cursor"/></summary>
		/// <param name="name">Name of the <see cref="XmlAttribute"/></param>
		/// <param name="toBase">Numerical base to use</param>
		/// <param name="value">Data to set the attribute text to</param>
		public abstract void WriteAttribute(TName name, ushort value, NumeralBase toBase=kDefaultRadix);
		/// <summary>Create a new attribute for <see cref="Cursor"/></summary>
		/// <param name="name">Name of the <see cref="XmlAttribute"/></param>
		/// <param name="toBase">Numerical base to use</param>
		/// <param name="value">Data to set the attribute text to</param>
		public abstract void WriteAttribute(TName name, short value, NumeralBase toBase=kDefaultRadix);
		/// <summary>Create a new attribute for <see cref="Cursor"/></summary>
		/// <param name="name">Name of the <see cref="XmlAttribute"/></param>
		/// <param name="toBase">Numerical base to use</param>
		/// <param name="value">Data to set the attribute text to</param>
		public abstract void WriteAttribute(TName name, uint value, NumeralBase toBase=kDefaultRadix);
		/// <summary>Create a new attribute for <see cref="Cursor"/></summary>
		/// <param name="name">Name of the <see cref="XmlAttribute"/></param>
		/// <param name="toBase">Numerical base to use</param>
		/// <param name="value">Data to set the attribute text to</param>
		public abstract void WriteAttribute(TName name, int value, NumeralBase toBase=kDefaultRadix);
		/// <summary>Create a new attribute for <see cref="Cursor"/></summary>
		/// <param name="name">Name of the <see cref="XmlAttribute"/></param>
		/// <param name="toBase">Numerical base to use</param>
		/// <param name="value">Data to set the attribute text to</param>
		public abstract void WriteAttribute(TName name, ulong value, NumeralBase toBase=kDefaultRadix);
		/// <summary>Create a new attribute for <see cref="Cursor"/></summary>
		/// <param name="name">Name of the <see cref="XmlAttribute"/></param>
		/// <param name="toBase">Numerical base to use</param>
		/// <param name="value">Data to set the attribute text to</param>
		public abstract void WriteAttribute(TName name, long value, NumeralBase toBase=kDefaultRadix);
		#endregion


		#region WriteElementOpt
		/// <summary>Create a new element in the underlying <see cref="XmlDocument"/>, relative to <see cref="Cursor"/></summary>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <param name="name">The <see cref="XmlElement"/>'s name</param>
		/// <param name="value">Data to set the element's <see cref="XmlElement.InnerText"/> to</param>
		/// <remarks>Does not change <see cref="Cursor"/></remarks>
		/// <returns>True if <paramref name="value"/> was written</returns>
		public bool WriteElementOptOnTrue(TName name, string value, Predicate<string> predicate)
		{
			Contract.Requires(predicate != null);
			Contract.Requires(this.ValidateNameArg(name));

			bool result = this.IgnoreWritePredicates || predicate(value);

			if (result)
				this.WriteElement(name, value);

			return result;
		}
		/// <summary>Create a new element in the underlying <see cref="XmlDocument"/>, relative to <see cref="Cursor"/></summary>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>isn't</b> written</param>
		/// <param name="name">The <see cref="XmlElement"/>'s name</param>
		/// <param name="value">Data to set the element's <see cref="XmlElement.InnerText"/> to</param>
		/// <remarks>Does not change <see cref="Cursor"/></remarks>
		/// <returns>True if <paramref name="value"/> was written</returns>
		public bool WriteElementOptOnFalse(TName name, string value, Predicate<string> predicate)
		{
			Contract.Requires(predicate != null);
			Contract.Requires(this.ValidateNameArg(name));
			if (predicate != string.IsNullOrEmpty && value == null)
				throw new ArgumentNullException(nameof(value));

			bool result = this.IgnoreWritePredicates || !predicate(value);

			if (result)
				this.WriteElement(name, value);

			return result;
		}
		/// <summary>Create a new element in the underlying <see cref="XmlDocument"/>, relative to <see cref="Cursor"/></summary>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <param name="name">The <see cref="XmlElement"/>'s name</param>
		/// <param name="value">Data to set the element's <see cref="XmlElement.InnerText"/> to</param>
		/// <remarks>Does not change <see cref="Cursor"/></remarks>
		/// <returns>True if <paramref name="value"/> was written</returns>
		public bool WriteElementOptOnTrue(TName name, char value, Predicate<char> predicate)
		{
			Contract.Requires(predicate != null);
			Contract.Requires(this.ValidateNameArg(name));

			bool result = this.IgnoreWritePredicates || predicate(value);

			if (result)
				this.WriteElement(name, value);

			return result;
		}
		/// <summary>Create a new element in the underlying <see cref="XmlDocument"/>, relative to <see cref="Cursor"/></summary>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>isn't</b> written</param>
		/// <param name="name">The <see cref="XmlElement"/>'s name</param>
		/// <param name="value">Data to set the element's <see cref="XmlElement.InnerText"/> to</param>
		/// <remarks>Does not change <see cref="Cursor"/></remarks>
		/// <returns>True if <paramref name="value"/> was written</returns>
		public bool WriteElementOptOnFalse(TName name, char value, Predicate<char> predicate)
		{
			Contract.Requires(predicate != null);
			Contract.Requires(this.ValidateNameArg(name));

			bool result = this.IgnoreWritePredicates || !predicate(value);

			if (result)
				this.WriteElement(name, value);

			return result;
		}
		/// <summary>Create a new element in the underlying <see cref="XmlDocument"/>, relative to <see cref="Cursor"/></summary>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <param name="name">The <see cref="XmlElement"/>'s name</param>
		/// <param name="value">Data to set the element's <see cref="XmlElement.InnerText"/> to</param>
		/// <remarks>Does not change <see cref="Cursor"/></remarks>
		/// <returns>True if <paramref name="value"/> was written</returns>
		public bool WriteElementOptOnTrue(TName name, bool value, Predicate<bool> predicate)
		{
			Contract.Requires(predicate != null);
			Contract.Requires(this.ValidateNameArg(name));

			bool result = this.IgnoreWritePredicates || predicate(value);

			if (result)
				this.WriteElement(name, value);

			return result;
		}
		/// <summary>Create a new element in the underlying <see cref="XmlDocument"/>, relative to <see cref="Cursor"/></summary>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>isn't</b> written</param>
		/// <param name="name">The <see cref="XmlElement"/>'s name</param>
		/// <param name="value">Data to set the element's <see cref="XmlElement.InnerText"/> to</param>
		/// <remarks>Does not change <see cref="Cursor"/></remarks>
		/// <returns>True if <paramref name="value"/> was written</returns>
		public bool WriteElementOptOnFalse(TName name, bool value, Predicate<bool> predicate)
		{
			Contract.Requires(predicate != null);
			Contract.Requires(this.ValidateNameArg(name));

			bool result = this.IgnoreWritePredicates || !predicate(value);

			if (result)
				this.WriteElement(name, value);

			return result;
		}
		/// <summary>Create a new element in the underlying <see cref="XmlDocument"/>, relative to <see cref="Cursor"/></summary>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <param name="name">The <see cref="XmlElement"/>'s name</param>
		/// <param name="value">Data to set the element's <see cref="XmlElement.InnerText"/> to</param>
		/// <remarks>Does not change <see cref="Cursor"/></remarks>
		/// <returns>True if <paramref name="value"/> was written</returns>
		public bool WriteElementOptOnTrue(TName name, float value, Predicate<float> predicate)
		{
			Contract.Requires(predicate != null);
			Contract.Requires(this.ValidateNameArg(name));

			bool result = this.IgnoreWritePredicates || predicate(value);

			if (result)
				this.WriteElement(name, value);

			return result;
		}
		/// <summary>Create a new element in the underlying <see cref="XmlDocument"/>, relative to <see cref="Cursor"/></summary>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>isn't</b> written</param>
		/// <param name="name">The <see cref="XmlElement"/>'s name</param>
		/// <param name="value">Data to set the element's <see cref="XmlElement.InnerText"/> to</param>
		/// <remarks>Does not change <see cref="Cursor"/></remarks>
		/// <returns>True if <paramref name="value"/> was written</returns>
		public bool WriteElementOptOnFalse(TName name, float value, Predicate<float> predicate)
		{
			Contract.Requires(predicate != null);
			Contract.Requires(this.ValidateNameArg(name));

			bool result = this.IgnoreWritePredicates || !predicate(value);

			if (result)
				this.WriteElement(name, value);

			return result;
		}
		/// <summary>Create a new element in the underlying <see cref="XmlDocument"/>, relative to <see cref="Cursor"/></summary>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <param name="name">The <see cref="XmlElement"/>'s name</param>
		/// <param name="value">Data to set the element's <see cref="XmlElement.InnerText"/> to</param>
		/// <remarks>Does not change <see cref="Cursor"/></remarks>
		/// <returns>True if <paramref name="value"/> was written</returns>
		public bool WriteElementOptOnTrue(TName name, double value, Predicate<double> predicate)
		{
			Contract.Requires(predicate != null);
			Contract.Requires(this.ValidateNameArg(name));

			bool result = this.IgnoreWritePredicates || predicate(value);

			if (result)
				this.WriteElement(name, value);

			return result;
		}
		/// <summary>Create a new element in the underlying <see cref="XmlDocument"/>, relative to <see cref="Cursor"/></summary>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>isn't</b> written</param>
		/// <param name="name">The <see cref="XmlElement"/>'s name</param>
		/// <param name="value">Data to set the element's <see cref="XmlElement.InnerText"/> to</param>
		/// <remarks>Does not change <see cref="Cursor"/></remarks>
		/// <returns>True if <paramref name="value"/> was written</returns>
		public bool WriteElementOptOnFalse(TName name, double value, Predicate<double> predicate)
		{
			Contract.Requires(predicate != null);
			Contract.Requires(this.ValidateNameArg(name));

			bool result = this.IgnoreWritePredicates || !predicate(value);

			if (result)
				this.WriteElement(name, value);

			return result;
		}

		/// <summary>Create a new element in the underlying <see cref="XmlDocument"/>, relative to <see cref="Cursor"/></summary>
		/// <param name="name">The <see cref="XmlElement"/>'s name</param>
		/// <param name="value">Data to set the element's <see cref="XmlElement.InnerText"/> to</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <param name="toBase">Numerical base to use</param>
		/// <remarks>Does not change <see cref="Cursor"/></remarks>
		/// <returns>True if <paramref name="value"/> was written</returns>
		public bool WriteElementOptOnTrue(TName name, byte value, Predicate<byte> predicate, NumeralBase toBase=NumeralBase.Decimal)
		{
			Contract.Requires(predicate != null);
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires(this.Cursor != null, TagElementStreamContract<TDoc,TCursor,TName>.kCursorNullMsg);

			bool result = this.IgnoreWritePredicates || predicate(value);

			if (result)
				this.WriteElement(name, value, toBase);

			return result;
		}
		/// <summary>Create a new element in the underlying <see cref="XmlDocument"/>, relative to <see cref="Cursor"/></summary>
		/// <param name="name">The <see cref="XmlElement"/>'s name</param>
		/// <param name="value">Data to set the element's <see cref="XmlElement.InnerText"/> to</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>isn't</b> written</param>
		/// <param name="toBase">Numerical base to use</param>
		/// <remarks>Does not change <see cref="Cursor"/></remarks>
		/// <returns>True if <paramref name="value"/> was written</returns>
		public bool WriteElementOptOnFalse(TName name, byte value, Predicate<byte> predicate, NumeralBase toBase=NumeralBase.Decimal)
		{
			Contract.Requires(predicate != null);
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires(this.Cursor != null, TagElementStreamContract<TDoc,TCursor,TName>.kCursorNullMsg);

			bool result = this.IgnoreWritePredicates || !predicate(value);

			if (result)
				this.WriteElement(name, value, toBase);

			return result;
		}
		/// <summary>Create a new element in the underlying <see cref="XmlDocument"/>, relative to <see cref="Cursor"/></summary>
		/// <param name="name">The <see cref="XmlElement"/>'s name</param>
		/// <param name="value">Data to set the element's <see cref="XmlElement.InnerText"/> to</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <param name="toBase">Numerical base to use</param>
		/// <remarks>Does not change <see cref="Cursor"/></remarks>
		/// <returns>True if <paramref name="value"/> was written</returns>
		public bool WriteElementOptOnTrue(TName name, sbyte value, Predicate<sbyte> predicate, NumeralBase toBase=NumeralBase.Decimal)
		{
			Contract.Requires(predicate != null);
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires(this.Cursor != null, TagElementStreamContract<TDoc,TCursor,TName>.kCursorNullMsg);

			bool result = this.IgnoreWritePredicates || predicate(value);

			if (result)
				this.WriteElement(name, value, toBase);

			return result;
		}
		/// <summary>Create a new element in the underlying <see cref="XmlDocument"/>, relative to <see cref="Cursor"/></summary>
		/// <param name="name">The <see cref="XmlElement"/>'s name</param>
		/// <param name="value">Data to set the element's <see cref="XmlElement.InnerText"/> to</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>isn't</b> written</param>
		/// <param name="toBase">Numerical base to use</param>
		/// <remarks>Does not change <see cref="Cursor"/></remarks>
		/// <returns>True if <paramref name="value"/> was written</returns>
		public bool WriteElementOptOnFalse(TName name, sbyte value, Predicate<sbyte> predicate, NumeralBase toBase=NumeralBase.Decimal)
		{
			Contract.Requires(predicate != null);
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires(this.Cursor != null, TagElementStreamContract<TDoc,TCursor,TName>.kCursorNullMsg);

			bool result = this.IgnoreWritePredicates || !predicate(value);

			if (result)
				this.WriteElement(name, value, toBase);

			return result;
		}
		/// <summary>Create a new element in the underlying <see cref="XmlDocument"/>, relative to <see cref="Cursor"/></summary>
		/// <param name="name">The <see cref="XmlElement"/>'s name</param>
		/// <param name="value">Data to set the element's <see cref="XmlElement.InnerText"/> to</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <param name="toBase">Numerical base to use</param>
		/// <remarks>Does not change <see cref="Cursor"/></remarks>
		/// <returns>True if <paramref name="value"/> was written</returns>
		public bool WriteElementOptOnTrue(TName name, ushort value, Predicate<ushort> predicate, NumeralBase toBase=NumeralBase.Decimal)
		{
			Contract.Requires(predicate != null);
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires(this.Cursor != null, TagElementStreamContract<TDoc,TCursor,TName>.kCursorNullMsg);

			bool result = this.IgnoreWritePredicates || predicate(value);

			if (result)
				this.WriteElement(name, value, toBase);

			return result;
		}
		/// <summary>Create a new element in the underlying <see cref="XmlDocument"/>, relative to <see cref="Cursor"/></summary>
		/// <param name="name">The <see cref="XmlElement"/>'s name</param>
		/// <param name="value">Data to set the element's <see cref="XmlElement.InnerText"/> to</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>isn't</b> written</param>
		/// <param name="toBase">Numerical base to use</param>
		/// <remarks>Does not change <see cref="Cursor"/></remarks>
		/// <returns>True if <paramref name="value"/> was written</returns>
		public bool WriteElementOptOnFalse(TName name, ushort value, Predicate<ushort> predicate, NumeralBase toBase=NumeralBase.Decimal)
		{
			Contract.Requires(predicate != null);
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires(this.Cursor != null, TagElementStreamContract<TDoc,TCursor,TName>.kCursorNullMsg);

			bool result = this.IgnoreWritePredicates || !predicate(value);

			if (result)
				this.WriteElement(name, value, toBase);

			return result;
		}
		/// <summary>Create a new element in the underlying <see cref="XmlDocument"/>, relative to <see cref="Cursor"/></summary>
		/// <param name="name">The <see cref="XmlElement"/>'s name</param>
		/// <param name="value">Data to set the element's <see cref="XmlElement.InnerText"/> to</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <param name="toBase">Numerical base to use</param>
		/// <remarks>Does not change <see cref="Cursor"/></remarks>
		/// <returns>True if <paramref name="value"/> was written</returns>
		public bool WriteElementOptOnTrue(TName name, short value, Predicate<short> predicate, NumeralBase toBase=NumeralBase.Decimal)
		{
			Contract.Requires(predicate != null);
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires(this.Cursor != null, TagElementStreamContract<TDoc,TCursor,TName>.kCursorNullMsg);

			bool result = this.IgnoreWritePredicates || predicate(value);

			if (result)
				this.WriteElement(name, value, toBase);

			return result;
		}
		/// <summary>Create a new element in the underlying <see cref="XmlDocument"/>, relative to <see cref="Cursor"/></summary>
		/// <param name="name">The <see cref="XmlElement"/>'s name</param>
		/// <param name="value">Data to set the element's <see cref="XmlElement.InnerText"/> to</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>isn't</b> written</param>
		/// <param name="toBase">Numerical base to use</param>
		/// <remarks>Does not change <see cref="Cursor"/></remarks>
		/// <returns>True if <paramref name="value"/> was written</returns>
		public bool WriteElementOptOnFalse(TName name, short value, Predicate<short> predicate, NumeralBase toBase=NumeralBase.Decimal)
		{
			Contract.Requires(predicate != null);
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires(this.Cursor != null, TagElementStreamContract<TDoc,TCursor,TName>.kCursorNullMsg);

			bool result = this.IgnoreWritePredicates || !predicate(value);

			if (result)
				this.WriteElement(name, value, toBase);

			return result;
		}
		/// <summary>Create a new element in the underlying <see cref="XmlDocument"/>, relative to <see cref="Cursor"/></summary>
		/// <param name="name">The <see cref="XmlElement"/>'s name</param>
		/// <param name="value">Data to set the element's <see cref="XmlElement.InnerText"/> to</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <param name="toBase">Numerical base to use</param>
		/// <remarks>Does not change <see cref="Cursor"/></remarks>
		/// <returns>True if <paramref name="value"/> was written</returns>
		public bool WriteElementOptOnTrue(TName name, uint value, Predicate<uint> predicate, NumeralBase toBase=NumeralBase.Decimal)
		{
			Contract.Requires(predicate != null);
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires(this.Cursor != null, TagElementStreamContract<TDoc,TCursor,TName>.kCursorNullMsg);

			bool result = this.IgnoreWritePredicates || predicate(value);

			if (result)
				this.WriteElement(name, value, toBase);

			return result;
		}
		/// <summary>Create a new element in the underlying <see cref="XmlDocument"/>, relative to <see cref="Cursor"/></summary>
		/// <param name="name">The <see cref="XmlElement"/>'s name</param>
		/// <param name="value">Data to set the element's <see cref="XmlElement.InnerText"/> to</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>isn't</b> written</param>
		/// <param name="toBase">Numerical base to use</param>
		/// <remarks>Does not change <see cref="Cursor"/></remarks>
		/// <returns>True if <paramref name="value"/> was written</returns>
		public bool WriteElementOptOnFalse(TName name, uint value, Predicate<uint> predicate, NumeralBase toBase=NumeralBase.Decimal)
		{
			Contract.Requires(predicate != null);
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires(this.Cursor != null, TagElementStreamContract<TDoc,TCursor,TName>.kCursorNullMsg);

			bool result = this.IgnoreWritePredicates || !predicate(value);

			if (result)
				this.WriteElement(name, value, toBase);

			return result;
		}
		/// <summary>Create a new element in the underlying <see cref="XmlDocument"/>, relative to <see cref="Cursor"/></summary>
		/// <param name="name">The <see cref="XmlElement"/>'s name</param>
		/// <param name="value">Data to set the element's <see cref="XmlElement.InnerText"/> to</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <param name="toBase">Numerical base to use</param>
		/// <remarks>Does not change <see cref="Cursor"/></remarks>
		/// <returns>True if <paramref name="value"/> was written</returns>
		public bool WriteElementOptOnTrue(TName name, int value, Predicate<int> predicate, NumeralBase toBase=NumeralBase.Decimal)
		{
			Contract.Requires(predicate != null);
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires(this.Cursor != null, TagElementStreamContract<TDoc,TCursor,TName>.kCursorNullMsg);

			bool result = this.IgnoreWritePredicates || predicate(value);

			if (result)
				this.WriteElement(name, value, toBase);

			return result;
		}
		/// <summary>Create a new element in the underlying <see cref="XmlDocument"/>, relative to <see cref="Cursor"/></summary>
		/// <param name="name">The <see cref="XmlElement"/>'s name</param>
		/// <param name="value">Data to set the element's <see cref="XmlElement.InnerText"/> to</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>isn't</b> written</param>
		/// <param name="toBase">Numerical base to use</param>
		/// <remarks>Does not change <see cref="Cursor"/></remarks>
		/// <returns>True if <paramref name="value"/> was written</returns>
		public bool WriteElementOptOnFalse(TName name, int value, Predicate<int> predicate, NumeralBase toBase=NumeralBase.Decimal)
		{
			Contract.Requires(predicate != null);
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires(this.Cursor != null, TagElementStreamContract<TDoc,TCursor,TName>.kCursorNullMsg);

			bool result = this.IgnoreWritePredicates || !predicate(value);

			if (result)
				this.WriteElement(name, value, toBase);

			return result;
		}
		/// <summary>Create a new element in the underlying <see cref="XmlDocument"/>, relative to <see cref="Cursor"/></summary>
		/// <param name="name">The <see cref="XmlElement"/>'s name</param>
		/// <param name="value">Data to set the element's <see cref="XmlElement.InnerText"/> to</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <param name="toBase">Numerical base to use</param>
		/// <remarks>Does not change <see cref="Cursor"/></remarks>
		/// <returns>True if <paramref name="value"/> was written</returns>
		public bool WriteElementOptOnTrue(TName name, ulong value, Predicate<ulong> predicate, NumeralBase toBase=NumeralBase.Decimal)
		{
			Contract.Requires(predicate != null);
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires(this.Cursor != null, TagElementStreamContract<TDoc,TCursor,TName>.kCursorNullMsg);

			bool result = this.IgnoreWritePredicates || predicate(value);

			if (result)
				this.WriteElement(name, value, toBase);

			return result;
		}
		/// <summary>Create a new element in the underlying <see cref="XmlDocument"/>, relative to <see cref="Cursor"/></summary>
		/// <param name="name">The <see cref="XmlElement"/>'s name</param>
		/// <param name="value">Data to set the element's <see cref="XmlElement.InnerText"/> to</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>isn't</b> written</param>
		/// <param name="toBase">Numerical base to use</param>
		/// <remarks>Does not change <see cref="Cursor"/></remarks>
		/// <returns>True if <paramref name="value"/> was written</returns>
		public bool WriteElementOptOnFalse(TName name, ulong value, Predicate<ulong> predicate, NumeralBase toBase=NumeralBase.Decimal)
		{
			Contract.Requires(predicate != null);
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires(this.Cursor != null, TagElementStreamContract<TDoc,TCursor,TName>.kCursorNullMsg);

			bool result = this.IgnoreWritePredicates || !predicate(value);

			if (result)
				this.WriteElement(name, value, toBase);

			return result;
		}
		/// <summary>Create a new element in the underlying <see cref="XmlDocument"/>, relative to <see cref="Cursor"/></summary>
		/// <param name="name">The <see cref="XmlElement"/>'s name</param>
		/// <param name="value">Data to set the element's <see cref="XmlElement.InnerText"/> to</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <param name="toBase">Numerical base to use</param>
		/// <remarks>Does not change <see cref="Cursor"/></remarks>
		/// <returns>True if <paramref name="value"/> was written</returns>
		public bool WriteElementOptOnTrue(TName name, long value, Predicate<long> predicate, NumeralBase toBase=NumeralBase.Decimal)
		{
			Contract.Requires(predicate != null);
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires(this.Cursor != null, TagElementStreamContract<TDoc,TCursor,TName>.kCursorNullMsg);

			bool result = this.IgnoreWritePredicates || predicate(value);

			if (result)
				this.WriteElement(name, value, toBase);

			return result;
		}
		/// <summary>Create a new element in the underlying <see cref="XmlDocument"/>, relative to <see cref="Cursor"/></summary>
		/// <param name="name">The <see cref="XmlElement"/>'s name</param>
		/// <param name="value">Data to set the element's <see cref="XmlElement.InnerText"/> to</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>isn't</b> written</param>
		/// <param name="toBase">Numerical base to use</param>
		/// <remarks>Does not change <see cref="Cursor"/></remarks>
		/// <returns>True if <paramref name="value"/> was written</returns>
		public bool WriteElementOptOnFalse(TName name, long value, Predicate<long> predicate, NumeralBase toBase=NumeralBase.Decimal)
		{
			Contract.Requires(predicate != null);
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires(this.Cursor != null, TagElementStreamContract<TDoc,TCursor,TName>.kCursorNullMsg);

			bool result = this.IgnoreWritePredicates || !predicate(value);

			if (result)
				this.WriteElement(name, value, toBase);

			return result;
		}
		#endregion


		#region WriteAttributeOpt
		/// <summary>Create a new attribute for <see cref="Cursor"/></summary>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <param name="name">Name of the <see cref="XmlAttribute"/></param>
		/// <param name="value">Data to set the attribute text to</param>
		/// <returns>True if <paramref name="value"/> was written</returns>
		public bool WriteAttributeOptOnTrue(TName name, string value, Predicate<string> predicate)
		{
			Contract.Requires(predicate != null);
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires(this.Cursor != null, TagElementStreamContract<TDoc,TCursor,TName>.kCursorNullMsg);

			bool result = this.IgnoreWritePredicates || predicate(value);

			if (result)
				this.WriteAttribute(name, value);

			return result;
		}
		/// <summary>Create a new attribute for <see cref="Cursor"/></summary>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>isn't</b> written</param>
		/// <param name="name">Name of the <see cref="XmlAttribute"/></param>
		/// <param name="value">Data to set the attribute text to</param>
		/// <returns>True if <paramref name="value"/> was written</returns>
		public bool WriteAttributeOptOnFalse(TName name, string value, Predicate<string> predicate)
		{
			Contract.Requires(predicate != null);
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires(this.Cursor != null, TagElementStreamContract<TDoc,TCursor,TName>.kCursorNullMsg);
			if (predicate != string.IsNullOrEmpty && value == null)
				throw new ArgumentNullException(nameof(value));

			bool result = this.IgnoreWritePredicates || !predicate(value);

			if (result)
				this.WriteAttribute(name, value);

			return result;
		}
		/// <summary>Create a new attribute for <see cref="Cursor"/></summary>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <param name="name">Name of the <see cref="XmlAttribute"/></param>
		/// <param name="value">Data to set the attribute text to</param>
		/// <returns>True if <paramref name="value"/> was written</returns>
		public bool WriteAttributeOptOnTrue(TName name, char value, Predicate<char> predicate)
		{
			Contract.Requires(predicate != null);
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires(this.Cursor != null, TagElementStreamContract<TDoc,TCursor,TName>.kCursorNullMsg);

			bool result = this.IgnoreWritePredicates || predicate(value);

			if (result)
				this.WriteAttribute(name, value);

			return result;
		}
		/// <summary>Create a new attribute for <see cref="Cursor"/></summary>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>isn't</b> written</param>
		/// <param name="name">Name of the <see cref="XmlAttribute"/></param>
		/// <param name="value">Data to set the attribute text to</param>
		/// <returns>True if <paramref name="value"/> was written</returns>
		public bool WriteAttributeOptOnFalse(TName name, char value, Predicate<char> predicate)
		{
			Contract.Requires(predicate != null);
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires(this.Cursor != null, TagElementStreamContract<TDoc,TCursor,TName>.kCursorNullMsg);

			bool result = this.IgnoreWritePredicates || !predicate(value);

			if (result)
				this.WriteAttribute(name, value);

			return result;
		}
		/// <summary>Create a new attribute for <see cref="Cursor"/></summary>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <param name="name">Name of the <see cref="XmlAttribute"/></param>
		/// <param name="value">Data to set the attribute text to</param>
		/// <returns>True if <paramref name="value"/> was written</returns>
		public bool WriteAttributeOptOnTrue(TName name, bool value, Predicate<bool> predicate)
		{
			Contract.Requires(predicate != null);
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires(this.Cursor != null, TagElementStreamContract<TDoc,TCursor,TName>.kCursorNullMsg);

			bool result = this.IgnoreWritePredicates || predicate(value);

			if (result)
				this.WriteAttribute(name, value);

			return result;
		}
		/// <summary>Create a new attribute for <see cref="Cursor"/></summary>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>isn't</b> written</param>
		/// <param name="name">Name of the <see cref="XmlAttribute"/></param>
		/// <param name="value">Data to set the attribute text to</param>
		/// <returns>True if <paramref name="value"/> was written</returns>
		public bool WriteAttributeOptOnFalse(TName name, bool value, Predicate<bool> predicate)
		{
			Contract.Requires(predicate != null);
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires(this.Cursor != null, TagElementStreamContract<TDoc,TCursor,TName>.kCursorNullMsg);

			bool result = this.IgnoreWritePredicates || !predicate(value);

			if (result)
				this.WriteAttribute(name, value);

			return result;
		}
		/// <summary>Create a new attribute for <see cref="Cursor"/></summary>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <param name="name">Name of the <see cref="XmlAttribute"/></param>
		/// <param name="value">Data to set the attribute text to</param>
		/// <returns>True if <paramref name="value"/> was written</returns>
		public bool WriteAttributeOptOnTrue(TName name, float value, Predicate<float> predicate)
		{
			Contract.Requires(predicate != null);
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires(this.Cursor != null, TagElementStreamContract<TDoc,TCursor,TName>.kCursorNullMsg);

			bool result = this.IgnoreWritePredicates || predicate(value);

			if (result)
				this.WriteAttribute(name, value);

			return result;
		}
		/// <summary>Create a new attribute for <see cref="Cursor"/></summary>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>isn't</b> written</param>
		/// <param name="name">Name of the <see cref="XmlAttribute"/></param>
		/// <param name="value">Data to set the attribute text to</param>
		/// <returns>True if <paramref name="value"/> was written</returns>
		public bool WriteAttributeOptOnFalse(TName name, float value, Predicate<float> predicate)
		{
			Contract.Requires(predicate != null);
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires(this.Cursor != null, TagElementStreamContract<TDoc,TCursor,TName>.kCursorNullMsg);

			bool result = this.IgnoreWritePredicates || !predicate(value);

			if (result)
				this.WriteAttribute(name, value);

			return result;
		}
		/// <summary>Create a new attribute for <see cref="Cursor"/></summary>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <param name="name">Name of the <see cref="XmlAttribute"/></param>
		/// <param name="value">Data to set the attribute text to</param>
		/// <returns>True if <paramref name="value"/> was written</returns>
		public bool WriteAttributeOptOnTrue(TName name, double value, Predicate<double> predicate)
		{
			Contract.Requires(predicate != null);
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires(this.Cursor != null, TagElementStreamContract<TDoc,TCursor,TName>.kCursorNullMsg);

			bool result = this.IgnoreWritePredicates || predicate(value);

			if (result)
				this.WriteAttribute(name, value);

			return result;
		}
		/// <summary>Create a new attribute for <see cref="Cursor"/></summary>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>isn't</b> written</param>
		/// <param name="name">Name of the <see cref="XmlAttribute"/></param>
		/// <param name="value">Data to set the attribute text to</param>
		/// <returns>True if <paramref name="value"/> was written</returns>
		public bool WriteAttributeOptOnFalse(TName name, double value, Predicate<double> predicate)
		{
			Contract.Requires(predicate != null);
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires(this.Cursor != null, TagElementStreamContract<TDoc,TCursor,TName>.kCursorNullMsg);

			bool result = this.IgnoreWritePredicates || !predicate(value);

			if (result)
				this.WriteAttribute(name, value);

			return result;
		}

		/// <summary>Create a new attribute for <see cref="Cursor"/></summary>
		/// <param name="name">Name of the <see cref="XmlAttribute"/></param>
		/// <param name="value">Data to set the attribute text to</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <param name="toBase">Numerical base to use</param>
		/// <returns>True if <paramref name="value"/> was written</returns>
		public bool WriteAttributeOptOnTrue(TName name, byte value, Predicate<byte> predicate, NumeralBase toBase=NumeralBase.Decimal)
		{
			Contract.Requires(predicate != null);
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires(this.Cursor != null, TagElementStreamContract<TDoc,TCursor,TName>.kCursorNullMsg);

			bool result = this.IgnoreWritePredicates || predicate(value);

			if (result)
				this.WriteAttribute(name, value, toBase);

			return result;
		}
		/// <summary>Create a new attribute for <see cref="Cursor"/></summary>
		/// <param name="name">Name of the <see cref="XmlAttribute"/></param>
		/// <param name="value">Data to set the attribute text to</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>isn't</b> written</param>
		/// <param name="toBase">Numerical base to use</param>
		/// <returns>True if <paramref name="value"/> was written</returns>
		public bool WriteAttributeOptOnFalse(TName name, byte value, Predicate<byte> predicate, NumeralBase toBase=NumeralBase.Decimal)
		{
			Contract.Requires(predicate != null);
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires(this.Cursor != null, TagElementStreamContract<TDoc,TCursor,TName>.kCursorNullMsg);

			bool result = this.IgnoreWritePredicates || !predicate(value);

			if (result)
				this.WriteAttribute(name, value, toBase);

			return result;
		}
		/// <summary>Create a new attribute for <see cref="Cursor"/></summary>
		/// <param name="name">Name of the <see cref="XmlAttribute"/></param>
		/// <param name="value">Data to set the attribute text to</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <param name="toBase">Numerical base to use</param>
		/// <returns>True if <paramref name="value"/> was written</returns>
		public bool WriteAttributeOptOnTrue(TName name, sbyte value, Predicate<sbyte> predicate, NumeralBase toBase=NumeralBase.Decimal)
		{
			Contract.Requires(predicate != null);
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires(this.Cursor != null, TagElementStreamContract<TDoc,TCursor,TName>.kCursorNullMsg);

			bool result = this.IgnoreWritePredicates || predicate(value);

			if (result)
				this.WriteAttribute(name, value, toBase);

			return result;
		}
		/// <summary>Create a new attribute for <see cref="Cursor"/></summary>
		/// <param name="name">Name of the <see cref="XmlAttribute"/></param>
		/// <param name="value">Data to set the attribute text to</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>isn't</b> written</param>
		/// <param name="toBase">Numerical base to use</param>
		/// <returns>True if <paramref name="value"/> was written</returns>
		public bool WriteAttributeOptOnFalse(TName name, sbyte value, Predicate<sbyte> predicate, NumeralBase toBase=NumeralBase.Decimal)
		{
			Contract.Requires(predicate != null);
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires(this.Cursor != null, TagElementStreamContract<TDoc,TCursor,TName>.kCursorNullMsg);

			bool result = this.IgnoreWritePredicates || !predicate(value);

			if (result)
				this.WriteAttribute(name, value, toBase);

			return result;
		}
		/// <summary>Create a new attribute for <see cref="Cursor"/></summary>
		/// <param name="name">Name of the <see cref="XmlAttribute"/></param>
		/// <param name="value">Data to set the attribute text to</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <param name="toBase">Numerical base to use</param>
		/// <returns>True if <paramref name="value"/> was written</returns>
		public bool WriteAttributeOptOnTrue(TName name, ushort value, Predicate<ushort> predicate, NumeralBase toBase=NumeralBase.Decimal)
		{
			Contract.Requires(predicate != null);
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires(this.Cursor != null, TagElementStreamContract<TDoc,TCursor,TName>.kCursorNullMsg);

			bool result = this.IgnoreWritePredicates || predicate(value);

			if (result)
				this.WriteAttribute(name, value, toBase);

			return result;
		}
		/// <summary>Create a new attribute for <see cref="Cursor"/></summary>
		/// <param name="name">Name of the <see cref="XmlAttribute"/></param>
		/// <param name="value">Data to set the attribute text to</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>isn't</b> written</param>
		/// <param name="toBase">Numerical base to use</param>
		/// <returns>True if <paramref name="value"/> was written</returns>
		public bool WriteAttributeOptOnFalse(TName name, ushort value, Predicate<ushort> predicate, NumeralBase toBase=NumeralBase.Decimal)
		{
			Contract.Requires(predicate != null);
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires(this.Cursor != null, TagElementStreamContract<TDoc,TCursor,TName>.kCursorNullMsg);

			bool result = this.IgnoreWritePredicates || !predicate(value);

			if (result)
				this.WriteAttribute(name, value, toBase);

			return result;
		}
		/// <summary>Create a new attribute for <see cref="Cursor"/></summary>
		/// <param name="name">Name of the <see cref="XmlAttribute"/></param>
		/// <param name="value">Data to set the attribute text to</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <param name="toBase">Numerical base to use</param>
		/// <returns>True if <paramref name="value"/> was written</returns>
		public bool WriteAttributeOptOnTrue(TName name, short value, Predicate<short> predicate, NumeralBase toBase=NumeralBase.Decimal)
		{
			Contract.Requires(predicate != null);
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires(this.Cursor != null, TagElementStreamContract<TDoc,TCursor,TName>.kCursorNullMsg);

			bool result = this.IgnoreWritePredicates || predicate(value);

			if (result)
				this.WriteAttribute(name, value, toBase);

			return result;
		}
		/// <summary>Create a new attribute for <see cref="Cursor"/></summary>
		/// <param name="name">Name of the <see cref="XmlAttribute"/></param>
		/// <param name="value">Data to set the attribute text to</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>isn't</b> written</param>
		/// <param name="toBase">Numerical base to use</param>
		/// <returns>True if <paramref name="value"/> was written</returns>
		public bool WriteAttributeOptOnFalse(TName name, short value, Predicate<short> predicate, NumeralBase toBase=NumeralBase.Decimal)
		{
			Contract.Requires(predicate != null);
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires(this.Cursor != null, TagElementStreamContract<TDoc,TCursor,TName>.kCursorNullMsg);

			bool result = this.IgnoreWritePredicates || !predicate(value);

			if (result)
				this.WriteAttribute(name, value, toBase);

			return result;
		}
		/// <summary>Create a new attribute for <see cref="Cursor"/></summary>
		/// <param name="name">Name of the <see cref="XmlAttribute"/></param>
		/// <param name="value">Data to set the attribute text to</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <param name="toBase">Numerical base to use</param>
		/// <returns>True if <paramref name="value"/> was written</returns>
		public bool WriteAttributeOptOnTrue(TName name, uint value, Predicate<uint> predicate, NumeralBase toBase=NumeralBase.Decimal)
		{
			Contract.Requires(predicate != null);
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires(this.Cursor != null, TagElementStreamContract<TDoc,TCursor,TName>.kCursorNullMsg);

			bool result = this.IgnoreWritePredicates || predicate(value);

			if (result)
				this.WriteAttribute(name, value, toBase);

			return result;
		}
		/// <summary>Create a new attribute for <see cref="Cursor"/></summary>
		/// <param name="name">Name of the <see cref="XmlAttribute"/></param>
		/// <param name="value">Data to set the attribute text to</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>isn't</b> written</param>
		/// <param name="toBase">Numerical base to use</param>
		/// <returns>True if <paramref name="value"/> was written</returns>
		public bool WriteAttributeOptOnFalse(TName name, uint value, Predicate<uint> predicate, NumeralBase toBase=NumeralBase.Decimal)
		{
			Contract.Requires(predicate != null);
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires(this.Cursor != null, TagElementStreamContract<TDoc,TCursor,TName>.kCursorNullMsg);

			bool result = this.IgnoreWritePredicates || !predicate(value);

			if (result)
				this.WriteAttribute(name, value, toBase);

			return result;
		}
		/// <summary>Create a new attribute for <see cref="Cursor"/></summary>
		/// <param name="name">Name of the <see cref="XmlAttribute"/></param>
		/// <param name="value">Data to set the attribute text to</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <param name="toBase">Numerical base to use</param>
		/// <returns>True if <paramref name="value"/> was written</returns>
		public bool WriteAttributeOptOnTrue(TName name, int value, Predicate<int> predicate, NumeralBase toBase=NumeralBase.Decimal)
		{
			Contract.Requires(predicate != null);
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires(this.Cursor != null, TagElementStreamContract<TDoc,TCursor,TName>.kCursorNullMsg);

			bool result = this.IgnoreWritePredicates || predicate(value);

			if (result)
				this.WriteAttribute(name, value, toBase);

			return result;
		}
		/// <summary>Create a new attribute for <see cref="Cursor"/></summary>
		/// <param name="name">Name of the <see cref="XmlAttribute"/></param>
		/// <param name="value">Data to set the attribute text to</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>isn't</b> written</param>
		/// <param name="toBase">Numerical base to use</param>
		/// <returns>True if <paramref name="value"/> was written</returns>
		public bool WriteAttributeOptOnFalse(TName name, int value, Predicate<int> predicate, NumeralBase toBase=NumeralBase.Decimal)
		{
			Contract.Requires(predicate != null);
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires(this.Cursor != null, TagElementStreamContract<TDoc,TCursor,TName>.kCursorNullMsg);

			bool result = this.IgnoreWritePredicates || !predicate(value);

			if (result)
				this.WriteAttribute(name, value, toBase);

			return result;
		}
		/// <summary>Create a new attribute for <see cref="Cursor"/></summary>
		/// <param name="name">Name of the <see cref="XmlAttribute"/></param>
		/// <param name="value">Data to set the attribute text to</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <param name="toBase">Numerical base to use</param>
		/// <returns>True if <paramref name="value"/> was written</returns>
		public bool WriteAttributeOptOnTrue(TName name, ulong value, Predicate<ulong> predicate, NumeralBase toBase=NumeralBase.Decimal)
		{
			Contract.Requires(predicate != null);
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires(this.Cursor != null, TagElementStreamContract<TDoc,TCursor,TName>.kCursorNullMsg);

			bool result = this.IgnoreWritePredicates || predicate(value);

			if (result)
				this.WriteAttribute(name, value, toBase);

			return result;
		}
		/// <summary>Create a new attribute for <see cref="Cursor"/></summary>
		/// <param name="name">Name of the <see cref="XmlAttribute"/></param>
		/// <param name="value">Data to set the attribute text to</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>isn't</b> written</param>
		/// <param name="toBase">Numerical base to use</param>
		/// <returns>True if <paramref name="value"/> was written</returns>
		public bool WriteAttributeOptOnFalse(TName name, ulong value, Predicate<ulong> predicate, NumeralBase toBase=NumeralBase.Decimal)
		{
			Contract.Requires(predicate != null);
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires(this.Cursor != null, TagElementStreamContract<TDoc,TCursor,TName>.kCursorNullMsg);

			bool result = this.IgnoreWritePredicates || !predicate(value);

			if (result)
				this.WriteAttribute(name, value, toBase);

			return result;
		}
		/// <summary>Create a new attribute for <see cref="Cursor"/></summary>
		/// <param name="name">Name of the <see cref="XmlAttribute"/></param>
		/// <param name="value">Data to set the attribute text to</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <param name="toBase">Numerical base to use</param>
		/// <returns>True if <paramref name="value"/> was written</returns>
		public bool WriteAttributeOptOnTrue(TName name, long value, Predicate<long> predicate, NumeralBase toBase=NumeralBase.Decimal)
		{
			Contract.Requires(predicate != null);
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires(this.Cursor != null, TagElementStreamContract<TDoc,TCursor,TName>.kCursorNullMsg);

			bool result = this.IgnoreWritePredicates || predicate(value);

			if (result)
				this.WriteAttribute(name, value, toBase);

			return result;
		}
		/// <summary>Create a new attribute for <see cref="Cursor"/></summary>
		/// <param name="name">Name of the <see cref="XmlAttribute"/></param>
		/// <param name="value">Data to set the attribute text to</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>isn't</b> written</param>
		/// <param name="toBase">Numerical base to use</param>
		/// <returns>True if <paramref name="value"/> was written</returns>
		public bool WriteAttributeOptOnFalse(TName name, long value, Predicate<long> predicate, NumeralBase toBase=NumeralBase.Decimal)
		{
			Contract.Requires(predicate != null);
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires(this.Cursor != null, TagElementStreamContract<TDoc,TCursor,TName>.kCursorNullMsg);

			bool result = this.IgnoreWritePredicates || !predicate(value);

			if (result)
				this.WriteAttribute(name, value, toBase);

			return result;
		}
		#endregion


		#region WriteElements
		public void WriteElements(TName elementName, ICollection< string > coll)
		{
			Contract.Requires(this.ValidateNameArg(elementName));
			Contract.Requires<ArgumentNullException>(coll != null);

			foreach (var value in coll)
				this.WriteElement(elementName, value);
		}
		public void WriteElements(TName elementName, ICollection< char > coll)
		{
			Contract.Requires(this.ValidateNameArg(elementName));
			Contract.Requires<ArgumentNullException>(coll != null);

			foreach (var value in coll)
				this.WriteElement(elementName, value);
		}
		public void WriteElements(TName elementName, ICollection< bool > coll)
		{
			Contract.Requires(this.ValidateNameArg(elementName));
			Contract.Requires<ArgumentNullException>(coll != null);

			foreach (var value in coll)
				this.WriteElement(elementName, value);
		}
		public void WriteElements(TName elementName, ICollection< float > coll)
		{
			Contract.Requires(this.ValidateNameArg(elementName));
			Contract.Requires<ArgumentNullException>(coll != null);

			foreach (var value in coll)
				this.WriteElement(elementName, value);
		}
		public void WriteElements(TName elementName, ICollection< double > coll)
		{
			Contract.Requires(this.ValidateNameArg(elementName));
			Contract.Requires<ArgumentNullException>(coll != null);

			foreach (var value in coll)
				this.WriteElement(elementName, value);
		}

		public void WriteElements(TName elementName, ICollection< byte > coll, NumeralBase toBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(elementName));
			Contract.Requires<ArgumentNullException>(coll != null);

			foreach (var value in coll)
				this.WriteElement(elementName, value, toBase);
		}
		public void WriteElements(TName elementName, ICollection< sbyte > coll, NumeralBase toBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(elementName));
			Contract.Requires<ArgumentNullException>(coll != null);

			foreach (var value in coll)
				this.WriteElement(elementName, value, toBase);
		}
		public void WriteElements(TName elementName, ICollection< ushort > coll, NumeralBase toBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(elementName));
			Contract.Requires<ArgumentNullException>(coll != null);

			foreach (var value in coll)
				this.WriteElement(elementName, value, toBase);
		}
		public void WriteElements(TName elementName, ICollection< short > coll, NumeralBase toBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(elementName));
			Contract.Requires<ArgumentNullException>(coll != null);

			foreach (var value in coll)
				this.WriteElement(elementName, value, toBase);
		}
		public void WriteElements(TName elementName, ICollection< uint > coll, NumeralBase toBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(elementName));
			Contract.Requires<ArgumentNullException>(coll != null);

			foreach (var value in coll)
				this.WriteElement(elementName, value, toBase);
		}
		public void WriteElements(TName elementName, ICollection< int > coll, NumeralBase toBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(elementName));
			Contract.Requires<ArgumentNullException>(coll != null);

			foreach (var value in coll)
				this.WriteElement(elementName, value, toBase);
		}
		public void WriteElements(TName elementName, ICollection< ulong > coll, NumeralBase toBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(elementName));
			Contract.Requires<ArgumentNullException>(coll != null);

			foreach (var value in coll)
				this.WriteElement(elementName, value, toBase);
		}
		public void WriteElements(TName elementName, ICollection< long > coll, NumeralBase toBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(elementName));
			Contract.Requires<ArgumentNullException>(coll != null);

			foreach (var value in coll)
				this.WriteElement(elementName, value, toBase);
		}
		#endregion
	};


	partial class TagElementStreamContract<TDoc, TCursor, TName>
	{
		#region WriteAttribute
		public override void WriteAttribute(TName name, string value)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(value != null);
			Contract.Requires(this.Cursor != null, kCursorNullMsg);

			throw new NotImplementedException();
		}
		public override void WriteAttribute(TName name, char value)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires(this.Cursor != null, kCursorNullMsg);

			throw new NotImplementedException();
		}
		public override void WriteAttribute(TName name, bool value)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires(this.Cursor != null, kCursorNullMsg);

			throw new NotImplementedException();
		}
		public override void WriteAttribute(TName name, float value)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires(this.Cursor != null, kCursorNullMsg);

			throw new NotImplementedException();
		}
		public override void WriteAttribute(TName name, double value)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires(this.Cursor != null, kCursorNullMsg);

			throw new NotImplementedException();
		}

		public override void WriteAttribute(TName name, byte value, NumeralBase toBase)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires(this.Cursor != null, kCursorNullMsg);

			throw new NotImplementedException();
		}
		public override void WriteAttribute(TName name, sbyte value, NumeralBase toBase)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires(this.Cursor != null, kCursorNullMsg);

			throw new NotImplementedException();
		}
		public override void WriteAttribute(TName name, ushort value, NumeralBase toBase)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires(this.Cursor != null, kCursorNullMsg);

			throw new NotImplementedException();
		}
		public override void WriteAttribute(TName name, short value, NumeralBase toBase)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires(this.Cursor != null, kCursorNullMsg);

			throw new NotImplementedException();
		}
		public override void WriteAttribute(TName name, uint value, NumeralBase toBase)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires(this.Cursor != null, kCursorNullMsg);

			throw new NotImplementedException();
		}
		public override void WriteAttribute(TName name, int value, NumeralBase toBase)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires(this.Cursor != null, kCursorNullMsg);

			throw new NotImplementedException();
		}
		public override void WriteAttribute(TName name, ulong value, NumeralBase toBase)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires(this.Cursor != null, kCursorNullMsg);

			throw new NotImplementedException();
		}
		public override void WriteAttribute(TName name, long value, NumeralBase toBase)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires(this.Cursor != null, kCursorNullMsg);

			throw new NotImplementedException();
		}
		#endregion
	};
};
