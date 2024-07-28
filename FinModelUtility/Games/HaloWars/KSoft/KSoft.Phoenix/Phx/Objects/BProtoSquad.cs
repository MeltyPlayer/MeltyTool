//SQUAD_NEEDS_ToLowerDataNames

using BVector = System.Numerics.Vector4;

namespace KSoft.Phoenix.Phx
{
	/* Deprecated fields:
	 * - MinimapIcon: This is in ProtoObject now.
	*/

	public sealed class BProtoSquad
		: DatabaseIdObject
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams("Squad")
		{
			DataName = kXmlAttrName,
			Flags = 0
#if SQUAD_NEEDS_ToLowerDataNames
				| XML.BCollectionXmlParamsFlags.ToLowerDataNames |
#endif
				| XML.BCollectionXmlParamsFlags.RequiresDataNamePreloading
				| XML.BCollectionXmlParamsFlags.SupportsUpdating
		};
		public static readonly Collections.BListAutoIdParams kBListParams
#if SQUAD_NEEDS_ToLowerDataNames
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
			FileName = "Squads.xml",
			RootName = kBListXmlParams.RootName
		};
		public static readonly Engine.XmlFileInfo kXmlFileInfoUpdate = new Engine.XmlFileInfo
		{
			Location = Engine.ContentStorage.Update,
			Directory = Engine.GameDirectory.Data,
			FileName = "Squads_Update.xml",
			RootName = kBListXmlParams.RootName
		};
		public static readonly Engine.ProtoDataXmlFileInfo kProtoFileInfo = new Engine.ProtoDataXmlFileInfo(
			Engine.XmlFilePriority.ProtoData,
			kXmlFileInfo,
			kXmlFileInfoUpdate);

		static readonly Collections.CodeEnum<BProtoSquadFlags> kFlagsProtoEnum = new Collections.CodeEnum<BProtoSquadFlags>();
		static readonly Collections.BBitSetParams kFlagsParams = new Collections.BBitSetParams(() => kFlagsProtoEnum);
		#endregion

		#region FormationType
		BProtoSquadFormationType mFormationType = BProtoSquadFormationType.Standard;
		public BProtoSquadFormationType FormationType
		{
			get { return this.mFormationType; }
			set { this.mFormationType = value; }
		}
		#endregion
		bool mUpdate;
		public bool Update { get { return this.mUpdate; } }

		#region PortraitIcon
		string mPortraitIcon;
		public string PortraitIcon
		{
			get { return this.mPortraitIcon; }
			set { this.mPortraitIcon = value; }
		}
		#endregion
		#region AltIcon
		string mAltIcon;
		public string AltIcon
		{
			get { return this.mAltIcon; }
			set { this.mAltIcon = value; }
		}
		#endregion
		#region Stance
		BSquadStance mStance = BSquadStance.Defensive;
		public BSquadStance Stance
		{
			get { return this.mStance; }
			set { this.mStance = value; }
		}
		#endregion
		#region TrainAnim
		string mTrainAnim;
		[Meta.BAnimTypeReference]
		public string TrainAnim
		{
			get { return this.mTrainAnim; }
			set { this.mTrainAnim = value; }
		}
		#endregion
		#region Selection
		static BVector cDefaultSelectionRadius { get { return new BVector(1.0f, 0.0f, 1.0f, 0.0f); } }

		BVector mSelectionRadius = cDefaultSelectionRadius;
		public BVector SelectionRadius
		{
			get { return this.mSelectionRadius; }
			set { this.mSelectionRadius = value; }
		}

		BVector mSelectionOffset;
		public BVector SelectionOffset
		{
			get { return this.mSelectionOffset; }
			set { this.mSelectionOffset = value; }
		}

		bool mSelectionConformToTerrain;
		public bool SelectionConformToTerrain
		{
			get { return this.mSelectionConformToTerrain; }
			set { this.mSelectionConformToTerrain = value; }
		}

		bool mSelectionAllowOrientation = true;
		public bool SelectionAllowOrientation
		{
			get { return this.mSelectionAllowOrientation; }
			set { this.mSelectionAllowOrientation = value; }
		}

		bool HasSelectionData { get {
			return this.SelectionRadius != cDefaultSelectionRadius
				|| PhxPredicates.IsNotZero(this.SelectionOffset)
				||
				this.SelectionConformToTerrain
				||
				this.SelectionAllowOrientation==false;
		} }
		#endregion
		#region HPBar

		int mHPBarID = TypeExtensions.kNone;
		[Meta.BProtoHPBarReference]
		public int HPBarID
		{
			get { return this.mHPBarID; }
			set { this.mHPBarID = value; }
		}

		BVector mHPBarSize;
		public BVector HPBarSize
		{
			get { return this.mHPBarSize; }
			set { this.mHPBarSize = value; }
		}

		BVector mHPBarOffset;
		public BVector HPBarOffset
		{
			get { return this.mHPBarOffset; }
			set { this.mHPBarOffset = value; }
		}

		bool HasHPBarData { get {
			return this.HPBarID.IsNotNone()
				|| PhxPredicates.IsNotZero(this.HPBarSize)
				|| PhxPredicates.IsNotZero(this.HPBarOffset);
		} }
		#endregion
		#region VeterancyBar
		static BVector cDefaultVeterancyBarSize { get { return new BVector(1.0f, 1.0f, 0.0f, 0.0f); } }

		int mVeterancyBarID = TypeExtensions.kNone;
		[Meta.BProtoVeterancyBarReference]
		public int VeterancyBarID
		{
			get { return this.mVeterancyBarID; }
			set { this.mVeterancyBarID = value; }
		}

		int mVeterancyCenteredBarID = TypeExtensions.kNone;
		[Meta.BProtoVeterancyBarReference]
		public int VeterancyCenteredBarID
		{
			get { return this.mVeterancyCenteredBarID; }
			set { this.mVeterancyCenteredBarID = value; }
		}

		BVector mVeterancyBarSize = cDefaultVeterancyBarSize;
		public BVector VeterancyBarSize
		{
			get { return this.mVeterancyBarSize; }
			set { this.mVeterancyBarSize = value; }
		}

		BVector mVeterancyBarOffset;
		public BVector VeterancyBarOffset
		{
			get { return this.mVeterancyBarOffset; }
			set { this.mVeterancyBarOffset = value; }
		}

		bool HasVeterancyBarData { get {
			return this.VeterancyBarID.IsNotNone()
				||
				this.VeterancyCenteredBarID.IsNotNone()
				||
				this.VeterancyBarSize != cDefaultVeterancyBarSize
				|| PhxPredicates.IsNotZero(this.VeterancyBarOffset);
		} }
		#endregion
		#region AbilityRecoveryBar
		static BVector cDefaultAbilityRecoveryBarSize { get { return new BVector(1.0f, 1.0f, 0.0f, 0.0f); } }

		int mAbilityRecoveryBarID = TypeExtensions.kNone;
		[Meta.BProtoPieProgressReference]
		public int AbilityRecoveryBarID
		{
			get { return this.mAbilityRecoveryBarID; }
			set { this.mAbilityRecoveryBarID = value; }
		}

		int mAbilityRecoveryCenteredBarID = TypeExtensions.kNone;
		[Meta.BProtoPieProgressReference]
		public int AbilityRecoveryCenteredBarID
		{
			get { return this.mAbilityRecoveryCenteredBarID; }
			set { this.mAbilityRecoveryCenteredBarID = value; }
		}

		BVector mAbilityRecoveryBarSize = cDefaultAbilityRecoveryBarSize;
		public BVector AbilityRecoveryBarSize
		{
			get { return this.mAbilityRecoveryBarSize; }
			set { this.mAbilityRecoveryBarSize = value; }
		}

		BVector mAbilityRecoveryBarOffset;
		public BVector AbilityRecoveryBarOffset
		{
			get { return this.mAbilityRecoveryBarOffset; }
			set { this.mAbilityRecoveryBarOffset = value; }
		}

		bool HasAbilityRecoveryBarData { get {
			return this.AbilityRecoveryBarID.IsNotNone()
				||
				this.AbilityRecoveryCenteredBarID.IsNotNone()
				||
				this.AbilityRecoveryBarSize != cDefaultAbilityRecoveryBarSize
				|| PhxPredicates.IsNotZero(this.AbilityRecoveryBarOffset);
		} }
		#endregion
		#region BobbleHeadID
		int mBobbleHeadID = TypeExtensions.kNone;
		[Meta.BProtoBobbleHeadReference]
		public int BobbleHeadID
		{
			get { return this.mBobbleHeadID; }
			set { this.mBobbleHeadID = value; }
		}
		#endregion
		#region BuildingStrengthDisplayID
		int mBuildingStrengthDisplayID = TypeExtensions.kNone;
		[Meta.BProtoBuildingStrengthReference]
		public int BuildingStrengthDisplayID
		{
			get { return this.mBuildingStrengthDisplayID; }
			set { this.mBuildingStrengthDisplayID = value; }
		}
		#endregion
		#region CryoPoints
		float mCryoPoints = PhxUtil.kInvalidSingle;
		/// <remarks>Actually defaults to the default value in GameData</remarks>
		public float CryoPoints
		{
			get { return this.mCryoPoints; }
			set { this.mCryoPoints = value; }
		}
		#endregion
		#region DazeResist
		float mDazeResist = 1.0f;
		public float DazeResist
		{
			get { return this.mDazeResist; }
			set { this.mDazeResist = value; }
		}
		#endregion
		#region Birth

		BSquadBirthType mBirthType = BSquadBirthType.Invalid;
		public BSquadBirthType BirthType
		{
			get { return this.mBirthType; }
			set { this.mBirthType = value; }
		}

		string mBirthBone;
		public string BirthBone
		{
			get { return this.mBirthBone; }
			set { this.mBirthBone = value; }
		}

		string mBirthEndBone;
		public string BirthEndBone
		{
			get { return this.mBirthEndBone; }
			set { this.mBirthEndBone = value; }
		}

		string mBirthAnim0;
		[Meta.BAnimTypeReference]
		public string BirthAnim0
		{
			get { return this.mBirthAnim0; }
			set { this.mBirthAnim0 = value; }
		}

		string mBirthAnim1;
		[Meta.BAnimTypeReference]
		public string BirthAnim1
		{
			get { return this.mBirthAnim1; }
			set { this.mBirthAnim1 = value; }
		}

		string mBirthAnim2;
		[Meta.BAnimTypeReference]
		public string BirthAnim2
		{
			get { return this.mBirthAnim2; }
			set { this.mBirthAnim2 = value; }
		}

		string mBirthAnim3;
		[Meta.BAnimTypeReference]
		public string BirthAnim3
		{
			get { return this.mBirthAnim3; }
			set { this.mBirthAnim3 = value; }
		}

		string mBirthTrainerAnim;
		[Meta.BAnimTypeReference]
		public string BirthTrainerAnim
		{
			get { return this.mBirthTrainerAnim; }
			set { this.mBirthTrainerAnim = value; }
		}

		bool HasBirthData { get {
			return this.BirthType != BSquadBirthType.Invalid
				||
				this.BirthBone.IsNotNullOrEmpty()
				||
				this.BirthEndBone.IsNotNullOrEmpty()
				||
				this.BirthAnim0.IsNotNullOrEmpty()
				||
				this.BirthAnim1.IsNotNullOrEmpty()
				||
				this.BirthAnim2.IsNotNullOrEmpty()
				||
				this.BirthAnim3.IsNotNullOrEmpty()
				||
				this.BirthTrainerAnim.IsNotNullOrEmpty();
		} }
		#endregion
		public Collections.BListArray<BProtoSquadUnit> Units { get; private set; }
		#region SubSelectSort
		int mSubSelectSort = int.MaxValue;
		public int SubSelectSort
		{
			get { return this.mSubSelectSort; }
			set { this.mSubSelectSort = value; }
		}
		#endregion
		#region TurnRadius

		float mTurnRadiusMin = PhxUtil.kInvalidSingle;
		public float TurnRadiusMin
		{
			get { return this.mTurnRadiusMin; }
			set { this.mTurnRadiusMin = value; }
		}

		float mTurnRadiusMax = PhxUtil.kInvalidSingle;
		public float TurnRadiusMax
		{
			get { return this.mTurnRadiusMax; }
			set { this.mTurnRadiusMax = value; }
		}

		bool HasTurnRadiusData { get {
			return PhxPredicates.IsNotInvalid(this.TurnRadiusMin)
				|| PhxPredicates.IsNotInvalid(this.TurnRadiusMax);
		} }
		#endregion
		#region LeashDistance
		float mLeashDistance;
		public float LeashDistance
		{
			get { return this.mLeashDistance; }
			set { this.mLeashDistance = value; }
		}
		#endregion
		#region LeashDeadzone
		const float cDefaultLeashDeadzone = 10.0f;

		float mLeashDeadzone = cDefaultLeashDeadzone;
		public float LeashDeadzone
		{
			get { return this.mLeashDeadzone; }
			set { this.mLeashDeadzone = value; }
		}
		#endregion
		#region LeashRecallDelay
		const int cDefaultLeashRecallDelay = 2 * 1000;

		int mLeashRecallDelay = cDefaultLeashRecallDelay;
		/// <remarks>in milliseconds</remarks>
		public int LeashRecallDelay
		{
			get { return this.mLeashRecallDelay; }
			set { this.mLeashRecallDelay = value; }
		}
		#endregion
		#region AggroDistance
		float mAggroDistance;
		public float AggroDistance
		{
			get { return this.mAggroDistance; }
			set { this.mAggroDistance = value; }
		}
		#endregion
		#region MinimapScale
		float mMinimapScale;
		public float MinimapScale
		{
			get { return this.mMinimapScale; }
			set { this.mMinimapScale = value; }
		}
		#endregion
		public Collections.BBitSet Flags { get; private set; }
		#region Level
		int mLevel;
		public int Level
		{
			get { return this.mLevel; }
			set { this.mLevel = value; }
		}
		#endregion
		#region TechLevel
		int mTechLevel;
		public int TechLevel
		{
			get { return this.mTechLevel; }
			set { this.mTechLevel = value; }
		}
		#endregion
		public Collections.BListArray<BProtoSquadSound> Sounds { get; private set; }
		#region RecoveringEffectID
		int mRecoveringEffectID = TypeExtensions.kNone;
		[Meta.BProtoObjectReference]
		public int RecoveringEffectID
		{
			get { return this.mRecoveringEffectID; }
			set { this.mRecoveringEffectID = value; }
		}
		#endregion
		#region CanAttackWhileMoving
		bool mCanAttackWhileMoving = true;
		public bool CanAttackWhileMoving
		{
			get { return this.mCanAttackWhileMoving; }
			set { this.mCanAttackWhileMoving = value; }
		}
		#endregion

		/// <summary>Is this Squad just made up of a single Unit?</summary>
		public bool SquadIsUnit { get {
			return this.Units.Count == 1 && this.Units[0].Count == 1;
		}}

		public BProtoSquad() : base(BResource.kBListTypeValuesParams, BResource.kBListTypeValuesXmlParams_CostLowercaseType)
		{
			var textData = this.CreateDatabaseObjectUserInterfaceTextData();
			textData.HasDisplayNameID = true;
			textData.HasRolloverTextID = true;
			textData.HasStatsNameID = true;
			textData.HasPrereqTextID = true;
			textData.HasRoleTextID = true;

			this.Units = new Collections.BListArray<BProtoSquadUnit>();

			this.Flags = new Collections.BBitSet(kFlagsParams);

			this.Sounds = new Collections.BListArray<BProtoSquadSound>();
		}

		#region IXmlElementStreamable Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);
			var xs = s.GetSerializerInterface();

			s.StreamAttributeEnumOpt("formationType", ref this.mFormationType, e => e != BProtoSquadFormationType.Standard);
			s.StreamAttributeOpt("update", ref this.mUpdate, Predicates.IsTrue);

			s.StreamElementOpt("PortraitIcon", ref this.mPortraitIcon, Predicates.IsNotNullOrEmpty);
			s.StreamElementOpt("AltIcon", ref this.mAltIcon, Predicates.IsNotNullOrEmpty);
			s.StreamElementEnumOpt("Stance", ref this.mStance, e => e != BSquadStance.Defensive);
			s.StreamElementOpt("TrainAnim", ref this.mTrainAnim, Predicates.IsNotNullOrEmpty);
			#region Selection
			using (var bm = s.EnterCursorBookmarkOpt("Selection", this, v => v.HasSelectionData)) if (bm.IsNotNull)
			{
				// #NOTE assumes cDefaultSelectionRadius's XZ values are 1.0
				s.StreamElementOpt("RadiusX", ref this.mSelectionRadius.X, PhxPredicates.IsNotOne);
				s.StreamElementOpt("RadiusZ", ref this.mSelectionRadius.Z, PhxPredicates.IsNotOne);
				s.StreamElementOpt("YOffset", ref this.mSelectionOffset.Y, Predicates.IsNotZero);
				s.StreamElementOpt("ZOffset", ref this.mSelectionOffset.Z, Predicates.IsNotZero);
				s.StreamElementOpt("ConformToTerrain", ref this.mSelectionConformToTerrain, Predicates.IsTrue);
				s.StreamElementOpt("AllowOrientation", ref this.mSelectionAllowOrientation, Predicates.IsFalse);
			}
			#endregion
			#region HPBar
			using (var bm = s.EnterCursorBookmarkOpt("HPBar", this, v => v.HasHPBarData)) if (bm.IsNotNull)
			{
				xs.StreamHPBarName(s, XML.XmlUtil.kNoXmlName, ref this.mHPBarID, HPBarDataObjectKind.HPBar, isOptional: false, xmlSource: XML.XmlUtil.kSourceCursor);
				s.StreamAttributeOpt("sizeX", ref this.mHPBarSize.X, Predicates.IsNotZero);
				s.StreamAttributeOpt("sizeY", ref this.mHPBarSize.Y, Predicates.IsNotZero);
				s.StreamBVector("offset", ref this.mHPBarOffset, xmlSource: XML.XmlUtil.kSourceAttr);
			}
			#endregion
			#region VeterancyBar
			using (var bm = s.EnterCursorBookmarkOpt("VeterancyBar", this, v => v.HasVeterancyBarData)) if (bm.IsNotNull)
			{
				xs.StreamHPBarName(s, XML.XmlUtil.kNoXmlName, ref this.mVeterancyBarID, HPBarDataObjectKind.VeterancyBar, isOptional: false, xmlSource: XML.XmlUtil.kSourceCursor);
				xs.StreamHPBarName(s, "Centered", ref this.mVeterancyCenteredBarID, HPBarDataObjectKind.VeterancyBar, isOptional: false, xmlSource: XML.XmlUtil.kSourceAttr);
				// #NOTE assumes cDefaultVeterancyBarSize's XY values are 1.0
				s.StreamAttributeOpt("sizeX", ref this.mVeterancyBarSize.X, PhxPredicates.IsNotOne);
				s.StreamAttributeOpt("sizeY", ref this.mVeterancyBarSize.Y, PhxPredicates.IsNotOne);
				s.StreamAttributeOpt("offsetX", ref this.mVeterancyBarOffset.X, Predicates.IsNotZero);
				s.StreamAttributeOpt("offsetY", ref this.mVeterancyBarOffset.Y, Predicates.IsNotZero);
			}
			#endregion
			#region AbilityRecoveryBar
			using (var bm = s.EnterCursorBookmarkOpt("AbilityRecoveryBar", this, v => v.HasAbilityRecoveryBarData)) if (bm.IsNotNull)
			{
				xs.StreamHPBarName(s, XML.XmlUtil.kNoXmlName, ref this.mAbilityRecoveryBarID, HPBarDataObjectKind.PieProgress, isOptional: false, xmlSource: XML.XmlUtil.kSourceCursor);
				xs.StreamHPBarName(s, "Centered", ref this.mAbilityRecoveryCenteredBarID, HPBarDataObjectKind.PieProgress, isOptional: false, xmlSource: XML.XmlUtil.kSourceAttr);
				// #NOTE assumes cDefaultAbilityRecoveryBarSize's XY values are 1.0
				s.StreamAttributeOpt("sizeX", ref this.mAbilityRecoveryBarSize.X, PhxPredicates.IsNotOne);
				s.StreamAttributeOpt("sizeY", ref this.mAbilityRecoveryBarSize.Y, PhxPredicates.IsNotOne);
				s.StreamAttributeOpt("offsetX", ref this.mAbilityRecoveryBarOffset.X, Predicates.IsNotZero);
				s.StreamAttributeOpt("offsetY", ref this.mAbilityRecoveryBarOffset.Y, Predicates.IsNotZero);
			}
			#endregion
			xs.StreamHPBarName(s, "BobbleHead", ref this.mBobbleHeadID, HPBarDataObjectKind.BobbleHead);
			xs.StreamHPBarName(s, "BuildingStrengthDisplay", ref this.mBuildingStrengthDisplayID, HPBarDataObjectKind.BuildingStrength);
			s.StreamElementOpt("CryoPoints", ref this.mCryoPoints, PhxPredicates.IsNotInvalid);
			s.StreamElementOpt("DazeResist", ref this.mDazeResist, PhxPredicates.IsNotOne);
			#region Birth
			using (var bm = s.EnterCursorBookmarkOpt("Birth", this, v => v.HasBirthData)) if (bm.IsNotNull)
			{
				s.StreamCursorEnum(ref this.mBirthType);
				s.StreamAttributeOpt("spawnpoint", ref this.mBirthBone, Predicates.IsNotNullOrEmpty);
				s.StreamAttributeOpt("endPoint", ref this.mBirthEndBone, Predicates.IsNotNullOrEmpty);
				s.StreamAttributeOpt("anim0", ref this.mBirthAnim0, Predicates.IsNotNullOrEmpty);
				s.StreamAttributeOpt("anim1", ref this.mBirthAnim1, Predicates.IsNotNullOrEmpty);
				s.StreamAttributeOpt("anim2", ref this.mBirthAnim2, Predicates.IsNotNullOrEmpty);
				s.StreamAttributeOpt("anim3", ref this.mBirthAnim3, Predicates.IsNotNullOrEmpty);
				s.StreamAttributeOpt("trainerAnim", ref this.mBirthTrainerAnim, Predicates.IsNotNullOrEmpty);
			}
			#endregion
			XML.XmlUtil.Serialize(s, this.Units, BProtoSquadUnit.kBListXmlParams);
			s.StreamElementOpt("SubSelectSort", ref this.mSubSelectSort, v => v != int.MaxValue);
			#region TurnRadius
			using (var bm = s.EnterCursorBookmarkOpt("TurnRadius", this, v => v.HasTurnRadiusData)) if (bm.IsNotNull)
			{
				s.StreamAttributeOpt("min", ref this.mTurnRadiusMin, PhxPredicates.IsNotInvalid);
				s.StreamAttributeOpt("max", ref this.mTurnRadiusMax, PhxPredicates.IsNotInvalid);
			}
			#endregion
			s.StreamElementOpt("LeashDistance", ref this.mLeashDistance, Predicates.IsNotZero);
			s.StreamElementOpt("LeashDeadzone", ref this.mLeashDeadzone, v => v != cDefaultLeashDeadzone);
			s.StreamElementOpt("LeashRecallDelay", ref this.mLeashRecallDelay, v => v != cDefaultLeashRecallDelay);
			s.StreamElementOpt("AggroDistance", ref this.mAggroDistance, Predicates.IsNotZero);
			s.StreamElementOpt("MinimapScale", ref this.mMinimapScale, Predicates.IsNotZero);
			XML.XmlUtil.Serialize(s, this.Flags, XML.BBitSetXmlParams.kFlagsSansRoot);
			s.StreamElementOpt("Level", ref this.mLevel, Predicates.IsNotZero);
			s.StreamElementOpt("TechLevel", ref this.mTechLevel, Predicates.IsNotZero);
			XML.XmlUtil.Serialize(s, this.Sounds, BProtoSquadSound.kBListXmlParams);
			xs.StreamDBID(s, "RecoveringEffect", ref this.mRecoveringEffectID, DatabaseObjectKind.Object);
			s.StreamElementOpt("CanAttackWhileMoving", ref this.mCanAttackWhileMoving, Predicates.IsFalse);
		}
		#endregion
	};
}