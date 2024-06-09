using fin.image.io;

using grezzo.image;
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

    public uint DataOffset { get; private set; }

    [StringLengthSource(0x10)]
    public string name { get; private set; }

    public IImageReader GetImageReader()
      => new CmbImageReader(this.width,
                            this.height,
                            CollapseFormat_(this.imageFormat));

    private GlTextureFormat CollapseFormat_(GlTextureFormat format) {
      var lowerFormat = (GlTextureFormat) ((int) format & 0xFFFF);

      if (lowerFormat == GlTextureFormat.ETC1) {
        format = GlTextureFormat.ETC1;
      } else if (lowerFormat == GlTextureFormat.ETC1a4) {
        format = GlTextureFormat.ETC1a4;
      }

      return format;
    }
  }
}