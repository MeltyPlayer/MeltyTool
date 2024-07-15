using fin.io;
using fin.scene;


namespace vrml.api;

public class VrmlSceneFileBundle : ISceneFileBundle {
  public string? GameName { get; }
  public IReadOnlyTreeFile MainFile => this.WrlFile;
  public required IFileHierarchyFile WrlFile { get; init; }
}