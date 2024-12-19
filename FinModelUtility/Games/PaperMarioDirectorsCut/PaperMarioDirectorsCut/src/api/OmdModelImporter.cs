using fin.image;
using fin.io;
using fin.math.transform;
using fin.model;
using fin.model.impl;
using fin.model.io;
using fin.model.io.importers;
using fin.util.sets;

using pmdc.schema.omd;

namespace pmdc.api {
  public class OmdModelFileBundle : IModelFileBundle {
    public required string GameName { get; init; }

    public required IReadOnlyTreeFile OmdFile { get; init; }
    public IReadOnlyTreeFile MainFile => this.OmdFile;
  }

  public class OmdModelImporter : IModelImporter<OmdModelFileBundle> {
    public IModel Import(OmdModelFileBundle modelFileBundle) {
      var omdFile = modelFileBundle.OmdFile;
      var omd = omdFile.ReadNewFromText<Omd>();

      var files = omdFile.AsFileSet();
      var finModel = new ModelImpl<NormalUvVertexImpl>(
          (index, position) => new NormalUvVertexImpl(index, position)) {
          FileBundle = modelFileBundle,
          Files = files
      };

      var finSkeleton = finModel.Skeleton;
      var finRoot = finSkeleton.Root.AddRoot(0, 0, 0);
      finRoot.LocalTransform.SetRotationDegrees(-90, 180, 0);
      finRoot.LocalTransform.SetScale(-1, 1, 1);

      var finSkin = finModel.Skin;
      var boneWeights = finSkin.GetOrCreateBoneWeights(
          VertexSpace.RELATIVE_TO_BONE,
          finRoot);

      var finMaterialManager = finModel.MaterialManager;
      var finMaterials =
          omd
              .Materials
              .Select(omdMaterial => {
                var texturePath = omdMaterial.TexturePath;

                IMaterial finMaterial;
                if (texturePath.Length == 0 ||
                    !omdFile.AssertGetParent()
                            .TryToGetExistingFile(
                                texturePath,
                                out var imageFile)) {
                  finMaterial = finMaterialManager.AddNullMaterial();
                } else {
                  var image = FinImage.FromFile(imageFile);
                  files.Add(imageFile);

                  var finTexture = finMaterialManager.CreateTexture(image);
                  finTexture.Name = imageFile.NameWithoutExtension.ToString();
                  finTexture.WrapModeU = WrapMode.REPEAT;
                  finTexture.WrapModeV = WrapMode.REPEAT;

                  finMaterial =
                      finMaterialManager.AddTextureMaterial(finTexture);
                }

                finMaterial.Name = omdMaterial.Name;

                return finMaterial;
              })
              .ToArray();

      foreach (var omdMesh in omd.Meshes) {
        var finMesh = finSkin.AddMesh();
        finMesh.Name = omdMesh.Name;

        var finVertices =
            omdMesh
                .Vertices
                .Where(omdVertex => omdVertex.Something == 8)
                .Select(omdVertex => {
                  var finVertex = finSkin.AddVertex(omdVertex.Position);
                  finVertex.SetLocalNormal(-omdVertex.Normal);
                  finVertex.SetUv(omdVertex.Uv);
                  finVertex.SetBoneWeights(boneWeights);

                  return finVertex;
                })
                .ToArray();

        var finPrimitive = finMesh.AddTriangles(finVertices);
        finPrimitive.SetMaterial(finMaterials[omdMesh.MaterialIndex]);
      }

      return finModel;
    }
  }
}