
namespace KSoft.Phoenix.Phx
{
	public sealed class BTriggerGroup
		: TriggerScriptIdObject
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams
		{
			RootName = "TriggerGroups",
			ElementName = "Group",
			DataName = DatabaseNamedObject.kXmlAttrNameN,
		};
	#endregion

	//string mValue;

	public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);

			//s.StreamCursor(mode, ref mValue);
		}
	};
}