using schema.binary;

namespace visceral.schema.dat;

[BinarySchema]
public partial class Dat : IBinaryConvertible {
  private readonly string magic_ = "BIGH";
}