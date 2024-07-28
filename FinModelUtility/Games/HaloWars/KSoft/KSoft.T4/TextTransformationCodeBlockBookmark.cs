using System;

namespace KSoft.T4
{
	enum TextTransformationCodeBlockType
	{
		NoBrackets,
		Brackets,
		/// <summary>Mainly for a class definition statement</summary>
		BracketsStatement,
	};
	struct TextTransformationCodeBlockBookmark
		: IDisposable
	{
		internal const string kIndent = "\t";

		readonly Microsoft.VisualStudio.TextTemplating.TextTransformation mFile;
		readonly TextTransformationCodeBlockType mType;
		readonly int mIndentCount;

		public TextTransformationCodeBlockBookmark(Microsoft.VisualStudio.TextTemplating.TextTransformation file,
			TextTransformationCodeBlockType type, int indentCount = 1)
		{
			this.mFile = file;
			this.mType = type;
			this.mIndentCount = indentCount;
		}

		internal void Enter()
		{
			if (this.mType == TextTransformationCodeBlockType.Brackets ||
			    this.mType == TextTransformationCodeBlockType.BracketsStatement)
				this.mFile.WriteLine("{");

			for (int x = 0; x < this.mIndentCount; x++)
				this.mFile.PushIndent(kIndent);
		}

		public void Dispose()
		{
			for (int x = 0; x < this.mIndentCount; x++)
				this.mFile.PopIndent();

			if (this.mType == TextTransformationCodeBlockType.Brackets)
				this.mFile.WriteLine("}");
			else if (this.mType == TextTransformationCodeBlockType.BracketsStatement)
				this.mFile.WriteLine("};");
		}
	};
}