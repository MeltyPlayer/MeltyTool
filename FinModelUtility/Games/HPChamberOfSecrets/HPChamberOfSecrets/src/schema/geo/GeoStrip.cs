using schema.binary;

namespace hpcos.schema.geo;

/// <summary>
///   Render flags for a single texture layer of a strip. Meanings were
///   inferred by correlating them with texture names, so only the blending
///   flags are known for certain.
/// </summary>
[Flags]
public enum GeoLayerFlags : ushort {
  /// <summary>
  ///   Probably environment/sphere mapping (ENV_* textures).
  /// </summary>
  ENVIRONMENT_MAP = 0x02,

  /// <summary>
  ///   Probably alpha blending (glass, curtains, foliage).
  /// </summary>
  ALPHA_BLEND = 0x04,

  /// <summary>
  ///   Probably disables backface culling (cobwebs, pennants, foliage,
  ///   character bodies). Foliage without this flag has its back faces
  ///   modeled as separate triangles instead, while foliage with it never
  ///   does.
  /// </summary>
  DOUBLE_SIDED = 0x08,

  /// <summary>
  ///   Brightens the surface underneath (glows, lightbeams).
  /// </summary>
  ADDITIVE = 0x10,

  /// <summary>
  ///   Darkens the surface underneath (shadows, dirt decals).
  /// </summary>
  SUBTRACTIVE = 0x20,

  /// <summary>
  ///   Probably alpha testing, always combined with ALPHA_BLEND (foliage).
  /// </summary>
  ALPHA_TEST = 0x80,

  /// <summary>
  ///   Probably UV scrolling (*SCROLL* textures).
  /// </summary>
  UV_SCROLL = 0x100,
}

public readonly record struct GeoStripLayer(
    GeoLayerFlags Flags,
    short TextureIndex);

/// <summary>
///   Each vertex has one texcoord index per layer, but only the first and
///   last layers' are needed (see GeoModelImporter.AddMesh_()).
/// </summary>
public readonly record struct GeoStripVertex(
    ushort PositionIndex,
    ushort FirstTexcoordIndex,
    ushort LastTexcoordIndex);

/// <summary>
///   A triangle strip that may be drawn multiple times, once per texture
///   layer. Each vertex indexes positions and texcoords separately.
/// </summary>
public sealed class GeoStrip : IBinaryDeserializable {
  /// <summary>
  ///   0 for regular geometry. Otherwise, this strip is an overlay pass (env
  ///   map shine, shadows, terrain blends) drawn exactly on top of existing
  ///   geometry, where higher values are drawn later. Every overlay triangle
  ///   in the game coincides with a triangle from a non-overlay strip.
  /// </summary>
  public byte OverlayPass { get; private set; }

  public GeoStripLayer[] Layers { get; private set; }
  public GeoStripVertex[] Vertices { get; private set; }

  public void Read(IBinaryReader br) {
    // Header: u16 triangle count, u8 layer count, u8 overlay pass, 2 unknown
    // bytes, then a (u16 flags, u16 texture index) pair for each layer.
    ushort triangleCount = br.ReadUInt16();
    byte layerCount = br.ReadByte();
    this.OverlayPass = br.ReadByte();
    br.Position += 2;

    this.Layers = new GeoStripLayer[layerCount];
    for (int i = 0; i < layerCount; ++i) {
      GeoLayerFlags flags = (GeoLayerFlags) br.ReadUInt16();
      short textureIndex = br.ReadInt16();
      this.Layers[i] = new GeoStripLayer(flags, textureIndex);
    }

    Span<ushort> texcoordIndices = stackalloc ushort[layerCount];
    this.Vertices = new GeoStripVertex[triangleCount + 2];
    for (int i = 0; i < this.Vertices.Length; ++i) {
      ushort positionIndex = br.ReadUInt16();
      br.ReadUInt16s(texcoordIndices);
      this.Vertices[i] = layerCount > 0
          ? new GeoStripVertex(positionIndex,
                               texcoordIndices[0],
                               texcoordIndices[^1])
          : new GeoStripVertex(positionIndex, 0, 0);
    }
  }
}
