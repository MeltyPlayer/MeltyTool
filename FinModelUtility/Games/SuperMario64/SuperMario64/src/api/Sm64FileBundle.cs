using fin.io;
using fin.io.bundles;

namespace sm64.api {
  public abstract class BSm64FileBundle : IFileBundle {
    public string GameName => "super_mario_64";
    public abstract IReadOnlyTreeFile? MainFile { get; }
  }
}