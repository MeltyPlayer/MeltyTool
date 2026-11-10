using fin.io;
using fin.model.io;

namespace modl.api;

public sealed class OutModelFileBundle
    : IModelFileBundle<OutModelFileBundle, OutModelImporter> {
  public IReadOnlyTreeFile MainFile => this.OutFile;

  public required GameVersion GameVersion { get; init; }
  public required IReadOnlyTreeFile OutFile { get; init; }

  public IEnumerable<IReadOnlyTreeDirectory>? TextureDirectories { get; init; }
    = null;
}