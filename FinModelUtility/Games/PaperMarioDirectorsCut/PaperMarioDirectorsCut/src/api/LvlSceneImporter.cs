using System.Numerics;

using fin.image;
using fin.io;
using fin.math.transform;
using fin.model;
using fin.model.impl;
using fin.model.util;
using fin.scene;
using fin.util.sets;

using pmdc.schema.lvl;
using pmdc.schema.mod;

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

      if (lvl.Trees.Count > 0) {
        var treeModel = CreateTreeModel_(sceneFileBundle.RootDirectory);

        foreach (var treePosition in lvl.Trees) {
          finArea.AddObject()
                 .SetPosition(treePosition.X, treePosition.Z, treePosition.Y)
                 .AddSceneModel(treeModel);
        }
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

    private static IModel
        CreateTreeModel_(IReadOnlyTreeDirectory rootDirectory) {
      var treeDirectory
          = rootDirectory.AssertGetExistingSubdir(
              "Models/Tree");

      var treeModel = new ModelImpl<NormalUvVertexImpl>(
          (index, position) => new NormalUvVertexImpl(index, position)) {
          FileBundle = null,
          Files = null,
      };

      var treeSkin = treeModel.Skin;
      var treeMaterialManager = treeModel.MaterialManager;
      ModModelImporter.CreateAdjustedRootBone(
          treeModel,
          out var treeRootBone);

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