using schema.text;
using schema.text.reader;

namespace pmdc.schema.omd {
  public class Omd : ITextDeserializable {
    public OmdMaterial[] Materials { get; set; }
    public OmdMesh[] Meshes { get; set; }

    public void Read(ITextReader tr) {
      var something = tr.ReadInt32();

      var materialCount = tr.ReadInt32();
      this.Materials = tr.ReadNews<OmdMaterial>(materialCount);

      var meshCount = tr.ReadInt32();
      this.Meshes = tr.ReadNews<OmdMesh>(meshCount);
    }
  }
}