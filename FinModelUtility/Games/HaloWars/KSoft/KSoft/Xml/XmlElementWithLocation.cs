using System.Xml;

namespace KSoft.Xml
{
	class XmlElementWithLocation : XmlElement, IXmlLineInfo, Text.ITextLineInfo
	{
		readonly Text.TextLineInfo mLineInfo;

		internal XmlElementWithLocation(string prefix, string localName, string namespaceURI, XmlDocumentWithLocation document)
			: base(prefix, localName, namespaceURI, document)
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

		XmlAttributeWithLocation GetAttributeWithLocation(string name)
		{
			var attr = this.Attributes[name];

			return (XmlAttributeWithLocation)attr;
		}

		public Text.TextLineInfo GetAttributeLineInfo(string name)
		{
			var attr_with_location = this.GetAttributeWithLocation(name);

			return attr_with_location != null
				? attr_with_location.LineInfo
				: Text.TextLineInfo.Empty;
		}
	};
}