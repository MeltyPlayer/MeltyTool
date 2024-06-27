using fin.io;
using fin.model.io;

namespace xmod.api;

public class PedModelFileBundle : IModelFileBundle {
  public string GameName => "midnight_club_2";
  public IReadOnlyTreeFile MainFile => this.PedFile;
  public required IReadOnlyTreeFile PedFile { get; init; }
}