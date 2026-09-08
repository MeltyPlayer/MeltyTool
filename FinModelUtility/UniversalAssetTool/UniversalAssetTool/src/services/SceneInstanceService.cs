using fin.scene;
using fin.scene.instance;
using fin.ui.rendering;
using fin.ui.rendering.gl.scene;
using fin.util.types;

using uni.ui.winforms.common.fileTreeView;

namespace uni.services;

[IocCandiate]
public static class SceneInstanceService {
  static SceneInstanceService() {
    ModelService.OnModelSuccessfullyOpened
        += (fileTreeLeafNode, model) => {
          SceneTypeService.IsASingleModel = true;
          var scene = new SceneImpl {
              FileBundle = model.FileBundle,
              Files = model.Files
          };
          var area = scene.AddArea();
          var obj = area.AddRootNode();

          scene.CreateDefaultLighting(() => obj, [model]);
          obj.AddComponent(new SimpleModelRenderComponent(model));

          OpenSceneInstance(fileTreeLeafNode, new SceneInstanceImpl(scene));
        };
    SceneService.OnSceneSuccessfullyOpened
        += (fileTreeLeafNode, scene) =>
            OpenSceneInstance(fileTreeLeafNode,
                              new SceneInstanceImpl(scene));

    SceneService.OnSceneFailedToOpen
        += (_, _) => OpenSceneInstance(null,
                                       new SceneInstanceImpl(
                                           SceneImpl.CreateForViewer()));
    ModelService.OnModelFailedToOpen
        += (_, _) => OpenSceneInstance(null,
                                       new SceneInstanceImpl(
                                           SceneImpl.CreateForViewer()));
  }

  public static event Action<IFileTreeLeafNode?, ISceneInstance>
      OnSceneInstanceOpened;

  public static void OpenSceneInstance(IFileTreeLeafNode? fileTreeLeafNode,
                                       ISceneInstance sceneInstance)
    => OnSceneInstanceOpened?.Invoke(fileTreeLeafNode, sceneInstance);
}