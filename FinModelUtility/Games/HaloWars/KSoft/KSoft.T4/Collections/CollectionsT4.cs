using System.Collections.Generic;

namespace KSoft.T4
{
	public static class CollectionsT4
	{
		public sealed class BitSetEnumeratorDef
		{
			public string Name { get; set; }
			public PrimitiveCodeDefinition ResultCodeDef { get; set; }

			public BitSetEnumeratorDef(string name, PrimitiveCodeDefinition def)
			{
				Name = name;
				ResultCodeDef = def;
			}
		};
		public static IEnumerable<BitSetEnumeratorDef> BitSetEnumeratorDefs { get {
			yield return new BitSetEnumeratorDef("State", PrimitiveDefinitions.kBool);
			yield return new BitSetEnumeratorDef("StateFilter", PrimitiveDefinitions.kInt32);
		} }

		public sealed class BitStateDef
		{
			public string ApiName { get; private set; }
			public bool Value { get; private set; }

			public BitStateDef(string name, bool value)
			{
				ApiName = name;
				Value = value;
			}

			// name to use in XMLdoc contents
			public string DocName { get {
				return ApiName.ToLower(UtilT4.InvariantCultureInfo);
			} }
			// C# bool keyword
			public string ValueKeyword { get {
				return Value.ToString(UtilT4.InvariantCultureInfo).ToLower(UtilT4.InvariantCultureInfo);
			} }
			public string BinaryName { get {
				return Value
					? "1"
					: "0";
			} }

			public string DocNameVerbose { get {
				return string.Format(UtilT4.InvariantCultureInfo, "{0} ({1})", BinaryName, DocName);
			} }
		};
		public static IEnumerable<BitStateDef> BitStateDefs { get {
			yield return new BitStateDef("Clear", false);
			yield return new BitStateDef("Set", true);
		} }

		public static IEnumerable<string> BitVectorMathBinaryOperatorOverloads { get {
			yield return "&";
			yield return "|";
			yield return "^";
		} }
		public static IEnumerable<string> BitVectorMathUnaryOperatorOverloads { get {
			yield return "~";
		} }
		public static IEnumerable<string> BitVectorBooleanOperatorOverloads { get {
			yield return "==";
			yield return "!=";
		} }
	};
}
