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
			DataName = DatabaseNamedObject.kXmlAttrName,
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
		public int UnusedIs { get { return mUnusedIs; } }
		int mUnusedId = TypeExtensions.kNone;
		public int UnusedId { get { return mUnusedId; } }
		#endregion
		bool mUpdate;
		public bool Update { get { return mUpdate; } }

		#region MovementType
		BProtoObjectMovementType mMovementType = BProtoObjectMovementType.None;
		public BProtoObjectMovementType MovementType
		{
			get { return mMovementType; }
			set { mMovementType = value; }
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
			get { return mObstructionRadius; }
			set { mObstructionRadius = value; }
		}
		#endregion

		#region TerrainFlatten
		// Automatic terrain flattening for building placement

		BVector mTerrainFlattennMin0;
		public BVector TerrainFlattenMin0
		{
			get { return mTerrainFlattennMin0; }
			set { mTerrainFlattennMin0 = value; }
		}
		BVector mTerrainFlattennMax0;
		public BVector TerrainFlattenMax0
		{
			get { return mTerrainFlattennMax0; }
			set { mTerrainFlattennMax0 = value; }
		}

		BVector mTerrainFlattennMin1;
		public BVector TerrainFlattenMin1
		{
			get { return mTerrainFlattennMin1; }
			set { mTerrainFlattennMin1 = value; }
		}
		BVector mTerrainFlattennMax1;
		public BVector TerrainFlattenMax1
		{
			get { return mTerrainFlattennMax1; }
			set { mTerrainFlattennMax1 = value; }
		}
		#endregion
		#region Parking lot
		// Parking lot position for building placement

		BVector mParkingLotMin;
		public BVector ParkingLotMin
		{
			get { return mParkingLotMin; }
			set { mParkingLotMin = value; }
		}
		BVector mParkingLotMax;
		public BVector ParkingLotMax
		{
			get { return mParkingLotMax; }
			set { mParkingLotMax = value; }
		}
		#endregion
		#region TerrainHeightTolerance
		const float cDefaultTerrainHeightTolerance = 10.0f;

		float mTerrainHeightTolerance = cDefaultTerrainHeightTolerance;
		public float TerrainHeightTolerance
		{
			get { return mTerrainHeightTolerance; }
			set { mTerrainHeightTolerance = value; }
		}
		#endregion
		#region Physics
		string mPhysicsInfo;
		[Meta.PhysicsInfoReference]
		public string PhysicsInfo
		{
			get { return mPhysicsInfo; }
			set { mPhysicsInfo = value; }
		}

		string mPhysicsReplacementInfo;
		[Meta.PhysicsInfoReference]
		public string PhysicsReplacementInfo
		{
			get { return mPhysicsReplacementInfo; }
			set { mPhysicsReplacementInfo = value; }
		}

		float mVelocity;
		public float Velocity
		{
			get { return mVelocity; }
			set { mVelocity = value; }
		}

		float mMaxVelocity;
		public float MaxVelocity
		{
			get { return mMaxVelocity; }
			set { mMaxVelocity = value; }
		}

		const float cDefaultReverseSpeed = 1.0f;
		float mReverseSpeed = cDefaultReverseSpeed;
		public float ReverseSpeed
		{
			get { return mReverseSpeed; }
			set { mReverseSpeed = value; }
		}

		float mAcceleration;
		public float Acceleration
		{
			get { return mAcceleration; }
			set { mAcceleration = value; }
		}

		float mTrackingDelay;
		// in seconds
		public float TrackingDelay
		{
			get { return mTrackingDelay; }
			set { mTrackingDelay = value; }
		}

		float mStartingVelocity;
		public float StartingVelocity
		{
			get { return mStartingVelocity; }
			set { mStartingVelocity = value; }
		}
		#endregion
		#region Fuel
		float mFuel;
		public float Fuel
		{
			get { return mFuel; }
			set { mFuel = value; }
		}
		#endregion
		#region Perturbance

		float mPerturbanceChance;
		public float PerturbanceChance
		{
			get { return mPerturbanceChance; }
			set { mPerturbanceChance = value; }
		}

		float mPerturbanceVelocity;
		public float PerturbanceVelocity
		{
			get { return mPerturbanceVelocity; }
			set { mPerturbanceVelocity = value; }
		}

		float mPerturbanceMinTime;
		public float PerturbanceMinTime
		{
			get { return mPerturbanceMinTime; }
			set { mPerturbanceMinTime = value; }
		}

		float mPerturbanceMaxTime;
		public float PerturbanceMaxTime
		{
			get { return mPerturbanceMaxTime; }
			set { mPerturbanceMaxTime = value; }
		}

		float mPerturbInitialVelocity;
		public float PerturbInitialVelocity
		{
			get { return mPerturbInitialVelocity; }
			set { mPerturbInitialVelocity = value; }
		}

		float mInitialPerturbanceMinTime;
		public float InitialPerturbanceMinTime
		{
			get { return mInitialPerturbanceMinTime; }
			set { mInitialPerturbanceMinTime = value; }
		}

		float mInitialPerturbanceMaxTime;
		public float InitialPerturbanceMaxTime
		{
			get { return mInitialPerturbanceMaxTime; }
			set { mInitialPerturbanceMaxTime = value; }
		}

		bool HasInitialPerturbanceData { get {
			return PerturbInitialVelocity != 0.0
				|| InitialPerturbanceMinTime > 0.0
				|| InitialPerturbanceMaxTime > 0.0;
		} }
		#endregion
		#region ActiveScan
		float mActiveScanChance;
		public float ActiveScanChance
		{
			get { return mActiveScanChance; }
			set { mActiveScanChance = value; }
		}

		float mActiveScanRadiusScale;
		public float ActiveScanRadiusScale
		{
			get { return mActiveScanRadiusScale; }
			set { mActiveScanRadiusScale = value; }
		}

		bool HasActiveScanData { get {
			return ActiveScanChance > 0.0
				|| ActiveScanRadiusScale > 0.0;
		} }
		#endregion
		#region TurnRate
		float mTurnRate;
		public float TurnRate
		{
			get { return mTurnRate; }
			set { mTurnRate = value; }
		}
		#endregion
		#region Hitpoints
		float mHitpoints;
		public float Hitpoints
		{
			get { return mHitpoints; }
			set { mHitpoints = value; }
		}
		#endregion
		#region Shieldpoints
		float mShieldpoints;
		public float Shieldpoints
		{
			get { return mShieldpoints; }
			set { mShieldpoints = value; }
		}
		#endregion
		#region LOS
		float mLOS;
		public float LOS
		{
			get { return mLOS; }
			set { mLOS = value; }
		}
		#endregion
		#region Pick and Select

		float mPickRadius;
		public float PickRadius
		{
			get { return mPickRadius; }
			set { mPickRadius = value; }
		}

		float mPickOffset;
		public float PickOffset
		{
			get { return mPickOffset; }
			set { mPickOffset = value; }
		}

		BPickPriority mPickPriority;
		public BPickPriority PickPriority
		{
			get { return mPickPriority; }
			set { mPickPriority = value; }
		}

		BProtoObjectSelectType mSelectType;
		public BProtoObjectSelectType SelectType
		{
			get { return mSelectType; }
			set { mSelectType = value; }
		}

		BGotoType mGotoType;
		public BGotoType GotoType
		{
			get { return mGotoType; }
			set { mGotoType = value; }
		}

		BVector mSelectedRadius;
		public BVector SelectedRadius
		{
			get { return mSelectedRadius; }
			set { mSelectedRadius = value; }
		}

		#endregion
		#region RepairPoints
		float mRepairPoints;
		[Meta.UnusedData]
		public float RepairPoints
		{
			get { return mRepairPoints; }
			set { mRepairPoints = value; }
		}
		#endregion
		#region ClassType
		BProtoObjectClassType mClassType = BProtoObjectClassType.Object;
		public BProtoObjectClassType ClassType
		{
			get { return mClassType; }
			set { mClassType = value; }
		}
		#endregion
		#region TrainerType
		int mTrainerType = TypeExtensions.kNone;
		public int TrainerType
		{
			get { return mTrainerType; }
			set { mTrainerType = value; }
		}

		bool mTrainerApplyFormation;
		public bool TrainerApplyFormation
		{
			get { return mTrainerApplyFormation; }
			set { TrainerApplyFormation = value; }
		}

		bool HasTrainerTypeData { get {
			return TrainerType.IsNotNone()
				|| TrainerApplyFormation;
		} }
		#endregion
		#region AutoLockDown
		BAutoLockDown mAutoLockDown;
		public BAutoLockDown AutoLockDown
		{
			get { return mAutoLockDown; }
			set { mAutoLockDown = value; }
		}
		#endregion
		#region CostEscalation
		float mCostEscalation = 1.0f;
		/// <summary>see: UNSC reactors</summary>
		// Also, CostEscalationObject and Flag.LinearCostEscalation
		public float CostEscalation
		{
			get { return mCostEscalation; }
			set { mCostEscalation = value; }
		}
		public bool HasCostEscalation { get { return CostEscalation > 0.0f; } }
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
			get { return mBounty; }
			set { mBounty = value; }
		}
		#endregion
		#region AIAssetValueAdjust
		float mAIAssetValueAdjust;
		public float AIAssetValueAdjust
		{
			get { return mAIAssetValueAdjust; }
			set { mAIAssetValueAdjust = value; }
		}
		#endregion
		#region CombatValue
		float mCombatValue;
		/// <summary>Score value</summary>
		public float CombatValue
		{
			get { return mCombatValue; }
			set { mCombatValue = value; }
		}
		#endregion
		#region ResourceAmount
		float mResourceAmount;
		public float ResourceAmount
		{
			get { return mResourceAmount; }
			set { mResourceAmount = value; }
		}
		#endregion
		#region PlacementRules
		string mPlacementRules;
		/// <summary>PlacementRules file name (sans extension)</summary>
		public string PlacementRules
		{
			get { return mPlacementRules; }
			set { mPlacementRules = value; }
		}
		#endregion
		#region DeathFadeTime
		float mDeathFadeTime = 1.0f;
		public float DeathFadeTime
		{
			get { return mDeathFadeTime; }
			set { mDeathFadeTime = value; }
		}
		#endregion
		#region DeathFadeDelayTime
		float mDeathFadeDelayTime;
		public float DeathFadeDelayTime
		{
			get { return mDeathFadeDelayTime; }
			set { mDeathFadeDelayTime = value; }
		}
		#endregion
		#region TrainAnim
		string mTrainAnim;
		[Meta.BAnimTypeReference]
		public string TrainAnim
		{
			get { return mTrainAnim; }
			set { mTrainAnim = value; }
		}
		#endregion
		/// <remarks>Engine actually uses a fixed array that maps a BSquadMode to an AnimType</remarks>
		public Collections.BListArray<		BProtoObjectSquadModeAnim> SquadModeAnims { get; private set; }
			= new Collections.BListArray<	BProtoObjectSquadModeAnim>();
		#region RallyPoint
		BRallyPointType mRallyPoint = BRallyPointType.Invalid;
		public BRallyPointType RallyPoint
		{
			get { return mRallyPoint; }
			set { mRallyPoint = value; }
		}
		#endregion
		#region MaxProjectileHeight
		float mMaxProjectileHeight;
		public float MaxProjectileHeight
		{
			get { return mMaxProjectileHeight; }
			set { mMaxProjectileHeight = value; }
		}
		#endregion
		#region GroundIKTilt

		float mGroundIKTiltFactor;
		public float GroundIKTiltFactor
		{
			get { return mGroundIKTiltFactor; }
			set { mGroundIKTiltFactor = value; }
		}

		string mGroundIKTiltBoneName;
		public string GroundIKTiltBoneName
		{
			get { return mGroundIKTiltBoneName; }
			set { mGroundIKTiltBoneName = value; }
		}

		bool HasGroundIKTiltData { get {
			return GroundIKTiltFactor > 0.0
				|| GroundIKTiltBoneName.IsNotNullOrEmpty();
		} }
		#endregion
		#region DeathReplacement
		int mDeathReplacementID = TypeExtensions.kNone;
		[Meta.BProtoObjectReference]
		public int DeathReplacementID
		{
			get { return mDeathReplacementID; }
			set { mDeathReplacementID = value; }
		}
		#endregion
		#region DeathSpawnSquad

		int mDeathSpawnSquadID = TypeExtensions.kNone;
		[Meta.BProtoSquadReference]
		public int DeathSpawnSquadID
		{
			get { return mDeathSpawnSquadID; }
			set { mDeathSpawnSquadID = value; }
		}

		bool mDeathSpawnSquadCheckPosition;
		public bool DeathSpawnSquadCheckPosition
		{
			get { return mDeathSpawnSquadCheckPosition; }
			set { mDeathSpawnSquadCheckPosition = value; }
		}

		int mDeathSpawnSquadMaxPopCount;
		public int DeathSpawnSquadMaxPopCount
		{
			get { return mDeathSpawnSquadMaxPopCount; }
			set { mDeathSpawnSquadMaxPopCount = value; }
		}

		bool HasDeathSpawnSquadData { get {
			return mDeathSpawnSquadID.IsNotNone()
				|| mDeathSpawnSquadCheckPosition
				|| mDeathSpawnSquadMaxPopCount > 0;
		} }
		#endregion
		#region SurfaceType
		int mSurfaceType = TerrainTileType.cUndefinedIndex;
		[Meta.TerrainTileTypeReference]
		public int SurfaceType
		{
			get { return mSurfaceType; }
			set { mSurfaceType = value; }
		}
		#endregion
		#region Visual
		string mVisual;
		[Meta.VisualReference]
		public string Visual
		{
			get { return mVisual; }
			set { mVisual = value; }
		}
		#endregion
		#region CorpseDeath
		string mCorpseDeath;
		[Meta.VisualReference]
		public string CorpseDeath
		{
			get { return mCorpseDeath; }
			set { mCorpseDeath = value; }
		}
		#endregion
		#region AbilityCommandID
		int mAbilityCommandID = TypeExtensions.kNone;
		[Meta.BAbilityReference]
		public int AbilityCommandID
		{
			get { return mAbilityCommandID; }
			set { mAbilityCommandID = value; }
		}
		#endregion
		#region PowerID
		int mPowerID = TypeExtensions.kNone;
		[Meta.BProtoPowerReference]
		public int PowerID
		{
			get { return mPowerID; }
			set { mPowerID = value; }
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
			get { return mExistSoundBoneName; }
			set { mExistSoundBoneName = value; }
		}
		#endregion
		#region GathererLimit
		int mGathererLimit = -1;
		public int GathererLimit
		{
			get { return mGathererLimit; }
			set { mGathererLimit = value; }
		}
		#endregion
		#region BlockMovementObjectID
		int mBlockMovementObjectID = TypeExtensions.kNone;
		[Meta.BProtoObjectReference]
		public int BlockMovementObjectID
		{
			get { return mBlockMovementObjectID; }
			set { mBlockMovementObjectID = value; }
		}
		#endregion
		#region Lifespan
		float mLifespan;
		public float Lifespan
		{
			get { return mLifespan; }
			set { mLifespan = value; }
		}
		#endregion
		#region AmmoMax
		float mAmmoMax;
		public float AmmoMax
		{
			get { return mAmmoMax; }
			set { mAmmoMax = value; }
		}
		#endregion
		#region AmmoRegenRate
		float mAmmoRegenRate;
		public float AmmoRegenRate
		{
			get { return mAmmoRegenRate; }
			set { mAmmoRegenRate = value; }
		}
		#endregion
		#region NumConversions
		int mNumConversions;
		public int NumConversions
		{
			get { return mNumConversions; }
			set { mNumConversions = value; }
		}
		#endregion
		#region NumStasisFieldsToStop
		int mNumStasisFieldsToStop = 1;
		public int NumStasisFieldsToStop
		{
			get { return mNumStasisFieldsToStop; }
			set { mNumStasisFieldsToStop = value; }
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
			get { return mExtendedSoundBank; }
			set { mExtendedSoundBank = value; }
		}
		#endregion
		#region PortraitIcon
		string mPortraitIcon;
		public string PortraitIcon
		{
			get { return mPortraitIcon; }
			set { mPortraitIcon = value; }
		}
		#endregion
		#region Minimap

		string mMinimapIcon;
		public string MinimapIcon
		{
			get { return mMinimapIcon; }
			set { mMinimapIcon = value; }
		}

		float mMiniMapIconSize = 1.0f;
		public float MiniMapIconSize
		{
			get { return mMiniMapIconSize; }
			set { mMiniMapIconSize = value; }
		}

		static BVector cDefaultMinimapColor { get { return new BVector(1.0f, 1.0f, 1.0f, 0.0f); } }
		BVector mMinimapColor = cDefaultMinimapColor;
		public BVector MinimapColor
		{
			get { return mMinimapColor; }
			set { mMinimapColor = value; }
		}

		bool HasMiniMapIconData { get {
			return MinimapIcon.IsNotNullOrEmpty()
				|| MiniMapIconSize != 1.0f;
		} }
		bool HasMinimapColorData { get { return MinimapColor != cDefaultMinimapColor; } }
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
			get { return mGatherLinkObjectType; }
			set { mGatherLinkObjectType = value; }
		}

		float mGatherLinkRadius;
		public float GatherLinkRadius
		{
			get { return mGatherLinkRadius; }
			set { mGatherLinkRadius = value; }
		}

		int mGatherLinkTarget = TypeExtensions.kNone;
		[Meta.ObjectTypeReference]
		public int GatherLinkTarget
		{
			get { return mGatherLinkTarget; }
			set { mGatherLinkTarget = value; }
		}

		bool mGatherLinkSelf;
		public bool GatherLinkSelf
		{
			get { return mGatherLinkSelf; }
			set { mGatherLinkSelf = value; }
		}

		bool HasGatherLinkData { get {
			return GatherLinkObjectType.IsNotNone()
				|| GatherLinkRadius > 0.0f
				|| GatherLinkTarget.IsNotNone()
				|| GatherLinkSelf;
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
			get { return mTactics; }
			set { mTactics = value; }
		}
		#endregion
		#region FlightLevel
		const float cDefaultFlightLevel = 10.0f;

		float mFlightLevel = cDefaultFlightLevel;
		/// <summary>relative Y displacement of the object</summary>
		public float FlightLevel
		{
			get { return mFlightLevel; }
			set { mFlightLevel = value; }
		}
		#endregion
		#region ExitFromDirection
		int mExitFromDirection = (int)BProtoObjectExitDirection.FromFront;
		public int ExitFromDirection
		{
			get { return mExitFromDirection; }
			set { mExitFromDirection = value; }
		}
		#endregion
		#region HPBar

		// #TODO this needs to be an actual ID
		string mHPBarID;
		public string HPBarID
		{
			get { return mHPBarID; }
			set { mHPBarID = value; }
		}

		BVector mHPBarSize;
		public BVector HPBarSize
		{
			get { return mHPBarSize; }
			set { mHPBarSize = value; }
		}

		BVector mHPBarOffset;
		public BVector HPBarOffset
		{
			get { return mHPBarOffset; }
			set { mHPBarOffset = value; }
		}

		bool HasHPBarData { get {
			return mHPBarID.IsNotNullOrEmpty()
				|| PhxPredicates.IsNotZero(HPBarSize)
				|| PhxPredicates.IsNotZero(HPBarOffset);
		} }
		#endregion
		public Collections.BListArray<		BHitZone> HitZones { get; private set; }
			= new Collections.BListArray<	BHitZone>();
		#region BeamHead
		int mBeamHead = TypeExtensions.kNone;
		[Meta.ObjectTypeReference]
		public int BeamHead
		{
			get { return mBeamHead; }
			set { mBeamHead = value; }
		}
		#endregion
		#region BeamTail
		int mBeamTail = TypeExtensions.kNone;
		[Meta.ObjectTypeReference]
		public int BeamTail
		{
			get { return mBeamTail; }
			set { mBeamTail = value; }
		}
		#endregion
		#region Level
		int mLevel;
		public int Level
		{
			get { return mLevel; }
			set { mLevel = value; }
		}
		#endregion
		#region LevelUpEffect
		int mLevelUpEffect = TypeExtensions.kNone;
		[Meta.BProtoObjectReference]
		public int LevelUpEffect
		{
			get { return mLevelUpEffect; }
			set { mLevelUpEffect = value; }
		}
		#endregion
		#region RecoveringEffect
		int mRecoveringEffect = TypeExtensions.kNone;
		[Meta.BProtoObjectReference]
		public int RecoveringEffect
		{
			get { return mRecoveringEffect; }
			set { mRecoveringEffect = value; }
		}
		#endregion
		#region AutoTrainOnBuilt
		int mAutoTrainOnBuilt = TypeExtensions.kNone;
		[Meta.BProtoSquadReference]
		public int AutoTrainOnBuilt
		{
			get { return mAutoTrainOnBuilt; }
			set { mAutoTrainOnBuilt = value; }
		}
		#endregion
		#region Socket

		int mSocketID = TypeExtensions.kNone;
		[Meta.ObjectTypeReference]
		public int SocketID
		{
			get { return mSocketID; }
			set { mSocketID = value; }
		}

		BPlayerScope mSocketPlayerScope = BPlayerScope.Player;
		public BPlayerScope SocketPlayerScope
		{
			get { return mSocketPlayerScope; }
			set { mSocketPlayerScope = value; }
		}

		bool mAutoSocket;
		public bool AutoSocket
		{
			get { return mAutoSocket; }
			set { mAutoSocket = value; }
		}

		bool HasSocketData { get {
			return SocketID.IsNotNone()
				|| SocketPlayerScope != BPlayerScope.Player
				|| AutoSocket;
		} }
		#endregion
		#region Rate

		int mRateID = TypeExtensions.kNone;
		[Meta.RateReference]
		public int RateID
		{
			get { return mRateID; }
			set { mRateID = value; }
		}

		float mRateAmount;
		public float RateAmount
		{
			get { return mRateAmount; }
			set { mRateAmount = value; }
		}

		bool HasRateData { get {
			return RateID.IsNotNone()
				|| RateAmount > 0.0f;
		} }
		#endregion
		#region MaxContained
		int mMaxContained;
		public int MaxContained
		{
			get { return mMaxContained; }
			set { mMaxContained = value; }
		}
		#endregion
		#region MaxFlameEffects
		int mMaxFlameEffects = TypeExtensions.kNone;
		public int MaxFlameEffects
		{
			get { return mMaxFlameEffects; }
			set { mMaxFlameEffects = value; }
		}
		#endregion
		[Meta.ObjectTypeReference]
		public List<	BProtoObjectID> Contains { get; private set; }
			= new List<	BProtoObjectID>();
		#region GarrisonSquadMode
		BSquadMode mGarrisonSquadMode = BSquadMode.Invalid;
		public BSquadMode GarrisonSquadMode
		{
			get { return mGarrisonSquadMode; }
			set { mGarrisonSquadMode = value; }
		}
		#endregion
		#region BuildStatsObjectID
		int mBuildStatsObjectID = TypeExtensions.kNone;
		[Meta.BProtoObjectReference]
		public int BuildStatsObjectID
		{
			get { return mBuildStatsObjectID; }
			set { mBuildStatsObjectID = value; }
		}
		#endregion
		#region SubSelectSort
		int mSubSelectSort = int.MaxValue;
		public int SubSelectSort
		{
			get { return mSubSelectSort; }
			set { mSubSelectSort = value; }
		}
		#endregion
		#region AttackGradeDPS
		float mAttackGradeDPS;
		public float AttackGradeDPS
		{
			get { return mAttackGradeDPS; }
			set { mAttackGradeDPS = value; }
		}
		#endregion
		#region RamDodgeFactor
		float mRamDodgeFactor;
		public float RamDodgeFactor
		{
			get { return mRamDodgeFactor; }
			set { mRamDodgeFactor = value; }
		}
		#endregion
		public BRumbleEvent HoveringRumble { get; set; }
		#region VisualDisplayPriority
		BVisualDisplayPriority mVisualDisplayPriority = BVisualDisplayPriority.Normal;
		public BVisualDisplayPriority VisualDisplayPriority
		{
			get { return mVisualDisplayPriority; }
			set { mVisualDisplayPriority = value; }
		}
		#endregion
		#region ChildObjectDamageTakenScalar
		float mChildObjectDamageTakenScalar;
		public float ChildObjectDamageTakenScalar
		{
			get { return mChildObjectDamageTakenScalar; }
			set { mChildObjectDamageTakenScalar = value; }
		}
		#endregion
		#region TrueLOSHeight
		const float cDefaultTrueLOSHeight = 3.0f;

		float mTrueLOSHeight = cDefaultTrueLOSHeight;
		public float TrueLOSHeight
		{
			get { return mTrueLOSHeight; }
			set { mTrueLOSHeight = value; }
		}
		#endregion
		#region GarrisonTime
		float mGarrisonTime;
		public float GarrisonTime
		{
			get { return mGarrisonTime; }
			set { mGarrisonTime = value; }
		}
		#endregion
		#region BuildRotation
		float mBuildRotation;
		public float BuildRotation
		{
			get { return mBuildRotation; }
			set { mBuildRotation = value; }
		}
		#endregion
		#region BuildOffset
		BVector mBuildOffset;
		public BVector BuildOffset
		{
			get { return mBuildOffset; }
			set { mBuildOffset = value; }
		}
		#endregion
		#region AutoParkingLot

		int mAutoParkingLotObjectID = TypeExtensions.kNone;
		[Meta.BProtoObjectReference]
		public int AutoParkingLotObjectID
		{
			get { return mAutoParkingLotObjectID; }
			set { mAutoParkingLotObjectID = value; }
		}

		float mAutoParkingLotRotation;
		public float AutoParkingLotRotation
		{
			get { return mAutoParkingLotRotation; }
			set { mAutoParkingLotRotation = value; }
		}

		BVector mAutoParkingLotOffset;
		public BVector AutoParkingLotOffset
		{
			get { return mAutoParkingLotOffset; }
			set { mAutoParkingLotOffset = value; }
		}

		bool HasAutoParkingLotData { get {
			return AutoParkingLotObjectID.IsNotNone()
				|| AutoParkingLotRotation != 0.0
				|| PhxPredicates.IsNotZero(AutoParkingLotOffset);
		} }
		#endregion
		#region BuildingStrengthID
		// #TODO this needs to be an actual ID

		string mBuildingStrengthID;
		public string BuildingStrengthID
		{
			get { return mBuildingStrengthID; }
			set { mBuildingStrengthID = value; }
		}
		#endregion
		#region ShieldType
		int mShieldType = TypeExtensions.kNone;
		[Meta.ObjectTypeReference]
		public int ShieldType
		{
			get { return mShieldType; }
			set { mShieldType = value; }
		}
		#endregion
		#region RevealRadius
		float mRevealRadius;
		public float RevealRadius
		{
			get { return mRevealRadius; }
			set { mRevealRadius = value; }
		}
		#endregion
		#region TargetBeam
		int mTargetBeam = TypeExtensions.kNone;
		[Meta.BProtoObjectReference]
		public int TargetBeam
		{
			get { return mTargetBeam; }
			set { mTargetBeam = value; }
		}
		#endregion
		#region KillBeam
		int mKillBeam = TypeExtensions.kNone;
		[Meta.BProtoObjectReference]
		public int KillBeam
		{
			get { return mKillBeam; }
			set { mKillBeam = value; }
		}
		#endregion
		#region MinimapIconName (EDITOR ONLY)
		string mMinimapIconName;
		public string MinimapIconName
		{
			get { return mMinimapIconName; }
			set { mMinimapIconName = value; }
		}
		#endregion

		public BProtoObject() : base(BResource.kBListTypeValuesParams, BResource.kBListTypeValuesXmlParams_Cost)
		{
			var textData = base.CreateDatabaseObjectUserInterfaceTextData();
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

			s.StreamAttributeOpt("is", ref mUnusedIs, Predicates.IsNotNone);
			s.StreamAttributeOpt("id", ref mUnusedId, Predicates.IsNotNone);
			s.StreamAttributeOpt("update", ref mUpdate, Predicates.IsTrue);
			s.StreamElementEnumOpt("MovementType", ref mMovementType, e => e != BProtoObjectMovementType.None);
			XML.XmlUtil.Serialize(s, Hardpoints, BHardpoint.kBListXmlParams);
			s.StreamElements("SingleBoneIK", SingleBoneIKs, xs, XML.BDatabaseXmlSerializerBase.StreamStringValue, dummy => (string)null);
			XML.XmlUtil.Serialize(s, GroundIKs, BGroundIKNode.kBListXmlParams);
			XML.XmlUtil.Serialize(s, SweetSpotIKs, BSweetSpotIKNode.kBListXmlParams);
			#region ObstructionRadius
			s.StreamElementOpt("ObstructionRadiusX", ref mObstructionRadius.X, Predicates.IsNotZero);
			s.StreamElementOpt("ObstructionRadiusY", ref mObstructionRadius.Y, Predicates.IsNotZero);
			s.StreamElementOpt("ObstructionRadiusZ", ref mObstructionRadius.Z, Predicates.IsNotZero);
			#endregion
			#region TerrainFlatten
			s.StreamElementOpt("FlattenMinX0", ref mTerrainFlattennMin0.X, Predicates.IsNotZero);
			s.StreamElementOpt("FlattenMaxX0", ref mTerrainFlattennMax0.X, Predicates.IsNotZero);
			s.StreamElementOpt("FlattenMinZ0", ref mTerrainFlattennMin0.Z, Predicates.IsNotZero);
			s.StreamElementOpt("FlattenMaxZ0", ref mTerrainFlattennMax0.Z, Predicates.IsNotZero);

			s.StreamElementOpt("FlattenMinX1", ref mTerrainFlattennMin1.X, Predicates.IsNotZero);
			s.StreamElementOpt("FlattenMaxX1", ref mTerrainFlattennMax1.X, Predicates.IsNotZero);
			s.StreamElementOpt("FlattenMinZ1", ref mTerrainFlattennMin1.Z, Predicates.IsNotZero);
			s.StreamElementOpt("FlattenMaxZ1", ref mTerrainFlattennMax1.Z, Predicates.IsNotZero);
			#endregion
			#region Parking lot
			s.StreamElementOpt("ParkingMinX", ref mParkingLotMin.X, Predicates.IsNotZero);
			s.StreamElementOpt("ParkingMaxX", ref mParkingLotMax.X, Predicates.IsNotZero);
			s.StreamElementOpt("ParkingMinZ", ref mParkingLotMin.Z, Predicates.IsNotZero);
			s.StreamElementOpt("ParkingMaxZ", ref mParkingLotMax.Z, Predicates.IsNotZero);
			#endregion
			s.StreamElementOpt("TerrainHeightTolerance", ref mTerrainHeightTolerance, f => f != cDefaultTerrainHeightTolerance);
			#region Physics
			s.StreamElementOpt("PhysicsInfo", ref mPhysicsInfo, Predicates.IsNotNullOrEmpty);
			s.StreamElementOpt("PhysicsReplacementInfo", ref mPhysicsReplacementInfo, Predicates.IsNotNullOrEmpty);
			s.StreamElementOpt("Velocity", ref mVelocity, Predicates.IsNotZero);
			s.StreamElementOpt("MaxVelocity", ref mMaxVelocity, Predicates.IsNotZero);
			s.StreamElementOpt(kXmlElementReverseSpeed, ref mReverseSpeed, f => f != cDefaultReverseSpeed);
			s.StreamElementOpt("Acceleration", ref mAcceleration, Predicates.IsNotZero);
			s.StreamElementOpt("TrackingDelay", ref mTrackingDelay, Predicates.IsNotZero);
			s.StreamElementOpt("StartingVelocity", ref mStartingVelocity, Predicates.IsNotZero);
			#endregion
			s.StreamElementOpt("Fuel", ref mFuel, Predicates.IsNotZero);
			#region Perturbance
			s.StreamElementOpt("PerturbanceChance", ref mPerturbanceChance, Predicates.IsNotZero);
			s.StreamElementOpt("PerturbanceVelocity", ref mPerturbanceVelocity, Predicates.IsNotZero);
			s.StreamElementOpt("PerturbanceMinTime", ref mPerturbanceMinTime, Predicates.IsNotZero);
			s.StreamElementOpt("PerturbanceMaxTime", ref mPerturbanceMaxTime, Predicates.IsNotZero);
			using (var bm = s.EnterCursorBookmarkOpt("PerturbInitialVelocity", this, v => v.HasInitialPerturbanceData)) if (bm.IsNotNull)
			{
				s.StreamCursor(ref mPerturbInitialVelocity);
				s.StreamAttributeOpt("minTime", ref mInitialPerturbanceMinTime, Predicates.IsNotZero);
				s.StreamAttributeOpt("maxTime", ref mInitialPerturbanceMaxTime, Predicates.IsNotZero);
			}
			#endregion
			#region ActiveScan
			using (var bm = s.EnterCursorBookmarkOpt("ActiveScanChance", this, v => v.HasActiveScanData)) if (bm.IsNotNull)
			{
				s.StreamCursor(ref mActiveScanChance);
				s.StreamAttributeOpt("radiusScale", ref mActiveScanRadiusScale, Predicates.IsNotZero);
			}
			#endregion
			s.StreamElementOpt("TurnRate", ref mTurnRate, Predicates.IsNotZero);
			s.StreamElementOpt("Hitpoints", ref mHitpoints, Predicates.IsNotZero);
			#region Shieldpoints
			{
				bool streamedShieldpoints = s.StreamElementOpt("Shieldpoints", ref mShieldpoints, Predicates.IsNotZero);
				// #HACK fucking deal with original HW game data that was hand edited, but only when reading
				if (s.IsReading && !streamedShieldpoints)
					s.StreamElementOpt("ShieldPoints", ref mShieldpoints, Predicates.IsNotZero);
			}
			#endregion
			s.StreamElementOpt("LOS", ref mLOS, Predicates.IsNotZero);
			#region Pick and Select
			s.StreamElementOpt("PickRadius", ref mPickRadius, Predicates.IsNotZero);
			s.StreamElementOpt("PickOffset", ref mPickOffset, Predicates.IsNotZero);
			s.StreamElementEnumOpt("PickPriority", ref mPickPriority, e => e != BPickPriority.None);
			s.StreamElementEnumOpt("SelectType", ref mSelectType, e => e != BProtoObjectSelectType.None);
			s.StreamElementEnumOpt("GotoType", ref mGotoType, e => e != BGotoType.None);
			s.StreamElementOpt("SelectedRadiusX", ref mSelectedRadius.X, Predicates.IsNotZero);
			s.StreamElementOpt("SelectedRadiusZ", ref mSelectedRadius.Z, Predicates.IsNotZero);
			#endregion
			s.StreamElementOpt("RepairPoints", ref mRepairPoints, Predicates.IsNotZero);
			s.StreamElementEnumOpt("ObjectClass", ref mClassType, x => x != BProtoObjectClassType.Object);
			#region TrainerType
			using (var bm = s.EnterCursorBookmarkOpt("TrainerType", this, v => v.HasTrainerTypeData)) if (bm.IsNotNull)
			{
				s.StreamCursor(ref mTrainerType);
				s.StreamAttributeOpt("ApplyFormation", ref mTrainerApplyFormation, Predicates.IsTrue);
			}
			#endregion
			s.StreamElementEnumOpt("AutoLockDown", ref mAutoLockDown, e => e != BAutoLockDown.None);
			s.StreamElementOpt("CostEscalation", ref mCostEscalation, PhxPredicates.IsNotOne);
			s.StreamElements("CostEscalationObject", CostEscalationObjects, xs, XML.BDatabaseXmlSerializerBase.StreamObjectID);
			XML.XmlUtil.Serialize(s, CaptureCosts, BProtoObjectCaptureCost.kBListXmlParams);
			s.StreamElementOpt("Bounty", ref mBounty, Predicates.IsNotZero);
			s.StreamElementOpt("AIAssetValueAdjust", ref mAIAssetValueAdjust, Predicates.IsNotZero);
			s.StreamElementOpt("CombatValue", ref mCombatValue, Predicates.IsNotZero);
			s.StreamElementOpt("ResourceAmount", ref mResourceAmount, Predicates.IsNotZero);
			s.StreamElementOpt("PlacementRules", ref mPlacementRules, Predicates.IsNotNullOrEmpty);
			s.StreamElementOpt("DeathFadeTime", ref mDeathFadeTime, PhxPredicates.IsNotOne);
			s.StreamElementOpt("DeathFadeDelayTime", ref mDeathFadeDelayTime, Predicates.IsNotZero);
			s.StreamElementOpt("TrainAnim", ref mTrainAnim, Predicates.IsNotNullOrEmpty);
			XML.XmlUtil.Serialize(s, SquadModeAnims, BProtoObjectSquadModeAnim.kBListXmlParams);
			s.StreamElementEnumOpt("RallyPoint", ref mRallyPoint, x => x != BRallyPointType.Invalid);
			s.StreamElementOpt("MaxProjectileHeight", ref mMaxProjectileHeight, Predicates.IsNotZero);
			#region GroundIKTilt
			using (var bm = s.EnterCursorBookmarkOpt("GroundIKTilt", this, v => v.HasGroundIKTiltData)) if (bm.IsNotNull)
			{
				s.StreamCursor(ref mGroundIKTiltBoneName);
				s.StreamAttributeOpt("factor", ref mGroundIKTiltFactor, Predicates.IsNotZero);
			}
			#endregion
			xs.StreamDBID(s, "DeathReplacement", ref mDeathReplacementID, DatabaseObjectKind.Object);
			#region DeathSpawnSquad
			using (var bm = s.EnterCursorBookmarkOpt("DeathSpawnSquad", this, v => v.HasDeathSpawnSquadData)) if (bm.IsNotNull)
			{
				xs.StreamDBID(s, XML.XmlUtil.kNoXmlName, ref mDeathSpawnSquadID, DatabaseObjectKind.Squad, xmlSource: XML.XmlUtil.kSourceCursor);

				// #NOTE engine streams this as CheckPos, but it is also case insensitive
				const string kCheckPosName = "checkPos";
				// #NOTE the engine interprets the presence of this attribute as true
				if (s.IsReading)
				{
					mDeathSpawnSquadCheckPosition = s.AttributeExists(kCheckPosName);
				}
				else if (s.IsWriting)
				{
					if (mDeathSpawnSquadCheckPosition)
						s.WriteAttribute(kCheckPosName, true);
				}

				s.StreamAttributeOpt("MaxPopCount", ref mDeathSpawnSquadMaxPopCount, Predicates.IsNotZero);
			}
			#endregion
			xs.StreamDBID(s, "SurfaceType", ref mSurfaceType, DatabaseObjectKind.TerrainTileType);
			s.StreamElementOpt("Visual", ref mVisual, Predicates.IsNotNullOrEmpty);
			s.StreamElementOpt("CorpseDeath", ref mCorpseDeath, Predicates.IsNotNullOrEmpty);
			xs.StreamDBID(s, "AbilityCommand", ref mAbilityCommandID, DatabaseObjectKind.Ability);
			xs.StreamDBID(s, "Power", ref mPowerID, DatabaseObjectKind.Power);
			s.StreamElements("Ability", AbilityTriggerScripts, xs, XML.BDatabaseXmlSerializerBase.StreamStringValue, dummy => (string)null);
			XML.XmlUtil.Serialize(s, Veterancy, BProtoObjectVeterancy.kBListExplicitIndexXmlParams);
			XML.XmlUtil.Serialize(s, AddResource, BResource.kBListTypeValuesXmlParams_AddResource, "Amount");
			#region ExistSound
			using (var bm = s.EnterCursorBookmarkOpt("ExistSound", mExistSoundBoneName, Predicates.IsNotNullOrEmpty)) if (bm.IsNotNull)
			{
				s.StreamAttribute("bone", ref mExistSoundBoneName);
			}
			#endregion
			s.StreamElementOpt("GathererLimit", ref mGathererLimit, Predicates.IsNotNone);
			xs.StreamDBID(s, "BlockMovementObject", ref mBlockMovementObjectID, DatabaseObjectKind.Object);
			s.StreamElementOpt("Lifespan", ref mLifespan, Predicates.IsNotZero);
			s.StreamElementOpt("AmmoMax", ref mAmmoMax, Predicates.IsNotZero);
			s.StreamElementOpt("AmmoRegenRate", ref mAmmoRegenRate, Predicates.IsNotZero);
			s.StreamElementOpt("NumConversions", ref mNumConversions, Predicates.IsNotZero);
			s.StreamElementOpt("NumStasisFieldsToStop", ref mNumStasisFieldsToStop, PhxPredicates.IsNotOne);
			XML.XmlUtil.Serialize(s, Flags, XML.BBitSetXmlParams.kFlagsSansRoot);
			XML.XmlUtil.Serialize(s, ObjectTypes, kObjectTypesXmlParams);
			XML.XmlUtil.Serialize(s, DamageTypes, BProtoObjectDamageType.kBListXmlParams);
			XML.XmlUtil.Serialize(s, Sounds, BProtoObjectSound.kBListXmlParams);
			#region ImpactDecal
			using (var bm = s.EnterCursorBookmarkOpt(BTerrainImpactDecalHandle.kBListXmlParams.ElementName, ImpactDecal, Predicates.IsNotNull)) if (bm.IsNotNull)
			{
				if (s.IsReading)
					ImpactDecal = new BTerrainImpactDecalHandle();

				ImpactDecal.Serialize(s);
			}
			#endregion
			s.StreamElementOpt("ExtendedSoundBank", ref mExtendedSoundBank, Predicates.IsNotNullOrEmpty);
			s.StreamElementOpt("PortraitIcon", ref mPortraitIcon, Predicates.IsNotNullOrEmpty);
			#region Minimap
			using (var bm = s.EnterCursorBookmarkOpt("MinimapIcon", this, v => v.HasMiniMapIconData)) if (bm.IsNotNull)
			{
				s.StreamCursor(ref mMinimapIcon);
				s.StreamAttributeOpt("size", ref mMiniMapIconSize, PhxPredicates.IsNotOne);
			}
			using (var bm = s.EnterCursorBookmarkOpt("MinimapColor", this, v => v.HasMinimapColorData)) if (bm.IsNotNull)
			{
				// #NOTE we use IsNotZero here instead of IsNotOne (for cDefaultMinimapColor)
				// because when loading the game defaults the temp rgb values to 0 and then sets
				// the final game data to those values (so excluding red would mean it is zero).
				// #NOTE the engine parses these names in lowercase, but actual data uses uppercase
				s.StreamAttributeOpt("Red", ref mMinimapColor.X, Predicates.IsNotZero);
				s.StreamAttributeOpt("Green", ref mMinimapColor.Y, Predicates.IsNotZero);
				s.StreamAttributeOpt("Blue", ref mMinimapColor.Z, Predicates.IsNotZero);
			}
			#endregion
			XML.XmlUtil.Serialize(s, Commands, BProtoObjectCommand.kBListXmlParams);
			XML.XmlUtil.Serialize(s, TrainLimits, BProtoObjectTrainLimit.kBListXmlParams);
			#region GatherLink
			using (var bm = s.EnterCursorBookmarkOpt("GatherLink", this, v => v.HasGatherLinkData)) if (bm.IsNotNull)
			{
				xs.StreamDBID(s, XML.XmlUtil.kNoXmlName, ref mGatherLinkObjectType, DatabaseObjectKind.ObjectType, isOptional: false, xmlSource: XML.XmlUtil.kSourceCursor);
				s.StreamAttributeOpt("Radius", ref mGatherLinkRadius, Predicates.IsNotZero);
				xs.StreamDBID(s, "Target", ref mGatherLinkObjectType, DatabaseObjectKind.ObjectType, isOptional: false, xmlSource: XML.XmlUtil.kSourceAttr);
				s.StreamAttributeOpt("Self", ref mGatherLinkSelf, Predicates.IsTrue);
			}
			#endregion
			XML.XmlUtil.Serialize(s, ChildObjects, BProtoObjectChildObject.kBListXmlParams);
			XML.XmlUtil.Serialize(s, Populations, BPopulation.kBListXmlParamsSingle);
			XML.XmlUtil.Serialize(s, PopulationsCapAddition, BPopulation.kBListXmlParamsSingle_CapAddition);
			xs.StreamTactic(s, "Tactics", ref mTactics);
			s.StreamElementOpt("FlightLevel", ref mFlightLevel, f => f != cDefaultFlightLevel);
			s.StreamElementOpt("ExitFromDirection", ref mExitFromDirection, Predicates.IsNotZero);
			#region HPBar
			using (var bm = s.EnterCursorBookmarkOpt("HPBar", this, v => v.HasHPBarData)) if (bm.IsNotNull)
			{
				s.StreamCursor(ref mHPBarID);
				s.StreamAttributeOpt("sizeX", ref mHPBarSize.X, Predicates.IsNotZero);
				s.StreamAttributeOpt("sizeY", ref mHPBarSize.Y, Predicates.IsNotZero);
				s.StreamBVector("offset", ref mHPBarOffset, xmlSource: XML.XmlUtil.kSourceAttr);
			}
			#endregion
			XML.XmlUtil.Serialize(s, HitZones, BHitZone.kBListXmlParams);
			xs.StreamDBID(s, "BeamHead", ref mBeamHead, DatabaseObjectKind.Unit);
			xs.StreamDBID(s, "BeamTail", ref mBeamTail, DatabaseObjectKind.Unit);
			s.StreamElementOpt("Level", ref mLevel, Predicates.IsNotZero);
			xs.StreamDBID(s, "LevelUpEffect", ref mLevelUpEffect, DatabaseObjectKind.Object);
			xs.StreamDBID(s, "RecoveringEffect", ref mRecoveringEffect, DatabaseObjectKind.Object);
			xs.StreamDBID(s, "AutoTrainOnBuilt", ref mAutoTrainOnBuilt, DatabaseObjectKind.Squad);
			#region Socket
			using (var bm = s.EnterCursorBookmarkOpt("Socket", this, v => v.HasSocketData)) if (bm.IsNotNull)
			{
				xs.StreamDBID(s, XML.XmlUtil.kNoXmlName, ref mSocketID, DatabaseObjectKind.ObjectType, isOptional: false, xmlSource: XML.XmlUtil.kSourceCursor);
				// #NOTE engine reads this Player in lower case, but actual uses pascal case
				s.StreamAttributeEnumOpt("Player", ref mSocketPlayerScope, e => e != BPlayerScope.Player);
				s.StreamAttributeOpt("AutoSocket", ref mAutoSocket, Predicates.IsTrue);
			}
			#endregion
			#region Rate
			using (var bm = s.EnterCursorBookmarkOpt(BGameData.kRatesBListTypeValuesXmlParams.ElementName, this, v => v.HasRateData)) if (bm.IsNotNull)
			{
				// #NOTE engine reads Rate as lower case, but actual data is in pascal case
				xs.StreamTypeName(s, BGameData.kRatesBListTypeValuesXmlParams.DataName, ref mRateID, GameDataObjectKind.Rate, isOptional: false, xmlSource: XML.XmlUtil.kSourceAttr);
				s.StreamCursor(ref mRateAmount);
			}
			#endregion
			s.StreamElementOpt("MaxContained", ref mMaxContained, Predicates.IsNotZero);
			s.StreamElementOpt("MaxFlameEffects", ref mMaxFlameEffects, Predicates.IsNotNone);
			s.StreamElements("Contain", Contains, xs, XML.BDatabaseXmlSerializerBase.StreamObjectType);
			s.StreamElementEnumOpt("GarrisonSquadMode", ref mGarrisonSquadMode, e => e != BSquadMode.Invalid);
			xs.StreamDBID(s, "BuildStatsObject", ref mBuildStatsObjectID, DatabaseObjectKind.Object);
			s.StreamElementOpt("SubSelectSort", ref mSubSelectSort, v => v != int.MaxValue);
			s.StreamElementOpt(kXmlElementAttackGradeDPS, ref mAttackGradeDPS, Predicates.IsNotZero);
			s.StreamElementOpt("RamDodgeFactor", ref mRamDodgeFactor, Predicates.IsNotZero);
			#region HoveringRumble
			using (var bm = s.EnterCursorBookmarkOpt("HoveringRumble", HoveringRumble, Predicates.IsNotNull)) if (bm.IsNotNull)
			{
				if (s.IsReading)
					HoveringRumble = new BRumbleEvent();

				HoveringRumble.Serialize(s);
			}
			#endregion
			s.StreamElementEnumOpt("VisualDisplayPriority", ref mVisualDisplayPriority, e => e != BVisualDisplayPriority.Normal);
			s.StreamElementOpt("ChildObjectDamageTakenScalar", ref mChildObjectDamageTakenScalar, Predicates.IsNotZero);
			s.StreamElementOpt("TrueLOSHeight", ref mTrueLOSHeight, f => f != cDefaultTrueLOSHeight);
			s.StreamElementOpt("GarrisonTime", ref mGarrisonTime, Predicates.IsNotZero);
			s.StreamElementOpt("BuildRotation", ref mBuildRotation, Predicates.IsNotZero);
			s.StreamBVector("BuildOffset", ref mBuildOffset);
			#region AutoParkingLot
			using (var bm = s.EnterCursorBookmarkOpt("AutoParkingLot", this, v => v.HasAutoParkingLotData)) if (bm.IsNotNull)
			{
				xs.StreamDBID(s, XML.XmlUtil.kNoXmlName, ref mAutoParkingLotObjectID, DatabaseObjectKind.Object, isOptional: false, xmlSource: XML.XmlUtil.kSourceCursor);
				s.StreamAttributeOpt("Rotation", ref mAutoParkingLotRotation, Predicates.IsNotZero);
				s.StreamBVector("Offset", ref mAutoParkingLotOffset, xmlSource: XML.XmlUtil.kSourceAttr);
			}
			#endregion
			s.StreamElementOpt("BuildingStrengthDisplay", ref mBuildingStrengthID, Predicates.IsNotNullOrEmpty);
			xs.StreamDBID(s, "ShieldType", ref mShieldType, DatabaseObjectKind.Unit);
			s.StreamElementOpt("RevealRadius", ref mRevealRadius, Predicates.IsNotZero);
			xs.StreamDBID(s, "TargetBeam", ref mTargetBeam, DatabaseObjectKind.Object);
			xs.StreamDBID(s, "KillBeam", ref mKillBeam, DatabaseObjectKind.Object);
			s.StreamElementOpt("MinimapIconName", ref mMinimapIconName, Predicates.IsNotNullOrEmpty);

			if (s.IsReading)
			{
				PostDeserialize();
			}
		}

		private void PostDeserialize()
		{
			if (SortCommandsAfterReading)
			{
				SortCommands();
			}

			SquadModeAnims.Sort();
		}
		#endregion

		public static bool SortCommandsAfterReading = false;
		private void SortCommands()
		{
			Commands.Sort(CompareCommands);
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
