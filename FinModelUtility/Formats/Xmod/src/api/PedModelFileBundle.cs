using fin.io;
using fin.model.io;


namespace xmod.api;

public class PedModelFileBundle : IModelFileBundle {
  public IReadOnlyTreeFile MainFile => this.PedFile;
  public required IReadOnlyTreeFile PedFile { get; init; }
}