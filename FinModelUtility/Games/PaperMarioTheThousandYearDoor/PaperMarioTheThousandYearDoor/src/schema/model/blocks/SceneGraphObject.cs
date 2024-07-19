using gx;

using schema.binary;
using schema.binary.attributes;

using SharpGLTF.Geometry.VertexTypes;

namespace ttyd.schema.model.blocks;

[BinarySchema]
public partial class VertexDataTuple : IBinaryConvertible {
  public int BaseIndex { get; set; }
  public int CoordCount { get; set; }
}

public enum BlendMode : int {
  ALPHA_CLIP,
  OPAQUE,
  TRANSLUCENT_PLUS_ONE,
  TRANSLUCENT,
}

public enum CullMode : int {
  BACK,
  FRONT,
  ALL,
  NONE,
}

[BinarySchema]
public partial class SceneGraphObject : IBinaryDeserializable {
  [StringLengthSource(64)]
  public string Name { get; set; }

  public VertexDataTuple VertexPosition { get; } = new();
  public VertexDataTuple VertexNormal { get; } = new();
  public VertexDataTuple VertexColor { get; } = new();

  [SequenceLengthSource(8)]
  public VertexDataTuple[] TexCoords { get; private set; }

  public int MeshBaseIndex { get; set; }
  public int MeshCount { get; set; }

  public BlendMode BlendMode { get; set; }
  public CullMode CullMode { get; set; }

  public override string ToString()
    => $"meshes: from {this.MeshBaseIndex}, length {this.MeshCount}";
}