using fin.model;
using fin.util.sets;

namespace fin.ui.rendering;

public static class SelectedMaterialsService {
  static SelectedMaterialsService() {
    SelectedTextureService.OnTextureSelected += modelAndTexture => {
      if (modelAndTexture != null) {
        var (model, texture) = modelAndTexture.Value;
        SelectMaterials(model.MaterialManager
                             .All
                             .Where(m => m.Textures.Contains(texture))
                             .ToHashSet());
      } else {
        SelectMaterials(null);
      }
    };
  }

  public static event Action<IReadOnlySet<IReadOnlyMaterial>?>
      OnMaterialsSelected;

  public static void SelectMaterial(IReadOnlyMaterial? material) {
    if (SelectedServiceUtils.SelectableType is SelectableType.MATERIAL
                                               or SelectableType.TEXTURE ||
        material == null) {
      OnMaterialsSelected?.Invoke(material.AsSet());
    }
  }

  public static void SelectMaterials(
      IReadOnlySet<IReadOnlyMaterial>? materials) {
    if (SelectedServiceUtils.SelectableType ==
        SelectableType.MATERIAL ||
        materials == null ||
        !(materials?.Any() ?? false)) {
      OnMaterialsSelected?.Invoke(materials);
    }
  }
}