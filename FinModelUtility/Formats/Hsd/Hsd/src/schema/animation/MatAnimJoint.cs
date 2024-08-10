using schema.binary;
using schema.binary.attributes;

namespace sysdolphin.schema.animation;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/Ploaj/HSDLib/blob/b7554d5c753cca2d50090cdd7366afe64dd8f175/HSDRaw/Common/Animation/HSD_MatAnim.cs#L3
/// </summary>
[BinarySchema]
public partial class MatAnimJoint : IDatTreeNode<MatAnimJoint>,
                                    IBinaryDeserializable {
  public uint FirstChildOffset { get; set; }
  public uint NextSiblingOffset { get; set; }
  public uint FirstMatAnimOffset { get; set; }


  [RAtPositionOrNull(nameof(FirstChildOffset))]
  public MatAnimJoint? FirstChild { get; set; }

  [RAtPositionOrNull(nameof(NextSiblingOffset))]
  public MatAnimJoint? NextSibling { get; set; }

  [RAtPositionOrNull(nameof(FirstMatAnimOffset))]
  public MatAnim? FirstMatAnim { get; set; }


  [Skip]
  public IEnumerable<MatAnim> MatAnims => this.FirstMatAnim.GetSelfAndSiblings();
}