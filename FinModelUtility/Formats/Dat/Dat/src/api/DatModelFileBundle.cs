using fin.io;
using fin.model.io;
using fin.util.enumerables;

namespace dat.api;

public class DatModelFileBundle : IModelFileBundle {
  public required string GameName { get; init; }

  public IReadOnlyTreeFile MainFile => this.DatFile;
  public required IReadOnlyTreeFile DatFile { get; init; }

  public IEnumerable<IReadOnlyGenericFile> Files => this.MainFile.Yield();
}