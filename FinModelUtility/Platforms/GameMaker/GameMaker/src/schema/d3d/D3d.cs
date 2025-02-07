using schema.text;
using schema.text.reader;

namespace gm.schema.d3d;

public class D3d : ITextDeserializable {
  public D3dCommand[] Commands { get; private set; }

  public void Read(ITextReader tr) {
    tr.AssertInt32(100);
    var vertexCount = tr.ReadInt32();
    this.Commands = tr.ReadNews<D3dCommand>(vertexCount);
  }
}