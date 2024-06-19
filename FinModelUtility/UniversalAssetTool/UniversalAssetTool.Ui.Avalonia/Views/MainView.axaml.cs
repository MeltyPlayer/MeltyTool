using Avalonia.Controls;

namespace uni.ui.avalonia.Views;

public partial class MainView : UserControl {
  public MainView() {
    InitializeComponent();

    SceneInstanceService.OnSceneInstanceOpened
        += (_, sceneInstance) => {
             this.SceneViewerGlPanel.Scene = sceneInstance;
           };

    this.FileSelectorPanel.PointerEntered
        += (_, _) => this.SetFileSelectorHoverPseudoclass_(true);

    this.FileSelectorPanel.PointerExited
        += (_, _) => this.SetFileSelectorHoverPseudoclass_(false);

    this.InfoPanel.PointerEntered
        += (_, _) => this.SetInfoPanelHoverPseudoclass_(true);

    this.InfoPanel.PointerExited
        += (_, _) => this.SetInfoPanelHoverPseudoclass_(false);
  }

  private void SetFileSelectorHoverPseudoclass_(bool value)
    => this.PseudoClasses.Set(":fileSelectorHover", value);

  private void SetInfoPanelHoverPseudoclass_(bool value)
    => this.PseudoClasses.Set(":infoPanelHover", value);
}