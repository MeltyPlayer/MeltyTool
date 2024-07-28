using System;
using System.Collections.Generic;
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
		#region Stream Cursor
		/// <summary>Stream the Value of <see cref="Cursor"/> to or from <paramref name="value"/></summary>
		/// <param name="value">Source or destination value</param>
		/// <seealso cref="ReadCursor(string, ref string)"/>
		/// <seealso cref="WriteCursor(string, string)"/>
		public void StreamCursor(ref string value)
		{
				 if (this.IsReading)
					 this.ReadCursor(ref value);
			else if (this.IsWriting)
				this.WriteCursor(value);
		}
		/// <summary>Stream the Value of <see cref="Cursor"/> to or from <paramref name="value"/></summary>
		/// <param name="value">Source or destination value</param>
		/// <seealso cref="ReadCursor(string, ref char)"/>
		/// <seealso cref="WriteCursor(string, char)"/>
		public void StreamCursor(ref char value)
		{
				 if (this.IsReading)
					 this.ReadCursor(ref value);
			else if (this.IsWriting)
				this.WriteCursor(value);
		}
		/// <summary>Stream the Value of <see cref="Cursor"/> to or from <paramref name="value"/></summary>
		/// <param name="value">Source or destination value</param>
		/// <seealso cref="ReadCursor(string, ref bool)"/>
		/// <seealso cref="WriteCursor(string, bool)"/>
		public void StreamCursor(ref bool value)
		{
				 if (this.IsReading)
					 this.ReadCursor(ref value);
			else if (this.IsWriting)
				this.WriteCursor(value);
		}
		/// <summary>Stream the Value of <see cref="Cursor"/> to or from <paramref name="value"/></summary>
		/// <param name="value">Source or destination value</param>
		/// <seealso cref="ReadCursor(string, ref float)"/>
		/// <seealso cref="WriteCursor(string, float)"/>
		public void StreamCursor(ref float value)
		{
				 if (this.IsReading)
					 this.ReadCursor(ref value);
			else if (this.IsWriting)
				this.WriteCursor(value);
		}
		/// <summary>Stream the Value of <see cref="Cursor"/> to or from <paramref name="value"/></summary>
		/// <param name="value">Source or destination value</param>
		/// <seealso cref="ReadCursor(string, ref double)"/>
		/// <seealso cref="WriteCursor(string, double)"/>
		public void StreamCursor(ref double value)
		{
				 if (this.IsReading)
					 this.ReadCursor(ref value);
			else if (this.IsWriting)
				this.WriteCursor(value);
		}

		/// <summary>Stream the Value of <see cref="Cursor"/> to or from <paramref name="value"/></summary>
		/// <param name="value">Source or destination value</param>
		/// <param name="numBase">numerical base to use</param>
		/// <seealso cref="ReadCursor(string, ref byte, NumeralBase)"/>
		/// <seealso cref="WriteCursor(string, byte, NumeralBase)"/>
		public void StreamCursor(ref byte value, NumeralBase numBase=kDefaultRadix)
		{
				 if (this.IsReading)
					 this.ReadCursor(ref value, numBase);
			else if (this.IsWriting)
				this.WriteCursor(value, numBase);
		}
		/// <summary>Stream the Value of <see cref="Cursor"/> to or from <paramref name="value"/></summary>
		/// <param name="value">Source or destination value</param>
		/// <param name="numBase">numerical base to use</param>
		/// <seealso cref="ReadCursor(string, ref sbyte, NumeralBase)"/>
		/// <seealso cref="WriteCursor(string, sbyte, NumeralBase)"/>
		public void StreamCursor(ref sbyte value, NumeralBase numBase=kDefaultRadix)
		{
				 if (this.IsReading)
					 this.ReadCursor(ref value, numBase);
			else if (this.IsWriting)
				this.WriteCursor(value, numBase);
		}
		/// <summary>Stream the Value of <see cref="Cursor"/> to or from <paramref name="value"/></summary>
		/// <param name="value">Source or destination value</param>
		/// <param name="numBase">numerical base to use</param>
		/// <seealso cref="ReadCursor(string, ref ushort, NumeralBase)"/>
		/// <seealso cref="WriteCursor(string, ushort, NumeralBase)"/>
		public void StreamCursor(ref ushort value, NumeralBase numBase=kDefaultRadix)
		{
				 if (this.IsReading)
					 this.ReadCursor(ref value, numBase);
			else if (this.IsWriting)
				this.WriteCursor(value, numBase);
		}
		/// <summary>Stream the Value of <see cref="Cursor"/> to or from <paramref name="value"/></summary>
		/// <param name="value">Source or destination value</param>
		/// <param name="numBase">numerical base to use</param>
		/// <seealso cref="ReadCursor(string, ref short, NumeralBase)"/>
		/// <seealso cref="WriteCursor(string, short, NumeralBase)"/>
		public void StreamCursor(ref short value, NumeralBase numBase=kDefaultRadix)
		{
				 if (this.IsReading)
					 this.ReadCursor(ref value, numBase);
			else if (this.IsWriting)
				this.WriteCursor(value, numBase);
		}
		/// <summary>Stream the Value of <see cref="Cursor"/> to or from <paramref name="value"/></summary>
		/// <param name="value">Source or destination value</param>
		/// <param name="numBase">numerical base to use</param>
		/// <seealso cref="ReadCursor(string, ref uint, NumeralBase)"/>
		/// <seealso cref="WriteCursor(string, uint, NumeralBase)"/>
		public void StreamCursor(ref uint value, NumeralBase numBase=kDefaultRadix)
		{
				 if (this.IsReading)
					 this.ReadCursor(ref value, numBase);
			else if (this.IsWriting)
				this.WriteCursor(value, numBase);
		}
		/// <summary>Stream the Value of <see cref="Cursor"/> to or from <paramref name="value"/></summary>
		/// <param name="value">Source or destination value</param>
		/// <param name="numBase">numerical base to use</param>
		/// <seealso cref="ReadCursor(string, ref int, NumeralBase)"/>
		/// <seealso cref="WriteCursor(string, int, NumeralBase)"/>
		public void StreamCursor(ref int value, NumeralBase numBase=kDefaultRadix)
		{
				 if (this.IsReading)
					 this.ReadCursor(ref value, numBase);
			else if (this.IsWriting)
				this.WriteCursor(value, numBase);
		}
		/// <summary>Stream the Value of <see cref="Cursor"/> to or from <paramref name="value"/></summary>
		/// <param name="value">Source or destination value</param>
		/// <param name="numBase">numerical base to use</param>
		/// <seealso cref="ReadCursor(string, ref ulong, NumeralBase)"/>
		/// <seealso cref="WriteCursor(string, ulong, NumeralBase)"/>
		public void StreamCursor(ref ulong value, NumeralBase numBase=kDefaultRadix)
		{
				 if (this.IsReading)
					 this.ReadCursor(ref value, numBase);
			else if (this.IsWriting)
				this.WriteCursor(value, numBase);
		}
		/// <summary>Stream the Value of <see cref="Cursor"/> to or from <paramref name="value"/></summary>
		/// <param name="value">Source or destination value</param>
		/// <param name="numBase">numerical base to use</param>
		/// <seealso cref="ReadCursor(string, ref long, NumeralBase)"/>
		/// <seealso cref="WriteCursor(string, long, NumeralBase)"/>
		public void StreamCursor(ref long value, NumeralBase numBase=kDefaultRadix)
		{
				 if (this.IsReading)
					 this.ReadCursor(ref value, numBase);
			else if (this.IsWriting)
				this.WriteCursor(value, numBase);
		}

		public void StreamCursor<T>( T theObj, Exprs.Expression<Func<T, string >> propExpr  )
		{
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (this.IsReading)
			{
				var value = default( string );
				this.ReadCursor( ref value );
				{
					property.SetValue(theObj, value, null);
				}
			}
			else if (this.IsWriting)
				this.WriteCursor( (string)property.GetValue(theObj, null) );
		}
		public void StreamCursor<T>( T theObj, Exprs.Expression<Func<T, char >> propExpr  )
		{
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (this.IsReading)
			{
				var value = default( char );
				this.ReadCursor( ref value );
				{
					property.SetValue(theObj, value, null);
				}
			}
			else if (this.IsWriting)
				this.WriteCursor( (char)property.GetValue(theObj, null) );
		}
		public void StreamCursor<T>( T theObj, Exprs.Expression<Func<T, bool >> propExpr  )
		{
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (this.IsReading)
			{
				var value = default( bool );
				this.ReadCursor( ref value );
				{
					property.SetValue(theObj, value, null);
				}
			}
			else if (this.IsWriting)
				this.WriteCursor( (bool)property.GetValue(theObj, null) );
		}
		public void StreamCursor<T>( T theObj, Exprs.Expression<Func<T, float >> propExpr  )
		{
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (this.IsReading)
			{
				var value = default( float );
				this.ReadCursor( ref value );
				{
					property.SetValue(theObj, value, null);
				}
			}
			else if (this.IsWriting)
				this.WriteCursor( (float)property.GetValue(theObj, null) );
		}
		public void StreamCursor<T>( T theObj, Exprs.Expression<Func<T, double >> propExpr  )
		{
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (this.IsReading)
			{
				var value = default( double );
				this.ReadCursor( ref value );
				{
					property.SetValue(theObj, value, null);
				}
			}
			else if (this.IsWriting)
				this.WriteCursor( (double)property.GetValue(theObj, null) );
		}

		public void StreamCursor<T>( T theObj, Exprs.Expression<Func<T, byte >> propExpr  , NumeralBase numBase=kDefaultRadix)
		{
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (this.IsReading)
			{
				var value = default( byte );
				this.ReadCursor( ref value , numBase);
				{
					property.SetValue(theObj, value, null);
				}
			}
			else if (this.IsWriting)
				this.WriteCursor( (byte)property.GetValue(theObj, null) , numBase);
		}
		public void StreamCursor<T>( T theObj, Exprs.Expression<Func<T, sbyte >> propExpr  , NumeralBase numBase=kDefaultRadix)
		{
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (this.IsReading)
			{
				var value = default( sbyte );
				this.ReadCursor( ref value , numBase);
				{
					property.SetValue(theObj, value, null);
				}
			}
			else if (this.IsWriting)
				this.WriteCursor( (sbyte)property.GetValue(theObj, null) , numBase);
		}
		public void StreamCursor<T>( T theObj, Exprs.Expression<Func<T, ushort >> propExpr  , NumeralBase numBase=kDefaultRadix)
		{
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (this.IsReading)
			{
				var value = default( ushort );
				this.ReadCursor( ref value , numBase);
				{
					property.SetValue(theObj, value, null);
				}
			}
			else if (this.IsWriting)
				this.WriteCursor( (ushort)property.GetValue(theObj, null) , numBase);
		}
		public void StreamCursor<T>( T theObj, Exprs.Expression<Func<T, short >> propExpr  , NumeralBase numBase=kDefaultRadix)
		{
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (this.IsReading)
			{
				var value = default( short );
				this.ReadCursor( ref value , numBase);
				{
					property.SetValue(theObj, value, null);
				}
			}
			else if (this.IsWriting)
				this.WriteCursor( (short)property.GetValue(theObj, null) , numBase);
		}
		public void StreamCursor<T>( T theObj, Exprs.Expression<Func<T, uint >> propExpr  , NumeralBase numBase=kDefaultRadix)
		{
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (this.IsReading)
			{
				var value = default( uint );
				this.ReadCursor( ref value , numBase);
				{
					property.SetValue(theObj, value, null);
				}
			}
			else if (this.IsWriting)
				this.WriteCursor( (uint)property.GetValue(theObj, null) , numBase);
		}
		public void StreamCursor<T>( T theObj, Exprs.Expression<Func<T, int >> propExpr  , NumeralBase numBase=kDefaultRadix)
		{
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (this.IsReading)
			{
				var value = default( int );
				this.ReadCursor( ref value , numBase);
				{
					property.SetValue(theObj, value, null);
				}
			}
			else if (this.IsWriting)
				this.WriteCursor( (int)property.GetValue(theObj, null) , numBase);
		}
		public void StreamCursor<T>( T theObj, Exprs.Expression<Func<T, ulong >> propExpr  , NumeralBase numBase=kDefaultRadix)
		{
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (this.IsReading)
			{
				var value = default( ulong );
				this.ReadCursor( ref value , numBase);
				{
					property.SetValue(theObj, value, null);
				}
			}
			else if (this.IsWriting)
				this.WriteCursor( (ulong)property.GetValue(theObj, null) , numBase);
		}
		public void StreamCursor<T>( T theObj, Exprs.Expression<Func<T, long >> propExpr  , NumeralBase numBase=kDefaultRadix)
		{
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (this.IsReading)
			{
				var value = default( long );
				this.ReadCursor( ref value , numBase);
				{
					property.SetValue(theObj, value, null);
				}
			}
			else if (this.IsWriting)
				this.WriteCursor( (long)property.GetValue(theObj, null) , numBase);
		}

		public void StreamCursor<T>( T theObj, Exprs.Expression<Func<T, Values.KGuid >> propExpr  )
		{
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (this.IsReading)
			{
				var value = default( Values.KGuid );
				this.ReadCursor( ref value );
				{
					property.SetValue(theObj, value, null);
				}
			}
			else if (this.IsWriting)
				this.WriteCursor( (Values.KGuid)property.GetValue(theObj, null) );
		}
		#endregion


		#region Stream Element
		/// <summary>Stream the Value of element <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Element name</param>
		/// <param name="value">Source or destination value</param>
		/// <seealso cref="ReadElement(string, ref string)"/>
		/// <seealso cref="WriteElement(string, string)"/>
		public void StreamElement(TName name, ref string value)
		{
			Contract.Requires(this.ValidateNameArg(name));

				 if (this.IsReading)
					 this.ReadElement(name, ref value);
			else if (this.IsWriting)
				this.WriteElement(name, value);
		}
		/// <summary>Stream the Value of element <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Element name</param>
		/// <param name="value">Source or destination value</param>
		/// <seealso cref="ReadElement(string, ref char)"/>
		/// <seealso cref="WriteElement(string, char)"/>
		public void StreamElement(TName name, ref char value)
		{
			Contract.Requires(this.ValidateNameArg(name));

				 if (this.IsReading)
					 this.ReadElement(name, ref value);
			else if (this.IsWriting)
				this.WriteElement(name, value);
		}
		/// <summary>Stream the Value of element <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Element name</param>
		/// <param name="value">Source or destination value</param>
		/// <seealso cref="ReadElement(string, ref bool)"/>
		/// <seealso cref="WriteElement(string, bool)"/>
		public void StreamElement(TName name, ref bool value)
		{
			Contract.Requires(this.ValidateNameArg(name));

				 if (this.IsReading)
					 this.ReadElement(name, ref value);
			else if (this.IsWriting)
				this.WriteElement(name, value);
		}
		/// <summary>Stream the Value of element <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Element name</param>
		/// <param name="value">Source or destination value</param>
		/// <seealso cref="ReadElement(string, ref float)"/>
		/// <seealso cref="WriteElement(string, float)"/>
		public void StreamElement(TName name, ref float value)
		{
			Contract.Requires(this.ValidateNameArg(name));

				 if (this.IsReading)
					 this.ReadElement(name, ref value);
			else if (this.IsWriting)
				this.WriteElement(name, value);
		}
		/// <summary>Stream the Value of element <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Element name</param>
		/// <param name="value">Source or destination value</param>
		/// <seealso cref="ReadElement(string, ref double)"/>
		/// <seealso cref="WriteElement(string, double)"/>
		public void StreamElement(TName name, ref double value)
		{
			Contract.Requires(this.ValidateNameArg(name));

				 if (this.IsReading)
					 this.ReadElement(name, ref value);
			else if (this.IsWriting)
				this.WriteElement(name, value);
		}

		/// <summary>Stream the Value of element <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Element name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="numBase">numerical base to use</param>
		/// <seealso cref="ReadElement(string, ref byte, NumeralBase)"/>
		/// <seealso cref="WriteElement(string, byte, NumeralBase)"/>
		public void StreamElement(TName name, ref byte value, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));

				 if (this.IsReading)
					 this.ReadElement(name, ref value, numBase);
			else if (this.IsWriting)
				this.WriteElement(name, value, numBase);
		}
		/// <summary>Stream the Value of element <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Element name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="numBase">numerical base to use</param>
		/// <seealso cref="ReadElement(string, ref sbyte, NumeralBase)"/>
		/// <seealso cref="WriteElement(string, sbyte, NumeralBase)"/>
		public void StreamElement(TName name, ref sbyte value, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));

				 if (this.IsReading)
					 this.ReadElement(name, ref value, numBase);
			else if (this.IsWriting)
				this.WriteElement(name, value, numBase);
		}
		/// <summary>Stream the Value of element <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Element name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="numBase">numerical base to use</param>
		/// <seealso cref="ReadElement(string, ref ushort, NumeralBase)"/>
		/// <seealso cref="WriteElement(string, ushort, NumeralBase)"/>
		public void StreamElement(TName name, ref ushort value, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));

				 if (this.IsReading)
					 this.ReadElement(name, ref value, numBase);
			else if (this.IsWriting)
				this.WriteElement(name, value, numBase);
		}
		/// <summary>Stream the Value of element <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Element name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="numBase">numerical base to use</param>
		/// <seealso cref="ReadElement(string, ref short, NumeralBase)"/>
		/// <seealso cref="WriteElement(string, short, NumeralBase)"/>
		public void StreamElement(TName name, ref short value, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));

				 if (this.IsReading)
					 this.ReadElement(name, ref value, numBase);
			else if (this.IsWriting)
				this.WriteElement(name, value, numBase);
		}
		/// <summary>Stream the Value of element <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Element name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="numBase">numerical base to use</param>
		/// <seealso cref="ReadElement(string, ref uint, NumeralBase)"/>
		/// <seealso cref="WriteElement(string, uint, NumeralBase)"/>
		public void StreamElement(TName name, ref uint value, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));

				 if (this.IsReading)
					 this.ReadElement(name, ref value, numBase);
			else if (this.IsWriting)
				this.WriteElement(name, value, numBase);
		}
		/// <summary>Stream the Value of element <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Element name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="numBase">numerical base to use</param>
		/// <seealso cref="ReadElement(string, ref int, NumeralBase)"/>
		/// <seealso cref="WriteElement(string, int, NumeralBase)"/>
		public void StreamElement(TName name, ref int value, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));

				 if (this.IsReading)
					 this.ReadElement(name, ref value, numBase);
			else if (this.IsWriting)
				this.WriteElement(name, value, numBase);
		}
		/// <summary>Stream the Value of element <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Element name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="numBase">numerical base to use</param>
		/// <seealso cref="ReadElement(string, ref ulong, NumeralBase)"/>
		/// <seealso cref="WriteElement(string, ulong, NumeralBase)"/>
		public void StreamElement(TName name, ref ulong value, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));

				 if (this.IsReading)
					 this.ReadElement(name, ref value, numBase);
			else if (this.IsWriting)
				this.WriteElement(name, value, numBase);
		}
		/// <summary>Stream the Value of element <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Element name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="numBase">numerical base to use</param>
		/// <seealso cref="ReadElement(string, ref long, NumeralBase)"/>
		/// <seealso cref="WriteElement(string, long, NumeralBase)"/>
		public void StreamElement(TName name, ref long value, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));

				 if (this.IsReading)
					 this.ReadElement(name, ref value, numBase);
			else if (this.IsWriting)
				this.WriteElement(name, value, numBase);
		}

		public void StreamElement<T>(TName name, T theObj, Exprs.Expression<Func<T, string >> propExpr  )
		{
			Contract.Requires(this.ValidateNameArg(name));

			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (this.IsReading)
			{
				var value = default( string );
				this.ReadElement(name, ref value );
				{
					property.SetValue(theObj, value, null);
				}
			}
			else if (this.IsWriting)
				this.WriteElement(name, (string)property.GetValue(theObj, null) );
		}
		public void StreamElement<T>(TName name, T theObj, Exprs.Expression<Func<T, char >> propExpr  )
		{
			Contract.Requires(this.ValidateNameArg(name));

			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (this.IsReading)
			{
				var value = default( char );
				this.ReadElement(name, ref value );
				{
					property.SetValue(theObj, value, null);
				}
			}
			else if (this.IsWriting)
				this.WriteElement(name, (char)property.GetValue(theObj, null) );
		}
		public void StreamElement<T>(TName name, T theObj, Exprs.Expression<Func<T, bool >> propExpr  )
		{
			Contract.Requires(this.ValidateNameArg(name));

			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (this.IsReading)
			{
				var value = default( bool );
				this.ReadElement(name, ref value );
				{
					property.SetValue(theObj, value, null);
				}
			}
			else if (this.IsWriting)
				this.WriteElement(name, (bool)property.GetValue(theObj, null) );
		}
		public void StreamElement<T>(TName name, T theObj, Exprs.Expression<Func<T, float >> propExpr  )
		{
			Contract.Requires(this.ValidateNameArg(name));

			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (this.IsReading)
			{
				var value = default( float );
				this.ReadElement(name, ref value );
				{
					property.SetValue(theObj, value, null);
				}
			}
			else if (this.IsWriting)
				this.WriteElement(name, (float)property.GetValue(theObj, null) );
		}
		public void StreamElement<T>(TName name, T theObj, Exprs.Expression<Func<T, double >> propExpr  )
		{
			Contract.Requires(this.ValidateNameArg(name));

			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (this.IsReading)
			{
				var value = default( double );
				this.ReadElement(name, ref value );
				{
					property.SetValue(theObj, value, null);
				}
			}
			else if (this.IsWriting)
				this.WriteElement(name, (double)property.GetValue(theObj, null) );
		}

		public void StreamElement<T>(TName name, T theObj, Exprs.Expression<Func<T, byte >> propExpr  , NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));

			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (this.IsReading)
			{
				var value = default( byte );
				this.ReadElement(name, ref value , numBase);
				{
					property.SetValue(theObj, value, null);
				}
			}
			else if (this.IsWriting)
				this.WriteElement(name, (byte)property.GetValue(theObj, null) , numBase);
		}
		public void StreamElement<T>(TName name, T theObj, Exprs.Expression<Func<T, sbyte >> propExpr  , NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));

			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (this.IsReading)
			{
				var value = default( sbyte );
				this.ReadElement(name, ref value , numBase);
				{
					property.SetValue(theObj, value, null);
				}
			}
			else if (this.IsWriting)
				this.WriteElement(name, (sbyte)property.GetValue(theObj, null) , numBase);
		}
		public void StreamElement<T>(TName name, T theObj, Exprs.Expression<Func<T, ushort >> propExpr  , NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));

			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (this.IsReading)
			{
				var value = default( ushort );
				this.ReadElement(name, ref value , numBase);
				{
					property.SetValue(theObj, value, null);
				}
			}
			else if (this.IsWriting)
				this.WriteElement(name, (ushort)property.GetValue(theObj, null) , numBase);
		}
		public void StreamElement<T>(TName name, T theObj, Exprs.Expression<Func<T, short >> propExpr  , NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));

			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (this.IsReading)
			{
				var value = default( short );
				this.ReadElement(name, ref value , numBase);
				{
					property.SetValue(theObj, value, null);
				}
			}
			else if (this.IsWriting)
				this.WriteElement(name, (short)property.GetValue(theObj, null) , numBase);
		}
		public void StreamElement<T>(TName name, T theObj, Exprs.Expression<Func<T, uint >> propExpr  , NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));

			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (this.IsReading)
			{
				var value = default( uint );
				this.ReadElement(name, ref value , numBase);
				{
					property.SetValue(theObj, value, null);
				}
			}
			else if (this.IsWriting)
				this.WriteElement(name, (uint)property.GetValue(theObj, null) , numBase);
		}
		public void StreamElement<T>(TName name, T theObj, Exprs.Expression<Func<T, int >> propExpr  , NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));

			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (this.IsReading)
			{
				var value = default( int );
				this.ReadElement(name, ref value , numBase);
				{
					property.SetValue(theObj, value, null);
				}
			}
			else if (this.IsWriting)
				this.WriteElement(name, (int)property.GetValue(theObj, null) , numBase);
		}
		public void StreamElement<T>(TName name, T theObj, Exprs.Expression<Func<T, ulong >> propExpr  , NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));

			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (this.IsReading)
			{
				var value = default( ulong );
				this.ReadElement(name, ref value , numBase);
				{
					property.SetValue(theObj, value, null);
				}
			}
			else if (this.IsWriting)
				this.WriteElement(name, (ulong)property.GetValue(theObj, null) , numBase);
		}
		public void StreamElement<T>(TName name, T theObj, Exprs.Expression<Func<T, long >> propExpr  , NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));

			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (this.IsReading)
			{
				var value = default( long );
				this.ReadElement(name, ref value , numBase);
				{
					property.SetValue(theObj, value, null);
				}
			}
			else if (this.IsWriting)
				this.WriteElement(name, (long)property.GetValue(theObj, null) , numBase);
		}

		public void StreamElement<T>(TName name, T theObj, Exprs.Expression<Func<T, Values.KGuid >> propExpr  )
		{
			Contract.Requires(this.ValidateNameArg(name));

			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (this.IsReading)
			{
				var value = default( Values.KGuid );
				this.ReadElement(name, ref value );
				{
					property.SetValue(theObj, value, null);
				}
			}
			else if (this.IsWriting)
				this.WriteElement(name, (Values.KGuid)property.GetValue(theObj, null) );
		}
		#endregion


		#region StreamElementOpt
		/// <summary>Stream the Value of element <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Element name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <returns>True if <paramref name="value"/> was read/written from/to stream</returns>
		/// <seealso cref="ReadElementOpt(TName, ref string)"/>
		/// <seealso cref="WriteElementOptOnTrue(TName, string, Predicate{string})"/>
		public bool StreamElementOpt(TName name, ref string value, Predicate<string> predicate = null)
		{
			Contract.Requires(this.ValidateNameArg(name));

			if (predicate == null)
				predicate = Predicates.True< string >;

			bool executed = false;
				 if (this.IsReading) executed = this.ReadElementOpt(name, ref value);
			else if (this.IsWriting) executed = this.WriteElementOptOnTrue(name, value, predicate);
			return executed;
		}
		/// <summary>Stream the Value of element <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Element name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <returns>True if <paramref name="value"/> was read/written from/to stream</returns>
		/// <seealso cref="ReadElementOpt(TName, ref char)"/>
		/// <seealso cref="WriteElementOptOnTrue(TName, char, Predicate{char})"/>
		public bool StreamElementOpt(TName name, ref char value, Predicate<char> predicate = null)
		{
			Contract.Requires(this.ValidateNameArg(name));

			if (predicate == null)
				predicate = Predicates.True< char >;

			bool executed = false;
				 if (this.IsReading) executed = this.ReadElementOpt(name, ref value);
			else if (this.IsWriting) executed = this.WriteElementOptOnTrue(name, value, predicate);
			return executed;
		}
		/// <summary>Stream the Value of element <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Element name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <returns>True if <paramref name="value"/> was read/written from/to stream</returns>
		/// <seealso cref="ReadElementOpt(TName, ref bool)"/>
		/// <seealso cref="WriteElementOptOnTrue(TName, bool, Predicate{bool})"/>
		public bool StreamElementOpt(TName name, ref bool value, Predicate<bool> predicate = null)
		{
			Contract.Requires(this.ValidateNameArg(name));

			if (predicate == null)
				predicate = Predicates.True< bool >;

			bool executed = false;
				 if (this.IsReading) executed = this.ReadElementOpt(name, ref value);
			else if (this.IsWriting) executed = this.WriteElementOptOnTrue(name, value, predicate);
			return executed;
		}
		/// <summary>Stream the Value of element <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Element name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <returns>True if <paramref name="value"/> was read/written from/to stream</returns>
		/// <seealso cref="ReadElementOpt(TName, ref float)"/>
		/// <seealso cref="WriteElementOptOnTrue(TName, float, Predicate{float})"/>
		public bool StreamElementOpt(TName name, ref float value, Predicate<float> predicate = null)
		{
			Contract.Requires(this.ValidateNameArg(name));

			if (predicate == null)
				predicate = Predicates.True< float >;

			bool executed = false;
				 if (this.IsReading) executed = this.ReadElementOpt(name, ref value);
			else if (this.IsWriting) executed = this.WriteElementOptOnTrue(name, value, predicate);
			return executed;
		}
		/// <summary>Stream the Value of element <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Element name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <returns>True if <paramref name="value"/> was read/written from/to stream</returns>
		/// <seealso cref="ReadElementOpt(TName, ref double)"/>
		/// <seealso cref="WriteElementOptOnTrue(TName, double, Predicate{double})"/>
		public bool StreamElementOpt(TName name, ref double value, Predicate<double> predicate = null)
		{
			Contract.Requires(this.ValidateNameArg(name));

			if (predicate == null)
				predicate = Predicates.True< double >;

			bool executed = false;
				 if (this.IsReading) executed = this.ReadElementOpt(name, ref value);
			else if (this.IsWriting) executed = this.WriteElementOptOnTrue(name, value, predicate);
			return executed;
		}

		/// <summary>Stream the Value of element <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Element name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <param name="numBase">numerical base to use</param>
		/// <returns>True if <paramref name="value"/> was read/written from/to stream</returns>
		/// <seealso cref="ReadElementOpt(TName, ref byte, NumeralBase)"/>
		/// <seealso cref="WriteElementOptOnTrue(TName, byte, Predicate{byte}, NumeralBase)"/>
		public bool StreamElementOpt(TName name, ref byte value, Predicate<byte> predicate = null, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));

			if (predicate == null)
				predicate = Predicates.True< byte >;

			bool executed = false;
				 if (this.IsReading) executed = this.ReadElementOpt(name, ref value, numBase);
			else if (this.IsWriting) executed = this.WriteElementOptOnTrue(name, value, predicate, numBase);
			return executed;
		}
		/// <summary>Stream the Value of element <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Element name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <param name="numBase">numerical base to use</param>
		/// <returns>True if <paramref name="value"/> was read/written from/to stream</returns>
		/// <seealso cref="ReadElementOpt(TName, ref sbyte, NumeralBase)"/>
		/// <seealso cref="WriteElementOptOnTrue(TName, sbyte, Predicate{sbyte}, NumeralBase)"/>
		public bool StreamElementOpt(TName name, ref sbyte value, Predicate<sbyte> predicate = null, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));

			if (predicate == null)
				predicate = Predicates.True< sbyte >;

			bool executed = false;
				 if (this.IsReading) executed = this.ReadElementOpt(name, ref value, numBase);
			else if (this.IsWriting) executed = this.WriteElementOptOnTrue(name, value, predicate, numBase);
			return executed;
		}
		/// <summary>Stream the Value of element <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Element name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <param name="numBase">numerical base to use</param>
		/// <returns>True if <paramref name="value"/> was read/written from/to stream</returns>
		/// <seealso cref="ReadElementOpt(TName, ref ushort, NumeralBase)"/>
		/// <seealso cref="WriteElementOptOnTrue(TName, ushort, Predicate{ushort}, NumeralBase)"/>
		public bool StreamElementOpt(TName name, ref ushort value, Predicate<ushort> predicate = null, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));

			if (predicate == null)
				predicate = Predicates.True< ushort >;

			bool executed = false;
				 if (this.IsReading) executed = this.ReadElementOpt(name, ref value, numBase);
			else if (this.IsWriting) executed = this.WriteElementOptOnTrue(name, value, predicate, numBase);
			return executed;
		}
		/// <summary>Stream the Value of element <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Element name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <param name="numBase">numerical base to use</param>
		/// <returns>True if <paramref name="value"/> was read/written from/to stream</returns>
		/// <seealso cref="ReadElementOpt(TName, ref short, NumeralBase)"/>
		/// <seealso cref="WriteElementOptOnTrue(TName, short, Predicate{short}, NumeralBase)"/>
		public bool StreamElementOpt(TName name, ref short value, Predicate<short> predicate = null, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));

			if (predicate == null)
				predicate = Predicates.True< short >;

			bool executed = false;
				 if (this.IsReading) executed = this.ReadElementOpt(name, ref value, numBase);
			else if (this.IsWriting) executed = this.WriteElementOptOnTrue(name, value, predicate, numBase);
			return executed;
		}
		/// <summary>Stream the Value of element <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Element name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <param name="numBase">numerical base to use</param>
		/// <returns>True if <paramref name="value"/> was read/written from/to stream</returns>
		/// <seealso cref="ReadElementOpt(TName, ref uint, NumeralBase)"/>
		/// <seealso cref="WriteElementOptOnTrue(TName, uint, Predicate{uint}, NumeralBase)"/>
		public bool StreamElementOpt(TName name, ref uint value, Predicate<uint> predicate = null, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));

			if (predicate == null)
				predicate = Predicates.True< uint >;

			bool executed = false;
				 if (this.IsReading) executed = this.ReadElementOpt(name, ref value, numBase);
			else if (this.IsWriting) executed = this.WriteElementOptOnTrue(name, value, predicate, numBase);
			return executed;
		}
		/// <summary>Stream the Value of element <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Element name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <param name="numBase">numerical base to use</param>
		/// <returns>True if <paramref name="value"/> was read/written from/to stream</returns>
		/// <seealso cref="ReadElementOpt(TName, ref int, NumeralBase)"/>
		/// <seealso cref="WriteElementOptOnTrue(TName, int, Predicate{int}, NumeralBase)"/>
		public bool StreamElementOpt(TName name, ref int value, Predicate<int> predicate = null, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));

			if (predicate == null)
				predicate = Predicates.True< int >;

			bool executed = false;
				 if (this.IsReading) executed = this.ReadElementOpt(name, ref value, numBase);
			else if (this.IsWriting) executed = this.WriteElementOptOnTrue(name, value, predicate, numBase);
			return executed;
		}
		/// <summary>Stream the Value of element <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Element name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <param name="numBase">numerical base to use</param>
		/// <returns>True if <paramref name="value"/> was read/written from/to stream</returns>
		/// <seealso cref="ReadElementOpt(TName, ref ulong, NumeralBase)"/>
		/// <seealso cref="WriteElementOptOnTrue(TName, ulong, Predicate{ulong}, NumeralBase)"/>
		public bool StreamElementOpt(TName name, ref ulong value, Predicate<ulong> predicate = null, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));

			if (predicate == null)
				predicate = Predicates.True< ulong >;

			bool executed = false;
				 if (this.IsReading) executed = this.ReadElementOpt(name, ref value, numBase);
			else if (this.IsWriting) executed = this.WriteElementOptOnTrue(name, value, predicate, numBase);
			return executed;
		}
		/// <summary>Stream the Value of element <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Element name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <param name="numBase">numerical base to use</param>
		/// <returns>True if <paramref name="value"/> was read/written from/to stream</returns>
		/// <seealso cref="ReadElementOpt(TName, ref long, NumeralBase)"/>
		/// <seealso cref="WriteElementOptOnTrue(TName, long, Predicate{long}, NumeralBase)"/>
		public bool StreamElementOpt(TName name, ref long value, Predicate<long> predicate = null, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));

			if (predicate == null)
				predicate = Predicates.True< long >;

			bool executed = false;
				 if (this.IsReading) executed = this.ReadElementOpt(name, ref value, numBase);
			else if (this.IsWriting) executed = this.WriteElementOptOnTrue(name, value, predicate, numBase);
			return executed;
		}

		public bool StreamElementOpt<T>(TName name, T theObj, Exprs.Expression<Func<T, string >> propExpr , Predicate<string> predicate = null )
		{
			Contract.Requires(this.ValidateNameArg(name));

			if (predicate == null)
				predicate = x => true;

			bool executed = false;
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (this.IsReading)
			{
				var value = default( string );
				executed = this.ReadElementOpt(name, ref value );
				if (executed)
				{
					property.SetValue(theObj, value, null);
				}
			}
			else if (this.IsWriting)
				executed = this.WriteElementOptOnTrue(name, (string)property.GetValue(theObj, null) , predicate);

			return executed;
		}
		public bool StreamElementOpt<T>(TName name, T theObj, Exprs.Expression<Func<T, char >> propExpr , Predicate<char> predicate = null )
		{
			Contract.Requires(this.ValidateNameArg(name));

			if (predicate == null)
				predicate = x => true;

			bool executed = false;
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (this.IsReading)
			{
				var value = default( char );
				executed = this.ReadElementOpt(name, ref value );
				if (executed)
				{
					property.SetValue(theObj, value, null);
				}
			}
			else if (this.IsWriting)
				executed = this.WriteElementOptOnTrue(name, (char)property.GetValue(theObj, null) , predicate);

			return executed;
		}
		public bool StreamElementOpt<T>(TName name, T theObj, Exprs.Expression<Func<T, bool >> propExpr , Predicate<bool> predicate = null )
		{
			Contract.Requires(this.ValidateNameArg(name));

			if (predicate == null)
				predicate = x => true;

			bool executed = false;
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (this.IsReading)
			{
				var value = default( bool );
				executed = this.ReadElementOpt(name, ref value );
				if (executed)
				{
					property.SetValue(theObj, value, null);
				}
			}
			else if (this.IsWriting)
				executed = this.WriteElementOptOnTrue(name, (bool)property.GetValue(theObj, null) , predicate);

			return executed;
		}
		public bool StreamElementOpt<T>(TName name, T theObj, Exprs.Expression<Func<T, float >> propExpr , Predicate<float> predicate = null )
		{
			Contract.Requires(this.ValidateNameArg(name));

			if (predicate == null)
				predicate = x => true;

			bool executed = false;
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (this.IsReading)
			{
				var value = default( float );
				executed = this.ReadElementOpt(name, ref value );
				if (executed)
				{
					property.SetValue(theObj, value, null);
				}
			}
			else if (this.IsWriting)
				executed = this.WriteElementOptOnTrue(name, (float)property.GetValue(theObj, null) , predicate);

			return executed;
		}
		public bool StreamElementOpt<T>(TName name, T theObj, Exprs.Expression<Func<T, double >> propExpr , Predicate<double> predicate = null )
		{
			Contract.Requires(this.ValidateNameArg(name));

			if (predicate == null)
				predicate = x => true;

			bool executed = false;
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (this.IsReading)
			{
				var value = default( double );
				executed = this.ReadElementOpt(name, ref value );
				if (executed)
				{
					property.SetValue(theObj, value, null);
				}
			}
			else if (this.IsWriting)
				executed = this.WriteElementOptOnTrue(name, (double)property.GetValue(theObj, null) , predicate);

			return executed;
		}

		public bool StreamElementOpt<T>(TName name, T theObj, Exprs.Expression<Func<T, byte >> propExpr , Predicate<byte> predicate = null , NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));

			if (predicate == null)
				predicate = x => true;

			bool executed = false;
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (this.IsReading)
			{
				var value = default( byte );
				executed = this.ReadElementOpt(name, ref value , numBase);
				if (executed)
				{
					property.SetValue(theObj, value, null);
				}
			}
			else if (this.IsWriting)
				executed = this.WriteElementOptOnTrue(name, (byte)property.GetValue(theObj, null) , predicate, numBase);

			return executed;
		}
		public bool StreamElementOpt<T>(TName name, T theObj, Exprs.Expression<Func<T, sbyte >> propExpr , Predicate<sbyte> predicate = null , NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));

			if (predicate == null)
				predicate = x => true;

			bool executed = false;
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (this.IsReading)
			{
				var value = default( sbyte );
				executed = this.ReadElementOpt(name, ref value , numBase);
				if (executed)
				{
					property.SetValue(theObj, value, null);
				}
			}
			else if (this.IsWriting)
				executed = this.WriteElementOptOnTrue(name, (sbyte)property.GetValue(theObj, null) , predicate, numBase);

			return executed;
		}
		public bool StreamElementOpt<T>(TName name, T theObj, Exprs.Expression<Func<T, ushort >> propExpr , Predicate<ushort> predicate = null , NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));

			if (predicate == null)
				predicate = x => true;

			bool executed = false;
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (this.IsReading)
			{
				var value = default( ushort );
				executed = this.ReadElementOpt(name, ref value , numBase);
				if (executed)
				{
					property.SetValue(theObj, value, null);
				}
			}
			else if (this.IsWriting)
				executed = this.WriteElementOptOnTrue(name, (ushort)property.GetValue(theObj, null) , predicate, numBase);

			return executed;
		}
		public bool StreamElementOpt<T>(TName name, T theObj, Exprs.Expression<Func<T, short >> propExpr , Predicate<short> predicate = null , NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));

			if (predicate == null)
				predicate = x => true;

			bool executed = false;
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (this.IsReading)
			{
				var value = default( short );
				executed = this.ReadElementOpt(name, ref value , numBase);
				if (executed)
				{
					property.SetValue(theObj, value, null);
				}
			}
			else if (this.IsWriting)
				executed = this.WriteElementOptOnTrue(name, (short)property.GetValue(theObj, null) , predicate, numBase);

			return executed;
		}
		public bool StreamElementOpt<T>(TName name, T theObj, Exprs.Expression<Func<T, uint >> propExpr , Predicate<uint> predicate = null , NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));

			if (predicate == null)
				predicate = x => true;

			bool executed = false;
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (this.IsReading)
			{
				var value = default( uint );
				executed = this.ReadElementOpt(name, ref value , numBase);
				if (executed)
				{
					property.SetValue(theObj, value, null);
				}
			}
			else if (this.IsWriting)
				executed = this.WriteElementOptOnTrue(name, (uint)property.GetValue(theObj, null) , predicate, numBase);

			return executed;
		}
		public bool StreamElementOpt<T>(TName name, T theObj, Exprs.Expression<Func<T, int >> propExpr , Predicate<int> predicate = null , NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));

			if (predicate == null)
				predicate = x => true;

			bool executed = false;
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (this.IsReading)
			{
				var value = default( int );
				executed = this.ReadElementOpt(name, ref value , numBase);
				if (executed)
				{
					property.SetValue(theObj, value, null);
				}
			}
			else if (this.IsWriting)
				executed = this.WriteElementOptOnTrue(name, (int)property.GetValue(theObj, null) , predicate, numBase);

			return executed;
		}
		public bool StreamElementOpt<T>(TName name, T theObj, Exprs.Expression<Func<T, ulong >> propExpr , Predicate<ulong> predicate = null , NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));

			if (predicate == null)
				predicate = x => true;

			bool executed = false;
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (this.IsReading)
			{
				var value = default( ulong );
				executed = this.ReadElementOpt(name, ref value , numBase);
				if (executed)
				{
					property.SetValue(theObj, value, null);
				}
			}
			else if (this.IsWriting)
				executed = this.WriteElementOptOnTrue(name, (ulong)property.GetValue(theObj, null) , predicate, numBase);

			return executed;
		}
		public bool StreamElementOpt<T>(TName name, T theObj, Exprs.Expression<Func<T, long >> propExpr , Predicate<long> predicate = null , NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));

			if (predicate == null)
				predicate = x => true;

			bool executed = false;
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (this.IsReading)
			{
				var value = default( long );
				executed = this.ReadElementOpt(name, ref value , numBase);
				if (executed)
				{
					property.SetValue(theObj, value, null);
				}
			}
			else if (this.IsWriting)
				executed = this.WriteElementOptOnTrue(name, (long)property.GetValue(theObj, null) , predicate, numBase);

			return executed;
		}

		public bool StreamElementOpt<T>(TName name, T theObj, Exprs.Expression<Func<T, Values.KGuid >> propExpr , Predicate<Values.KGuid> predicate = null )
		{
			Contract.Requires(this.ValidateNameArg(name));

			if (predicate == null)
				predicate = x => true;

			bool executed = false;
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (this.IsReading)
			{
				var value = default( Values.KGuid );
				executed = this.ReadElementOpt(name, ref value );
				if (executed)
				{
					property.SetValue(theObj, value, null);
				}
			}
			else if (this.IsWriting)
				executed = this.WriteElementOptOnTrue(name, (Values.KGuid)property.GetValue(theObj, null) , predicate);

			return executed;
		}
		#endregion


		#region Stream Attribute
		/// <summary>Stream the Value of attribute <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Attribute name</param>
		/// <param name="value">Source or destination value</param>
		/// <seealso cref="ReadAttribute(TName, ref string)"/>
		/// <seealso cref="WriteAttribute(TName, string)"/>
		public void StreamAttribute(TName name, ref string value)
		{
			Contract.Requires(this.ValidateNameArg(name));

				 if (this.IsReading)
					 this.ReadAttribute(name, ref value);
			else if (this.IsWriting)
				this.WriteAttribute(name, value);
		}
		/// <summary>Stream the Value of attribute <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Attribute name</param>
		/// <param name="value">Source or destination value</param>
		/// <seealso cref="ReadAttribute(TName, ref char)"/>
		/// <seealso cref="WriteAttribute(TName, char)"/>
		public void StreamAttribute(TName name, ref char value)
		{
			Contract.Requires(this.ValidateNameArg(name));

				 if (this.IsReading)
					 this.ReadAttribute(name, ref value);
			else if (this.IsWriting)
				this.WriteAttribute(name, value);
		}
		/// <summary>Stream the Value of attribute <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Attribute name</param>
		/// <param name="value">Source or destination value</param>
		/// <seealso cref="ReadAttribute(TName, ref bool)"/>
		/// <seealso cref="WriteAttribute(TName, bool)"/>
		public void StreamAttribute(TName name, ref bool value)
		{
			Contract.Requires(this.ValidateNameArg(name));

				 if (this.IsReading)
					 this.ReadAttribute(name, ref value);
			else if (this.IsWriting)
				this.WriteAttribute(name, value);
		}
		/// <summary>Stream the Value of attribute <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Attribute name</param>
		/// <param name="value">Source or destination value</param>
		/// <seealso cref="ReadAttribute(TName, ref float)"/>
		/// <seealso cref="WriteAttribute(TName, float)"/>
		public void StreamAttribute(TName name, ref float value)
		{
			Contract.Requires(this.ValidateNameArg(name));

				 if (this.IsReading)
					 this.ReadAttribute(name, ref value);
			else if (this.IsWriting)
				this.WriteAttribute(name, value);
		}
		/// <summary>Stream the Value of attribute <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Attribute name</param>
		/// <param name="value">Source or destination value</param>
		/// <seealso cref="ReadAttribute(TName, ref double)"/>
		/// <seealso cref="WriteAttribute(TName, double)"/>
		public void StreamAttribute(TName name, ref double value)
		{
			Contract.Requires(this.ValidateNameArg(name));

				 if (this.IsReading)
					 this.ReadAttribute(name, ref value);
			else if (this.IsWriting)
				this.WriteAttribute(name, value);
		}

		/// <summary>Stream the Value of attribute <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Attribute name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="numBase">numerical base to use</param>
		/// <seealso cref="ReadAttribute(TName, ref byte, NumeralBase)"/>
		/// <seealso cref="WriteAttribute(TName, byte, NumeralBase)"/>
		public void StreamAttribute(TName name, ref byte value, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));

				 if (this.IsReading)
					 this.ReadAttribute(name, ref value, numBase);
			else if (this.IsWriting)
				this.WriteAttribute(name, value, numBase);
		}
		/// <summary>Stream the Value of attribute <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Attribute name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="numBase">numerical base to use</param>
		/// <seealso cref="ReadAttribute(TName, ref sbyte, NumeralBase)"/>
		/// <seealso cref="WriteAttribute(TName, sbyte, NumeralBase)"/>
		public void StreamAttribute(TName name, ref sbyte value, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));

				 if (this.IsReading)
					 this.ReadAttribute(name, ref value, numBase);
			else if (this.IsWriting)
				this.WriteAttribute(name, value, numBase);
		}
		/// <summary>Stream the Value of attribute <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Attribute name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="numBase">numerical base to use</param>
		/// <seealso cref="ReadAttribute(TName, ref ushort, NumeralBase)"/>
		/// <seealso cref="WriteAttribute(TName, ushort, NumeralBase)"/>
		public void StreamAttribute(TName name, ref ushort value, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));

				 if (this.IsReading)
					 this.ReadAttribute(name, ref value, numBase);
			else if (this.IsWriting)
				this.WriteAttribute(name, value, numBase);
		}
		/// <summary>Stream the Value of attribute <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Attribute name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="numBase">numerical base to use</param>
		/// <seealso cref="ReadAttribute(TName, ref short, NumeralBase)"/>
		/// <seealso cref="WriteAttribute(TName, short, NumeralBase)"/>
		public void StreamAttribute(TName name, ref short value, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));

				 if (this.IsReading)
					 this.ReadAttribute(name, ref value, numBase);
			else if (this.IsWriting)
				this.WriteAttribute(name, value, numBase);
		}
		/// <summary>Stream the Value of attribute <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Attribute name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="numBase">numerical base to use</param>
		/// <seealso cref="ReadAttribute(TName, ref uint, NumeralBase)"/>
		/// <seealso cref="WriteAttribute(TName, uint, NumeralBase)"/>
		public void StreamAttribute(TName name, ref uint value, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));

				 if (this.IsReading)
					 this.ReadAttribute(name, ref value, numBase);
			else if (this.IsWriting)
				this.WriteAttribute(name, value, numBase);
		}
		/// <summary>Stream the Value of attribute <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Attribute name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="numBase">numerical base to use</param>
		/// <seealso cref="ReadAttribute(TName, ref int, NumeralBase)"/>
		/// <seealso cref="WriteAttribute(TName, int, NumeralBase)"/>
		public void StreamAttribute(TName name, ref int value, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));

				 if (this.IsReading)
					 this.ReadAttribute(name, ref value, numBase);
			else if (this.IsWriting)
				this.WriteAttribute(name, value, numBase);
		}
		/// <summary>Stream the Value of attribute <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Attribute name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="numBase">numerical base to use</param>
		/// <seealso cref="ReadAttribute(TName, ref ulong, NumeralBase)"/>
		/// <seealso cref="WriteAttribute(TName, ulong, NumeralBase)"/>
		public void StreamAttribute(TName name, ref ulong value, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));

				 if (this.IsReading)
					 this.ReadAttribute(name, ref value, numBase);
			else if (this.IsWriting)
				this.WriteAttribute(name, value, numBase);
		}
		/// <summary>Stream the Value of attribute <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Attribute name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="numBase">numerical base to use</param>
		/// <seealso cref="ReadAttribute(TName, ref long, NumeralBase)"/>
		/// <seealso cref="WriteAttribute(TName, long, NumeralBase)"/>
		public void StreamAttribute(TName name, ref long value, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));

				 if (this.IsReading)
					 this.ReadAttribute(name, ref value, numBase);
			else if (this.IsWriting)
				this.WriteAttribute(name, value, numBase);
		}

		public void StreamAttribute<T>(TName name, T theObj, Exprs.Expression<Func<T, string >> propExpr  )
		{
			Contract.Requires(this.ValidateNameArg(name));

			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (this.IsReading)
			{
				var value = default( string );
				this.ReadAttribute(name, ref value );
				{
					property.SetValue(theObj, value, null);
				}
			}
			else if (this.IsWriting)
				this.WriteAttribute(name, (string)property.GetValue(theObj, null) );
		}
		public void StreamAttribute<T>(TName name, T theObj, Exprs.Expression<Func<T, char >> propExpr  )
		{
			Contract.Requires(this.ValidateNameArg(name));

			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (this.IsReading)
			{
				var value = default( char );
				this.ReadAttribute(name, ref value );
				{
					property.SetValue(theObj, value, null);
				}
			}
			else if (this.IsWriting)
				this.WriteAttribute(name, (char)property.GetValue(theObj, null) );
		}
		public void StreamAttribute<T>(TName name, T theObj, Exprs.Expression<Func<T, bool >> propExpr  )
		{
			Contract.Requires(this.ValidateNameArg(name));

			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (this.IsReading)
			{
				var value = default( bool );
				this.ReadAttribute(name, ref value );
				{
					property.SetValue(theObj, value, null);
				}
			}
			else if (this.IsWriting)
				this.WriteAttribute(name, (bool)property.GetValue(theObj, null) );
		}
		public void StreamAttribute<T>(TName name, T theObj, Exprs.Expression<Func<T, float >> propExpr  )
		{
			Contract.Requires(this.ValidateNameArg(name));

			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (this.IsReading)
			{
				var value = default( float );
				this.ReadAttribute(name, ref value );
				{
					property.SetValue(theObj, value, null);
				}
			}
			else if (this.IsWriting)
				this.WriteAttribute(name, (float)property.GetValue(theObj, null) );
		}
		public void StreamAttribute<T>(TName name, T theObj, Exprs.Expression<Func<T, double >> propExpr  )
		{
			Contract.Requires(this.ValidateNameArg(name));

			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (this.IsReading)
			{
				var value = default( double );
				this.ReadAttribute(name, ref value );
				{
					property.SetValue(theObj, value, null);
				}
			}
			else if (this.IsWriting)
				this.WriteAttribute(name, (double)property.GetValue(theObj, null) );
		}

		public void StreamAttribute<T>(TName name, T theObj, Exprs.Expression<Func<T, byte >> propExpr  , NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));

			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (this.IsReading)
			{
				var value = default( byte );
				this.ReadAttribute(name, ref value , numBase);
				{
					property.SetValue(theObj, value, null);
				}
			}
			else if (this.IsWriting)
				this.WriteAttribute(name, (byte)property.GetValue(theObj, null) , numBase);
		}
		public void StreamAttribute<T>(TName name, T theObj, Exprs.Expression<Func<T, sbyte >> propExpr  , NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));

			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (this.IsReading)
			{
				var value = default( sbyte );
				this.ReadAttribute(name, ref value , numBase);
				{
					property.SetValue(theObj, value, null);
				}
			}
			else if (this.IsWriting)
				this.WriteAttribute(name, (sbyte)property.GetValue(theObj, null) , numBase);
		}
		public void StreamAttribute<T>(TName name, T theObj, Exprs.Expression<Func<T, ushort >> propExpr  , NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));

			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (this.IsReading)
			{
				var value = default( ushort );
				this.ReadAttribute(name, ref value , numBase);
				{
					property.SetValue(theObj, value, null);
				}
			}
			else if (this.IsWriting)
				this.WriteAttribute(name, (ushort)property.GetValue(theObj, null) , numBase);
		}
		public void StreamAttribute<T>(TName name, T theObj, Exprs.Expression<Func<T, short >> propExpr  , NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));

			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (this.IsReading)
			{
				var value = default( short );
				this.ReadAttribute(name, ref value , numBase);
				{
					property.SetValue(theObj, value, null);
				}
			}
			else if (this.IsWriting)
				this.WriteAttribute(name, (short)property.GetValue(theObj, null) , numBase);
		}
		public void StreamAttribute<T>(TName name, T theObj, Exprs.Expression<Func<T, uint >> propExpr  , NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));

			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (this.IsReading)
			{
				var value = default( uint );
				this.ReadAttribute(name, ref value , numBase);
				{
					property.SetValue(theObj, value, null);
				}
			}
			else if (this.IsWriting)
				this.WriteAttribute(name, (uint)property.GetValue(theObj, null) , numBase);
		}
		public void StreamAttribute<T>(TName name, T theObj, Exprs.Expression<Func<T, int >> propExpr  , NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));

			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (this.IsReading)
			{
				var value = default( int );
				this.ReadAttribute(name, ref value , numBase);
				{
					property.SetValue(theObj, value, null);
				}
			}
			else if (this.IsWriting)
				this.WriteAttribute(name, (int)property.GetValue(theObj, null) , numBase);
		}
		public void StreamAttribute<T>(TName name, T theObj, Exprs.Expression<Func<T, ulong >> propExpr  , NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));

			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (this.IsReading)
			{
				var value = default( ulong );
				this.ReadAttribute(name, ref value , numBase);
				{
					property.SetValue(theObj, value, null);
				}
			}
			else if (this.IsWriting)
				this.WriteAttribute(name, (ulong)property.GetValue(theObj, null) , numBase);
		}
		public void StreamAttribute<T>(TName name, T theObj, Exprs.Expression<Func<T, long >> propExpr  , NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));

			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (this.IsReading)
			{
				var value = default( long );
				this.ReadAttribute(name, ref value , numBase);
				{
					property.SetValue(theObj, value, null);
				}
			}
			else if (this.IsWriting)
				this.WriteAttribute(name, (long)property.GetValue(theObj, null) , numBase);
		}

		public void StreamAttribute<T>(TName name, T theObj, Exprs.Expression<Func<T, Values.KGuid >> propExpr  )
		{
			Contract.Requires(this.ValidateNameArg(name));

			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (this.IsReading)
			{
				var value = default( Values.KGuid );
				this.ReadAttribute(name, ref value );
				{
					property.SetValue(theObj, value, null);
				}
			}
			else if (this.IsWriting)
				this.WriteAttribute(name, (Values.KGuid)property.GetValue(theObj, null) );
		}
		#endregion


		#region StreamAttributeOpt
		/// <summary>Stream the Value of attribute <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Attribute name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <returns>True if <paramref name="value"/> was read/written from/to stream</returns>
		/// <seealso cref="ReadAttributeOpt(TName, ref string)"/>
		/// <seealso cref="WriteAttributeOptOnTrue(TName, string, Predicate{string})"/>
		public bool StreamAttributeOpt(TName name, ref string value, Predicate<string> predicate = null)
		{
			Contract.Requires(this.ValidateNameArg(name));

			if (predicate == null)
				predicate = Predicates.True< string >;

			bool executed = false;
				 if (this.IsReading) executed = this.ReadAttributeOpt(name, ref value);
			else if (this.IsWriting) executed = this.WriteAttributeOptOnTrue(name, value, predicate);
			return executed;
		}
		/// <summary>Stream the Value of attribute <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Attribute name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <returns>True if <paramref name="value"/> was read/written from/to stream</returns>
		/// <seealso cref="ReadAttributeOpt(TName, ref char)"/>
		/// <seealso cref="WriteAttributeOptOnTrue(TName, char, Predicate{char})"/>
		public bool StreamAttributeOpt(TName name, ref char value, Predicate<char> predicate = null)
		{
			Contract.Requires(this.ValidateNameArg(name));

			if (predicate == null)
				predicate = Predicates.True< char >;

			bool executed = false;
				 if (this.IsReading) executed = this.ReadAttributeOpt(name, ref value);
			else if (this.IsWriting) executed = this.WriteAttributeOptOnTrue(name, value, predicate);
			return executed;
		}
		/// <summary>Stream the Value of attribute <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Attribute name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <returns>True if <paramref name="value"/> was read/written from/to stream</returns>
		/// <seealso cref="ReadAttributeOpt(TName, ref bool)"/>
		/// <seealso cref="WriteAttributeOptOnTrue(TName, bool, Predicate{bool})"/>
		public bool StreamAttributeOpt(TName name, ref bool value, Predicate<bool> predicate = null)
		{
			Contract.Requires(this.ValidateNameArg(name));

			if (predicate == null)
				predicate = Predicates.True< bool >;

			bool executed = false;
				 if (this.IsReading) executed = this.ReadAttributeOpt(name, ref value);
			else if (this.IsWriting) executed = this.WriteAttributeOptOnTrue(name, value, predicate);
			return executed;
		}
		/// <summary>Stream the Value of attribute <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Attribute name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <returns>True if <paramref name="value"/> was read/written from/to stream</returns>
		/// <seealso cref="ReadAttributeOpt(TName, ref float)"/>
		/// <seealso cref="WriteAttributeOptOnTrue(TName, float, Predicate{float})"/>
		public bool StreamAttributeOpt(TName name, ref float value, Predicate<float> predicate = null)
		{
			Contract.Requires(this.ValidateNameArg(name));

			if (predicate == null)
				predicate = Predicates.True< float >;

			bool executed = false;
				 if (this.IsReading) executed = this.ReadAttributeOpt(name, ref value);
			else if (this.IsWriting) executed = this.WriteAttributeOptOnTrue(name, value, predicate);
			return executed;
		}
		/// <summary>Stream the Value of attribute <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Attribute name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <returns>True if <paramref name="value"/> was read/written from/to stream</returns>
		/// <seealso cref="ReadAttributeOpt(TName, ref double)"/>
		/// <seealso cref="WriteAttributeOptOnTrue(TName, double, Predicate{double})"/>
		public bool StreamAttributeOpt(TName name, ref double value, Predicate<double> predicate = null)
		{
			Contract.Requires(this.ValidateNameArg(name));

			if (predicate == null)
				predicate = Predicates.True< double >;

			bool executed = false;
				 if (this.IsReading) executed = this.ReadAttributeOpt(name, ref value);
			else if (this.IsWriting) executed = this.WriteAttributeOptOnTrue(name, value, predicate);
			return executed;
		}

		/// <summary>Stream the Value of attribute <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Attribute name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <param name="numBase">numerical base to use</param>
		/// <returns>True if <paramref name="value"/> was read/written from/to stream</returns>
		/// <seealso cref="ReadAttributeOpt(string, ref byte, NumeralBase)"/>
		/// <seealso cref="WriteAttributeOptOnTrue(string, byte, Predicate{byte}, NumeralBase)"/>
		public bool StreamAttributeOpt(TName name, ref byte value, Predicate<byte> predicate = null, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));

			if (predicate == null)
				predicate = Predicates.True< byte >;

			bool executed = false;
				 if (this.IsReading) executed = this.ReadAttributeOpt(name, ref value, numBase);
			else if (this.IsWriting) executed = this.WriteAttributeOptOnTrue(name, value, predicate, numBase);
			return executed;
		}
		/// <summary>Stream the Value of attribute <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Attribute name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <param name="numBase">numerical base to use</param>
		/// <returns>True if <paramref name="value"/> was read/written from/to stream</returns>
		/// <seealso cref="ReadAttributeOpt(string, ref sbyte, NumeralBase)"/>
		/// <seealso cref="WriteAttributeOptOnTrue(string, sbyte, Predicate{sbyte}, NumeralBase)"/>
		public bool StreamAttributeOpt(TName name, ref sbyte value, Predicate<sbyte> predicate = null, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));

			if (predicate == null)
				predicate = Predicates.True< sbyte >;

			bool executed = false;
				 if (this.IsReading) executed = this.ReadAttributeOpt(name, ref value, numBase);
			else if (this.IsWriting) executed = this.WriteAttributeOptOnTrue(name, value, predicate, numBase);
			return executed;
		}
		/// <summary>Stream the Value of attribute <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Attribute name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <param name="numBase">numerical base to use</param>
		/// <returns>True if <paramref name="value"/> was read/written from/to stream</returns>
		/// <seealso cref="ReadAttributeOpt(string, ref ushort, NumeralBase)"/>
		/// <seealso cref="WriteAttributeOptOnTrue(string, ushort, Predicate{ushort}, NumeralBase)"/>
		public bool StreamAttributeOpt(TName name, ref ushort value, Predicate<ushort> predicate = null, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));

			if (predicate == null)
				predicate = Predicates.True< ushort >;

			bool executed = false;
				 if (this.IsReading) executed = this.ReadAttributeOpt(name, ref value, numBase);
			else if (this.IsWriting) executed = this.WriteAttributeOptOnTrue(name, value, predicate, numBase);
			return executed;
		}
		/// <summary>Stream the Value of attribute <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Attribute name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <param name="numBase">numerical base to use</param>
		/// <returns>True if <paramref name="value"/> was read/written from/to stream</returns>
		/// <seealso cref="ReadAttributeOpt(string, ref short, NumeralBase)"/>
		/// <seealso cref="WriteAttributeOptOnTrue(string, short, Predicate{short}, NumeralBase)"/>
		public bool StreamAttributeOpt(TName name, ref short value, Predicate<short> predicate = null, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));

			if (predicate == null)
				predicate = Predicates.True< short >;

			bool executed = false;
				 if (this.IsReading) executed = this.ReadAttributeOpt(name, ref value, numBase);
			else if (this.IsWriting) executed = this.WriteAttributeOptOnTrue(name, value, predicate, numBase);
			return executed;
		}
		/// <summary>Stream the Value of attribute <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Attribute name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <param name="numBase">numerical base to use</param>
		/// <returns>True if <paramref name="value"/> was read/written from/to stream</returns>
		/// <seealso cref="ReadAttributeOpt(string, ref uint, NumeralBase)"/>
		/// <seealso cref="WriteAttributeOptOnTrue(string, uint, Predicate{uint}, NumeralBase)"/>
		public bool StreamAttributeOpt(TName name, ref uint value, Predicate<uint> predicate = null, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));

			if (predicate == null)
				predicate = Predicates.True< uint >;

			bool executed = false;
				 if (this.IsReading) executed = this.ReadAttributeOpt(name, ref value, numBase);
			else if (this.IsWriting) executed = this.WriteAttributeOptOnTrue(name, value, predicate, numBase);
			return executed;
		}
		/// <summary>Stream the Value of attribute <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Attribute name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <param name="numBase">numerical base to use</param>
		/// <returns>True if <paramref name="value"/> was read/written from/to stream</returns>
		/// <seealso cref="ReadAttributeOpt(string, ref int, NumeralBase)"/>
		/// <seealso cref="WriteAttributeOptOnTrue(string, int, Predicate{int}, NumeralBase)"/>
		public bool StreamAttributeOpt(TName name, ref int value, Predicate<int> predicate = null, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));

			if (predicate == null)
				predicate = Predicates.True< int >;

			bool executed = false;
				 if (this.IsReading) executed = this.ReadAttributeOpt(name, ref value, numBase);
			else if (this.IsWriting) executed = this.WriteAttributeOptOnTrue(name, value, predicate, numBase);
			return executed;
		}
		/// <summary>Stream the Value of attribute <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Attribute name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <param name="numBase">numerical base to use</param>
		/// <returns>True if <paramref name="value"/> was read/written from/to stream</returns>
		/// <seealso cref="ReadAttributeOpt(string, ref ulong, NumeralBase)"/>
		/// <seealso cref="WriteAttributeOptOnTrue(string, ulong, Predicate{ulong}, NumeralBase)"/>
		public bool StreamAttributeOpt(TName name, ref ulong value, Predicate<ulong> predicate = null, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));

			if (predicate == null)
				predicate = Predicates.True< ulong >;

			bool executed = false;
				 if (this.IsReading) executed = this.ReadAttributeOpt(name, ref value, numBase);
			else if (this.IsWriting) executed = this.WriteAttributeOptOnTrue(name, value, predicate, numBase);
			return executed;
		}
		/// <summary>Stream the Value of attribute <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Attribute name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <param name="numBase">numerical base to use</param>
		/// <returns>True if <paramref name="value"/> was read/written from/to stream</returns>
		/// <seealso cref="ReadAttributeOpt(string, ref long, NumeralBase)"/>
		/// <seealso cref="WriteAttributeOptOnTrue(string, long, Predicate{long}, NumeralBase)"/>
		public bool StreamAttributeOpt(TName name, ref long value, Predicate<long> predicate = null, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));

			if (predicate == null)
				predicate = Predicates.True< long >;

			bool executed = false;
				 if (this.IsReading) executed = this.ReadAttributeOpt(name, ref value, numBase);
			else if (this.IsWriting) executed = this.WriteAttributeOptOnTrue(name, value, predicate, numBase);
			return executed;
		}

		public bool StreamAttributeOpt<T>(TName name, T theObj, Exprs.Expression<Func<T, string >> propExpr , Predicate<string> predicate = null )
		{
			Contract.Requires(this.ValidateNameArg(name));

			if (predicate == null)
				predicate = x => true;

			bool executed = false;
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (this.IsReading)
			{
				var value = default( string );
				executed = this.ReadAttributeOpt(name, ref value );
				if (executed)
				{
					property.SetValue(theObj, value, null);
				}
			}
			else if (this.IsWriting)
				executed = this.WriteAttributeOptOnTrue(name, (string)property.GetValue(theObj, null) , predicate);

			return executed;
		}
		public bool StreamAttributeOpt<T>(TName name, T theObj, Exprs.Expression<Func<T, char >> propExpr , Predicate<char> predicate = null )
		{
			Contract.Requires(this.ValidateNameArg(name));

			if (predicate == null)
				predicate = x => true;

			bool executed = false;
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (this.IsReading)
			{
				var value = default( char );
				executed = this.ReadAttributeOpt(name, ref value );
				if (executed)
				{
					property.SetValue(theObj, value, null);
				}
			}
			else if (this.IsWriting)
				executed = this.WriteAttributeOptOnTrue(name, (char)property.GetValue(theObj, null) , predicate);

			return executed;
		}
		public bool StreamAttributeOpt<T>(TName name, T theObj, Exprs.Expression<Func<T, bool >> propExpr , Predicate<bool> predicate = null )
		{
			Contract.Requires(this.ValidateNameArg(name));

			if (predicate == null)
				predicate = x => true;

			bool executed = false;
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (this.IsReading)
			{
				var value = default( bool );
				executed = this.ReadAttributeOpt(name, ref value );
				if (executed)
				{
					property.SetValue(theObj, value, null);
				}
			}
			else if (this.IsWriting)
				executed = this.WriteAttributeOptOnTrue(name, (bool)property.GetValue(theObj, null) , predicate);

			return executed;
		}
		public bool StreamAttributeOpt<T>(TName name, T theObj, Exprs.Expression<Func<T, float >> propExpr , Predicate<float> predicate = null )
		{
			Contract.Requires(this.ValidateNameArg(name));

			if (predicate == null)
				predicate = x => true;

			bool executed = false;
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (this.IsReading)
			{
				var value = default( float );
				executed = this.ReadAttributeOpt(name, ref value );
				if (executed)
				{
					property.SetValue(theObj, value, null);
				}
			}
			else if (this.IsWriting)
				executed = this.WriteAttributeOptOnTrue(name, (float)property.GetValue(theObj, null) , predicate);

			return executed;
		}
		public bool StreamAttributeOpt<T>(TName name, T theObj, Exprs.Expression<Func<T, double >> propExpr , Predicate<double> predicate = null )
		{
			Contract.Requires(this.ValidateNameArg(name));

			if (predicate == null)
				predicate = x => true;

			bool executed = false;
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (this.IsReading)
			{
				var value = default( double );
				executed = this.ReadAttributeOpt(name, ref value );
				if (executed)
				{
					property.SetValue(theObj, value, null);
				}
			}
			else if (this.IsWriting)
				executed = this.WriteAttributeOptOnTrue(name, (double)property.GetValue(theObj, null) , predicate);

			return executed;
		}

		public bool StreamAttributeOpt<T>(TName name, T theObj, Exprs.Expression<Func<T, byte >> propExpr , Predicate<byte> predicate = null , NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));

			if (predicate == null)
				predicate = x => true;

			bool executed = false;
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (this.IsReading)
			{
				var value = default( byte );
				executed = this.ReadAttributeOpt(name, ref value , numBase);
				if (executed)
				{
					property.SetValue(theObj, value, null);
				}
			}
			else if (this.IsWriting)
				executed = this.WriteAttributeOptOnTrue(name, (byte)property.GetValue(theObj, null) , predicate, numBase);

			return executed;
		}
		public bool StreamAttributeOpt<T>(TName name, T theObj, Exprs.Expression<Func<T, sbyte >> propExpr , Predicate<sbyte> predicate = null , NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));

			if (predicate == null)
				predicate = x => true;

			bool executed = false;
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (this.IsReading)
			{
				var value = default( sbyte );
				executed = this.ReadAttributeOpt(name, ref value , numBase);
				if (executed)
				{
					property.SetValue(theObj, value, null);
				}
			}
			else if (this.IsWriting)
				executed = this.WriteAttributeOptOnTrue(name, (sbyte)property.GetValue(theObj, null) , predicate, numBase);

			return executed;
		}
		public bool StreamAttributeOpt<T>(TName name, T theObj, Exprs.Expression<Func<T, ushort >> propExpr , Predicate<ushort> predicate = null , NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));

			if (predicate == null)
				predicate = x => true;

			bool executed = false;
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (this.IsReading)
			{
				var value = default( ushort );
				executed = this.ReadAttributeOpt(name, ref value , numBase);
				if (executed)
				{
					property.SetValue(theObj, value, null);
				}
			}
			else if (this.IsWriting)
				executed = this.WriteAttributeOptOnTrue(name, (ushort)property.GetValue(theObj, null) , predicate, numBase);

			return executed;
		}
		public bool StreamAttributeOpt<T>(TName name, T theObj, Exprs.Expression<Func<T, short >> propExpr , Predicate<short> predicate = null , NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));

			if (predicate == null)
				predicate = x => true;

			bool executed = false;
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (this.IsReading)
			{
				var value = default( short );
				executed = this.ReadAttributeOpt(name, ref value , numBase);
				if (executed)
				{
					property.SetValue(theObj, value, null);
				}
			}
			else if (this.IsWriting)
				executed = this.WriteAttributeOptOnTrue(name, (short)property.GetValue(theObj, null) , predicate, numBase);

			return executed;
		}
		public bool StreamAttributeOpt<T>(TName name, T theObj, Exprs.Expression<Func<T, uint >> propExpr , Predicate<uint> predicate = null , NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));

			if (predicate == null)
				predicate = x => true;

			bool executed = false;
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (this.IsReading)
			{
				var value = default( uint );
				executed = this.ReadAttributeOpt(name, ref value , numBase);
				if (executed)
				{
					property.SetValue(theObj, value, null);
				}
			}
			else if (this.IsWriting)
				executed = this.WriteAttributeOptOnTrue(name, (uint)property.GetValue(theObj, null) , predicate, numBase);

			return executed;
		}
		public bool StreamAttributeOpt<T>(TName name, T theObj, Exprs.Expression<Func<T, int >> propExpr , Predicate<int> predicate = null , NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));

			if (predicate == null)
				predicate = x => true;

			bool executed = false;
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (this.IsReading)
			{
				var value = default( int );
				executed = this.ReadAttributeOpt(name, ref value , numBase);
				if (executed)
				{
					property.SetValue(theObj, value, null);
				}
			}
			else if (this.IsWriting)
				executed = this.WriteAttributeOptOnTrue(name, (int)property.GetValue(theObj, null) , predicate, numBase);

			return executed;
		}
		public bool StreamAttributeOpt<T>(TName name, T theObj, Exprs.Expression<Func<T, ulong >> propExpr , Predicate<ulong> predicate = null , NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));

			if (predicate == null)
				predicate = x => true;

			bool executed = false;
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (this.IsReading)
			{
				var value = default( ulong );
				executed = this.ReadAttributeOpt(name, ref value , numBase);
				if (executed)
				{
					property.SetValue(theObj, value, null);
				}
			}
			else if (this.IsWriting)
				executed = this.WriteAttributeOptOnTrue(name, (ulong)property.GetValue(theObj, null) , predicate, numBase);

			return executed;
		}
		public bool StreamAttributeOpt<T>(TName name, T theObj, Exprs.Expression<Func<T, long >> propExpr , Predicate<long> predicate = null , NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));

			if (predicate == null)
				predicate = x => true;

			bool executed = false;
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (this.IsReading)
			{
				var value = default( long );
				executed = this.ReadAttributeOpt(name, ref value , numBase);
				if (executed)
				{
					property.SetValue(theObj, value, null);
				}
			}
			else if (this.IsWriting)
				executed = this.WriteAttributeOptOnTrue(name, (long)property.GetValue(theObj, null) , predicate, numBase);

			return executed;
		}

		public bool StreamAttributeOpt<T>(TName name, T theObj, Exprs.Expression<Func<T, Values.KGuid >> propExpr , Predicate<Values.KGuid> predicate = null )
		{
			Contract.Requires(this.ValidateNameArg(name));

			if (predicate == null)
				predicate = x => true;

			bool executed = false;
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (this.IsReading)
			{
				var value = default( Values.KGuid );
				executed = this.ReadAttributeOpt(name, ref value );
				if (executed)
				{
					property.SetValue(theObj, value, null);
				}
			}
			else if (this.IsWriting)
				executed = this.WriteAttributeOptOnTrue(name, (Values.KGuid)property.GetValue(theObj, null) , predicate);

			return executed;
		}
		#endregion


		#region Stream Elements
		public void StreamElements(TName name, ICollection< string > coll)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(coll != null);

				 if (this.IsReading)
					 this.ReadElements(name, coll);
			else if (this.IsWriting)
				this.WriteElements(name, coll);
		}
		public void StreamElements(TName name, ICollection< char > coll)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(coll != null);

				 if (this.IsReading)
					 this.ReadElements(name, coll);
			else if (this.IsWriting)
				this.WriteElements(name, coll);
		}
		public void StreamElements(TName name, ICollection< bool > coll)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(coll != null);

				 if (this.IsReading)
					 this.ReadElements(name, coll);
			else if (this.IsWriting)
				this.WriteElements(name, coll);
		}
		public void StreamElements(TName name, ICollection< float > coll)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(coll != null);

				 if (this.IsReading)
					 this.ReadElements(name, coll);
			else if (this.IsWriting)
				this.WriteElements(name, coll);
		}
		public void StreamElements(TName name, ICollection< double > coll)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(coll != null);

				 if (this.IsReading)
					 this.ReadElements(name, coll);
			else if (this.IsWriting)
				this.WriteElements(name, coll);
		}

		public void StreamElements(TName name, ICollection< byte > coll, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(coll != null);

				 if (this.IsReading)
					 this.ReadElements(name, coll, numBase);
			else if (this.IsWriting)
				this.WriteElements(name, coll, numBase);
		}
		public void StreamElements(TName name, ICollection< sbyte > coll, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(coll != null);

				 if (this.IsReading)
					 this.ReadElements(name, coll, numBase);
			else if (this.IsWriting)
				this.WriteElements(name, coll, numBase);
		}
		public void StreamElements(TName name, ICollection< ushort > coll, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(coll != null);

				 if (this.IsReading)
					 this.ReadElements(name, coll, numBase);
			else if (this.IsWriting)
				this.WriteElements(name, coll, numBase);
		}
		public void StreamElements(TName name, ICollection< short > coll, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(coll != null);

				 if (this.IsReading)
					 this.ReadElements(name, coll, numBase);
			else if (this.IsWriting)
				this.WriteElements(name, coll, numBase);
		}
		public void StreamElements(TName name, ICollection< uint > coll, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(coll != null);

				 if (this.IsReading)
					 this.ReadElements(name, coll, numBase);
			else if (this.IsWriting)
				this.WriteElements(name, coll, numBase);
		}
		public void StreamElements(TName name, ICollection< int > coll, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(coll != null);

				 if (this.IsReading)
					 this.ReadElements(name, coll, numBase);
			else if (this.IsWriting)
				this.WriteElements(name, coll, numBase);
		}
		public void StreamElements(TName name, ICollection< ulong > coll, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(coll != null);

				 if (this.IsReading)
					 this.ReadElements(name, coll, numBase);
			else if (this.IsWriting)
				this.WriteElements(name, coll, numBase);
		}
		public void StreamElements(TName name, ICollection< long > coll, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(coll != null);

				 if (this.IsReading)
					 this.ReadElements(name, coll, numBase);
			else if (this.IsWriting)
				this.WriteElements(name, coll, numBase);
		}
		#endregion


		#region Stream Fixed Array
		public int StreamFixedArray(TName name, string[] array)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(array != null);

				 if (this.IsReading) return this.ReadFixedArray(name, array);
			else if (this.IsWriting)
				this.WriteElements(name, array);

			return array.Length;
		}
		public int StreamFixedArray(TName name, char[] array)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(array != null);

				 if (this.IsReading) return this.ReadFixedArray(name, array);
			else if (this.IsWriting)
				this.WriteElements(name, array);

			return array.Length;
		}
		public int StreamFixedArray(TName name, bool[] array)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(array != null);

				 if (this.IsReading) return this.ReadFixedArray(name, array);
			else if (this.IsWriting)
				this.WriteElements(name, array);

			return array.Length;
		}
		public int StreamFixedArray(TName name, float[] array)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(array != null);

				 if (this.IsReading) return this.ReadFixedArray(name, array);
			else if (this.IsWriting)
				this.WriteElements(name, array);

			return array.Length;
		}
		public int StreamFixedArray(TName name, double[] array)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(array != null);

				 if (this.IsReading) return this.ReadFixedArray(name, array);
			else if (this.IsWriting)
				this.WriteElements(name, array);

			return array.Length;
		}

		public int StreamFixedArray(TName name, byte[] array, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(array != null);

				 if (this.IsReading) return this.ReadFixedArray(name, array, numBase);
			else if (this.IsWriting)
				this.WriteElements(name, array, numBase);

			return array.Length;
		}
		public int StreamFixedArray(TName name, sbyte[] array, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(array != null);

				 if (this.IsReading) return this.ReadFixedArray(name, array, numBase);
			else if (this.IsWriting)
				this.WriteElements(name, array, numBase);

			return array.Length;
		}
		public int StreamFixedArray(TName name, ushort[] array, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(array != null);

				 if (this.IsReading) return this.ReadFixedArray(name, array, numBase);
			else if (this.IsWriting)
				this.WriteElements(name, array, numBase);

			return array.Length;
		}
		public int StreamFixedArray(TName name, short[] array, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(array != null);

				 if (this.IsReading) return this.ReadFixedArray(name, array, numBase);
			else if (this.IsWriting)
				this.WriteElements(name, array, numBase);

			return array.Length;
		}
		public int StreamFixedArray(TName name, uint[] array, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(array != null);

				 if (this.IsReading) return this.ReadFixedArray(name, array, numBase);
			else if (this.IsWriting)
				this.WriteElements(name, array, numBase);

			return array.Length;
		}
		public int StreamFixedArray(TName name, int[] array, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(array != null);

				 if (this.IsReading) return this.ReadFixedArray(name, array, numBase);
			else if (this.IsWriting)
				this.WriteElements(name, array, numBase);

			return array.Length;
		}
		public int StreamFixedArray(TName name, ulong[] array, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(array != null);

				 if (this.IsReading) return this.ReadFixedArray(name, array, numBase);
			else if (this.IsWriting)
				this.WriteElements(name, array, numBase);

			return array.Length;
		}
		public int StreamFixedArray(TName name, long[] array, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(this.ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(array != null);

				 if (this.IsReading) return this.ReadFixedArray(name, array, numBase);
			else if (this.IsWriting)
				this.WriteElements(name, array, numBase);

			return array.Length;
		}
		#endregion
	};
}