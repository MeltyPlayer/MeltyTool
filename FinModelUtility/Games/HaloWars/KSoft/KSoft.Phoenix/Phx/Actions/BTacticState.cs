
namespace KSoft.Phoenix.Phx
{
	/*public*/ sealed class BTacticState // suicide grunts use this...name and action are omitted, so fuck this
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams
		{
			ElementName = "State",
			DataName = "Name",
			Flags = XML.BCollectionXmlParamsFlags.UseElementForData |
				XML.BCollectionXmlParamsFlags.ForceNoRootElementStreaming,
		};
	#endregion
  };
}