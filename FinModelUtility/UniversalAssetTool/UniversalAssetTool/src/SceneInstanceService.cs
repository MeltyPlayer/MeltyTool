using fin.scene;
using fin.scene.instance;

using uni.ui.winforms.common.fileTreeView;

namespace uni;

public static class SceneInstanceService {
  static SceneInstanceService() {
    SceneService.OnSceneOpened
        += (fileTreeLeafNode, scene) =>
            OpenSceneInstance(fileTreeLeafNode,
                              new SceneInstanceImpl(scene));
  }

  public static event Action<IFileTreeLeafNode?, ISceneInstance>
      OnSceneInstanceOpened;

  public static void OpenSceneInstance(IFileTreeLeafNode? fileTreeLeafNode,
                                       ISceneInstance sceneInstance)
    => OnSceneInstanceOpened?.Invoke(fileTreeLeafNode, sceneInstance);
}