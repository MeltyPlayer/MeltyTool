using Avalonia.Controls;

using fin.scene.instance;

namespace uni.ui.avalonia.Views;

public partial class MainView : UserControl {
  public MainView() {
    InitializeComponent();

    SceneService.OnSceneOpened
        += (fileTreeLeafNode, scene) => {
             this.SceneViewerGlPanel.Scene = new SceneInstanceImpl(scene);
           };
  }
}