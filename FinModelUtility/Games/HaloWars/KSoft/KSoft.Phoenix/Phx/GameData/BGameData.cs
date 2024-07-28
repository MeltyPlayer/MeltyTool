using System;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Phoenix.Phx
{
	[ProtoDataTypeObjectSourceKind(ProtoDataObjectSourceKind.GameData)]
	public sealed class BGameData
		: IO.ITagElementStringNameStreamable
		, IProtoDataObjectDatabaseProvider
	{
		public ProtoDataObjectDatabase ObjectDatabase { get; private set; }

		#region Xml constants
		const string kXmlRoot = "GameData";

		public static readonly Engine.XmlFileInfo kXmlFileInfo = new Engine.XmlFileInfo
		{
			Directory = Engine.GameDirectory.Data,
			FileName = "GameData.xml",
			RootName = kXmlRoot
		};
		public static readonly Engine.ProtoDataXmlFileInfo kProtoFileInfo = new Engine.ProtoDataXmlFileInfo(
			Engine.XmlFilePriority.GameData,
			kXmlFileInfo);

		public static readonly Collections.BTypeValuesParams<float> kRatesBListTypeValuesParams = new
			Collections.BTypeValuesParams<float>(db => db.GameData.Rates) { kTypeGetInvalid = PhxUtil.kGetInvalidSingle };
		static readonly XML.BListXmlParams kRatesXmlParams = new XML.BListXmlParams("Rate");
		public static readonly XML.BTypeValuesXmlParams<float> kRatesBListTypeValuesXmlParams = new
			XML.BTypeValuesXmlParams<float>("Rate", "Rate"); // oiy, really? name the 'type' attribute with the same name as the element?
		static readonly XML.BListXmlParams kPlayerStatesXmlParams = new XML.BListXmlParams("PlayerState");
		static readonly XML.BListXmlParams kPopsXmlParams = new XML.BListXmlParams("Pop");
		static readonly XML.BListXmlParams kRefCountsXmlParams = new XML.BListXmlParams("RefCount");
		static readonly XML.BListXmlParams kHUDItemsXmlParams = new XML.BListXmlParams("HUDItem");
		static readonly XML.BListXmlParams kFlashableItemsXmlParams = new XML.BListXmlParams
		{
			RootName = "FlashableItems",
			ElementName = "Item",
			Flags = XML.BCollectionXmlParamsFlags.UseInnerTextForData,
		};
		static readonly XML.BListXmlParams kUnitFlagsXmlParams = new XML.BListXmlParams("UnitFlag");
		static readonly XML.BListXmlParams kSquadFlagsXmlParams = new XML.BListXmlParams("SquadFlag");

		static readonly Collections.BTypeValuesParams<string> kCodeProtoObjectsParams = new Collections.BTypeValuesParams<string>(db => db.GameProtoObjectTypes);
		static readonly XML.BTypeValuesXmlParams<string> kCodeProtoObjectsXmlParams = new XML.BTypeValuesXmlParams<string>("CodeProtoObject", "Type")
		{
			RootName = "CodeProtoObjects",
		};
		static readonly Collections.BTypeValuesParams<string> kCodeObjectTypesParams = new Collections.BTypeValuesParams<string>(db => db.GameObjectTypes);
		static readonly XML.BTypeValuesXmlParams<string> kCodeObjectTypesXmlParams = new XML.BTypeValuesXmlParams<string>("CodeObjectType", "Type")
		{
			RootName = "CodeObjectTypes",
		};
		#endregion

		public Collections.BListAutoId<BResource> Resources { get; private set; }
		public Collections.BTypeNames Rates { get; private set; }

		#region GoodAgainstGrades
		uint[] mGoodAgainstGrades = new uint[(int)ReticleAttackGrade.kNumberOf];
		public uint[] GoodAgainstGrades { get { return this.mGoodAgainstGrades; } }
		#endregion

		#region DifficultyModifiers
		float[] mDifficultyModifiers = new float[(int)BDifficultyTypeModifier.kNumberOf];
		public float[] DifficultyModifiers { get { return this.mDifficultyModifiers; } }
		#endregion

		public Collections.BTypeNames Populations { get; private set; }
		public Collections.BTypeNames RefCounts { get; private set; }
		public Collections.BTypeNames PlayerStates { get; private set; }

		#region GarrisonDamageMultiplier
		float mGarrisonDamageMultiplier = 1.0f;
		public float GarrisonDamageMultiplier
		{
			get { return this.mGarrisonDamageMultiplier; }
			set { this.mGarrisonDamageMultiplier = value; }
		}
		#endregion

		#region ConstructionDamageMultiplier
		float mConstructionDamageMultiplier = 1.0f;
		public float ConstructionDamageMultiplier
		{
			get { return this.mConstructionDamageMultiplier; }
			set { this.mConstructionDamageMultiplier = value; }
		}
		#endregion

		#region CaptureDecayRate
		float mCaptureDecayRate;
		public float CaptureDecayRate
		{
			get { return this.mCaptureDecayRate; }
			set { this.mCaptureDecayRate = value; }
		}
		#endregion

		#region SquadLeashLength
		float mSquadLeashLength;
		public float SquadLeashLength
		{
			get { return this.mSquadLeashLength; }
			set { this.mSquadLeashLength = value; }
		}
		#endregion

		#region SquadAggroLength
		float mSquadAggroLength;
		public float SquadAggroLength
		{
			get { return this.mSquadAggroLength; }
			set { this.mSquadAggroLength = value; }
		}

		/// <summary>Engine clamps aggro length to leash length</summary>
		public bool SquadAggroLengthIsLessThanLeash { get { return this.SquadAggroLength < this.SquadLeashLength; } }
		#endregion

		#region UnitLeashLength
		float mUnitLeashLength;
		public float UnitLeashLength
		{
			get { return this.mUnitLeashLength; }
			set { this.mUnitLeashLength = value; }
		}
		#endregion

		#region MaxNumCorpses
		int mMaxNumCorpses;
		public int MaxNumCorpses
		{
			get { return this.mMaxNumCorpses; }
			set { this.mMaxNumCorpses = value; }
		}
		#endregion

		#region BurningEffectLimits
		int mDefaultBurningEffectLimit = 1;
		public int DefaultBurningEffectLimit
		{
			get { return this.mDefaultBurningEffectLimit; }
			set { this.mDefaultBurningEffectLimit = value; }
		}

		public Collections.BListArray<BBurningEffectLimit> BurningEffectLimits { get; private set; }
		#endregion

		#region Fatality
		float mFatalityTransitionScale;
		public float FatalityTransitionScale
		{
			get { return this.mFatalityTransitionScale; }
			set { this.mFatalityTransitionScale = value; }
		}

		float mFatalityMaxTransitionTime;
		public float FatalityMaxTransitionTime
		{
			get { return this.mFatalityMaxTransitionTime; }
			set { this.mFatalityMaxTransitionTime = value; }
		}

		float mFatalityPositionOffsetTolerance;
		public float FatalityPositionOffsetTolerance
		{
			get { return this.mFatalityPositionOffsetTolerance; }
			set { this.mFatalityPositionOffsetTolerance = value; }
		}

		float mFatalityOrientationOffsetTolerance;
		/// <summary>angle</summary>
		public float FatalityOrientationOffsetTolerance
		{
			get { return this.mFatalityOrientationOffsetTolerance; }
			set { this.mFatalityOrientationOffsetTolerance = value; }
		}

		float mFatalityExclusionRange;
		public float FatalityExclusionRange
		{
			get { return this.mFatalityExclusionRange; }
			set { this.mFatalityExclusionRange = value; }
		}
		#endregion

		#region GameOverDelay
		float mGameOverDelay;
		public float GameOverDelay
		{
			get { return this.mGameOverDelay; }
			set { this.mGameOverDelay = value; }
		}
		#endregion

		#region InfantryCorpseDecayTime
		float mInfantryCorpseDecayTime;
		public float InfantryCorpseDecayTime
		{
			get { return this.mInfantryCorpseDecayTime; }
			set { this.mInfantryCorpseDecayTime = value; }
		}
		#endregion

		#region CorpseSinkingSpacing
		float mCorpseSinkingSpacing;
		public float CorpseSinkingSpacing
		{
			get { return this.mCorpseSinkingSpacing; }
			set { this.mCorpseSinkingSpacing = value; }
		}
		#endregion

		#region MaxCorpseDisposalCount
		int mMaxCorpseDisposalCount;
		public int MaxCorpseDisposalCount
		{
			get { return this.mMaxCorpseDisposalCount; }
			set { this.mMaxCorpseDisposalCount = value; }
		}
		#endregion

		#region MaxSquadPathsPerFrame
		uint mMaxSquadPathsPerFrame = 10;
		public uint MaxSquadPathsPerFrame
		{
			get { return this.mMaxSquadPathsPerFrame; }
			set { this.mMaxSquadPathsPerFrame = value; }
		}
		#endregion

		#region MaxPlatoonPathsPerFrame
		uint mMaxPlatoonPathsPerFrame = 10;
		public uint MaxPlatoonPathsPerFrame
		{
			get { return this.mMaxPlatoonPathsPerFrame; }
			set { this.mMaxPlatoonPathsPerFrame = value; }
		}
		#endregion

		#region ProjectileGravity
		float mProjectileGravity;
		public float ProjectileGravity
		{
			get { return this.mProjectileGravity; }
			set { this.mProjectileGravity = value; }
		}
		#endregion

		#region ProjectileTumbleRate
		float mProjectileTumbleRate;
		/// <summary>angle</summary>
		public float ProjectileTumbleRate
		{
			get { return this.mProjectileTumbleRate; }
			set { this.mProjectileTumbleRate = value; }
		}
		#endregion

		#region TrackInterceptDistance
		float mTrackInterceptDistance;
		public float TrackInterceptDistance
		{
			get { return this.mTrackInterceptDistance; }
			set { this.mTrackInterceptDistance = value; }
		}
		#endregion

		#region StationaryTargetAttackToleranceAngle
		float mStationaryTargetAttackToleranceAngle;
		public float StationaryTargetAttackToleranceAngle
		{
			get { return this.mStationaryTargetAttackToleranceAngle; }
			set { this.mStationaryTargetAttackToleranceAngle = value; }
		}
		#endregion

		#region MovingTargetAttackToleranceAngle
		float mMovingTargetAttackToleranceAngle;
		public float MovingTargetAttackToleranceAngle
		{
			get { return this.mMovingTargetAttackToleranceAngle; }
			set { this.mMovingTargetAttackToleranceAngle = value; }
		}
		#endregion

		#region MovingTargetTrackingAttackToleranceAngle
		float mMovingTargetTrackingAttackToleranceAngle;
		public float MovingTargetTrackingAttackToleranceAngle
		{
			get { return this.mMovingTargetTrackingAttackToleranceAngle; }
			set { this.mMovingTargetTrackingAttackToleranceAngle = value; }
		}
		#endregion

		#region MovingTargetRangeMultiplier
		float mMovingTargetRangeMultiplier = 1.0f;
		public float MovingTargetRangeMultiplier
		{
			get { return this.mMovingTargetRangeMultiplier; }
			set { this.mMovingTargetRangeMultiplier = value; }
		}
		#endregion

		#region CloakingDelay
		float mCloakingDelay;
		public float CloakingDelay
		{
			get { return this.mCloakingDelay; }
			set { this.mCloakingDelay = value; }
		}
		#endregion

		#region ReCloakDelay
		float mReCloakDelay;
		public float ReCloakDelay
		{
			get { return this.mReCloakDelay; }
			set { this.mReCloakDelay = value; }
		}
		#endregion

		#region CloakDetectFrequency
		float mCloakDetectFrequency;
		public float CloakDetectFrequency
		{
			get { return this.mCloakDetectFrequency; }
			set { this.mCloakDetectFrequency = value; }
		}
		#endregion

		#region ShieldRegenDelay
		float mShieldRegenDelay;
		public float ShieldRegenDelay
		{
			get { return this.mShieldRegenDelay; }
			set { this.mShieldRegenDelay = value; }
		}
		#endregion

		#region ShieldRegenTime
		float mShieldRegenTime;
		public float ShieldRegenTime
		{
			get { return this.mShieldRegenTime; }
			set { this.mShieldRegenTime = value; }
		}
		#endregion

		#region AttackedRevealerLOS
		float mAttackedRevealerLOS;
		public float AttackedRevealerLOS
		{
			get { return this.mAttackedRevealerLOS; }
			set { this.mAttackedRevealerLOS = value; }
		}
		#endregion

		#region AttackedRevealerLifespan
		float mAttackedRevealerLifespan;
		public float AttackedRevealerLifespan
		{
			get { return this.mAttackedRevealerLifespan; }
			set { this.mAttackedRevealerLifespan = value; }
		}
		#endregion

		#region AttackRevealerLOS
		float mAttackRevealerLOS;
		public float AttackRevealerLOS
		{
			get { return this.mAttackRevealerLOS; }
			set { this.mAttackRevealerLOS = value; }
		}
		#endregion

		#region AttackRevealerLifespan
		float mAttackRevealerLifespan;
		public float AttackRevealerLifespan
		{
			get { return this.mAttackRevealerLifespan; }
			set { this.mAttackRevealerLifespan = value; }
		}
		#endregion

		#region MinimumRevealerSize
		float mMinimumRevealerSize;
		public float MinimumRevealerSize
		{
			get { return this.mMinimumRevealerSize; }
			set { this.mMinimumRevealerSize = value; }
		}
		#endregion

		#region AttackRatingMultiplier
		float mAttackRatingMultiplier = 20f;
		public float AttackRatingMultiplier
		{
			get { return this.mAttackRatingMultiplier; }
			set { this.mAttackRatingMultiplier = value; }
		}
		#endregion

		#region DefenseRatingMultiplier
		float mDefenseRatingMultiplier = 10f;
		public float DefenseRatingMultiplier
		{
			get { return this.mDefenseRatingMultiplier; }
			set { this.mDefenseRatingMultiplier = value; }
		}
		#endregion

		#region GoodAgainstMinAttackGrade
		uint mGoodAgainstMinAttackGrade = 3;
		public uint GoodAgainstMinAttackGrade
		{
			get { return this.mGoodAgainstMinAttackGrade; }
			set { this.mGoodAgainstMinAttackGrade = value; }
		}
		#endregion

		#region HeightBonusDamage
		float mHeightBonusDamage;
		public float HeightBonusDamage
		{
			get { return this.mHeightBonusDamage; }
			set { this.mHeightBonusDamage = value; }
		}
		#endregion

		#region ShieldBarColor
		System.Drawing.Color mShieldBarColor;
		public System.Drawing.Color ShieldBarColor
		{
			get { return this.mShieldBarColor; }
			set { this.mShieldBarColor = value; }
		}
		#endregion

		#region AmmoBarColor
		System.Drawing.Color mAmmoBarColor;
		public System.Drawing.Color AmmoBarColor
		{
			get { return this.mAmmoBarColor; }
			set { this.mAmmoBarColor = value; }
		}
		#endregion

		#region OpportunityDistPriFactor
		float mOpportunityDistPriFactor = 1.0f;
		public float OpportunityDistPriFactor
		{
			get { return this.mOpportunityDistPriFactor; }
			set { this.mOpportunityDistPriFactor = value; }
		}
		#endregion

		#region OpportunityBeingAttackedPriBonus
		float mOpportunityBeingAttackedPriBonus;
		public float OpportunityBeingAttackedPriBonus
		{
			get { return this.mOpportunityBeingAttackedPriBonus; }
			set { this.mOpportunityBeingAttackedPriBonus = value; }
		}
		#endregion

		#region ChanceToRocket
		float mChanceToRocket;
		public float ChanceToRocket
		{
			get { return this.mChanceToRocket; }
			set { this.mChanceToRocket = value; }
		}
		#endregion

		#region MaxDamageBankPctAdjust
		float mMaxDamageBankPctAdjust;
		public float MaxDamageBankPctAdjust
		{
			get { return this.mMaxDamageBankPctAdjust; }
			set { this.mMaxDamageBankPctAdjust = value; }
		}
		#endregion

		#region DamageBankTimer
		float mDamageBankTimer;
		public float DamageBankTimer
		{
			get { return this.mDamageBankTimer; }
			set { this.mDamageBankTimer = value; }
		}
		#endregion

		#region BuildingSelfDestructTime
		float mBuildingSelfDestructTime;
		public float BuildingSelfDestructTime
		{
			get { return this.mBuildingSelfDestructTime; }
			set { this.mBuildingSelfDestructTime = value; }
		}
		#endregion

		#region TributeAmount
		float mTributeAmount = 500f;
		public float TributeAmount
		{
			get { return this.mTributeAmount; }
			set { this.mTributeAmount = value; }
		}
		#endregion

		#region TributeCost
		float mTributeCost;
		public float TributeCost
		{
			get { return this.mTributeCost; }
			set { this.mTributeCost = value; }
		}
		#endregion

		#region UnscSupplyPadBonus
		float mUnscSupplyPadBonus;
		public float UnscSupplyPadBonus
		{
			get { return this.mUnscSupplyPadBonus; }
			set { this.mUnscSupplyPadBonus = value; }
		}
		#endregion

		#region UnscSupplyPadBreakEvenPoint
		float mUnscSupplyPadBreakEvenPoint;
		public float UnscSupplyPadBreakEvenPoint
		{
			get { return this.mUnscSupplyPadBreakEvenPoint; }
			set { this.mUnscSupplyPadBreakEvenPoint = value; }
		}
		#endregion

		#region CovSupplyPadBonus
		float mCovSupplyPadBonus;
		public float CovSupplyPadBonus
		{
			get { return this.mCovSupplyPadBonus; }
			set { this.mCovSupplyPadBonus = value; }
		}
		#endregion

		#region CovSupplyPadBreakEvenPoint
		float mCovSupplyPadBreakEvenPoint;
		public float CovSupplyPadBreakEvenPoint
		{
			get { return this.mCovSupplyPadBreakEvenPoint; }
			set { this.mCovSupplyPadBreakEvenPoint = value; }
		}
		#endregion

		#region LeaderPowerChargeResourceID
		int mLeaderPowerChargeResourceID = TypeExtensions.kNone;
		[Meta.ResourceReference]
		public int LeaderPowerChargeResourceID
		{
			get { return this.mLeaderPowerChargeResourceID; }
			set { this.mLeaderPowerChargeResourceID = value; }
		}
		#endregion

		#region LeaderPowerChargeRateID
		int mLeaderPowerChargeRateID = TypeExtensions.kNone;
		[Meta.RateReference]
		public int LeaderPowerChargeRateID
		{
			get { return this.mLeaderPowerChargeRateID; }
			set { this.mLeaderPowerChargeRateID = value; }
		}
		#endregion

		#region DamageReceivedXPFactor
		float mDamageReceivedXPFactor;
		public float DamageReceivedXPFactor
		{
			get { return this.mDamageReceivedXPFactor; }
			set { this.mDamageReceivedXPFactor = value; }
		}
		#endregion

		#region AirStrikeLoiterTime
		float mAirStrikeLoiterTime;
		public float AirStrikeLoiterTime
		{
			get { return this.mAirStrikeLoiterTime; }
			set { this.mAirStrikeLoiterTime = value; }
		}
		#endregion

		#region RecyleRefundRate
		float mRecyleRefundRate = 1.0f;
		public float RecyleRefundRate
		{
			get { return this.mRecyleRefundRate; }
			set { this.mRecyleRefundRate = value; }
		}
		#endregion

		#region BaseRebuildTimer
		float mBaseRebuildTimer;
		public float BaseRebuildTimer
		{
			get { return this.mBaseRebuildTimer; }
			set { this.mBaseRebuildTimer = value; }
		}
		#endregion

		#region ObjectiveArrowRadialOffset
		float mObjectiveArrowRadialOffset;
		public float ObjectiveArrowRadialOffset
		{
			get { return this.mObjectiveArrowRadialOffset; }
			set { this.mObjectiveArrowRadialOffset = value; }
		}
		#endregion

		#region ObjectiveArrowSwitchOffset
		float mObjectiveArrowSwitchOffset;
		public float ObjectiveArrowSwitchOffset
		{
			get { return this.mObjectiveArrowSwitchOffset; }
			set { this.mObjectiveArrowSwitchOffset = value; }
		}
		#endregion

		#region ObjectiveArrowYOffset
		float mObjectiveArrowYOffset;
		public float ObjectiveArrowYOffset
		{
			get { return this.mObjectiveArrowYOffset; }
			set { this.mObjectiveArrowYOffset = value; }
		}
		#endregion

		#region ObjectiveArrowMaxIndex
		byte mObjectiveArrowMaxIndex;
		public byte ObjectiveArrowMaxIndex
		{
			get { return this.mObjectiveArrowMaxIndex; }
			set { this.mObjectiveArrowMaxIndex = value; }
		}
		#endregion

		#region OverrunMinVel
		float mOverrunMinVel;
		public float OverrunMinVel
		{
			get { return this.mOverrunMinVel; }
			set { this.mOverrunMinVel = value; }
		}
		#endregion

		#region OverrunJumpForce
		float mOverrunJumpForce;
		public float OverrunJumpForce
		{
			get { return this.mOverrunJumpForce; }
			set { this.mOverrunJumpForce = value; }
		}
		#endregion

		#region OverrunDistance
		float mOverrunDistance;
		public float OverrunDistance
		{
			get { return this.mOverrunDistance; }
			set { this.mOverrunDistance = value; }
		}
		#endregion

		#region CoopResourceSplitRate
		float mCoopResourceSplitRate = 1.0f;
		public float CoopResourceSplitRate
		{
			get { return this.mCoopResourceSplitRate; }
			set { this.mCoopResourceSplitRate = value; }
		}
		#endregion

		#region Hero globals
		#region HeroDownedLOS
		float mHeroDownedLOS;
		public float HeroDownedLOS
		{
			get { return this.mHeroDownedLOS; }
			set { this.mHeroDownedLOS = value; }
		}
		#endregion

		#region HeroHPRegenTime
		float mHeroHPRegenTime;
		public float HeroHPRegenTime
		{
			get { return this.mHeroHPRegenTime; }
			set { this.mHeroHPRegenTime = value; }
		}
		#endregion

		#region HeroRevivalDistance
		float mHeroRevivalDistance;
		public float HeroRevivalDistance
		{
			get { return this.mHeroRevivalDistance; }
			set { this.mHeroRevivalDistance = value; }
		}
		#endregion

		#region HeroPercentHPRevivalThreshhold
		float mHeroPercentHPRevivalThreshhold;
		public float HeroPercentHPRevivalThreshhold
		{
			get { return this.mHeroPercentHPRevivalThreshhold; }
			set { this.mHeroPercentHPRevivalThreshhold = value; }
		}
		#endregion

		#region MaxDeadHeroTransportDist
		float mMaxDeadHeroTransportDist;
		public float MaxDeadHeroTransportDist
		{
			get { return this.mMaxDeadHeroTransportDist; }
			set { this.mMaxDeadHeroTransportDist = value; }
		}
		#endregion
		#endregion

		#region Transport
		float mTransportClearRadiusScale = 1.0f;
		public float TransportClearRadiusScale
		{
			get { return this.mTransportClearRadiusScale; }
			set { this.mTransportClearRadiusScale = value; }
		}

		float mTransportMaxSearchRadiusScale = 1.0f;
		public float TransportMaxSearchRadiusScale
		{
			get { return this.mTransportMaxSearchRadiusScale; }
			set { this.mTransportMaxSearchRadiusScale = value; }
		}

		uint mTransportMaxSearchLocations = 1;
		public uint TransportMaxSearchLocations
		{
			get { return this.mTransportMaxSearchLocations; }
			set { this.mTransportMaxSearchLocations = value; }
		}

		uint mTransportBlockTime;
		public uint TransportBlockTime
		{
			get { return this.mTransportBlockTime; }
			set { this.mTransportBlockTime = value; }
		}

		uint mTransportLoadBlockTime;
		public uint TransportLoadBlockTime
		{
			get { return this.mTransportLoadBlockTime; }
			set { this.mTransportLoadBlockTime = value; }
		}
		#endregion

		#region Ambient life
		uint mALMaxWanderFrequency;
		public uint ALMaxWanderFrequency
		{
			get { return this.mALMaxWanderFrequency; }
			set { this.mALMaxWanderFrequency = value; }
		}

		uint mALPredatorCheckFrequency;
		public uint ALPredatorCheckFrequency
		{
			get { return this.mALPredatorCheckFrequency; }
			set { this.mALPredatorCheckFrequency = value; }
		}

		uint mALPreyCheckFrequency;
		public uint ALPreyCheckFrequency
		{
			get { return this.mALPreyCheckFrequency; }
			set { this.mALPreyCheckFrequency = value; }
		}

		float mALOppCheckRadius;
		public float ALOppCheckRadius
		{
			get { return this.mALOppCheckRadius; }
			set { this.mALOppCheckRadius = value; }
		}

		float mALFleeDistance;
		public float ALFleeDistance
		{
			get { return this.mALFleeDistance; }
			set { this.mALFleeDistance = value; }
		}

		float mALFleeMovementModifier;
		public float ALFleeMovementModifier
		{
			get { return this.mALFleeMovementModifier; }
			set { this.mALFleeMovementModifier = value; }
		}

		float mALMinWanderDistance;
		public float ALMinWanderDistance
		{
			get { return this.mALMinWanderDistance; }
			set { this.mALMinWanderDistance = value; }
		}

		float mALMaxWanderDistance;
		public float ALMaxWanderDistance
		{
			get { return this.mALMaxWanderDistance; }
			set { this.mALMaxWanderDistance = value; }
		}

		float mALSpawnerCheckFrequency;
		public float ALSpawnerCheckFrequency
		{
			get { return this.mALSpawnerCheckFrequency; }
			set { this.mALSpawnerCheckFrequency = value; }
		}
		#endregion

		#region Transport
		uint mTransportMaxBlockAttempts = 1;
		public uint TransportMaxBlockAttempts
		{
			get { return this.mTransportMaxBlockAttempts; }
			set { this.mTransportMaxBlockAttempts = value; }
		}

		float mTransportIncomingHeight = 60.0f;
		public float TransportIncomingHeight
		{
			get { return this.mTransportIncomingHeight; }
			set { this.mTransportIncomingHeight = value; }
		}

		float mTransportIncomingOffset = 40.0f;
		public float TransportIncomingOffset
		{
			get { return this.mTransportIncomingOffset; }
			set { this.mTransportIncomingOffset = value; }
		}

		float mTransportOutgoingHeight = 60.0f;
		public float TransportOutgoingHeight
		{
			get { return this.mTransportOutgoingHeight; }
			set { this.mTransportOutgoingHeight = value; }
		}

		float mTransportOutgoingOffset = 40.0f;
		public float TransportOutgoingOffset
		{
			get { return this.mTransportOutgoingOffset; }
			set { this.mTransportOutgoingOffset = value; }
		}

		float mTransportPickupHeight = 12.0f;
		public float TransportPickupHeight
		{
			get { return this.mTransportPickupHeight; }
			set { this.mTransportPickupHeight = value; }
		}

		float mTransportDropoffHeight = 12.0f;
		public float TransportDropoffHeight
		{
			get { return this.mTransportDropoffHeight; }
			set { this.mTransportDropoffHeight = value; }
		}

		uint mTransportMax = 3;
		public uint TransportMax
		{
			get { return this.mTransportMax; }
			set { this.mTransportMax = value; }
		}
		#endregion

		#region HitchOffset
		float mHitchOffset = 8.0f;
		public float HitchOffset
		{
			get { return this.mHitchOffset; }
			set { this.mHitchOffset = value; }
		}
		#endregion

		#region Cyro globals
		#region TimeFrozenToThaw
		float mTimeFrozenToThaw;
		public float TimeFrozenToThaw
		{
			get { return this.mTimeFrozenToThaw; }
			set { this.mTimeFrozenToThaw = value; }
		}
		#endregion

		#region TimeFreezingToThaw
		float mTimeFreezingToThaw;
		public float TimeFreezingToThaw
		{
			get { return this.mTimeFreezingToThaw; }
			set { this.mTimeFreezingToThaw = value; }
		}
		#endregion

		#region DefaultCryoPoints
		float mDefaultCryoPoints;
		public float DefaultCryoPoints
		{
			get { return this.mDefaultCryoPoints; }
			set { this.mDefaultCryoPoints = value; }
		}
		#endregion

		#region DefaultThawSpeed
		float mDefaultThawSpeed;
		public float DefaultThawSpeed
		{
			get { return this.mDefaultThawSpeed; }
			set { this.mDefaultThawSpeed = value; }
		}
		#endregion

		#region FreezingSpeedModifier
		float mFreezingSpeedModifier;
		public float FreezingSpeedModifier
		{
			get { return this.mFreezingSpeedModifier; }
			set { this.mFreezingSpeedModifier = value; }
		}
		#endregion

		#region FreezingDamageModifier
		float mFreezingDamageModifier;
		public float FreezingDamageModifier
		{
			get { return this.mFreezingDamageModifier; }
			set { this.mFreezingDamageModifier = value; }
		}
		#endregion

		#region FrozenDamageModifier
		float mFrozenDamageModifier;
		public float FrozenDamageModifier
		{
			get { return this.mFrozenDamageModifier; }
			set { this.mFrozenDamageModifier = value; }
		}
		#endregion
		#endregion

		#region SmallDotSize
		float mSmallDotSize;
		public float SmallDotSize
		{
			get { return this.mSmallDotSize; }
			set { this.mSmallDotSize = value; }
		}
		#endregion

		#region MediumDotSize
		float mMediumDotSize;
		public float MediumDotSize
		{
			get { return this.mMediumDotSize; }
			set { this.mMediumDotSize = value; }
		}
		#endregion

		public Collections.BTypeValuesString CodeProtoObjects { get; private set; }
		public Collections.BTypeValuesString CodeObjectTypes { get; private set; }
		public Collections.BListArray<BInfectionMap> InfectionMap { get; private set; }

		#region Nonsense
		/// <remarks>Engine doesn't process these, but some trigger scripts use these dynamic types, so keep them on record</remarks>
		public Collections.BTypeNames HUDItems { get; private set; }
		/// <remarks>Engine doesn't process these, but some trigger scripts use these dynamic types, so keep them on record</remarks>
		public Collections.BTypeNames FlashableItems { get; private set; }
		/// <remarks>Engine doesn't process these, but some trigger scripts use these dynamic types, so keep them on record</remarks>
		public Collections.BTypeNames UnitFlags { get; private set; }
		/// <remarks>Engine doesn't process these, but some trigger scripts use these dynamic types, so keep them on record</remarks>
		public Collections.BTypeNames SquadFlags { get; private set; }
		#endregion

		/// <summary>Get how much it costs, in total, to tribute a resource to another player</summary>
		public float TotalTributeCost { get { return (this.mTributeAmount * this.mTributeCost) + this.mTributeAmount; } }

		public BGameData()
		{
			this.ObjectDatabase = new ProtoDataObjectDatabase(this, typeof(GameDataObjectKind));

			this.Resources = new Collections.BListAutoId<BResource>();
			this.Rates = new Collections.BTypeNames();
			#region DifficultyModifiers

			this.mDifficultyModifiers[(int)BDifficultyTypeModifier.Normal] = 0.34f;
			this.mDifficultyModifiers[(int)BDifficultyTypeModifier.Hard] = 0.67f;
			this.mDifficultyModifiers[(int)BDifficultyTypeModifier.Legendary] = 1.0f;
			this.mDifficultyModifiers[(int)BDifficultyTypeModifier.Default] = 0.4f;
			this.mDifficultyModifiers[(int)BDifficultyTypeModifier.SPCAIDefault] = 0.5f;
			#endregion

			this.Populations = new Collections.BTypeNames();
			this.RefCounts = new Collections.BTypeNames();
			this.PlayerStates = new Collections.BTypeNames();
			this.BurningEffectLimits = new Collections.BListArray<BBurningEffectLimit>();
			this.CodeProtoObjects = new Collections.BTypeValuesString(kCodeProtoObjectsParams);
			this.CodeObjectTypes = new Collections.BTypeValuesString(kCodeObjectTypesParams);
			this.InfectionMap = new Collections.BListArray<BInfectionMap>();

			#region Nonsense

			this.HUDItems = new Collections.BTypeNames();
			this.FlashableItems = new Collections.BTypeNames();
			this.UnitFlags = new Collections.BTypeNames();
			this.SquadFlags = new Collections.BTypeNames();
			#endregion
		}

		#region Database interfaces
		internal Collections.IBTypeNames GetNamesInterface(GameDataObjectKind kind)
		{
			Contract.Requires<ArgumentOutOfRangeException>(kind != GameDataObjectKind.None);

			switch (kind)
			{
			case GameDataObjectKind.Cost: return this.Resources;
			case GameDataObjectKind.Pop:  return this.Populations;
			case GameDataObjectKind.Rate: return this.Rates;

			default: throw new KSoft.Debug.UnreachableException(kind.ToString());
			}
		}

		internal Collections.IHasUndefinedProtoMemberInterface GetMembersInterface(GameDataObjectKind kind)
		{
			Contract.Requires<ArgumentOutOfRangeException>(kind != GameDataObjectKind.None);

			switch (kind)
			{
			case GameDataObjectKind.Cost: return this.Resources;
			case GameDataObjectKind.Pop:  return this.Populations;
			case GameDataObjectKind.Rate: return this.Rates;

			default: throw new KSoft.Debug.UnreachableException(kind.ToString());
			}
		}
		#endregion

		#region ITagElementStreamable<string> Members
		/// <remarks>For streaming directly from gamedata.xml</remarks>
		internal void StreamGameData<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			XML.XmlUtil.Serialize(s, this.Resources, BResource.kBListXmlParams);
			XML.XmlUtil.Serialize(s, this.Rates, kRatesXmlParams);
			#region GoodAgainstReticle
			using (s.EnterCursorBookmark("GoodAgainstReticle"))
			{
				s.StreamElementOpt("NoEffect", ref this.mGoodAgainstGrades[(int)ReticleAttackGrade.NoEffect], Predicates.IsNotZero);
				s.StreamElementOpt("Weak",     ref this.mGoodAgainstGrades[(int)ReticleAttackGrade.Weak], Predicates.IsNotZero);
				s.StreamElementOpt("Fair",     ref this.mGoodAgainstGrades[(int)ReticleAttackGrade.Fair], Predicates.IsNotZero);
				s.StreamElementOpt("Good",     ref this.mGoodAgainstGrades[(int)ReticleAttackGrade.Good], Predicates.IsNotZero);
				s.StreamElementOpt("Extreme",  ref this.mGoodAgainstGrades[(int)ReticleAttackGrade.Extreme], Predicates.IsNotZero);
			}
			#endregion
			#region DifficultyModifiers
			s.StreamElementOpt("DifficultyEasy",         ref this.mDifficultyModifiers[(int)BDifficultyTypeModifier.Easy], Predicates.IsNotZero);
			// #NOTE The engine has a typo in it and looks for "DifficultyNormali"
			s.StreamElementOpt("DifficultyNormal",       ref this.mDifficultyModifiers[(int)BDifficultyTypeModifier.Normal], Predicates.IsNotZero);
			s.StreamElementOpt("DifficultyHard",         ref this.mDifficultyModifiers[(int)BDifficultyTypeModifier.Hard], Predicates.IsNotZero);
			s.StreamElementOpt("DifficultyLegendary",    ref this.mDifficultyModifiers[(int)BDifficultyTypeModifier.Legendary], Predicates.IsNotZero);
			s.StreamElementOpt("DifficultyDefault",      ref this.mDifficultyModifiers[(int)BDifficultyTypeModifier.Default], Predicates.IsNotZero);
			s.StreamElementOpt("DifficultySPCAIDefault", ref this.mDifficultyModifiers[(int)BDifficultyTypeModifier.SPCAIDefault], Predicates.IsNotZero);
			#endregion
			XML.XmlUtil.Serialize(s, this.Populations, kPopsXmlParams);
			XML.XmlUtil.Serialize(s, this.RefCounts, kRefCountsXmlParams);
			XML.XmlUtil.Serialize(s, this.PlayerStates, kPlayerStatesXmlParams);
			s.StreamElementOpt("GarrisonDamageMultiplier", ref this.mGarrisonDamageMultiplier, PhxPredicates.IsNotOne);
			s.StreamElementOpt("ConstructionDamageMultiplier", ref this.mConstructionDamageMultiplier, PhxPredicates.IsNotOne);
			s.StreamElementOpt("CaptureDecayRate", ref this.mCaptureDecayRate, Predicates.IsNotZero);
			s.StreamElementOpt("SquadLeashLength", ref this.mSquadLeashLength, Predicates.IsNotZero);
			s.StreamElementOpt("SquadAggroLength", ref this.mSquadAggroLength, Predicates.IsNotZero);
			s.StreamElementOpt("UnitLeashLength", ref this.mUnitLeashLength, Predicates.IsNotZero);
			s.StreamElementOpt("MaxNumCorpses", ref this.mMaxNumCorpses, Predicates.IsNotZero);
			#region BurningEffectLimits
			using (s.EnterCursorBookmark("BurningEffectLimits"))
			{
				s.StreamAttribute("DefaultLimit", ref this.mDefaultBurningEffectLimit);
				XML.XmlUtil.Serialize(s, this.BurningEffectLimits, BBurningEffectLimit.kBListXmlParams);
			}
			#endregion
			#region Fatality
			s.StreamElementOpt("FatalityTransitionScale", ref this.mFatalityTransitionScale, Predicates.IsNotZero);
			s.StreamElementOpt("FatalityMaxTransitionTime", ref this.mFatalityMaxTransitionTime, Predicates.IsNotZero);
			s.StreamElementOpt("FatalityPositionOffsetTolerance", ref this.mFatalityPositionOffsetTolerance, Predicates.IsNotZero);
			s.StreamElementOpt("FatalityOrientationOffsetTolerance", ref this.mFatalityOrientationOffsetTolerance, Predicates.IsNotZero);
			s.StreamElementOpt("FatalityExclusionRange", ref this.mFatalityExclusionRange, Predicates.IsNotZero);
			#endregion
			s.StreamElementOpt("GameOverDelay", ref this.mGameOverDelay, Predicates.IsNotZero);
			s.StreamElementOpt("InfantryCorpseDecayTime", ref this.mInfantryCorpseDecayTime, Predicates.IsNotZero);
			s.StreamElementOpt("CorpseSinkingSpacing", ref this.mCorpseSinkingSpacing, Predicates.IsNotZero);
			s.StreamElementOpt("MaxCorpseDisposalCount", ref this.mMaxCorpseDisposalCount, Predicates.IsNotZero);
			s.StreamElementOpt("MaxSquadPathsPerFrame", ref this.mMaxSquadPathsPerFrame, Predicates.IsNotZero);
			s.StreamElementOpt("MaxPlatoonPathsPerFrame", ref this.mMaxPlatoonPathsPerFrame, Predicates.IsNotZero);
			s.StreamElementOpt("ProjectileGravity", ref this.mProjectileGravity, Predicates.IsNotZero);
			s.StreamElementOpt("ProjectileTumbleRate", ref this.mProjectileTumbleRate, Predicates.IsNotZero);
			s.StreamElementOpt("TrackInterceptDistance", ref this.mTrackInterceptDistance, Predicates.IsNotZero);
			s.StreamElementOpt("StationaryTargetAttackToleranceAngle", ref this.mStationaryTargetAttackToleranceAngle, Predicates.IsNotZero);
			s.StreamElementOpt("MovingTargetAttackToleranceAngle", ref this.mMovingTargetAttackToleranceAngle, Predicates.IsNotZero);
			s.StreamElementOpt("MovingTargetTrackingAttackToleranceAngle", ref this.mMovingTargetTrackingAttackToleranceAngle, Predicates.IsNotZero);
			s.StreamElementOpt("MovingTargetRangeMultiplier", ref this.mMovingTargetRangeMultiplier, PhxPredicates.IsNotOne);
			s.StreamElementOpt("CloakingDelay", ref this.mCloakingDelay, Predicates.IsNotZero);
			s.StreamElementOpt("ReCloakDelay", ref this.mReCloakDelay, Predicates.IsNotZero);
			s.StreamElementOpt("CloakDetectFrequency", ref this.mCloakDetectFrequency, Predicates.IsNotZero);
			s.StreamElementOpt("ShieldRegenDelay", ref this.mShieldRegenDelay, Predicates.IsNotZero);
			s.StreamElementOpt("ShieldRegenTime", ref this.mShieldRegenTime, Predicates.IsNotZero);
			s.StreamElementOpt("AttackedRevealerLOS", ref this.mAttackedRevealerLOS, Predicates.IsNotZero);
			s.StreamElementOpt("AttackedRevealerLifespan", ref this.mAttackedRevealerLifespan, Predicates.IsNotZero);
			s.StreamElementOpt("AttackRevealerLOS", ref this.mAttackRevealerLOS, Predicates.IsNotZero);
			s.StreamElementOpt("AttackRevealerLifespan", ref this.mAttackRevealerLifespan, Predicates.IsNotZero);
			s.StreamElementOpt("MinimumRevealerSize", ref this.mMinimumRevealerSize, Predicates.IsNotZero);
			s.StreamElementOpt("AttackRatingMultiplier", ref this.mAttackRatingMultiplier, Predicates.IsNotZero);
			s.StreamElementOpt("DefenseRatingMultiplier", ref this.mDefenseRatingMultiplier, Predicates.IsNotZero);
			// #NOTE data has this as "GoodAgainstMinAttackRating"
			s.StreamElementOpt("GoodAgainstMinAttackRating", ref this.mGoodAgainstMinAttackGrade, Predicates.IsNotZero);
			s.StreamElementOpt("HeightBonusDamage", ref this.mHeightBonusDamage, Predicates.IsNotZero);
			s.StreamIntegerColor("ShieldBarColor", ref this.mShieldBarColor);
			s.StreamIntegerColor("AmmoBarColor", ref this.mAmmoBarColor);
			s.StreamElementOpt("OpportunityDistPriFactor", ref this.mOpportunityDistPriFactor, PhxPredicates.IsNotOne);
			s.StreamElementOpt("OpportunityBeingAttackedPriBonus", ref this.mOpportunityBeingAttackedPriBonus, Predicates.IsNotZero);
			s.StreamElementOpt("ChanceToRocket", ref this.mChanceToRocket, Predicates.IsNotZero);
			s.StreamElementOpt("MaxDamageBankPctAdjust", ref this.mMaxDamageBankPctAdjust, Predicates.IsNotZero);
			s.StreamElementOpt("DamageBankTimer", ref this.mDamageBankTimer, Predicates.IsNotZero);
			s.StreamElementOpt("BuildingSelfDestructTime", ref this.mBuildingSelfDestructTime, Predicates.IsNotZero);
			s.StreamElementOpt("TributeAmount", ref this.mTributeAmount, Predicates.IsNotZero);
			s.StreamElementOpt("TributeCost", ref this.mTributeCost, Predicates.IsNotZero);
			s.StreamElementOpt("UnscSupplyPadBonus", ref this.mUnscSupplyPadBonus, Predicates.IsNotZero);
			s.StreamElementOpt("UnscSupplyPadBreakEvenPoint", ref this.mUnscSupplyPadBreakEvenPoint, Predicates.IsNotZero);
			s.StreamElementOpt("CovSupplyPadBonus", ref this.mCovSupplyPadBonus, Predicates.IsNotZero);
			s.StreamElementOpt("CovSupplyPadBreakEvenPoint", ref this.mCovSupplyPadBreakEvenPoint, Predicates.IsNotZero);
			xs.StreamTypeName(s, "LeaderPowerChargeResource", ref this.mLeaderPowerChargeResourceID, GameDataObjectKind.Cost);
			xs.StreamTypeName(s, "LeaderPowerChargeRate", ref this.mLeaderPowerChargeRateID, GameDataObjectKind.Rate);
			s.StreamElementOpt("DamageReceivedXPFactor", ref this.mDamageReceivedXPFactor, PhxPredicates.IsNotInvalid);
			s.StreamElementOpt("AirStrikeLoiterTime", ref this.mAirStrikeLoiterTime, Predicates.IsNotZero);
			s.StreamElementOpt("RecyleRefundRate", ref this.mRecyleRefundRate, PhxPredicates.IsNotOne);
			s.StreamElementOpt("BaseRebuildTimer", ref this.mBaseRebuildTimer, Predicates.IsNotZero);
			s.StreamElementOpt("ObjectiveArrowRadialOffset", ref this.mObjectiveArrowRadialOffset, Predicates.IsNotZero);
			s.StreamElementOpt("ObjectiveArrowSwitchOffset", ref this.mObjectiveArrowSwitchOffset, Predicates.IsNotZero);
			s.StreamElementOpt("ObjectiveArrowYOffset", ref this.mObjectiveArrowYOffset, Predicates.IsNotZero);
			s.StreamElementOpt("ObjectiveArrowMaxIndex", ref this.mObjectiveArrowMaxIndex, Predicates.IsNotZero);
			s.StreamElementOpt("OverrunMinVel", ref this.mOverrunMinVel, Predicates.IsNotZero);
			s.StreamElementOpt("OverrunJumpForce", ref this.mOverrunJumpForce, Predicates.IsNotZero);
			s.StreamElementOpt("OverrunDistance", ref this.mOverrunDistance, Predicates.IsNotZero);
			s.StreamElementOpt("CoopResourceSplitRate", ref this.mCoopResourceSplitRate, PhxPredicates.IsNotOne);
			#region Hero globals
			s.StreamElementOpt("HeroDownedLOS", ref this.mHeroDownedLOS, Predicates.IsNotZero);
			s.StreamElementOpt("HeroHPRegenTime", ref this.mHeroHPRegenTime, Predicates.IsNotZero);
			s.StreamElementOpt("HeroRevivalDistance", ref this.mHeroRevivalDistance, Predicates.IsNotZero);
			s.StreamElementOpt("HeroPercentHPRevivalThreshhold", ref this.mHeroPercentHPRevivalThreshhold, Predicates.IsNotZero);
			s.StreamElementOpt("MaxDeadHeroTransportDist", ref this.mMaxDeadHeroTransportDist, Predicates.IsNotZero);
			#endregion
			#region Transport
			s.StreamElementOpt("TransportClearRadiusScale", ref this.mTransportClearRadiusScale, PhxPredicates.IsNotOne);
			s.StreamElementOpt("TransportMaxSearchRadiusScale", ref this.mTransportMaxSearchRadiusScale, PhxPredicates.IsNotOne);
			s.StreamElementOpt("TransportMaxSearchLocations", ref this.mTransportMaxSearchLocations, PhxPredicates.IsNotOne);
			s.StreamElementOpt("TransportBlockTime", ref this.mTransportBlockTime, Predicates.IsNotZero);
			s.StreamElementOpt("TransportLoadBlockTime", ref this.mTransportLoadBlockTime, Predicates.IsNotZero);
			#endregion
			#region Ambient life
			s.StreamElementOpt("ALMaxWanderFrequency", ref this.mALMaxWanderFrequency, Predicates.IsNotZero);
			s.StreamElementOpt("ALPredatorCheckFrequency", ref this.mALPredatorCheckFrequency, Predicates.IsNotZero);
			s.StreamElementOpt("ALPreyCheckFrequency", ref this.mALPreyCheckFrequency, Predicates.IsNotZero);
			s.StreamElementOpt("ALOppCheckRadius", ref this.mALOppCheckRadius, Predicates.IsNotZero);
			s.StreamElementOpt("ALFleeDistance", ref this.mALFleeDistance, Predicates.IsNotZero);
			s.StreamElementOpt("ALFleeMovementModifier", ref this.mALFleeMovementModifier, Predicates.IsNotZero);
			s.StreamElementOpt("ALMinWanderDistance", ref this.mALMinWanderDistance, Predicates.IsNotZero);
			s.StreamElementOpt("ALMaxWanderDistance", ref this.mALMaxWanderDistance, Predicates.IsNotZero);
			s.StreamElementOpt("ALSpawnerCheckFrequency", ref this.mALSpawnerCheckFrequency, Predicates.IsNotZero);
			#endregion
			#region Transport
			s.StreamElementOpt("TransportMaxBlockAttempts", ref this.mTransportMaxBlockAttempts, PhxPredicates.IsNotOne);
			s.StreamElementOpt("TransportIncomingHeight", ref this.mTransportIncomingHeight, Predicates.IsNotZero);
			s.StreamElementOpt("TransportIncomingOffset", ref this.mTransportIncomingOffset, Predicates.IsNotZero);
			s.StreamElementOpt("TransportOutgoingHeight", ref this.mTransportOutgoingHeight, Predicates.IsNotZero);
			s.StreamElementOpt("TransportOutgoingOffset", ref this.mTransportOutgoingOffset, Predicates.IsNotZero);
			s.StreamElementOpt("TransportPickupHeight", ref this.mTransportPickupHeight, Predicates.IsNotZero);
			s.StreamElementOpt("TransportDropoffHeight", ref this.mTransportDropoffHeight, Predicates.IsNotZero);
			s.StreamElementOpt("TransportMax", ref this.mTransportMax, Predicates.IsNotZero);
			#endregion
			s.StreamElementOpt("HitchOffset", ref this.mHitchOffset, Predicates.IsNotZero);
			#region Cryo globals
			s.StreamElementOpt("TimeFrozenToThaw", ref this.mTimeFrozenToThaw, Predicates.IsNotZero);
			s.StreamElementOpt("TimeFreezingToThaw", ref this.mTimeFreezingToThaw, Predicates.IsNotZero);
			s.StreamElementOpt("DefaultCryoPoints", ref this.mDefaultCryoPoints, Predicates.IsNotZero);
			s.StreamElementOpt("DefaultThawSpeed", ref this.mDefaultThawSpeed, Predicates.IsNotZero);
			s.StreamElementOpt("FreezingSpeedModifier", ref this.mFreezingSpeedModifier, Predicates.IsNotZero);
			s.StreamElementOpt("FreezingDamageModifier", ref this.mFreezingDamageModifier, Predicates.IsNotZero);
			s.StreamElementOpt("FrozenDamageModifier", ref this.mFrozenDamageModifier, Predicates.IsNotZero);
			#endregion
			using (s.EnterCursorBookmark("Dot"))
			{
				s.StreamAttributeOpt("small", ref this.mSmallDotSize, Predicates.IsNotZero);
				s.StreamAttributeOpt("medium", ref this.mMediumDotSize, Predicates.IsNotZero);
			}

			XML.XmlUtil.Serialize(s, this.CodeProtoObjects, kCodeProtoObjectsXmlParams);
			XML.XmlUtil.Serialize(s, this.CodeObjectTypes, kCodeObjectTypesXmlParams);
			XML.XmlUtil.Serialize(s, this.InfectionMap, BInfectionMap.kBListXmlParams);

			#region Nonsense
			XML.XmlUtil.Serialize(s, this.HUDItems, kHUDItemsXmlParams);
			XML.XmlUtil.Serialize(s, this.FlashableItems, kFlashableItemsXmlParams);
			XML.XmlUtil.Serialize(s, this.UnitFlags, kUnitFlagsXmlParams);
			XML.XmlUtil.Serialize(s, this.SquadFlags, kSquadFlagsXmlParams);
			#endregion
		}
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			using (s.EnterCursorBookmark(kXmlRoot))
				this.StreamGameData(s);
		}
		#endregion

		#region IProtoDataObjectDatabaseProvider members
		Engine.XmlFileInfo IProtoDataObjectDatabaseProvider.SourceFileReference { get { return kXmlFileInfo; } }

		Collections.IBTypeNames IProtoDataObjectDatabaseProvider.GetNamesInterface(int objectKind)
		{
			var kind = (GameDataObjectKind)objectKind;
			return this.GetNamesInterface(kind);
		}

		Collections.IHasUndefinedProtoMemberInterface IProtoDataObjectDatabaseProvider.GetMembersInterface(int objectKind)
		{
			var kind = (GameDataObjectKind)objectKind;
			return this.GetMembersInterface(kind);
		}
		#endregion
	};

	public sealed class BBurningEffectLimit
		: IO.ITagElementStringNameStreamable
		, IComparable<BBurningEffectLimit>
		, IEquatable<BBurningEffectLimit>
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams
		{
			ElementName = "BurningEffectLimitEntry",
		};
		#endregion

		#region Limit
		int mLimit;
		public int Limit
		{
			get { return this.mLimit; }
			set { this.mLimit = value; }
		}
		#endregion

		#region ObjectTypeID
		int mObjectTypeID = TypeExtensions.kNone;
		[Meta.UnitReference]
		public int ObjectTypeID
		{
			get { return this.mObjectTypeID; }
			set { this.mObjectTypeID = value; }
		}
		#endregion

		public bool IsInvalid { get { return PhxUtil.IsUndefinedReferenceHandleOrNone(this.ObjectTypeID); } }

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			s.StreamAttribute("Limit", ref this.mLimit);
			xs.StreamDBID(s, XML.XmlUtil.kNoXmlName, ref this.mObjectTypeID, DatabaseObjectKind.Unit, false, XML.XmlUtil.kSourceCursor);
		}
		#endregion

		#region IComparable Members
		public int CompareTo(BBurningEffectLimit other)
		{
			if (this.Limit != other.Limit)
				this.Limit.CompareTo(other.Limit);

			return this.ObjectTypeID.CompareTo(other.ObjectTypeID);
		}
		#endregion

		#region IEquatable Members
		public bool Equals(BBurningEffectLimit other)
		{
			return this.Limit == other.Limit
				&&
				this.ObjectTypeID == other.ObjectTypeID;
		}
		#endregion
	};
}