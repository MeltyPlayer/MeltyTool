using schema.binary;

namespace hpcos.schema.geo;

/// <summary>
///   A GameCube GEO file's header and texture headers. Meshes are read
///   separately, since some of them are stored in the GEO's GDT, and texture
///   data is read on demand (see GeoTexture.ToImage()).
/// </summary>
public sealed class Geo : IBinaryDeserializable {
  public GeoHeader Header { get; private set; }
  public GeoTexture[] Textures { get; private set; }

  public void Read(IBinaryReader br) {
    this.Header = br.ReadNew<GeoHeader>();
    this.Textures = br.ReadNewsAt<GeoTexture>(this.Header.TexturesOffset,
                                              this.Header.TextureCount);
  }
}
