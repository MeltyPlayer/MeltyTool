using System;
using System.Linq;
using System.Numerics;

using Avalonia.Controls;
using Avalonia.Interactivity;

using fin.io.web;
using fin.model.io;
using fin.scene;
using fin.scene.components;
using fin.scene.instance;
using fin.services;
using fin.ui.rendering.gl.scene;
using fin.ui.rendering.viewer;
using fin.util.asserts;
using fin.util.tasks;

using marioartist.api;

using marioartisttool.backgrounds;
using marioartisttool.config;
using marioartisttool.file_select;
using marioartisttool.services;
using marioartisttool.view;
using marioartisttool.view.games.ball;
using marioartisttool.ViewModels;

using static marioartisttool.file_select.FileSelectTopBar;

namespace marioartisttool.Views;

public partial class MainView : UserControl {
  private (Vector3 translation, float pitchDegrees, float yawDegrees)?
      ma3d1CameraTransform_ = null;

  private IModelFileBundle? currentModelFileBundle_;
  private MfsTreeFile? currentFile_;
  private bool isInBallMode_;

  public MainView() {
    InitializeComponent();

    this.ViewerGlPanel.BackdropRenderer = new PolygonStudioSkyboxRenderer();

    this.DataContextChanged += (_, _) => {
      var mainViewModel = this.DataContext.AssertAsA<MainViewModel>();
      var com = mainViewModel.Com;
      this.ViewerGlPanel.PointerPressed += (_, _) => {
        com.IsMouseDown = true;
      };
      this.ViewerGlPanel.PointerReleased += (_, _) => {
        com.IsMouseDown = false;
      };
      this.ViewerGlPanel.Bind(Panel.CursorProperty, com.Cursor);
    };

    MfsFileSystemService.OnFileSelected += file =>
        FinTask.Run(() => this.LoadNextModel_(file));

    EasterEggService.ΔIsInBallMode.Subscribe(isInBallMode => {
      this.isInBallMode_ = isInBallMode;

      if (this.currentFile_?.FileType.ToLower() is ".tstlt") {
        this.LoadNextModel_(this.currentFile_);
      }
    });
  }

  private void LoadNextModel_(MfsTreeFile? file) {
    LoadingStatusService.IsLoading = true;

    this.currentFile_ = file;

    var camera = this.ViewerGlPanel.Camera;
    if (this.currentModelFileBundle_ is Ma3d1ModelFileBundle or null) {
      this.ma3d1CameraTransform_ = (
          camera.Position, camera.PitchDegrees, camera.YawDegrees);
    }

    var scene = new SceneImpl {
        FileBundle = null,
        Files = null
    };

    var area = scene.AddArea();

    var allowMovingCamera = true;
    var showGrid = true;

    IOrthoRenderable? newBackdropRenderer = null;

    switch (file?.FileType.ToLower()) {
      case ".ma3d1": {
        var bundle = new Ma3d1ModelFileBundle(file);
        var model = new Ma3d1ModelLoader().Import(bundle);

        var config = Config.INSTANCE;
        config.MostRecentFileName = file.FullPath;
        config.Save();

        var obj = area.AddRootNode();
        obj.AddSceneModel(model);

        var lightingObj = area.AddRootNode();
        scene.CreateDefaultLighting(lightingObj);

        this.currentModelFileBundle_ = bundle;

        break;
      }
      case ".tstlt": {
        allowMovingCamera = false;
        showGrid = false;

        try {
          var bundle = new TstltModelFileBundle(file);
          var model = TstltModelImporter.Import(bundle, out var gender);

          var config = Config.INSTANCE;
          config.MostRecentFileName = file.FullPath;
          config.Save();

          scene.CreateDefaultLighting(area.AddRootNode(), [model]);

          var modelRenderComponent = new SimpleModelRenderComponent(model);
          var characterObj = area.AddRootNode();

          var modelObj = characterObj.AddChildNode();
          modelObj.AddComponent(modelRenderComponent);

          var shadowPlacementObj = characterObj.AddChildNode();
          shadowPlacementObj.SetPosition(20, -20, -100);
          shadowPlacementObj.SetScale(1, 1, 0);

          var shadowModelObj = shadowPlacementObj.AddChildNode();
          shadowModelObj.AddComponent(
              new ShadowRenderComponent(
                  new LambdaSceneNodeRenderComponent(_
                      => modelRenderComponent
                          .Render(false))));

          if (this.isInBallMode_) {
            var ballObj = characterObj.AddChildNode();
            ballObj.AddComponent(
                new BallGameComponent(
                    modelRenderComponent.SimpleBoneTransformView,
                    model,
                    3));
          } else {
            characterObj.AddComponent(
                new LookAtMouseTickComponent(
                    modelRenderComponent.SimpleBoneTransformView,
                    model.Skeleton
                         .Bones
                         .Single(b => b.Name?.StartsWith(
                                          $"{JointIndex.NECK}:") ??
                                      false),
                    model.Skeleton
                         .Bones
                         .Single(b => b.Name?.StartsWith(
                                          $"{JointIndex.TORSO}:") ??
                                      false)));
            modelObj.AddComponent(new RotateTalentTickComponent());
            shadowModelObj.AddComponent(new RotateTalentTickComponent());
          }

          this.currentModelFileBundle_ = bundle;

          camera.Position = new Vector3(0, -1.5f, .35f);
          camera.PitchDegrees = 0;
          camera.YawDegrees = 90;

          newBackdropRenderer = new TalentStudioBackdropRenderer(gender);
        } catch (Exception e) {
          ExceptionService.HandleException(e, new LoadFileException(file));
          this.ViewerGlPanel.Scene = null;
        }

        break;
      }
    }

    // Need to wait to change camera position or it causes jitter.
    if (file?.FileType.ToLower() != ".tstlt") {
      if (this.ma3d1CameraTransform_ != null) {
        (camera.Position, camera.PitchDegrees, camera.YawDegrees)
            = this.ma3d1CameraTransform_.Value;
      }
    }

    newBackdropRenderer ??= new PolygonStudioSkyboxRenderer();
    this.ViewerGlPanel.BackdropRenderer = newBackdropRenderer;

    var sceneInstance = new SceneInstanceImpl(scene);
    this.ViewerGlPanel.Scene = sceneInstance;

    this.ViewerGlPanel.AllowMovingCamera = allowMovingCamera;
    this.ViewerGlPanel.ShowGrid = showGrid;

    LoadingStatusService.IsLoading = false;
  }
}