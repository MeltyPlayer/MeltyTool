using System.Drawing;

using fin.data.lazy;
using fin.io;
using fin.math.rotations;
using fin.model;
using fin.scene;
using fin.ui.rendering.gl.scene;

using gdl.schema.worlds;


namespace gdl.api;

public sealed class GauntletDarkLegacySceneFileBundle : ISceneFileBundle {
  public required IReadOnlyTreeFile WorldsFile { get; init; }
  public required IReadOnlyTreeFile ObjectsFile { get; init; }
  public required IReadOnlyTreeFile AnimFile { get; init; }
  public required IReadOnlyTreeFile TexturesFile { get; init; }

  public required IReadOnlyTreeDirectory MonstersDirectory { get; init; }

  public IReadOnlyTreeFile MainFile => this.WorldsFile;
}

public sealed class GauntletDarkLegacySceneImporter
    : ISceneImporter<GauntletDarkLegacySceneFileBundle> {
  public IScene Import(GauntletDarkLegacySceneFileBundle fileBundle) {
    var finScene = new SceneImpl {
        FileBundle = fileBundle,
        Files = new HashSet<IReadOnlyStandaloneFile>([
            fileBundle.WorldsFile,
            fileBundle.ObjectsFile,
            fileBundle.AnimFile,
            fileBundle.TexturesFile,
        ])
    };

    var finArea = finScene.AddArea();
    finArea.CreateCustomSkyboxNode();
    finArea.BackgroundColor = Color.Black;

    var mapNode = finArea.AddRootNode();
    mapNode.AddComponent(
        new SimpleModelRenderComponent(
            new GauntletDarkLegacyWorldModelImporter().Import(
                new GauntletDarkLegacyWorldModelFileBundle {
                    WorldsFile = fileBundle.WorldsFile,
                    ObjectsFile = fileBundle.ObjectsFile,
                    AnimFile = fileBundle.AnimFile,
                    TexturesFile = fileBundle.TexturesFile
                })));

    var monstersDirectory = fileBundle.MonstersDirectory;

    var modelImporter = new GauntletDarkLegacyModelImporter();

    var lazyModels =
        new LazyCaseInvariantStringDictionary<IReadOnlyModel?>(description => {
          if (monstersDirectory.TryToGetExistingSubdir(
                  description,
                  out var monsterDirectory)) {
            if (monsterDirectory.TryToGetExistingFile(
                    "objects.ngc",
                    out var objectsFile) &&
                monsterDirectory.TryToGetExistingFile(
                    "ANIM.PS2",
                    out var animFile) &&
                monsterDirectory.TryToGetExistingFile(
                    "textures.ngc",
                    out var texturesFile)) {
              return modelImporter.Import(new GauntletDarkLegacyModelFileBundle {
                  ObjectsFile = objectsFile,
                  AnimFile = animFile,
                  TexturesFile = texturesFile,
                  IncludeMeshAnimations = false,
              });
            }
          }

          return null;
        });
    var lazyComponents =
        new LazyCaseInvariantStringDictionary<
            SimpleModelRenderComponent?>(description => {
          var model = lazyModels[description];
          return model != null ? new SimpleModelRenderComponent(model) : null;
        });

    var worlds = fileBundle.WorldsFile.ReadNew<Worlds>();
    foreach (var itemInstance in worlds.ItemInstances) {
      var itemInstanceInfo =
          worlds.ItemInstanceInfos[itemInstance.InstanceIndex];

      var component = lazyComponents[itemInstanceInfo.Description];
      if (component != null) {
        var itemNode = finArea.AddRootNode();
        itemNode.Name = itemInstanceInfo.Description;

        itemNode.SetPosition(itemInstance.Position with {
            X = -itemInstance.Position.X,
        });

        var rotationRadians = GdlRadiansUtil
                              .CreateQuaternion(itemInstance.Rotation)
                              .ToEulerRadians();
        itemNode.SetRotationRadians(rotationRadians);

        itemNode.AddComponent(component);
      }
    }

    return finScene;
  }
}