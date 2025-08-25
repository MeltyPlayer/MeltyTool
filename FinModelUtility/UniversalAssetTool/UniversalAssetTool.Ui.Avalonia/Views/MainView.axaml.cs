using Avalonia.Controls;
using Avalonia.Interactivity;

using uni.config;
using uni.model;

namespace uni.ui.avalonia.Views;

public partial class MainView : UserControl {
  private PanelType activePanelType_ = PanelType.NEITHER;

  private const string FILE_SELECTOR_HOVER_PSEUDOCLASS = ":fileSelectorHover";
  private const string INFO_PANEL_HOVER_PSEUDOCLASS = ":infoPanelHover";

  public enum PanelType {
    NEITHER,
    FILE_SELECTOR,
    INFO_PANEL,
  }

  public MainView() {
    this.InitializeComponent();

    // TODO: Handle config changes, update ShowGrid automatically

    SceneInstanceService.OnSceneInstanceOpened
        += (_, sceneInstance) => {
          this.SceneViewerGlPanel.Scene = sceneInstance;

          if (sceneInstance == null) {
            this.SceneViewerGlPanel.ViewerScale = 1;
          } else {
            this.SceneViewerGlPanel.ViewerScale =
                new ScaleSource(Config.Instance.Viewer.ViewerModelScaleSource)
                    .GetScale(sceneInstance);
          }
        };

    this.RegisterPanel_(this.SceneViewerGlPanel, PanelType.NEITHER);
    this.RegisterPanel_(this.FileSelectorPanel, PanelType.FILE_SELECTOR);
    this.RegisterPanel_(this.InfoPanel, PanelType.INFO_PANEL);
  }

  private void RegisterPanel_(Control panel, PanelType panelType) {
    panel.AddHandler(PointerEnteredEvent,
                     (_, _) => this.PointerEnteredPanel_(panelType));
    panel.AddHandler(PointerExitedEvent,
                     (_, _) => this.PointerExitedPanel_(panelType));
    panel.AddHandler(PointerPressedEvent,
                     (_, _) => this.ClickedPanel_(panelType),
                     RoutingStrategies.Bubble,
                     handledEventsToo: true);
  }

  private void PointerEnteredPanel_(PanelType panelType)
    => this.TrySetPanelPseudoclass_(panelType, true);

  private void PointerExitedPanel_(PanelType panelType) {
    if (this.activePanelType_ != panelType) {
      this.TrySetPanelPseudoclass_(panelType, false);
    }
  }

  private void ClickedPanel_(PanelType panelType) {
    var previousActivePanelType = this.activePanelType_;
    this.activePanelType_ = panelType;

    if (previousActivePanelType != panelType) {
      this.TrySetPanelPseudoclass_(previousActivePanelType, false);
    }
  }

  private void TrySetPanelPseudoclass_(PanelType panelType, bool value) {
    switch (panelType) {
      case PanelType.INFO_PANEL:
        this.PseudoClasses.Set(INFO_PANEL_HOVER_PSEUDOCLASS, value);
        break;
      case PanelType.FILE_SELECTOR:
        this.PseudoClasses.Set(FILE_SELECTOR_HOVER_PSEUDOCLASS, value);
        break;
    }
  }
}