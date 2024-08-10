using schema.binary;
using schema.binary.attributes;

namespace sysdolphin.schema.melee;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/Ploaj/HSDLib/blob/master/HSDRaw/Melee/Pl/SBM_FighterData.cs
/// </summary>
[BinarySchema]
public partial class MeleeFighterData : IDatNode, IBinaryDeserializable {
  public uint Attributes { get; set; }
  public uint Attributes2 { get; set; }
  public uint ModelLookupTablesOffset { get; set; }
  public uint FighterActionTableOffset { get; set; }

  public uint FighterActionDynamicBehaviorsOffset { get; set; }
  public uint DemoActionTableOffset { get; set; }
  public uint DemoActionDynamicBehaviorsOffsetO { get; set; }
  public uint ModelPartAnimationsOffset { get; set; }

  public uint ShieldPoseContainerOffset { get; set; }
  public uint IdleActionChancesOffset { get; set; }
  public uint WaitIdleActionChancesOffset { get; set; }
  public uint PhysicsOffset { get; set; }

  public uint HurtboxesOffset { get; set; }
  public uint CenterBubbleOffset { get; set; }
  public uint CoinCollisionSpheresOffset { get; set; }
  public uint CameraBoxOffset { get; set; }

  public uint ItemPickupParamsOffset { get; set; }
  public uint EnvironmentCollisionOffset { get; set; }
  public uint ArticlesOffset { get; set; }
  public uint CommonSoundEffectTableOffset { get; set; }

  public uint JostleBoxOffset { get; set; }
  public uint FighterBoneTableOffset { get; set; }

  // TODO: Handle everything else here

  [RAtPosition(nameof(ModelLookupTablesOffset))]
  public MeleeModelLookupTables ModelLookupTables { get; } = new();

  [RAtPosition(nameof(FighterBoneTableOffset))]
  public MeleeFighterBoneIds FighterBoneIds { get; } = new();
}