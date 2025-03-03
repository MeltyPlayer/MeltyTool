using schema.binary;
using schema.binary.attributes;


namespace jsystem.schema.j3dgraph.bmd;

[BinarySchema]
public partial class BmdHeader : IBinaryConvertible {
  private readonly string magic_ = "J3D2bmd3";

  [WSizeOfStreamInBytes]
  public uint FileSize;

  public uint NrSections;
  public readonly byte[] Padding = new byte[16];
}