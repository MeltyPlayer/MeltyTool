using fin.schema;

using schema.binary;
using schema.binary.attributes;

namespace mp64.schema.mainfs;

[BinarySchema]
public sealed partial class MainFs : IBinaryDeserializable {
  [SequenceLengthSource(SchemaIntegerType.UINT32)]
  public RPointer<MainFsDirectory>[] Directories { get; set; }
}