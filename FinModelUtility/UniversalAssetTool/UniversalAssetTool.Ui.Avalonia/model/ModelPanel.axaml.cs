using Avalonia.Controls;

using fin.model;

using ReactiveUI;

using uni.ui.avalonia.model.materials;
using uni.ui.avalonia.textures;
using uni.ui.avalonia.ViewModels;

namespace uni.ui.avalonia.model {
  public class ModelPanelViewModelForDesigner : ModelPanelViewModel {
    public ModelPanelViewModelForDesigner() {
      var (model, _) = MaterialDesignerUtil.CreateStubModelAndMaterial();
      this.Model = model;
    }
  }

  public class ModelPanelViewModel : ViewModelBase {
    private IReadOnlyModel model_;
    private MaterialsPanelViewModel materialsPanel_;
    private TexturesPanelViewModel texturesPanel_;

    public IReadOnlyModel Model {
      get => this.model_;
      set {
        this.RaiseAndSetIfChanged(ref this.model_, value);
        this.MaterialsPanel = new MaterialsPanelViewModel {
            ModelAndMaterials = (value, value.MaterialManager.All)
        };
        this.TexturesPanel = new TexturesPanelViewModel {
            Textures = value.MaterialManager.Textures,
        };
      }
    }

    public MaterialsPanelViewModel? MaterialsPanel {
      get => this.materialsPanel_;
      private set => this.RaiseAndSetIfChanged(ref this.materialsPanel_, value);
    }

    public TexturesPanelViewModel? TexturesPanel {
      get => this.texturesPanel_;
      private set => this.RaiseAndSetIfChanged(ref this.texturesPanel_, value);
    }
  }

  public partial class ModelPanel : UserControl {
    public ModelPanel() {
      InitializeComponent();
    }
  }
}