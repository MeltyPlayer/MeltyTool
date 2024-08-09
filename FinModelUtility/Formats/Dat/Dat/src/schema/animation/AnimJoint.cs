using schema.binary;
using schema.binary.attributes;

namespace dat.schema.animation;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/Ploaj/HSDLib/blob/master/HSDRaw/Common/Animation/HSD_AnimJoint.cs#L4
/// </summary>
[BinarySchema]
public partial class AnimJoint
    : IDatTreeNode<AnimJoint>,
      IBinaryDeserializable {
  public uint FirstChildOffset { get; set; }
  public uint NextSiblingOffset { get; set; }
  public uint AObjOffset { get; set; }
  public uint FirstRObjOffset { get; set; }
  public uint Flags { get; set; }


  [RAtPositionOrNull(nameof(FirstChildOffset))]
  public AnimJoint? FirstChild { get; set; }

  [RAtPositionOrNull(nameof(NextSiblingOffset))]
  public AnimJoint? NextSibling { get; set; }

  [RAtPositionOrNull(nameof(AObjOffset))]
  public AObj? AObj { get; set; }

  [RAtPositionOrNull(nameof(FirstRObjOffset))]
  public RObj? FirstRObj { get; set; }
}