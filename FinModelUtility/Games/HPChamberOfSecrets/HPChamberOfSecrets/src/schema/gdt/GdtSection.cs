using schema.binary;

namespace hpcos.schema.gdt;

/// <summary>
///   A section of a GDT file, as referenced by one of its GEO's subfiles.
/// </summary>
public sealed class GdtSection : IBinaryDeserializable {
  public int MeshCount { get; private set; }
  public long MeshesOffset { get; private set; }

  public void Read(IBinaryReader br) {
    long offset = br.Position;

    this.MeshCount = br.ReadInt32At(offset + 0x18);
    this.MeshesOffset = br.ReadRelativePointerAt(offset + 0x1c);
  }
}
