using fin.model;

namespace fin.ui.rendering;

public static class SelectedMeshService {
  public static event Action<IReadOnlyMesh?>? OnMeshSelected;

  public static void SelectMesh(IReadOnlyMesh? mesh) {
    if (SelectedServiceUtils.SelectableType is SelectableType.MESH ||
        mesh == null) {
      OnMeshSelected?.Invoke(mesh);
    }
  }
}