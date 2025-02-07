using System.Numerics;

using fin.data.lazy;
using fin.image;
using fin.io;
using fin.math;
using fin.math.transform;
using fin.model;
using fin.model.impl;
using fin.model.util;
using fin.scene;
using fin.util.asserts;
using fin.util.enums;
using fin.util.sets;

using gm.api;
using gm.schema.mod;

using pmdc.schema.lvl;

namespace pmdc.api {
  public class LvlSceneFileBundle : ISceneFileBundle {
    public string? GameName => "paper_mario_directors_cut";

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
            = new OmdModelImporter().Import(new OmdModelFileBundle {
                GameName = sceneFileBundle.GameName,
                OmdFile = modelFile
            });
        finArea.AddObject().AddSceneModel(finModel);
      }

      var textureDirectory
          = sceneFileBundle.RootDirectory.AssertGetExistingSubdir("Textures");
      var lazyImageMap = new LazyDictionary<string?, IImage?>(
          imageName => imageName != null &&
                       textureDirectory.TryToGetExistingFile(
                           $"{imageName}.png",
                           out var textureFile)
              ? FinImage.FromFile(textureFile)
              : null);

      if (lvl.FloorBlocks.Count > 0) {
        foreach (var floorBlockParams in lvl.FloorBlocks) {
          var (start, end, textureName, type, flags) = floorBlockParams;

          var (floorBlockModel, floorBlockRootBone)
              = ModModelImporter.CreateModel();
          var floorBlockSkin = floorBlockModel.Skin;
          var floorBlockMesh = floorBlockSkin.AddMesh();

          var shouldRepeat = !flags.CheckFlag(FloorBlockFlags.NO_REPEAT);
          var floorBlockMaterialManager = floorBlockModel.MaterialManager;
          IMaterial? floorBlockMaterial = null;
          if (flags.CheckFlag(FloorBlockFlags.INVISIBLE)) {
            floorBlockMaterial = floorBlockMaterialManager.AddHiddenMaterial();
          } else {
            var image = lazyImageMap[textureName];
            if (image != null) {
              var floorBlockTexture
                  = floorBlockMaterialManager.CreateTexture(image);
              floorBlockTexture.Name = textureName;
              floorBlockMaterial
                  = floorBlockMaterialManager.AddTextureMaterial(
                      floorBlockTexture);

              if (shouldRepeat) {
                floorBlockTexture.WrapModeU = WrapMode.REPEAT;
                floorBlockTexture.WrapModeV = WrapMode.REPEAT;
              }
            }
          }

          switch (type) {
            case FloorBlockType.WALL: {
              (float, float)? repeat = shouldRepeat
                  ? ((end.Xy() - start.Xy()).Length() / 64,
                     Math.Abs(end.Z - start.Z) / 64)
                  : null;
              floorBlockMesh.AddSimpleWall(floorBlockSkin,
                                           start,
                                           end,
                                           floorBlockMaterial,
                                           floorBlockRootBone,
                                           repeat);
              break;
            }
            case FloorBlockType.FLOOR: {
              (float, float, float)? repeat = shouldRepeat
                  ? (Math.Abs(end.X - start.X) / 64,
                     Math.Abs(end.Y - start.Y) / 64,
                     Math.Abs(end.Z - start.Z) / 64)
                  : null;

              floorBlockMesh.AddSimpleCube(floorBlockSkin,
                                           start,
                                           end,
                                           floorBlockMaterial,
                                           floorBlockRootBone,
                                           repeat);
              break;
            }
          }

          finArea.AddObject().AddSceneModel(floorBlockModel);
        }
      }

      if (lvl.Trees.Count > 0) {
        var treeModel = CreateTreeModel_(sceneFileBundle.RootDirectory);

        foreach (var treePosition in lvl.Trees) {
          finArea.AddObject()
                 .SetPosition(treePosition.X, treePosition.Z, treePosition.Y)
                 .AddSceneModel(treeModel);
        }
      }

      if (sceneFileBundle.LvlFile.Name is "battle.lvl") {
        var battleWallModel = CreateBattleWallModel_(lazyImageMap);

        finArea.AddObject()
               .SetPosition(176, 0, 176)
               .AddSceneModel(battleWallModel);
        finArea.AddObject()
               .SetPosition(176, 0, 464)
               .AddSceneModel(battleWallModel);

        var (battleFloorModel, battleFloorRootBone)
            = ModModelImporter.CreateModel();

        var bfSkin = battleFloorModel.Skin;
        var bfMesh = bfSkin.AddMesh();
        var bfMaterialManager = battleFloorModel.MaterialManager;

        var frontOfFloorImage = lazyImageMap["bacFrontOfFloor"].AssertNonnull();
        var frontOfFloorTexture
            = bfMaterialManager.CreateTexture(frontOfFloorImage);
        frontOfFloorTexture.WrapModeV = WrapMode.REPEAT;
        var frontOfFloorMaterial
            = bfMaterialManager.AddTextureMaterial(frontOfFloorTexture);

        bfMesh.AddSimpleFloor(bfSkin,
                              new Vector3(0, 16, -64),
                              new Vector3(64, 640, 0),
                              frontOfFloorMaterial,
                              battleFloorRootBone,
                              (1, 10));
        finArea.AddObject()
               .AddSceneModel(battleFloorModel);
      }

      if (lvl.BackgroundName != null) {
        var backgroundImageFile
            = sceneFileBundle
              .RootDirectory.AssertGetExistingSubdir("Backgrounds")
              .AssertGetExistingFile($"{lvl.BackgroundName}.png");

        var backgroundImage = FinImage.FromFile(backgroundImageFile);
        var backgroundObject = finArea.CreateCustomSkyboxObject();
      }

      finScene.CreateDefaultLighting(finArea.AddObject());

      return finScene;
    }

    private static IModel CreateBattleWallModel_(
        ILazyDictionary<string?, IImage?> lazyImageMap) {
      var (battleWallModel, battleWallRootBone)
          = ModModelImporter.CreateModel();

      var battleSkin = battleWallModel.Skin;
      var battleWallMesh = battleSkin.AddMesh();
      var battleMaterialManager = battleWallModel.MaterialManager;

      var frontOfFloorImage = lazyImageMap["bacFrontOfFloor"].AssertNonnull();
      var frontOfFloorTexture
          = battleMaterialManager.CreateTexture(frontOfFloorImage);
      frontOfFloorTexture.WrapModeU = WrapMode.REPEAT;
      var frontOfFloorMaterial
          = battleMaterialManager.AddTextureMaterial(frontOfFloorTexture);

      battleWallMesh.AddSimpleWall(battleSkin,
                                   new Vector3(-32, -12, 160),
                                   new Vector3(32, -12, 0),
                                   frontOfFloorMaterial,
                                   battleWallRootBone,
                                   (-1, 1));
      battleWallMesh.AddSimpleWall(battleSkin,
                                   new Vector3(-32, 12, 160),
                                   new Vector3(32, 12, 0),
                                   frontOfFloorMaterial,
                                   battleWallRootBone,
                                   (-1, 1));

      var battleWallImage = lazyImageMap["bacBattleWall"].AssertNonnull();
      var battleWallTexture
          = battleMaterialManager.CreateTexture(battleWallImage);
      battleWallTexture.WrapModeV = WrapMode.REPEAT;
      var battleWallMaterial
          = battleMaterialManager.AddTextureMaterial(battleWallTexture);

      battleWallMesh.AddSimpleWall(battleSkin,
                                   new Vector3(-32, -12, 160),
                                   new Vector3(-32, 12, 0),
                                   battleWallMaterial,
                                   battleWallRootBone,
                                   (1, 6));

      return battleWallModel;
    }

    private static IModel CreateTreeModel_(
        IReadOnlyTreeDirectory rootDirectory) {
      var treeDirectory
          = rootDirectory.AssertGetExistingSubdir(
              "Models/Tree");

      var (treeModel, treeRootBone) = ModModelImporter.CreateModel();
      var treeSkin = treeModel.Skin;
      var treeMaterialManager = treeModel.MaterialManager;

      // Bark
      {
        var barkRootBone = treeRootBone.AddChild(0, 0, 12);
        barkRootBone.LocalTransform.SetRotationDegrees(0, 0, 45);
        barkRootBone.LocalTransform.SetScale(1.5f, 1.5f, 2);

        ModModelImporter.AddToModel(
            treeDirectory
                .AssertGetExistingFile("treemodel1.mod")
                .ReadNewFromText<Mod>(),
            treeModel,
            barkRootBone,
            out _,
            out var barkPrimitive);
        var barkTexture = treeMaterialManager.CreateTexture(
            FinImage.FromFile(
                treeDirectory.AssertGetExistingFile("bacTree.png")));
        var barkMaterial = treeMaterialManager.AddTextureMaterial(barkTexture);
        barkPrimitive.SetMaterial(barkMaterial);
      }

      // Leaves1
      { }

      // Shadow
      {
        var shadowBone = treeRootBone.AddChild(0, 0, 0.05f);

        var shadowTexture = treeMaterialManager.CreateTexture(
            FinImage.FromFile(
                treeDirectory.AssertGetExistingFile("bacShadowXL.png")));
        var shadowMaterial
            = treeMaterialManager.AddTextureMaterial(shadowTexture);

        var shadowSize = 96;
        var shadowMesh = treeSkin.AddMesh();
        shadowMesh.AddSimpleCube(
            treeSkin,
            new Vector3(-shadowSize, -shadowSize, 0),
            new Vector3(shadowSize, shadowSize, 0),
            shadowMaterial,
            shadowBone);
      }

      return treeModel;
    }
  }
}