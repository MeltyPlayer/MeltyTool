using fin.image;
using fin.io;
using fin.model;
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
      var (finModel, finRootBone) = ModModelImporter.CreateModel((modelFileBundle, files));

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
        ModModelImporter.AddToModel(omdMesh.Mod,
                                    finModel,
                                    finRootBone,
                                    out var finMesh,
                                    out var finPrimitive);
        finMesh.Name = omdMesh.Name;
        finPrimitive.SetMaterial(finMaterials[omdMesh.MaterialIndex]);
      }

      return finModel;
    }
  }
}