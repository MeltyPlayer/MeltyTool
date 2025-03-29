using schema.binary;

namespace gm.schema.dataWin.chunk;

public abstract class BListChunk<T> : IBinaryDeserializable
    where T : IBinaryDeserializable, new() {
  public T[] Elements { get; set; }

  public void Read(IBinaryReader br) {
    var addressCount = br.ReadUInt32();
    this.Elements = new T[addressCount];

    for (var i = 0; i < addressCount; ++i) {
      var address = br.ReadUInt32();

      var tmp = br.Position;
      br.Position = address;

      this.Elements[i] = br.ReadNew<T>();

      br.Position = tmp;
    }
  }
}