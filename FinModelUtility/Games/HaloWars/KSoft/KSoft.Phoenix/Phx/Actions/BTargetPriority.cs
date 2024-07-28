using System.Collections.Generic;

namespace KSoft.Phoenix.Phx
{
	// TODO: change to struct?
	public sealed class BTargetPriority
		: IO.ITagElementStringNameStreamable
		, IEqualityComparer<BTargetPriority>
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams
		{
			ElementName = "TargetPriority",
		};
		#endregion

		int mUnitTypeID = TypeExtensions.kNone;
		[Meta.UnitReference]
		public int UnitTypeID { get { return this.mUnitTypeID; } }

		float mPriority = PhxUtil.kInvalidSingle;
		public float Priority { get { return this.mPriority; } }

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			xs.StreamDBID(s, "type", ref this.mUnitTypeID, DatabaseObjectKind.Unit, false, XML.XmlUtil.kSourceAttr);
			s.StreamCursor(ref this.mPriority);
		}
		#endregion

		#region IEqualityComparer<BTargetPriority> Members
		public bool Equals(BTargetPriority x, BTargetPriority y)
		{
			return x.UnitTypeID == y.UnitTypeID && x.Priority == y.Priority;
		}

		public int GetHashCode(BTargetPriority obj)
		{
			return obj.UnitTypeID.GetHashCode() ^ obj.Priority.GetHashCode();
		}
		#endregion
	};
}