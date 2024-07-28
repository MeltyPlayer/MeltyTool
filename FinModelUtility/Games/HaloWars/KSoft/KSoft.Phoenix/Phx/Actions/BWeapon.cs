
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
		public float DamagePerSecond { get { return this.mDamagePerSecond; } }
		float mDOTRate = PhxUtil.kInvalidSingle;
		public float DOTRate { get { return this.mDOTRate; } }
		float mDOTDuration = PhxUtil.kInvalidSingle;
		public float DOTDuration { get { return this.mDOTDuration; } }

		float mAttackRate = PhxUtil.kInvalidSingle;
		public float AttackRate { get { return this.mAttackRate; } }
		int mProjectileObjectID = TypeExtensions.kNone;
		public float ProjectileObjectID { get { return this.mProjectileObjectID; } }

		int mWeaponTypeID = TypeExtensions.kNone;
		public int WeaponTypeID { get { return this.mWeaponTypeID; } }
		int mVisualAmmo = TypeExtensions.kNone;
		public int VisualAmmo { get { return this.mVisualAmmo; } }
		int mTriggerScriptID = TypeExtensions.kNone;
		public int TriggerScriptID { get { return this.mTriggerScriptID; } }

		float mMinRange = PhxUtil.kInvalidSingle;
		public float MinRange { get { return this.mMinRange; } }
		float mMaxRange = PhxUtil.kInvalidSingle;
		public float MaxRange { get { return this.mMaxRange; } }

		float mReflectDamageFactor = PhxUtil.kInvalidSingle;
		public float ReflectDamageFactor { get { return this.mReflectDamageFactor; } }
		float mMovingAccuracy = PhxUtil.kInvalidSingle;
		public float MovingAccuracy { get { return this.mMovingAccuracy; } }
		float mMaxDeviation = PhxUtil.kInvalidSingle;
		public float MaxDeviation { get { return this.mMaxDeviation; } }
		float mMovingMaxDeviation = PhxUtil.kInvalidSingle;
		public float MovingMaxDeviation { get { return this.mMovingMaxDeviation; } }
		float mAccuracyDistanceFactor = PhxUtil.kInvalidSingle;
		public float AccuracyDistanceFactor { get { return this.mAccuracyDistanceFactor; } }
		float mAccuracyDeviationFactor = PhxUtil.kInvalidSingle;
		public float AccuracyDeviationFactor { get { return this.mAccuracyDeviationFactor; } }
		float mMaxVelocityLead = PhxUtil.kInvalidSingle;
		public float MaxVelocityLead { get { return this.mMaxVelocityLead; } }
		float mAirBurstSpan = PhxUtil.kInvalidSingle;
		public float AirBurstSpan { get { return this.mAirBurstSpan; } }

		public Collections.BTypeValues<BDamageRatingOverride> DamageOverrides { get; private set; } = new Collections.BTypeValues<BDamageRatingOverride>(BDamageRatingOverride.kBListParams);
		public Collections.BListArray<BTargetPriority> TargetPriorities { get; private set; } = new Collections.BListArray<BTargetPriority>();

		bool mStasisSmartTargeting;
		public bool StasisSmartTargeting { get { return this.mStasisSmartTargeting; } }
		float mStasisHealToDrainRatio = PhxUtil.kInvalidSingle;
		public float StasisHealToDrainRatio { get { return this.mStasisHealToDrainRatio; } }

		sbyte mBounces = TypeExtensions.kNone;
		public sbyte Bounces { get { return this.mBounces; } }
		float mBounceRange = PhxUtil.kInvalidSingle;
		public float BounceRange { get { return this.mBounceRange; } }

		float mMaxPullRange = PhxUtil.kInvalidSingle;
		public float MaxPullRange { get { return this.mMaxPullRange; } }
		#endregion

		#region BListAutoIdObject Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			var xs = s.GetSerializerInterface();

			s.StreamElementOpt("DamagePerSecond", ref this.mDamagePerSecond, PhxPredicates.IsNotInvalid);
			s.StreamElementOpt("DOTrate", ref this.mDOTRate, PhxPredicates.IsNotInvalid);
			s.StreamElementOpt("DOTduration", ref this.mDOTDuration, PhxPredicates.IsNotInvalid);

			s.StreamElementOpt("AttackRate", ref this.mAttackRate, PhxPredicates.IsNotInvalid);
			xs.StreamDBID(s, "Projectile", ref this.mProjectileObjectID, DatabaseObjectKind.Object);

			xs.StreamDBID(s, "WeaponType", ref this.mWeaponTypeID, DatabaseObjectKind.WeaponType);
			s.StreamElementOpt("VisualAmmo", ref this.mVisualAmmo, Predicates.IsNotNone);
			//TriggerScript

			s.StreamElementOpt("MinRange", ref this.mMinRange, PhxPredicates.IsNotInvalid);
			s.StreamElementOpt("MaxRange", ref this.mMaxRange, PhxPredicates.IsNotInvalid);

			s.StreamElementOpt("ReflectDamageFactor", ref this.mReflectDamageFactor, PhxPredicates.IsNotInvalid);
			s.StreamElementOpt("MovingAccuracy", ref this.mMovingAccuracy, PhxPredicates.IsNotInvalid);
			s.StreamElementOpt("MaxDeviation", ref this.mMaxDeviation, PhxPredicates.IsNotInvalid);
			s.StreamElementOpt("MovingMaxDeviation", ref this.mMovingMaxDeviation, PhxPredicates.IsNotInvalid);
			s.StreamElementOpt("AccuracyDistanceFactor", ref this.mAccuracyDistanceFactor, PhxPredicates.IsNotInvalid);
			s.StreamElementOpt("AccuracyDeviationFactor", ref this.mAccuracyDeviationFactor, PhxPredicates.IsNotInvalid);
			s.StreamElementOpt("MaxVelocityLead", ref this.mMaxVelocityLead, PhxPredicates.IsNotInvalid);
			s.StreamElementOpt("AirBurstSpan", ref this.mAirBurstSpan, PhxPredicates.IsNotInvalid);

			XML.XmlUtil.Serialize(s, this.DamageOverrides, BDamageRatingOverride.kBListXmlParams);
			XML.XmlUtil.Serialize(s, this.TargetPriorities, BTargetPriority.kBListXmlParams);

			using (var bm = s.EnterCursorBookmarkOpt("Stasis", this, o => o.mStasisSmartTargeting)) if (bm.IsNotNull)
				s.StreamAttribute("SmartTargeting", ref this.mStasisSmartTargeting);

			s.StreamElementOpt("StasisHealToDrainRatio", ref this.mStasisHealToDrainRatio, PhxPredicates.IsNotInvalid);

			s.StreamElementOpt("Bounces", ref this.mBounces, Predicates.IsNotNone);
			s.StreamElementOpt("BounceRange", ref this.mBounceRange, PhxPredicates.IsNotInvalid);

			s.StreamElementOpt("MaxPullRange", ref this.mMaxPullRange, PhxPredicates.IsNotInvalid);
		}
		#endregion
	};
}
