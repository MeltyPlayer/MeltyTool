using fin.io;
using fin.model.io;
using fin.util.enumerables;

namespace sysdolphin.api;

public sealed class DatModelFileBundle
    : IModelFileBundle<DatModelFileBundle, DatModelImporter> {
  public IReadOnlyTreeFile MainFile => this.DatFile;
  public required IReadOnlyTreeFile DatFile { get; init; }

  public IEnumerable<IReadOnlyStandaloneFile> Files => this.MainFile.Yield();
}