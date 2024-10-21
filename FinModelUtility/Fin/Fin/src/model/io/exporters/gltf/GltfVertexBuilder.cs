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
  private static readonly (int, float)[] defaultSkinning_ = [(0, 1)];

  private readonly IndexableDictionary<IReadOnlyBoneWeights, (int, float)[]>
      skinningByBoneWeights_ = new();

  public GltfVertexBuilder(IReadOnlyModel model,
                           IIndexableDictionary<IReadOnlyBone, int> boneToIndex) {
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
    var materialType = GltfBuilderUtil.GetMaterialType(colorCount, uvCount);
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

      if (hasNormals) {
        var normal
            = Vector3.Normalize(
                new Vector3(outNormal.X, outNormal.Y, outNormal.Z));

        if (hasTangents) {
          var tangent = new Vector4(outTangent.X,
                                    outTangent.Y,
                                    outTangent.Z,
                                    outTangent.W) /
                        Math.Abs(outTangent.W);

          vertexBuilder.SetGeometry(
              new VertexPositionNormalTangent(
                  position,
                  normal,
                  tangent));
        } else {
          vertexBuilder.SetGeometry(
              new VertexPositionNormal(position, normal));
        }
      } else {
        vertexBuilder.SetGeometry(new VertexPosition(position));
      }
    }

    // Material
    vertexBuilder.SetMaterial(
        GetVertexMaterial_(vertexAccessor, colorCount, uvCount));

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
      int uvCount) {
    var color0 = FinToGltfColor_(vertexAccessor.GetColor(0));
    var color1 = FinToGltfColor_(vertexAccessor.GetColor(1));

    var uv0 = FinToGltfUv_(vertexAccessor.GetUv(0));
    var uv1 = FinToGltfUv_(vertexAccessor.GetUv(1));

    if (colorCount >= 2) {
      return uvCount switch {
          >= 2 => new VertexColor2Texture2(color0, color1, uv0, uv1),
          1    => new VertexColor2Texture1(color0, color1, uv0),
          _    => new VertexColor2(color0, color1)
      };
    }

    if (uvCount >= 2) {
      return colorCount == 1
          ? new VertexColor1Texture2(color0, uv0, uv1)
          : new VertexTexture2(uv0, uv1);
    }

    if (colorCount == 1 && uvCount == 1) {
      return new VertexColor1Texture1(color0, uv0);
    }

    if (colorCount == 1) {
      return new VertexColor1(color0);
    }

    if (uvCount == 1) {
      return new VertexTexture1(uv0);
    }

    return new VertexEmpty();
  }

  private static Vector4 FinToGltfColor_(IColor? color)
    => new(color?.Rf ?? 1, color?.Gf ?? 1, color?.Bf ?? 1, color?.Af ?? 1);

  private static Vector2 FinToGltfUv_(Vector2? uv)
    => new(uv?.X ?? 0, uv?.Y ?? 0);
}