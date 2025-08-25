using fin.schema.color;

using schema.binary;
namespace marioartist.schema;

[BinarySchema]
public partial class MfsThumbnail : IBinaryDeserializable {
  public Argb1555Image Image { get; } = new(24, 24);
}