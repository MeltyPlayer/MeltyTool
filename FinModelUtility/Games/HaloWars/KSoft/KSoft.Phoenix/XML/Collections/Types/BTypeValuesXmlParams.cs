
namespace KSoft.Phoenix.XML
{
	public class BTypeValuesXmlParams<T>
		: BListExplicitIndexXmlParams<T>
	{
		/// <summary>Sets ElementName and DataName (which defaults to XML attribute usage)</summary>
		/// <param name="elementName"></param>
		/// <param name="typeName">Name of the xml node which represents the type (enum) value</param>
		public BTypeValuesXmlParams(string elementName, string typeName, BCollectionXmlParamsFlags flags = 0)
		{
			this.ElementName = elementName;
			this.DataName = typeName;
			this.Flags = flags;
		}
	};
}