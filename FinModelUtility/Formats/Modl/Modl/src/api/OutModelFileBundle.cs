using fin.io;

namespace modl.api;

public class OutModelFileBundle : IBattalionWarsModelFileBundle {
  public required string GameName { get; init; }

  public IReadOnlyTreeFile MainFile => this.OutFile;

  public required GameVersion GameVersion { get; init; }
  public required IReadOnlyTreeFile OutFile { get; init; }

  public IEnumerable<IReadOnlyTreeDirectory>? TextureDirectories {
    get;
    init;
  } = null;
}