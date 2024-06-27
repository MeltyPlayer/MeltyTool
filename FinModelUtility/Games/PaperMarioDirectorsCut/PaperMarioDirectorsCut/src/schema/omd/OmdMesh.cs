using schema.text;
using schema.text.reader;

namespace pmdc.schema.omd {
  public struct OmdMesh : ITextDeserializable {
    public string Name { get; private set; }
    public int MaterialIndex { get; private set; }
    public OmdVertex[] Vertices { get; private set; }

    public void Read(ITextReader tr) {
      this.Name = tr.ReadLine();
      this.MaterialIndex = tr.ReadInt32();
      var something = tr.ReadInt32();

      var vertexCount = tr.ReadInt32();
      this.Vertices = tr.ReadNews<OmdVertex>(vertexCount);
    }
  }
}