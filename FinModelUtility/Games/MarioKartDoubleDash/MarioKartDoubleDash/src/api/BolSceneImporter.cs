using fin.data.lazy;
using fin.io;
using fin.model;
using fin.model.util;
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
      skyModel.DisableDepthOnAllMaterials();

      var skyObject = finArea.CreateCustomSkyboxObject();
      skyObject.AddSceneModel(skyModel);

      var scale = .05f;
      skyObject.SetScale(scale, scale, scale);
      skyObject.Rotation.SetDegrees(90, 0, 0);
    }

    var objectsDir = courseDirectory.AssertGetExistingSubdir("objects");
    var bmdPlugin = new BmdModelImporterPlugin();
    var lazyObjectModels = new LazyDictionary<ushort, IReadOnlyModel[]?>(
        objId => ObjectIdUtil
                 .GetFileNames(objId)
                 ?.Select(modelPaths => {
                   var modelFiles = modelPaths.Select(
                       p => objectsDir.AssertGetExistingFile(p));
                   return bmdPlugin.Import(modelFiles, 60);
                 })
                 .ToArray());

    foreach (var bolObj in bol.Objects) {
      var objModels = lazyObjectModels[bolObj.ObjectId];
      if (objModels != null) {
        var finObj = finArea.AddObject();
        finObj.SetPosition(bolObj.Position);

        foreach (var objModel in objModels) {
          finObj.AddSceneModel(objModel);
        }
      }
    }

    return finScene;
  }
}