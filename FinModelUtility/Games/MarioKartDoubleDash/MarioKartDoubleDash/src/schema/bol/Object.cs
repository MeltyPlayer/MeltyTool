using System.Numerics;

using fin.schema.vector;

using schema.binary;
using schema.binary.attributes;

namespace mkdd.schema.bol;

[BinarySchema]
public partial class Object : IBinaryDeserializable {
  public Vector3 Position { get; set; }
  public Vector3 Scale { get; set; }
  public Vector3i Rotation { get; } = new();
  public ushort ObjectId { get; set; }
  public short PathId { get; set; }
  public uint Unk0 { get; set; }
  public byte Unk1 { get; set; }
  public byte PresenceFlag { get; set; }

  [SequenceLengthSource(9)]
  public ushort[] Unk2 { get; set; }
}