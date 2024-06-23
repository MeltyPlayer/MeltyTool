using schema.binary;
using schema.binary.attributes;

namespace ttyd.schema.model.blocks {
  [BinarySchema]
  public partial class Group : IBinaryDeserializable {
    [StringLengthSource(64)]
    public string Name { get; set; }

    public int NextGroupIndex { get; set; }
    public int ChildGroupIndex { get; set; }

    public int SceneGraphObjectIndex { get; set; }
    public int VisibilityGroupIndex { get; set; }
    public int TransformBaseIndex { get; set; }

    [IntegerFormat(SchemaIntegerType.UINT32)]
    public bool IsJoint { get; set; }
  }
}