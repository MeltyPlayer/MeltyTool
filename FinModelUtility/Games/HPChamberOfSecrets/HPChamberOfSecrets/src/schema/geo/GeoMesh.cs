using System.Numerics;

using schema.binary;

namespace hpcos.schema.geo;

[Flags]
public enum GeoVertexFormat {
  /// <summary>
  ///   Positions are floats, otherwise s16s with PositionShift fractional bits.
  /// </summary>
  FLOAT_POSITIONS = 0x01,

  /// <summary>
  ///   Each position is followed by an s16 normal with 14 fractional bits.
  /// </summary>
  S16_NORMALS = 0x04,

  /// <summary>
  ///   Each position is followed by an s8 normal with 6 fractional bits + 1
  ///   byte of padding.
  /// </summary>
  S8_NORMALS = 0x08,

  FLOAT_TEXCOORDS = 0x10,

  /// <summary>
  ///   Texcoords are s16s with 13 fractional bits.
  /// </summary>
  S16_TEXCOORDS = 0x20,
}

/// <summary>
///   A mesh's vertex and strip data, read from where its header points.
/// </summary>
public sealed class GeoMesh {
  private GeoMesh(GeoMeshHeader header) {
    this.Header = header;
  }

  public GeoMeshHeader Header { get; }

  public string Name => this.Header.Name;
  public int Index => this.Header.Index;

  public Vector3[] Positions { get; private set; } = [];

  /// <summary>
  ///   Per-position normals. Null if the mesh has no normals.
  /// </summary>
  public Vector3[]? Normals { get; private set; }

  public Vector2[] Texcoords { get; private set; } = [];

  /// <summary>
  ///   Per-vertex RGBA colors, indexed by the texcoord index of the first
  ///   texture layer. Null if the mesh has no colors.
  /// </summary>
  public byte[]? Colors { get; private set; }

  public GeoStrip[] Strips { get; private set; } = [];

  /// <summary>
  ///   Discards the mesh's colors if they're all black. Meshes that the game
  ///   positions at runtime are often copied into levels with a blank
  ///   colormap, since their lighting isn't baked, so they'd otherwise render
  ///   pure black.
  /// </summary>
  public void DiscardColorsIfUnlit() {
    if (this.Colors == null) {
      return;
    }

    for (int i = 0; i < this.Colors.Length; i += 4) {
      if (this.Colors[i] != 0 ||
          this.Colors[i + 1] != 0 ||
          this.Colors[i + 2] != 0) {
        return;
      }
    }

    this.Colors = null;
  }

  public static GeoMesh Read(IBinaryReader br, GeoMeshHeader header) {
    GeoMesh mesh = new(header);
    mesh.ReadPositions_(br);

    if (!header.IsCollisionMesh) {
      mesh.ReadTexcoords_(br);

      // Only the first colormap is used, the rest appear to be alternate
      // lighting setups.
      if (header.ColormapCount > 0) {
        br.Position = header.ColorsOffset;
        mesh.Colors = br.ReadBytes(4 * header.TexcoordCount);
      }

      mesh.Strips = ReadStrips(br, header);
    }

    return mesh;
  }

  /// <summary>
  ///   Reads only the strips, for callers that don't need the vertex data.
  /// </summary>
  public static GeoStrip[] ReadStrips(IBinaryReader br, GeoMeshHeader header) {
    List<GeoStrip> strips = [];
    for (int i = 0; i < header.StripGroupCount; ++i) {
      long stripGroupOffset = header.StripGroupsOffset + i * 0x1c;
      int stripCount = br.ReadInt32At(stripGroupOffset);
      long stripOffset = br.ReadRelativePointerAt(stripGroupOffset + 0x10);

      // A pointer to itself is a null pointer, i.e. the group has no strips.
      if (stripOffset == stripGroupOffset + 0x10) {
        continue;
      }

      strips.AddRange(br.ReadNewsAt<GeoStrip>(stripOffset, stripCount));
    }

    return [.. strips];
  }

  private void ReadPositions_(IBinaryReader br) {
    GeoVertexFormat vertexFormat = this.Header.VertexFormat;
    bool hasFloatPositions
        = vertexFormat.HasFlag(GeoVertexFormat.FLOAT_POSITIONS);
    bool hasS16Normals = vertexFormat.HasFlag(GeoVertexFormat.S16_NORMALS);
    bool hasS8Normals
        = !hasS16Normals && vertexFormat.HasFlag(GeoVertexFormat.S8_NORMALS);
    float positionScale = 1f / (1 << this.Header.PositionShift);

    int positionCount = this.Header.PositionCount;
    this.Positions = new Vector3[positionCount];
    this.Normals = hasS16Normals || hasS8Normals
        ? new Vector3[positionCount]
        : null;
    br.Position = this.Header.PositionsOffset;
    for (int i = 0; i < positionCount; ++i) {
      this.Positions[i] = hasFloatPositions
          ? br.ReadVector3()
          : new Vector3(br.ReadInt16(), br.ReadInt16(), br.ReadInt16()) *
            positionScale;

      if (hasS16Normals) {
        this.Normals![i]
            = new Vector3(br.ReadInt16(), br.ReadInt16(), br.ReadInt16()) /
              16384f;
      } else if (hasS8Normals) {
        this.Normals![i]
            = new Vector3(br.ReadSByte(), br.ReadSByte(), br.ReadSByte()) /
              64f;
        br.Position += 1;
      }
    }
  }

  private void ReadTexcoords_(IBinaryReader br) {
    int texcoordCount = this.Header.TexcoordCount;
    this.Texcoords = new Vector2[texcoordCount];

    br.Position = this.Header.TexcoordsOffset;
    GeoVertexFormat vertexFormat = this.Header.VertexFormat;
    if (vertexFormat.HasFlag(GeoVertexFormat.S16_TEXCOORDS)) {
      for (int i = 0; i < texcoordCount; ++i) {
        this.Texcoords[i] = new Vector2(br.ReadInt16(), br.ReadInt16()) /
                            8192f;
      }
    } else if (vertexFormat.HasFlag(GeoVertexFormat.FLOAT_TEXCOORDS)) {
      for (int i = 0; i < texcoordCount; ++i) {
        this.Texcoords[i] = new Vector2(br.ReadSingle(), br.ReadSingle());
      }
    }
  }
}
