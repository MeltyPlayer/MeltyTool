using schema.binary;

namespace ttyd.schema.tpl;

[BinarySchema]
public partial class TplTextureHeader : IBinaryDeserializable {
  public ushort Height { get; set; }
  public ushort Width { get; set; }

  public TplImageFormat Format { get; set; }
  public uint DataOffset { get; set; }

  public uint WrapS { get; set; }
  public uint WrapT { get; set; }

  public uint MinFilter { get; set; }
  public uint MagFilter { get; set; }

  public float LodBias { get; set; }

  public byte EdgeLod { get; set; }
  public byte MinLod { get; set; }
  public byte MaxLod { get; set; }
  public byte Unpacked { get; set; }
}