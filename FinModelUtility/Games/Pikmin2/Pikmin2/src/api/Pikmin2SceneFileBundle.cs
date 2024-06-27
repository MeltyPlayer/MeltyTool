using fin.io;
using fin.scene;

namespace games.pikmin2.api;

public class Pikmin2SceneFileBundle : ISceneFileBundle {
  public string? GameName => "pikmin_2";
  public IReadOnlyTreeFile? MainFile => LevelBmd;

  public required IReadOnlyTreeFile LevelBmd { get; init; }
  public required IReadOnlyTreeFile RouteTxt { get; init; }
}