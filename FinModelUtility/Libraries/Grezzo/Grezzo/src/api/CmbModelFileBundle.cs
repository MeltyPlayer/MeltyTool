using System.Collections.Generic;

using fin.io;
using fin.model.io;
using fin.util.enumerables;

namespace grezzo.api;

public sealed class CmbModelFileBundle(
    IReadOnlyTreeFile cmbFile,
    IReadOnlyList<IReadOnlyTreeFile>? csabFiles,
    IReadOnlyList<IReadOnlyTreeFile>? ctxbFiles,
    IReadOnlyList<IReadOnlyTreeFile>? shpaFiles)
    : IModelFileBundle<CmbModelFileBundle, CmbModelImporter> {
  public CmbModelFileBundle(IReadOnlyTreeFile cmbFile) :
      this(cmbFile, null, null, null) { }

  public CmbModelFileBundle(IReadOnlyTreeFile cmbFile,
                            IReadOnlyList<IReadOnlyTreeFile>? csabFiles) :
      this(cmbFile, csabFiles, null, null) { }

  public IReadOnlyTreeFile MainFile => this.CmbFile;

  public IEnumerable<IReadOnlyStandaloneFile> Files
    => this.CmbFile.Yield()
           .ConcatIfNonnull(this.CsabFiles)
           .ConcatIfNonnull(this.CtxbFiles)
           .ConcatIfNonnull(this.ShpaFiles);

  public IReadOnlyTreeFile CmbFile { get; } = cmbFile;
  public IReadOnlyList<IReadOnlyTreeFile>? CsabFiles { get; } = csabFiles;
  public IReadOnlyList<IReadOnlyTreeFile>? CtxbFiles { get; } = ctxbFiles;
  public IReadOnlyList<IReadOnlyTreeFile>? ShpaFiles { get; } = shpaFiles;
}