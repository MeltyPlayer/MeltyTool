using System;
using System.Collections.Generic;
using TextTemplating = Microsoft.VisualStudio.TextTemplating;

namespace KSoft.T4
{
	public enum TagElementStreamSubjectType
	{
		Cursor,
		Element,
		ElementOpt,
		Attribute,
		AttributeOpt,
	};

	public static class TagElementStreamsT4
	{
		public sealed class OperationDefinition
		{
			public TagElementStreamSubjectType SubjectType { get; private set; }
			public string Name { get; private set; }
			public bool SupportsOptional { get; private set; }

			public OperationDefinition(TagElementStreamSubjectType type, bool supportsOpt = true)
			{
				this.SubjectType = type;
				this.Name = type.ToString();
				this.SupportsOptional = supportsOpt;
			}
		};
		static readonly OperationDefinition kCursorOp = new OperationDefinition(TagElementStreamSubjectType.Cursor, false);
		static readonly OperationDefinition kElementOp = new OperationDefinition(TagElementStreamSubjectType.Element);
		static readonly OperationDefinition kAttributeOp = new OperationDefinition(TagElementStreamSubjectType.Attribute);

		public static IEnumerable<OperationDefinition> Operations { get {
			yield return kCursorOp;
			yield return kElementOp;
			yield return kAttributeOp;
		} }

		public static IEnumerable<PrimitiveCodeDefinition> SerializableTypesMisc { get {
			yield return PrimitiveDefinitions.kString;
			yield return PrimitiveDefinitions.kChar;
			yield return PrimitiveDefinitions.kBool;

			yield return PrimitiveDefinitions.kSingle;
			yield return PrimitiveDefinitions.kDouble;
		} }

		public static IEnumerable<NumberCodeDefinition> SerializableTypesIntegers { get {
			foreach (var num_type in PrimitiveDefinitions.Numbers)
				if (num_type.IsInteger)
					yield return num_type;
		} }

		public static IEnumerable<PrimitiveCodeDefinition> SerializableTypesSpecial { get {
			yield return PrimitiveDefinitions.kKGuid;
		} }

		public static void GenerateObjectPropertyStreamMethod(TextTemplating.TextTransformation ttFile,
			TagElementStreamSubjectType subject, PrimitiveCodeDefinition codeDef, bool hasTNameParam = true)
		{
			if (ttFile == null)
				throw new ArgumentNullException(nameof(ttFile));
			if (codeDef == null)
				throw new ArgumentNullException(nameof(codeDef));

			ttFile.PushIndent("\t");
			ttFile.PushIndent("\t");

			bool is_opt =
				subject == TagElementStreamSubjectType.ElementOpt ||
				subject == TagElementStreamSubjectType.AttributeOpt
				;

			string method_name = subject.ToString();
			ttFile.WriteLine(
				"public {5} Stream{0}<T>({2} T theObj, Exprs.Expression<Func<T, {1} >> propExpr {3} {4})",
				method_name,
				codeDef.Keyword,
				hasTNameParam.UseStringOrEmpty("TName name,"),
				is_opt.UseStringOrEmpty(", Predicate<{0}> predicate = null", codeDef.Keyword),
				codeDef.IsInteger.UseStringOrEmpty(", NumeralBase numBase=kDefaultRadix"),
				!is_opt ? "void" : "bool"
			);

			ttFile.WriteLine("{");
			ttFile.PushIndent("\t");

			if (hasTNameParam)
			{
				ttFile.WriteLine("Contract.Requires(ValidateNameArg(name));");
				ttFile.WriteLine("");
			}

			if (is_opt)
			{
				ttFile.WriteLine("if (predicate == null)");
				using (var cb1 = ttFile.EnterCodeBlock())
					ttFile.WriteLine("predicate = x => true;");

				ttFile.NewLine();
				ttFile.WriteLine("bool executed = false;");
			}

			ttFile.WriteLine("var property = Reflection.Util.PropertyFromExpr(propExpr);");
			ttFile.WriteLine("if (IsReading)");
			using (var cb1 = ttFile.EnterCodeBlock(TextTransformationCodeBlockType.Brackets))
			{
				ttFile.WriteLine("var value = default( {0} );", codeDef.Keyword);
				ttFile.WriteLine("{1}Read{0}({2} ref value {3});",
					method_name,
					is_opt.UseStringOrEmpty("executed = "),
					hasTNameParam.UseStringOrEmpty("name,"),
					codeDef.IsInteger.UseStringOrEmpty(", numBase")
				);
				if (is_opt)
					ttFile.WriteLine("if (executed)");
				using (var cb2 = ttFile.EnterCodeBlock(TextTransformationCodeBlockType.Brackets))
					ttFile.WriteLine("property.SetValue(theObj, value, null);");
			}

			ttFile.WriteLine("else if (IsWriting)");
			using (var cb1 = ttFile.EnterCodeBlock())
			{
				ttFile.WriteLine("{2}Write{0}{3}({4} ({1})property.GetValue(theObj, null) {5}{6});",
					method_name,									// 0
					codeDef.Keyword,								// 1
					is_opt.UseStringOrEmpty("executed = "),			// 2
					is_opt.UseStringOrEmpty("OnTrue"),				// 3
					hasTNameParam.UseStringOrEmpty("name,"),		// 4
					is_opt.UseStringOrEmpty(", predicate"),			// 5
					codeDef.IsInteger.UseStringOrEmpty(", numBase")	// 6
				);
			};

			if (is_opt)
			{
				ttFile.NewLine();
				ttFile.WriteLine("return executed;");
			}

			ttFile.PopIndent();
			ttFile.WriteLine("}");

			ttFile.PopIndent();
			ttFile.PopIndent();
		}
	};
}
