using System;

using SharpGLTF.Geometry.VertexTypes;

namespace fin.model.io.exporters.gltf {
  public static class GltfBuilderUtil {
    public static Type GetGeometryType(bool hasNormals, bool hasTangents)
      => hasNormals switch {
          true when hasTangents => typeof(VertexPositionNormalTangent),
          true                  => typeof(VertexPositionNormal),
          _                     => typeof(VertexPosition)
      };

    public static Type GetMaterialType(int colorCount, int uvCount) {
      if (colorCount >= 2) {
        return uvCount switch {
            >= 2 => typeof(VertexColor2Texture2),
            1    => typeof(VertexColor2Texture1),
            _    => typeof(VertexColor2)
        };
      }

      if (uvCount >= 2) {
        return colorCount == 1
            ? typeof(VertexColor1Texture2)
            : typeof(VertexTexture2);
      }

      if (colorCount == 1 && uvCount == 1) {
        return typeof(VertexColor1Texture1);
      }

      if (colorCount == 1) {
        return typeof(VertexColor1);
      }

      if (uvCount == 1) {
        return typeof(VertexTexture1);
      }

      return typeof(VertexEmpty);
    }

    public static Type GetSkinningType(int weightCount)
      => weightCount switch {
          > 4 => typeof(VertexJoints8),
          > 0 => typeof(VertexJoints4),
          _   => typeof(VertexEmpty)
      };
  }
}