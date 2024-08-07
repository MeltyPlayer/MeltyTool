using fin.io;
using fin.model.io;
using fin.util.enumerables;

namespace dat.api;

public class MeleeModelFileBundle : IModelFileBundle {
  public required string GameName { get; init; }

  public IReadOnlyTreeFile MainFile => this.PrimaryDatFile;
  public required IReadOnlyTreeFile PrimaryDatFile { get; init; }
  public IReadOnlyTreeFile? AnimationDatFile { get; init; }
  public IReadOnlyTreeFile? FighterDatFile { get; init; }

  public IEnumerable<IReadOnlyGenericFile> Files
    => this.MainFile.Yield()
           .ConcatIfNonnull(this.AnimationDatFile)
           .ConcatIfNonnull(this.FighterDatFile);
}