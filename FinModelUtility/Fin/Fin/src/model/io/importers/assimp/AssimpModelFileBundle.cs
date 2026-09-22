using fin.io;

namespace fin.model.io.importers.assimp;

public sealed class AssimpModelFileBundle
    : IModelFileBundle<AssimpModelFileBundle, AssimpModelImporter> {
  public required IReadOnlyTreeFile MainFile { get; init; }
}