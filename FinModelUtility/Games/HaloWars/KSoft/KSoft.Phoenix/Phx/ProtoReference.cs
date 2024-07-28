using System;
using Contracts = System.Diagnostics.Contracts;

namespace KSoft.Phoenix.Phx.Meta
{
	public interface IProtoDataReferenceAttribute
	{
		ProtoDataObjectSourceKind ObjectSourceKind { get; }
		Type ProtoType { get; }
		string ProtoKindName { get; }
		int ProtoKindId { get; }
	};

	[AttributeUsage(kValidOn, AllowMultiple = false)]
	public abstract class ProtoDataReferenceAttribute
		: Attribute
	{
		public const AttributeTargets kValidOn = 0
			| AttributeTargets.Field
			| AttributeTargets.Property
			| AttributeTargets.Parameter
			| AttributeTargets.ReturnValue
			| AttributeTargets.GenericParameter
			;

		public abstract ProtoDataObjectSourceKind ObjectSourceKind { get; }
		public abstract Type ProtoType { get; }
	};

	/// <summary>For fields in ProtoData that are not actually used in any meaningful way</summary>
	[AttributeUsage(ProtoDataReferenceAttribute.kValidOn, AllowMultiple = false)]
	public sealed class UnusedDataAttribute : Attribute
	{
		public UnusedDataAttribute() { }
		public UnusedDataAttribute(string note) { }
	};

	/// <summary>Localized string reference</summary>
	[AttributeUsage(ProtoDataReferenceAttribute.kValidOn, AllowMultiple=false)]
	public sealed class LocStringReferenceAttribute : Attribute
	{
	};

	#region ProtoFileReferences
	[AttributeUsage(ProtoDataReferenceAttribute.kValidOn, AllowMultiple=false)]
	public abstract class ProtoFileReferenceAttribute : Attribute
	{
		public abstract string FileExtension { get; }
	};

	/// <summary>DDX reference</summary>
	[AttributeUsage(ProtoDataReferenceAttribute.kValidOn, AllowMultiple=false)]
	public sealed class TextureReferenceAttribute : ProtoFileReferenceAttribute
	{
		public override string FileExtension { get { return "ddx"; } }
	};

	/// <summary>.physics reference</summary>
	[AttributeUsage(ProtoDataReferenceAttribute.kValidOn, AllowMultiple = false)]
	public sealed class PhysicsInfoReferenceAttribute : ProtoFileReferenceAttribute
	{
		public override string FileExtension { get { return "physics"; } }
	};

	/// <summary>.TriggerScript reference</summary>
	[AttributeUsage(ProtoDataReferenceAttribute.kValidOn, AllowMultiple = false)]
	public sealed class TriggerScriptReferenceAttribute : ProtoFileReferenceAttribute
	{
		public override string FileExtension { get { return "triggerscript"; } }
	};

	/// <summary>.vis reference</summary>
	[AttributeUsage(ProtoDataReferenceAttribute.kValidOn, AllowMultiple = false)]
	public sealed class VisualReferenceAttribute : ProtoFileReferenceAttribute
	{
		public override string FileExtension { get { return "vis"; } }
	};
	#endregion

	[AttributeUsage(ProtoDataReferenceAttribute.kValidOn, AllowMultiple = false)]
	public sealed class SoundCueReferenceAttribute : Attribute
	{
	};

	[AttributeUsage(ProtoDataReferenceAttribute.kValidOn, AllowMultiple = false)]
	public sealed class BAnimTypeReferenceAttribute : Attribute
	{
	};
	[AttributeUsage(ProtoDataReferenceAttribute.kValidOn, AllowMultiple = false)]
	public sealed class AttachmentTypeReferenceAttribute : Attribute
	{
	};

	[AttributeUsage(ProtoDataReferenceAttribute.kValidOn, AllowMultiple = false)]
	public sealed class CameraEffectReferenceAttribute : Attribute
	{
	};

	/// <summary>Reference to an Action in a Tactic</summary>
	[AttributeUsage(ProtoDataReferenceAttribute.kValidOn, AllowMultiple = false)]
	public sealed class BProtoActionReferenceAttribute : Attribute
	{
	};

	#region GameData
	[AttributeUsage(kValidOn, AllowMultiple = false)]
	public abstract class GameDataObjectReferenceAttribute
		: ProtoDataReferenceAttribute
		, IProtoDataReferenceAttribute
	{
		public override ProtoDataObjectSourceKind ObjectSourceKind { get { return ProtoDataObjectSourceKind.GameData; } }

		public abstract GameDataObjectKind ProtoKind { get; }

		Type IProtoDataReferenceAttribute.ProtoType { get { return this.ProtoType; } }
		string IProtoDataReferenceAttribute.ProtoKindName { get { return this.ProtoKind.ToString(); } }
		int IProtoDataReferenceAttribute.ProtoKindId { get { return (int) this.ProtoKind; } }
	};

	/// <summary>Cost/Resource type reference</summary>
	[AttributeUsage(kValidOn, AllowMultiple = false)]
	public sealed class ResourceReferenceAttribute : GameDataObjectReferenceAttribute
	{
		public override Type ProtoType { get { return typeof(string); } }
		public override GameDataObjectKind ProtoKind { get { return GameDataObjectKind.Cost; } }
	};
	[AttributeUsage(kValidOn, AllowMultiple = false)]
	public sealed class PopulationReferenceAttribute : GameDataObjectReferenceAttribute
	{
		public override Type ProtoType { get { return typeof(string); } }
		public override GameDataObjectKind ProtoKind { get { return GameDataObjectKind.Pop; } }
	};
	[AttributeUsage(kValidOn, AllowMultiple = false)]
	public sealed class RateReferenceAttribute : GameDataObjectReferenceAttribute
	{
		public override Type ProtoType { get { return typeof(string); } }
		public override GameDataObjectKind ProtoKind { get { return GameDataObjectKind.Rate; } }
	};
	#endregion

	#region HPBar data
	[AttributeUsage(kValidOn, AllowMultiple=false)]
	public abstract class ProtoHPBarObjectReferenceAttribute
		: ProtoDataReferenceAttribute
		, IProtoDataReferenceAttribute
	{
		public override ProtoDataObjectSourceKind ObjectSourceKind { get { return ProtoDataObjectSourceKind.HPData; } }

		public abstract HPBarDataObjectKind ProtoKind { get; }

		Type IProtoDataReferenceAttribute.ProtoType { get { return this.ProtoType; } }
		string IProtoDataReferenceAttribute.ProtoKindName { get { return this.ProtoKind.ToString(); } }
		int IProtoDataReferenceAttribute.ProtoKindId { get { return (int) this.ProtoKind; } }
	};

	[AttributeUsage(kValidOn, AllowMultiple=false)]
	public sealed class BProtoHPBarReferenceAttribute : ProtoHPBarObjectReferenceAttribute
	{
		public override Type ProtoType { get { return typeof(BProtoHPBar); } }
		public override HPBarDataObjectKind ProtoKind { get { return HPBarDataObjectKind.HPBar; } }
	};

	[AttributeUsage(kValidOn, AllowMultiple=false)]
	public sealed class BProtoHPBarColorStagesReferenceAttribute : ProtoHPBarObjectReferenceAttribute
	{
		public override Type ProtoType { get { return typeof(BProtoHPBarColorStages); } }
		public override HPBarDataObjectKind ProtoKind { get { return HPBarDataObjectKind.ColorStages; } }
	};

	[AttributeUsage(kValidOn, AllowMultiple=false)]
	public sealed class BProtoVeterancyBarReferenceAttribute : ProtoHPBarObjectReferenceAttribute
	{
		public override Type ProtoType { get { return typeof(BProtoVeterancyBar); } }
		public override HPBarDataObjectKind ProtoKind { get { return HPBarDataObjectKind.VeterancyBar; } }
	};

	[AttributeUsage(kValidOn, AllowMultiple=false)]
	public sealed class BProtoPieProgressReferenceAttribute : ProtoHPBarObjectReferenceAttribute
	{
		public override Type ProtoType { get { return typeof(BProtoPieProgress); } }
		public override HPBarDataObjectKind ProtoKind { get { return HPBarDataObjectKind.PieProgress; } }
	};

	[AttributeUsage(kValidOn, AllowMultiple=false)]
	public sealed class BProtoBobbleHeadReferenceAttribute : ProtoHPBarObjectReferenceAttribute
	{
		public override Type ProtoType { get { return typeof(BProtoBobbleHead); } }
		public override HPBarDataObjectKind ProtoKind { get { return HPBarDataObjectKind.BobbleHead; } }
	};

	[AttributeUsage(kValidOn, AllowMultiple=false)]
	public sealed class BProtoBuildingStrengthReferenceAttribute : ProtoHPBarObjectReferenceAttribute
	{
		public override Type ProtoType { get { return typeof(BProtoBuildingStrength); } }
		public override HPBarDataObjectKind ProtoKind { get { return HPBarDataObjectKind.BuildingStrength; } }
	};
	#endregion

	#region Proto data
	[AttributeUsage(kValidOn, AllowMultiple = false)]
	public abstract class ProtoReferenceAttribute
		: ProtoDataReferenceAttribute
		, IProtoDataReferenceAttribute
	{
		public override ProtoDataObjectSourceKind ObjectSourceKind { get { return ProtoDataObjectSourceKind.Database; } }

		public abstract DatabaseObjectKind ProtoKind { get; }

		Type IProtoDataReferenceAttribute.ProtoType { get { return this.ProtoType; } }
		string IProtoDataReferenceAttribute.ProtoKindName { get { return this.ProtoKind.ToString(); } }
		int IProtoDataReferenceAttribute.ProtoKindId { get { return (int) this.ProtoKind; } }
	};

	[AttributeUsage(kValidOn, AllowMultiple=false)]
	public sealed class BAbilityReferenceAttribute : ProtoReferenceAttribute
	{
		public override Type ProtoType { get { return typeof(BAbility); } }
		public override DatabaseObjectKind ProtoKind { get { return DatabaseObjectKind.Ability; } }
	};

	[AttributeUsage(kValidOn, AllowMultiple=false)]
	public sealed class BCivReferenceAttribute : ProtoReferenceAttribute
	{
		public override Type ProtoType { get { return typeof(BCiv); } }
		public override DatabaseObjectKind ProtoKind { get { return DatabaseObjectKind.Civ; } }
	};

	[AttributeUsage(kValidOn, AllowMultiple=false)]
	public sealed class BDamageTypeReferenceAttribute : ProtoReferenceAttribute
	{
		public override Type ProtoType { get { return typeof(BDamageType); } }
		public override DatabaseObjectKind ProtoKind { get { return DatabaseObjectKind.DamageType; } }
	};

	[AttributeUsage(kValidOn, AllowMultiple=false)]
	public sealed class BProtoImpactEffectReferenceAttribute : ProtoReferenceAttribute
	{
		public override Type ProtoType { get { return typeof(BProtoImpactEffect); } }
		public override DatabaseObjectKind ProtoKind { get { return DatabaseObjectKind.ImpactEffect; } }
	};

	[AttributeUsage(kValidOn, AllowMultiple=false)]
	public sealed class BLeaderReferenceAttribute : ProtoReferenceAttribute
	{
		public override Type ProtoType { get { return typeof(BLeader); } }
		public override DatabaseObjectKind ProtoKind { get { return DatabaseObjectKind.Leader; } }
	};

	[AttributeUsage(kValidOn, AllowMultiple=false)]
	public sealed class BProtoObjectReferenceAttribute : ProtoReferenceAttribute
	{
		public override Type ProtoType { get { return typeof(BProtoObject); } }
		public override DatabaseObjectKind ProtoKind { get { return DatabaseObjectKind.Object; } }
	};

	[AttributeUsage(kValidOn, AllowMultiple=false)]
	public sealed class ObjectTypeReferenceAttribute : ProtoReferenceAttribute
	{
		public override Type ProtoType { get { return null; } }
		public override DatabaseObjectKind ProtoKind { get { return DatabaseObjectKind.ObjectType; } }
	};

	[AttributeUsage(kValidOn, AllowMultiple=false)]
	public sealed class BProtoPowerReferenceAttribute : ProtoReferenceAttribute
	{
		public override Type ProtoType { get { return typeof(BProtoPower); } }
		public override DatabaseObjectKind ProtoKind { get { return DatabaseObjectKind.Power; } }
	};

	[AttributeUsage(kValidOn, AllowMultiple=false)]
	public sealed class BProtoSquadReferenceAttribute : ProtoReferenceAttribute
	{
		public override Type ProtoType { get { return typeof(BProtoSquad); } }
		public override DatabaseObjectKind ProtoKind { get { return DatabaseObjectKind.Squad; } }
	};

	[AttributeUsage(kValidOn, AllowMultiple=false)]
	public sealed class BTacticDataReferenceAttribute : ProtoReferenceAttribute
	{
		public override Type ProtoType { get { return typeof(BTacticData); } }
		public override DatabaseObjectKind ProtoKind { get { return DatabaseObjectKind.Tactic; } }
	};

	[AttributeUsage(kValidOn, AllowMultiple=false)]
	public sealed class BProtoTechReferenceAttribute : ProtoReferenceAttribute
	{
		public override Type ProtoType { get { return typeof(BProtoTech); } }
		public override DatabaseObjectKind ProtoKind { get { return DatabaseObjectKind.Tech; } }
	};

	[AttributeUsage(kValidOn, AllowMultiple=false)]
	public sealed class TerrainTileTypeReferenceAttribute : ProtoReferenceAttribute
	{
		public override Type ProtoType { get { return typeof(TerrainTileType); } }
		public override DatabaseObjectKind ProtoKind { get { return DatabaseObjectKind.TerrainTileType; } }
	};

	/// <summary>Object or ObjectType</summary>
	[AttributeUsage(kValidOn, AllowMultiple=false)]
	public sealed class UnitReferenceAttribute : ProtoReferenceAttribute
	{
		public override Type ProtoType { get { return null; } }
		public override DatabaseObjectKind ProtoKind { get { return DatabaseObjectKind.Unit; } }
	};

	[AttributeUsage(kValidOn, AllowMultiple=false)]
	public sealed class BUserClassReferenceAttribute : ProtoReferenceAttribute
	{
		public override Type ProtoType { get { return typeof(BUserClass); } }
		public override DatabaseObjectKind ProtoKind { get { return DatabaseObjectKind.UserClass; } }
	};

	[AttributeUsage(kValidOn, AllowMultiple=false)]
	public sealed class BWeaponTypeReferenceAttribute : ProtoReferenceAttribute
	{
		public override Type ProtoType { get { return typeof(BWeaponType); } }
		public override DatabaseObjectKind ProtoKind { get { return DatabaseObjectKind.WeaponType; } }
	};
	#endregion
}

namespace KSoft.Phoenix
{
	partial class TypeExtensionsPhx
	{
		[Contracts.Pure]
		public static string GetExportContractName(this Phx.Meta.IProtoDataReferenceAttribute attr)
		{
			if (attr == null)
				return null;

			return string.Format("{0}.{1}",
				attr.ObjectSourceKind, attr.ProtoKindName);
		}
	};
}