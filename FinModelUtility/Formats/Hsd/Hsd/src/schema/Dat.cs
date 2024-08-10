using schema.binary;

namespace sysdolphin.schema;

public class Dat : IBinaryDeserializable {
  public LinkedList<DatSubfile> Subfiles { get; } = [];

  public void Read(IBinaryReader br) {
    do {
      var offset = br.Position;
      try {
        br.PushLocalSpace();
        var subfile = br.ReadNew<DatSubfile>();
        br.PopLocalSpace();

        this.Subfiles.AddLast(subfile);
        br.Position = offset + subfile.FileSize;
        br.Align(0x20);
      } catch (Exception e) {
        br.PopLocalSpace();
        br.Position = offset;
        break;
      }
    } while (!br.Eof);
  }
}