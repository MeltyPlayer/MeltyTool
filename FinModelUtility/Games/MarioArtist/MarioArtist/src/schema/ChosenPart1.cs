using System.Numerics;

using schema.binary;
using schema.binary.attributes;


namespace marioartist.schema;

[BinarySchema]
public partial class ChosenPart1 : IBinaryDeserializable {
  public uint MeshSetId { get; set; }
  public uint MaybeFileIndex { get; set; }
  public uint ChosenModelIndex { get; set; }

  [IntegerFormat(SchemaIntegerType.UINT32)]
  public bool IsLeft { get; set; }

  [SequenceLengthSource(5)]
  public uint[] DisplayListSegmentedAddresses { get; set; }

  [SequenceLengthSource(0x54)]
  public byte[] Unk2 { get; set; }

  public Vector3 UnkVec3a { get; set; }
  public Vector3 UnkVec3b { get; set; }

  public uint UnkRamAddress { get; set; }

  public Vector3 UnkVec3c { get; set; }
  public Vector3 UnkVec3d { get; set; }

  [SequenceLengthSource(3)]
  public uint[] Unk4 { get; set; }
}