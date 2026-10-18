using fin.model;

namespace fin.ui.rendering;

public static class SelectedTextureService {
  public static event Action<(IReadOnlyModel, IReadOnlyTexture)?>?
      OnTextureSelected;

  public static void SelectTexture(
      (IReadOnlyModel, IReadOnlyTexture)? modelAndTexture) {
    if (SelectedServiceUtils.SelectableType is SelectableType.TEXTURE ||
        modelAndTexture == null) {
      OnTextureSelected?.Invoke(modelAndTexture);
    }
  }
}