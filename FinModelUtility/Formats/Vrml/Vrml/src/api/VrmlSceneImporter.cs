using fin.scene;
using fin.util.sets;

namespace vrml.api;

public class VrmlSceneImporter : ISceneImporter<VrmlSceneFileBundle> {
  public IScene Import(VrmlSceneFileBundle fileBundle) {
    var wrlFile = fileBundle.WrlFile.Impl;
    using var wrlFileStream = wrlFile.OpenRead();

    var finScene = new SceneImpl
        {FileBundle = fileBundle, Files = fileBundle.WrlFile.AsFileSet()};

    return finScene;
  }
}