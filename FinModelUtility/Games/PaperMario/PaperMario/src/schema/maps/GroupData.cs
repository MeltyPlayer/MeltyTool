using f3dzex2.rdp;

using schema.binary;
using schema.binary.attributes;

namespace pm.schema.maps;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/magcius/noclip.website/blob/6b16cfda00ef5af3ee2a66d8b928bb0bf700e5b6/src/PaperMario64/map_shape.ts#L191
/// </summary>
[BinarySchema]
public sealed partial class GroupData : IBinaryDeserializable {
  public uint ModelMatrixRamAddress { get; set; }
  public uint Unk0 { get; set; }
  public uint Unk1 { get; set; }
  public uint NumChildren { get; set; }
  public uint ChildrenTableRamAddress { get; set; }

  [Skip]
  public uint ModelMatrixOffset
    => Shape.ConvertRamAddressToOffset(this.ModelMatrixRamAddress);

  [RAtPositionOrNull(nameof(ModelMatrixOffset))]
  public RdpMatrix4x4? ModelMatrix { get; set; }

  [Skip]
  public uint ChildrenTableOffset
    => Shape.ConvertRamAddressToOffset(this.ChildrenTableRamAddress);

  [RAtPosition(nameof(ChildrenTableOffset))]
  [RSequenceLengthSource(nameof(NumChildren))]
  public uint[] ChildRamAddresses { get; set; }

  [Skip]
  public ModelTreeNode[] Children { get; set; }

  [ReadLogic]
  private void ReadChildren_(IBinaryReader br)
    => this.Children
        = this
          .ChildRamAddresses
          .Select(Shape.ConvertRamAddressToOffset)
          .Select(childOffset => br.SubreadAt(
                      childOffset,
                      () => br.ReadNew<ModelTreeNode>()))
          .ToArray();
}