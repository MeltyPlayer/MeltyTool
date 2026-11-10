using System.Collections.Generic;

using fin.io;
using fin.model.io;
using fin.util.enumerables;

namespace grezzo.api;

public sealed class ZsiModelFileBundle(IReadOnlyTreeFile zsiFile)
    : IModelFileBundle<ZsiModelFileBundle, ZsiModelImporter> {
  public IReadOnlyTreeFile MainFile => zsiFile;
  public IEnumerable<IReadOnlyStandaloneFile> Files => zsiFile.Yield();
  public IReadOnlyTreeFile ZsiFile => zsiFile;
}