using System.Collections.Generic;
using System.Linq;

using fin.io;

namespace fin.model.io.importers.assimp;

public class AssimpModelImporterPlugin : IModelImporterPlugin {
  public string DisplayName => "Assimp";
  public string Description => "Loads standard model formats via Assimp.";

  public IReadOnlyList<string> KnownPlatforms { get; } = [];
  public IReadOnlyList<string> KnownGames { get; } = [];

  public IReadOnlyList<string> MainFileExtensions { get; }
    = [".glb", ".gltf", ".fbx", ".obj"];

  public IReadOnlyList<string> FileExtensions => this.MainFileExtensions;

  public IModel Import(IEnumerable<IReadOnlySystemFile> files,
                       float frameRate = 30) {
    var assimpBundle = new AssimpModelFileBundle {
        MainFile = files.Single(),
    };
    return new AssimpModelImporter().Import(assimpBundle);
  }
}