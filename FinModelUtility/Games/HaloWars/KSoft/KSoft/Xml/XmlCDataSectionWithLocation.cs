using System.Xml;

namespace KSoft.Xml
{
	class XmlCDataSectionWithLocation : XmlCDataSection, IXmlLineInfo, Text.ITextLineInfo
	{
		readonly Text.TextLineInfo mLineInfo;

		internal XmlCDataSectionWithLocation(string data, XmlDocumentWithLocation document)
			: base(data, document)
		{
			this.mLineInfo = document.CurrentLineInfo;
		}

		internal Text.TextLineInfo LineInfo { get { return this.mLineInfo; } }

		public bool HasLineInfo { get { return this.mLineInfo.HasLineInfo; } }
		public int LineNumber	{ get { return this.mLineInfo.LineNumber; } }
		public int LinePosition	{ get { return this.mLineInfo.LinePosition; } }

		#region IXmlLineInfo Members
		bool IXmlLineInfo.HasLineInfo()	{ return this.mLineInfo.HasLineInfo; }
		int IXmlLineInfo.LineNumber		{ get { return this.mLineInfo.LineNumber; } }
		int IXmlLineInfo.LinePosition	{ get { return this.mLineInfo.LinePosition; } }
		#endregion
	};
}