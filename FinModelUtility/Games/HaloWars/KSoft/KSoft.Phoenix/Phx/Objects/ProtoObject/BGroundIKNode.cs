
using BVector = System.Numerics.Vector4;

namespace KSoft.Phoenix.Phx
{
	public sealed class BGroundIKNode
		: IO.ITagElementStringNameStreamable
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams
		{
			ElementName = "GroundIK",
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

		#region IKRange
		float mIKRange;
		public float IKRange
		{
			get { return this.mIKRange; }
			set { this.mIKRange = value; }
		}

		public bool IKRangeIsValid { get { return this.IKRange >= 0.0; } }
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

		#region AxisPositioning
		BVector mAxisPositioning;
		public BVector AxisPositioning
		{
			get { return this.mAxisPositioning; }
			set { this.mAxisPositioning = value; }
		}

		public bool OnLeft { get { return this.mAxisPositioning.X <= -1.0f; } }
		public bool OnRight { get { return this.mAxisPositioning.X >= +1.0f; } }

		public bool InFront { get { return this.mAxisPositioning.Z >= +1.0f; } }
		public bool InBack { get { return this.mAxisPositioning.Z <= -1.0f; } }
		#endregion

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamCursor(ref this.mName);
			s.StreamAttribute("ikRange", ref this.mIKRange);
			s.StreamAttribute("linkCount", ref this.mLinkCount);
			s.StreamAttributeOpt("x", ref this.mAxisPositioning.X, Predicates.IsNotZero);
			s.StreamAttributeOpt("z", ref this.mAxisPositioning.Z, Predicates.IsNotZero);
		}
		#endregion
	};
}