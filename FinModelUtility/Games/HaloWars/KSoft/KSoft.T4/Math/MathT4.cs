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
				this.Index = index;
				this.Name = name;
				this.LastComponent = index == (vecDimensions-1);
			}

			public string Prefix(string prefix)
			{
				return prefix + this.Name;
			}
			public string Suffix(string suffix)
			{
				return this.Name + suffix;
			}

			public string ContOrEnd(string cont, string end = "")
			{
				return this.LastComponent
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
				this.CodeDef = codeDef;
				this.Dimensions = dimensions;
			}

			public string TypeName { get {
				string typeChar = this.CodeDef.IsInteger
					? "i"
					: "f";

				return string.Format(System.Globalization.CultureInfo.InvariantCulture,
					"Vec{0}{1}{2}",
					this.Dimensions, typeChar,
					this.CodeDef.SizeOfInBits);
			} }

			public IEnumerable<VectorComponent> Components { get {
				if (this.Dimensions >= 1) yield return new VectorComponent(0, "x", this.Dimensions);
				if (this.Dimensions >= 2) yield return new VectorComponent(1, "y", this.Dimensions);
				if (this.Dimensions >= 3) yield return new VectorComponent(2, "z", this.Dimensions);
			} }

			public bool ComponentsRequireCast { get { return this.CodeDef.SizeOfInBytes < PrimitiveDefinitions.kInt32.SizeOfInBytes; } }

			public string ComponentDecls(string prefix = "", string suffix = "")
			{
				var sb = new System.Text.StringBuilder();

				sb.Append(this.CodeDef.Keyword);
				sb.Append(" ");

				foreach (var comp in this.Components)
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

				foreach (var comp in this.Components)
				{
					sb.Append(this.CodeDef.Keyword);
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
					useCastsIfNeeded = this.ComponentsRequireCast;

				foreach (var comp in this.Components)
				{
					if (useCastsIfNeeded)
					{
						sb.AppendFormat(System.Globalization.CultureInfo.InvariantCulture,
							"({0})( ",
							this.CodeDef.Keyword);
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
