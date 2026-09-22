using fin.io;
using fin.model.io;

namespace sm64ds.api;

public sealed class Sm64dsModelFileBundle
    : IModelFileBundle<Sm64dsModelFileBundle, Sm64dsModelImporter> {
  public required IReadOnlyTreeFile BmdFile { get; init; }
  public IReadOnlyList<IReadOnlyTreeFile>? BcaFiles { get; init; }

  public required string? GameName { get; init; }
  public IReadOnlyTreeFile MainFile => this.BmdFile;
}