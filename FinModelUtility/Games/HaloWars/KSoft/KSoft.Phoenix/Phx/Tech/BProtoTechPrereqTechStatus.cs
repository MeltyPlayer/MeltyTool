
namespace KSoft.Phoenix.Phx
{
	public sealed class BProtoTechPrereqTechStatus
		: IO.ITagElementStringNameStreamable
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams
		{
			ElementName = "TechStatus",
		};
		#endregion

		#region TechStatus
		BProtoTechStatus mTechStatus = BProtoTechStatus.Invalid;
		[Meta.UnusedData("Not actually parsed by the engine")]
		public BProtoTechStatus TechStatus
		{
			get { return this.mTechStatus; }
			set { this.mTechStatus = value; }
		}

		static System.Predicate<BProtoTechStatus> BProtoTechStatusIsNotInvalid = (BProtoTechStatus v) => v != BProtoTechStatus.Invalid;
		#endregion

		#region TechID
		int mTechID = TypeExtensions.kNone;
		[Meta.BProtoTechReference]
		public int TechID
		{
			get { return this.mTechID; }
			set { this.mTechID = value; }
		}
		#endregion

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			s.StreamAttributeEnumOpt("status", ref this.mTechStatus, BProtoTechStatusIsNotInvalid);
			xs.StreamDBID(s, XML.XmlUtil.kNoXmlName, ref this.mTechID, DatabaseObjectKind.Tech, false, XML.XmlUtil.kSourceCursor);
		}
		#endregion
	};
}