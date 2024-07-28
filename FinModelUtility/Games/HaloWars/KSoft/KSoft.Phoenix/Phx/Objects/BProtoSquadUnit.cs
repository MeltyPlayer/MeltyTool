
namespace KSoft.Phoenix.Phx
{
	public sealed class BProtoSquadUnit
		: IO.ITagElementStringNameStreamable
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams("Unit")
		{
			Flags = 0
		};
		#endregion

		#region Count
		int mCount;
		public int Count
		{
			get { return this.mCount; }
			set { this.mCount = value; }
		}
		#endregion

		#region UnitID
		int mUnitID;
		[Meta.BProtoObjectReference]
		public int UnitID
		{
			get { return this.mUnitID; }
			set { this.mUnitID = value; }
		}
		#endregion

		#region UnitRole
		BUnitRole mUnitRole;
		public BUnitRole UnitRole
		{
			get { return this.mUnitRole; }
			set { this.mUnitRole = value; }
		}
		#endregion

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			s.StreamAttribute("count", ref this.mCount);
			s.StreamAttributeEnumOpt("role", ref this.mUnitRole, e => e != BUnitRole.Normal);
			xs.StreamDBID(s, XML.XmlUtil.kNoXmlName, ref this.mUnitID, DatabaseObjectKind.Object, false, XML.XmlUtil.kSourceCursor);
		}
		#endregion
	};
}