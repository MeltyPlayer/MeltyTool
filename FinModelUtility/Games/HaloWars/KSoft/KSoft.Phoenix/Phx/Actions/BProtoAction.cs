using System;

namespace KSoft.Phoenix.Phx
{
	public sealed partial class BProtoAction
		: Collections.BListAutoIdObject
	{
		public static readonly Predicate<BActionType> kNotInvalidActionType = e => e != BActionType.Invalid;
		static readonly Predicate<BSquadMode> kNotInvalidSquadMode = e => e != BSquadMode.Invalid;

		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams
		{
			ElementName = "Action",
			DataName = "Name",
			Flags = XML.BCollectionXmlParamsFlags.UseElementForData |
				XML.BCollectionXmlParamsFlags.ForceNoRootElementStreaming,
		};
		#endregion

		#region Properties
		BActionType mActionType = BActionType.Invalid;
		float mProjectileSpread = PhxUtil.kInvalidSingle;

		[Meta.BProtoSquadReference]
		int mSquadTypeID = TypeExtensions.kNone;
		[Meta.BWeaponTypeReference]
		int mWeaponID = TypeExtensions.kNone;
		[Meta.BProtoActionReference]
		int mLinkedActionID = TypeExtensions.kNone;

		BSquadMode mSquadMode = BSquadMode.Invalid;
		BSquadMode mNewSquadMode = BSquadMode.Invalid;
#if false
		int mNewTacticStateID = TypeExtensions.kNone;
#endif

		float mWorkRate = PhxUtil.kInvalidSingle;
		float mWorkRateVariance = PhxUtil.kInvalidSingle;
		float mWorkRange = PhxUtil.kInvalidSingle;

		float mDamageModifiersDmg;
		float mDamageModifiersDmgTaken;
		bool mDamageModifiersByCombatValue;

		int mResourceID = TypeExtensions.kNone;
		bool mDefault;

		int mSlaveAttackActionID = TypeExtensions.kNone;
		int mBaseDPSWeaponID = TypeExtensions.kNone;

		BActionType mPersistentActionType = BActionType.Invalid;

		float mDuration = PhxUtil.kInvalidSingle;
		float mDurationSpread = PhxUtil.kInvalidSingle;

		int mAutoRepairIdleTime = TypeExtensions.kNone;
		float mAutoRepairThreshold = PhxUtil.kInvalidSingle;
		float mAutoRepairSearchDistance = PhxUtil.kInvalidSingle;
		int mInvalidTargetObjectID = TypeExtensions.kNone;

		int mProtoObjectID = TypeExtensions.kNone;
		bool mProtoObjectIsSquad;
#if false
		int mCountStringID = TypeExtensions.kNone;
#endif
		int mMaxNumUnitsPerformAction = TypeExtensions.kNone;
		float mDamageCharge = PhxUtil.kInvalidSingle;
		#endregion

		#region BListAutoIdObject Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			var xs = s.GetSerializerInterface();
			var td = KSoft.Debug.TypeCheck.CastReference<BTacticData>(s.UserData);

			s.StreamElementEnumOpt("ActionType", ref this.mActionType, kNotInvalidActionType);
			s.StreamElementOpt("ProjectileSpread", ref this.mProjectileSpread, PhxPredicates.IsNotInvalid);

			xs.StreamDBID(s, "SquadType", ref this.mSquadTypeID, DatabaseObjectKind.Squad);
			td.StreamID(s, "Weapon", ref this.mWeaponID, TacticDataObjectKind.Weapon);
			td.StreamID(s, "LinkedAction", ref this.mLinkedActionID, TacticDataObjectKind.Action);

			s.StreamElementEnumOpt("SquadMode", ref this.mSquadMode, kNotInvalidSquadMode);
			s.StreamElementEnumOpt("NewSquadMode", ref this.mNewSquadMode, kNotInvalidSquadMode);
#if false
			td.StreamID(s, "NewTacticState", ref mNewTacticStateID, BTacticData.ObjectKind.TacticState);
#endif

			#region Work
			s.StreamElementOpt("WorkRate", ref this.mWorkRate, PhxPredicates.IsNotInvalid);
			s.StreamElementOpt("WorkRateVariance", ref this.mWorkRateVariance, PhxPredicates.IsNotInvalid);
			s.StreamElementOpt("WorkRange", ref this.mWorkRange, PhxPredicates.IsNotInvalid);
			#endregion

			#region DamageModifiers
			using (var bm = s.EnterCursorBookmarkOpt("DamageModifiers", this, o => this.mDamageModifiersDmg != PhxUtil.kInvalidSingle)) if (bm.IsNotNull)
			{
				s.StreamAttribute("damage", ref this.mDamageModifiersDmg);
				s.StreamAttributeOpt("damageTaken", ref this.mDamageModifiersDmgTaken, PhxPredicates.IsNotInvalid);
				s.StreamAttributeOpt("byCombatValue", ref this.mDamageModifiersByCombatValue, Predicates.IsTrue);
			}
			#endregion

			xs.StreamTypeName(s, "Resource", ref this.mResourceID, GameDataObjectKind.Cost);
			// if element equals 'true' this is the default action
			s.StreamElementOpt("Default", ref this.mDefault, Predicates.IsTrue);

			td.StreamID(s, "SlaveAttackAction", ref this.mSlaveAttackActionID, TacticDataObjectKind.Action);
			td.StreamID(s, "BaseDPSWeapon", ref this.mBaseDPSWeaponID, TacticDataObjectKind.Weapon);

			s.StreamElementEnumOpt("PersistentActionType", ref this.mPersistentActionType, kNotInvalidActionType);

			#region Duration
			using (var bm = s.EnterCursorBookmarkOpt("Duration", this, o => this.mDuration != PhxUtil.kInvalidSingle)) if (bm.IsNotNull)
			{
				s.StreamCursor(ref this.mDuration);
				s.StreamAttributeOpt("DurationSpread", ref this.mDurationSpread, PhxPredicates.IsNotInvalid);
			}
			#endregion

			#region AutoRepair
			using (var bm = s.EnterCursorBookmarkOpt("AutoRepair", this, o => this.mAutoRepairIdleTime != PhxUtil.kInvalidSingle)) if (bm.IsNotNull)
			{
				s.StreamAttribute("AutoRepairIdleTime", ref this.mAutoRepairIdleTime);
				s.StreamAttribute("AutoRepairThreshold", ref this.mAutoRepairThreshold);
				s.StreamAttribute("AutoRepairSearchDistance", ref this.mAutoRepairSearchDistance);
			}
			#endregion
			xs.StreamDBID(s, "InvalidTarget", ref this.mInvalidTargetObjectID, DatabaseObjectKind.Object);

			#region ProtoObject
			using (var bm = s.EnterCursorBookmarkOpt("ProtoObject", this, o => this.mProtoObjectID.IsNotNone())) if (bm.IsNotNull)
			{
				// TODO: This IS optional, right? Only on 'true'?
				// inner text: if 0, proto object, if not, proto squad
				s.StreamAttributeOpt("Squad", ref this.mProtoObjectIsSquad, Predicates.IsTrue);
				xs.StreamDBID(s, null, ref this.mSquadTypeID,
				              this.mProtoObjectIsSquad ? DatabaseObjectKind.Squad : DatabaseObjectKind.Object, false, XML.XmlUtil.kSourceCursor);
			}
			#endregion
#if false
			xs.StreamXmlForStringID(s, "Count", ref mCountStringID);
#endif
			s.StreamElementOpt("MaxNumUnitsPerformAction", ref this.mMaxNumUnitsPerformAction, Predicates.IsNotNone);
			s.StreamElementOpt("DamageCharge", ref this.mDamageCharge, PhxPredicates.IsNotInvalid);
		}
		#endregion
	};
}