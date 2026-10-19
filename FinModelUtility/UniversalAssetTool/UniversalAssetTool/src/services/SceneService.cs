using fin.io.bundles;
using fin.scene;
using fin.services;
using fin.ui.rendering;
using fin.util.types;

using uni.api;
using uni.ui.winforms.common.fileTreeView;

namespace uni.services;

[IocCandiate]
public static class SceneService {
  static SceneService() {
    FileBundleService.OnFileBundleOpened
        += (fileTreeLeafNode, fileBundle) => {
          if (fileBundle is ISceneFileBundle sceneFileBundle) {
            SceneTypeService.IsASingleModel = false;
            LoadingStatusService.IsLoading = true;

            Task.Run(() => {
              try {
                var scene = new GlobalSceneImporter().Import(sceneFileBundle);
                OpenScene(fileTreeLeafNode, scene);
              } catch (Exception e) {
                FailToOpenScene(fileBundle, e);
              }
            });

            LoadingStatusService.IsLoading = false;
          }
        };
  }

  public static event Action<IFileTreeLeafNode?, IScene>
      OnSceneSuccessfullyOpened;

  public static event Action<IFileBundle?, Exception> OnSceneFailedToOpen;

  public static void OpenScene(
      IFileTreeLeafNode? fileTreeLeafNode,
      IScene scene)
    => OnSceneSuccessfullyOpened?.Invoke(fileTreeLeafNode, scene);

  public static void FailToOpenScene(
      IFileBundle fileBundle,
      Exception exception)
    => OnSceneFailedToOpen?.Invoke(fileBundle, exception);
}