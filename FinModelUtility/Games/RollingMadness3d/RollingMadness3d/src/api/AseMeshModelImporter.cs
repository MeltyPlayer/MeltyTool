using System.Numerics;

using fin.data.dictionaries;
using fin.data.lazy;
using fin.image;
using fin.io;
using fin.language.equations.fixedFunction;
using fin.model;
using fin.model.impl;
using fin.model.io;
using fin.model.io.importers;
using fin.util.sets;

using rollingMadness.schema;

namespace rollingMadness.api;

public record AseMeshModelFileBundle(
    IReadOnlyTreeFile AseMeshFile,
    IReadOnlyTreeDirectory TextureDirectory)
    : IModelFileBundle<AseMeshModelFileBundle, AseMeshModelImporter> {
  public IReadOnlyTreeFile MainFile => this.AseMeshFile;
}

public sealed class AseMeshModelImporter
    : IModelImporter<AseMeshModelImporter, AseMeshModelFileBundle> {
  public IModel Import(AseMeshModelFileBundle fileBundle) {
    var aseMesh = fileBundle.AseMeshFile.ReadNew<AseMesh>();

    var fileSet = fileBundle.AseMeshFile.AsFileSet();
    var finModel = new ModelImpl {
        FileBundle = fileBundle,
        Files = fileSet,
    };

    var finMaterialManager = finModel.MaterialManager;
    var lazyTextureMap
        = new LazyDictionary<(string aseImageName, int uvIndex, bool isLightmap),
            ITexture>(tuple => {
          var (aseImageName, uvIndex, isLightmap) = tuple;

          var imageFile = fileBundle.TextureDirectory.AssertGetExistingFile(
              aseImageName);
          fileSet.Add(imageFile);
          var finImage = FinImage.FromFile(imageFile);

          var finTexture = finMaterialManager.CreateTexture(finImage);
          finTexture.Name = imageFile.NameWithoutExtension.ToString();
          finTexture.MinFilter = TextureMinFilter.LINEAR;

          finTexture.UvIndex = uvIndex;

          if (!isLightmap) {
            finTexture.WrapModeU = finTexture.WrapModeV = WrapMode.REPEAT;
          }

          return finTexture;
        });
    var lazyMaterialMap
        = new LazyDictionary<(int aseMainTextureIndex, int aseDecalTextureIndex,
            int aseLightmapIndex), IReadOnlyMaterial>(tuple => {
          var (aseMainTextureIndex, aseDecalTextureIndex, aseLightmapIndex) = tuple;

          var finMaterial = finMaterialManager.AddFixedFunctionMaterial();

          var finMainTexture = lazyTextureMap[
              (aseMesh.ImageNames[aseMainTextureIndex].Value, 0, false)];
          finMaterial.Name = finMainTexture.Name;

          var (diffuseColor, diffuseAlpha)
              = finMaterial.AddTextureSourceColorAlpha(finMainTexture);

          var equations = finMaterial.Equations;
          if (aseDecalTextureIndex >= 0) {
            var finDecalTexture = lazyTextureMap[
                (aseMesh.ImageNames[aseDecalTextureIndex].Value,
                 1, false)];

            finMaterial.Name += $"/{finDecalTexture.Name}";

            var (decalColor, decalAlpha)
                = finMaterial.AddTextureSourceColorAlpha(finDecalTexture);

            diffuseColor = equations.ColorOps.MixWithScalar(
                    diffuseColor,
                    decalColor,
                    decalAlpha);
          }

          IColorValue ambientColor = equations.CreateOrGetColorInput(
                  FixedFunctionSource.LIGHT_AMBIENT_COLOR);
          if (aseLightmapIndex >= 0) {
            var aseLightmapName
                = aseMesh.LightmapNames[aseLightmapIndex].Value;
            var finLightmapTexture = lazyTextureMap[(aseLightmapName, 2, true)];
            finMaterial.Name += $"/{finLightmapTexture.Name}";
            ambientColor = ambientColor.Multiply(finMaterial.AddTextureSourceColor(finMainTexture));
          }

          var outputColorAlpha
              = equations.GenerateLighting((diffuseColor, diffuseAlpha),
                                           ambientColor);

          equations.SetOutputColorAlpha(outputColorAlpha);

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

                      finVertex.SetUv(0, aseUvData.Uv0);
                      finVertex.SetUv(1, aseUvData.Uv1);
                      finVertex.SetUv(2, aseUvData.LightmapUv);

                      return (IReadOnlyVertex) finVertex;
                    })
                    .ToArray();

    var trianglesByMaterialIndex
        = aseMesh.Triangles.ToListDictionary(t => (t.MainTextureIndex,
                                                   t.DecalTextureIndex,
                                                   t.LightmapIndex));

    var mainAndDecalAndLightmapIndexes = trianglesByMaterialIndex.Keys.OrderBy(k => k.MainTextureIndex)
                                       .ThenBy(k => k.DecalTextureIndex)
                                       .ThenBy(k => k.LightmapIndex);

    foreach (var mainAndDecalAndLightmapIndex in mainAndDecalAndLightmapIndexes) {
      var finMaterial = lazyMaterialMap[mainAndDecalAndLightmapIndex];

      var aseTriangles = trianglesByMaterialIndex[mainAndDecalAndLightmapIndex];
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