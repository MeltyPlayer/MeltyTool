namespace fin.model.util;

public static class MassChangeExtensions {
  public static void DisableDepthOnAllMaterials(this IModel model) {
    foreach (var material in model.MaterialManager.All) {
      material.DepthMode = DepthMode.NONE;
      material.DepthCompareType = DepthCompareType.Always;
    }
  }
}