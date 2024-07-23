
namespace KSoft.Phoenix.Phx
{
	public sealed class BWeapon
		: Collections.BListAutoIdObject
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams
		{
			ElementName = "Weapon",
			DataName = "Name",
			Flags = XML.BCollectionXmlParamsFlags.UseElementForData |
				XML.BCollectionXmlParamsFlags.ForceNoRootElementStreaming,
		};
		#endregion

		#region Properties
		float mDamagePerSecond = PhxUtil.kInvalidSingle;
		public float DamagePerSecond { get { return mDamagePerSecond; } }
		float mDOTRate = PhxUtil.kInvalidSingle;
		public float DOTRate { get { return mDOTRate; } }
		float mDOTDuration = PhxUtil.kInvalidSingle;
		public float DOTDuration { get { return mDOTDuration; } }

		float mAttackRate = PhxUtil.kInvalidSingle;
		public float AttackRate { get { return mAttackRate; } }
		int mProjectileObjectID = TypeExtensions.kNone;
		public float ProjectileObjectID { get { return mProjectileObjectID; } }

		int mWeaponTypeID = TypeExtensions.kNone;
		public int WeaponTypeID { get { return mWeaponTypeID; } }
		int mVisualAmmo = TypeExtensions.kNone;
		public int VisualAmmo { get { return mVisualAmmo; } }
		int mTriggerScriptID = TypeExtensions.kNone;
		public int TriggerScriptID { get { return mTriggerScriptID; } }

		float mMinRange = PhxUtil.kInvalidSingle;
		public float MinRange { get { return mMinRange; } }
		float mMaxRange = PhxUtil.kInvalidSingle;
		public float MaxRange { get { return mMaxRange; } }

		float mReflectDamageFactor = PhxUtil.kInvalidSingle;
		public float ReflectDamageFactor { get { return mReflectDamageFactor; } }
		float mMovingAccuracy = PhxUtil.kInvalidSingle;
		public float MovingAccuracy { get { return mMovingAccuracy; } }
		float mMaxDeviation = PhxUtil.kInvalidSingle;
		public float MaxDeviation { get { return mMaxDeviation; } }
		float mMovingMaxDeviation = PhxUtil.kInvalidSingle;
		public float MovingMaxDeviation { get { return mMovingMaxDeviation; } }
		float mAccuracyDistanceFactor = PhxUtil.kInvalidSingle;
		public float AccuracyDistanceFactor { get { return mAccuracyDistanceFactor; } }
		float mAccuracyDeviationFactor = PhxUtil.kInvalidSingle;
		public float AccuracyDeviationFactor { get { return mAccuracyDeviationFactor; } }
		float mMaxVelocityLead = PhxUtil.kInvalidSingle;
		public float MaxVelocityLead { get { return mMaxVelocityLead; } }
		float mAirBurstSpan = PhxUtil.kInvalidSingle;
		public float AirBurstSpan { get { return mAirBurstSpan; } }

		public Collections.BTypeValues<BDamageRatingOverride> DamageOverrides { get; private set; } = new Collections.BTypeValues<BDamageRatingOverride>(BDamageRatingOverride.kBListParams);
		public Collections.BListArray<BTargetPriority> TargetPriorities { get; private set; } = new Collections.BListArray<BTargetPriority>();

		bool mStasisSmartTargeting;
		public bool StasisSmartTargeting { get { return mStasisSmartTargeting; } }
		float mStasisHealToDrainRatio = PhxUtil.kInvalidSingle;
		public float StasisHealToDrainRatio { get { return mStasisHealToDrainRatio; } }

		sbyte mBounces = TypeExtensions.kNone;
		public sbyte Bounces { get { return mBounces; } }
		float mBounceRange = PhxUtil.kInvalidSingle;
		public float BounceRange { get { return mBounceRange; } }

		float mMaxPullRange = PhxUtil.kInvalidSingle;
		public float MaxPullRange { get { return mMaxPullRange; } }
		#endregion

		#region BListAutoIdObject Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			var xs = s.GetSerializerInterface();

			s.StreamElementOpt("DamagePerSecond", ref mDamagePerSecond, PhxPredicates.IsNotInvalid);
			s.StreamElementOpt("DOTrate", ref mDOTRate, PhxPredicates.IsNotInvalid);
			s.StreamElementOpt("DOTduration", ref mDOTDuration, PhxPredicates.IsNotInvalid);

			s.StreamElementOpt("AttackRate", ref mAttackRate, PhxPredicates.IsNotInvalid);
			xs.StreamDBID(s, "Projectile", ref mProjectileObjectID, DatabaseObjectKind.Object);

			xs.StreamDBID(s, "WeaponType", ref mWeaponTypeID, DatabaseObjectKind.WeaponType);
			s.StreamElementOpt("VisualAmmo", ref mVisualAmmo, Predicates.IsNotNone);
			//TriggerScript

			s.StreamElementOpt("MinRange", ref mMinRange, PhxPredicates.IsNotInvalid);
			s.StreamElementOpt("MaxRange", ref mMaxRange, PhxPredicates.IsNotInvalid);

			s.StreamElementOpt("ReflectDamageFactor", ref mReflectDamageFactor, PhxPredicates.IsNotInvalid);
			s.StreamElementOpt("MovingAccuracy", ref mMovingAccuracy, PhxPredicates.IsNotInvalid);
			s.StreamElementOpt("MaxDeviation", ref mMaxDeviation, PhxPredicates.IsNotInvalid);
			s.StreamElementOpt("MovingMaxDeviation", ref mMovingMaxDeviation, PhxPredicates.IsNotInvalid);
			s.StreamElementOpt("AccuracyDistanceFactor", ref mAccuracyDistanceFactor, PhxPredicates.IsNotInvalid);
			s.StreamElementOpt("AccuracyDeviationFactor", ref mAccuracyDeviationFactor, PhxPredicates.IsNotInvalid);
			s.StreamElementOpt("MaxVelocityLead", ref mMaxVelocityLead, PhxPredicates.IsNotInvalid);
			s.StreamElementOpt("AirBurstSpan", ref mAirBurstSpan, PhxPredicates.IsNotInvalid);

			XML.XmlUtil.Serialize(s, DamageOverrides, BDamageRatingOverride.kBListXmlParams);
			XML.XmlUtil.Serialize(s, TargetPriorities, BTargetPriority.kBListXmlParams);

			using (var bm = s.EnterCursorBookmarkOpt("Stasis", this, o => o.mStasisSmartTargeting)) if (bm.IsNotNull)
				s.StreamAttribute("SmartTargeting", ref mStasisSmartTargeting);

			s.StreamElementOpt("StasisHealToDrainRatio", ref mStasisHealToDrainRatio, PhxPredicates.IsNotInvalid);

			s.StreamElementOpt("Bounces", ref mBounces, Predicates.IsNotNone);
			s.StreamElementOpt("BounceRange", ref mBounceRange, PhxPredicates.IsNotInvalid);

			s.StreamElementOpt("MaxPullRange", ref mMaxPullRange, PhxPredicates.IsNotInvalid);
		}
		#endregion
	};
}
