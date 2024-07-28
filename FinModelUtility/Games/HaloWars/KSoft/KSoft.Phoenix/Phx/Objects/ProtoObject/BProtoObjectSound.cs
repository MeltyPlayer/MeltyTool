
namespace KSoft.Phoenix.Phx
{
	public sealed class BProtoObjectSound
		: IO.ITagElementStringNameStreamable
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams
		{
			ElementName = "Sound",
		};
		#endregion

		#region Sound
		string mSound;
		public string Sound
		{
			get { return this.mSound; }
			set { this.mSound = value; }
		}
		#endregion

		#region Type
		BObjectSoundType mType = BObjectSoundType.None;
		public BObjectSoundType Type
		{
			get { return this.mType; }
			set { this.mType = value; }
		}
		#endregion

		#region SquadID
		int mSquadID = TypeExtensions.kNone;
		[Meta.BProtoSquadReference]
		public int SquadID
		{
			get { return this.mSquadID; }
			set { this.mSquadID = value; }
		}
		#endregion

		#region Action
		string mAction;
		[Meta.BProtoActionReference]
		public string Action
		{
			get { return this.mAction; }
			set { this.mAction = value; }
		}
		#endregion

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			s.StreamCursor(ref this.mSound);

			if (s.StreamAttributeEnumOpt("Type", ref this.mType, e => e != BObjectSoundType.None))
			{
				xs.StreamDBID(s, "Squad", ref this.mSquadID, DatabaseObjectKind.Squad, xmlSource: XML.XmlUtil.kSourceAttr);
				s.StreamAttributeOpt("Action", ref this.mAction, Predicates.IsNotNullOrEmpty);
			}
		}
		#endregion
	};
}