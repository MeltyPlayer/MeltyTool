using schema.binary;
using schema.binary.attributes;

namespace hm64.schema.objects;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/harvestwhisperer/hm64-decomp/blob/master/tools/modding/map/blender_import.py#L250
/// </summary>
[BinarySchema]
public partial class Objects : IBinaryDeserializable {
  private uint pointer_;

  [SequenceLengthSource(SchemaIntegerType.BYTE)]
  public Group[] Groups { get; set; }
}