using fin.schema.color;
using fin.schema.vector;

using schema.binary;

using ttyd.schema.blocks;

namespace ttyd.schema {
  public class Model : IBinaryDeserializable {
    public Header Header { get; } = new();

    public SceneGraph[] SceneGraphs { get; private set; }
    public SceneGraphObject[] SceneGraphObjects { get; private set; }

    public Vector3f[] Vertices { get; private set; }
    public uint[] VertexIndices { get; private set; }

    public Vector3f[] Normals { get; private set; }
    public uint[] NormalIndices { get; private set; }

    public Rgba32[] Colors { get; private set; }
    public uint[] ColorIndices { get; private set; }

    public Vector2f[] TexCoords { get; private set; }
    public uint[] TexCoordIndices { get; private set; }

    public void Read(IBinaryReader br) {
      this.Header.Read(br);

      this.SceneGraphs = this.ReadNews_<SceneGraph>(br, BlockType.SCENE_GRAPH);
      this.SceneGraphObjects
          = this.ReadNews_<SceneGraphObject>(br, BlockType.SCENE_GRAPH_OBJECT);

      this.Vertices
          = this.ReadNews_<Vector3f>(br, BlockType.VERTEX);
      this.VertexIndices = this.ReadNews_(
          br,
          BlockType.POLYGON_VERTEX_MAP,
          br.ReadUInt32s);

      this.Normals
          = this.ReadNews_<Vector3f>(br, BlockType.NORMAL);
      this.NormalIndices = this.ReadNews_(
          br,
          BlockType.POLYGON_NORMAL_MAP,
          br.ReadUInt32s);

      this.Colors
          = this.ReadNews_<Rgba32>(br, BlockType.COLOR);
      this.ColorIndices = this.ReadNews_(
          br,
          BlockType.POLYGON_COLOR_MAP,
          br.ReadUInt32s);

      this.TexCoords
          = this.ReadNews_<Vector2f>(br, BlockType.TEX_COORD);
      this.TexCoordIndices = this.ReadNews_(
          br,
          BlockType.POLYGON_TEX_COORD_MAP,
          br.ReadUInt32s);
    }

    private T[] ReadNews_<T>(IBinaryReader br, BlockType blockType)
        where T : IBinaryDeserializable, new() {
      br.Position = this.Header.GetOffset(blockType);
      return br.ReadNews<T>(this.Header.GetCount(blockType));
    }

    private T[] ReadNews_<T>(IBinaryReader br,
                             BlockType blockType,
                             Func<long, T[]> readNew) {
      br.Position = this.Header.GetOffset(blockType);
      return readNew(this.Header.GetCount(blockType));
    }
  }
}