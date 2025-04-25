using System.Numerics;

using fin.data.lazy;
using fin.data.queues;
using fin.language.equations.fixedFunction;
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
    var saRootObj = new Object(fileBundle.ModelFileOffset + key, key);
    saRootObj.Read(br);

    var files = fileBundle.ModelFile.AsFileSet();
    files.Add(fileBundle.TextureFile);

    var finModel = new ModelImpl {
        FileBundle = fileBundle,
        Files = files,
    };

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
        this.AddAttach_(finModel, finBone, saAttach);
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
      ModelImpl finModel,
      IReadOnlyBone finBone,
      Attach saAttach) {
    var saVertices = saAttach.Vertices;
    var saNormals = saAttach.Normals;

    var finSkin = finModel.Skin;
    var finBoneWeights
        = finSkin.GetOrCreateBoneWeights(VertexSpace.RELATIVE_TO_BONE, finBone);

    var finMaterialManager = finModel.MaterialManager;
    var lazyMaterials
        = new LazyDictionary<(uint materialId, bool hasVertexColors),
            IReadOnlyMaterial>(
            tuple => {
              var (materialId, hasVertexColors) = tuple;

              var saMaterial = saAttach.Materials[materialId];
              var finMaterial
                  = finMaterialManager.AddFixedFunctionMaterial();

              var equations = finMaterial.Equations;

              var saDiffuseColor = saMaterial.DiffuseColor;
              var (diffuseColor, diffuseAlpha)
                  = finMaterial.GenerateDiffuse(
                      (equations.CreateColorConstant(
                           saDiffuseColor.Rf,
                           saDiffuseColor.Gf,
                           saDiffuseColor.Bf),
                       equations
                           .CreateScalarConstant(saDiffuseColor.Af)),
                      null,
                      (hasVertexColors, hasVertexColors));

              var saSpecularColor = saMaterial.SpecularColor;
              var specularColor = equations.CreateColorConstant(
                  saSpecularColor.Rf,
                  saSpecularColor.Gf,
                  saSpecularColor.Bf);

              var ambientColor = equations.CreateColorConstant(.2f);
              var (outputColor, outputAlpha) = equations.GenerateLighting(
                  (diffuseColor, diffuseAlpha),
                  ambientColor,
                  specularColor,
                  equations.ColorOps.Zero);

              equations.SetOutputColorAlpha((outputColor, outputAlpha));

              return finMaterial;
            });

    foreach (var saMesh in saAttach.Meshes) {
      var saVertexColors = saMesh.VertexColors;
      var saUvs = saMesh.Uvs?
                        .Select(uv => new Vector2(uv.X, uv.Y) * 1f / 0x10000)
                        .ToArray();

      var finMaterial
          = lazyMaterials[(saMesh.MaterialId, saVertexColors != null)];

      var finMesh = finSkin.AddMesh();
      switch (saMesh.PolyType) {
        case PolyType.TRIANGLES: {
          var finVertices
              = saMesh.Polys.AssertAsA<TrianglesPoly[]>()
                      .SelectMany(t => t.VertexIndices)
                      .Select((v, i) => {
                        var finVertex = finSkin.AddVertex(saVertices[v]);

                        if (saNormals != null) {
                          finVertex.SetLocalNormal(saNormals[v]);
                        }

                        if (saVertexColors != null) {
                          finVertex.SetColor(saVertexColors[i]);
                        }

                        if (saUvs != null) {
                          finVertex.SetUv(saUvs[i]);
                        }

                        finVertex.SetBoneWeights(finBoneWeights);
                        return finVertex;
                      })
                      .ToArray();
          var finPrimitive = finMesh.AddTriangles(finVertices);
          finPrimitive.SetMaterial(finMaterial);
          break;
        }
        case PolyType.QUADS: {
          var finVertices
              = saMesh.Polys.AssertAsA<QuadsPoly[]>()
                      .SelectMany(t => t.VertexIndices)
                      .Select((v, i) => {
                        var finVertex = finSkin.AddVertex(saVertices[v]);

                        if (saNormals != null) {
                          finVertex.SetLocalNormal(saNormals[v]);
                        }

                        if (saVertexColors != null) {
                          finVertex.SetColor(saVertexColors[i]);
                        }

                        if (saUvs != null) {
                          finVertex.SetUv(saUvs[i]);
                        }

                        finVertex.SetBoneWeights(finBoneWeights);
                        return finVertex;
                      })
                      .ToArray();
          var finPrimitive = finMesh.AddQuads(finVertices);
          finPrimitive.SetMaterial(finMaterial);
          break;
        }
        case PolyType.TRIANGLE_STRIP1 or PolyType.TRIANGLE_STRIP2: {
          var i = 0;
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

                    if (saVertexColors != null) {
                      finVertex.SetColor(saVertexColors[i++]);
                    }

                    if (saUvs != null) {
                      finVertex.SetUv(saUvs[i++]);
                    }

                    finVertex.SetBoneWeights(finBoneWeights);
                    return finVertex;
                  })
                  .ToArray();
            var finPrimitive = finMesh.AddTriangleStrip(finVertices);
            finPrimitive.SetMaterial(finMaterial);
            finPrimitive.SetVertexOrder(triangleStripPoly.Direction == 0
                                            ? VertexOrder.COUNTER_CLOCKWISE
                                            : VertexOrder.CLOCKWISE);
          }

          break;
        }
        default: throw new ArgumentOutOfRangeException();
      }
    }
  }
}