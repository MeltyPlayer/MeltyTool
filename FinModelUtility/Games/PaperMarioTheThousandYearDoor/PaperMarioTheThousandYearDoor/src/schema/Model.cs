using fin.schema.vector;

using schema.binary;

using ttyd.schema.blocks;

namespace ttyd.schema {
  public class Model : IBinaryDeserializable {
    public Header Header { get; } = new();

    public SceneGraphObject[] SceneGraphObjects { get; private set; }
    public Vector3f[] Vertices { get; private set; }
    public SceneGraph[] SceneGraphs { get; private set; }

    public void Read(IBinaryReader br) {
      this.Header.Read(br);

      this.SceneGraphObjects
          = this.ReadNews_<SceneGraphObject>(br, BlockType.SCENE_GRAPH_OBJECT);
      this.Vertices
          = this.ReadNews_<Vector3f>(br, BlockType.VERTEX);
      this.SceneGraphs = this.ReadNews_<SceneGraph>(br, BlockType.SCENE_GRAPH);
    }

    private T[] ReadNews_<T>(IBinaryReader br, BlockType blockType)
        where T : IBinaryDeserializable, new() {
      br.Position = this.Header.GetOffset(blockType);
      return br.ReadNews<T>(this.Header.GetCount(blockType));
    }
  }
}