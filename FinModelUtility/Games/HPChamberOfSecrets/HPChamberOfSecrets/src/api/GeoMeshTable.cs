using fin.io;

using hpcos.schema;
using hpcos.schema.gdt;
using hpcos.schema.geo;

using schema.binary;

namespace hpcos.api;

/// <summary>
///   Reads a GEO's meshes, which are split between the GEO's own mesh table
///   and sections of its GDT.
/// </summary>
public sealed class GeoMeshTable(
    GeoHeader geoHeader,
    IBinaryReader geoBr,
    GeoModelFileBundle fileBundle) {
  public List<GeoMesh> ReadMeshes()
    => this.Read_((_, _) => true, GeoMesh.Read).ConvertAll(m => m.Value);

  /// <summary>
  ///   Reads the mesh at the given ordinal (see Read_()).
  /// </summary>
  public GeoMesh ReadMesh(int ordinal)
    => this.Read_((i, _) => i == ordinal, GeoMesh.Read).Single().Value;

  /// <summary>
  ///   Meshes that the GEO's layout objects place. Layout objects reference
  ///   meshes by index, so if several meshes share an index, only the first is
  ///   placed and the rest are treated as unplaced.
  /// </summary>
  public List<GeoMesh> ReadPlacedMeshes() {
    Func<GeoMeshHeader, bool> isPlaced = this.CreateIsPlacedFilter_();
    return this.Read_((_, header) => isPlaced(header), GeoMesh.Read)
               .ConvertAll(m => m.Value);
  }

  /// <summary>
  ///   Meshes with geometry that the GEO's layout objects don't place, which
  ///   the game positions at runtime (doors, gates, pickups, effects). Each
  ///   is passed to the given read function, so callers can read only what
  ///   they need.
  /// </summary>
  public List<(int Ordinal, T Value)> ReadUnplacedMeshes<T>(
      Func<IBinaryReader, GeoMeshHeader, T> read) {
    Func<GeoMeshHeader, bool> isPlaced = this.CreateIsPlacedFilter_();
    return this.Read_(
        (_, header) => !isPlaced(header) && header.HasGeometry,
        read);
  }

  /// <summary>
  ///   Stateful, since only the first mesh with each placed index counts, so
  ///   it must see every header in order.
  /// </summary>
  private Func<GeoMeshHeader, bool> CreateIsPlacedFilter_() {
    HashSet<int> visitedPlacedIndices = [];
    return header => geoHeader.PlacedMeshIndices.Contains(header.Index) &&
                     visitedPlacedIndices.Add(header.Index);
  }

  /// <summary>
  ///   Reads the meshes whose headers match the filter, along with their
  ///   ordinal: their position across all of the GEO's mesh tables, which
  ///   identifies a mesh even if its name or index is shared. The filter sees
  ///   every header in order.
  /// </summary>
  private List<(int Ordinal, T Value)> Read_<T>(
      Func<int, GeoMeshHeader, bool> filter,
      Func<IBinaryReader, GeoMeshHeader, T> read) {
    List<(int, T)> values = [];
    int ordinal = 0;
    this.ForEachTable_((br, offset, count) => {
      foreach (GeoMeshHeader header in
               br.ReadNewsAt<GeoMeshHeader>(offset, count)) {
        if (filter(ordinal, header)) {
          values.Add((ordinal, read(br, header)));
        }

        ++ordinal;
      }
    });
    return values;
  }

  /// <summary>
  ///   Meshes in the GDT are counted as part of the GEO's total mesh count, so
  ///   they're visited first to know how many meshes the GEO itself has.
  /// </summary>
  private void ForEachTable_(Action<IBinaryReader, long, int> handleTable) {
    int gdtMeshCount = 0;
    if (geoHeader.Subfiles.Length > 1) {
      IReadOnlyTreeFile gdtFile = fileBundle.GdtFile ??
                                  throw new FileNotFoundException(
                                      $"GEO has subfiles, but no accompanying GDT was found for {fileBundle.GeoFile.DisplayFullPath}!");

      using IBinaryReader gdtBr = gdtFile.OpenReadAsBinary(Endianness.BigEndian);
      foreach (GeoSubfile subfile in geoHeader.Subfiles.Skip(1)) {
        gdtBr.Position = subfile.GdtOffset;
        GdtSection gdtSection = gdtBr.ReadNew<GdtSection>();
        handleTable(gdtBr, gdtSection.MeshesOffset, gdtSection.MeshCount);
        gdtMeshCount += gdtSection.MeshCount;
      }
    }

    handleTable(geoBr,
                geoHeader.MeshesOffset,
                Math.Max(0, geoHeader.TotalMeshCount - gdtMeshCount));
  }
}
