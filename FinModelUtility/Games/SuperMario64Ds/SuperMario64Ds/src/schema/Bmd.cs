using schema.binary;
using schema.binary.attributes;

namespace sm64ds.schema;

/// <summary>
///   Shamelessly stolen from:
///   https://kuribo64.net/get.php?id=KBNyhM0kmNiuUBb3
/// </summary>
[BinarySchema]
public partial class Bmd : IBinaryConvertible {
  public uint ScaleFactor { get; set; }

  public uint BoneCount { get; set; }
  public uint BonesOffset { get; set; }

  public uint DisplayListCount { get; set; }
  public uint DisplayListsOffset { get; set; }

  public uint TextureCount { get; set; }
  public uint TexturesOffset { get; set; }

  public uint TexturePaletteCount { get; set; }
  public uint TexturePalettesOffset { get; set; }

  public uint MaterialCount { get; set; }
  public uint MaterialsOffset { get; set; }

  public uint TransformAndBoneMapOffset { get; set; }
  public uint TextureAndPaletteDataBlock { get; set; }

  [RAtPosition(nameof(BonesOffset))]
  [RSequenceLengthSource(nameof(BoneCount))]
  public Bone[] Bones { get; set; }

  [RAtPosition(nameof(TexturesOffset))]
  [RSequenceLengthSource(nameof(TextureCount))]
  public Texture[] Textures { get; set; }

  [RAtPosition(nameof(MaterialsOffset))]
  [RSequenceLengthSource(nameof(MaterialCount))]
  public Material[] Materials { get; set; }
}