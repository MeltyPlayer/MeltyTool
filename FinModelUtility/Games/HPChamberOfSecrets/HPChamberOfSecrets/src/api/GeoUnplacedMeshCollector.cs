using System.IO.Hashing;
using System.Runtime.InteropServices;
using System.Text;

using fin.io;

using hpcos.schema.geo;

using schema.binary;

namespace hpcos.api;

/// <summary>
///   Collects the meshes that GEO layouts don't place (doors, pickups,
///   effects, etc., which the game positions at runtime), so they can be
///   viewed on their own. The same mesh is often copied into several GEOs, so
///   copies that only differ in their vertex colors (each level's baked
///   lighting) are only included once.
/// </summary>
public sealed class GeoUnplacedMeshCollector {
  private readonly record struct Variant_(
      GeoModelFileBundle GeoBundle,
      string MeshName,
      int MeshOrdinal);

  private static readonly char[] INVALID_NAME_CHARS_
      = [.. Path.GetInvalidFileNameChars(), '/', '\\'];

  /// <summary>
  ///   Variants of each base name with different content, keyed by a hash of
  ///   their content, in the order they were found.
  /// </summary>
  private readonly Dictionary<string, Dictionary<UInt128, Variant_>>
      variantsByBaseName_ = [];

  /// <summary>
  ///   Only GEOs with layouts leave meshes out of their model, so other GEOs
  ///   are skipped.
  /// </summary>
  public void Add(GeoModelFileBundle geoBundle, IBinaryReader geoBr) {
    Geo geo = geoBr.ReadNew<Geo>();
    if (geo.Header.LayoutObjects.Length == 0) {
      return;
    }

    foreach ((int ordinal, (GeoMeshHeader header, UInt128 contentHash)) in
             new GeoMeshTable(geo.Header, geoBr, geoBundle).ReadUnplacedMeshes(
                 (br, header) => (header,
                                  GetContentHash_(br, header, geo.Textures)))) {
      ref Dictionary<UInt128, Variant_>? variants
          = ref CollectionsMarshal.GetValueRefOrAddDefault(
              this.variantsByBaseName_,
              header.BaseName,
              out _);
      variants ??= [];
      variants.TryAdd(contentHash,
                      new Variant_(geoBundle, header.Name, ordinal));
    }
  }

  /// <summary>
  ///   Bundles are named by base name. Variants with different content (e.g.
  ///   different textures) are told apart by the GEO they were first found
  ///   in.
  /// </summary>
  public IEnumerable<GeoModelFileBundle> GetBundles() {
    // Case-insensitive, since these are also used as file names on export.
    HashSet<string> usedDisplayPaths = new(StringComparer.OrdinalIgnoreCase);
    foreach ((string baseName, Dictionary<UInt128, Variant_> variants) in
             this.variantsByBaseName_) {
      string directoryPath
          = GeoModelFileOrganizer.GetOrganizedMeshPath(baseName);
      bool hasDuplicateGeos = variants.Values
                                      .CountBy(v => v.GeoBundle.GeoFile)
                                      .Any(pair => pair.Value > 1);
      foreach (Variant_ variant in variants.Values) {
        IReadOnlyTreeFile geoFile = variant.GeoBundle.GeoFile;
        string displayName = SanitizeName_(
            variants.Count == 1
                ? baseName
                : $"{(hasDuplicateGeos ? variant.MeshName : baseName)} ({geoFile.NameWithoutExtension})");
        yield return new GeoModelFileBundle {
            GeoFile = geoFile,
            GdtFile = variant.GeoBundle.GdtFile,
            VirtualDirectoryPath = directoryPath,
            UnplacedMesh = new GeoUnplacedMesh(
                variant.MeshOrdinal,
                MakeUnique_(
                    usedDisplayPaths,
                    $"{GetHierarchyDisplayPath_(geoFile)}/{directoryPath}/{displayName}")),
        };
      }
    }
  }

  /// <summary>
  ///   Display names can still collide, e.g. if a GEO has several variants
  ///   with the same full name, or different names sanitize to the same one.
  /// </summary>
  private static string MakeUnique_(HashSet<string> usedPaths, string path) {
    string uniquePath = path;
    for (int i = 2; !usedPaths.Add(uniquePath); ++i) {
      uniquePath = $"{path} ({i})";
    }

    return uniquePath;
  }

  /// <summary>
  ///   The display path of the root of the file's hierarchy, which is the
  ///   file's own display path without its path within the hierarchy.
  /// </summary>
  private static string GetHierarchyDisplayPath_(IReadOnlyTreeFile file) {
    file.HasRoot(out _, out string localPath);
    return file.DisplayFullPath[..^localPath.Length].ToString();
  }

  /// <summary>
  ///   Mesh names come from game data and are used as file names, both in the
  ///   file tree and when exporting.
  /// </summary>
  private static string SanitizeName_(string name)
    => string.Join('_', name.Split(INVALID_NAME_CHARS_));

  /// <summary>
  ///   Covers everything that affects how the mesh is imported, except the
  ///   values of its vertex colors. Vertex data is hashed as raw bytes rather
  ///   than decoded, since this runs for every unplaced mesh while gathering.
  ///   Textures are compared by name, since the same texture has a different
  ///   index in each GEO.
  /// </summary>
  private static UInt128 GetContentHash_(IBinaryReader br,
                                         GeoMeshHeader header,
                                         GeoTexture[] textures) {
    XxHash128 hash = new();

    void Append<T>(ReadOnlySpan<T> values) where T : unmanaged {
      hash.Append(BitConverter.GetBytes(values.Length));
      hash.Append(MemoryMarshal.AsBytes(values));
    }

    void AppendBytesAt(long offset, int length) {
      br.Position = offset;
      Append<byte>(br.ReadBytes(length));
    }

    Append<int>([(int) header.VertexFormat, header.PositionShift]);
    AppendBytesAt(header.PositionsOffset,
                  header.PositionCount * header.PositionStride);
    AppendBytesAt(header.TexcoordsOffset,
                  header.TexcoordCount * header.TexcoordStride);
    Append<bool>([header.ColormapCount > 0]);
    foreach (GeoStrip strip in GeoMesh.ReadStrips(br, header)) {
      Append<byte>([strip.OverlayPass]);
      foreach (GeoStripLayer layer in strip.Layers) {
        Append<GeoLayerFlags>(
            [layer.Flags & GeoModelImporter.SUPPORTED_LAYER_FLAGS]);
        Append<byte>(layer.TextureIndex >= 0 &&
                     layer.TextureIndex < textures.Length
                         ? Encoding.UTF8.GetBytes(
                             textures[layer.TextureIndex].Name)
                         : []);
      }

      Append<GeoStripVertex>(strip.Vertices);
    }

    return hash.GetCurrentHashAsUInt128();
  }
}
