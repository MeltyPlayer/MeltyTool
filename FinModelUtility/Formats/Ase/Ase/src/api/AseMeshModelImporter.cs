using System.Numerics;

using fin.data.dictionaries;
using fin.data.lazy;
using fin.io;
using fin.model;
using fin.model.impl;
using fin.model.io;
using fin.model.io.importers;
using fin.model.util;
using fin.util.sets;

using rollingMadness.schema;

namespace rollingMadness.api;

public record AseMeshModelFileBundle(
    IReadOnlyTreeFile AseMeshFile,
    IReadOnlyTreeDirectory TextureDirectory) : IModelFileBundle {
  public IReadOnlyTreeFile MainFile => this.AseMeshFile;
}

public class AseMeshModelImporter : IModelImporter<AseMeshModelFileBundle> {
  public IModel Import(AseMeshModelFileBundle fileBundle) {
    var aseMesh = fileBundle.AseMeshFile.ReadNew<AseMesh>();

    var fileSet = fileBundle.AseMeshFile.AsFileSet();
    var finModel = new ModelImpl {
        FileBundle = fileBundle,
        Files = fileSet,
    };

    var finMaterialManager = finModel.MaterialManager;
    var lazyMaterialMap = new LazyDictionary<uint, IReadOnlyMaterial>(
        aseMaterialIndex => {
          var aseImageName = aseMesh.ImageNames0[aseMaterialIndex].Value;
          var imageFile
              = fileBundle.TextureDirectory.AssertGetExistingFile(aseImageName);
          var (finMaterial, finTexture)
              = finMaterialManager.AddSimpleTextureMaterialFromFile(imageFile);
          finTexture.WrapModeU = finTexture.WrapModeV = WrapMode.REPEAT;
          return finMaterial;
        });

    var finSkin = finModel.Skin;
    var finMesh = finSkin.AddMesh();

    var finVertices
        = Enumerable.Zip(aseMesh.Vertices, aseMesh.UvDatas)
                    .Select(tuple => {
                      var (aseVertex, aseUvData) = tuple;
                      var finVertex = finSkin.AddVertex(aseVertex.Position);
                      finVertex.SetLocalNormal(
                          Vector3.Normalize(aseVertex.Normal));

                      finVertex.SetUv(0, aseUvData.Uv);
                      finVertex.SetUv(1, aseUvData.LightmapUv);

                      return (IReadOnlyVertex) finVertex;
                    })
                    .ToArray();

    var trianglesByMaterialIndex
        = aseMesh.Triangles.ToListDictionary(t => t.MaterialIndex);
    foreach (var (materialIndex, aseTriangles) in trianglesByMaterialIndex
                 .GetPairs()) {
      var finMaterial = lazyMaterialMap[materialIndex];
      var triangleVertices = aseTriangles.Select(t => (
                                                     finVertices[t.Vertex1],
                                                     finVertices[t.Vertex2],
                                                     finVertices[t.Vertex3]))
                                         .ToArray();

      finMesh.AddTriangles(triangleVertices)
             .SetMaterial(finMaterial);
    }


    return finModel;
  }
}