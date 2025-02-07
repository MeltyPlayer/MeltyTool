using gm.schema.mod;

using schema.text;
using schema.text.reader;

namespace gm.schema.omd {
  public class OmdMesh : ITextDeserializable {
    public string Name { get; private set; }
    public int MaterialIndex { get; private set; }
    public Mod Mod { get; private set; }

    public void Read(ITextReader tr) {
      this.Name = tr.ReadLine();
      this.MaterialIndex = tr.ReadInt32();
      this.Mod = tr.ReadNew<Mod>();
    }
  }
}