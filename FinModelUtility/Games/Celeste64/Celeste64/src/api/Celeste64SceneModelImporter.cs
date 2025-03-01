using System.Numerics;

using Celeste64.map;

using fin.data.lazy;
using fin.image;
using fin.io;
using fin.math.rotations;
using fin.model;
using fin.model.impl;
using fin.model.io.importers.gltf;
using fin.model.util;
using fin.scene;
using fin.util.enumerables;
using fin.util.sets;

using Sledge.Formats.Map.Objects;

namespace Celeste64.api;

public class Celeste64MapSceneFileBundle : ISceneFileBundle {
  public required IReadOnlyTreeFile MapFile { get; init; }
  public required IReadOnlyTreeDirectory ModelDirectory { get; init; }
  public required IReadOnlyTreeDirectory TextureDirectory { get; init; }

  public IReadOnlyTreeFile MainFile => this.MapFile;
}

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/EXOK/Celeste64/blob/bf7b209b7c56dad0e86a225f4d591ae3bccff455/Source/Data/Map.cs#L26
/// </summary>
public class Celeste64MapSceneImporter
    : ISceneImporter<Celeste64MapSceneFileBundle> {
  public IScene Import(Celeste64MapSceneFileBundle fileBundle) {
    using var s = fileBundle.MapFile.OpenRead();
    var celeste64Map = new Map(s);

    var fileSet = fileBundle.MapFile.AsFileSet();
    var finScene = new SceneImpl {
        FileBundle = fileBundle,
        Files = fileSet,
    };

    var finArea = finScene.AddArea();

    var mapObj = finArea.AddObject();
    mapObj.AddSceneModel(
        new Celeste64MapModelImporter().Import(
            new Celeste64MapModelFileBundle {
                MapFile = fileBundle.MapFile,
                TextureDirectory = fileBundle.TextureDirectory,
            },
            celeste64Map));

    var gltfModelImporter = new GltfModelImporter();
    var lazyModelMap = new LazyCaseInvariantStringDictionary<IReadOnlyModel?>(
        modelName => gltfModelImporter.Import(
            new GltfModelFileBundle(
                fileBundle.ModelDirectory
                          .AssertGetExistingFile($"{modelName}.glb"))));

    var worldSpawn = celeste64Map.Entities.SingleOrDefault(
        e => e.ClassName == "worldspawn");
    if (worldSpawn != null) {
      if (worldSpawn.Properties.TryGetValue("skybox",
                                            out var skyboxTextureName)) {
        var textureDirectory = fileBundle.TextureDirectory;

        var skyboxModel = ModelImpl.CreateForViewer();
        var skyboxMaterialManager = skyboxModel.MaterialManager;

        if (!textureDirectory.TryToGetExistingFile(
                $"Skyboxes/{skyboxTextureName}.png",
                out var skyboxFile)) {
          skyboxFile = textureDirectory.AssertGetExistingFile(
              $"Skyboxes/{skyboxTextureName}_0.png");
        }

        SkyboxImageLoader.LoadSkyboxImages(skyboxFile,
                                           out var topImage,
                                           out var backImage,
                                           out var rightImage,
                                           out var frontImage,
                                           out var leftImage,
                                           out var bottomImage);
        var topMaterial
            = skyboxMaterialManager
              .AddSimpleTextureMaterialFromImage(topImage)
              .Item1;
        var backMaterial
            = skyboxMaterialManager.AddSimpleTextureMaterialFromImage(
                                       backImage)
                                   .Item1;
        var rightMaterial
            = skyboxMaterialManager.AddSimpleTextureMaterialFromImage(
                                       rightImage)
                                   .Item1;
        var frontMaterial
            = skyboxMaterialManager.AddSimpleTextureMaterialFromImage(
                                       frontImage)
                                   .Item1;
        var leftMaterial
            = skyboxMaterialManager.AddSimpleTextureMaterialFromImage(
                                       leftImage)
                                   .Item1;
        var bottomMaterial
            = skyboxMaterialManager.AddSimpleTextureMaterialFromImage(
                                       bottomImage)
                                   .Item1;

        var skyboxSkin = skyboxModel.Skin;
        var skyboxMesh = skyboxSkin.AddMesh();

        var size = 5;
        skyboxMesh.AddSimpleCube(skyboxSkin,
                                 new Vector3(size, -size, size),
                                 new Vector3(-size, size, -size),
                                 topMaterial,
                                 rightMaterial,
                                 backMaterial,
                                 leftMaterial,
                                 frontMaterial,
                                 bottomMaterial);

        skyboxModel.DisableDepthOnAllMaterials();
        skyboxModel.FlipAllCullingInsideOut();
        skyboxModel.RemoveAllNormals();
        skyboxModel.SetAllTextureFiltering(TextureMinFilter.NEAR,
                                           TextureMagFilter.NEAR);

        finArea.CreateCustomSkyboxObject().AddSceneModel(skyboxModel);
      }
    }

    foreach (var entity in celeste64Map.Entities) {
      var actorType = GetActorType_(entity.ClassName);
      if (actorType == null) {
        continue;
      }

      var modelData = GetModelDataForEntity_(actorType.Value, entity);
      if (modelData == null) {
        continue;
      }

      var (modelNames, modelScale) = modelData.Value;
      var finModels = modelNames.Select(modelName => lazyModelMap[modelName])
                                .Nonnull()
                                .ToArray();
      if (finModels.Length == 0) {
        continue;
      }

      var origin = entity.GetVectorProperty("origin", Vector3.Zero);
      var angleRadians =
          entity.GetIntProperty("angle", 0) * FinTrig.DEG_2_RAD + MathF.PI;

      var finObj = finArea.AddObject();
      finObj.SetPosition(-origin.X, origin.Z, origin.Y);
      finObj.SetRotationRadians(0, angleRadians, 0);
      finObj.SetScale(modelScale, modelScale, modelScale);
      foreach (var finModel in finModels) {
        finObj.AddSceneModel(finModel);
      }
    }

    finScene.CreateDefaultLighting(finArea.AddObject());

    return finScene;
  }

  private enum ActorType {
    BADELINE,
    BREAK_BLOCK,
    CAR,
    CASSETTE,
    CASSETTE_BLOCK,
    COIN,
    DOUBLE_DASH_PUZZLE_BLOCK,
    DEATH_BLOCK,
    FALLING_BLOCK,
    FEATHER,
    FLOATY_BLOCK,
    GATE_BLOCK,
    GRANNY,
    MOVING_BLOCK,
    PLAYER_SPAWN,
    REFILL,
    SIGN_POST,
    SPIKE_BLOCK,
    SPRING,
    STATIC_PROP,
    STRAWBERRY,
    THEO,
    TRAFFIC_BLOCK,
  }

  private static ActorType? GetActorType_(string entityClassName)
    => entityClassName switch {
        "Badeline"    => ActorType.BADELINE,
        "Cassette"    => ActorType.CASSETTE,
        "Coin"        => ActorType.COIN,
        "Feather"     => ActorType.FEATHER,
        "Granny"      => ActorType.GRANNY,
        "IntroCar"    => ActorType.CAR,
        "PlayerSpawn" => ActorType.PLAYER_SPAWN,
        "Refill"      => ActorType.REFILL,
        "SignPost"    => ActorType.SIGN_POST,
        "Spring"      => ActorType.SPRING,
        "StaticProp"  => ActorType.STATIC_PROP,
        "Strawberry"  => ActorType.STRAWBERRY,
        "Theo"        => ActorType.THEO,
        _             => null,
    };

  private static (string[] modelNames, float scale)? GetModelDataForEntity_(
      ActorType actorType,
      Entity entity)
    => actorType switch {
        ActorType.BADELINE => (["badeline"], 12),
        ActorType.CASSETTE => (["tape_1"], 12),
        ActorType.COIN     => (["coin"], 12),
        ActorType.CAR      => (["car_mirrors", "car_top", "car_wheels"], 36),
        ActorType.FEATHER  => (["feather"], 12),
        ActorType.GRANNY   => (["granny"], 12),
        ActorType.PLAYER_SPAWN =>
            entity.GetStringProperty("name", "Start") is "Start"
                ? (["player"], 12)
                : (["flag_off"], 1),
        ActorType.REFILL    => (["refill_gem"], 12),
        ActorType.SIGN_POST => (["sign"], 12),
        ActorType.SPRING    => (["spring_board"], 36),
        ActorType.STATIC_PROP => ([
            entity.GetStringProperty("model", "")[
                "Models/".Length..^".glb".Length]
        ], 1),
        ActorType.STRAWBERRY => (["strawberry"], 12),
        ActorType.THEO       => (["theo"], 12),
        _                    => null,
    };
}