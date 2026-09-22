using fin.io;
using fin.scene;

using modl.schema.xml;

namespace modl.api;

public sealed class BwSceneFileBundle : ISceneFileBundle {
  public IReadOnlyTreeFile MainFile => this.MainXmlFile;

  public required GameVersion GameVersion { get; init; }
  public required IReadOnlyTreeFile MainXmlFile { get; init; }
}

public sealed class BwSceneImporter : ISceneImporter<BwSceneFileBundle> {
  public IScene Import(BwSceneFileBundle sceneFileBundle)
    => new LevelXmlParser().Parse(sceneFileBundle);
}