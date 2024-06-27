using schema.binary;
using schema.binary.attributes;

namespace dat.schema.animation;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/Ploaj/HSDLib/blob/b7554d5c753cca2d50090cdd7366afe64dd8f175/HSDRaw/Common/Animation/HSD_MatAnim.cs#L3
/// </summary>
[BinarySchema]
public partial class MatAnim : IDatLinkedListNode<MatAnim>,
                               IBinaryDeserializable {
  public uint NextSiblingOffset { get; set; }
  public uint AObjOffset { get; set; }
  public uint TexAnimOffset { get; set; }

  [IntegerFormat(SchemaIntegerType.INT32)]
  public bool RenderAnim { get; set; }


  [RAtPositionOrNull(nameof(NextSiblingOffset))]
  public MatAnim? NextSibling { get; set; }

  [RAtPositionOrNull(nameof(AObjOffset))]
  public AObj? AObj { get; set; }
}