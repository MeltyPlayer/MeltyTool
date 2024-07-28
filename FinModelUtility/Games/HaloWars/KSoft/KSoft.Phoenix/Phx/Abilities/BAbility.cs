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
			DataName = kXmlAttrNameN,
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
			get { return this.mType; }
			set { this.mType = value; }
		}
		#endregion

		#region AmmoCost
		float mAmmoCost;
		public float AmmoCost
		{
			get { return this.mAmmoCost; }
			set { this.mAmmoCost = value; }
		}
		#endregion

		[Meta.BProtoObjectReference]
		public List<BProtoObjectID> ObjectIDs { get; private set; }

		#region SquadMode
		BSquadMode mSquadMode = BSquadMode.Invalid;
		public BSquadMode SquadMode
		{
			get { return this.mSquadMode; }
			set { this.mSquadMode = value; }
		}
		#endregion

		#region RecoverStart
		BRecoverType mRecoverStart = BRecoverType.Move;
		public BRecoverType RecoverStart
		{
			get { return this.mRecoverStart; }
			set { this.mRecoverStart = value; }
		}
		#endregion

		#region RecoverType
		BRecoverType mRecoverType = BRecoverType.Move;
		public BRecoverType RecoverType
		{
			get { return this.mRecoverType; }
			set { this.mRecoverType = value; }
		}
		#endregion

		#region RecoverTime
		float mRecoverTime;
		public float RecoverTime
		{
			get { return this.mRecoverTime; }
			set { this.mRecoverTime = value; }
		}
		#endregion

		#region MovementSpeedModifier
		float mMovementSpeedModifier;
		public float MovementSpeedModifier
		{
			get { return this.mMovementSpeedModifier; }
			set { this.mMovementSpeedModifier = value; }
		}
		#endregion

		#region MovementModifierType
		BMovementModifierType mMovementModifierType = BMovementModifierType.Mode;
		public BMovementModifierType MovementModifierType
		{
			get { return this.mMovementModifierType; }
			set { this.mMovementModifierType = value; }
		}
		#endregion

		#region DamageTakenModifier
		float mDamageTakenModifier;
		public float DamageTakenModifier
		{
			get { return this.mDamageTakenModifier; }
			set { this.mDamageTakenModifier = value; }
		}
		#endregion

		#region DodgeModifier
		float mDodgeModifier;
		public float DodgeModifier
		{
			get { return this.mDodgeModifier; }
			set { this.mDodgeModifier = value; }
		}
		#endregion

		#region Icon
		string mIcon;
		[Meta.TextureReference]
		public string Icon
		{
			get { return this.mIcon; }
			set { this.mIcon = value; }
		}
		#endregion

		#region TargetType
		BAbilityTargetType mTargetType = BAbilityTargetType.None;
		public BAbilityTargetType TargetType
		{
			get { return this.mTargetType; }
			set { this.mTargetType = value; }
		}
		#endregion

		#region RecoverAnimAttachment
		string mRecoverAnimAttachment;
		[Meta.AttachmentTypeReference]
		public string RecoverAnimAttachment
		{
			get { return this.mRecoverAnimAttachment; }
			set { this.mRecoverAnimAttachment = value; }
		}
		#endregion

		#region RecoverStartAnim
		string mRecoverStartAnim;
		[Meta.BAnimTypeReference]
		public string RecoverStartAnim
		{
			get { return this.mRecoverStartAnim; }
			set { this.mRecoverStartAnim = value; }
		}
		#endregion

		#region RecoverEndAnim
		string mRecoverEndAnim;
		[Meta.BAnimTypeReference]
		public string RecoverEndAnim
		{
			get { return this.mRecoverEndAnim; }
			set { this.mRecoverEndAnim = value; }
		}
		#endregion

		#region Sprinting
		bool mSprinting;
		public bool Sprinting
		{
			get { return this.mSprinting; }
			set { this.mSprinting = value; }
		}
		#endregion

		#region DontInterruptAttack
		bool mDontInterruptAttack;
		public bool DontInterruptAttack
		{
			get { return this.mDontInterruptAttack; }
			set { this.mDontInterruptAttack = value; }
		}
		#endregion

		#region KeepSquadMode
		bool mKeepSquadMode;
		public bool KeepSquadMode
		{
			get { return this.mKeepSquadMode; }
			set { this.mKeepSquadMode = value; }
		}
		#endregion

		#region AttackSquadMode
		bool mAttackSquadMode;
		public bool AttackSquadMode
		{
			get { return this.mAttackSquadMode; }
			set { this.mAttackSquadMode = value; }
		}
		#endregion

		#region Duration
		float mDuration;
		public float Duration
		{
			get { return this.mDuration; }
			set { this.mDuration = value; }
		}
		#endregion

		#region SmartTargetRange
		const float cDefaultSmartTargetRange = 15.0f;

		float mSmartTargetRange = cDefaultSmartTargetRange;
		public float SmartTargetRange
		{
			get { return this.mSmartTargetRange; }
			set { this.mSmartTargetRange = value; }
		}
		#endregion

		#region CanHeteroCommand
		bool mCanHeteroCommand = true;
		public bool CanHeteroCommand
		{
			get { return this.mCanHeteroCommand; }
			set { this.mCanHeteroCommand = value; }
		}
		#endregion

		#region NoAbilityReticle
		bool mNoAbilityReticle;
		public bool NoAbilityReticle
		{
			get { return this.mNoAbilityReticle; }
			set { this.mNoAbilityReticle = value; }
		}
		#endregion

		public BAbility()
		{
			var textData = this.CreateDatabaseObjectUserInterfaceTextData();
			textData.HasDisplayNameID = true;
			textData.HasDisplayName2ID = true;
			textData.HasRolloverTextID = true;

			this.ObjectIDs = new List<BProtoObjectID>();
		}

		#region ITagElementStreamable Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);
			var xs = s.GetSerializerInterface();

			s.StreamElementEnumOpt("Type", ref this.mType, e => e != BAbilityType.Invalid);
			s.StreamElementOpt("AmmoCost", ref this.mAmmoCost, Predicates.IsNotZero);
			s.StreamElements("Object", this.ObjectIDs, xs, XML.BXmlSerializerInterface.StreamObjectID);
			s.StreamElementEnumOpt("SquadMode", ref this.mSquadMode, e => e != BSquadMode.Invalid);
			s.StreamElementEnumOpt("RecoverStart", ref this.mRecoverStart, e => e != BRecoverType.Move);
			s.StreamElementEnumOpt("RecoverType", ref this.mRecoverType, e => e != BRecoverType.Move);
			s.StreamElementOpt("RecoverTime", ref this.mRecoverTime, Predicates.IsNotZero);
			s.StreamElementOpt("MovementSpeedModifier", ref this.mMovementSpeedModifier, Predicates.IsNotZero);
			s.StreamElementEnumOpt("MovementModifierType", ref this.mMovementModifierType, e => e != BMovementModifierType.Mode);
			s.StreamElementOpt("DamageTakenModifier", ref this.mDamageTakenModifier, Predicates.IsNotZero);
			s.StreamElementOpt("DodgeModifier", ref this.mDodgeModifier, Predicates.IsNotZero);
			s.StreamStringOpt("Icon", ref this.mIcon, toLower: false, type: XML.XmlUtil.kSourceElement);
			s.StreamElementEnumOpt("TargetType", ref this.mTargetType, e => e != BAbilityTargetType.None);
			s.StreamStringOpt("RecoverAnimAttachment", ref this.mRecoverAnimAttachment, toLower: false, type: XML.XmlUtil.kSourceElement);
			s.StreamStringOpt("RecoverStartAnim", ref this.mRecoverStartAnim, toLower: false, type: XML.XmlUtil.kSourceElement);
			s.StreamStringOpt("RecoverEndAnim", ref this.mRecoverEndAnim, toLower: false, type: XML.XmlUtil.kSourceElement);
			s.StreamElementOpt("Sprinting", ref this.mSprinting, Predicates.IsTrue);
			s.StreamElementOpt("DontInterruptAttack", ref this.mDontInterruptAttack, Predicates.IsTrue);
			s.StreamElementOpt("KeepSquadMode", ref this.mKeepSquadMode, Predicates.IsTrue);
			s.StreamElementOpt("AttackSquadMode", ref this.mAttackSquadMode, Predicates.IsTrue);
			s.StreamElementOpt("Duration", ref this.mDuration, Predicates.IsNotZero);
			s.StreamElementOpt("SmartTargetRange", ref this.mSmartTargetRange, v => v != cDefaultSmartTargetRange);
			s.StreamElementOpt("CanHeteroCommand", ref this.mCanHeteroCommand, Predicates.IsFalse);
			s.StreamElementOpt("NoAbilityReticle", ref this.mNoAbilityReticle, Predicates.IsTrue);
		}
		#endregion
	};
}
