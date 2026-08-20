using hm64.schema.mesh;

using schema.binary;
using schema.binary.attributes;

namespace hm64.schema;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/harvestwhisperer/hm64-decomp/blob/master/tools/modding/map/blender_import.py#L475
/// </summary>
[BinarySchema]
[Endianness(Endianness.BigEndian)]
public partial class Map : IBinaryDeserializable {
  public Offsets Offsets { get; } = new();

  [RAtPosition(nameof(Offsets.GridOffset))]
  public Grid Grid { get; } = new();

  [RAtPosition(nameof(Offsets.MeshOffset))]
  public Mesh Mesh { get; } = new();
}