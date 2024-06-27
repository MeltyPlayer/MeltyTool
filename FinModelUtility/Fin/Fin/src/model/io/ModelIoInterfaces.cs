using System.Collections.Generic;
using System.Linq;

using fin.importers;
using fin.io;

namespace fin.model.io;

public interface IModelFileBundle : I3dFileBundle;

public interface IModelPlugin {
  string DisplayName { get; }
  string Description { get; }
  IReadOnlyList<string> KnownPlatforms { get; }
  IReadOnlyList<string> KnownGames { get; }

  IReadOnlyList<string> MainFileExtensions { get; }
  IReadOnlyList<string> FileExtensions { get; }
}

public interface IModelImporterPlugin : IModelPlugin {
  bool SupportsFiles(IEnumerable<IReadOnlySystemFile> files) {
    var fileTypes = files.Select(file => file.FileType).ToArray();

    if (!fileTypes.All(this.FileExtensions.Contains)) {
      return false;
    }

    return fileTypes.Where(this.MainFileExtensions.Contains).Count() == 1;
  }

  IModel Import(IEnumerable<IReadOnlySystemFile> files, float frameRate = 30);
}

public interface IModelExporterPlugin : IModelPlugin {
  void ExportModel(IReadOnlyModel model);
}