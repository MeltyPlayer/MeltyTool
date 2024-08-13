using fin.schema;

using schema.binary;


namespace jsystem.schema.j3dgraph.bmd.shp1;

[BinarySchema]
public partial class MatrixData : IBinaryConvertible {
  [Unknown]
  public ushort Unknown { get; set; }

  public ushort Count { get; set; }
  public uint FirstIndex { get; set; }
}