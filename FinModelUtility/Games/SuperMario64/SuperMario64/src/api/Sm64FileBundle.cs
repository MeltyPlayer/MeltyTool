using fin.io;
using fin.io.bundles;

namespace sm64.api {
  public abstract class BSm64FileBundle : IFileBundle {
    public abstract IReadOnlyTreeFile? MainFile { get; }
  }
}