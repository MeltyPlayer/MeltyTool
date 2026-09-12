using fin.model;
using fin.ui;
using fin.ui.avalonia.controls;

using ReactiveUI;

namespace uni.ui.avalonia.resources.model.skeleton;

public sealed class SkeletonPanelViewModelForDesigner
    : SkeletonPanelViewModel {
  public SkeletonPanelViewModelForDesigner() {
    this.Model = ModelDesignerUtil.CreateStubModel();
  }
}

public class SkeletonPanelViewModel : BViewModel {
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
}

public partial class SkeletonPanel : BUserControl<SkeletonPanelViewModel> {
  public SkeletonPanel() {
    this.InitializeComponent();
  }
}