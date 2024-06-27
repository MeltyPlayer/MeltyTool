using System.Linq;

using SharpGLTF.Geometry;

using IGltfMeshBuilder
    = SharpGLTF.Geometry.IMeshBuilder<SharpGLTF.Materials.MaterialBuilder>;

namespace fin.model.io.exporters.gltf;

public static class GltfMeshBuilderUtil {
  private static readonly object?[] meshBuilderParams_ = [null];

  public static IGltfMeshBuilder CreateMeshBuilder(
      bool hasNormals,
      bool hasTangents,
      int colorCount,
      int uvCount,
      int weightCount) {
    var geometryType
        = GltfBuilderUtil.GetGeometryType(hasNormals, hasTangents);
    var materialType = GltfBuilderUtil.GetMaterialType(colorCount, uvCount);
    var skinningType = GltfBuilderUtil.GetSkinningType(weightCount);

    var meshBuilderType
        = typeof(MeshBuilder<,,>).MakeGenericType(
            [geometryType, materialType, skinningType]);

    var constructor = meshBuilderType.GetConstructors().Single();
    return (IGltfMeshBuilder) constructor.Invoke(meshBuilderParams_);
  }
}