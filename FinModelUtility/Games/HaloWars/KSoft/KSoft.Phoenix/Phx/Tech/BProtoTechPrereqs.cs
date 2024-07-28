
namespace KSoft.Phoenix.Phx
{
	public sealed class BProtoTechPrereqs
		: IO.ITagElementStringNameStreamable
	{
		public Collections.BListArray<BProtoTechPrereqTechStatus> TechStatus { get; private set; } = new Collections.BListArray<BProtoTechPrereqTechStatus>();
		public Collections.BListArray<BProtoTechPrereqTypeCount> TypeCounts { get; private set; } = new Collections.BListArray<BProtoTechPrereqTypeCount>();

		public bool IsNotEmpty { get { return this.TechStatus.Count > 0 || this.TypeCounts.Count > 0; } }

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			XML.XmlUtil.Serialize(s, this.TechStatus, BProtoTechPrereqTechStatus.kBListXmlParams);
			XML.XmlUtil.Serialize(s, this.TypeCounts, BProtoTechPrereqTypeCount.kBListXmlParams);
		}
		#endregion
	};
}
