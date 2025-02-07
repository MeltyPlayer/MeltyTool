using fin.image;
using fin.io;
using fin.model;
using fin.model.io;
using fin.model.io.importers;
using fin.util.sets;

using gm.schema.omd;

namespace gm.api;

public class OmdModelFileBundle : IModelFileBundle {
  public required string GameName { get; init; }
  public required IReadOnlyTreeFile OmdFile { get; init; }
  public Action<IModel>? Mutator { get; init; }
  public IReadOnlyTreeFile MainFile => this.OmdFile;
}

public class OmdModelImporter : IModelImporter<OmdModelFileBundle> {
  public IModel Import(OmdModelFileBundle modelFileBundle) {
    var omdFile = modelFileBundle.OmdFile;
    var omd = omdFile.ReadNewFromText<Omd>();

    var files = omdFile.AsFileSet();
    var (finModel, finRootBone)
        = D3dModelImporter.CreateModel((modelFileBundle, files));

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
      D3dModelImporter.AddToModel(omdMesh.D3d,
                                  finModel,
                                  finRootBone,
                                  out var finMesh,
                                  finMaterials[omdMesh.MaterialIndex]);
      finMesh.Name = omdMesh.Name;
    }

    modelFileBundle.Mutator?.Invoke(finModel);

    return finModel;
  }
}