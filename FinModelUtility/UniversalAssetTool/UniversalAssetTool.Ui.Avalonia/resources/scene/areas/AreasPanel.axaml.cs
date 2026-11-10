using Avalonia.Controls;

using fin.scene;
using fin.ui;
using fin.ui.rendering;

using ReactiveUI;

namespace uni.ui.avalonia.resources.scene.areas;

public sealed class AreasPanelViewModelForDesigner
    : AreasPanelViewModel {
  public AreasPanelViewModelForDesigner() {
    this.Scene = SceneDesignerUtil.CreateStubScene();
  }
}

public class AreasPanelViewModel : BViewModel, ISelectablePanelViewModel {
  public required IReadOnlyScene? Scene {
    set {
      this.Impl = value != null
          ? FullHierarchyTreeViewModel.FromScene(
              value,
              FullHierarchyTreeType.AREAS | FullHierarchyTreeType.NODES)
          : null;

      this.Impl?.ExpandCollapse(FullHierarchyTreeType.AREAS |
                                FullHierarchyTreeType.NODES);
    }
  }

  public FullHierarchyTreeViewModel? Impl {
    get;
    set {
      this.RaiseAndSetIfChanged(ref field, value);
      this.IsPopulated = (value?.Source.Rows.Count ?? 0) > 0;
    }
  }

  public bool IsPopulated {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
  }

  public SelectableType Type => SelectableType.NODE;

  public void ReselectSelectedElementViaService()
    => this.Impl?.SelectElementViaService();
}

public partial class AreasPanel : UserControl {
  public AreasPanel() {
    this.InitializeComponent();
  }
}