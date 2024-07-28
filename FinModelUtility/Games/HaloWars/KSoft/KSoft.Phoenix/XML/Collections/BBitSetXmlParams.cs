
namespace KSoft.Phoenix.XML
{
	public sealed class BBitSetXmlParams
		: BListXmlParams
	{
		// enabled flags are written as xml elements without any attributes or text. Eg:
		// <InfiniteUses /> means the BPowerFlags.InfiniteUses bit is set
		public bool ElementItselfMeansTrue;

		private BBitSetXmlParams()
		{
		}
		/// <summary>Sets ElementName, defaults to InnerText data usage and data interning</summary>
		/// <param name="elementName">Name of the xml element which represents the type (enum) value</param>
		public BBitSetXmlParams(string elementName)
		{
			this.ElementName = elementName;

			this.Flags = 0
			             | BCollectionXmlParamsFlags.UseInnerTextForData
			             | BCollectionXmlParamsFlags.InternDataNames;
		}

		public static readonly BBitSetXmlParams kFlagsSansRoot = new BBitSetXmlParams("Flag");
		public static readonly BBitSetXmlParams kFlagsAreElementNamesThatMeanTrue = new BBitSetXmlParams()
		{
			ElementItselfMeansTrue = true,
		};
	};
}