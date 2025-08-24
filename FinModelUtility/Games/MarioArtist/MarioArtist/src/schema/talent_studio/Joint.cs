using System.Numerics;

using schema.binary;
using schema.binary.attributes;


namespace marioartist.schema.talent_studio;

[BinarySchema]
public partial class Joint : IBinaryDeserializable {
  public ushort unk0;
  public ushort unk1;

  public ushort MeshSetId { get; set; }
  public short unk2;
  public uint unk3;

  public byte index;

  [IntegerFormat(SchemaIntegerType.BYTE)]
  public bool isLeft;

  public ushort unk6;
  public ushort unk7;

  public byte previousIndex;
  public byte nextIndex;

  public Matrix4x4 matrix;

  [SequenceLengthSource(12)]
  public byte[] unk4;
}