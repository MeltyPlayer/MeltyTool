using schema.binary;

namespace hpcos.schema.geo;

/// <summary>
///   Describes a section of the accompanying GDT file. The first subfile is
///   the GEO itself, the rest are stored in the GDT.
/// </summary>
public sealed class GeoSubfile : IBinaryDeserializable {
  public const int SIZE = 0x4c;

  public int GdtOffset { get; private set; }

  public void Read(IBinaryReader br) {
    long offset = br.Position;

    this.GdtOffset = br.ReadInt32At(offset + 0x1c);

    br.Position = offset + SIZE;
  }
}
