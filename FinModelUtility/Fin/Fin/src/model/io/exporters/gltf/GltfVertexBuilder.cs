using System;
using System.Linq;
using System.Numerics;

using fin.color;
using fin.data.indexable;
using fin.math;
using fin.model.accessor;

using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;

namespace fin.model.io.exporters.gltf;

public class GltfVertexBuilder {
  public bool UvIndices { get; set; }

  private static readonly (int, float)[] defaultSkinning_ = [(0, 1)];

  private readonly IndexableDictionary<IReadOnlyBoneWeights, (int, float)[]>
      skinningByBoneWeights_ = new();

  public GltfVertexBuilder(IReadOnlyModel model,
                           IIndexableDictionary<IReadOnlyBone, int>
                               boneToIndex) {
    foreach (var boneWeights in model.Skin.BoneWeights) {
      this.skinningByBoneWeights_[boneWeights] =
          boneWeights.Weights.Select(boneWeight => (
                                         boneToIndex[boneWeight.Bone],
                                         boneWeight.Weight))
                     .ToArray();
    }
  }

  public IVertexBuilder CreateVertexBuilder(
      IReadOnlyBoneTransformManager boneTransformManager,
      IVertexAccessor vertexAccessor,
      float scale,
      bool hasNormals,
      bool hasTangents,
      int colorCount,
      int uvCount,
      int weightCount) {
    var geometryType
        = GltfBuilderUtil.GetGeometryType(hasNormals, hasTangents);
    var materialType = !this.UvIndices
        ? GltfBuilderUtil.GetMaterialType(colorCount, uvCount)
        : typeof(VertexTexture1);
    var skinningType = GltfBuilderUtil.GetSkinningType(weightCount);

    var vertexBuilderType
        = typeof(VertexBuilder<,,>).MakeGenericType(
            [geometryType, materialType, skinningType]);

    var vertexBuilder
        = (IVertexBuilder) Activator.CreateInstance(vertexBuilderType);

    // Geo
    {
      boneTransformManager.ProjectVertexPositionNormalTangent(
          vertexAccessor,
          out var outPosition,
          out var outNormal,
          out var outTangent);

      var position =
          new Vector3(outPosition.X * scale,
                      outPosition.Y * scale,
                      outPosition.Z * scale);

      if (!hasNormals) {
        vertexBuilder.SetGeometry(new VertexPosition(position));
      } else {
        var normal = Vector3.Normalize(outNormal);

        if (!hasTangents) {
          vertexBuilder.SetGeometry(
              new VertexPositionNormal(position, normal));
        } else {
          var tangent = outTangent / outTangent.W;
          vertexBuilder.SetGeometry(
              new VertexPositionNormalTangent(
                  position,
                  normal,
                  tangent));
        }
      }
    }

    // Material
    if (!this.UvIndices) {
      vertexBuilder.SetMaterial(
          GetVertexMaterial_(vertexAccessor, colorCount, uvCount));
    } else {
      var index = vertexAccessor.Index;

      vertexBuilder.SetMaterial(
          new VertexTexture1(new Vector2(uvCount > 0 ? index : -1,
                                         colorCount > 0 ? index : -1)));
    }

    // Skinning
    {
      var boneWeights = vertexAccessor.BoneWeights;
      var skinningArray = boneWeights == null
          ? defaultSkinning_
          : this.skinningByBoneWeights_[boneWeights];

      IVertexSkinning skinning = weightCount switch {
          > 4 => new VertexJoints8(skinningArray),
          > 0 => new VertexJoints4(skinningArray),
          _   => new VertexEmpty()
      };

      vertexBuilder.SetSkinning(skinning);
    }

    return vertexBuilder;
  }

  private static IVertexMaterial GetVertexMaterial_(
      IVertexAccessor vertexAccessor,
      int colorCount,
      int uvCount)
    => colorCount switch {
        >= 2 => uvCount switch {
            >= 4 => new VertexColor2Texture4(
                vertexAccessor.GetColorOrDefault(0),
                vertexAccessor.GetColorOrDefault(1),
                vertexAccessor.GetUvOrDefault(0),
                vertexAccessor.GetUvOrDefault(1),
                vertexAccessor.GetUvOrDefault(2),
                vertexAccessor.GetUvOrDefault(3)),
            3 => new VertexColor2Texture3(
                vertexAccessor.GetColorOrDefault(0),
                vertexAccessor.GetColorOrDefault(1),
                vertexAccessor.GetUvOrDefault(0),
                vertexAccessor.GetUvOrDefault(1),
                vertexAccessor.GetUvOrDefault(2)),
            2 => new VertexColor2Texture2(
                vertexAccessor.GetColorOrDefault(0),
                vertexAccessor.GetColorOrDefault(1),
                vertexAccessor.GetUvOrDefault(0),
                vertexAccessor.GetUvOrDefault(1)),
            1 => new VertexColor2Texture1(
                vertexAccessor.GetColorOrDefault(0),
                vertexAccessor.GetColorOrDefault(1),
                vertexAccessor.GetUvOrDefault(0)),
            _ => new VertexColor2(
                vertexAccessor.GetColorOrDefault(0),
                vertexAccessor.GetColorOrDefault(1))
        },
        1 => uvCount switch {
            >= 4 => new VertexColor1Texture4(
                vertexAccessor.GetColorOrDefault(0),
                vertexAccessor.GetUvOrDefault(0),
                vertexAccessor.GetUvOrDefault(1),
                vertexAccessor.GetUvOrDefault(2),
                vertexAccessor.GetUvOrDefault(3)),
            3 => new VertexColor1Texture3(
                vertexAccessor.GetColorOrDefault(0),
                vertexAccessor.GetUvOrDefault(0),
                vertexAccessor.GetUvOrDefault(1),
                vertexAccessor.GetUvOrDefault(2)),
            2 => new VertexColor1Texture2(
                vertexAccessor.GetColorOrDefault(0),
                vertexAccessor.GetUvOrDefault(0),
                vertexAccessor.GetUvOrDefault(1)),
            1 => new VertexColor1Texture1(
                vertexAccessor.GetColorOrDefault(0),
                vertexAccessor.GetUvOrDefault(0)),
            _ => new VertexColor1(
                vertexAccessor.GetColorOrDefault(0))
        },
        _ => uvCount switch {
            >= 4 => new VertexTexture4(
                vertexAccessor.GetUvOrDefault(0),
                vertexAccessor.GetUvOrDefault(1),
                vertexAccessor.GetUvOrDefault(2),
                vertexAccessor.GetUvOrDefault(3)),
            3 => new VertexTexture3(
                vertexAccessor.GetUvOrDefault(0),
                vertexAccessor.GetUvOrDefault(1),
                vertexAccessor.GetUvOrDefault(2)),
            2 => new VertexTexture2(
                vertexAccessor.GetUvOrDefault(0),
                vertexAccessor.GetUvOrDefault(1)),
            1 => new VertexTexture1(
                vertexAccessor.GetUvOrDefault(0)),
            _ => new VertexEmpty()
        },
    };
}

public static class VertexAccessorExtensions {
  public static Vector4 GetColorOrDefault(this IVertexAccessor vertexAccessor,
                                          int index) {
    var color = vertexAccessor.GetColor(index);
    return new(color?.Rf ?? 1, color?.Gf ?? 1, color?.Bf ?? 1, color?.Af ?? 1);
  }

  public static Vector2 GetUvOrDefault(this IVertexAccessor vertexAccessor,
                                       int index)
    => vertexAccessor.GetUv(index) ?? Vector2.Zero;
}