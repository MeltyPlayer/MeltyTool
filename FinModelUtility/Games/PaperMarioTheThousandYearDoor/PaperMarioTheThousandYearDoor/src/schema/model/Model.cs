using System.Numerics;

using fin.schema.color;

using schema.binary;

using ttyd.schema.model.blocks;

namespace ttyd.schema.model;

public class Model : IBinaryDeserializable {
  public Header Header { get; } = new();

  public Group[] Groups { get; private set; }
  public SceneGraphObject[] SceneGraphObjects { get; private set; }
  public float[] GroupTransforms { get; private set; }

  public Mesh[] Meshes { get; private set; }
  public Polygon[] Polygons { get; private set; }
  public Texture[] Textures { get; private set; }
  public Sampler[] TextureMaps { get; private set; }

  public Vector3[] Vertices { get; private set; }
  public int[] VertexIndices { get; private set; }

  public Vector3[] Normals { get; private set; }
  public int[] NormalIndices { get; private set; }

  public Rgba32[] Colors { get; private set; }
  public int[] ColorIndices { get; private set; }

  public Vector2[] TexCoords { get; private set; }
  public int[] TexCoordIndices { get; private set; }

  public bool[] GroupVisibilities { get; set; }

  public Animation[] Animations { get; private set; }

  public void Read(IBinaryReader br) {
    this.Header.Read(br);

    this.Groups = this.ReadNews_<Group>(br, BlockType.GROUP);
    this.SceneGraphObjects
        = this.ReadNews_<SceneGraphObject>(br, BlockType.SCENE_GRAPH_OBJECT);
    this.GroupTransforms = this.ReadNews_(br,
                                          BlockType.GROUP_TRANSFORM,
                                          br.ReadSingles);

    this.Meshes = this.ReadNews_<Mesh>(br, BlockType.MESH);
    this.Polygons = this.ReadNews_<Polygon>(br, BlockType.POLYGON);
    this.Textures = this.ReadNews_<Texture>(br, BlockType.TEXTURE);
    this.TextureMaps = this.ReadNews_<Sampler>(br, BlockType.SAMPLER);

    this.Vertices = this.ReadVector3s_(br, BlockType.VERTEX_POSITION);
    this.VertexIndices = this.ReadNews_(
        br,
        BlockType.VERTEX_POSITION_INDEX,
        br.ReadInt32s);

    this.Normals = this.ReadVector3s_(br, BlockType.NORMAL);
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

    this.TexCoords = this.ReadVector2s_(br, BlockType.TEX_COORD);
    this.TexCoordIndices = this.ReadNews_(
        br,
        BlockType.POLYGON_TEX_COORD_0_MAP,
        br.ReadInt32s);

    this.GroupVisibilities
        = this.ReadNews_(br,
                         BlockType.GROUP_VISIBILITY,
                         br.ReadSBytes)
              .Select(b => b != 0)
              .ToArray();

    this.Animations = this.ReadNews_<Animation>(
        br,
        BlockType.ANIMATION);
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

  private Vector2[] ReadVector2s_(IBinaryReader br, BlockType blockType) {
    br.Position = this.Header.GetOffset(blockType);
    return br.ReadVector2s(this.Header.GetCount(blockType));
  }

  private Vector3[] ReadVector3s_(IBinaryReader br, BlockType blockType) {
    br.Position = this.Header.GetOffset(blockType);
    return br.ReadVector3s(this.Header.GetCount(blockType));
  }
}