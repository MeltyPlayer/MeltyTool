using System.Collections.Generic;

using fin.io;
using fin.scene;
using fin.util.enumerables;

namespace grezzo.api;

public class ZsiSceneFileBundle(IReadOnlyTreeFile zsiFile)
    : ISceneFileBundle {
  public IReadOnlyTreeFile MainFile => zsiFile;
  public IEnumerable<IReadOnlyGenericFile> Files => zsiFile.Yield();
  public IReadOnlyTreeFile ZsiFile => zsiFile;
}