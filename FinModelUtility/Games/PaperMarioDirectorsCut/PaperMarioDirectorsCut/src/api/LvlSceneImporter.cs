using fin.image;
using fin.io;
using fin.scene;
using fin.ui.rendering;
using fin.util.sets;

using pmdc.schema.lvl;

namespace pmdc.api {
  public class LvlSceneFileBundle : ISceneFileBundle {
    public string? GameName { get; }

    public IReadOnlyTreeFile MainFile => this.LvlFile;

    public required IReadOnlyTreeFile LvlFile { get; init; }
    public required IReadOnlyTreeDirectory RootDirectory { get; init; }
  }

  public class LvlSceneImporter : ISceneImporter<LvlSceneFileBundle> {
    public IScene Import(LvlSceneFileBundle sceneFileBundle) {
      var lvlFile = sceneFileBundle.LvlFile;
      var lvl = lvlFile.ReadNewFromText<Lvl>();

      var files = sceneFileBundle.LvlFile.AsFileSet();
      var finScene = new SceneImpl {
          FileBundle = sceneFileBundle,
          Files = files
      };

      var finArea = finScene.AddArea();

      if (lvl.HasRoomModel) {
        var modelFile = lvlFile.AssertGetParent()
                               .AssertGetExistingFile("model.omd");
        files.Add(modelFile);

        var finModel
            = new OmdModelImporter().Import(new OmdModelFileBundle
                                                { OmdFile = modelFile });
        finArea.AddObject().AddSceneModel(finModel);
      }

      if (lvl.BackgroundName != null) {
        var backgroundImageFile
            = sceneFileBundle
              .RootDirectory.AssertGetExistingSubdir("Backgrounds")
              .AssertGetExistingFile($"{lvl.BackgroundName}.png");

        var backgroundImage = FinImage.FromFile(backgroundImageFile);
        var backgroundObject = finArea.CreateCustomSkyboxObject();
      }

      return finScene;
    }
  }
}