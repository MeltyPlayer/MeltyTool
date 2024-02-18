using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

using fin.color;
using fin.data.indexable;
using fin.math;
using fin.util.enumerables;

using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Materials;
using SharpGLTF.Schema2;
using SharpGLTF.Transforms;

namespace fin.model.io.exporters.gltf {
  using VERTEX =
      VertexBuilder<VertexPositionNormal, VertexColor2Texture2, VertexJoints4>;

  public class GltfMeshBuilder {
    public bool UvIndices { get; set; }

    public IList<Mesh> BuildAndBindMesh(
        ModelRoot gltfModel,
        IModel model,
        float scale,
        IDictionary<IMaterial, MaterialBuilder> finToTexCoordAndGltfMaterial) {
      var skin = model.Skin;
      var vertexAccessor = ConsistentVertexAccessor.GetAccessorForModel(model);

      var boneTransformManager = new BoneTransformManager();
      boneTransformManager.CalculateStaticMatricesForManualProjection(model);

      var boneToIndex = model.Skeleton.Skip(1).ToIndexByValueIndexableDictionary();

      var nullMaterialBuilder =
          new MaterialBuilder("null").WithDoubleSide(false)
                                     .WithSpecularGlossiness();

      var DEFAULT_SKINNING = SparseWeight8.Create((0, 1));
      var skinningByBoneWeights =
          new IndexableDictionary<IBoneWeights, (int, float)[]>();

      var gltfMeshes = new List<Mesh>();
      foreach (var finMesh in skin.Meshes) {
        var gltfMeshBuilder = VERTEX.CreateCompatibleMesh(finMesh.Name);

        foreach (var primitive in finMesh.Primitives) {
          MaterialBuilder materialBuilder;
          if (primitive.Material != null) {
            materialBuilder =
                finToTexCoordAndGltfMaterial[primitive.Material];
          } else {
            materialBuilder = nullMaterialBuilder;
          }

          var points = primitive.Vertices;
          var pointsCount = points.Count;
          var vertices = new VERTEX[pointsCount];

          for (var p = 0; p < pointsCount; ++p) {
            vertexAccessor.Target(points[p]);
            var point = vertexAccessor;

            boneTransformManager.ProjectVertexPositionNormal(
                point,
                out var outPosition,
                out var outNormal);

            var position =
                new Vector3(outPosition.X * scale,
                            outPosition.Y * scale,
                            outPosition.Z * scale);
            // TODO: Don't regenerate the skinning for each vertex, cache this somehow!
            var vertexBuilder = VERTEX.Create(position);

            var boneWeights = point.BoneWeights;
            if (boneWeights != null) {
              if (!skinningByBoneWeights.TryGetValue(boneWeights,
                    out var skinning)) {
                skinningByBoneWeights[boneWeights] = skinning = boneWeights
                    .Weights.Select(
                        boneWeight
                            => (boneToIndex[boneWeight.Bone],
                                boneWeight.Weight))
                    .ToArray();
              }

              vertexBuilder = vertexBuilder.WithSkinning(skinning);
            } else {
              vertexBuilder = vertexBuilder.WithSkinning(DEFAULT_SKINNING);
            }

            if (point.LocalNormal != null) {
              var tangent = point.LocalTangent;

              if (tangent == null) {
                vertexBuilder = vertexBuilder.WithGeometry(
                    position,
                    new Vector3(outNormal.X, outNormal.Y, outNormal.Z));
              } else {
                vertexBuilder = vertexBuilder.WithGeometry(
                    position,
                    new Vector3(outNormal.X, outNormal.Y, outNormal.Z),
                    new Vector4(tangent.Value.X,
                                tangent.Value.Y,
                                tangent.Value.Z,
                                tangent.Value.W));
              }
            }

            var finColor0 = point.GetColor(0);
            var hasColor0 = finColor0 != null;
            var assColor0 = hasColor0
                ? GltfMeshBuilder.FinToGltfColor_(finColor0)
                : new Vector4(1, 1, 1, 1);
            var finColor1 = point.GetColor(1);
            var hasColor1 = finColor1 != null;
            var assColor1 = hasColor1
                ? GltfMeshBuilder.FinToGltfColor_(finColor1)
                : new Vector4(1, 1, 1, 1);

            var hasColor = hasColor0 || hasColor1;

            var hasUvs = vertexAccessor.UvCount > 0;
            if (!this.UvIndices) {
              if (hasUvs) {
                var uv = vertexAccessor.GetUv();
                vertexBuilder =
                    vertexBuilder.WithMaterial(assColor0,
                                               assColor1,
                                               new Vector2(
                                                   uv.Value.U,
                                                   uv.Value.V));
              } else if (hasColor) {
                vertexBuilder =
                    vertexBuilder.WithMaterial(assColor0, assColor1);
              }
            } else {
              // Importing the color directly via Assimp doesn't work for some
              // reason.
              vertexBuilder =
                  vertexBuilder.WithMaterial(new Vector4(1, 1, 1, 1),
                                             new Vector2(
                                                 hasUvs ? point.Index : -1,
                                                 hasColor ? point.Index : -1));
            }

            vertices[p] = vertexBuilder;
          }

          switch (primitive.Type) {
            case PrimitiveType.TRIANGLES:
            case PrimitiveType.TRIANGLE_STRIP:
            case PrimitiveType.TRIANGLE_FAN: {
              var triangles =
                  gltfMeshBuilder.UsePrimitive(materialBuilder, 3);

              foreach (var (v1, v2, v3) in primitive
                                           .GetOrderedTriangleVertexIndices()
                                           .Select(i => vertices[i])
                                           .SeparateTriplets()) {
                triangles.AddTriangle(v1, v2, v3);
              }

              break;
            }
            case PrimitiveType.QUADS: {
              var quads = gltfMeshBuilder.UsePrimitive(materialBuilder);
              for (var v = 0; v < pointsCount; v += 4) {
                quads.AddQuadrangle(vertices[v + 0],
                                    vertices[v + 1],
                                    vertices[v + 2],
                                    vertices[v + 3]);
              }

              break;
            }
            case PrimitiveType.POINTS: {
              var pointPrimitive =
                  gltfMeshBuilder.UsePrimitive(
                      materialBuilder,
                      1);
              for (var v = 0; v < pointsCount; v += 4) {
                pointPrimitive.AddPoint(vertices[v]);
              }

              break;
            }
            default: throw new NotSupportedException();
          }
        }

        gltfMeshes.Add(gltfModel.CreateMesh(gltfMeshBuilder));
      }

      return gltfMeshes;
    }

    private static Vector4 FinToGltfColor_(IColor? color)
      => new(color?.Rf ?? 1, color?.Gf ?? 0, color?.Bf ?? 1, color?.Af ?? 1);
  }
}