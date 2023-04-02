using fin.io;
using fin.model;


namespace xmod.api {
  public class PedModelFileBundle : IModelFileBundle {
    public string GameName => "midnight_club_2";
    public IFileHierarchyFile MainFile => this.PedFile;
    public required IFileHierarchyFile PedFile { get; init; }
  }

  public class PedModelLoader : IModelLoader<PedModelFileBundle> {
    public IModel LoadModel(PedModelFileBundle modelFileBundle) {
      throw new NotImplementedException();
    }
  }
}