using schema.binary;
using schema.binary.attributes;

namespace grezzo.schema.cmb.sklm;

[BinarySchema]
public sealed partial class Sklm : IBinaryConvertible {
  [Skip]
  public Version Version { get; init; }

  public uint mshOffset;
  public uint shpOffset;

  public readonly Mshs mshs = new();
  public readonly Shp shapes = new();
}