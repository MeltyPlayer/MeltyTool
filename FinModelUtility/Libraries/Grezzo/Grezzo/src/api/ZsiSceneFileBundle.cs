using System.Collections.Generic;

using fin.io;
using fin.scene;
using fin.util.enumerables;

namespace grezzo.api;

public sealed class ZsiSceneFileBundle(IReadOnlyTreeFile zsiFile)
    : ISceneFileBundle {
  public IReadOnlyTreeFile MainFile => zsiFile;
  public IEnumerable<IReadOnlyStandaloneFile> Files => zsiFile.Yield();
  public IReadOnlyTreeFile ZsiFile => zsiFile;
}