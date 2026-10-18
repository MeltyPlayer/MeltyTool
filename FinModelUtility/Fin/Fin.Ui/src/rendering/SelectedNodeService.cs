using fin.scene;

namespace fin.ui.rendering;

public static class SelectedNodeService {
  public static event Action<IReadOnlySceneNode?> OnNodeSelected;

  public static void SelectNode(IReadOnlySceneNode? node) {
    if (SelectedServiceUtils.SelectableType is SelectableType.NODE ||
        node == null) {
      OnNodeSelected?.Invoke(node);
    }
  }
}