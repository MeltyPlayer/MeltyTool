
namespace KSoft.Phoenix.Phx
{
	public sealed class BCollectiblesSkullManager
		: IO.ITagElementStringNameStreamable
	{
		public Collections.BListAutoId<BProtoSkull> Skulls { get; private set; } = new Collections.BListAutoId<BProtoSkull>();

		// bool RocketAllGrunts, MinimapHidden
		// int BonusSquadLevels, DeathExplodeObjectType, DeathExplodeProtoObject
		// float DeathExplodeChance

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			XML.XmlUtil.Serialize(s, this.Skulls, BProtoSkull.kBListXmlParams);
			//TimeLineEvent BProtoTimeLineEvent
		}
		#endregion
	};
}
