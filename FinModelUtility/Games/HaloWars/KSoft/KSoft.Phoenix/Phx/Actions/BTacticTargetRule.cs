using System.Collections.Generic;

using BDamageTypeID = System.Int32;
using BProtoUnitID = System.Int32; // object type or proto unit

namespace KSoft.Phoenix.Phx
{
	// BTargetRule
	public sealed class BTacticTargetRule
		: IO.ITagElementStringNameStreamable
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams
		{
			ElementName = "TargetRule",
			Flags = XML.BCollectionXmlParamsFlags.ForceNoRootElementStreaming
		};

		static readonly Collections.CodeEnum<BTargetRuleFlags> kFlagsProtoEnum = new Collections.CodeEnum<BTargetRuleFlags>();
		static readonly Collections.BBitSetParams kFlagsParams = new Collections.BBitSetParams(() => kFlagsProtoEnum);

		static readonly Collections.CodeEnum<BTargetRuleTargetStates> kTargetStatesProtoEnum = new Collections.CodeEnum<BTargetRuleTargetStates>();
		static readonly Collections.BBitSetParams kTargetStatesParams = new Collections.BBitSetParams(() => kTargetStatesProtoEnum);
		static readonly XML.BBitSetXmlParams kTargetStatesXmlParams = new XML.BBitSetXmlParams("TargetState");
		#endregion

		#region Relation
		BRelationType mRelation = BRelationType.Enemy;
		public BRelationType Relation
		{
			get { return mRelation; }
			set { mRelation = value; }
		}
		#endregion

		#region SquadMode
		BSquadMode mSquadMode = BSquadMode.Invalid;
		public BSquadMode SquadMode
		{
			get { return mSquadMode; }
			set { mSquadMode = value; }
		}

		public bool AutoTargetSquadMode { get; private set; }
		#endregion

		[Meta.BDamageTypeReference]
		public List<BDamageTypeID> DamageTypes { get; private set; } = new List<BDamageTypeID>();

		[Meta.UnitReference]
		public List<BProtoUnitID> TargetTypes { get; private set; } = new List<BProtoUnitID>();

		#region ActionID
		int mActionID = TypeExtensions.kNone;
		[Meta.BProtoActionReference]
		public int ActionID
		{
			get { return mActionID; }
			set { mActionID = value; }
		}
		#endregion

		public Collections.BBitSet Flags { get; private set; } = new Collections.BBitSet(kFlagsParams);
		public Collections.BBitSet TargetStates { get; private set; } = new Collections.BBitSet(kTargetStatesParams);

		#region AbilityID
		int mAbilityID = TypeExtensions.kNone;
		[Meta.BAbilityReference]
		public int AbilityID
		{
			get { return mAbilityID; }
			set { mAbilityID = value; }
		}

		public bool IsOptionalAbility { get; private set; }
		#endregion

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();
			var td = KSoft.Debug.TypeCheck.CastReference<BTacticData>(s.UserData);

			s.StreamElementEnumOpt("Relation", ref mRelation, e => e != BRelationType.Enemy);
			if (!s.StreamElementEnumOpt("SquadMode", ref mSquadMode, e => e != BSquadMode.Invalid))
				if (s.StreamElementEnumOpt("AutoTargetSquadMode", ref mSquadMode, e => e != BSquadMode.Invalid))
					AutoTargetSquadMode = true;

			s.StreamElements("DamageType", DamageTypes, xs, XML.BDatabaseXmlSerializerBase.StreamDamageType);
			s.StreamElements("TargetType", TargetTypes, xs, XML.BDatabaseXmlSerializerBase.StreamUnitID);

			td.StreamID(s, "Action", ref mActionID, TacticDataObjectKind.Action);

			XML.XmlUtil.Serialize(s, Flags, XML.BBitSetXmlParams.kFlagsAreElementNamesThatMeanTrue);
			XML.XmlUtil.Serialize(s, TargetStates, kTargetStatesXmlParams);

			if (!xs.StreamDBID(s, "Ability", ref mAbilityID, DatabaseObjectKind.Ability))
				IsOptionalAbility = xs.StreamDBID(s, "OptionalAbility", ref mAbilityID, DatabaseObjectKind.Ability);
		}
		#endregion
	};
}
