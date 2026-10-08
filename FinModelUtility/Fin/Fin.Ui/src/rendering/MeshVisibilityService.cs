using fin.model;

namespace fin.ui.rendering;

// Service that lets the user override mesh visibility and provides an event handler for when this happens
public static class MeshVisibilityService {
  public static event Action<IReadOnlyMesh, bool>? OnMeshVisibilityChanged;

  public static void SetVisibility(IReadOnlyMesh mesh, bool isVisible)
    => OnMeshVisibilityChanged?.Invoke(mesh, isVisible);
}