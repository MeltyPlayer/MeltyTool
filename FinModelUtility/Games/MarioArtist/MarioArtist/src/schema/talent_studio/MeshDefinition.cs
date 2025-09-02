using schema.binary;


namespace marioartist.schema.talent_studio;

[BinarySchema]
public partial class MeshDefinition : IBinaryDeserializable {
  public uint[] MeshSegmentedAddresses { get; } = new uint[4];
  public byte[] Unk0 { get; } = new byte[4];

  public byte MeshSetId { get; set; }
  public byte UnkIndex { get; set; }

  public ushort Unk2 { get; set; }
  public uint[] Unk3 { get; } = new uint[3];
  public float[] Unk4 { get; } = new float[6];

  public uint Unk5 { get; set; }
  
  public uint[] PrimitiveDisplayListSegmentedAddresses { get; } = new uint[4];
  public byte[] Unk6 { get; } = new byte[36];
  
  public uint[] VertexDisplayListSegmentedAddresses { get; } = new uint[4];
  public byte[] Unk7 { get; } = new byte[36];
}