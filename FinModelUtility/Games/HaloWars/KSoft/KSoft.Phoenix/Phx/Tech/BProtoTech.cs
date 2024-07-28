//#define TECH_NEEDS_ToLowerDataNames

namespace KSoft.Phoenix.Phx
{
	/* Deprecated fields:
	 * - type: This attribute is no longer a thing.
	*/

	public sealed class BProtoTech
		: DatabaseIdObject
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams("Tech")
		{
			RootName = "TechTree",
			DataName = "name",
			Flags = 0
#if TECH_NEEDS_ToLowerDataNames
				| XML.BCollectionXmlParamsFlags.ToLowerDataNames
#endif
				| XML.BCollectionXmlParamsFlags.RequiresDataNamePreloading
				| XML.BCollectionXmlParamsFlags.SupportsUpdating
		};
		public static readonly Collections.BListAutoIdParams kBListParams
#if TECH_NEEDS_ToLowerDataNames
			= new Collections.BListAutoIdParams()
		{
			ToLowerDataNames = kBListXmlParams.ToLowerDataNames,
		};
#else
			= null;
#endif

		public static readonly Engine.XmlFileInfo kXmlFileInfo = new Engine.XmlFileInfo
		{
			Location = Engine.ContentStorage.Game,
			Directory = Engine.GameDirectory.Data,
			FileName = "Techs.xml",
			RootName = kBListXmlParams.RootName
		};
		public static readonly Engine.XmlFileInfo kXmlFileInfoUpdate = new Engine.XmlFileInfo
		{
			Location = Engine.ContentStorage.Update,
			Directory = Engine.GameDirectory.Data,
			FileName = "Techs_Update.xml",
			RootName = kBListXmlParams.RootName
		};
		public static readonly Engine.ProtoDataXmlFileInfo kProtoFileInfo = new Engine.ProtoDataXmlFileInfo(
			Engine.XmlFilePriority.ProtoData,
			kXmlFileInfo,
			kXmlFileInfoUpdate);

		static readonly Collections.CodeEnum<BProtoTechFlags> kFlagsProtoEnum = new Collections.CodeEnum<BProtoTechFlags>();
		static readonly Collections.BBitSetParams kFlagsParams = new Collections.BBitSetParams(() => kFlagsProtoEnum);
		#endregion

		#region Alpha
		BProtoTechAlphaMode mAlphaMode = BProtoTechAlphaMode.None;
		public BProtoTechAlphaMode AlphaMode
		{
			get { return this.mAlphaMode; }
			set { this.mAlphaMode = value; }
		}
		#endregion

		public Collections.BBitSet Flags { get; private set; }

		#region Icon
		string mIcon;
		[Meta.TextureReference]
		public string Icon
		{
			get { return this.mIcon; }
			set { this.mIcon = value; }
		}
		#endregion

		#region ResearchCompleteSound
		string mResearchCompleteSound;
		[Meta.SoundCueReference]
		public string ResearchCompleteSound
		{
			get { return this.mResearchCompleteSound; }
			set { this.mResearchCompleteSound = value; }
		}
		#endregion

		#region ResearchAnim
		string mResearchAnim;
		[Meta.BAnimTypeReference]
		public string ResearchAnim
		{
			get { return this.mResearchAnim; }
			set { this.mResearchAnim = value; }
		}
		#endregion

		public BProtoTechPrereqs Prereqs { get; private set; }
		public Collections.BListArray<BProtoTechEffect> Effects { get; private set; }

		#region StatsObjectID
		int mStatsObjectID = TypeExtensions.kNone;
		[Meta.BProtoObjectReference]
		public int StatsObjectID
		{
			get { return this.mStatsObjectID; }
			set { this.mStatsObjectID = value; }
		}
		#endregion

		public bool HasPrereqs { get { return this.Prereqs != null && this.Prereqs.IsNotEmpty; } }

		public BProtoTech() : base(BResource.kBListTypeValuesParams, BResource.kBListTypeValuesXmlParams_CostLowercaseType)
		{
			var textData = this.CreateDatabaseObjectUserInterfaceTextData();
			textData.HasDisplayNameID = true;
			textData.HasRolloverTextID = true;
			textData.HasPrereqTextID = true;

			this.Flags = new Collections.BBitSet(kFlagsParams);
			this.Prereqs = new BProtoTechPrereqs();
			this.Effects = new Collections.BListArray<BProtoTechEffect>();
		}

		#region IXmlElementStreamable Members
		protected override void StreamDbId<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			// This isn't always used, nor unique.
			// In fact, the engine doesn't even use it beyond reading it!
			s.StreamElementOpt("DBID", this, obj => obj.DbId, Predicates.IsNotNone);
		}

		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);

			var xs = s.GetSerializerInterface();

			int alpha = (int) this.mAlphaMode;
			s.StreamAttributeOpt("Alpha", ref alpha, Predicates.IsNotNone);
			this.mAlphaMode = (BProtoTechAlphaMode)alpha;

			XML.XmlUtil.Serialize(s, this.Flags, XML.BBitSetXmlParams.kFlagsSansRoot);

			if (s.IsReading)
			{
				using (var bm = s.EnterCursorBookmarkOpt("Status")) if (bm.IsNotNull)
				{
					string statusValue = null;
					s.ReadCursor(ref statusValue);
					if (string.Equals(statusValue, "Unobtainable", System.StringComparison.OrdinalIgnoreCase))
						this.Flags.Set((int)BProtoTechFlags.Unobtainable);
				}
			}

			s.StreamStringOpt("Icon", ref this.mIcon, toLower: false, type: XML.XmlUtil.kSourceElement);
			s.StreamStringOpt("ResearchCompleteSound", ref this.mResearchCompleteSound, toLower: false, type: XML.XmlUtil.kSourceElement);
			s.StreamStringOpt("ResearchAnim", ref this.mResearchAnim, toLower: false, type: XML.XmlUtil.kSourceElement);
			using (var bm = s.EnterCursorBookmarkOpt("Prereqs", this, v => v.HasPrereqs)) if (bm.IsNotNull)
			{
				this.Prereqs.Serialize(s);
			}
			XML.XmlUtil.Serialize(s, this.Effects, BProtoTechEffect.kBListXmlParams);
			xs.StreamDBID(s, "StatsObject", ref this.mStatsObjectID, DatabaseObjectKind.Object);
		}
		#endregion
	};
}