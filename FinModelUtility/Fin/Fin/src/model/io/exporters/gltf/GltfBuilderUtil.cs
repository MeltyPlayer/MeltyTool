using System;

using SharpGLTF.Geometry.VertexTypes;

namespace fin.model.io.exporters.gltf;

public static class GltfBuilderUtil {
  public static Type GetGeometryType(bool hasNormals, bool hasTangents)
    => hasNormals switch {
        true when hasTangents => typeof(VertexPositionNormalTangent),
        true                  => typeof(VertexPositionNormal),
        _                     => typeof(VertexPosition)
    };

  public static Type GetMaterialType(int colorCount, int uvCount)
    => colorCount switch {
        >= 2 => uvCount switch {
            >= 4 => typeof(VertexColor2Texture4),
            3    => typeof(VertexColor2Texture3),
            2    => typeof(VertexColor2Texture2),
            1    => typeof(VertexColor2Texture1),
            _    => typeof(VertexColor2)
        },
        1 => uvCount switch {
            >= 4 => typeof(VertexColor1Texture4),
            3    => typeof(VertexColor1Texture3),
            2    => typeof(VertexColor1Texture2),
            1    => typeof(VertexColor1Texture1),
            _    => typeof(VertexColor1)
        },
        _ => uvCount switch {
            >= 4 => typeof(VertexTexture4),
            3    => typeof(VertexTexture3),
            2    => typeof(VertexTexture2),
            1    => typeof(VertexTexture1),
            _    => typeof(VertexEmpty)
        },
    };

  public static Type GetSkinningType(int weightCount)
    => weightCount switch {
        > 4 => typeof(VertexJoints8),
        > 0 => typeof(VertexJoints4),
        _   => typeof(VertexEmpty)
    };
}