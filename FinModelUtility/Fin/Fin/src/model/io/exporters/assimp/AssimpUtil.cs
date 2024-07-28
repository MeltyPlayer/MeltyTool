using System.Collections.Generic;
using System.Linq;

using Assimp;

namespace fin.model.io.exporters.assimp;

public static class AssimpUtil {
  static AssimpUtil() {
    using var ctx = new AssimpContext();
    SupportedExportFormats = ctx.GetSupportedExportFormats();

    ExportFormatsById =
        SupportedExportFormats.ToDictionary(ef => ef.FormatId);
  }

  public static IReadOnlyList<ExportFormatDescription> SupportedExportFormats {
    get;
  }

  public static IReadOnlyDictionary<string, ExportFormatDescription>
      ExportFormatsById { get; }

  public static ExportFormatDescription GetExportFormatFromExtension(
      string extension)
    => extension switch {
        ".dae"  => ExportFormatsById["dae"],
        ".fbx"  => ExportFormatsById["fbx"],
        ".gltf" => ExportFormatsById["gltf2"],
        ".glb"  => ExportFormatsById["glb2"],
        ".obj"  => ExportFormatsById["obj"],
        _       => ExportFormatsById[extension[1..]],
    };
}