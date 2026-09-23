using fin.io;
using fin.model.io;
using fin.util.enumerables;

namespace modl.api;

public sealed class ModlModelFileBundle
    : IModelFileBundle<ModlModelFileBundle, ModlModelImporter> {
  public IReadOnlyTreeFile MainFile => this.ModlFile;

  public IEnumerable<IReadOnlyStandaloneFile> Files
    => this.ModlFile.Yield().ConcatIfNonnull(this.AnimFiles);

  public required GameVersion GameVersion { get; init; }
  public required IReadOnlyTreeFile ModlFile { get; init; }

  public required IReadOnlyList<IReadOnlyTreeFile>? AnimFiles { get; init; }
}