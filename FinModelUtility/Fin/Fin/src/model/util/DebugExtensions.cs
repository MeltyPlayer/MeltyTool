using System.Drawing;
using System.Linq;

namespace fin.model.util;

public static class DebugExtensions {
  public static void AddDebugNormals<TVertex>(this IModel<ISkin<TVertex>> model)
      where TVertex : INormalVertex {
    var scale = 200;

    var skin = model.Skin;

    var vertices = skin.TypedVertices.ToArray();
    var lineVertices =
        vertices.Where(v => v.LocalNormal != null)
                .Select(v => {
                  var toVertex
                      = skin.AddVertex(v.LocalPosition +
                                       scale * v.LocalNormal!.Value);
                  toVertex.SetBoneWeights(v.BoneWeights!);
                  return ((IReadOnlyVertex) v, (IReadOnlyVertex) toVertex);
                });

    skin.AddMesh()
        .AddLines(lineVertices.ToArray())
        .SetMaterial(model.MaterialManager.AddColorMaterial(Color.Cyan));
  }
}