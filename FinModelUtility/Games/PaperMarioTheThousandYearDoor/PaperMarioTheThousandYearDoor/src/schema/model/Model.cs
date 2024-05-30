using fin.schema.color;
using fin.schema.vector;

using schema.binary;

using ttyd.schema.model.blocks;

namespace ttyd.schema.model {
  public class Model : IBinaryDeserializable {
    public Header Header { get; } = new();

    public SceneGraph[] SceneGraphs { get; private set; }
    public SceneGraphObject[] SceneGraphObjects { get; private set; }
    public byte[] SceneGraphObjectVisibilities { get; private set; }
    public float[] SceneGraphObjectTransforms { get; private set; }

    public Mesh[] Meshes { get; private set; }
    public Polygon[] Polygons { get; private set; }
    public Texture[] Textures { get; private set; }
    public TextureMap[] TextureMaps { get; private set; }

    public Vector3f[] Vertices { get; private set; }
    public int[] VertexIndices { get; private set; }

    public Vector3f[] Normals { get; private set; }
    public int[] NormalIndices { get; private set; }

    public Rgba32[] Colors { get; private set; }
    public int[] ColorIndices { get; private set; }

    public Vector2f[] TexCoords { get; private set; }
    public int[] TexCoordIndices { get; private set; }

    public void Read(IBinaryReader br) {
      this.Header.Read(br);

      this.SceneGraphs = this.ReadNews_<SceneGraph>(br, BlockType.SCENE_GRAPH);
      this.SceneGraphObjects
          = this.ReadNews_<SceneGraphObject>(br, BlockType.SCENE_GRAPH_OBJECT);
      this.SceneGraphObjectVisibilities = this.ReadNews_(br,
        BlockType.SCENE_GRAPH_OBJECT_VISIBILITY,
        br.ReadBytes);
      this.SceneGraphObjectTransforms = this.ReadNews_(br,
        BlockType.SCENE_GRAPH_OBJECT_TRANSFORM,
        br.ReadSingles);

      this.Meshes = this.ReadNews_<Mesh>(br, BlockType.MESH);
      this.Polygons = this.ReadNews_<Polygon>(br, BlockType.POLYGON);
      this.Textures = this.ReadNews_<Texture>(br, BlockType.TEXTURE);
      this.TextureMaps = this.ReadNews_<TextureMap>(br, BlockType.TEXTURE_MAP);

      this.Vertices
          = this.ReadNews_<Vector3f>(br, BlockType.VERTEX);
      this.VertexIndices = this.ReadNews_(
          br,
          BlockType.POLYGON_VERTEX_MAP,
          br.ReadInt32s);

      this.Normals
          = this.ReadNews_<Vector3f>(br, BlockType.NORMAL);
      this.NormalIndices = this.ReadNews_(
          br,
          BlockType.POLYGON_NORMAL_MAP,
          br.ReadInt32s);

      this.Colors
          = this.ReadNews_<Rgba32>(br, BlockType.COLOR);
      this.ColorIndices = this.ReadNews_(
          br,
          BlockType.POLYGON_COLOR_MAP,
          br.ReadInt32s);

      this.TexCoords
          = this.ReadNews_<Vector2f>(br, BlockType.TEX_COORD);
      this.TexCoordIndices = this.ReadNews_(
          br,
          BlockType.POLYGON_TEX_COORD_MAP,
          br.ReadInt32s);
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