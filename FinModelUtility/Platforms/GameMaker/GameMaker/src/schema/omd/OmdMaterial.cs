using schema.text;
using schema.text.reader;

namespace gm.schema.omd;

public struct OmdMaterial : ITextDeserializable {
  public string Name { get; private set; }
  public string TexturePath { get; private set; }

  public void Read(ITextReader tr) {
    this.Name = tr.ReadLine();
    this.TexturePath = tr.ReadLine();

    for (var i = 0; i < 9; i++) {
      tr.ReadLine();
    }
  }
}