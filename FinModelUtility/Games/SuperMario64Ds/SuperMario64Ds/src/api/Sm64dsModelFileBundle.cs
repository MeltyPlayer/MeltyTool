using fin.io;
using fin.model.io;

namespace sm64ds.api;

public class Sm64dsModelFileBundle : IModelFileBundle {
  public required IReadOnlyTreeFile BmdFile { get; init; }
  public IReadOnlyList<IReadOnlyTreeFile>? BcaFiles { get; init; }

  public required string? GameName { get; init; }
  public IReadOnlyTreeFile MainFile => this.BmdFile;
}