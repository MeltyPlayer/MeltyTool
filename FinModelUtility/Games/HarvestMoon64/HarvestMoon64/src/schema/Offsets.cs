using schema.binary;

namespace hm64.schema;

[BinarySchema]
public partial class Offsets : IBinaryDeserializable {
  public uint GridOffset { get; set; }
  public uint MeshOffset { get; set; }
  public uint Offset2 { get; set; }
  public uint Offset3 { get; set; }
  public uint ObjectsOffset { get; set; }
  public uint Offset5 { get; set; }
  public uint Offset6 { get; set; }
  public uint Offset7 { get; set; }
  public uint Offset8 { get; set; }
  public uint Offset9 { get; set; }
  public uint Offset10 { get; set; }
  public uint Offset11 { get; set; }
  public uint Offset12 { get; set; }
  public uint Offset13 { get; set; }
  public uint Offset14 { get; set; }
  public uint Offset15 { get; set; }
}
