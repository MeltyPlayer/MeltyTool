using fin.schema;

using schema.binary;

namespace ast.schema;

[BinarySchema]
public partial class BlckHeader : IBinaryConvertible {
  private readonly string magic_ = "BLCK";

  public uint BlockSizeInBytes { get; private set; }

  [Unknown]
  public uint Unknown1 { get; private set; }

  [Unknown]
  public uint Unknown2 { get; private set; }

  [Unknown]
  public byte[] Unknowns { get; } = new byte[0x10];
}