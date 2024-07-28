
namespace KSoft.Phoenix.Phx
{
	public sealed class BSweetSpotIKNode
		: IO.ITagElementStringNameStreamable
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams
		{
			ElementName = "SweetSpotIK",
		};
		#endregion

		#region Name
		string mName;
		public string Name
		{
			get { return this.mName; }
			set { this.mName = value; }
		}
		#endregion

		#region LinkCount
		int mLinkCount;
		public int LinkCount
		{
			get { return this.mLinkCount; }
			set { this.mLinkCount = value; }
		}

		public bool LinkCountIsValid { get { return this.LinkCount >= byte.MinValue && this.LinkCount <= byte.MaxValue; } }
		#endregion

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamCursor(ref this.mName);
			s.StreamAttribute("linkCount", ref this.mLinkCount);
		}
		#endregion
	};
}