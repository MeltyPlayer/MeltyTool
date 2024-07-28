
namespace KSoft.Phoenix.Phx
{
	public sealed class BUserClassField
		: Collections.BListAutoIdObject
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams()
		{
			ElementName = "Fields",
			DataName = "Name",
		};
		#endregion

		BTriggerVarType mType;

		#region IXmlElementStreamable Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			s.StreamAttributeEnum("Type", ref this.mType);
		}
		#endregion
	};
}