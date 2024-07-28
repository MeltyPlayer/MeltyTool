#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Phoenix.Phx
{
	// internal engine structure is only 0x34 bytes...
	public sealed partial class BProtoTechEffect
		: IO.ITagElementStringNameStreamable
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams("Effect")
		{
			Flags = 0
		};
		#endregion

		BProtoTechEffectType mType;
		public BProtoTechEffectType Type { get { return this.mType; } }

		DataUnion mDU;

		#region ObjectData
		bool mAllActions;

		string mAction;

		public BObjectDataType SubType { get { return this.mDU.SubType; } }

		// Amount can be negative, so use NaN as the 'invalid' value instead
		float mAmount = PhxUtil.kInvalidSingleNaN;

		BObjectDataRelative mRelativity = BObjectDataRelative.Invalid;

		#region Command
		public BProtoObjectCommandType CommandType { get {
			Contract.Requires(this.Type == BProtoTechEffectType.Data);
			Contract.Requires(this.SubType == BObjectDataType.CommandEnable || this.SubType == BObjectDataType.CommandSelectable);
			return this.mDU.CommandType;
		} }

		public int CommandDataID { get {
			Contract.Requires(this.Type == BProtoTechEffectType.Data);
			Contract.Requires(this.SubType == BObjectDataType.CommandEnable || this.SubType == BObjectDataType.CommandSelectable);
			return this.mDU.CommandData;
		} }
		public BSquadMode CommandDataSquadMode { get {
			Contract.Requires(this.Type == BProtoTechEffectType.Data);
			Contract.Requires(this.SubType == BObjectDataType.CommandEnable || this.SubType == BObjectDataType.CommandSelectable);
			return this.mDU.CommandDataSM;
		} }

		public DatabaseObjectKind CommandDataObjectKind { get {
			Contract.Requires(this.Type == BProtoTechEffectType.Data);
			Contract.Requires(this.SubType == BObjectDataType.CommandEnable || this.SubType == BObjectDataType.CommandSelectable);
			switch (this.mDU.CommandType)
			{
			case BProtoObjectCommandType.Research:		return DatabaseObjectKind.Tech;
			case BProtoObjectCommandType.TrainUnit:
			case BProtoObjectCommandType.Build:			return DatabaseObjectKind.Object;
			case BProtoObjectCommandType.TrainSquad:
			case BProtoObjectCommandType.BuildOther:	return DatabaseObjectKind.Squad;
			case BProtoObjectCommandType.Ability:		return DatabaseObjectKind.Ability;
			case BProtoObjectCommandType.Power:			return DatabaseObjectKind.Power;

			default: throw new KSoft.Debug.UnreachableException(this.mDU.CommandType.ToString());
			}
		} }
		#endregion
		#endregion

		public BProtoTechEffectSetAgeLevel SetAgeLevel { get { return this.mDU.SetAgeLevel; } }

		public Collections.BListArray<BProtoTechEffectTarget> Targets { get; private set; }
		public bool HasTargets { get { return this.Targets != null && this.Targets.Count != 0; } }

		public BProtoTechEffect()
		{
			this.Targets = new Collections.BListArray<BProtoTechEffectTarget>();

			this.mDU.Initialize();
		}

		#region ITagElementStreamable<string> Members
		public DatabaseObjectKind TransformProtoObjectKind { get {
			switch (this.Type)
			{
				case BProtoTechEffectType.TransformProtoUnit:
					return DatabaseObjectKind.Unit;
				case BProtoTechEffectType.TransformProtoSquad:
					return DatabaseObjectKind.Squad;
				default:
					return DatabaseObjectKind.None;
			}
		} }

		void StreamXmlObjectData<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, XML.BXmlSerializerInterface xs)
			where TDoc : class
			where TCursor : class
		{
			// Unused - SubTypes (with data) which no techs in HW1 made use of
			switch (this.mDU.SubType)
			{
			#region Unused
			case BObjectDataType.RateAmount:
			case BObjectDataType.RateMultiplier:
				xs.StreamTypeName(s, "Rate", ref this.mDU.ID, GameDataObjectKind.Rate, false, XML.XmlUtil.kSourceAttr);
				break;
			#endregion

			case BObjectDataType.CommandEnable:
			case BObjectDataType.CommandSelectable: // Unused
				this.mDU.StreamCommand(s, xs);
				break;

			case BObjectDataType.Cost:
				this.mDU.StreamCost(s, xs);
				break;

			#region Unused
			case BObjectDataType.DamageModifier:
				this.mDU.StreamDamageModifier(s, xs);
				break;
			#endregion

			case BObjectDataType.PopCap:
			case BObjectDataType.PopMax:
				// #NOTE engine parses this as "PopType", but its parser ignores case
				xs.StreamTypeName(s, "popType", ref this.mDU.ID, GameDataObjectKind.Pop, false, XML.XmlUtil.kSourceAttr);
				break;

			#region Unused
			case BObjectDataType.UnitTrainLimit:
				this.mDU.StreamTrainLimit(s, xs, DatabaseObjectKind.Object);
				break;
			case BObjectDataType.SquadTrainLimit:
				this.mDU.StreamTrainLimit(s, xs, DatabaseObjectKind.Squad);
				break;
			#endregion

			case BObjectDataType.PowerRechargeTime:
			case BObjectDataType.PowerUseLimit:
			case BObjectDataType.PowerLevel:
				// #NOTE engine parses this as "Power", but its parser ignores case
				xs.StreamDBID(s, "power", ref this.mDU.ID, DatabaseObjectKind.Power, false, XML.XmlUtil.kSourceAttr);
				break;
 			case BObjectDataType.ImpactEffect:
				// #NOTE engine parses this as "ImpactEffect", but its parser ignores case
				xs.StreamDBID(s, "impactEffect", ref this.mDU.ID, DatabaseObjectKind.ImpactEffect, false, XML.XmlUtil.kSourceAttr);
 				break;

			#region Unused
			case BObjectDataType.DisplayNameID:
				xs.StreamStringID(s, "StringID", ref this.mDU.ID, XML.XmlUtil.kSourceAttr);
				break;
			#endregion

			case BObjectDataType.Icon:
			// #NOTE engine actually doesn't explicitly handle this case when loading, but it is supported at runtime for Squads
			case BObjectDataType.AltIcon:
				this.mDU.StreamIcon(s, xs);
				break;

			case BObjectDataType.TurretYawRate:
 			case BObjectDataType.TurretPitchRate:
				// #TODO need to validate this type so that Targets.Count==1, TargetType=ProtoUnit, and resolve the Hardpoint name
				s.StreamStringOpt("Hardpoint", ref this.mDU.TurretRate_HardpointName, false);
 				break;

			case BObjectDataType.AbilityRecoverTime:
				xs.StreamDBID(s, "Ability", ref this.mDU.ID, DatabaseObjectKind.Ability, false, XML.XmlUtil.kSourceAttr);
				break;

			case BObjectDataType.HPBar:
				// #TODO need to make an BProtoHPBar reference
				s.StreamStringOpt("hpbar", ref this.mDU.HPBar_Name, false);
				break;

			#region Unused
			case BObjectDataType.DeathSpawn:
				xs.StreamDBID(s, "squadName", ref this.mDU.ID, DatabaseObjectKind.Squad, false, XML.XmlUtil.kSourceAttr);
				break;
			#endregion

			#region Object effects
			// although some apply to squads too
			case BObjectDataType.Enable: // Amount>0
			case BObjectDataType.Shieldpoints:
			case BObjectDataType.Hitpoints:
			case BObjectDataType.AmmoMax:
			case BObjectDataType.LOS:
			case BObjectDataType.MaximumVelocity:
			#region Weapon effects
			case BObjectDataType.MaximumRange:
			case BObjectDataType.Damage:
			case BObjectDataType.MinRange:
			case BObjectDataType.AOERadius:
			case BObjectDataType.AOEPrimaryTargetFactor:
			case BObjectDataType.AOEDistanceFactor:
			case BObjectDataType.AOEDamageFactor:
			case BObjectDataType.Accuracy:
			case BObjectDataType.MaxDeviation:
			case BObjectDataType.MovingMaxDeviation:
			case BObjectDataType.DataAccuracyDistanceFactor:
			case BObjectDataType.AccuracyDeviationFactor:
			case BObjectDataType.MaxVelocityLead:
			case BObjectDataType.MaxDamagePerRam:
			case BObjectDataType.ReflectDamageFactor:
			case BObjectDataType.AirBurstSpan:
			case BObjectDataType.DOTrate:
			case BObjectDataType.DOTduration:
			case BObjectDataType.Stasis:

			case BObjectDataType.Projectile:
			#endregion
			#region ProtoAction effects
			case BObjectDataType.WorkRate:
			case BObjectDataType.ActionEnable:
			case BObjectDataType.BoardTime:
			#endregion
			case BObjectDataType.BuildPoints:
			case BObjectDataType.AutoCloak: // Amount>0
			case BObjectDataType.MoveWhileCloaked: // Amount>0
			case BObjectDataType.AttackWhileCloaked: // Amount>0
			case BObjectDataType.Bounty:
			case BObjectDataType.MaxContained:
			case BObjectDataType.AbilityDisabled: // Amount>0
			case BObjectDataType.AmmoRegenRate:
			case BObjectDataType.ShieldRegenRate:
			case BObjectDataType.ShieldRegenDelay:
			#endregion
			#region Squad effects
			case BObjectDataType.Level:
			case BObjectDataType.TechLevel:
			#endregion
			case BObjectDataType.ResearchPoints: // Tech and TechAll only
			#region Player effects
			case BObjectDataType.ResourceTrickleRate:
			case BObjectDataType.BountyResource: // Amount!=0, uses Cost
			case BObjectDataType.RepairCost:
			case BObjectDataType.RepairTime:
			case BObjectDataType.WeaponPhysicsMultiplier:
			#endregion
			default:
				this.mDU.StreamCost(s, xs, isResourceOptional: true);
				break;
			}
		}
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			s.StreamAttributeEnum("type", ref this.mType);

			bool stream_targets = false;
			switch (this.mType)
			{
			case BProtoTechEffectType.Data:
				// #NOTE engine parses these as AllActions,Action,SubType,Amount,Relativity

				// e.g., SubType==Icon and these won't be used...TODO: is Icon the only one?
				s.StreamAttributeOpt("amount", ref this.mAmount, PhxPredicates.IsNotInvalidNaN);
				s.StreamAttributeEnum("subtype", ref this.mDU.SubType);
				// #NOTE the engine treats AllActions being present as 'true', no matter its actual value
				s.StreamAttributeOpt("allactions", ref this.mAllActions, Predicates.IsTrue);
				s.StreamStringOpt("action", ref this.mAction, false, intern: true);
				s.StreamAttributeEnumOpt("relativity", ref this.mRelativity, x => x != BObjectDataRelative.Invalid);
				this.StreamXmlObjectData(s, xs);
				stream_targets = true;
				break;
			case BProtoTechEffectType.TransformUnit:
			case BProtoTechEffectType.Build:
				xs.StreamDBID(s, XML.XmlUtil.kNoXmlName, ref this.mDU.ToTypeID, DatabaseObjectKind.Object, false, XML.XmlUtil.kSourceCursor);
				break;
			case BProtoTechEffectType.TransformProtoUnit:
			case BProtoTechEffectType.TransformProtoSquad:
				xs.StreamDBID(s, "FromType", ref this.mDU.FromTypeID, this.TransformProtoObjectKind, false, XML.XmlUtil.kSourceAttr);
				xs.StreamDBID(s, "ToType", ref this.mDU.ToTypeID, this.TransformProtoObjectKind, false, XML.XmlUtil.kSourceAttr);
				break;
			#region Unused
			case BProtoTechEffectType.SetAge:
				s.StreamCursorEnum(ref this.mDU.SetAgeLevel);
				break;
			#endregion
			case BProtoTechEffectType.GodPower:
				xs.StreamDBID(s, XML.XmlUtil.kNoXmlName, ref this.mDU.ID, DatabaseObjectKind.Power, false, XML.XmlUtil.kSourceCursor);
				s.StreamAttribute("amount", ref this.mAmount);
				break;
			#region Unused
			case BProtoTechEffectType.TechStatus:
				xs.StreamDBID(s, XML.XmlUtil.kNoXmlName, ref this.mDU.ID, DatabaseObjectKind.Tech, false, XML.XmlUtil.kSourceCursor);
				break;
			case BProtoTechEffectType.Ability:
				xs.StreamDBID(s, XML.XmlUtil.kNoXmlName, ref this.mDU.ID, DatabaseObjectKind.Ability, false, XML.XmlUtil.kSourceCursor);
				break;
 			case BProtoTechEffectType.SharedLOS: // no extra parsed data
 				break;
			case BProtoTechEffectType.AttachSquad:
				xs.StreamDBID(s, "squadType", ref this.mDU.ID, DatabaseObjectKind.Squad, false, XML.XmlUtil.kSourceAttr);
				stream_targets = true;
				break;
			#endregion
			}

			if (stream_targets)
				XML.XmlUtil.Serialize(s, this.Targets, BProtoTechEffectTarget.kBListXmlParams);
		}
		#endregion
	};
}