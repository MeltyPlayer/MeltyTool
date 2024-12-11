using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

using CommunityToolkit.HighPerformance;

using fin.color;
using fin.math;
using fin.model.accessor;
using fin.model.util;

using SharpGLTF.Schema2;

using FinPrimitiveType = fin.model.PrimitiveType;
using GltfPrimitiveType = SharpGLTF.Schema2.PrimitiveType;


namespace fin.model.io.exporters.gltf.lowlevel;

public class LowLevelGltfMeshBuilder {
  public bool UvIndices { get; set; }

  public IList<Mesh> BuildAndBindMesh(
      ModelRoot gltfModel,
      IReadOnlyModel model,
      float scale,
      IDictionary<IReadOnlyMaterial, Material> finToTexCoordAndGltfMaterial) {
    var skin = model.Skin;
    var vertexAccessor = ConsistentVertexAccessor.GetAccessorForModel(model);

    var boneTransformManager = new BoneTransformManager();
    boneTransformManager.CalculateStaticMatricesForManualProjection(model);

    var nullMaterial = gltfModel.CreateMaterial("null");
    nullMaterial.DoubleSided = false;
    nullMaterial.WithPBRSpecularGlossiness();

    var points = model.Skin.Vertices;
    var pointsCount = points.Count;

    var positionView = gltfModel.CreateBufferView(
        4 * 3 * pointsCount,
        0,
        BufferMode.ARRAY_BUFFER);
    var positionSpan = positionView.Content.AsSpan().Cast<byte, Vector3>();

    var normalView = gltfModel.CreateBufferView(
        4 * 3 * pointsCount,
        0,
        BufferMode.ARRAY_BUFFER);
    var normalSpan = normalView.Content.AsSpan().Cast<byte, Vector3>();

    for (var p = 0; p < pointsCount; ++p) {
      vertexAccessor.Target(points[p]);
      var point = vertexAccessor;

      boneTransformManager.ProjectVertexPositionNormal(
          point,
          out var outPosition,
          out var outNormal);
      positionSpan[p] = outPosition * scale;

      if (point.LocalNormal != null) {
        normalSpan[p] = outNormal;
      }

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

    var positionAccessor = gltfModel.CreateAccessor();
    positionAccessor.SetVertexData(
        positionView,
        0,
        pointsCount);

    var normalAccessor = gltfModel.CreateAccessor();
    normalAccessor.SetVertexData(
        normalView,
        0,
        pointsCount);

    var gltfMeshes = new List<Mesh>();
    foreach (var finMesh in skin.Meshes) {
      var gltfMesh = gltfModel.CreateMesh(finMesh.Name);

      foreach (var finPrimitive in finMesh.Primitives) {
        Material material;
        if (finPrimitive.Material != null) {
          material = finToTexCoordAndGltfMaterial[finPrimitive.Material];
        } else {
          material = nullMaterial;
        }

        var gltfPrimitive = gltfMesh.CreatePrimitive();
        gltfPrimitive.Material = material;

        gltfPrimitive.SetVertexAccessor("POSITION", positionAccessor);
        gltfPrimitive.SetVertexAccessor("NORMAL", normalAccessor);

        if (finPrimitive.Type != FinPrimitiveType.QUADS &&
            finPrimitive.VertexOrder == VertexOrder.NORMAL) {
          gltfPrimitive.DrawPrimitiveType = finPrimitive.Type switch {
              FinPrimitiveType.TRIANGLES => GltfPrimitiveType.TRIANGLES,
              FinPrimitiveType.TRIANGLE_STRIP => GltfPrimitiveType
                  .TRIANGLE_STRIP,
              FinPrimitiveType.TRIANGLE_FAN => GltfPrimitiveType.TRIANGLE_FAN,
              FinPrimitiveType.LINES        => GltfPrimitiveType.LINES,
              FinPrimitiveType.LINE_STRIP   => GltfPrimitiveType.LINE_STRIP,
              FinPrimitiveType.POINTS       => GltfPrimitiveType.POINTS,
          };

          var finPrimitiveVertices = finPrimitive.Vertices;
          gltfPrimitive.SetIndexAccessor(
              CreateIndexAccessor_(
                  gltfModel,
                  finPrimitiveVertices.Select(vertex => vertex.Index)
                                      .ToArray()));
        } else {
          gltfPrimitive.DrawPrimitiveType = GltfPrimitiveType.TRIANGLES;

          var finTriangleVertexIndices =
              finPrimitive.GetOrderedTriangleVertexIndices().ToArray();
          gltfPrimitive.SetIndexAccessor(
              CreateIndexAccessor_(
                  gltfModel,
                  finTriangleVertexIndices));
          break;
        }
      }

      gltfMeshes.Add(gltfMesh);
    }

    // Vertex colors
    if (vertexAccessor.ColorCount > 0) {
      var colorView = gltfModel.CreateBufferView(
          4 * 4 * pointsCount,
          0,
          BufferMode.ARRAY_BUFFER);
      var colorSpan = colorView.Content.AsSpan().Cast<byte, Vector4>();

      for (var p = 0; p < pointsCount; ++p) {
        vertexAccessor.Target(points[p]);
        var point = vertexAccessor;

        var finColor0 = point.GetColor(0);
        var hasColor0 = finColor0 != null;
        var assColor0 = hasColor0
            ? FinToGltfColor_(
                finColor0)
            : Vector4.One;
        colorSpan[p] = assColor0;
      }

      var colorAccessor = gltfModel.CreateAccessor();
      colorAccessor.SetVertexData(
          colorView,
          0,
          pointsCount,
          DimensionType.VEC4);

      foreach (var gltfMesh in gltfMeshes) {
        foreach (var gltfPrimitive in gltfMesh.Primitives) {
          gltfPrimitive.SetVertexAccessor("COLOR_0", colorAccessor);
        }
      }
    }

    // UVs
    if (vertexAccessor.UvCount > 0) {
      var uvView = gltfModel.CreateBufferView(
          2 * sizeof(float) * pointsCount,
          0,
          BufferMode.ARRAY_BUFFER);
      var uvSpan = uvView.Content.AsSpan().Cast<byte, Vector2>();

      for (var p = 0; p < pointsCount; ++p) {
        vertexAccessor.Target(points[p]);
        var point = vertexAccessor;

        var finUv = point.GetUv(0);
        uvSpan[p] = finUv.Value;
      }

      var uvAccessor = gltfModel.CreateAccessor();
      uvAccessor.SetVertexData(
          uvView,
          0,
          pointsCount,
          DimensionType.VEC2);

      foreach (var gltfMesh in gltfMeshes) {
        foreach (var gltfPrimitive in gltfMesh.Primitives) {
          gltfPrimitive.SetVertexAccessor("TEXCOORD_0", uvAccessor);
        }
      }
    }

    return gltfMeshes;
  }

  private static Vector4 FinToGltfColor_(IColor? color)
    => new(color?.Rf ?? 1, color?.Gf ?? 0, color?.Bf ?? 1, color?.Af ?? 1);

  private static Accessor CreateIndexAccessor_(
      ModelRoot gltfModelRoot,
      int[] vertexIndices) {
    var maxIndex = vertexIndices.Max();
    int bytesPerIndex = maxIndex switch {
        < byte.MaxValue   => 1,
        < ushort.MaxValue => 2,
        _                 => 4
    };
    var indexEncodingType = bytesPerIndex switch {
        1 => IndexEncodingType.UNSIGNED_BYTE,
        2 => IndexEncodingType.UNSIGNED_SHORT,
        4 => IndexEncodingType.UNSIGNED_INT,
    };

    var indexView = gltfModelRoot.CreateBufferView(
        bytesPerIndex * vertexIndices.Length,
        0,
        BufferMode.ELEMENT_ARRAY_BUFFER);

    switch (indexEncodingType) {
      case IndexEncodingType.UNSIGNED_BYTE: {
        int i = 0;
        var bSpan = indexView.Content.AsSpan();
        foreach (var v in vertexIndices) {
          bSpan[i++] = (byte) v;
        }
        break;
      }
      case IndexEncodingType.UNSIGNED_SHORT: {
        int i = 0;
        var sSpan = indexView.Content.AsSpan().Cast<byte, ushort>();
        foreach (var v in vertexIndices) {
          sSpan[i++] = (ushort) v;
        }
        break;
      }
      case IndexEncodingType.UNSIGNED_INT: {
        int i = 0;
        var iSpan = indexView.Content.AsSpan().Cast<byte, uint>();
        foreach (var v in vertexIndices) {
          iSpan[i++] = (uint) v;
        }
        break;
      }
      default:                               throw new ArgumentOutOfRangeException();
    }

    var indexAccessor = gltfModelRoot.CreateAccessor();
    indexAccessor.SetIndexData(indexView,
                               0,
                               vertexIndices.Length,
                               indexEncodingType);

    return indexAccessor;
  }
}