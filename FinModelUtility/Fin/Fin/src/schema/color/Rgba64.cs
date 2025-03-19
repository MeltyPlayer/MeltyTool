using schema.binary;

namespace fin.schema.color;

[BinarySchema]
public partial class Rgba64 : IBinaryConvertible {
  public ushort R { get; set; }
  public ushort G { get; set; }
  public ushort B { get; set; }
  public ushort A { get; set; }

  public override string ToString()
    => $"rgba({this.R}, {this.G}, {this.B}, {this.A})";
}