using schema.binary;
using schema.binary.attributes;

namespace sm64ds.schema.bca;

/// <summary>
///   Shamelessly stolen from:
///   https://kuribo64.net/get.php?id=KNIJfjt4ZG2tcgDM
///   https://github.com/Arisotura/SM64DSe/blob/master/SM64DSFormats/BCA.cs
/// </summary>
[BinarySchema]
public partial class Bca : IBinaryDeserializable {
  public ushort NumBones { get; set; }
  public ushort NumFrames { get; set; }

  [NumberFormat(SchemaNumberType.UINT32)]
  public bool Looped { get; set; }

  public uint ScaleValuesOffset { get; set; }
  public uint RotationValuesOffset { get; set; }
  public uint TranslationValuesOffset { get; set; }
  public uint AnimationOffset { get; set; }

  [RAtPosition(nameof(AnimationOffset))]
  [RSequenceLengthSource(nameof(NumBones))]
  public BoneAnimationData[] BoneAnimationData { get; set; }
}