namespace fin.ui.rendering;

public enum SelectableType {
  NONE,
  BONE,
  MATERIAL,
  MESH,
  NODE,
  TEXTURE,
}

public static class SelectedServiceUtils {
  public static SelectableType SelectableType {
    get;
    set {
      field = value;

      if (value != SelectableType.BONE) {
        SelectedBoneService.SelectBone(null);
      }
      if (value != SelectableType.MATERIAL) {
        SelectedMaterialsService.SelectMaterial(null);
      }
      if (value != SelectableType.MESH) {
        SelectedMeshService.SelectMesh(null);
      }
      if (value != SelectableType.NODE) {
        SelectedNodeService.SelectNode(null);
      }
      if (value != SelectableType.TEXTURE) {
        SelectedTextureService.SelectTexture(null);
      }
    }
  } = SelectableType.NONE;
}