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

    this.FileSelectorPanel.PointerEntered
        += (_, _) => this.SetFileSelectorHoverPseudoclass_(true);

    this.FileSelectorPanel.PointerExited
        += (_, _) => this.SetFileSelectorHoverPseudoclass_(false);
  }

  private void SetFileSelectorHoverPseudoclass_(bool value)
    => this.PseudoClasses.Set(":fileSelectorHover", value);
}