using System.Collections.Generic;

using fin.io;
using fin.model.io;
using fin.util.enumerables;

namespace grezzo.api;

public class CmbModelFileBundle(
    string gameName,
    IReadOnlyTreeFile cmbFile,
    IReadOnlyList<IReadOnlyTreeFile>? csabFiles,
    IReadOnlyList<IReadOnlyTreeFile>? ctxbFiles,
    IReadOnlyList<IReadOnlyTreeFile>? shpaFiles)
    : IModelFileBundle {
  public CmbModelFileBundle(string gameName,
                            IReadOnlyTreeFile cmbFile) :
      this(gameName, cmbFile, null, null, null) { }

  public CmbModelFileBundle(string gameName,
                            IReadOnlyTreeFile cmbFile,
                            IReadOnlyList<IReadOnlyTreeFile>? csabFiles) :
      this(gameName, cmbFile, csabFiles, null, null) { }

  public string GameName { get; } = gameName;

  public IReadOnlyTreeFile MainFile => this.CmbFile;

  public IEnumerable<IReadOnlyGenericFile> Files
    => this.CmbFile.Yield()
           .ConcatIfNonnull(this.CsabFiles)
           .ConcatIfNonnull(this.CtxbFiles)
           .ConcatIfNonnull(this.ShpaFiles);

  public IReadOnlyTreeFile CmbFile { get; } = cmbFile;
  public IReadOnlyList<IReadOnlyTreeFile>? CsabFiles { get; } = csabFiles;
  public IReadOnlyList<IReadOnlyTreeFile>? CtxbFiles { get; } = ctxbFiles;
  public IReadOnlyList<IReadOnlyTreeFile>? ShpaFiles { get; } = shpaFiles;
}