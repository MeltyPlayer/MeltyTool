using System.Numerics;

using fin.util.linq;

namespace fin.model.util;

public static class MassChangeExtensions {
  public static void DisableDepthOnAllMaterials(this IModel model) {
    foreach (var material in model.MaterialManager.All) {
      material.DepthMode = DepthMode.NONE;
      material.DepthCompareType = DepthCompareType.Always;
    }
  }

  public static void FlipAllCullingInsideOut(this IModel model) {
    foreach (var material in model.MaterialManager.All) {
      material.CullingMode = CullingMode.SHOW_BACK_ONLY;
    }
  }

  public static void RemoveAllNormals(this IModel model) {
    foreach (var vertex in
             model.Skin.Vertices.WhereIs<IVertex, INormalVertex>()) {
      vertex.SetLocalNormal((Vector3?) null);
    }
  }

  public static void SetAllTextureFiltering(this IModel model,
                                            TextureMinFilter minFilter,
                                            TextureMagFilter magFilter) {
    foreach (var texture in model.MaterialManager.Textures) {
      texture.MinFilter = minFilter;
      texture.MagFilter = magFilter;
    }
  }
}