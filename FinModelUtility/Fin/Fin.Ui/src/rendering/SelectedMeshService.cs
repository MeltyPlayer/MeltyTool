using fin.model;

namespace fin.ui.rendering {
  public static class SelectedMeshService {
    public static event Action<IReadOnlyMesh?>? OnMeshSelected;

    public static void SelectMesh(IReadOnlyMesh? mesh)
      => OnMeshSelected?.Invoke(mesh);
  }
}