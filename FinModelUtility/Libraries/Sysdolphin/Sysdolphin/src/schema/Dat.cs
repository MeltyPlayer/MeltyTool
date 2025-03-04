using schema.binary;

namespace sysdolphin.schema;

public class Dat : IBinaryDeserializable {
  private const bool IGNORE_ERRORS = false;

  public LinkedList<DatSubfile> Subfiles { get; } = [];

  public void Read(IBinaryReader br) {
    do {
      if (!IGNORE_ERRORS) {
        var offset = br.Position;
        br.PushLocalSpace();
        var subfile = br.ReadNew<DatSubfile>();
        br.PopLocalSpace();

        this.Subfiles.AddLast(subfile);
        br.Position = offset + subfile.FileSize;
        br.Align(0x20);
      } else {
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
      }
    } while (!br.Eof);
  }
}