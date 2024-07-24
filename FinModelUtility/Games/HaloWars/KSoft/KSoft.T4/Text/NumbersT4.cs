using System.Collections.Generic;

namespace KSoft.T4
{
	public static class NumbersT4
	{
		/// <summary>
		/// 	Should KSoft.Numbers.ToString use the ToStringBuilder() overload that uses a List&amp;char&amp;?
		/// </summary>
		public static bool ToStringShouldUseListOfChar { get; } = true;

		public static bool ParseShouldUseUppercaseCheck { get; } = false;

		public static bool ParseShouldAllowLeadingWhite { get; } = true; // a la NumberStyles.AllowLeadingWhite

		public static IEnumerable<NumberCodeDefinition> ParseableIntegersSmall { get {
			yield return PrimitiveDefinitions.kByte;
			yield return PrimitiveDefinitions.kSByte;

			yield return PrimitiveDefinitions.kUInt16;
			yield return PrimitiveDefinitions.kInt16;
		} }

		public static IEnumerable<NumberCodeDefinition> ParseableIntegersWordAligned { get {
			yield return PrimitiveDefinitions.kUInt32;
			yield return PrimitiveDefinitions.kInt32;

			yield return PrimitiveDefinitions.kUInt64;
			yield return PrimitiveDefinitions.kInt64;
		} }
	};
}
