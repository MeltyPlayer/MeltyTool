using fin.io;
using fin.model.io;


namespace xmod.api;

public class PedModelFileBundle : IModelFileBundle {
  public IReadOnlyTreeFile MainFile => this.PedFile;
  public required IReadOnlyTreeFile PedFile { get; init; }
  public required IReadOnlyTreeDirectory ModelDirectory { get; init; }
  public required IReadOnlyTreeDirectory TextureDirectory { get; init; }
}