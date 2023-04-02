using System;
using System.Collections.Generic;
using System.Numerics;

using fin.color;
using fin.math;
using fin.model;
using fin.model.impl;

using SharpGLTF.Memory;
using SharpGLTF.Schema2;
using SharpGLTF.Transforms;

using PrimitiveType = fin.model.PrimitiveType;


namespace fin.exporter.gltf.lowlevel {
  public class LowLevelGltfMeshBuilder {
    public bool UvIndices { get; set; }

    public IList<Mesh> BuildAndBindMesh(
        ModelRoot gltfModel,
        IModel model,
        float scale,
        Dictionary<IMaterial, (IList<byte>, Material)>
            finToTexCoordAndGltfMaterial) {
      var skin = model.Skin;

      var boneTransformManager = new BoneTransformManager();
      var boneToIndex = boneTransformManager.CalculateMatrices(
          model.Skeleton.Root,
          model.Skin.BoneWeights,
          null);
      boneTransformManager.InitModelVertices(model);

      var nullMaterial = gltfModel.CreateMaterial("null");
      nullMaterial.DoubleSided = false;
      nullMaterial.WithPBRSpecularGlossiness();

      var DEFAULT_SKINNING = SparseWeight8.Create((0, 1));

      var points = model.Skin.Vertices;
      var pointsCount = points.Count;

      var positionView = gltfModel.CreateBufferView(
          4 * 3 * pointsCount, 0,
          BufferMode.ARRAY_BUFFER);
      var positionArray = new Vector3Array(positionView.Content);

      var normalView = gltfModel.CreateBufferView(
          4 * 3 * pointsCount, 0,
          BufferMode.ARRAY_BUFFER);
      var normalArray = new Vector3Array(normalView.Content);

      var colorView = gltfModel.CreateBufferView(
          4 * 4 * pointsCount, 0,
          BufferMode.ARRAY_BUFFER);
      var colorArray = new ColorArray(colorView.Content);

      var uvView = gltfModel.CreateBufferView(
          4 * 2 * pointsCount, 0,
          BufferMode.ARRAY_BUFFER);
      var uvArray = new Vector2Array(uvView.Content);

      for (var p = 0; p < pointsCount; ++p) {
        var point = points[p];

        boneTransformManager.ProjectVertexPositionNormal(point, out var outPosition, out var outNormal);
        var pos = positionArray[p];
        pos.X = outPosition.X * scale;
        pos.Y = outPosition.Y * scale;
        pos.Z = outPosition.Z * scale;
        positionArray[p] = pos;

        if (point.LocalNormal != null) {
          var norm = normalArray[p];

          norm.X = outNormal.X;
          norm.Y = outNormal.Y;
          norm.Z = outNormal.Z;
          normalArray[p] = norm;
        }

        var finColor0 = point.GetColor(0);
        var hasColor0 = finColor0 != null;
        var assColor0 = hasColor0
                            ? LowLevelGltfMeshBuilder.FinToGltfColor_(
                                finColor0)
                            : new Vector4(1, 1, 1, 1);
        var col = colorArray[p];
        col.X = assColor0.X;
        col.Y = assColor0.Y;
        col.Z = assColor0.Z;
        col.W = assColor0.W;
        colorArray[p] = col;

        var finUv = point.GetUv(0);
        var uv = uvArray[p];
        uv.X = finUv.U;
        uv.Y = finUv.V;
        uvArray[p] = uv;

        /*if (point.Weights != null) {
          vertexBuilder = vertexBuilder.WithSkinning(
              point.Weights.Select(
                       boneWeight
                           => (boneToIndex[boneWeight.Bone],
                               boneWeight.Weight))
                   .ToArray());
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
                new Vector4(tangent.X, tangent.Y, tangent.Z, tangent.W));
          }
        }

        var finColor0 = point.GetColor(0);
        var hasColor0 = finColor0 != null;
        var assColor0 = hasColor0
                            ? LowLevelGltfMeshBuilder.FinToGltfColor_(
                                finColor0)
                            : new Vector4(1, 1, 1, 1);
        var finColor1 = point.GetColor(1);
        var hasColor1 = finColor1 != null;
        var assColor1 = hasColor1
                            ? LowLevelGltfMeshBuilder.FinToGltfColor_(
                                finColor1)
                            : new Vector4(1, 1, 1, 1);

        var hasColor = hasColor0 || hasColor1;

        var uvs = point.Uvs;
        var hasUvs = (uvs?.Count ?? 0) > 0;
        if (!this.UvIndices) {
          if (hasUvs) {
            var uv = uvs[0];
            vertexBuilder =
                vertexBuilder.WithMaterial(assColor0,
                                           assColor1,
                                           new Vector2(uv.U, uv.V));
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

        vertices[p] = vertexBuilder;*/
      }

      var gltfMeshes = new List<Mesh>();
      foreach (var finMesh in skin.Meshes) {
        var gltfMesh = gltfModel.CreateMesh(finMesh.Name);

        foreach (var primitive in finMesh.Primitives) {
          Material material;
          if (primitive.Material != null) {
            (_, material) =
                finToTexCoordAndGltfMaterial[primitive.Material];
          } else {
            material = nullMaterial;
          }

          var gltfPrimitive = gltfMesh.CreatePrimitive();
          gltfPrimitive.Material = material;

          // TODO: Use shared position/normal accessors?
          var positionAccessor = gltfModel.CreateAccessor();
          positionAccessor.SetVertexData(
              positionView, 0, pointsCount);
          gltfPrimitive.SetVertexAccessor("POSITION", positionAccessor);

          var normalAccessor = gltfModel.CreateAccessor();
          normalAccessor.SetVertexData(
              normalView, 0, pointsCount);
          gltfPrimitive.SetVertexAccessor("NORMAL", normalAccessor);

          var colorAccessor = gltfModel.CreateAccessor();
          colorAccessor.SetVertexData(
              colorView, 0, pointsCount, DimensionType.VEC4);
          gltfPrimitive.SetVertexAccessor("COLOR_0", colorAccessor);

          var uvAccessor = gltfModel.CreateAccessor();
          uvAccessor.SetVertexData(
              uvView, 0, pointsCount, DimensionType.VEC2);
          gltfPrimitive.SetVertexAccessor("TEXCOORD_0", uvAccessor);

          var primitivePoints = primitive.Vertices;

          switch (primitive.Type) {
            case PrimitiveType.TRIANGLES: {
              gltfPrimitive.DrawPrimitiveType =
                  SharpGLTF.Schema2.PrimitiveType.TRIANGLES;

              var indexView = gltfModel.CreateBufferView(4 * primitivePoints.Count, 0,
                BufferMode.ELEMENT_ARRAY_BUFFER);
              var indexArray = new IntegerArray(indexView.Content);

              for (var v = 0; v < primitivePoints.Count; v += 3) {
                var v1 = primitivePoints[v + 0].Index;
                var v2 = primitivePoints[v + 1].Index;
                var v3 = primitivePoints[v + 2].Index;

                if (primitive.VertexOrder == VertexOrder.FLIP) {
                  indexArray[v + 0] = (uint) v1;
                  indexArray[v + 1] = (uint) v3;
                  indexArray[v + 2] = (uint) v2;
                } else {
                  indexArray[v + 0] = (uint) v1;
                  indexArray[v + 1] = (uint) v2;
                  indexArray[v + 2] = (uint) v3;
                }
              }

              var indexAccessor = gltfModel.CreateAccessor();
              indexAccessor.SetIndexData(indexView, 0, primitivePoints.Count,
                                         IndexEncodingType.UNSIGNED_INT);

              gltfPrimitive.SetIndexAccessor(indexAccessor);

              break;
            }
            case PrimitiveType.TRIANGLE_STRIP: {
              /*var triangleStrip =
                  gltfMeshBuilder.UsePrimitive(materialBuilder, 3);
              for (var v = 0; v < pointsCount - 2; ++v) {
                VERTEX v1, v2, v3;
                if (v % 2 == 0) {
                  v1 = vertices[v + 0];
                  v2 = vertices[v + 1];
                  v3 = vertices[v + 2];
                } else {
                  // Switches drawing order to maintain proper winding:
                  // https://www.khronos.org/opengl/wiki/Primitive
                  v1 = vertices[v + 1];
                  v2 = vertices[v + 0];
                  v3 = vertices[v + 2];
                }
  
                // Intentionally flipped to fix bug where faces were backwards.
                triangleStrip.AddTriangle(v1, v3, v2);
              }*/
              break;
            }
            case PrimitiveType.TRIANGLE_FAN: {
              /*var triangleStrip =
                  gltfMeshBuilder.UsePrimitive(materialBuilder, 3);
  
              // https://stackoverflow.com/a/8044252
              var firstVertex = vertices[0];
              for (var v = 2; v < pointsCount; ++v) {
                var v1 = firstVertex;
                var v2 = vertices[v - 1];
                var v3 = vertices[v];
  
                // Intentionally flipped to fix bug where faces were backwards.
                triangleStrip.AddTriangle(v1, v3, v2);
              }*/
              break;
            }
            default: throw new NotImplementedException();
          }
        }

        gltfMeshes.Add(gltfMesh);
      }
      return gltfMeshes;
    }

    private static Vector4 FinToGltfColor_(IColor? color)
      => new(color?.Rf ?? 1, color?.Gf ?? 0, color?.Bf ?? 1, color?.Af ?? 1);
  }
}