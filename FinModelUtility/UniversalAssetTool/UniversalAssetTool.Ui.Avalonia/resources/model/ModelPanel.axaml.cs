using Avalonia.Controls;

using fin.animation;
using fin.data.dictionaries;
using fin.model;
using fin.ui;
using fin.ui.avalonia.controls;
using fin.ui.rendering;

using ReactiveUI;

using uni.ui.avalonia.resources.animation;
using uni.ui.avalonia.resources.registers;
using uni.ui.avalonia.resources.model.materials;
using uni.ui.avalonia.resources.model.mesh;
using uni.ui.avalonia.resources.model.skeleton;
using uni.ui.avalonia.resources.texture;

namespace uni.ui.avalonia.resources.model;

public sealed class ModelPanelViewModelForDesigner : ModelPanelViewModel {
  public ModelPanelViewModelForDesigner() {
    this.Model = ModelDesignerUtil.CreateStubModel();
  }
}

public class ModelPanelViewModel : BViewModel {
  public IReadOnlyModel Model {
    get;
    set {
      this.RaiseAndSetIfChanged(ref field, value);
      this.AnimationsPanel = new AnimationsPanelViewModel {
          Animations = value.AnimationManager.Animations,
          AnimationPlaybackManager = new FrameAdvancer {
              LoopPlayback = true,
          }
      };
      this.FilesPanel = new FilesPanelViewModel(value);
      this.MaterialsPanel = new MaterialsPanelViewModel {
          ModelsAndMaterials
              = ListDictionary.From(value, value.MaterialManager.All)
      };
      this.MeshesPanel = new MeshesPanelViewModel { Model = value };
      this.RegistersPanel = new RegistersPanelViewModel() {
          Registers = value.MaterialManager.Registers,
      };
      this.SkeletonPanel = new SkeletonPanelViewModel { Model = value };
      this.TexturesPanel = new TexturesPanelViewModel {
          ModelsAndTextures
              = ListDictionary.From(value, value.MaterialManager.Textures)
      };
    }
  }

  public AnimationsPanelViewModel AnimationsPanel {
    get;
    private set
      => this.RaiseAndSetIfChanged(ref field, value);
  }

  public FilesPanelViewModel FilesPanel {
    get;
    private set => this.RaiseAndSetIfChanged(ref field, value);
  }

  public MaterialsPanelViewModel MaterialsPanel {
    get;
    private set => this.RaiseAndSetIfChanged(ref field, value);
  }

  public MeshesPanelViewModel MeshesPanel {
    get;
    private set => this.RaiseAndSetIfChanged(ref field, value);
  }

  public RegistersPanelViewModel RegistersPanel {
    get;
    private set => this.RaiseAndSetIfChanged(ref field, value);
  }

  public SkeletonPanelViewModel SkeletonPanel {
    get;
    private set => this.RaiseAndSetIfChanged(ref field, value);
  }

  public TexturesPanelViewModel TexturesPanel {
    get;
    private set => this.RaiseAndSetIfChanged(ref field, value);
  }
}

public partial class ModelPanel : BUserControl<ModelPanelViewModel> {
  public ModelPanel() {
    this.InitializeComponent();
  }

  private void ClearSelectedItemsWhenTabChanged_(
      object? sender,
      SelectionChangedEventArgs e) {
    if (e.Source != this.ModelTabs) {
      return;
    }

    ISelectablePanelViewModel? selectedPanel = null;
    var viewModel = this.ViewModel;

    if (e.AddedItems.Count > 0) {
      if (e.AddedItems[0] is TabItem item) {
        var header = item.Header;

        if (header == this.SkeletonTabHeader) {
          selectedPanel = viewModel.SkeletonPanel;
        }

        if (header == this.MaterialsTabHeader) {
          selectedPanel = viewModel.MaterialsPanel;
        }


        if (header == this.MeshesTabHeader) {
          selectedPanel = viewModel.MeshesPanel;
        }

        if (header == this.TexturesTabHeader) {
          selectedPanel = viewModel.TexturesPanel;
        }
      }
    }

    SelectedServiceUtils.SelectableType
        = selectedPanel?.Type ?? SelectableType.NONE;
    selectedPanel?.ReselectSelectedElementViaService();
  }
}