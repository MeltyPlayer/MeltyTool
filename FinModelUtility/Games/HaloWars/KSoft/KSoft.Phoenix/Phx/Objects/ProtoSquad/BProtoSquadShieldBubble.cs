
namespace KSoft.Phoenix.Phx
{
	public sealed class BProtoSquadShieldBubble
		: IO.ITagElementStringNameStreamable
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams
		{
			ElementName = "ShieldBubble",
		};
		#endregion

		int mTargetSquadID = TypeExtensions.kNone;
		[Meta.BProtoSquadReference]
		public int TargetShieldSquadID
		{
			get { return this.mTargetSquadID; }
			set { this.mTargetSquadID = value; }
		}

		int mShieldSquadID = TypeExtensions.kNone;
		[Meta.BProtoSquadReference]
		public int ShieldSquadID
		{
			get { return this.mShieldSquadID; }
			set { this.mShieldSquadID = value; }
		}

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			xs.StreamDBID(s, "target", ref this.mTargetSquadID, DatabaseObjectKind.Squad, false, XML.XmlUtil.kSourceAttr);
			xs.StreamDBID(s, XML.XmlUtil.kNoXmlName, ref this.mShieldSquadID, DatabaseObjectKind.Squad, false, XML.XmlUtil.kSourceCursor);
		}
		#endregion
	};
}