using System.Collections.Generic;
using System.Linq;

using Assimp;

namespace fin.model.io.exporters.assimp;

public static class AssimpUtil {
  static AssimpUtil() {
    using var ctx = new AssimpContext();
    AssimpUtil.SupportedExportFormats = ctx.GetSupportedExportFormats();

    AssimpUtil.ExportFormatsById = 
        AssimpUtil.SupportedExportFormats.ToDictionary(ef => ef.FormatId);
  }

  public static IReadOnlyList<ExportFormatDescription> SupportedExportFormats {
    get;
  }

  public static IReadOnlyDictionary<string, ExportFormatDescription> ExportFormatsById {
    get;
  }

  public static ExportFormatDescription GetExportFormatFromExtension(
      string extension) {
    var extensionsById = ExportFormatsById;
    return extension switch {
        ".gltf" => extensionsById["gltf2"],
        ".glb"  => extensionsById["glb2"],
        ".fbx"  => extensionsById["fbx"],
        _       => extensionsById[extension[1..]],
    };
  }
}