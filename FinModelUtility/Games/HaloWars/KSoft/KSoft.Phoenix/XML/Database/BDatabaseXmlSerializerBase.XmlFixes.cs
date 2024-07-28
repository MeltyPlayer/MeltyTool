using System.Xml;

namespace KSoft.Phoenix.XML
{
	partial class BDatabaseXmlSerializerBase
	{
		protected virtual void FixWeaponTypes() {}

		protected static XmlNode XPathSelectNodeByName(IO.XmlElementStream s, BListXmlParams op,
			string dataName, string attributeName = Phx.DatabaseNamedObject.kXmlAttrName)
		{
			string xpath = string.Format(
				"/{0}/{1}[@{2}='{3}']",
				op.RootName, op.ElementName, attributeName, dataName);
			return s.Document.SelectSingleNode(xpath);
		}

		protected static XmlElement XPathSelectElementByName(IO.XmlElementStream s, string rootName
			, string dataName)
		{
			string xpath = string.Format("/{0}/{1}",
				rootName, dataName);
			var element = s.Document.SelectSingleNode(xpath);
			return element as XmlElement;
		}

		protected virtual void FixGameDataXml(IO.XmlElementStream s)
		{ }
		protected virtual void FixGameData()
		{ }

		protected virtual void FixObjectsXml(IO.XmlElementStream s)
		{ }

		protected virtual void FixSquadsXml(IO.XmlElementStream s)
		{ }

		protected virtual void FixTechsXml(IO.XmlElementStream s)
		{ }

		protected virtual void FixPowersXml(IO.XmlElementStream s)
		{ }

		protected static void FixTacticsTraceFixEvent(IO.XmlElementStream s, string tacticName, string xpath)
		{
			Debug.Trace.XML.TraceEvent(System.Diagnostics.TraceEventType.Warning, TypeExtensions.kNone,
				"{0}: Fixing Tactic with XPath={1}", s.StreamName, xpath);
		}

		protected virtual void FixTacticsXml(IO.XmlElementStream s, string name)
		{ }

		protected static void FixXmlTraceFixEvent(IO.XmlElementStream s, XmlNode node, string message, params object[] args)
		{
			var lineInfo = node as Text.ITextLineInfo;
			string lineInfoString = lineInfo != null
				? string.Format("{0} ({1})", s.StreamName, Text.TextLineInfo.ToString(lineInfo, verboseString: true))
				: s.StreamName;

			string messageOutput = message;
			if (!args.IsNullOrEmpty())
				messageOutput = message.Format(args);

			Debug.Trace.XML.TraceEvent(System.Diagnostics.TraceEventType.Warning, TypeExtensions.kNone,
				"{0}: {1}", lineInfoString, messageOutput);
		}
	};
}