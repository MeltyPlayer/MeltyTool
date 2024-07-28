
using BVector = System.Numerics.Vector4;

namespace KSoft.Phoenix.Phx
{
	public sealed partial class BProtoObjectChildObject
		: IO.ITagElementStringNameStreamable
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams
		{
			RootName = "ChildObjects",
			ElementName = "Object",
		};
		#endregion

		#region Type
		ChildObjectType mType = ChildObjectType.Object;
		public ChildObjectType Type
		{
			get { return this.mType; }
			set { this.mType = value; }
		}
		#endregion

		#region ID
		int mID = TypeExtensions.kNone;
		public int ID
		{
			get { return this.mID; }
			set { this.mID = value; }
		}
		#endregion

		#region AttachBone
		string mAttachBone;
		public string AttachBone
		{
			get { return this.mAttachBone; }
			set { this.mAttachBone = value; }
		}
		#endregion

		#region Offset
		BVector mOffset;
		public BVector Offset
		{
			get { return this.mOffset; }
			set { this.mOffset = value; }
		}
		#endregion

		#region Rotation
		float mRotation;
		public float Rotation
		{
			get { return this.mRotation; }
			set { this.mRotation = value; }
		}
		#endregion

		#region UserCivID
		int mUserCivID = TypeExtensions.kNone;
		[Meta.BCivReference]
		public int UserCivID
		{
			get { return this.mUserCivID; }
			set { this.mUserCivID = value; }
		}
		#endregion

		public DatabaseObjectKind TypeObjectKind { get {
			if (this.Type == ChildObjectType.OneTimeSpawnSquad)
				return DatabaseObjectKind.Squad;

			return DatabaseObjectKind.Object;
		} }

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			s.StreamAttributeEnumOpt("Type", ref this.mType, e => e != ChildObjectType.Object);

			xs.StreamDBID(s, XML.XmlUtil.kNoXmlName, ref this.mID, this.TypeObjectKind, isOptional: false, xmlSource: XML.XmlUtil.kSourceCursor);
			s.StreamAttributeOpt("AttachBone", ref this.mAttachBone, Predicates.IsNotNullOrEmpty);
			s.StreamBVector("Offset", ref this.mOffset, xmlSource: XML.XmlUtil.kSourceAttr);
			s.StreamAttributeOpt("Rotation", ref this.mRotation, Predicates.IsNotZero);
			xs.StreamDBID(s, "UserCiv", ref this.mUserCivID, DatabaseObjectKind.Civ, xmlSource: XML.XmlUtil.kSourceAttr);
		}
		#endregion
	};
}