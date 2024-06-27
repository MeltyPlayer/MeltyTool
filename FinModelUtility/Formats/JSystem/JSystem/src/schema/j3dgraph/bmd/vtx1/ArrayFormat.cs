using fin.schema;

using gx;

using schema.binary;

namespace jsystem.schema.j3dgraph.bmd.vtx1;

[BinarySchema]
public partial class ArrayFormat : IBinaryConvertible {
  public GxAttribute ArrayType;
  public uint ComponentCount;
  public uint DataType;
  public byte DecimalPoint;
  [Unknown]
  public byte Unknown1;
  [Unknown]
  public ushort Unknown2;
}