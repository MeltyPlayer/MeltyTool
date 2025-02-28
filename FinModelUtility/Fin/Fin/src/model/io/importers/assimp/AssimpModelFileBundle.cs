using fin.io;

namespace fin.model.io.importers.assimp;

public class AssimpModelFileBundle : IModelFileBundle {
  public required IReadOnlyTreeFile MainFile { get; init; }
}