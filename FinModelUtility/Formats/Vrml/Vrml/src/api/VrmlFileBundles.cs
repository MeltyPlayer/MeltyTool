using fin.io;
using fin.io.bundles;
using fin.model.io;
using fin.scene;


namespace vrml.api;

public interface IVrmlFileBundle : IFileBundle {
  IFileHierarchyFile WrlFile { get; }
  IReadOnlyTreeFile IFileBundle.MainFile => this.WrlFile;
}

public class VrmlModelFileBundle : IVrmlFileBundle, IModelFileBundle {
  public string? GameName { get; }
  public required IFileHierarchyFile WrlFile { get; init; }
}

public class VrmlSceneFileBundle : IVrmlFileBundle, ISceneFileBundle {
  public string? GameName { get; }
  public required IFileHierarchyFile WrlFile { get; init; }
}