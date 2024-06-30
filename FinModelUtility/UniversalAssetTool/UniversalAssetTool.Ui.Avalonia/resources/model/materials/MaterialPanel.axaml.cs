using System.Linq;

using Avalonia.Controls;

using fin.model;

using ReactiveUI;

using uni.ui.avalonia.ViewModels;

namespace uni.ui.avalonia.resources.model.materials {
  public class MaterialPanelViewModelForDesigner
      : MaterialPanelViewModel {
    public MaterialPanelViewModelForDesigner() {
      this.ModelAndMaterial = ModelDesignerUtil.CreateStubModelAndMaterial();
    }
  }

  public class MaterialPanelViewModel : ViewModelBase {
    private (IReadOnlyModel, IReadOnlyMaterial?) modelAndMaterial_;
    private MaterialTexturesPanelViewModel materialTexturesPanelViewModel_;
    private MaterialShadersPanelViewModel materialShadersPanelViewModel_;

    public required (IReadOnlyModel, IReadOnlyMaterial?) ModelAndMaterial {
      get => this.modelAndMaterial_;
      set {
        this.RaiseAndSetIfChanged(ref this.modelAndMaterial_, value);

        var (model, material) = value;
        this.MaterialTexturesPanel = new() {
            ModelAndTextures = material != null
                ? (model, material.Textures.ToArray())
                : null,
        };
        this.MaterialShadersPanel = new() {
            ModelAndMaterial = value,
        };
      }
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