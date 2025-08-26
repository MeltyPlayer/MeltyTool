using System;

using Avalonia.Controls;

using fin.io.web;
using fin.scene;
using fin.scene.instance;
using fin.services;

using marioartist.api;

using marioartisttool.services;

namespace marioartisttool.Views;

public partial class MainView : UserControl {
  public MainView() {
    InitializeComponent();

    MfsFileSystemService.OnFileSelected += file => {
      switch (file?.FileType.ToLower()) {
        case ".tstlt": {
          try {
            var bundle = new TstltModelFileBundle(file);
            var model = new TstltModelLoader().Import(bundle);

            var scene = new SceneImpl {
                FileBundle = model.FileBundle,
                Files = model.Files
            };
            var area = scene.AddArea();
            var obj = area.AddObject();
            obj.AddSceneModel(model);
            scene.CreateDefaultLighting(obj);

            var sceneInstance = new SceneInstanceImpl(scene);
            this.ViewerGlPanel.Scene = sceneInstance;
          } catch (Exception e) {
            ExceptionService.HandleException(e, new LoadFileException(file));
            this.ViewerGlPanel.Scene = null;
          }

          break;
        }
        default: {
          this.ViewerGlPanel.Scene = null;
          break;
        }
      }
    };
  }
}