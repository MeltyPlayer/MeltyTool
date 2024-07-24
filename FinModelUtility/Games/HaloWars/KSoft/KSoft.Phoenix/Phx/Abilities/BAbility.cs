using System.Collections.Generic;

using BProtoObjectID = System.Int32;

namespace KSoft.Phoenix.Phx
{
	public sealed class BAbility
		: DatabaseNamedObject
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams
		{
			RootName = "Abilities",
			ElementName = "Ability",
			DataName = DatabaseNamedObject.kXmlAttrNameN,
		};
		public static readonly Engine.XmlFileInfo kXmlFileInfo = new Engine.XmlFileInfo
		{
			Directory = Engine.GameDirectory.Data,
			FileName = "Abilities.xml",
			RootName = kBListXmlParams.RootName
		};
		public static readonly Engine.ProtoDataXmlFileInfo kProtoFileInfo = new Engine.ProtoDataXmlFileInfo(
			Engine.XmlFilePriority.GameData,
			kXmlFileInfo);
		#endregion

		#region Type
		BAbilityType mType = BAbilityType.Invalid;
		public BAbilityType Type
		{
			get { return mType; }
			set { mType = value; }
		}
		#endregion

		#region AmmoCost
		float mAmmoCost;
		public float AmmoCost
		{
			get { return mAmmoCost; }
			set { mAmmoCost = value; }
		}
		#endregion

		[Meta.BProtoObjectReference]
		public List<BProtoObjectID> ObjectIDs { get; private set; }

		#region SquadMode
		BSquadMode mSquadMode = BSquadMode.Invalid;
		public BSquadMode SquadMode
		{
			get { return mSquadMode; }
			set { mSquadMode = value; }
		}
		#endregion

		#region RecoverStart
		BRecoverType mRecoverStart = BRecoverType.Move;
		public BRecoverType RecoverStart
		{
			get { return mRecoverStart; }
			set { mRecoverStart = value; }
		}
		#endregion

		#region RecoverType
		BRecoverType mRecoverType = BRecoverType.Move;
		public BRecoverType RecoverType
		{
			get { return mRecoverType; }
			set { mRecoverType = value; }
		}
		#endregion

		#region RecoverTime
		float mRecoverTime;
		public float RecoverTime
		{
			get { return mRecoverTime; }
			set { mRecoverTime = value; }
		}
		#endregion

		#region MovementSpeedModifier
		float mMovementSpeedModifier;
		public float MovementSpeedModifier
		{
			get { return mMovementSpeedModifier; }
			set { mMovementSpeedModifier = value; }
		}
		#endregion

		#region MovementModifierType
		BMovementModifierType mMovementModifierType = BMovementModifierType.Mode;
		public BMovementModifierType MovementModifierType
		{
			get { return mMovementModifierType; }
			set { mMovementModifierType = value; }
		}
		#endregion

		#region DamageTakenModifier
		float mDamageTakenModifier;
		public float DamageTakenModifier
		{
			get { return mDamageTakenModifier; }
			set { mDamageTakenModifier = value; }
		}
		#endregion

		#region DodgeModifier
		float mDodgeModifier;
		public float DodgeModifier
		{
			get { return mDodgeModifier; }
			set { mDodgeModifier = value; }
		}
		#endregion

		#region Icon
		string mIcon;
		[Meta.TextureReference]
		public string Icon
		{
			get { return mIcon; }
			set { mIcon = value; }
		}
		#endregion

		#region TargetType
		BAbilityTargetType mTargetType = BAbilityTargetType.None;
		public BAbilityTargetType TargetType
		{
			get { return mTargetType; }
			set { mTargetType = value; }
		}
		#endregion

		#region RecoverAnimAttachment
		string mRecoverAnimAttachment;
		[Meta.AttachmentTypeReference]
		public string RecoverAnimAttachment
		{
			get { return mRecoverAnimAttachment; }
			set { mRecoverAnimAttachment = value; }
		}
		#endregion

		#region RecoverStartAnim
		string mRecoverStartAnim;
		[Meta.BAnimTypeReference]
		public string RecoverStartAnim
		{
			get { return mRecoverStartAnim; }
			set { mRecoverStartAnim = value; }
		}
		#endregion

		#region RecoverEndAnim
		string mRecoverEndAnim;
		[Meta.BAnimTypeReference]
		public string RecoverEndAnim
		{
			get { return mRecoverEndAnim; }
			set { mRecoverEndAnim = value; }
		}
		#endregion

		#region Sprinting
		bool mSprinting;
		public bool Sprinting
		{
			get { return mSprinting; }
			set { mSprinting = value; }
		}
		#endregion

		#region DontInterruptAttack
		bool mDontInterruptAttack;
		public bool DontInterruptAttack
		{
			get { return mDontInterruptAttack; }
			set { mDontInterruptAttack = value; }
		}
		#endregion

		#region KeepSquadMode
		bool mKeepSquadMode;
		public bool KeepSquadMode
		{
			get { return mKeepSquadMode; }
			set { mKeepSquadMode = value; }
		}
		#endregion

		#region AttackSquadMode
		bool mAttackSquadMode;
		public bool AttackSquadMode
		{
			get { return mAttackSquadMode; }
			set { mAttackSquadMode = value; }
		}
		#endregion

		#region Duration
		float mDuration;
		public float Duration
		{
			get { return mDuration; }
			set { mDuration = value; }
		}
		#endregion

		#region SmartTargetRange
		const float cDefaultSmartTargetRange = 15.0f;

		float mSmartTargetRange = cDefaultSmartTargetRange;
		public float SmartTargetRange
		{
			get { return mSmartTargetRange; }
			set { mSmartTargetRange = value; }
		}
		#endregion

		#region CanHeteroCommand
		bool mCanHeteroCommand = true;
		public bool CanHeteroCommand
		{
			get { return mCanHeteroCommand; }
			set { mCanHeteroCommand = value; }
		}
		#endregion

		#region NoAbilityReticle
		bool mNoAbilityReticle;
		public bool NoAbilityReticle
		{
			get { return mNoAbilityReticle; }
			set { mNoAbilityReticle = value; }
		}
		#endregion

		public BAbility()
		{
			var textData = base.CreateDatabaseObjectUserInterfaceTextData();
			textData.HasDisplayNameID = true;
			textData.HasDisplayName2ID = true;
			textData.HasRolloverTextID = true;

			ObjectIDs = new List<BProtoObjectID>();
		}

		#region ITagElementStreamable Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);
			var xs = s.GetSerializerInterface();

			s.StreamElementEnumOpt("Type", ref mType, e => e != BAbilityType.Invalid);
			s.StreamElementOpt("AmmoCost", ref mAmmoCost, Predicates.IsNotZero);
			s.StreamElements("Object", ObjectIDs, xs, XML.BDatabaseXmlSerializerBase.StreamObjectID);
			s.StreamElementEnumOpt("SquadMode", ref mSquadMode, e => e != BSquadMode.Invalid);
			s.StreamElementEnumOpt("RecoverStart", ref mRecoverStart, e => e != BRecoverType.Move);
			s.StreamElementEnumOpt("RecoverType", ref mRecoverType, e => e != BRecoverType.Move);
			s.StreamElementOpt("RecoverTime", ref mRecoverTime, Predicates.IsNotZero);
			s.StreamElementOpt("MovementSpeedModifier", ref mMovementSpeedModifier, Predicates.IsNotZero);
			s.StreamElementEnumOpt("MovementModifierType", ref mMovementModifierType, e => e != BMovementModifierType.Mode);
			s.StreamElementOpt("DamageTakenModifier", ref mDamageTakenModifier, Predicates.IsNotZero);
			s.StreamElementOpt("DodgeModifier", ref mDodgeModifier, Predicates.IsNotZero);
			s.StreamStringOpt("Icon", ref mIcon, toLower: false, type: XML.XmlUtil.kSourceElement);
			s.StreamElementEnumOpt("TargetType", ref mTargetType, e => e != BAbilityTargetType.None);
			s.StreamStringOpt("RecoverAnimAttachment", ref mRecoverAnimAttachment, toLower: false, type: XML.XmlUtil.kSourceElement);
			s.StreamStringOpt("RecoverStartAnim", ref mRecoverStartAnim, toLower: false, type: XML.XmlUtil.kSourceElement);
			s.StreamStringOpt("RecoverEndAnim", ref mRecoverEndAnim, toLower: false, type: XML.XmlUtil.kSourceElement);
			s.StreamElementOpt("Sprinting", ref mSprinting, Predicates.IsTrue);
			s.StreamElementOpt("DontInterruptAttack", ref mDontInterruptAttack, Predicates.IsTrue);
			s.StreamElementOpt("KeepSquadMode", ref mKeepSquadMode, Predicates.IsTrue);
			s.StreamElementOpt("AttackSquadMode", ref mAttackSquadMode, Predicates.IsTrue);
			s.StreamElementOpt("Duration", ref mDuration, Predicates.IsNotZero);
			s.StreamElementOpt("SmartTargetRange", ref mSmartTargetRange, v => v != cDefaultSmartTargetRange);
			s.StreamElementOpt("CanHeteroCommand", ref mCanHeteroCommand, Predicates.IsFalse);
			s.StreamElementOpt("NoAbilityReticle", ref mNoAbilityReticle, Predicates.IsTrue);
		}
		#endregion
	};
}
