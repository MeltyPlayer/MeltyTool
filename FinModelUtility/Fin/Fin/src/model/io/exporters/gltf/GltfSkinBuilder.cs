using System;
using System.Collections.Generic;
using System.Linq;

using fin.data.indexable;
using fin.math;
using fin.model.accessor;
using fin.util.enumerables;

using SharpGLTF.Geometry;
using SharpGLTF.Materials;
using SharpGLTF.Schema2;

using IGltfMeshBuilder
    = SharpGLTF.Geometry.IMeshBuilder<SharpGLTF.Materials.MaterialBuilder>;

namespace fin.model.io.exporters.gltf;

public class GltfSkinBuilder {
  public bool UvIndices { get; set; }

  public IList<(Mesh gltfMesh, bool hasJoints)> AddSkin(
      ModelRoot gltfModel,
      IReadOnlyModel model,
      float scale,
      IDictionary<IReadOnlyMaterial, MaterialBuilder> finToTexCoordAndGltfMaterial) {
    var skin = model.Skin;

    var boneTransformManager = new BoneTransformManager();
    boneTransformManager.CalculateStaticMatricesForManualProjection(model);

    var boneToIndex
        = model.Skeleton.Skip(1)
               .ToIndexByValueIndexableDictionary();

    var nullMaterialBuilder =
        new MaterialBuilder("null").WithDoubleSide(false)
                                   .WithSpecularGlossiness();

    var vertexAccessor = MaximalVertexAccessor.GetAccessorForModel(model);
    var vertexMap
        = new IndexableDictionary<IReadOnlyVertex, IVertexBuilder>(
            skin.Vertices.Count);

    var gltfVertexBuilder = new GltfVertexBuilder(model, boneToIndex);

    var gltfMeshes = new List<(Mesh, bool)>();
    foreach (var finMesh in skin.Meshes) {
      bool hasNormals = false;
      bool hasTangents = false;
      int colorCount = 0;
      int uvCount = 0;
      int weightCount = 0;

      var verticesInMesh = finMesh.Primitives.SelectMany(p => p.Vertices)
                                  .Distinct()
                                  .ToArray();
      foreach (var finVertex in verticesInMesh) {
        vertexAccessor.Target(finVertex);

        hasNormals |= vertexAccessor.LocalNormal != null;
        hasTangents |= vertexAccessor.LocalTangent != null;
        colorCount = Math.Max(colorCount, vertexAccessor.ColorCount);
        uvCount = Math.Max(uvCount, vertexAccessor.UvCount);
        weightCount = Math.Max(weightCount,
                               vertexAccessor.BoneWeights?.Weights.Count ??
                               0);
      }

      foreach (var finVertex in verticesInMesh) {
        vertexAccessor.Target(finVertex);

        var vertexBuilder = gltfVertexBuilder.CreateVertexBuilder(
            boneTransformManager,
            vertexAccessor,
            scale,
            hasNormals,
            hasTangents,
            colorCount,
            uvCount,
            weightCount);

        vertexMap[finVertex] = vertexBuilder;
      }

      IGltfMeshBuilder gltfMeshBuilder
          = GltfMeshBuilderUtil.CreateMeshBuilder(hasNormals,
                                                  hasTangents,
                                                  colorCount,
                                                  uvCount,
                                                  weightCount);
      gltfMeshBuilder.Name = finMesh.Name;

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

        var vertices = primitive.Vertices.Select(vertex => vertexMap[vertex])
                                .ToArray();

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

      gltfMeshes.Add((gltfModel.CreateMesh(gltfMeshBuilder),
                      weightCount > 0));
    }

    return gltfMeshes;
  }
}