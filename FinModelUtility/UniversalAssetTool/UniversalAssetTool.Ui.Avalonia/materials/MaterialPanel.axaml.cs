using Avalonia.Controls;

using fin.model;

using ReactiveUI;

using uni.ui.avalonia.ViewModels;

namespace uni.ui.avalonia.materials {
  public class MaterialPanelViewModelForDesigner
      : MaterialPanelViewModel {
    public MaterialPanelViewModelForDesigner() {
      this.ModelAndMaterial = MaterialDesignerUtil.CreateStubModelAndMaterial();
    }
  }

  public class MaterialPanelViewModel : ViewModelBase {
    private (IReadOnlyModel, IReadOnlyMaterial) modelAndMaterial_;
    private string materialLabel_;
    private MaterialTexturesPanelViewModel materialTexturesPanelViewModel_;
    private MaterialShadersPanelViewModel materialShadersPanelViewModel_;

    public required (IReadOnlyModel, IReadOnlyMaterial?) ModelAndMaterial {
      get => this.modelAndMaterial_;
      set {
        this.RaiseAndSetIfChanged(ref this.modelAndMaterial_, value);

        var (_, material) = value;

        this.MaterialLabel = $"Material \"{material?.Name ?? "(null)"}\"";
        if (this.materialShadersPanelViewModel_ == null) {
          this.MaterialTexturesPanel = new() {
              Material = material,
          };
          this.MaterialShadersPanel = new() {
              ModelAndMaterial = value,
          };
        } else {
          this.MaterialTexturesPanel.Material = material;
          this.MaterialShadersPanel.ModelAndMaterial = value;
        }
      }
    }

    public string MaterialLabel {
      get => this.materialLabel_;
      set => this.RaiseAndSetIfChanged(ref this.materialLabel_, value);
    }

    public MaterialTexturesPanelViewModel MaterialTexturesPanel {
      get => this.materialTexturesPanelViewModel_;
      set => this.RaiseAndSetIfChanged(ref this.materialTexturesPanelViewModel_,
                                       value);
    }

    public MaterialShadersPanelViewModel MaterialShadersPanel {
      get => this.materialShadersPanelViewModel_;
      set => this.RaiseAndSetIfChanged(ref this.materialShadersPanelViewModel_,
                                       value);
    }
  }

  public partial class MaterialPanel : UserControl {
    public MaterialPanel() {
      InitializeComponent();
    }
  }
}