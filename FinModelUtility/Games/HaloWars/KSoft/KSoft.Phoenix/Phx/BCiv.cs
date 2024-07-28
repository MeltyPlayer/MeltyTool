using System.ComponentModel;

namespace KSoft.Phoenix.Phx
{
	public sealed class BCiv
		: DatabaseNamedObject
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams("Civ")
		{
			DataName = "Name",
			Flags = XML.BCollectionXmlParamsFlags.UseElementForData
		};
		public static readonly Engine.XmlFileInfo kXmlFileInfo = new Engine.XmlFileInfo
		{
			Directory = Engine.GameDirectory.Data,
			FileName = "Civs.xml",
			RootName = kBListXmlParams.RootName
		};
		public static readonly Engine.ProtoDataXmlFileInfo kProtoFileInfo = new Engine.ProtoDataXmlFileInfo(
			Engine.XmlFilePriority.ProtoData,
			kXmlFileInfo);
		#endregion

		#region IsExcludedFromAlpha
		int mAlpha = TypeExtensions.kNone;
		public bool IsExcludedFromAlpha
		{
			get { return this.mAlpha == 0; }
			set { this.mAlpha = value ? 0 : TypeExtensions.kNone; }
		}
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

		#region CommandAckObjectID
		int mCommandAckObjectID = TypeExtensions.kNone;
		[Meta.BProtoObjectReference]
		public int CommandAckObjectID
		{
			get { return this.mCommandAckObjectID; }
			set { this.mCommandAckObjectID = value; }
		}
		#endregion

		#region RallyPointObjectID
		int mRallyPointObjectID = TypeExtensions.kNone;
		[Meta.BProtoObjectReference]
		public int RallyPointObjectID
		{
			get { return this.mRallyPointObjectID; }
			set { this.mRallyPointObjectID = value; }
		}
		#endregion

		#region LocalRallyPointObjectID
		int mLocalRallyPointObjectID = TypeExtensions.kNone;
		[Meta.BProtoObjectReference]
		public int LocalRallyPointObjectID
		{
			get { return this.mLocalRallyPointObjectID; }
			set { this.mLocalRallyPointObjectID = value; }
		}
		#endregion

		#region TransportObjectID
		int mTransportObjectID = TypeExtensions.kNone;
		[Meta.BProtoObjectReference]
		public int TransportObjectID
		{
			get { return this.mTransportObjectID; }
			set { this.mTransportObjectID = value; }
		}
		#endregion

		#region TransportTriggerObjectID
		int mTransportTriggerObjectID = TypeExtensions.kNone;
		[Meta.BProtoObjectReference]
		public int TransportTriggerObjectID
		{
			get { return this.mTransportTriggerObjectID; }
			set { this.mTransportTriggerObjectID = value; }
		}
		#endregion

		#region HullExpansionRadius
		float mHullExpansionRadius;
		public float HullExpansionRadius
		{
			get { return this.mHullExpansionRadius; }
			set { this.mHullExpansionRadius = value; }
		}
		#endregion

		#region TerrainPushOffRadius
		float mTerrainPushOffRadius;
		public float TerrainPushOffRadius
		{
			get { return this.mTerrainPushOffRadius; }
			set { this.mTerrainPushOffRadius = value; }
		}
		#endregion

		#region BuildingMagnetRange
		float mBuildingMagnetRange;
		public float BuildingMagnetRange
		{
			get { return this.mBuildingMagnetRange; }
			set { this.mBuildingMagnetRange = value; }
		}
		#endregion

		#region SoundBank
		string mSoundBank;
		// .bnk
		public string SoundBank
		{
			get { return this.mSoundBank; }
			set { this.mSoundBank = value; }
		}
		#endregion

		#region LeaderMenuNameID
		int mLeaderMenuNameID = TypeExtensions.kNone;
		[Meta.LocStringReference]
		public int LeaderMenuNameID
		{
			get { return this.mLeaderMenuNameID; }
			set { this.mLeaderMenuNameID = value; }
		}
		#endregion

		#region PowerFromHero
		bool mPowerFromHero;
		public bool PowerFromHero
		{
			get { return this.mPowerFromHero; }
			set { this.mPowerFromHero = value; }
		}
		#endregion

		#region UIControlBackground
		string mUIControlBackground;
		[Meta.TextureReference]
		public string UIControlBackground
		{
			get { return this.mUIControlBackground; }
			set { this.mUIControlBackground = value; }
		}
		#endregion

		// Empty Civs just have a name
		[Browsable(false)]
		public bool IsEmpty { get { return this.mTechID.IsNotNone(); } }

		public BCiv()
		{
			var textData = this.CreateDatabaseObjectUserInterfaceTextData();
			textData.HasDisplayNameID = true;
		}

		#region ITagElementStreamable<string> Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);

			var xs = s.GetSerializerInterface();

			s.StreamAttributeOpt("Alpha", ref this.mAlpha, Predicates.IsNotNone);
			xs.StreamDBID(s, "CivTech", ref this.mTechID, DatabaseObjectKind.Tech);
			xs.StreamDBID(s, "CommandAckObject", ref this.mCommandAckObjectID, DatabaseObjectKind.Object);
			xs.StreamDBID(s, "RallyPointObject", ref this.mRallyPointObjectID, DatabaseObjectKind.Object);
			xs.StreamDBID(s, "LocalRallyPointObject", ref this.mLocalRallyPointObjectID, DatabaseObjectKind.Object);
			xs.StreamDBID(s, "Transport", ref this.mTransportObjectID, DatabaseObjectKind.Object);
			xs.StreamDBID(s, "TransportTrigger", ref this.mTransportTriggerObjectID, DatabaseObjectKind.Object);
			s.StreamElementOpt("ExpandHull", ref this.mHullExpansionRadius, Predicates.IsNotZero);
			s.StreamElementOpt("TerrainPushOff", ref this.mTerrainPushOffRadius, Predicates.IsNotZero);
			s.StreamElementOpt("BuildingMagnetRange", ref this.mBuildingMagnetRange, Predicates.IsNotZero);
			s.StreamStringOpt("SoundBank", ref this.mSoundBank, toLower: false, type: XML.XmlUtil.kSourceElement);
			xs.StreamStringID(s, "LeaderMenuNameID", ref this.mLeaderMenuNameID);
			s.StreamElementOpt("PowerFromHero", ref this.mPowerFromHero, Predicates.IsTrue);
			s.StreamStringOpt("UIControlBackground", ref this.mUIControlBackground, toLower: false, type: XML.XmlUtil.kSourceElement);
		}
		#endregion
	};
}