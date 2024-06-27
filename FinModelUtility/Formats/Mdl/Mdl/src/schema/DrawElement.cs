using schema.binary;

namespace mdl.schema {
  [BinarySchema]
  public partial class DrawElement : IBinaryConvertible {
    public ushort MaterialIndex { get; set; }
    public ushort ShapeIndex { get; set; }
  }
}