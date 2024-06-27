using fin.io;
using fin.scene;

using HaloWarsTools;

namespace hw.api;

// TODO: Switch this to a scene model or nested model file bundle?
public class VisSceneFileBundle : IHaloWarsFileBundle, ISceneFileBundle {
  public VisSceneFileBundle(IReadOnlyTreeFile visFile, HWContext context) {
      this.VisFile = visFile;
      this.Context = context;
    }

  public string GameName => "halo_wars";
  public IReadOnlyTreeFile MainFile => this.VisFile;
  public IReadOnlyTreeFile VisFile { get; }

  public HWContext Context { get; }
}