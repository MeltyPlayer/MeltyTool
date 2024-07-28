using System;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Memory.Strings
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
	public class StringStorageMarkupAttribute : Attribute
	{
		public StringStorage Storage { get; private set; }

		#region Ctor
		/// <summary>Define a string storage markup</summary>
		/// <param name="widthType">Width size of a single character of the string type</param>
		/// <param name="type">Storage method for this string type</param>
		/// <param name="byteOrder"></param>
		/// <param name="fixedLength">The storage fixed length (in characters) of this string type</param>
		public StringStorageMarkupAttribute(StringStorageWidthType widthType, StringStorageType type,
			Shell.EndianFormat byteOrder = Shell.EndianFormat.Little, short fixedLength = 0)
		{
			Contract.Requires(fixedLength >= 0);

			this.Storage = new StringStorage(widthType, type, byteOrder, fixedLength);
		}
		/// <summary>Define a string storage markup (in <see cref="Shell.EndianFormat.Little"/> byte order)</summary>
		/// <param name="widthType">Width size of a single character of this string type</param>
		/// <param name="type">Storage method for this string type</param>
		/// <param name="fixedLength">The storage fixed length (in characters) of this string type</param>
		public StringStorageMarkupAttribute(StringStorageWidthType widthType, StringStorageType type, short fixedLength) :
			this(widthType, type, Shell.EndianFormat.Little, fixedLength)
		{
			Contract.Requires(fixedLength >= 0);
		}
		#endregion
	};

	/// <summary>CString, Ascii</summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
	public sealed class CStringStorageMarkupAsciiAttribute : StringStorageMarkupAttribute
	{
		public CStringStorageMarkupAsciiAttribute()
			: base(StringStorageWidthType.Ascii, StringStorageType.CString) { }
	};
	/// <summary>CString, Unicode</summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
	public sealed class CStringStorageMarkupUnicodeAttribute : StringStorageMarkupAttribute
	{
		public CStringStorageMarkupUnicodeAttribute()
			: base(StringStorageWidthType.Unicode, StringStorageType.CString) { }
	};
	/// <summary>CString, Unicode-BE</summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
	public sealed class CStringStorageMarkupUnicodeBEAttribute : StringStorageMarkupAttribute
	{
		public CStringStorageMarkupUnicodeBEAttribute()
			: base(StringStorageWidthType.Unicode, StringStorageType.CString, Shell.EndianFormat.Big) { }
	};

	/// <summary>CharArray, Ascii</summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
	public sealed class StringStorageMarkupAsciiAttribute : StringStorageMarkupAttribute
	{
		public StringStorageMarkupAsciiAttribute()
			: base(StringStorageWidthType.Ascii, StringStorageType.CharArray) { }
	};
	/// <summary>CharArray, Unicode</summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
	public sealed class StringStorageMarkupUnicodeAttribute : StringStorageMarkupAttribute
	{
		public StringStorageMarkupUnicodeAttribute()
			: base(StringStorageWidthType.Unicode, StringStorageType.CharArray) { }
	};
	/// <summary>CharArray, Unicode-BE</summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
	public sealed class StringStorageMarkupUnicodeBEAttribute : StringStorageMarkupAttribute
	{
		public StringStorageMarkupUnicodeBEAttribute()
			: base(StringStorageWidthType.Unicode, StringStorageType.CharArray, Shell.EndianFormat.Big) { }
	};
}