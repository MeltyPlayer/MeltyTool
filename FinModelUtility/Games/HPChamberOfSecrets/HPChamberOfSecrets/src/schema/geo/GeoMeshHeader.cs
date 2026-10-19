using System.Globalization;

using schema.binary;

namespace hpcos.schema.geo;

/// <summary>
///   A mesh's entry in a GEO/GDT mesh table, without its vertex or strip
///   data.
/// </summary>
public sealed class GeoMeshHeader : IBinaryDeserializable {
  public const int SIZE = 0xec;

  public long Offset { get; private set; }

  public string Name { get; private set; }

  /// <summary>
  ///   Index of this mesh within the GEO's mesh table, which is only stored
  ///   as the "_X" suffix of the mesh's name. -1 if the name has no such
  ///   suffix.
  /// </summary>
  public int Index { get; private set; }

  /// <summary>
  ///   The mesh's name without its "_X" index suffix, which is how the same
  ///   mesh is named across GEOs.
  /// </summary>
  public string BaseName { get; private set; }

  public int PositionCount { get; private set; }
  public long PositionsOffset { get; private set; }
  public long ColorsOffset { get; private set; }
  public int ColormapCount { get; private set; }
  public long TexcoordsOffset { get; private set; }
  public GeoVertexFormat VertexFormat { get; private set; }
  public int PositionShift { get; private set; }
  public int StripGroupCount { get; private set; }
  public long StripGroupsOffset { get; private set; }
  public int TexcoordCount { get; private set; }

  /// <summary>
  ///   Meshes without positions are collision meshes, which aren't supported
  ///   yet.
  /// </summary>
  public bool IsCollisionMesh => this.PositionCount == 0;

  public bool HasGeometry => !this.IsCollisionMesh && this.StripGroupCount > 0;

  /// <summary>
  ///   Size of each position, along with its normal if it has one.
  /// </summary>
  public int PositionStride
    => (this.VertexFormat.HasFlag(GeoVertexFormat.FLOAT_POSITIONS) ? 12 : 6) +
       (this.VertexFormat.HasFlag(GeoVertexFormat.S16_NORMALS) ? 6
        : this.VertexFormat.HasFlag(GeoVertexFormat.S8_NORMALS) ? 4
        : 0);

  public int TexcoordStride
    => this.VertexFormat.HasFlag(GeoVertexFormat.S16_TEXCOORDS) ? 4
        : this.VertexFormat.HasFlag(GeoVertexFormat.FLOAT_TEXCOORDS) ? 8
        : 0;

  public void Read(IBinaryReader br) {
    this.Offset = br.Position;

    this.Name = br.ReadRelativeStringAt(this.Offset + 0x04);
    this.PositionCount = br.ReadInt32At(this.Offset + 0x0c);
    this.PositionsOffset = br.ReadRelativePointerAt(this.Offset + 0x10);
    this.ColorsOffset = br.ReadRelativePointerAt(this.Offset + 0x14);
    this.ColormapCount = br.ReadInt32At(this.Offset + 0x74);
    this.TexcoordsOffset = br.ReadRelativePointerAt(this.Offset + 0x98);
    this.VertexFormat = (GeoVertexFormat) br.ReadInt32At(this.Offset + 0xc4);
    this.PositionShift = br.ReadInt32At(this.Offset + 0xc8);
    this.StripGroupCount = br.ReadInt32At(this.Offset + 0xcc);
    this.StripGroupsOffset = br.ReadRelativePointerAt(this.Offset + 0xd0);
    this.TexcoordCount = br.ReadInt32At(this.Offset + 0xe0);

    int indexSeparator = this.Name.LastIndexOf('_');
    if (indexSeparator >= 0 &&
        int.TryParse(this.Name.AsSpan(indexSeparator + 1),
                     NumberStyles.None,
                     CultureInfo.InvariantCulture,
                     out int index)) {
      this.Index = index;
      this.BaseName = this.Name[..indexSeparator];
    } else {
      this.Index = -1;
      this.BaseName = this.Name;
    }

    br.Position = this.Offset + SIZE;
  }
}
