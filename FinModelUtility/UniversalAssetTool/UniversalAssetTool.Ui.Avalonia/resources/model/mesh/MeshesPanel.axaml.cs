using fin.model;
using fin.ui;
using fin.ui.avalonia.controls;
using fin.ui.rendering;

using ReactiveUI;


namespace uni.ui.avalonia.resources.model.mesh;

public sealed class MeshesPanelViewModelForDesigner : MeshesPanelViewModel {
  public MeshesPanelViewModelForDesigner() {
    this.Model = ModelDesignerUtil.CreateStubModel();
  }
}

public class MeshesPanelViewModel : BViewModel, ISelectablePanelViewModel {
  public required IReadOnlyModel? Model {
    set {
      this.Impl = value != null
          ? FullHierarchyTreeViewModel.FromModel(
              value,
              FullHierarchyTreeType.MESHES | FullHierarchyTreeType.PRIMITIVES)
          : null;

      this.Impl?.ExpandCollapse(FullHierarchyTreeType.MESHES |
                                FullHierarchyTreeType.PRIMITIVES);
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

  public SelectableType Type => SelectableType.MESH;

  public void ReselectSelectedElementViaService()
    => this.Impl.SelectElementViaService();
}

public partial class MeshesPanel : BUserControl<MeshesPanelViewModel> {
  public MeshesPanel() {
    this.InitializeComponent();
  }
}