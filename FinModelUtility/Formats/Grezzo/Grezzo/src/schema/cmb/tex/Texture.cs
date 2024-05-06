using grezzo.schema.ctxb;

using schema.binary;
using schema.binary.attributes;

namespace grezzo.schema.cmb.tex {
  [BinarySchema]
  public partial class Texture : IBinaryConvertible {
    public uint dataLength { get; private set; }
    public ushort mimapCount { get; private set; }

    [IntegerFormat(SchemaIntegerType.BYTE)]
    public bool isEtc1 { get; private set; }

    [IntegerFormat(SchemaIntegerType.BYTE)]
    public bool isCubemap { get; private set; }

    public ushort width { get; private set; }
    public ushort height { get; private set; }

    public GlTextureFormat imageFormat { get; private set; }

    public uint dataOffset { get; private set; }

    [StringLengthSource(0x10)]
    public string name { get; private set; }
  }
}