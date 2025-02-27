using fin.io;
using fin.model;
using fin.scene;
using fin.util.sets;

using jsystem.api;

using mkdd.schema.bol;

namespace mkdd.api;

public record BolSceneFileBundle(IFileHierarchyFile BolFile)
    : ISceneFileBundle {
  public string? GameName => "mario_kart_double_dash";
  public IReadOnlyTreeFile? MainFile => this.BolFile;
}

public class BolSceneImporter : ISceneImporter<BolSceneFileBundle> {
  public IScene Import(BolSceneFileBundle fileBundle) {
    var bol = fileBundle.BolFile.ReadNew<Bol>();

    var fileSet = fileBundle.BolFile.AsFileSet();
    var finScene = new SceneImpl() {
        FileBundle = fileBundle,
        Files = fileSet,
    };

    var courseDirectory = fileBundle.BolFile.Parent!;
    var bmdFiles = courseDirectory.FilesWithExtension(".bmd").ToArray();

    var finArea = finScene.AddArea();

    var bmdModelImporter = new BmdModelImporter();

    var courseBmd
        = bmdFiles.Single(f => f.NameWithoutExtension.EndsWith("_course"));
    fileSet.Add(courseBmd);
    finArea.AddObject()
           .AddSceneModel(bmdModelImporter.Import(new BmdModelFileBundle {
               BmdFile = courseBmd
           }));

    var skyBmd = bmdFiles.SingleOrDefault(
        f => f.NameWithoutExtension.EndsWith("_sky"));
    if (skyBmd != null) {
      fileSet.Add(skyBmd);

      var skyModel = bmdModelImporter.Import(new BmdModelFileBundle {
          BmdFile = skyBmd
      });
      foreach (var finMaterial in skyModel.MaterialManager.All) {
        finMaterial.DepthMode = DepthMode.NONE;
        finMaterial.DepthCompareType = DepthCompareType.Always;
      }

      var skyObject = finArea.CreateCustomSkyboxObject();
      skyObject.AddSceneModel(skyModel);

      var scale = .05f;
      skyObject.SetScale(scale, scale, scale);
      skyObject.Rotation.SetDegrees(90, 0, 0);
    }

    return finScene;
  }
}