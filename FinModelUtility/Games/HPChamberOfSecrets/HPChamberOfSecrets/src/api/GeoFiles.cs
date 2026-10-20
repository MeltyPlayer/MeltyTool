using System.Diagnostics.CodeAnalysis;

using fin.io;
using fin.util.asserts;

namespace hpcos.api;

/// <summary>
///   Naming conventions shared by GEO files and their accompanying GDTs.
/// </summary>
public static class GeoFiles {
  /// <summary>
  ///   Some GEOs store submeshes in a GDT with the same name, possibly for
  ///   streaming reasons.
  /// </summary>
  public static bool TryToGetGdt(
      IFileHierarchyFile geoFile,
      [NotNullWhen(true)] out IFileHierarchyFile? gdtFile)
    => Asserts.CastNonnull(geoFile.Parent)
              .TryToGetExistingFile($"{geoFile.NameWithoutExtension}.gdt",
                                    out gdtFile);
}
