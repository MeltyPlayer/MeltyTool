using System.Collections.Generic;
using System.Linq;
using System.Numerics;

using fin.data.indexable;

namespace fin.model.processing;

public static class TextureTransformBaking {
  public static bool TryToBakeTextureTransforms(IModel model) {
    var skin = model.Skin;
    var vertices = skin.Vertices;

    var textureTransformByVertex
        = new IndexableDictionary<IReadOnlyVertex, IReadOnlyTextureTransform
            ?>(vertices.Count);

    var transforms = new HashSet<ITextureTransform>();

    // Gathers all transforms, makes sure each vertex only ever needs one.
    // Otherwise, fail early.
    foreach (var mesh in skin.Meshes) {
      foreach (var primitive in mesh.Primitives) {
        var material = primitive.Material;

        if (!TryToGetTextureTransform_(material, out var newTransform)) {
          return false;
        }

        if (newTransform != null) {
          transforms.Add(newTransform);
        }

        foreach (var vertex in primitive.Vertices.Distinct()) {
          if (!textureTransformByVertex.TryGetValue(
                  vertex,
                  out IReadOnlyTextureTransform oldTransform)) {
            textureTransformByVertex[vertex] = newTransform;
          } else if (!Equals(oldTransform, newTransform)) {
            return false;
          }
        }
      }
    }

    // Applies transforms to all UVs.
    foreach (var vertex in vertices) {
      if (!textureTransformByVertex.TryGetValue(vertex, out var transform)) {
        continue;
      }

      if (Equals(transform, null)) {
        continue;
      }

      var matrix = transform.AsMatrix();

      switch (vertex) {
        case IMultiUvVertex multiUvVertex: {
          for (var i = 0; i < multiUvVertex.UvCount; ++i) {
            var uv = multiUvVertex.GetUv(i);
            if (uv == null) {
              continue;
            }
            
            multiUvVertex.SetUv(i, Vector2.Transform(uv.Value, matrix));
          }

          break;
        }
        case ISingleUvVertex singleUvVertex: {
          var uv = singleUvVertex.GetUv();
          if (uv == null) {
            continue;
          }

          singleUvVertex.SetUv(Vector2.Transform(uv.Value, matrix));
          break;
        }
      }
    }

    // Clears all transforms.
    foreach (var transform in transforms) {
      transform.SetCenter2d(0, 0);
      transform.SetTranslation2d(0, 0);
      transform.SetRotationRadians2d(0);
      transform.SetScale2d(1, 1);
    }

    return true;
  }

  private static bool TryToGetTextureTransform_(
      IReadOnlyMaterial? material,
      out ITextureTransform? textureTransform) {
    if (material == null) {
      textureTransform = null;
      return true;
    }

    var distinctTextures = material.Textures.Distinct();
    var distinctTextureCount = distinctTextures.Count();
    if (distinctTextureCount == 0) {
      textureTransform = null;
      return true;
    }

    var singleTextureTransform
        = distinctTextures.Select(t => t.TextureTransform)
                          .Distinct()
                          .SingleOrDefault();
    if (singleTextureTransform == null) {
      textureTransform = default;
      return false;
    }

    textureTransform = singleTextureTransform as ITextureTransform;
    return true;
  }
}