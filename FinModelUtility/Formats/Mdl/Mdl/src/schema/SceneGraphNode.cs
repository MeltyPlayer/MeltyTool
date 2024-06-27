using schema.binary;

namespace mdl.schema;

[BinarySchema]
public partial class SceneGraphNode : IBinaryConvertible {
  public ushort InverseMatrixIndex { get; set; }
  public ushort ChildIndexShift { get; set; }
  public ushort SiblingIndexShift { get; set; }
  public ushort Padding1 { get; set; }
  public ushort DrawElementCount { get; set; }
  public ushort DrawElementBeginIndex { get; set; }
  public uint Padding2 { get; set; }
}