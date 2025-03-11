using System.Collections.Generic;

using fin.io;
using fin.model.io;
using fin.util.enumerables;

namespace grezzo.api;

public class ZsiModelFileBundle(IReadOnlyTreeFile zsiFile)
    : IModelFileBundle {
  public IReadOnlyTreeFile MainFile => zsiFile;
  public IEnumerable<IReadOnlyGenericFile> Files => zsiFile.Yield();
  public IReadOnlyTreeFile ZsiFile => zsiFile;
}