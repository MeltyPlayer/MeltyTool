using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace KSoft.Phoenix.Xmb
{
	public enum XmbFileBuilderOptions
	{
		[Display(	Name="Force string encoding",
					Description="XML element and attribute values are treated as string data, even if it could be detected as a integer, bool, etc")]
		ForceStringVariants,
		[Browsable(false)]
		[Display(	Name="Unicode strings are permitted",
					Description="")]
		AllowUnicode,
		[Display(	Name="Force Unicode strings",
					Description="XML element and attribute values are treated as Unicode strings")]
		ForceUnicode,
		[Browsable(false)]
		LittleEndian,

		kNumberOf
	};

	public sealed class XmbFileBuilder
	{
		public const int kCreatorToolVersion = 1;

		#region BuilderOptions
		public Collections.BitVector32 BuilderOptions;
		public string DebugBuilderOptions { get {
			return this.BuilderOptions.ToString(XmbFileBuilderOptions.kNumberOf);
		} }

		public bool ForceStringVariants { get {
			return this.BuilderOptions.Test(XmbFileBuilderOptions.ForceStringVariants);
		} }
		public bool AllowUnicode { get {
			return this.BuilderOptions.Test(XmbFileBuilderOptions.AllowUnicode);
		} }
		public bool ForceUnicode { get {
			return this.BuilderOptions.Test(XmbFileBuilderOptions.ForceUnicode);
		} }
		#endregion

		internal XmbFile Xmb;

		#region Stats
		public int NumberOfElements { get; set; }
		public int NumberOfAttributes { get; set; }

		public int NumberOfInputVariants { get; set; }
		public int NumberOfRedundantVariants { get; set; }

		public int NumberOfDirectVariants { get; set; }
		public int NumberOfIndirectVariants { get; set; }

		public int NumberOfNulls { get; set; }
		public int NumberOfSingle24 { get; set; }
		public int NumberOfSingles { get; set; }
		public int NumberOfInt24 { get; set; }
		public int NumberOfInts { get; set; }
		public int NumberOfFixedPoint { get; set; }
		public int NumberOfDouble { get; set; }
		public int NumberOfBooleans { get; set; }
		public int NumberOfDirectStringAnsi { get; set; }
		public int NumberOfIndirectStringAnsi { get; set; }
		// There's no support for indirect Unicode
		public int NumberOfIndirectStringUnicode { get; set; }
		public int NumberOfVectors { get; set; }

		public int NumberOfIndirectStringBytes { get; set; }
		public int NumberOfVariantBytes { get; set; }
		#endregion

		public XmbFileBuilder()
		{
			this.BuilderOptions.Set(XmbFileBuilderOptions.AllowUnicode);
		}

		public string GetCreatorToolCommandLine(string xmlFileName)
		{
			var sb = new System.Text.StringBuilder();

			sb.AppendFormat("XMLCOMP -file {0}",
				xmlFileName);
			sb.Append(this.BuilderOptions.Test(XmbFileBuilderOptions.LittleEndian)==false ? "" :
				" -littleEndian");
			sb.Append(this.ForceStringVariants==false ? "" :
				" -disableNumerics");
			sb.Append(this.ForceUnicode==false ? "" :
				" -forceUnicode");

			return sb.ToString();
		}
	};
}