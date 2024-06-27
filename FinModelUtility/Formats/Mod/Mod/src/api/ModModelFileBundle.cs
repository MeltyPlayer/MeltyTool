using System.Collections.Generic;

using fin.io;
using fin.model.io;
using fin.util.enumerables;

namespace mod.api {
  public class ModModelFileBundle : IModelFileBundle {
    public required string GameName { get; init; }

    public IReadOnlyTreeFile MainFile => this.ModFile;
    public IEnumerable<IReadOnlyGenericFile> Files
      => this.ModFile.Yield().ConcatIfNonnull(this.AnmFile);

    public required IReadOnlyTreeFile ModFile { get; init; }
    public required IReadOnlyTreeFile? AnmFile { get; init; }
  }
}