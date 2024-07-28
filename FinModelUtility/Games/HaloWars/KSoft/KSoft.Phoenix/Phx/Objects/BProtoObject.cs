using System.Collections.Generic;

using BProtoObjectID = System.Int32;
using BVector = System.Numerics.Vector4;

namespace KSoft.Phoenix.Phx
{
	/* Deprecated fields:
	 * - TrackInterceptDistance: This was made a global and moved to GameData.
	 * - FlashUI: these are no longer defined in this proto.
	 * - UIVisual: Not sure what happened with this. There's just the Visual field now though.
	 * - DazeResist: This was moved to ProtoSquad.
	 *
	 * #NOTE
	 * - "fx_impact_effect_01" - Hitpoints field with value "20000000" gets written as "2E+07" with TagElementTextStream's ToString("r") impl. This *should* get parsed correctly.
	 * - "cpgn_scn07_scarabBoss_02" - DeathFadeDelayTime value, 99999999, gets rounded up to 1E+08 when we serialize it
	*/

	public sealed class BProtoObject
		: DatabaseIdObject
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams("Object")
		{
			DataName = kXmlAttrName,
			Flags = 0
				| XML.BCollectionXmlParamsFlags.ToLowerDataNames
				| XML.BCollectionXmlParamsFlags.RequiresDataNamePreloading
				| XML.BCollectionXmlParamsFlags.SupportsUpdating
		};
		public static readonly Collections.BListAutoIdParams kBListParams = new Collections.BListAutoIdParams()
		{
			ToLowerDataNames = kBListXmlParams.ToLowerDataNames,
		};

		public static readonly Engine.XmlFileInfo kXmlFileInfo = new Engine.XmlFileInfo
		{
			Location = Engine.ContentStorage.Game,
			Directory = Engine.GameDirectory.Data,
			FileName = "Objects.xml",
			RootName = kBListXmlParams.RootName
		};
		public static readonly Engine.XmlFileInfo kXmlFileInfoUpdate = new Engine.XmlFileInfo
		{
			Location = Engine.ContentStorage.Update,
			Directory = Engine.GameDirectory.Data,
			FileName = "Objects_Update.xml",
			RootName = kBListXmlParams.RootName
		};
		public static readonly Engine.ProtoDataXmlFileInfo kProtoFileInfo = new Engine.ProtoDataXmlFileInfo(
			Engine.XmlFilePriority.ProtoData,
			kXmlFileInfo,
			kXmlFileInfoUpdate);

		static readonly Collections.CodeEnum<BProtoObjectFlags> kFlagsProtoEnum = new Collections.CodeEnum<BProtoObjectFlags>();
		static readonly Collections.BBitSetParams kFlagsParams = new Collections.BBitSetParams(() => kFlagsProtoEnum);

		static readonly Collections.BBitSetParams kObjectTypesParams = new Collections.BBitSetParams(db => db.ObjectTypes);
		static readonly XML.BBitSetXmlParams kObjectTypesXmlParams = new XML.BBitSetXmlParams("ObjectType");
	internal const string kXmlElementAttackGradeDPS = "AttackGradeDPS";
		internal const string kXmlElementReverseSpeed = "ReverseSpeed";
		#endregion

		#region Unused poop
		int mUnusedIs = TypeExtensions.kNone;
		public int UnusedIs { get { return this.mUnusedIs; } }
		int mUnusedId = TypeExtensions.kNone;
		public int UnusedId { get { return this.mUnusedId; } }
		#endregion
		bool mUpdate;
		public bool Update { get { return this.mUpdate; } }

		#region MovementType
		BProtoObjectMovementType mMovementType = BProtoObjectMovementType.None;
		public BProtoObjectMovementType MovementType
		{
			get { return this.mMovementType; }
			set { this.mMovementType = value; }
		}
		#endregion

		public Collections.BListArray<		BHardpoint> Hardpoints { get; private set; }
			= new Collections.BListArray<	BHardpoint>();
		public List<string> SingleBoneIKs { get; private set; }
			= new List<string>();
		public Collections.BListArray<		BGroundIKNode> GroundIKs { get; private set; }
			= new Collections.BListArray<	BGroundIKNode>();
		public Collections.BListArray<		BSweetSpotIKNode> SweetSpotIKs { get; private set; }
			= new Collections.BListArray<	BSweetSpotIKNode>();

		#region ObstructionRadius
		BVector mObstructionRadius;
		public BVector ObstructionRadius
		{
			get { return this.mObstructionRadius; }
			set { this.mObstructionRadius = value; }
		}
		#endregion

		#region TerrainFlatten
		// Automatic terrain flattening for building placement

		BVector mTerrainFlattennMin0;
		public BVector TerrainFlattenMin0
		{
			get { return this.mTerrainFlattennMin0; }
			set { this.mTerrainFlattennMin0 = value; }
		}
		BVector mTerrainFlattennMax0;
		public BVector TerrainFlattenMax0
		{
			get { return this.mTerrainFlattennMax0; }
			set { this.mTerrainFlattennMax0 = value; }
		}

		BVector mTerrainFlattennMin1;
		public BVector TerrainFlattenMin1
		{
			get { return this.mTerrainFlattennMin1; }
			set { this.mTerrainFlattennMin1 = value; }
		}
		BVector mTerrainFlattennMax1;
		public BVector TerrainFlattenMax1
		{
			get { return this.mTerrainFlattennMax1; }
			set { this.mTerrainFlattennMax1 = value; }
		}
		#endregion
		#region Parking lot
		// Parking lot position for building placement

		BVector mParkingLotMin;
		public BVector ParkingLotMin
		{
			get { return this.mParkingLotMin; }
			set { this.mParkingLotMin = value; }
		}
		BVector mParkingLotMax;
		public BVector ParkingLotMax
		{
			get { return this.mParkingLotMax; }
			set { this.mParkingLotMax = value; }
		}
		#endregion
		#region TerrainHeightTolerance
		const float cDefaultTerrainHeightTolerance = 10.0f;

		float mTerrainHeightTolerance = cDefaultTerrainHeightTolerance;
		public float TerrainHeightTolerance
		{
			get { return this.mTerrainHeightTolerance; }
			set { this.mTerrainHeightTolerance = value; }
		}
		#endregion
		#region Physics
		string mPhysicsInfo;
		[Meta.PhysicsInfoReference]
		public string PhysicsInfo
		{
			get { return this.mPhysicsInfo; }
			set { this.mPhysicsInfo = value; }
		}

		string mPhysicsReplacementInfo;
		[Meta.PhysicsInfoReference]
		public string PhysicsReplacementInfo
		{
			get { return this.mPhysicsReplacementInfo; }
			set { this.mPhysicsReplacementInfo = value; }
		}

		float mVelocity;
		public float Velocity
		{
			get { return this.mVelocity; }
			set { this.mVelocity = value; }
		}

		float mMaxVelocity;
		public float MaxVelocity
		{
			get { return this.mMaxVelocity; }
			set { this.mMaxVelocity = value; }
		}

		const float cDefaultReverseSpeed = 1.0f;
		float mReverseSpeed = cDefaultReverseSpeed;
		public float ReverseSpeed
		{
			get { return this.mReverseSpeed; }
			set { this.mReverseSpeed = value; }
		}

		float mAcceleration;
		public float Acceleration
		{
			get { return this.mAcceleration; }
			set { this.mAcceleration = value; }
		}

		float mTrackingDelay;
		// in seconds
		public float TrackingDelay
		{
			get { return this.mTrackingDelay; }
			set { this.mTrackingDelay = value; }
		}

		float mStartingVelocity;
		public float StartingVelocity
		{
			get { return this.mStartingVelocity; }
			set { this.mStartingVelocity = value; }
		}
		#endregion
		#region Fuel
		float mFuel;
		public float Fuel
		{
			get { return this.mFuel; }
			set { this.mFuel = value; }
		}
		#endregion
		#region Perturbance

		float mPerturbanceChance;
		public float PerturbanceChance
		{
			get { return this.mPerturbanceChance; }
			set { this.mPerturbanceChance = value; }
		}

		float mPerturbanceVelocity;
		public float PerturbanceVelocity
		{
			get { return this.mPerturbanceVelocity; }
			set { this.mPerturbanceVelocity = value; }
		}

		float mPerturbanceMinTime;
		public float PerturbanceMinTime
		{
			get { return this.mPerturbanceMinTime; }
			set { this.mPerturbanceMinTime = value; }
		}

		float mPerturbanceMaxTime;
		public float PerturbanceMaxTime
		{
			get { return this.mPerturbanceMaxTime; }
			set { this.mPerturbanceMaxTime = value; }
		}

		float mPerturbInitialVelocity;
		public float PerturbInitialVelocity
		{
			get { return this.mPerturbInitialVelocity; }
			set { this.mPerturbInitialVelocity = value; }
		}

		float mInitialPerturbanceMinTime;
		public float InitialPerturbanceMinTime
		{
			get { return this.mInitialPerturbanceMinTime; }
			set { this.mInitialPerturbanceMinTime = value; }
		}

		float mInitialPerturbanceMaxTime;
		public float InitialPerturbanceMaxTime
		{
			get { return this.mInitialPerturbanceMaxTime; }
			set { this.mInitialPerturbanceMaxTime = value; }
		}

		bool HasInitialPerturbanceData { get {
			return this.PerturbInitialVelocity != 0.0
				||
				this.InitialPerturbanceMinTime > 0.0
				||
				this.InitialPerturbanceMaxTime > 0.0;
		} }
		#endregion
		#region ActiveScan
		float mActiveScanChance;
		public float ActiveScanChance
		{
			get { return this.mActiveScanChance; }
			set { this.mActiveScanChance = value; }
		}

		float mActiveScanRadiusScale;
		public float ActiveScanRadiusScale
		{
			get { return this.mActiveScanRadiusScale; }
			set { this.mActiveScanRadiusScale = value; }
		}

		bool HasActiveScanData { get {
			return this.ActiveScanChance > 0.0
				||
				this.ActiveScanRadiusScale > 0.0;
		} }
		#endregion
		#region TurnRate
		float mTurnRate;
		public float TurnRate
		{
			get { return this.mTurnRate; }
			set { this.mTurnRate = value; }
		}
		#endregion
		#region Hitpoints
		float mHitpoints;
		public float Hitpoints
		{
			get { return this.mHitpoints; }
			set { this.mHitpoints = value; }
		}
		#endregion
		#region Shieldpoints
		float mShieldpoints;
		public float Shieldpoints
		{
			get { return this.mShieldpoints; }
			set { this.mShieldpoints = value; }
		}
		#endregion
		#region LOS
		float mLOS;
		public float LOS
		{
			get { return this.mLOS; }
			set { this.mLOS = value; }
		}
		#endregion
		#region Pick and Select

		float mPickRadius;
		public float PickRadius
		{
			get { return this.mPickRadius; }
			set { this.mPickRadius = value; }
		}

		float mPickOffset;
		public float PickOffset
		{
			get { return this.mPickOffset; }
			set { this.mPickOffset = value; }
		}

		BPickPriority mPickPriority;
		public BPickPriority PickPriority
		{
			get { return this.mPickPriority; }
			set { this.mPickPriority = value; }
		}

		BProtoObjectSelectType mSelectType;
		public BProtoObjectSelectType SelectType
		{
			get { return this.mSelectType; }
			set { this.mSelectType = value; }
		}

		BGotoType mGotoType;
		public BGotoType GotoType
		{
			get { return this.mGotoType; }
			set { this.mGotoType = value; }
		}

		BVector mSelectedRadius;
		public BVector SelectedRadius
		{
			get { return this.mSelectedRadius; }
			set { this.mSelectedRadius = value; }
		}

		#endregion
		#region RepairPoints
		float mRepairPoints;
		[Meta.UnusedData]
		public float RepairPoints
		{
			get { return this.mRepairPoints; }
			set { this.mRepairPoints = value; }
		}
		#endregion
		#region ClassType
		BProtoObjectClassType mClassType = BProtoObjectClassType.Object;
		public BProtoObjectClassType ClassType
		{
			get { return this.mClassType; }
			set { this.mClassType = value; }
		}
		#endregion
		#region TrainerType
		int mTrainerType = TypeExtensions.kNone;
		public int TrainerType
		{
			get { return this.mTrainerType; }
			set { this.mTrainerType = value; }
		}

		bool mTrainerApplyFormation;
		public bool TrainerApplyFormation
		{
			get { return this.mTrainerApplyFormation; }
			set { this.TrainerApplyFormation = value; }
		}

		bool HasTrainerTypeData { get {
			return this.TrainerType.IsNotNone()
				||
				this.TrainerApplyFormation;
		} }
		#endregion
		#region AutoLockDown
		BAutoLockDown mAutoLockDown;
		public BAutoLockDown AutoLockDown
		{
			get { return this.mAutoLockDown; }
			set { this.mAutoLockDown = value; }
		}
		#endregion
		#region CostEscalation
		float mCostEscalation = 1.0f;
		/// <summary>see: UNSC reactors</summary>
		// Also, CostEscalationObject and Flag.LinearCostEscalation
		public float CostEscalation
		{
			get { return this.mCostEscalation; }
			set { this.mCostEscalation = value; }
		}
		public bool HasCostEscalation { get { return this.CostEscalation > 0.0f; } }
		#endregion
		[Meta.BProtoObjectReference]
		public List<	BProtoObjectID> CostEscalationObjects { get; private set; }
			 = new List<BProtoObjectID>();
		public Collections.BListArray<		BProtoObjectCaptureCost> CaptureCosts { get; private set; }
			= new Collections.BListArray<	BProtoObjectCaptureCost>();
		#region Bounty
		float mBounty;
		/// <summary>Vet XP contribution value</summary>
		public float Bounty
		{
			get { return this.mBounty; }
			set { this.mBounty = value; }
		}
		#endregion
		#region AIAssetValueAdjust
		float mAIAssetValueAdjust;
		public float AIAssetValueAdjust
		{
			get { return this.mAIAssetValueAdjust; }
			set { this.mAIAssetValueAdjust = value; }
		}
		#endregion
		#region CombatValue
		float mCombatValue;
		/// <summary>Score value</summary>
		public float CombatValue
		{
			get { return this.mCombatValue; }
			set { this.mCombatValue = value; }
		}
		#endregion
		#region ResourceAmount
		float mResourceAmount;
		public float ResourceAmount
		{
			get { return this.mResourceAmount; }
			set { this.mResourceAmount = value; }
		}
		#endregion
		#region PlacementRules
		string mPlacementRules;
		/// <summary>PlacementRules file name (sans extension)</summary>
		public string PlacementRules
		{
			get { return this.mPlacementRules; }
			set { this.mPlacementRules = value; }
		}
		#endregion
		#region DeathFadeTime
		float mDeathFadeTime = 1.0f;
		public float DeathFadeTime
		{
			get { return this.mDeathFadeTime; }
			set { this.mDeathFadeTime = value; }
		}
		#endregion
		#region DeathFadeDelayTime
		float mDeathFadeDelayTime;
		public float DeathFadeDelayTime
		{
			get { return this.mDeathFadeDelayTime; }
			set { this.mDeathFadeDelayTime = value; }
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
		/// <remarks>Engine actually uses a fixed array that maps a BSquadMode to an AnimType</remarks>
		public Collections.BListArray<		BProtoObjectSquadModeAnim> SquadModeAnims { get; private set; }
			= new Collections.BListArray<	BProtoObjectSquadModeAnim>();
		#region RallyPoint
		BRallyPointType mRallyPoint = BRallyPointType.Invalid;
		public BRallyPointType RallyPoint
		{
			get { return this.mRallyPoint; }
			set { this.mRallyPoint = value; }
		}
		#endregion
		#region MaxProjectileHeight
		float mMaxProjectileHeight;
		public float MaxProjectileHeight
		{
			get { return this.mMaxProjectileHeight; }
			set { this.mMaxProjectileHeight = value; }
		}
		#endregion
		#region GroundIKTilt

		float mGroundIKTiltFactor;
		public float GroundIKTiltFactor
		{
			get { return this.mGroundIKTiltFactor; }
			set { this.mGroundIKTiltFactor = value; }
		}

		string mGroundIKTiltBoneName;
		public string GroundIKTiltBoneName
		{
			get { return this.mGroundIKTiltBoneName; }
			set { this.mGroundIKTiltBoneName = value; }
		}

		bool HasGroundIKTiltData { get {
			return this.GroundIKTiltFactor > 0.0
				||
				this.GroundIKTiltBoneName.IsNotNullOrEmpty();
		} }
		#endregion
		#region DeathReplacement
		int mDeathReplacementID = TypeExtensions.kNone;
		[Meta.BProtoObjectReference]
		public int DeathReplacementID
		{
			get { return this.mDeathReplacementID; }
			set { this.mDeathReplacementID = value; }
		}
		#endregion
		#region DeathSpawnSquad

		int mDeathSpawnSquadID = TypeExtensions.kNone;
		[Meta.BProtoSquadReference]
		public int DeathSpawnSquadID
		{
			get { return this.mDeathSpawnSquadID; }
			set { this.mDeathSpawnSquadID = value; }
		}

		bool mDeathSpawnSquadCheckPosition;
		public bool DeathSpawnSquadCheckPosition
		{
			get { return this.mDeathSpawnSquadCheckPosition; }
			set { this.mDeathSpawnSquadCheckPosition = value; }
		}

		int mDeathSpawnSquadMaxPopCount;
		public int DeathSpawnSquadMaxPopCount
		{
			get { return this.mDeathSpawnSquadMaxPopCount; }
			set { this.mDeathSpawnSquadMaxPopCount = value; }
		}

		bool HasDeathSpawnSquadData { get {
			return this.mDeathSpawnSquadID.IsNotNone()
				||
				this.mDeathSpawnSquadCheckPosition
				||
				this.mDeathSpawnSquadMaxPopCount > 0;
		} }
		#endregion
		#region SurfaceType
		int mSurfaceType = TerrainTileType.cUndefinedIndex;
		[Meta.TerrainTileTypeReference]
		public int SurfaceType
		{
			get { return this.mSurfaceType; }
			set { this.mSurfaceType = value; }
		}
		#endregion
		#region Visual
		string mVisual;
		[Meta.VisualReference]
		public string Visual
		{
			get { return this.mVisual; }
			set { this.mVisual = value; }
		}
		#endregion
		#region CorpseDeath
		string mCorpseDeath;
		[Meta.VisualReference]
		public string CorpseDeath
		{
			get { return this.mCorpseDeath; }
			set { this.mCorpseDeath = value; }
		}
		#endregion
		#region AbilityCommandID
		int mAbilityCommandID = TypeExtensions.kNone;
		[Meta.BAbilityReference]
		public int AbilityCommandID
		{
			get { return this.mAbilityCommandID; }
			set { this.mAbilityCommandID = value; }
		}
		#endregion
		#region PowerID
		int mPowerID = TypeExtensions.kNone;
		[Meta.BProtoPowerReference]
		public int PowerID
		{
			get { return this.mPowerID; }
			set { this.mPowerID = value; }
		}
		#endregion
		[Meta.TriggerScriptReference]
		public List<string> AbilityTriggerScripts { get; private set; }
			= new List<string>();
		public Collections.BListExplicitIndex<		BProtoObjectVeterancy> Veterancy { get; private set; }
			= new Collections.BListExplicitIndex<	BProtoObjectVeterancy>(BProtoObjectVeterancy.kBListExplicitIndexParams);
		public Collections.BTypeValuesSingle AddResource { get; private set; }
			= new Collections.BTypeValuesSingle(BResource.kBListTypeValuesParams);
		#region ExistSoundBoneName
		string mExistSoundBoneName;
		public string ExistSoundBoneName
		{
			get { return this.mExistSoundBoneName; }
			set { this.mExistSoundBoneName = value; }
		}
		#endregion
		#region GathererLimit
		int mGathererLimit = -1;
		public int GathererLimit
		{
			get { return this.mGathererLimit; }
			set { this.mGathererLimit = value; }
		}
		#endregion
		#region BlockMovementObjectID
		int mBlockMovementObjectID = TypeExtensions.kNone;
		[Meta.BProtoObjectReference]
		public int BlockMovementObjectID
		{
			get { return this.mBlockMovementObjectID; }
			set { this.mBlockMovementObjectID = value; }
		}
		#endregion
		#region Lifespan
		float mLifespan;
		public float Lifespan
		{
			get { return this.mLifespan; }
			set { this.mLifespan = value; }
		}
		#endregion
		#region AmmoMax
		float mAmmoMax;
		public float AmmoMax
		{
			get { return this.mAmmoMax; }
			set { this.mAmmoMax = value; }
		}
		#endregion
		#region AmmoRegenRate
		float mAmmoRegenRate;
		public float AmmoRegenRate
		{
			get { return this.mAmmoRegenRate; }
			set { this.mAmmoRegenRate = value; }
		}
		#endregion
		#region NumConversions
		int mNumConversions;
		public int NumConversions
		{
			get { return this.mNumConversions; }
			set { this.mNumConversions = value; }
		}
		#endregion
		#region NumStasisFieldsToStop
		int mNumStasisFieldsToStop = 1;
		public int NumStasisFieldsToStop
		{
			get { return this.mNumStasisFieldsToStop; }
			set { this.mNumStasisFieldsToStop = value; }
		}
		#endregion
		public Collections.BBitSet Flags { get; private set; }
			= new Collections.BBitSet(kFlagsParams);
		public Collections.BBitSet ObjectTypes { get; private set; }
			 = new Collections.BBitSet(kObjectTypesParams);
		public Collections.BListArray<		BProtoObjectDamageType> DamageTypes { get; private set; }
			= new Collections.BListArray<	BProtoObjectDamageType>();
		public Collections.BListArray<		BProtoObjectSound> Sounds { get; private set; }
			= new Collections.BListArray<	BProtoObjectSound>();
		public BTerrainImpactDecalHandle ImpactDecal { get; set; }
		#region ExtendedSoundBank
		string mExtendedSoundBank;
		public string ExtendedSoundBank
		{
			get { return this.mExtendedSoundBank; }
			set { this.mExtendedSoundBank = value; }
		}
		#endregion
		#region PortraitIcon
		string mPortraitIcon;
		public string PortraitIcon
		{
			get { return this.mPortraitIcon; }
			set { this.mPortraitIcon = value; }
		}
		#endregion
		#region Minimap

		string mMinimapIcon;
		public string MinimapIcon
		{
			get { return this.mMinimapIcon; }
			set { this.mMinimapIcon = value; }
		}

		float mMiniMapIconSize = 1.0f;
		public float MiniMapIconSize
		{
			get { return this.mMiniMapIconSize; }
			set { this.mMiniMapIconSize = value; }
		}

		static BVector cDefaultMinimapColor { get { return new BVector(1.0f, 1.0f, 1.0f, 0.0f); } }
		BVector mMinimapColor = cDefaultMinimapColor;
		public BVector MinimapColor
		{
			get { return this.mMinimapColor; }
			set { this.mMinimapColor = value; }
		}

		bool HasMiniMapIconData { get {
			return this.MinimapIcon.IsNotNullOrEmpty()
				||
				this.MiniMapIconSize != 1.0f;
		} }
		bool HasMinimapColorData { get { return this.MinimapColor != cDefaultMinimapColor; } }
		#endregion
		public Collections.BListArray<		BProtoObjectCommand> Commands { get; private set; }
			= new Collections.BListArray<	BProtoObjectCommand>();
		public Collections.BListArray<		BProtoObjectTrainLimit> TrainLimits { get; private set; }
			= new Collections.BListArray<	BProtoObjectTrainLimit>();
		#region GatherLink

		int mGatherLinkObjectType = TypeExtensions.kNone;
		[Meta.ObjectTypeReference]
		public int GatherLinkObjectType
		{
			get { return this.mGatherLinkObjectType; }
			set { this.mGatherLinkObjectType = value; }
		}

		float mGatherLinkRadius;
		public float GatherLinkRadius
		{
			get { return this.mGatherLinkRadius; }
			set { this.mGatherLinkRadius = value; }
		}

		int mGatherLinkTarget = TypeExtensions.kNone;
		[Meta.ObjectTypeReference]
		public int GatherLinkTarget
		{
			get { return this.mGatherLinkTarget; }
			set { this.mGatherLinkTarget = value; }
		}

		bool mGatherLinkSelf;
		public bool GatherLinkSelf
		{
			get { return this.mGatherLinkSelf; }
			set { this.mGatherLinkSelf = value; }
		}

		bool HasGatherLinkData { get {
			return this.GatherLinkObjectType.IsNotNone()
				||
				this.GatherLinkRadius > 0.0f
				||
				this.GatherLinkTarget.IsNotNone()
				||
				this.GatherLinkSelf;
		} }
		#endregion
		public Collections.BListArray<		BProtoObjectChildObject> ChildObjects { get; private set; }
			= new Collections.BListArray<	BProtoObjectChildObject>();
		public Collections.BTypeValuesSingle Populations { get; private set; }
			= new Collections.BTypeValuesSingle(BPopulation.kBListParamsSingle);
		public Collections.BTypeValuesSingle PopulationsCapAddition { get; private set; }
			= new Collections.BTypeValuesSingle(BPopulation.kBListParamsSingle);
		#region Tactics
		int mTactics = TypeExtensions.kNone;
		[Meta.BTacticDataReference]
		public int Tactics
		{
			get { return this.mTactics; }
			set { this.mTactics = value; }
		}
		#endregion
		#region FlightLevel
		const float cDefaultFlightLevel = 10.0f;

		float mFlightLevel = cDefaultFlightLevel;
		/// <summary>relative Y displacement of the object</summary>
		public float FlightLevel
		{
			get { return this.mFlightLevel; }
			set { this.mFlightLevel = value; }
		}
		#endregion
		#region ExitFromDirection
		int mExitFromDirection = (int)BProtoObjectExitDirection.FromFront;
		public int ExitFromDirection
		{
			get { return this.mExitFromDirection; }
			set { this.mExitFromDirection = value; }
		}
		#endregion
		#region HPBar

		// #TODO this needs to be an actual ID
		string mHPBarID;
		public string HPBarID
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
			return this.mHPBarID.IsNotNullOrEmpty()
				|| PhxPredicates.IsNotZero(this.HPBarSize)
				|| PhxPredicates.IsNotZero(this.HPBarOffset);
		} }
		#endregion
		public Collections.BListArray<		BHitZone> HitZones { get; private set; }
			= new Collections.BListArray<	BHitZone>();
		#region BeamHead
		int mBeamHead = TypeExtensions.kNone;
		[Meta.ObjectTypeReference]
		public int BeamHead
		{
			get { return this.mBeamHead; }
			set { this.mBeamHead = value; }
		}
		#endregion
		#region BeamTail
		int mBeamTail = TypeExtensions.kNone;
		[Meta.ObjectTypeReference]
		public int BeamTail
		{
			get { return this.mBeamTail; }
			set { this.mBeamTail = value; }
		}
		#endregion
		#region Level
		int mLevel;
		public int Level
		{
			get { return this.mLevel; }
			set { this.mLevel = value; }
		}
		#endregion
		#region LevelUpEffect
		int mLevelUpEffect = TypeExtensions.kNone;
		[Meta.BProtoObjectReference]
		public int LevelUpEffect
		{
			get { return this.mLevelUpEffect; }
			set { this.mLevelUpEffect = value; }
		}
		#endregion
		#region RecoveringEffect
		int mRecoveringEffect = TypeExtensions.kNone;
		[Meta.BProtoObjectReference]
		public int RecoveringEffect
		{
			get { return this.mRecoveringEffect; }
			set { this.mRecoveringEffect = value; }
		}
		#endregion
		#region AutoTrainOnBuilt
		int mAutoTrainOnBuilt = TypeExtensions.kNone;
		[Meta.BProtoSquadReference]
		public int AutoTrainOnBuilt
		{
			get { return this.mAutoTrainOnBuilt; }
			set { this.mAutoTrainOnBuilt = value; }
		}
		#endregion
		#region Socket

		int mSocketID = TypeExtensions.kNone;
		[Meta.ObjectTypeReference]
		public int SocketID
		{
			get { return this.mSocketID; }
			set { this.mSocketID = value; }
		}

		BPlayerScope mSocketPlayerScope = BPlayerScope.Player;
		public BPlayerScope SocketPlayerScope
		{
			get { return this.mSocketPlayerScope; }
			set { this.mSocketPlayerScope = value; }
		}

		bool mAutoSocket;
		public bool AutoSocket
		{
			get { return this.mAutoSocket; }
			set { this.mAutoSocket = value; }
		}

		bool HasSocketData { get {
			return this.SocketID.IsNotNone()
				||
				this.SocketPlayerScope != BPlayerScope.Player
				||
				this.AutoSocket;
		} }
		#endregion
		#region Rate

		int mRateID = TypeExtensions.kNone;
		[Meta.RateReference]
		public int RateID
		{
			get { return this.mRateID; }
			set { this.mRateID = value; }
		}

		float mRateAmount;
		public float RateAmount
		{
			get { return this.mRateAmount; }
			set { this.mRateAmount = value; }
		}

		bool HasRateData { get {
			return this.RateID.IsNotNone()
				||
				this.RateAmount > 0.0f;
		} }
		#endregion
		#region MaxContained
		int mMaxContained;
		public int MaxContained
		{
			get { return this.mMaxContained; }
			set { this.mMaxContained = value; }
		}
		#endregion
		#region MaxFlameEffects
		int mMaxFlameEffects = TypeExtensions.kNone;
		public int MaxFlameEffects
		{
			get { return this.mMaxFlameEffects; }
			set { this.mMaxFlameEffects = value; }
		}
		#endregion
		[Meta.ObjectTypeReference]
		public List<	BProtoObjectID> Contains { get; private set; }
			= new List<	BProtoObjectID>();
		#region GarrisonSquadMode
		BSquadMode mGarrisonSquadMode = BSquadMode.Invalid;
		public BSquadMode GarrisonSquadMode
		{
			get { return this.mGarrisonSquadMode; }
			set { this.mGarrisonSquadMode = value; }
		}
		#endregion
		#region BuildStatsObjectID
		int mBuildStatsObjectID = TypeExtensions.kNone;
		[Meta.BProtoObjectReference]
		public int BuildStatsObjectID
		{
			get { return this.mBuildStatsObjectID; }
			set { this.mBuildStatsObjectID = value; }
		}
		#endregion
		#region SubSelectSort
		int mSubSelectSort = int.MaxValue;
		public int SubSelectSort
		{
			get { return this.mSubSelectSort; }
			set { this.mSubSelectSort = value; }
		}
		#endregion
		#region AttackGradeDPS
		float mAttackGradeDPS;
		public float AttackGradeDPS
		{
			get { return this.mAttackGradeDPS; }
			set { this.mAttackGradeDPS = value; }
		}
		#endregion
		#region RamDodgeFactor
		float mRamDodgeFactor;
		public float RamDodgeFactor
		{
			get { return this.mRamDodgeFactor; }
			set { this.mRamDodgeFactor = value; }
		}
		#endregion
		public BRumbleEvent HoveringRumble { get; set; }
		#region VisualDisplayPriority
		BVisualDisplayPriority mVisualDisplayPriority = BVisualDisplayPriority.Normal;
		public BVisualDisplayPriority VisualDisplayPriority
		{
			get { return this.mVisualDisplayPriority; }
			set { this.mVisualDisplayPriority = value; }
		}
		#endregion
		#region ChildObjectDamageTakenScalar
		float mChildObjectDamageTakenScalar;
		public float ChildObjectDamageTakenScalar
		{
			get { return this.mChildObjectDamageTakenScalar; }
			set { this.mChildObjectDamageTakenScalar = value; }
		}
		#endregion
		#region TrueLOSHeight
		const float cDefaultTrueLOSHeight = 3.0f;

		float mTrueLOSHeight = cDefaultTrueLOSHeight;
		public float TrueLOSHeight
		{
			get { return this.mTrueLOSHeight; }
			set { this.mTrueLOSHeight = value; }
		}
		#endregion
		#region GarrisonTime
		float mGarrisonTime;
		public float GarrisonTime
		{
			get { return this.mGarrisonTime; }
			set { this.mGarrisonTime = value; }
		}
		#endregion
		#region BuildRotation
		float mBuildRotation;
		public float BuildRotation
		{
			get { return this.mBuildRotation; }
			set { this.mBuildRotation = value; }
		}
		#endregion
		#region BuildOffset
		BVector mBuildOffset;
		public BVector BuildOffset
		{
			get { return this.mBuildOffset; }
			set { this.mBuildOffset = value; }
		}
		#endregion
		#region AutoParkingLot

		int mAutoParkingLotObjectID = TypeExtensions.kNone;
		[Meta.BProtoObjectReference]
		public int AutoParkingLotObjectID
		{
			get { return this.mAutoParkingLotObjectID; }
			set { this.mAutoParkingLotObjectID = value; }
		}

		float mAutoParkingLotRotation;
		public float AutoParkingLotRotation
		{
			get { return this.mAutoParkingLotRotation; }
			set { this.mAutoParkingLotRotation = value; }
		}

		BVector mAutoParkingLotOffset;
		public BVector AutoParkingLotOffset
		{
			get { return this.mAutoParkingLotOffset; }
			set { this.mAutoParkingLotOffset = value; }
		}

		bool HasAutoParkingLotData { get {
			return this.AutoParkingLotObjectID.IsNotNone()
				||
				this.AutoParkingLotRotation != 0.0
				|| PhxPredicates.IsNotZero(this.AutoParkingLotOffset);
		} }
		#endregion
		#region BuildingStrengthID
		// #TODO this needs to be an actual ID

		string mBuildingStrengthID;
		public string BuildingStrengthID
		{
			get { return this.mBuildingStrengthID; }
			set { this.mBuildingStrengthID = value; }
		}
		#endregion
		#region ShieldType
		int mShieldType = TypeExtensions.kNone;
		[Meta.ObjectTypeReference]
		public int ShieldType
		{
			get { return this.mShieldType; }
			set { this.mShieldType = value; }
		}
		#endregion
		#region RevealRadius
		float mRevealRadius;
		public float RevealRadius
		{
			get { return this.mRevealRadius; }
			set { this.mRevealRadius = value; }
		}
		#endregion
		#region TargetBeam
		int mTargetBeam = TypeExtensions.kNone;
		[Meta.BProtoObjectReference]
		public int TargetBeam
		{
			get { return this.mTargetBeam; }
			set { this.mTargetBeam = value; }
		}
		#endregion
		#region KillBeam
		int mKillBeam = TypeExtensions.kNone;
		[Meta.BProtoObjectReference]
		public int KillBeam
		{
			get { return this.mKillBeam; }
			set { this.mKillBeam = value; }
		}
		#endregion
		#region MinimapIconName (EDITOR ONLY)
		string mMinimapIconName;
		public string MinimapIconName
		{
			get { return this.mMinimapIconName; }
			set { this.mMinimapIconName = value; }
		}
		#endregion

		public BProtoObject() : base(BResource.kBListTypeValuesParams, BResource.kBListTypeValuesXmlParams_Cost)
		{
			var textData = this.CreateDatabaseObjectUserInterfaceTextData();
			textData.HasDisplayNameID = true;
			textData.HasRolloverTextID = true;
			textData.HasStatsNameID = true;
			textData.HasGaiaRolloverTextID = true;
			textData.HasEnemyRolloverTextID = true;
			textData.HasPrereqTextID = true;
			textData.HasRoleTextID = true;
		}

		#region ITagElementStreamable<string> Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);
			var xs = s.GetSerializerInterface();

			s.StreamAttributeOpt("is", ref this.mUnusedIs, Predicates.IsNotNone);
			s.StreamAttributeOpt("id", ref this.mUnusedId, Predicates.IsNotNone);
			s.StreamAttributeOpt("update", ref this.mUpdate, Predicates.IsTrue);
			s.StreamElementEnumOpt("MovementType", ref this.mMovementType, e => e != BProtoObjectMovementType.None);
			XML.XmlUtil.Serialize(s, this.Hardpoints, BHardpoint.kBListXmlParams);
			s.StreamElements("SingleBoneIK", this.SingleBoneIKs, xs, XML.BXmlSerializerInterface.StreamStringValue, dummy => (string)null);
			XML.XmlUtil.Serialize(s, this.GroundIKs, BGroundIKNode.kBListXmlParams);
			XML.XmlUtil.Serialize(s, this.SweetSpotIKs, BSweetSpotIKNode.kBListXmlParams);
			#region ObstructionRadius
			s.StreamElementOpt("ObstructionRadiusX", ref this.mObstructionRadius.X, Predicates.IsNotZero);
			s.StreamElementOpt("ObstructionRadiusY", ref this.mObstructionRadius.Y, Predicates.IsNotZero);
			s.StreamElementOpt("ObstructionRadiusZ", ref this.mObstructionRadius.Z, Predicates.IsNotZero);
			#endregion
			#region TerrainFlatten
			s.StreamElementOpt("FlattenMinX0", ref this.mTerrainFlattennMin0.X, Predicates.IsNotZero);
			s.StreamElementOpt("FlattenMaxX0", ref this.mTerrainFlattennMax0.X, Predicates.IsNotZero);
			s.StreamElementOpt("FlattenMinZ0", ref this.mTerrainFlattennMin0.Z, Predicates.IsNotZero);
			s.StreamElementOpt("FlattenMaxZ0", ref this.mTerrainFlattennMax0.Z, Predicates.IsNotZero);

			s.StreamElementOpt("FlattenMinX1", ref this.mTerrainFlattennMin1.X, Predicates.IsNotZero);
			s.StreamElementOpt("FlattenMaxX1", ref this.mTerrainFlattennMax1.X, Predicates.IsNotZero);
			s.StreamElementOpt("FlattenMinZ1", ref this.mTerrainFlattennMin1.Z, Predicates.IsNotZero);
			s.StreamElementOpt("FlattenMaxZ1", ref this.mTerrainFlattennMax1.Z, Predicates.IsNotZero);
			#endregion
			#region Parking lot
			s.StreamElementOpt("ParkingMinX", ref this.mParkingLotMin.X, Predicates.IsNotZero);
			s.StreamElementOpt("ParkingMaxX", ref this.mParkingLotMax.X, Predicates.IsNotZero);
			s.StreamElementOpt("ParkingMinZ", ref this.mParkingLotMin.Z, Predicates.IsNotZero);
			s.StreamElementOpt("ParkingMaxZ", ref this.mParkingLotMax.Z, Predicates.IsNotZero);
			#endregion
			s.StreamElementOpt("TerrainHeightTolerance", ref this.mTerrainHeightTolerance, f => f != cDefaultTerrainHeightTolerance);
			#region Physics
			s.StreamElementOpt("PhysicsInfo", ref this.mPhysicsInfo, Predicates.IsNotNullOrEmpty);
			s.StreamElementOpt("PhysicsReplacementInfo", ref this.mPhysicsReplacementInfo, Predicates.IsNotNullOrEmpty);
			s.StreamElementOpt("Velocity", ref this.mVelocity, Predicates.IsNotZero);
			s.StreamElementOpt("MaxVelocity", ref this.mMaxVelocity, Predicates.IsNotZero);
			s.StreamElementOpt(kXmlElementReverseSpeed, ref this.mReverseSpeed, f => f != cDefaultReverseSpeed);
			s.StreamElementOpt("Acceleration", ref this.mAcceleration, Predicates.IsNotZero);
			s.StreamElementOpt("TrackingDelay", ref this.mTrackingDelay, Predicates.IsNotZero);
			s.StreamElementOpt("StartingVelocity", ref this.mStartingVelocity, Predicates.IsNotZero);
			#endregion
			s.StreamElementOpt("Fuel", ref this.mFuel, Predicates.IsNotZero);
			#region Perturbance
			s.StreamElementOpt("PerturbanceChance", ref this.mPerturbanceChance, Predicates.IsNotZero);
			s.StreamElementOpt("PerturbanceVelocity", ref this.mPerturbanceVelocity, Predicates.IsNotZero);
			s.StreamElementOpt("PerturbanceMinTime", ref this.mPerturbanceMinTime, Predicates.IsNotZero);
			s.StreamElementOpt("PerturbanceMaxTime", ref this.mPerturbanceMaxTime, Predicates.IsNotZero);
			using (var bm = s.EnterCursorBookmarkOpt("PerturbInitialVelocity", this, v => v.HasInitialPerturbanceData)) if (bm.IsNotNull)
			{
				s.StreamCursor(ref this.mPerturbInitialVelocity);
				s.StreamAttributeOpt("minTime", ref this.mInitialPerturbanceMinTime, Predicates.IsNotZero);
				s.StreamAttributeOpt("maxTime", ref this.mInitialPerturbanceMaxTime, Predicates.IsNotZero);
			}
			#endregion
			#region ActiveScan
			using (var bm = s.EnterCursorBookmarkOpt("ActiveScanChance", this, v => v.HasActiveScanData)) if (bm.IsNotNull)
			{
				s.StreamCursor(ref this.mActiveScanChance);
				s.StreamAttributeOpt("radiusScale", ref this.mActiveScanRadiusScale, Predicates.IsNotZero);
			}
			#endregion
			s.StreamElementOpt("TurnRate", ref this.mTurnRate, Predicates.IsNotZero);
			s.StreamElementOpt("Hitpoints", ref this.mHitpoints, Predicates.IsNotZero);
			#region Shieldpoints
			{
				bool streamedShieldpoints = s.StreamElementOpt("Shieldpoints", ref this.mShieldpoints, Predicates.IsNotZero);
				// #HACK fucking deal with original HW game data that was hand edited, but only when reading
				if (s.IsReading && !streamedShieldpoints)
					s.StreamElementOpt("ShieldPoints", ref this.mShieldpoints, Predicates.IsNotZero);
			}
			#endregion
			s.StreamElementOpt("LOS", ref this.mLOS, Predicates.IsNotZero);
			#region Pick and Select
			s.StreamElementOpt("PickRadius", ref this.mPickRadius, Predicates.IsNotZero);
			s.StreamElementOpt("PickOffset", ref this.mPickOffset, Predicates.IsNotZero);
			s.StreamElementEnumOpt("PickPriority", ref this.mPickPriority, e => e != BPickPriority.None);
			s.StreamElementEnumOpt("SelectType", ref this.mSelectType, e => e != BProtoObjectSelectType.None);
			s.StreamElementEnumOpt("GotoType", ref this.mGotoType, e => e != BGotoType.None);
			s.StreamElementOpt("SelectedRadiusX", ref this.mSelectedRadius.X, Predicates.IsNotZero);
			s.StreamElementOpt("SelectedRadiusZ", ref this.mSelectedRadius.Z, Predicates.IsNotZero);
			#endregion
			s.StreamElementOpt("RepairPoints", ref this.mRepairPoints, Predicates.IsNotZero);
			s.StreamElementEnumOpt("ObjectClass", ref this.mClassType, x => x != BProtoObjectClassType.Object);
			#region TrainerType
			using (var bm = s.EnterCursorBookmarkOpt("TrainerType", this, v => v.HasTrainerTypeData)) if (bm.IsNotNull)
			{
				s.StreamCursor(ref this.mTrainerType);
				s.StreamAttributeOpt("ApplyFormation", ref this.mTrainerApplyFormation, Predicates.IsTrue);
			}
			#endregion
			s.StreamElementEnumOpt("AutoLockDown", ref this.mAutoLockDown, e => e != BAutoLockDown.None);
			s.StreamElementOpt("CostEscalation", ref this.mCostEscalation, PhxPredicates.IsNotOne);
			s.StreamElements("CostEscalationObject", this.CostEscalationObjects, xs, XML.BXmlSerializerInterface.StreamObjectID);
			XML.XmlUtil.Serialize(s, this.CaptureCosts, BProtoObjectCaptureCost.kBListXmlParams);
			s.StreamElementOpt("Bounty", ref this.mBounty, Predicates.IsNotZero);
			s.StreamElementOpt("AIAssetValueAdjust", ref this.mAIAssetValueAdjust, Predicates.IsNotZero);
			s.StreamElementOpt("CombatValue", ref this.mCombatValue, Predicates.IsNotZero);
			s.StreamElementOpt("ResourceAmount", ref this.mResourceAmount, Predicates.IsNotZero);
			s.StreamElementOpt("PlacementRules", ref this.mPlacementRules, Predicates.IsNotNullOrEmpty);
			s.StreamElementOpt("DeathFadeTime", ref this.mDeathFadeTime, PhxPredicates.IsNotOne);
			s.StreamElementOpt("DeathFadeDelayTime", ref this.mDeathFadeDelayTime, Predicates.IsNotZero);
			s.StreamElementOpt("TrainAnim", ref this.mTrainAnim, Predicates.IsNotNullOrEmpty);
			XML.XmlUtil.Serialize(s, this.SquadModeAnims, BProtoObjectSquadModeAnim.kBListXmlParams);
			s.StreamElementEnumOpt("RallyPoint", ref this.mRallyPoint, x => x != BRallyPointType.Invalid);
			s.StreamElementOpt("MaxProjectileHeight", ref this.mMaxProjectileHeight, Predicates.IsNotZero);
			#region GroundIKTilt
			using (var bm = s.EnterCursorBookmarkOpt("GroundIKTilt", this, v => v.HasGroundIKTiltData)) if (bm.IsNotNull)
			{
				s.StreamCursor(ref this.mGroundIKTiltBoneName);
				s.StreamAttributeOpt("factor", ref this.mGroundIKTiltFactor, Predicates.IsNotZero);
			}
			#endregion
			xs.StreamDBID(s, "DeathReplacement", ref this.mDeathReplacementID, DatabaseObjectKind.Object);
			#region DeathSpawnSquad
			using (var bm = s.EnterCursorBookmarkOpt("DeathSpawnSquad", this, v => v.HasDeathSpawnSquadData)) if (bm.IsNotNull)
			{
				xs.StreamDBID(s, XML.XmlUtil.kNoXmlName, ref this.mDeathSpawnSquadID, DatabaseObjectKind.Squad, xmlSource: XML.XmlUtil.kSourceCursor);

				// #NOTE engine streams this as CheckPos, but it is also case insensitive
				const string kCheckPosName = "checkPos";
				// #NOTE the engine interprets the presence of this attribute as true
				if (s.IsReading)
				{
					this.mDeathSpawnSquadCheckPosition = s.AttributeExists(kCheckPosName);
				}
				else if (s.IsWriting)
				{
					if (this.mDeathSpawnSquadCheckPosition)
						s.WriteAttribute(kCheckPosName, true);
				}

				s.StreamAttributeOpt("MaxPopCount", ref this.mDeathSpawnSquadMaxPopCount, Predicates.IsNotZero);
			}
			#endregion
			xs.StreamDBID(s, "SurfaceType", ref this.mSurfaceType, DatabaseObjectKind.TerrainTileType);
			s.StreamElementOpt("Visual", ref this.mVisual, Predicates.IsNotNullOrEmpty);
			s.StreamElementOpt("CorpseDeath", ref this.mCorpseDeath, Predicates.IsNotNullOrEmpty);
			xs.StreamDBID(s, "AbilityCommand", ref this.mAbilityCommandID, DatabaseObjectKind.Ability);
			xs.StreamDBID(s, "Power", ref this.mPowerID, DatabaseObjectKind.Power);
			s.StreamElements("Ability", this.AbilityTriggerScripts, xs, XML.BXmlSerializerInterface.StreamStringValue, dummy => (string)null);
			XML.XmlUtil.Serialize(s, this.Veterancy, BProtoObjectVeterancy.kBListExplicitIndexXmlParams);
			XML.XmlUtil.Serialize(s, this.AddResource, BResource.kBListTypeValuesXmlParams_AddResource, "Amount");
			#region ExistSound
			using (var bm = s.EnterCursorBookmarkOpt("ExistSound", this.mExistSoundBoneName, Predicates.IsNotNullOrEmpty)) if (bm.IsNotNull)
			{
				s.StreamAttribute("bone", ref this.mExistSoundBoneName);
			}
			#endregion
			s.StreamElementOpt("GathererLimit", ref this.mGathererLimit, Predicates.IsNotNone);
			xs.StreamDBID(s, "BlockMovementObject", ref this.mBlockMovementObjectID, DatabaseObjectKind.Object);
			s.StreamElementOpt("Lifespan", ref this.mLifespan, Predicates.IsNotZero);
			s.StreamElementOpt("AmmoMax", ref this.mAmmoMax, Predicates.IsNotZero);
			s.StreamElementOpt("AmmoRegenRate", ref this.mAmmoRegenRate, Predicates.IsNotZero);
			s.StreamElementOpt("NumConversions", ref this.mNumConversions, Predicates.IsNotZero);
			s.StreamElementOpt("NumStasisFieldsToStop", ref this.mNumStasisFieldsToStop, PhxPredicates.IsNotOne);
			XML.XmlUtil.Serialize(s, this.Flags, XML.BBitSetXmlParams.kFlagsSansRoot);
			XML.XmlUtil.Serialize(s, this.ObjectTypes, kObjectTypesXmlParams);
			XML.XmlUtil.Serialize(s, this.DamageTypes, BProtoObjectDamageType.kBListXmlParams);
			XML.XmlUtil.Serialize(s, this.Sounds, BProtoObjectSound.kBListXmlParams);
			#region ImpactDecal
			using (var bm = s.EnterCursorBookmarkOpt(BTerrainImpactDecalHandle.kBListXmlParams.ElementName, this.ImpactDecal, Predicates.IsNotNull)) if (bm.IsNotNull)
			{
				if (s.IsReading)
					this.ImpactDecal = new BTerrainImpactDecalHandle();

				this.ImpactDecal.Serialize(s);
			}
			#endregion
			s.StreamElementOpt("ExtendedSoundBank", ref this.mExtendedSoundBank, Predicates.IsNotNullOrEmpty);
			s.StreamElementOpt("PortraitIcon", ref this.mPortraitIcon, Predicates.IsNotNullOrEmpty);
			#region Minimap
			using (var bm = s.EnterCursorBookmarkOpt("MinimapIcon", this, v => v.HasMiniMapIconData)) if (bm.IsNotNull)
			{
				s.StreamCursor(ref this.mMinimapIcon);
				s.StreamAttributeOpt("size", ref this.mMiniMapIconSize, PhxPredicates.IsNotOne);
			}
			using (var bm = s.EnterCursorBookmarkOpt("MinimapColor", this, v => v.HasMinimapColorData)) if (bm.IsNotNull)
			{
				// #NOTE we use IsNotZero here instead of IsNotOne (for cDefaultMinimapColor)
				// because when loading the game defaults the temp rgb values to 0 and then sets
				// the final game data to those values (so excluding red would mean it is zero).
				// #NOTE the engine parses these names in lowercase, but actual data uses uppercase
				s.StreamAttributeOpt("Red", ref this.mMinimapColor.X, Predicates.IsNotZero);
				s.StreamAttributeOpt("Green", ref this.mMinimapColor.Y, Predicates.IsNotZero);
				s.StreamAttributeOpt("Blue", ref this.mMinimapColor.Z, Predicates.IsNotZero);
			}
			#endregion
			XML.XmlUtil.Serialize(s, this.Commands, BProtoObjectCommand.kBListXmlParams);
			XML.XmlUtil.Serialize(s, this.TrainLimits, BProtoObjectTrainLimit.kBListXmlParams);
			#region GatherLink
			using (var bm = s.EnterCursorBookmarkOpt("GatherLink", this, v => v.HasGatherLinkData)) if (bm.IsNotNull)
			{
				xs.StreamDBID(s, XML.XmlUtil.kNoXmlName, ref this.mGatherLinkObjectType, DatabaseObjectKind.ObjectType, isOptional: false, xmlSource: XML.XmlUtil.kSourceCursor);
				s.StreamAttributeOpt("Radius", ref this.mGatherLinkRadius, Predicates.IsNotZero);
				xs.StreamDBID(s, "Target", ref this.mGatherLinkObjectType, DatabaseObjectKind.ObjectType, isOptional: false, xmlSource: XML.XmlUtil.kSourceAttr);
				s.StreamAttributeOpt("Self", ref this.mGatherLinkSelf, Predicates.IsTrue);
			}
			#endregion
			XML.XmlUtil.Serialize(s, this.ChildObjects, BProtoObjectChildObject.kBListXmlParams);
			XML.XmlUtil.Serialize(s, this.Populations, BPopulation.kBListXmlParamsSingle);
			XML.XmlUtil.Serialize(s, this.PopulationsCapAddition, BPopulation.kBListXmlParamsSingle_CapAddition);
			xs.StreamTactic(s, "Tactics", ref this.mTactics);
			s.StreamElementOpt("FlightLevel", ref this.mFlightLevel, f => f != cDefaultFlightLevel);
			s.StreamElementOpt("ExitFromDirection", ref this.mExitFromDirection, Predicates.IsNotZero);
			#region HPBar
			using (var bm = s.EnterCursorBookmarkOpt("HPBar", this, v => v.HasHPBarData)) if (bm.IsNotNull)
			{
				s.StreamCursor(ref this.mHPBarID);
				s.StreamAttributeOpt("sizeX", ref this.mHPBarSize.X, Predicates.IsNotZero);
				s.StreamAttributeOpt("sizeY", ref this.mHPBarSize.Y, Predicates.IsNotZero);
				s.StreamBVector("offset", ref this.mHPBarOffset, xmlSource: XML.XmlUtil.kSourceAttr);
			}
			#endregion
			XML.XmlUtil.Serialize(s, this.HitZones, BHitZone.kBListXmlParams);
			xs.StreamDBID(s, "BeamHead", ref this.mBeamHead, DatabaseObjectKind.Unit);
			xs.StreamDBID(s, "BeamTail", ref this.mBeamTail, DatabaseObjectKind.Unit);
			s.StreamElementOpt("Level", ref this.mLevel, Predicates.IsNotZero);
			xs.StreamDBID(s, "LevelUpEffect", ref this.mLevelUpEffect, DatabaseObjectKind.Object);
			xs.StreamDBID(s, "RecoveringEffect", ref this.mRecoveringEffect, DatabaseObjectKind.Object);
			xs.StreamDBID(s, "AutoTrainOnBuilt", ref this.mAutoTrainOnBuilt, DatabaseObjectKind.Squad);
			#region Socket
			using (var bm = s.EnterCursorBookmarkOpt("Socket", this, v => v.HasSocketData)) if (bm.IsNotNull)
			{
				xs.StreamDBID(s, XML.XmlUtil.kNoXmlName, ref this.mSocketID, DatabaseObjectKind.ObjectType, isOptional: false, xmlSource: XML.XmlUtil.kSourceCursor);
				// #NOTE engine reads this Player in lower case, but actual uses pascal case
				s.StreamAttributeEnumOpt("Player", ref this.mSocketPlayerScope, e => e != BPlayerScope.Player);
				s.StreamAttributeOpt("AutoSocket", ref this.mAutoSocket, Predicates.IsTrue);
			}
			#endregion
			#region Rate
			using (var bm = s.EnterCursorBookmarkOpt(BGameData.kRatesBListTypeValuesXmlParams.ElementName, this, v => v.HasRateData)) if (bm.IsNotNull)
			{
				// #NOTE engine reads Rate as lower case, but actual data is in pascal case
				xs.StreamTypeName(s, BGameData.kRatesBListTypeValuesXmlParams.DataName, ref this.mRateID, GameDataObjectKind.Rate, isOptional: false, xmlSource: XML.XmlUtil.kSourceAttr);
				s.StreamCursor(ref this.mRateAmount);
			}
			#endregion
			s.StreamElementOpt("MaxContained", ref this.mMaxContained, Predicates.IsNotZero);
			s.StreamElementOpt("MaxFlameEffects", ref this.mMaxFlameEffects, Predicates.IsNotNone);
			s.StreamElements("Contain", this.Contains, xs, XML.BXmlSerializerInterface.StreamObjectType);
			s.StreamElementEnumOpt("GarrisonSquadMode", ref this.mGarrisonSquadMode, e => e != BSquadMode.Invalid);
			xs.StreamDBID(s, "BuildStatsObject", ref this.mBuildStatsObjectID, DatabaseObjectKind.Object);
			s.StreamElementOpt("SubSelectSort", ref this.mSubSelectSort, v => v != int.MaxValue);
			s.StreamElementOpt(kXmlElementAttackGradeDPS, ref this.mAttackGradeDPS, Predicates.IsNotZero);
			s.StreamElementOpt("RamDodgeFactor", ref this.mRamDodgeFactor, Predicates.IsNotZero);
			#region HoveringRumble
			using (var bm = s.EnterCursorBookmarkOpt("HoveringRumble", this.HoveringRumble, Predicates.IsNotNull)) if (bm.IsNotNull)
			{
				if (s.IsReading)
					this.HoveringRumble = new BRumbleEvent();

				this.HoveringRumble.Serialize(s);
			}
			#endregion
			s.StreamElementEnumOpt("VisualDisplayPriority", ref this.mVisualDisplayPriority, e => e != BVisualDisplayPriority.Normal);
			s.StreamElementOpt("ChildObjectDamageTakenScalar", ref this.mChildObjectDamageTakenScalar, Predicates.IsNotZero);
			s.StreamElementOpt("TrueLOSHeight", ref this.mTrueLOSHeight, f => f != cDefaultTrueLOSHeight);
			s.StreamElementOpt("GarrisonTime", ref this.mGarrisonTime, Predicates.IsNotZero);
			s.StreamElementOpt("BuildRotation", ref this.mBuildRotation, Predicates.IsNotZero);
			s.StreamBVector("BuildOffset", ref this.mBuildOffset);
			#region AutoParkingLot
			using (var bm = s.EnterCursorBookmarkOpt("AutoParkingLot", this, v => v.HasAutoParkingLotData)) if (bm.IsNotNull)
			{
				xs.StreamDBID(s, XML.XmlUtil.kNoXmlName, ref this.mAutoParkingLotObjectID, DatabaseObjectKind.Object, isOptional: false, xmlSource: XML.XmlUtil.kSourceCursor);
				s.StreamAttributeOpt("Rotation", ref this.mAutoParkingLotRotation, Predicates.IsNotZero);
				s.StreamBVector("Offset", ref this.mAutoParkingLotOffset, xmlSource: XML.XmlUtil.kSourceAttr);
			}
			#endregion
			s.StreamElementOpt("BuildingStrengthDisplay", ref this.mBuildingStrengthID, Predicates.IsNotNullOrEmpty);
			xs.StreamDBID(s, "ShieldType", ref this.mShieldType, DatabaseObjectKind.Unit);
			s.StreamElementOpt("RevealRadius", ref this.mRevealRadius, Predicates.IsNotZero);
			xs.StreamDBID(s, "TargetBeam", ref this.mTargetBeam, DatabaseObjectKind.Object);
			xs.StreamDBID(s, "KillBeam", ref this.mKillBeam, DatabaseObjectKind.Object);
			s.StreamElementOpt("MinimapIconName", ref this.mMinimapIconName, Predicates.IsNotNullOrEmpty);

			if (s.IsReading)
			{
				this.PostDeserialize();
			}
		}

		private void PostDeserialize()
		{
			if (SortCommandsAfterReading)
			{
				this.SortCommands();
			}

			this.SquadModeAnims.Sort();
		}
		#endregion

		public static bool SortCommandsAfterReading = false;
		private void SortCommands()
		{
			this.Commands.Sort(CompareCommands);
		}

		private static int CompareCommands(BProtoObjectCommand x, BProtoObjectCommand y)
		{
			if (x.Position != y.Position)
				return x.Position.CompareTo(y.Position);

			if (x.CommandType != y.CommandType)
				return ((int)x.CommandType).CompareTo((int)y.CommandType);

			// assuming Proto upgrades are defined after earlier Protos
			return x.ID.CompareTo(y.ID);
		}
	};
}
