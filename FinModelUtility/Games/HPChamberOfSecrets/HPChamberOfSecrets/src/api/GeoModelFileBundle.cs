using fin.io;
using fin.model.io;

namespace hpcos.api;

/// <summary>
///   A single mesh that GEO layouts don't place, shown in the file tree under
///   its organized directory rather than its GEO's.
/// </summary>
/// <param name="MeshOrdinal">The mesh's position across its GEO's mesh
/// tables (see GeoMeshTable), since names and indices can be shared.</param>
/// <param name="DisplayFullPath">Where the mesh is shown in the file tree.
/// </param>
public sealed record GeoUnplacedMesh(int MeshOrdinal, string DisplayFullPath);

/// <summary>
///   Bundle that represents a GEO model and its associated GDT, if any, or
///   just one of its unplaced meshes.
/// </summary>
public sealed class GeoModelFileBundle
    : IModelFileBundle<GeoModelFileBundle, GeoModelImporter> {
  public required IReadOnlyTreeFile GeoFile { get; init; }
  public IReadOnlyTreeFile? GdtFile { get; init; }
  public GeoUnplacedMesh? UnplacedMesh { get; init; }

  public IReadOnlyTreeFile MainFile => this.GeoFile;

  public required string VirtualDirectoryPath { get; init; }

  public ReadOnlySpan<char> DisplayFullPath
    => this.UnplacedMesh != null
        ? this.UnplacedMesh.DisplayFullPath
        : this.GeoFile.DisplayFullPath;

  /// <summary>
  ///   Creates a bundle for a GEO, along with its GDT if one exists.
  /// </summary>
  public static GeoModelFileBundle Create(IFileHierarchyFile geoFile) {
    GeoFiles.TryToGetGdt(geoFile, out IFileHierarchyFile? gdtFile);
    return new GeoModelFileBundle {
        GeoFile = geoFile,
        GdtFile = gdtFile,
        VirtualDirectoryPath = GeoModelFileOrganizer.GetOrganizedGeoPath(
            geoFile.NameWithoutExtension.ToString()),
    };
  }
}
