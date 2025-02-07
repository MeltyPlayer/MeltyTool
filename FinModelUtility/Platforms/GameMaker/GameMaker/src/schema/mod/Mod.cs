using schema.text;
using schema.text.reader;

namespace gm.schema.mod {
  public class Mod : ITextDeserializable {
    public ModVertex[] Vertices { get; private set; }

    public void Read(ITextReader tr) {
      var something = tr.ReadInt32();
      var vertexCount = tr.ReadInt32();
      this.Vertices = tr.ReadNews<ModVertex>(vertexCount);
    }
  }
}