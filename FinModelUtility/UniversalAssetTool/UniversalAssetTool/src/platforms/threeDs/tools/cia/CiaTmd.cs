using schema.binary;
using schema.binary.attributes;

namespace uni.platforms.threeDs.tools.cia {
  [BinarySchema]
  public partial class CiaTmd : IChildOf<Cia>, IBinaryDeserializable {
    public Cia Parent { get; set; }
  }
}
