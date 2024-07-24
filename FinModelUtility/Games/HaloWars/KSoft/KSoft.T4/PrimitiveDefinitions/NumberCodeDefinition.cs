using System;

namespace KSoft.T4
{
	/// <summary>Type definition for a code numeric (integer or real) primitive</summary>
	public class NumberCodeDefinition
		: PrimitiveCodeDefinition
	{
		/// <summary>Integer type keyword to use for in code for performing number operations</summary>
		public string OperationWord { get; private set; }

		public NumberCodeDefinition(TypeCode typeCode)
			: base(TypeCodeToKeyword(typeCode), typeCode)
		{
			OperationWord = TypeCodeToOperationWord(typeCode) ?? Keyword;

			string desc_prefix = "";
			string desc_id;
			string desc_postfix;
			if(IsInteger)
			{
				desc_prefix = IsSigned
					?   "signed"
					: "unsigned";

				desc_id = SizeOfInBits + "-bit";

				desc_postfix = "integer";
			}
			else
			{
				desc_id = Code.ToString().ToLower(UtilT4.InvariantCultureInfo) + "-precision";

				desc_postfix = "number";
			}

			SetupDescription(string.Format(UtilT4.InvariantCultureInfo,
				"{0} {1} {2}",
				desc_prefix, desc_id, desc_postfix).Trim());
		}

		public override bool IsInteger { get {
			return Code >= TypeCode.SByte && Code <= TypeCode.UInt64;
		} }
		public bool IsByte { get {
			return Code == TypeCode.SByte || Code == TypeCode.Byte;
		} }
		public bool IsSigned { get {
			return IsInteger && Keyword[0] != 'u' && Keyword[0] != 'b';
		} }
		public bool IsUnsigned { get {
			return IsInteger && !IsSigned;
		} }

		public override int SizeOfInBytes { get {
			switch(Code)
			{
				case TypeCode.Byte:
				case TypeCode.SByte:
					return sizeof(byte);

				case TypeCode.UInt16:
				case TypeCode.Int16:
					return sizeof(ushort);

				case TypeCode.UInt32:
				case TypeCode.Int32:
				case TypeCode.Single:
					return sizeof(uint);

				case TypeCode.UInt64:
				case TypeCode.Int64:
				case TypeCode.Double:
					return sizeof(ulong);

				default:
					throw new InvalidOperationException(Code.ToString());
			}
		} }

		public TypeCode SignedCode { get {
			switch(Code)
			{
				case TypeCode.Byte:
					return TypeCode.SByte;

				case TypeCode.UInt16:
					return TypeCode.Int16;

				case TypeCode.UInt32:
					return TypeCode.Int32;

				case TypeCode.UInt64:
					return TypeCode.Int64;

				default:
					return Code;
			}
		} }
		public string SignedKeyword { get {
			return TypeCodeToKeyword(SignedCode);
		} }

		/// <summary>Suffix to use on literal values of this number type</summary>
		public string LiteralSuffix { get {
			switch(Code)
			{
				case TypeCode.UInt32:
					return "U";

				case TypeCode.Int64:
					return "L";

				case TypeCode.UInt64:
					return "UL";

				case TypeCode.Single:
					return "f";

				default:
					return "";
			}
		} }

		/// <summary>ToString format string to get the integer's value in a non-truncated hexadecimal value</summary>
		public string ToStringHexFormat { get {
			return IsInteger
				? "X" + (SizeOfInBytes * 2)
				: "";
		} }

		/// <summary>Do the bit operators of this integer type cause the value to be implicitly upgraded to a larger int type?</summary>
		public bool BitOperatorsImplicitlyUpCast { get {
			return IsInteger && SizeOfInBits < 32;
		} }

		/// <summary>Shift amount to get or set the byte with the MSB</summary>
		public int MostSignificantByteBitShift { get {
			return SizeOfInBits - Bitwise.BitwiseT4.kBitsPerByte;
		} }

		static string TypeCodeToOperationWord(TypeCode typeCode)
		{
			switch(typeCode)
			{
				case TypeCode.Byte:		return "uint";
				case TypeCode.SByte:	return  "int";

				case TypeCode.UInt16:	return "uint";
				case TypeCode.Int16:	return  "int";

				default:
					return null;
			}
		}
		internal static string TypeCodeToKeyword(TypeCode typeCode)
		{
			switch(typeCode)
			{
				case TypeCode.Byte:		return  "byte";
				case TypeCode.SByte:	return "sbyte";

				case TypeCode.UInt16:	return "ushort";
				case TypeCode.Int16:	return  "short";

				case TypeCode.UInt32:	return "uint";
				case TypeCode.Int32:	return  "int";

				case TypeCode.UInt64:	return "ulong";
				case TypeCode.Int64:	return  "long";

				case TypeCode.Single:	return "float";
				case TypeCode.Double:	return "double";

				default:
					throw new ArgumentException(typeCode.ToString());
			}
		}
	};
}
