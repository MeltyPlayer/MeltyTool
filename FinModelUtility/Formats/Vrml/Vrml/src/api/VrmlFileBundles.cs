using fin.io;
using fin.io.bundles;
using fin.model.io;
using fin.scene;


namespace vrml.api;

public interface IVrmlFileBundle : IFileBundle {
  IReadOnlyTreeFile WrlFile { get; }
  IReadOnlyTreeFile IFileBundle.MainFile => this.WrlFile;
}

public sealed class VrmlModelFileBundle
    : IVrmlFileBundle,
      IModelFileBundle<VrmlModelFileBundle, VrmlModelImporter> {
  public string? GameName { get; }
  public required IReadOnlyTreeFile WrlFile { get; init; }
}

public sealed class VrmlSceneFileBundle : IVrmlFileBundle, ISceneFileBundle {
  public string? GameName { get; }
  public required IReadOnlyTreeFile WrlFile { get; init; }
}