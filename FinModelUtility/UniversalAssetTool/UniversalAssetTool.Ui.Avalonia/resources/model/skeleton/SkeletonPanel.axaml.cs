using fin.model;
using fin.ui;
using fin.ui.avalonia.controls;
using fin.ui.rendering;

using ReactiveUI;

namespace uni.ui.avalonia.resources.model.skeleton;

public sealed class SkeletonPanelViewModelForDesigner
    : SkeletonPanelViewModel {
  public SkeletonPanelViewModelForDesigner() {
    this.Model = ModelDesignerUtil.CreateStubModel();
  }
}

public class SkeletonPanelViewModel : BViewModel, ISelectablePanelViewModel {
  public required IReadOnlyModel? Model {
    set {
      this.Impl = value != null
          ? FullHierarchyTreeViewModel.FromModel(
              value,
              FullHierarchyTreeType.BONES)
          : null;

      this.Impl?.ExpandCollapse(FullHierarchyTreeType.BONES);
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

  public SelectableType Type => SelectableType.BONE;

  public void ReselectSelectedElementViaService()
    => this.Impl?.SelectElementViaService();
}

public partial class SkeletonPanel : BUserControl<SkeletonPanelViewModel> {
  public SkeletonPanel() {
    this.InitializeComponent();
  }
}