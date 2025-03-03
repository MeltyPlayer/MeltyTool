using schema.binary;

namespace sysdolphin.schema;

public class HsdNullTerminatedPointerArray<T> : IBinaryDeserializable
    where T : IBinaryDeserializable, new() {
  private readonly List<T> values_ = new();

  public IReadOnlyList<T> Values => this.values_;

  public void Read(IBinaryReader br) {
    this.values_.Clear();
    while (!br.Eof) {
      var pointer = br.ReadUInt32();
      if (pointer == 0) {
        return;
      }

      this.values_.Add(br.SubreadAt(pointer, br.ReadNew<T>));
    }
  }
}