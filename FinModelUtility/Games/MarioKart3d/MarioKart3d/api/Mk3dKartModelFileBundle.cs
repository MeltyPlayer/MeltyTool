using System.Collections.Generic;
using System.Numerics;

using fin.io;
using fin.model;
using fin.model.io;
using fin.model.io.importers;
using fin.model.util;

using gm.api;

namespace mk3d.api;

public sealed record Mk3dKartModelFileBundle(IReadOnlyTreeFile PlaceholderFile)
    : IModelFileBundle<Mk3dKartModelFileBundle, Mk3dKartModelImporter> {
  public IReadOnlyTreeFile MainFile => this.PlaceholderFile;
}

public sealed class Mk3dKartModelImporter
    : IModelImporter<Mk3dKartModelImporter, Mk3dKartModelFileBundle> {
  public IModel Import(Mk3dKartModelFileBundle fileBundle) {
    var rootDir = fileBundle.PlaceholderFile.AssertGetParent();

    var modelNames = new [] {
        "kart",
        "mario",
        "skeleton",
        "tires",
    };

    var fileSet = new HashSet<IReadOnlyStandaloneFile>();
    var (finModel, finRootBone) = D3dModelImporter.CreateModel((fileBundle, fileSet));

    foreach (var modelName in modelNames) {
      var modelDirectory = rootDir.AssertGetExistingSubdir(modelName);
      var smkFiles = modelDirectory.GetFilesWithFileType(".smk");

      foreach (var smkFile in smkFiles) {
        var smkBundle = Mk3dModelFileBundleUtil.FromSmkFile(smkFile);

        // TODO: Should deduplicate textures across files
        switch (smkFile.NameWithoutExtension) {
          case "mariohead": {
            var headBone = finRootBone.AddChild(Vector3.Zero);
            headBone.Name = "head";
            D3dModelImporter.AddToModel(smkBundle, finModel, fileSet, headBone, true);
            break;
          }
          case "tires2": {
            var leftTireBone = finRootBone.AddChild(new Vector3(-7, -8, 0));
            leftTireBone.Name = "leftTire";
            D3dModelImporter.AddToModel(smkBundle, finModel, fileSet, leftTireBone, true);

            var rightTireBone = finRootBone.AddChild(new Vector3(7, -8, 0));
            rightTireBone.Name = "rightTire";
            D3dModelImporter.AddToModel(smkBundle, finModel, fileSet, rightTireBone, true);
            break;
          }
          default: {
            D3dModelImporter.AddToModel(smkBundle, finModel, fileSet, finRootBone, true);
            break;
          }
        }
      }
    }

    return finModel;
  }
}