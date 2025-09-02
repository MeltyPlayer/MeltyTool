using System;
using System.Numerics;

using Avalonia.Controls;

using fin.io.web;
using fin.scene;
using fin.scene.instance;
using fin.services;
using fin.ui.rendering;

using marioartist.api;

using MarioArtistTool.backgrounds;
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
      IRenderable? sceneryRenderer = null;

      switch (file?.FileType.ToLower()) {
        case ".tstlt": {
          try {
            var bundle = new TstltModelFileBundle(file);
            var model = new TstltModelLoader().Import(bundle);

            // TODO: Decide which to render based on gender.
            {
              area.BackgroundImage
                  = AssetLoaderUtil.LoadImage("background_pretty.png");
              area.BackgroundImageScale = .3f;
              sceneryRenderer = new PrettySceneryRenderer();
            }

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

      if (sceneryRenderer != null) {
        // Hides the default skybox.
        area.CreateCustomSkyboxObject();

        var backgroundObj = area.AddObject();
        backgroundObj.AddRenderable(sceneryRenderer);
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
}