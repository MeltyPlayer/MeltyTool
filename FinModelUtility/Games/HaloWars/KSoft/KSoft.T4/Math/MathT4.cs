using System.Collections.Generic;

namespace KSoft.T4.Math
{
	public static class MathT4
	{
		public sealed class VectorComponent
		{
			public string Name { get; private set; }
			public int Index { get; private set; }
			public bool LastComponent { get; private set; }

			internal VectorComponent(int index, string name, int vecDimensions = 0)
			{
				Index = index;
				Name = name;
				LastComponent = index == (vecDimensions-1);
			}

			public string Prefix(string prefix)
			{
				return prefix + Name;
			}
			public string Suffix(string suffix)
			{
				return Name + suffix;
			}

			public string ContOrEnd(string cont, string end = "")
			{
				return LastComponent
					? end
					: cont;
			}
		};

		public sealed class VectorDef
		{
			public NumberCodeDefinition CodeDef { get; private set; }
			public int Dimensions { get; private set; }

			public VectorDef(NumberCodeDefinition codeDef, int dimensions)
			{
				CodeDef = codeDef;
				Dimensions = dimensions;
			}

			public string TypeName { get {
				string typeChar = CodeDef.IsInteger
					? "i"
					: "f";

				return string.Format(System.Globalization.CultureInfo.InvariantCulture,
					"Vec{0}{1}{2}",
					Dimensions, typeChar, CodeDef.SizeOfInBits);
			} }

			public IEnumerable<VectorComponent> Components { get {
				if (Dimensions >= 1) yield return new VectorComponent(0, "x", Dimensions);
				if (Dimensions >= 2) yield return new VectorComponent(1, "y", Dimensions);
				if (Dimensions >= 3) yield return new VectorComponent(2, "z", Dimensions);
			} }

			public bool ComponentsRequireCast { get { return CodeDef.SizeOfInBytes < PrimitiveDefinitions.kInt32.SizeOfInBytes; } }

			public string ComponentDecls(string prefix = "", string suffix = "")
			{
				var sb = new System.Text.StringBuilder();

				sb.Append(CodeDef.Keyword);
				sb.Append(" ");

				foreach (var comp in Components)
				{
					sb.Append(prefix);
					sb.Append(comp.Name);
					sb.Append(suffix);

					if (!comp.LastComponent)
						sb.Append(", ");
				}

				return sb.ToString();
			}

			public string ComponentParams(string prefix = "", string suffix = "")
			{
				var sb = new System.Text.StringBuilder();

				foreach (var comp in Components)
				{
					sb.Append(CodeDef.Keyword);
					sb.Append(" ");

					sb.Append(prefix);
					sb.Append(comp.Name);
					sb.Append(suffix);

					if (!comp.LastComponent)
						sb.Append(", ");
				}

				return sb.ToString();
			}

			public string ComponentArgs(string prefix = "", string suffix = "", bool useCastsIfNeeded = false)
			{
				var sb = new System.Text.StringBuilder();

				if (useCastsIfNeeded)
					useCastsIfNeeded = ComponentsRequireCast;

				foreach (var comp in Components)
				{
					if (useCastsIfNeeded)
					{
						sb.AppendFormat(System.Globalization.CultureInfo.InvariantCulture,
							"({0})( ", CodeDef.Keyword);
					}

					sb.Append(prefix);
					sb.Append(comp.Name);
					sb.Append(suffix);

					if (useCastsIfNeeded)
						sb.Append(" )");

					if (!comp.LastComponent)
						sb.Append(", ");
				}

				return sb.ToString();
			}
		};
		public static IEnumerable<VectorDef> IntegralVectors { get {
			yield return new VectorDef(PrimitiveDefinitions.kInt16, 2);
			yield return new VectorDef(PrimitiveDefinitions.kInt16, 3);
			yield return new VectorDef(PrimitiveDefinitions.kInt32, 2);
			yield return new VectorDef(PrimitiveDefinitions.kInt32, 3);
		} }
	};
}
