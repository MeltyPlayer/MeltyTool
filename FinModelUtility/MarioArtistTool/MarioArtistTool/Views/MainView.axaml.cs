using System;
using System.Numerics;

using Avalonia.Controls;

using fin.io.web;
using fin.math.matrix.four;
using fin.model.impl;
using fin.model.util;
using fin.scene;
using fin.scene.instance;
using fin.services;
using fin.ui.rendering.gl;
using fin.ui.rendering.gl.model;

using marioartist.api;

using MarioArtistTool.config;

using marioartisttool.services;
using marioartisttool.util;

namespace marioartisttool.Views;

public partial class MainView : UserControl {
  public MainView() {
    InitializeComponent();

    MfsFileSystemService.OnFileSelected += file => {
      LoadingStatusService.IsLoading = true;
      
      var scene = new SceneImpl {
          FileBundle = default,
          Files = default
      };

      var area = scene.AddArea();
      SetUpBackground_(area);
      area.CreateCustomSkyboxObject();

      switch (file?.FileType.ToLower()) {
        case ".tstlt": {
          try {
            var bundle = new TstltModelFileBundle(file);
            var model = new TstltModelLoader().Import(bundle);

            var config = Config.INSTANCE;
            config.MostRecentFileName = file.FullPath;
            config.Save();

            var characterObj = area.AddObject();
            characterObj.AddSceneModel(model);

            var lightingObj = area.AddObject();
            scene.CreateDefaultLighting(lightingObj);
          } catch (Exception e) {
            ExceptionService.HandleException(e, new LoadFileException(file));
            this.ViewerGlPanel.Scene = null;
          }

          break;
        }
      }

      var sceneInstance = new SceneInstanceImpl(scene);
      this.ViewerGlPanel.Scene = sceneInstance;

      LoadingStatusService.IsLoading = false;
    };

    this.ViewerGlPanel.OnInit += () => MfsFileSystemService.SelectFile(null);

    var camera = this.ViewerGlPanel.Camera;
    camera.Position = new Vector3(0, -1.35f, .3f);
    camera.PitchDegrees = 0;
    camera.YawDegrees = 90;
  }

  private static void SetUpBackground_(ISceneArea area) {
    area.BackgroundImage
        = AssetLoaderUtil.LoadImage("background_pretty.png");
    area.BackgroundImageScale = .3f;

    var backgroundFlowerImage
        = AssetLoaderUtil.LoadImage("background_flower.png");

    var backgroundFlowerModel = ModelImpl.CreateForViewer(4);
    var (backgroundFlowerModelMaterial, _) = backgroundFlowerModel
                                             .MaterialManager
                                             .AddSimpleTextureMaterialFromImage(
                                                 backgroundFlowerImage);

    var backgroundFlowerModelSkin = backgroundFlowerModel.Skin;
    backgroundFlowerModelSkin
        .AddMesh()
        .AddSimpleFloor(backgroundFlowerModelSkin,
                        new Vector3(-1, -1, 0),
                        new Vector3(1, 1, 0),
                        backgroundFlowerModelMaterial);

    var backgroundFlowerModelRenderer
        = new ModelRenderer(backgroundFlowerModel);

    var (topLeftCenter, topLeftSize)
        = GetCenterAndSize_(new Vector2(109, -2), new Vector2(221, 96));
    var (middleLeftCenter, middleLeftSize)
        = GetCenterAndSize_(new Vector2(171, 103), new Vector2(302, 234));
    var (bottomLeftCenter, bottomLeftSize)
        = GetCenterAndSize_(new Vector2(69, 280), new Vector2(240, 445));
    var (topRightCenter, topRightSize)
        = GetCenterAndSize_(new Vector2(393, -4), new Vector2(563, 153));
    var (middleRightCenter, middleRightSize)
        = GetCenterAndSize_(new Vector2(545, 147), new Vector2(646, 253));
    var (bottomRightCenter, bottomRightSize)
        = GetCenterAndSize_(new Vector2(365, 236), new Vector2(519, 387));

    var backgroundFlowersObj = area.AddObject();
    backgroundFlowersObj.AddComponent(_ => {
      GlTransform.PushMatrix();

      // TODO: Set color
      // TODO: Set alpha

      // Top left
      GlTransform.Set(GetMergedMatrix_(topLeftCenter, topLeftSize));
      backgroundFlowerModelRenderer.Render();

      // Middle left
      GlTransform.Set(GetMergedMatrix_(middleLeftCenter, middleLeftSize));
      backgroundFlowerModelRenderer.Render();

      // Bottom left
      GlTransform.Set(GetMergedMatrix_(bottomLeftCenter, bottomLeftSize));
      backgroundFlowerModelRenderer.Render();

      // Top right
      GlTransform.Set(GetMergedMatrix_(topRightCenter, topRightSize));
      backgroundFlowerModelRenderer.Render();

      // Middle right
      GlTransform.Set(GetMergedMatrix_(middleRightCenter, middleRightSize));
      backgroundFlowerModelRenderer.Render();

      // Bottom right
      GlTransform.Set(GetMergedMatrix_(bottomRightCenter, bottomRightSize));
      backgroundFlowerModelRenderer.Render();

      GlTransform.PopMatrix();
    });
  }

  private static (Vector2 center, Vector2 size) GetCenterAndSize_(
      Vector2 pt1,
      Vector2 pt2) {
    var center = (pt1 + pt2) / 2;
    var size = (pt2 - pt1) / 2;
    return (center, size);
  }

  private static Matrix4x4 GetMergedMatrix_(Vector2 center, Vector2 size)
    => SystemMatrix4x4Util.FromTrs(
        new Vector3(center - new Vector2(320, 0), 0),
        (Quaternion?) null,
        new Vector3(5 * size, 1));
}