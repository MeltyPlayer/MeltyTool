using schema.binary;
using schema.binary.attributes;


namespace marioartist.schema;

[BinarySchema]
public partial class MeshDefinition : IBinaryDeserializable {
  [SequenceLengthSource(4)]
  public uint[] MeshSegmentedAddresses { get; set; }

  [SequenceLengthSource(4)]
  public byte[] Unk0 { get; set; }

  public byte MeshSetId { get; set; }
  public byte UnkIndex { get; set; }

  public ushort Unk2 { get; set; }

  [SequenceLengthSource(3)]
  public uint[] Unk3 { get; set; }

  [SequenceLengthSource(6)]
  public float[] Unk4 { get; set; }

  public uint Unk5 { get; set; }

  [SequenceLengthSource(4)]
  public uint[] PrimitiveDisplayListSegmentedAddresses { get; set; }

  [SequenceLengthSource(9)]
  public uint[] Unk6 { get; set; }

  [SequenceLengthSource(4)]
  public uint[] VertexDisplayListSegmentedAddresses { get; set; }

  [SequenceLengthSource(9)]
  public uint[] Unk7 { get; set; }
}