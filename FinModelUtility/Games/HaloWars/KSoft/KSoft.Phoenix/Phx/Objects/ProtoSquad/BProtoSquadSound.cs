
namespace KSoft.Phoenix.Phx
{
	public sealed class BProtoSquadSound
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
		BSquadSoundType mType = BSquadSoundType.None;
		public BSquadSoundType Type
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

		#region WorldID
		// #NOTE assumes 0 is the World enum "None" member
		const int cWorldIdNone = 0;

		int mWorldID = cWorldIdNone;
		public int WorldID
		{
			get { return this.mWorldID; }
			set { this.mWorldID = value; }
		}
		#endregion

		#region CastingUnitOnly
		bool mCastingUnitOnly;
		public bool CastingUnitOnly
		{
			get { return this.mCastingUnitOnly; }
			set { this.mCastingUnitOnly = value; }
		}
		#endregion

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			s.StreamCursor(ref this.mSound);

			if (s.StreamAttributeEnumOpt("Type", ref this.mType, e => e != BSquadSoundType.None))
			{
				// #NOTE Engine, in debug builds, asserts Squad is valid when specified
				xs.StreamDBID(s, "Squad", ref this.mSquadID, DatabaseObjectKind.Squad, xmlSource: XML.XmlUtil.kSourceAttr);
				// #NOTE Engine, in debug builds, asserts the world ID is not cWorldIdNone when the World value is defined.
				// It doesn't explicitly parse None, but defaults to None when it doesn't recognize the provided value
				s.StreamProtoEnum("World", ref this.mWorldID, xs.Database.GameScenarioWorlds, xmlSource: XML.XmlUtil.kSourceAttr
					, isOptionalDefaultValue: cWorldIdNone);
				s.StreamAttributeOpt("CastingUnitOnly", ref this.mCastingUnitOnly, Predicates.IsTrue);
			}
		}
		#endregion
	};
}