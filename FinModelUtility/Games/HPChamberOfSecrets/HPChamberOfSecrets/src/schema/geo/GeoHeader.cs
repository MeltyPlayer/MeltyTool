using schema.binary;

namespace hpcos.schema.geo;

/// <summary>
///   Header of a GameCube GEO file: its counts and tables, without any
///   texture or mesh data.
/// </summary>
public sealed class GeoHeader : IBinaryDeserializable {
  public const byte SUPPORTED_VERSION = 0x10;
  private const int VERSION_OFFSET_ = 0x06;

  public byte Version { get; private set; }

  /// <summary>
  ///   Total number of meshes, including those stored in the GDT.
  /// </summary>
  public int TotalMeshCount { get; private set; }

  /// <summary>
  ///   Offset of the GEO's mesh table, which only contains the meshes that
  ///   aren't stored in its GDT.
  /// </summary>
  public long MeshesOffset { get; private set; }

  public int TextureCount { get; private set; }
  public long TexturesOffset { get; private set; }

  public GeoLayoutObject[] LayoutObjects { get; private set; } = [];

  /// <summary>
  ///   Indices of the meshes that layout objects place.
  /// </summary>
  public IReadOnlySet<int> PlacedMeshIndices { get; private set; }
    = new HashSet<int>();

  public GeoSubfile[] Subfiles { get; private set; } = [];

  public static byte ReadVersion(IBinaryReader br) {
    br.Position = VERSION_OFFSET_;
    return br.ReadByte();
  }

  public void Read(IBinaryReader br) {
    this.Version = ReadVersion(br);
    if (this.Version != SUPPORTED_VERSION) {
      throw new NotSupportedException(
          $"Unsupported GEO version: 0x{this.Version:X2}");
    }

    this.TotalMeshCount = br.ReadInt32At(0x0c);
    this.MeshesOffset = br.ReadRelativePointerAt(0x10);

    this.TextureCount = br.ReadInt32At(0x14);
    this.TexturesOffset = br.ReadRelativePointerAt(0x18);

    // Only the first layout is used.
    int layoutCount = br.ReadInt32At(0x2c);
    long layoutsOffset = br.ReadRelativePointerAt(0x30);
    if (layoutCount > 0) {
      int objectCount = br.ReadInt32At(layoutsOffset + 0x64);
      long objectsOffset = br.ReadRelativePointerAt(layoutsOffset + 0x68);
      this.LayoutObjects
          = br.ReadNewsAt<GeoLayoutObject>(objectsOffset, objectCount);
      this.PlacedMeshIndices = this.LayoutObjects
                                   .Select(o => o.MeshIndex)
                                   .Where(meshIndex => meshIndex >= 0)
                                   .ToHashSet();
    }

    int subfileCount = br.ReadInt32At(0x64);
    long subfilesOffset = br.ReadRelativePointerAt(0x68);
    this.Subfiles = br.ReadNewsAt<GeoSubfile>(subfilesOffset, subfileCount);
  }
}
