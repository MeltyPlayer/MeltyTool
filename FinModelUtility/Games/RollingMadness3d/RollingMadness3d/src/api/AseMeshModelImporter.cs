using System.Numerics;

using fin.data.dictionaries;
using fin.data.lazy;
using fin.image;
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
    var lazyTextureMap
        = new LazyDictionary<(string aseImageName, bool isLightmap),
            ITexture>(
            tuple => {
              var (aseImageName, isLightmap) = tuple;

              var imageFile = fileBundle.TextureDirectory.AssertGetExistingFile(
                  aseImageName);
              var finImage = FinImage.FromFile(imageFile);

              var finTexture = finMaterialManager.CreateTexture(finImage);
              finTexture.Name = imageFile.NameWithoutExtension.ToString();

              if (isLightmap) {
                finTexture.UvIndex = 1;
              } else {
                finTexture.WrapModeU = finTexture.WrapModeV = WrapMode.REPEAT;
              }

              return finTexture;
            });
    var lazyMaterialMap
        = new LazyDictionary<(uint aseMaterialIndex, int aseLightmapIndex),
            IReadOnlyMaterial>(
            tuple => {
              var (aseMaterialIndex, aseLightmapIndex) = tuple;

              var aseImageName = aseMesh.ImageNames[aseMaterialIndex].Value;
              var finTexture = lazyTextureMap[(aseImageName, false)];

              var finMaterial = finMaterialManager.AddStandardMaterial();
              finMaterial.Name = finTexture.Name;
              finMaterial.DiffuseTexture = finTexture;

              if (aseLightmapIndex >= 0) {
                var aseLightmapName
                    = aseMesh.LightmapNames[aseLightmapIndex].Value;
                var lightmapTexture = lazyTextureMap[(aseLightmapName, true)];
                finMaterial.Name += $"/{lightmapTexture.Name}";
                finMaterial.AmbientOcclusionTexture = lightmapTexture;
              }

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
        = aseMesh.Triangles.ToListDictionary(
            t => (t.MaterialIndex, t.LightmapIndex));
    foreach (var (materialAndLightmapIndex, aseTriangles) in
             trianglesByMaterialIndex.GetPairs()) {
      var (materialIndex, lightmapIndex) = materialAndLightmapIndex;
      var finMaterial = lazyMaterialMap[(materialIndex, lightmapIndex)];
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