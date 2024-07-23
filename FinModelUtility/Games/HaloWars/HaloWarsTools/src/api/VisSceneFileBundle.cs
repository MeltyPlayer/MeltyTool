using fin.io;
using fin.scene;

using HaloWarsTools;

namespace hw.api {
  // TODO: Switch this to a scene model or nested model file bundle?
  public class VisSceneFileBundle(IReadOnlyTreeFile visFile, HWContext context)
      : IHaloWarsFileBundle, ISceneFileBundle {
    public string GameName => "halo_wars";
    public IReadOnlyTreeFile MainFile => this.VisFile;
    public IReadOnlyTreeFile VisFile { get; } = visFile;

    public HWContext Context { get; } = context;
  }
}