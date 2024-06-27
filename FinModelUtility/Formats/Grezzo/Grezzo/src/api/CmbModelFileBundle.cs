using System.Collections.Generic;

using fin.io;
using fin.model.io;
using fin.util.enumerables;

namespace grezzo.api;

public class CmbModelFileBundle : IModelFileBundle {
  public CmbModelFileBundle(string gameName,
                            IReadOnlyTreeFile cmbFile) :
      this(gameName, cmbFile, null, null, null) { }

  public CmbModelFileBundle(string gameName,
                            IReadOnlyTreeFile cmbFile,
                            IReadOnlyList<IReadOnlyTreeFile>? csabFiles) :
      this(gameName, cmbFile, csabFiles, null, null) { }

  public CmbModelFileBundle(string gameName,
                            IReadOnlyTreeFile cmbFile,
                            IReadOnlyList<IReadOnlyTreeFile>? csabFiles,
                            IReadOnlyList<IReadOnlyTreeFile>? ctxbFiles,
                            IReadOnlyList<IReadOnlyTreeFile>? shpaFiles) {
      this.GameName = gameName;
      this.CmbFile = cmbFile;
      this.CsabFiles = csabFiles;
      this.CtxbFiles = ctxbFiles;
      this.ShpaFiles = shpaFiles;
    }

  public string GameName { get; }

  public IReadOnlyTreeFile MainFile => this.CmbFile;

  public IEnumerable<IReadOnlyGenericFile> Files
    => this.CmbFile.Yield()
           .ConcatIfNonnull(this.CsabFiles)
           .ConcatIfNonnull(this.CtxbFiles)
           .ConcatIfNonnull(this.ShpaFiles);

  public IReadOnlyTreeFile CmbFile { get; }
  public IReadOnlyList<IReadOnlyTreeFile>? CsabFiles { get; }
  public IReadOnlyList<IReadOnlyTreeFile>? CtxbFiles { get; }
  public IReadOnlyList<IReadOnlyTreeFile>? ShpaFiles { get; }
}