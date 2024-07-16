using fin.io;
using fin.model.io;


namespace vrml.api;

public class VrmlModelFileBundle : IModelFileBundle {
  public string? GameName { get; }
  public IReadOnlyTreeFile MainFile => this.WrlFile;
  public required IFileHierarchyFile WrlFile { get; init; }
}