using System;
using System.Diagnostics.CodeAnalysis;

namespace KSoft.IO
{
	[SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
	[SuppressMessage("Microsoft.Design", "CA2237:MarkISerializableTypesWithSerializable")]
	public partial class VersionMismatchException
		: Exception
	{
		const string kFormat = "Invalid version! @{0} Expected '{1}', got '{2}' ({3} data)";
		const string kDescFormat = "Invalid '{0}' version! Expected '{1}', got '{2}' ({3} data)";

		static string VersionCompareDesc<T>(T expected, T found)
			where T : struct, IComparable<T>
		{
			if (found.CompareTo(expected) > 0)
				return "newer";

			return "older";
		}

		VersionMismatchException(long pos, string cmp, string expected, string found)
			: base(string.Format(Util.InvariantCultureInfo, kFormat, pos.ToFilePositionHexString(), expected, found, cmp))
		{
		}
	};

	[SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
	[SuppressMessage("Microsoft.Design", "CA2237:MarkISerializableTypesWithSerializable")]
	public partial class VersionOutOfRangeException
		: Exception
	{
		const string kFormat = "Invalid version! @{0} Expected value between {1} and {2}, got '{3}' ({4} data)";
		const string kDescFormat = "Invalid '{0}' version! Expected value between {1} and {2}, got '{3}' ({4} data)";

		static string VersionCompareDesc<T>(
			[SuppressMessage("Microsoft.Design", "CA1801:ReviewUnusedParameters")]
			T expectedMin,
			T expectedMax,
			T found)
			where T : struct, IComparable<T>
		{
			if (found.CompareTo(expectedMax) > 0)
				return "newer";

			return "older";
		}

		VersionOutOfRangeException(long pos
			, string cmp
			, string expectedMin
			, string expectedMax
			, string found)
			: base(string.Format(Util.InvariantCultureInfo, kFormat, pos.ToFilePositionHexString(), expectedMin, expectedMax, found, cmp))
		{
		}

		/// <summary>
		/// Read a zero-based and positive enum value and assert it falls within zero and <paramref name="maxCount"/>-1.
		/// Uses underlying type for bit width.
		/// </summary>
		/// <typeparam name="TEnum"></typeparam>
		/// <param name="s"></param>
		/// <param name="maxCount"></param>
		/// <returns></returns>
		public static TEnum AssertZeroBasedEnum<TEnum>(EndianReader s
			, TEnum maxCount)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			var type_code = Reflection.EnumUtil<TEnum>.UnderlyingTypeCode;

			var result = new TEnum();
			switch (type_code)
			{
				case TypeCode.SByte:
				case TypeCode.Byte:
				{
					var maxCountMinusOne = Reflection.EnumValue<TEnum>.ToByte(maxCount);
					var version = Assert(s, 0, maxCountMinusOne);
					result = Reflection.EnumValue<TEnum>.FromByte(version);
				} break;

				case TypeCode.Int16:
				case TypeCode.UInt16:
				{
					var maxCountMinusOne = Reflection.EnumValue<TEnum>.ToUInt16(maxCount);
					var version = Assert(s, 0, maxCountMinusOne);
					result = Reflection.EnumValue<TEnum>.FromUInt16(version);
				} break;

				case TypeCode.Int32:
				case TypeCode.UInt32:
				{
					var maxCountMinusOne = Reflection.EnumValue<TEnum>.ToUInt32(maxCount);
					var version = Assert(s, 0, maxCountMinusOne);
					result = Reflection.EnumValue<TEnum>.FromUInt32(version);
				} break;

				case TypeCode.Int64:
				case TypeCode.UInt64:
				{
					var maxCountMinusOne = Reflection.EnumValue<TEnum>.ToUInt64(maxCount);
					var version = Assert(s, 0, maxCountMinusOne);
					result = Reflection.EnumValue<TEnum>.FromUInt64(version);
				} break;

				default:
					throw new Debug.UnreachableException(type_code.ToString());
			}

			return result;
		}
	}
}
