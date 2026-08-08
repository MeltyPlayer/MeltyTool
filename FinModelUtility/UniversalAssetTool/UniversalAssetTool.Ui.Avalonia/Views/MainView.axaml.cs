using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;

using Avalonia.Controls;

using fin.ui.avalonia.controls;

using uni.config;
using uni.model;
using uni.services;
using uni.ui.avalonia.ViewModels;

namespace uni.ui.avalonia.Views;

public partial class MainView : BUserControl<MainViewModel> {
  private BehaviorSubject<bool> inWindowSubject_ = new(true);
  private BehaviorSubject<PanelType> activePanelTypeSubject_
      = new(PanelType.NEITHER);

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

    Observable
        .CombineLatest(this.activePanelTypeSubject_,
                       this.inWindowSubject_,
                       (activePanelType, inWindow)
                           => (activePanelType, inWindow))
        .Subscribe(tuple => {
          var (activePanelType, inWindow) = tuple;
          if (inWindow) {
            this.TrySetPanelPseudoclass_(PanelType.FILE_SELECTOR,
                                         activePanelType == PanelType.FILE_SELECTOR);
            this.TrySetPanelPseudoclass_(PanelType.INFO_PANEL,
                                         activePanelType == PanelType.INFO_PANEL);
          }
        });

    this.AddHandler(PointerEnteredEvent, (_, _) => this.inWindowSubject_.OnNext(true));
    this.AddHandler(PointerExitedEvent, (_, _) => this.inWindowSubject_.OnNext(false));

    this.RegisterPanel_(this.SceneViewerGlPanel, PanelType.NEITHER);
    this.RegisterPanel_(this.FileSelectorPanel, PanelType.FILE_SELECTOR);
    this.RegisterPanel_(this.InfoPanel, PanelType.INFO_PANEL);
  }

  private void RegisterPanel_(Control panel, PanelType panelType) {
    panel.AddHandler(PointerEnteredEvent,
                     (_, _) => this.activePanelTypeSubject_.OnNext(panelType));
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