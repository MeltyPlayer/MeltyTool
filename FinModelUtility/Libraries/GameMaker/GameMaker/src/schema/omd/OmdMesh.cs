using gm.schema.d3d;

using schema.text;
using schema.text.reader;

namespace gm.schema.omd;

public class OmdMesh : ITextDeserializable {
  public string Name { get; private set; }
  public int MaterialIndex { get; private set; }
  public D3d D3d { get; private set; }

  public void Read(ITextReader tr) {
    this.Name = tr.ReadLine();
    this.MaterialIndex = tr.ReadInt32();
    this.D3d = tr.ReadNew<D3d>();
  }
}