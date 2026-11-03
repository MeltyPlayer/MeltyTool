using fin.schema;

using schema.binary;
using schema.binary.attributes;

namespace mp64.schema.mainfs;

[BinarySchema]
public sealed partial class MainFsDirectory : IBinaryDeserializable {
  [SequenceLengthSource(SchemaIntegerType.UINT32)]
  public RPointer<MainFsFile>[] Files { get; set; }
}