using fin.io;
using fin.util.enumerables;

namespace modl.api;

public class ModlModelFileBundle : IBattalionWarsModelFileBundle {
  public IReadOnlyTreeFile MainFile => this.ModlFile;
  public IEnumerable<IReadOnlyGenericFile> Files
    => this.ModlFile.Yield().ConcatIfNonnull(this.AnimFiles);

  public required GameVersion GameVersion { get; init; }
  public required IReadOnlyTreeFile ModlFile { get; init; }

  public required IReadOnlyList<IReadOnlyTreeFile>? AnimFiles { get; init; }
}