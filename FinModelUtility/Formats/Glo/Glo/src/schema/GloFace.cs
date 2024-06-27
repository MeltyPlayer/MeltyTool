using fin.schema.color;

using schema.binary;
using schema.binary.attributes;

namespace glo.schema;

[BinarySchema]
public sealed partial class GloFace : IBinaryConvertible {
  [StringLengthSource(16)]
  public string TextureFilename { get; set; }

  public Rgba32 Color { get; private set; } = new();
  public GloObjectFlags Flags { get; set; }

  public GloVertexRef[] VertexRefs { get; } = [new(), new(), new()];

  public override string ToString()
    => $"{this.TextureFilename} - {this.Flags}";
}