using fin.io.web;
using fin.scene;
using fin.services;

using uni.api;
using uni.ui.winforms.common.fileTreeView;

namespace uni;

public static class SceneService {
  static SceneService() {
    FileBundleService.OnFileBundleOpened
        += (fileTreeLeafNode, fileBundle) => {
          if (fileBundle.FileBundle is ISceneFileBundle sceneFileBundle) {
            try {
              var scene = new GlobalSceneImporter().Import(sceneFileBundle);
              OpenScene(fileTreeLeafNode, scene);
            } catch (Exception e) {
              ExceptionService.HandleException(
                  e,
                  new LoadFileBundleExceptionContext(fileBundle));
            }
          }
        };

    ModelService.OnModelOpened
        += (fileTreeLeafNode, model) => {
          var scene = new SceneImpl {
              FileBundle = model.FileBundle,
              Files = model.Files
          };
          var area = scene.AddArea();
          var obj = area.AddObject();
          obj.AddSceneModel(model);

          scene.CreateDefaultLighting(obj);

          OpenScene(fileTreeLeafNode, scene);
        };
  }

  public static event Action<IFileTreeLeafNode?, IScene> OnSceneOpened;

  public static void OpenScene(IFileTreeLeafNode? fileTreeLeafNode,
                               IScene scene)
    => OnSceneOpened?.Invoke(fileTreeLeafNode, scene);
}