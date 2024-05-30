using fin.schema.vector;

using schema.binary;
using schema.binary.attributes;

namespace ttyd.schema.blocks {
  [BinarySchema]
  public partial class SceneGraph : IBinaryDeserializable {
    [StringLengthSource(64)]
    public string Name { get; set; }

    public int NextRecord { get; set; }
    public int ChildRecord { get; set; }

    public int SceneGraphObjectIndex { get; set; }
    public int SceneGraphObjectVisibilityIndex { get; set; }
    public int SceneGraphObjectTransformationIndex { get; set; }
    public int Joint { get; set; }
  }
}