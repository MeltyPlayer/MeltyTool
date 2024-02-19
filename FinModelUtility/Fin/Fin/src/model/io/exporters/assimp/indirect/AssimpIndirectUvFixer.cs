using System;
using System.Linq;

using Assimp;

using fin.color;
using fin.model.accessor;

namespace fin.model.io.exporters.assimp.indirect {
  public class AssimpIndirectUvFixer {
    public void Fix(IModel model, Scene sc) {
      var finVertices = model.Skin.Vertices;

      var vertexAccessor = ConsistentVertexAccessor.GetAccessorForModel(model);
      vertexAccessor.Target(finVertices[0]);
      var uvCount = vertexAccessor.UvCount;
      var colorCount = vertexAccessor.ColorCount;

      // Has to have a value or it will get deleted. 
      var nullUv = new Vector3D(0, 0, 0);
      var nullColor = new Color4D(1, 1, 1, 1);

      // Fix the UVs.
      var assMeshes = sc.Meshes;
      foreach (var assMesh in assMeshes) {
        var assUvIndices =
            assMesh.TextureCoordinateChannels[0].Select(uv => uv.X).ToList();
        var assColorIndices =
            assMesh.TextureCoordinateChannels[0]
                   .Select(uv => 1 - uv.Y)
                   .ToList();

        var assUvs = assMesh.TextureCoordinateChannels;
        foreach (var e in assUvs) {
          e.Clear();
        }

        var hadUv = new bool[uvCount];
        foreach (var assUvIndexFloat in assUvIndices) {
          var assUvIndex = (int) Math.Round(assUvIndexFloat);

          var finVertex = assUvIndex != -1 ? finVertices[assUvIndex] : null;
          for (var t = 0; t < uvCount; ++t) {
            TexCoord? uv = null;
            if (finVertex != null) {
              vertexAccessor.Target(finVertex);
              uv = vertexAccessor.GetUv(t);
            }

            if (uv != null) {
              hadUv[t] = true;
              assUvs[t].Add(new Vector3D(uv.Value.U, 1 - uv.Value.V, 0));
            } else {
              assUvs[t].Add(nullUv);
            }
          }
        }

        var assColors = assMesh.VertexColorChannels;
        foreach (var e in assColors) {
          e.Clear();
        }

        var hadColor = new bool[colorCount];
        foreach (var assColorIndexFloat in assColorIndices) {
          var assColorIndex = (int) Math.Round(assColorIndexFloat);

          var finVertex =
              assColorIndex != -1 ? finVertices[assColorIndex] : null;
          for (var c = 0; c < colorCount; ++c) {
            IColor? finColor = null;
            if (finVertex != null) {
              vertexAccessor.Target(finVertex);
              finColor = vertexAccessor.GetColor(c);
            }

            if (finColor != null) {
              hadColor[c] = true;
              assColors[c]
                  .Add(new Color4D(finColor.Rf,
                                   finColor.Gf,
                                   finColor.Bf,
                                   finColor.Af));
            } else {
              assColors[c].Add(nullColor);
            }
          }
        }


        for (var t = 0; t < uvCount; ++t) {
          // Deletes the channels that had no UVs.
          // UV component count has to be updated to work in Blender!
          if (!hadUv[t]) {
            assUvs[t].Clear();
            assMesh.UVComponentCount[t] = 0;
          } else {
            assMesh.UVComponentCount[t] = 2;
          }
        }

        for (var c = 0; c < colorCount; ++c) {
          if (!hadColor[c]) {
            assColors[c].Clear();
          }
        }
      }
    }
  }
}