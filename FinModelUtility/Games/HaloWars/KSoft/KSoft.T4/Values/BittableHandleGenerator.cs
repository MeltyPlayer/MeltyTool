using System;
using Debug = System.Diagnostics.Debug;
using TextTemplating = Microsoft.VisualStudio.TextTemplating;

namespace KSoft.T4.Values
{
	public enum BittableHandleKind
	{
		/// <summary>Handle is otherwise opaque</summary>
		Undefined,
		/// <summary>Handle represents a value where NONE (-1) is the 'invalid' sentinel</summary>
		Noneable,
		/// <summary>Handle represents a value where NULL (0) is the 'invalid' sentinel</summary>
		Nullable,
		/// <summary>Handle is a wrapper for a void* value</summary>
		IntPtrWrapper,
	};

	public sealed class BittableHandleGenerator
	{
		#region Constants
		const string kNoneValueCodeName = "KSoft.TypeExtensions.kNone";
		const string kNoneFieldName = "None";

		const string kNullFieldName = "Null";
		#endregion

		readonly TextTemplating.TextTransformation mFile;
		readonly string mUnderlyingTypeName;
		readonly string mStructName;

		public string BackingFieldName { get; set; }

		/// <summary>Is this handle only internally accessed, or is it public (default value)?</summary>
		public bool IsInternal { get; set; }
		/// <summary>Should the type decl include a 'partial' specification? (default is true)</summary>
		public bool IsPartial { get; set; }

		public BittableHandleKind Kind { get; set; }

		public bool SupportsIComparable { get; set; }
		public bool SupportsIEquatable { get; set; }

		public string CtorFromValueTypeName { get; set; }
		public string CtorFromValueParamName { get; set; }

		#region Ctors
		BittableHandleGenerator()
		{
			this.BackingFieldName = "mValue";

			this.IsPartial = true;

			this.Kind = BittableHandleKind.Undefined;

			this.SupportsIComparable = true;
			this.SupportsIEquatable = true;

			this.CtorFromValueTypeName = null;
			this.CtorFromValueParamName = "value";
		}
		public BittableHandleGenerator(TextTemplating.TextTransformation ttFile,
			string underlyingTypeName, string structName)
			: this()
		{
			this.mFile = ttFile;
			this.mUnderlyingTypeName = underlyingTypeName;
			this.mStructName = structName;
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods")]
		public BittableHandleGenerator(TextTemplating.TextTransformation ttFile,
			NumberCodeDefinition underlyingType, string structName)
			: this(ttFile, underlyingType.Code.ToString(), structName)
		{
			if (underlyingType == null)
				throw new ArgumentNullException(nameof(underlyingType));
		}

		public static BittableHandleGenerator ForIntPtr(TextTemplating.TextTransformation ttFile,
			string structName)
		{
			return new BittableHandleGenerator(ttFile, "IntPtr", structName)
			{
				Kind = BittableHandleKind.IntPtrWrapper,

				SupportsIComparable = false,

				CtorFromValueTypeName = "IntPtr",
				CtorFromValueParamName = "pointer",
			};
		}
		#endregion

		#region Generate type declaration
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1822", Justification="Remove supression once this does something")]
		void WriteDeclAttributes()
		{
			// #TODO_IMPLEMENT
		}
		void WriteDeclHeader()
		{
			string visibility = this.IsInternal
				? "internal"
				: "public";

			string partial = this.IsPartial
				? "partial "
				: "";

			this.mFile.WriteLine("{0} {1} struct {2}", visibility, partial, this.mStructName);
		}
		#region GenerateDeclInheritance
		// Curiously recurring template pattern
		string BuildDeclInheritanceTextCRTP(string genericClassName)
		{
			return string.Format(UtilT4.InvariantCultureInfo, "{0}<{1}>", genericClassName, this.mStructName);
		}
		int WriteDeclInheritanceEntry(int position, bool entryExists, string entryText)
		{
			if (entryExists)
			{
				string prefix = position > 0
					? ","
					: ":";

				this.mFile.WriteLine("{0} {1}", prefix, entryText);
				position++;
			}

			return position;
		}
		void WriteDeclInheritance()
		{
			int entry_pos = 0;
			// indent the inheritance entries +1 more than the type decl (ie, public struct...)
			using (this.mFile.EnterCodeBlock())
			{
				entry_pos = this.WriteDeclInheritanceEntry(entry_pos,
				                                           this.SupportsIComparable,
				                                           this.BuildDeclInheritanceTextCRTP("IComparable"));

				entry_pos = this.WriteDeclInheritanceEntry(entry_pos,
				                                           this.SupportsIEquatable,
				                                           this.BuildDeclInheritanceTextCRTP("IEquatable"));
			}
		}
		#endregion
		public void GenerateDecl()
		{
			// indent the decl by 1. it is assumed the decl is nested in a single namespace declaration (like this codegen class)
			using (this.mFile.EnterCodeBlock())
			{
				this.WriteDeclAttributes();
				this.WriteDeclHeader();
				this.WriteDeclInheritance();
			}
		}
		#endregion

		public void GenerateBackingFieldDecl()
		{
			this.mFile.WriteLine("{0} {1};",
			                     this.mUnderlyingTypeName,
			                     this.BackingFieldName);

			this.mFile.NewLine();
		}

		// Use non-default CtorFromValue... inputs when the 'value' is of a different bit size than the underlying type
		// eg, underlying type is Int16, but we construct from Int32 inputs
		public void GenerateCtorFromValueMethod()
		{
			if (this.CtorFromValueTypeName == null)
				this.CtorFromValueTypeName = this.mUnderlyingTypeName;

			this.mFile.WriteLine("public {0}({1} {2})",
			                     this.mStructName,
			                     this.CtorFromValueTypeName,
			                     this.CtorFromValueParamName);

			using (this.mFile.EnterCodeBlock(TextTransformationCodeBlockType.Brackets))
			{
				this.mFile.WriteLine("{0} = ({1}){2};",
				                     this.BackingFieldName,
				                     this.mUnderlyingTypeName,
				                     this.CtorFromValueParamName);
			}

			this.mFile.NewLine();
		}

		#region Generate cast to and from methods
		void WriteCastToUnderlyingTypeMethod()
		{
			const string k_param_name = "handle";

			this.mFile.WriteLine("public static implicit operator {0}({1} {2})",
			                     this.mUnderlyingTypeName,
			                     this.mStructName, k_param_name);

			using (this.mFile.EnterCodeBlock(TextTransformationCodeBlockType.Brackets))
			{
				this.mFile.WriteLine("return {0}.{1}",
				                     k_param_name,
				                     this.BackingFieldName);
			}
		}
		void WriteCastFromUnderlyingTypeMethod()
		{
			const string k_param_name = "value";

			this.mFile.WriteLine("public static implicit operator {0}({1} {2})",
			                     this.mStructName,
			                     this.mUnderlyingTypeName, k_param_name);

			using (this.mFile.EnterCodeBlock(TextTransformationCodeBlockType.Brackets))
			{
				this.mFile.WriteLine("return new {0}({1});",
				                     this.mStructName, k_param_name);
			}
		}
		public void GenerateStockCastMethods()
		{
			this.WriteCastToUnderlyingTypeMethod();
			this.WriteCastFromUnderlyingTypeMethod();
			this.mFile.NewLine();
		}
		#endregion

		#region Generate equality methods
		void WriteEqualityOperatorMethod(string operatorSymbol)
		{
			Debug.Assert(this.SupportsIEquatable,
				"Trying to generate handle without IEquatable support, but with equality operators (this is non-optimal)",
				"in {0}",
				this.mStructName);

			this.mFile.WriteLine("public static bool operator {0}({1} x, {1} y)",
			                     operatorSymbol,
			                     this.mStructName);

			using (this.mFile.EnterCodeBlock(TextTransformationCodeBlockType.Brackets))
			{
				this.mFile.WriteLine("return x.Equals(y) {0} true",
				                     operatorSymbol);
			}
		}
		public void GenerateEqualityOperatorMethods()
		{
			this.WriteEqualityOperatorMethod("==");
			this.WriteEqualityOperatorMethod("!=");
			this.mFile.NewLine();
		}
		#endregion

		#region Generate NONE utilities
		void WriteNoneField(string fieldName = kNoneFieldName)
		{
			this.mFile.WriteLine("public static readonly {0} {1} = new {0}({2});",
			                     this.mStructName, fieldName, kNoneValueCodeName);
		}
		void WriteNoneableProperties()
		{
			this.mFile.WriteLine("public bool IsNone { get { return {0} == {1}; } }",
			                     this.BackingFieldName, kNoneValueCodeName);

			this.mFile.WriteLine("public bool IsNotNone { get { return {0} != {1}; } }",
			                     this.BackingFieldName, kNoneValueCodeName);
		}
		public void GenereateNoneableMembers(string noneFieldName = kNoneFieldName)
		{
			if (this.Kind == BittableHandleKind.Noneable)
			{
				this.WriteNoneField(noneFieldName);
				this.mFile.NewLine();

				this.WriteNoneableProperties();
				this.mFile.NewLine();
			}
		}
		#endregion

		#region Generate NULL utilities
		void WriteNullField(string fieldName = kNullFieldName)
		{
			this.mFile.WriteLine("public static readonly {0} {1} = new {0}( 0 );",
			                     this.mStructName, fieldName);
		}
		void WriteNullableProperties()
		{
			this.mFile.WriteLine("public bool IsNull { get { return {0} == 0; } }",
			                     this.BackingFieldName);

			this.mFile.WriteLine("public bool IsNotNull { get { return {0} != 0; } }",
			                     this.BackingFieldName);
		}
		public void GenereateNullableMembers(string nullFieldName = kNullFieldName)
		{
			if (this.Kind == BittableHandleKind.Nullable)
			{
				this.WriteNullField(nullFieldName);
				this.mFile.NewLine();

				this.WriteNullableProperties();
				this.mFile.NewLine();
			}
		}
		#endregion

		#region Generate object overrides
		void WriteGetHashCodeOverride()
		{
			this.mFile.WriteLine("public override int GetHashCode()");

			using (this.mFile.EnterCodeBlock(TextTransformationCodeBlockType.Brackets))
			{
				this.mFile.WriteLine("{0}.GetHashCode();",
				                     this.BackingFieldName);
			}
		}
		void WriteEqualsOverride()
		{
			this.mFile.WriteLine("public override bool Equals(object obj)");

			using (this.mFile.EnterCodeBlock(TextTransformationCodeBlockType.Brackets))
			{
				this.mFile.WriteLine("if (!(obj is {0}))",
				                     this.mStructName);
				using (this.mFile.EnterCodeBlock(TextTransformationCodeBlockType.Brackets))
				{
					this.mFile.WriteLine("return false;");
				}

				this.mFile.WriteLine("return Equals( ({0})obj );",
				                     this.mStructName);
			}
		}
		public void GenerateObjectMethodOverrides()
		{
			this.WriteGetHashCodeOverride();
			this.mFile.NewLine();

			this.WriteEqualsOverride();
			this.mFile.NewLine();
		}
		#endregion

		#region Generate Interface impls
		#region IComparable<struct> Members
		void GenerateIComparableImpl()
		{
			const string k_interface_name = "IComparable";

			this.mFile.WriteLine("#region {0}<{1}> Members",
			                     k_interface_name,
			                     this.mStructName);
			{
				const string k_return_type = "int";

				this.mFile.WriteLine("public {0} {1}({2} other)",
				                     k_return_type, "ComapreTo",
				                     this.mStructName);

				using (this.mFile.EnterCodeBlock(TextTransformationCodeBlockType.Brackets))
				{
					this.mFile.WriteLine("return ({0})({1} - other.{1});",
					                     k_return_type,
					                     this.BackingFieldName);
				}
			}
			this.mFile.WriteLine("#endregion");
		}
		#endregion

		#region IEquatable<struct> Members
		void GenerateIEquatableImpl()
		{
			const string k_interface_name = "IEquatable";

			this.mFile.WriteLine("#region {0}<{1}> Members",
			                     k_interface_name,
			                     this.mStructName);
			{
				const string k_return_type = "bool";

				this.mFile.WriteLine("public {0} {1}({2} other)",
				                     k_return_type, "Equals",
				                     this.mStructName);

				using (this.mFile.EnterCodeBlock(TextTransformationCodeBlockType.Brackets))
				{
					this.mFile.WriteLine("return {0} == other.{0};",
					                     this.BackingFieldName);
				}
			}
			this.mFile.WriteLine("#endregion");
		}
		#endregion

		public void GenerateSupportedInterfaceImplementations()
		{
			if (this.SupportsIComparable)
			{
				this.GenerateIComparableImpl();
				this.mFile.NewLine();
			}

			if (this.SupportsIEquatable)
			{
				this.GenerateIEquatableImpl();
				this.mFile.NewLine();
			}
		}
		#endregion

		public void GenerateDefaultBody()
		{
			using (this.mFile.EnterCodeBlock(TextTransformationCodeBlockType.BracketsStatement))
			{
				switch (this.Kind)
				{
				case BittableHandleKind.Noneable:
					this.WriteNoneField();
					break;
				case BittableHandleKind.Nullable:
					this.WriteNullField();
					break;
				}
				if (this.Kind != BittableHandleKind.Undefined)
					this.mFile.NewLine();

				this.GenerateBackingFieldDecl();

				this.GenerateCtorFromValueMethod();

				this.GenerateStockCastMethods();
				this.GenerateEqualityOperatorMethods();

				switch (this.Kind)
				{
				case BittableHandleKind.Noneable:
					this.WriteNoneableProperties();
					break;
				case BittableHandleKind.Nullable:
					this.WriteNullableProperties();
					break;
				}
				if (this.Kind != BittableHandleKind.Undefined)
					this.mFile.NewLine();

				this.GenerateObjectMethodOverrides();

				this.GenerateSupportedInterfaceImplementations();
			}
		}

		public void GenerateDefinition()
		{
			this.GenerateDecl();
			this.GenerateDefaultBody();
		}
	};
}
