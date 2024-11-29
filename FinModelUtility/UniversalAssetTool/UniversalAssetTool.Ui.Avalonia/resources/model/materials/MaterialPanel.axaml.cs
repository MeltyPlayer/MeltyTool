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
    public required (IReadOnlyModel, IReadOnlyMaterial?) ModelAndMaterial {
      get;
      set {
        this.RaiseAndSetIfChanged(ref field, value);

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
      get;
      set => this.RaiseAndSetIfChanged(ref field,
                                       value);
    }

    public MaterialShadersPanelViewModel MaterialShadersPanel {
      get;
      set => this.RaiseAndSetIfChanged(ref field,
                                       value);
    }
  }

  public partial class MaterialPanel : UserControl {
    public MaterialPanel() {
      this.InitializeComponent();
    }
  }
}