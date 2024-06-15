using fin.scene;

using uni.api;

namespace uni {
  public static class SceneService {
    static SceneService() {
      FileBundleService.OnFileBundleOpened
          += fileBundle => {
               if (fileBundle is ISceneFileBundle sceneFileBundle) {
                 var scene = new GlobalSceneImporter().Import(sceneFileBundle);
                 OpenScene(scene);
               }
             };
    }

    public static event Action<IScene> OnSceneOpened;

    public static void OpenScene(IScene scene)
      => OnSceneOpened?.Invoke(scene);
  }
}