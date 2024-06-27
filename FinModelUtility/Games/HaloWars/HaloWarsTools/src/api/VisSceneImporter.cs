using fin.scene;

using HaloWarsTools;

namespace hw.api;

public class VisSceneImporter : ISceneImporter<VisSceneFileBundle> {
  public IScene Import(VisSceneFileBundle sceneFileBundle) {
      var visResource =
          HWVisResource.FromFile(sceneFileBundle.Context,
                                 sceneFileBundle.VisFile.FullPath);
      return visResource.Scene;
    }
}