using System.Numerics;

using fin.data.queues;
using fin.math.transform;
using fin.model;
using fin.model.impl;
using fin.model.io.importers;
using fin.model.util;
using fin.util.asserts;
using fin.util.sets;

using schema.binary;

using sonicadventure.schema.model;

using Object = sonicadventure.schema.model.Object;

namespace sonicadventure.api;

public class SonicAdventureModelFileImporter
    : IModelImporter<SonicAdventureModelFileBundle> {
  public IModel Import(SonicAdventureModelFileBundle fileBundle) {
    using var fs = fileBundle.ModelFile.OpenRead();
    fs.Position = fileBundle.ModelFileOffset;

    var br = new SchemaBinaryReader(fs, Endianness.LittleEndian);

    var key = fileBundle.ModelFileKey;
    var saRootObj = new Object(key);
    saRootObj.Read(br);

    var files = fileBundle.ModelFile.AsFileSet();
    files.Add(fileBundle.TextureFile);

    var finModel = new ModelImpl {
        FileBundle = fileBundle,
        Files = files,
    };

    var finSkin = finModel.Skin;
    var objAndBoneQueue
        = new FinTuple2Queue<Object, IBone>((saRootObj,
                                             finModel.Skeleton.Root));
    while (objAndBoneQueue.TryDequeue(out var saObj, out var parentFinBone)) {
      var finBone = parentFinBone.AddChild(saObj.Position);
      finBone.LocalTransform.SetRotationDegrees(
          new Vector3(saObj.Rotation.X, saObj.Rotation.Y, saObj.Rotation.Z) /
          0x10000 *
          360);
      finBone.LocalTransform.SetScale(saObj.Scale);

      var saAttach = saObj.Attach;
      if (saAttach != null) {
        this.AddAttach_(finBone, finSkin, saAttach);
      }

      if (saObj.NextSibling != null) {
        objAndBoneQueue.Enqueue((saObj.NextSibling, parentFinBone));
      }

      if (saObj.FirstChild != null) {
        objAndBoneQueue.Enqueue((saObj.FirstChild, finBone));
      }
    }

    return finModel;
  }

  private void AddAttach_(
      IBone finBone,
      ISkin<NormalTangentMultiColorMultiUvVertexImpl> finSkin,
      Attach saAttach) {
    var saVertices = saAttach.Vertices;
    var saNormals = saAttach.Normals;

    var finBoneWeights
        = finSkin.GetOrCreateBoneWeights(VertexSpace.RELATIVE_TO_BONE, finBone);

    foreach (var saMesh in saAttach.Meshes) {
      var finMesh = finSkin.AddMesh();
      switch (saMesh.PolyType) {
        case PolyType.TRIANGLES: {
          var finVertices
              = saMesh.Polys.AssertAsA<TrianglesPoly[]>()
                      .SelectMany(t => t.VertexIndices)
                      .Select(v => {
                        var finVertex = finSkin.AddVertex(saVertices[v]);
                        if (saNormals != null) {
                          finVertex.SetLocalNormal(saNormals[v]);
                        }
                        finVertex.SetBoneWeights(finBoneWeights);
                        return finVertex;
                      })
                      .ToArray();
          finMesh.AddTriangles(finVertices);
          break;
        }
        case PolyType.QUADS: {
          var finVertices
              = saMesh.Polys.AssertAsA<QuadsPoly[]>()
                      .SelectMany(t => t.VertexIndices)
                      .Select(v => {
                        var finVertex = finSkin.AddVertex(saVertices[v]);
                        if (saNormals != null) {
                          finVertex.SetLocalNormal(saNormals[v]);
                        }
                        finVertex.SetBoneWeights(finBoneWeights);
                        return finVertex;
                      })
                      .ToArray();
          finMesh.AddQuads(finVertices);
          break;
        }
        case PolyType.TRIANGLE_STRIP1 or PolyType.TRIANGLE_STRIP2: {
          foreach (var triangleStripPoly in saMesh
                                            .Polys
                                            .AssertAsA<TriangleStripPoly[]>()) {
            var finVertices
                = triangleStripPoly
                  .VertexIndices
                  .Select(v => {
                    var finVertex = finSkin.AddVertex(saVertices[v]);
                    if (saNormals != null) {
                      finVertex.SetLocalNormal(saNormals[v]);
                    }
                    finVertex.SetBoneWeights(finBoneWeights);
                    return finVertex;
                  })
                  .ToArray();
            finMesh.AddTriangleStrip(finVertices);
          }
          break;
        }
        default: throw new ArgumentOutOfRangeException();
      }
    }
  }
}