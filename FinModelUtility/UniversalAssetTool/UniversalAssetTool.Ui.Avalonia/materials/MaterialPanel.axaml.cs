using Avalonia.Controls;

using fin.model;
using fin.model.impl;

using ReactiveUI;

using uni.ui.avalonia.ViewModels;

namespace uni.ui.avalonia.materials {
  public class MaterialPanelViewModelForDesigner
      : MaterialPanelViewModel {
    public MaterialPanelViewModelForDesigner() {
      this.ModelAndMaterial = (new ModelImpl(), null);
    }
  }

  public class MaterialPanelViewModel : ViewModelBase {
    private (IModel, IReadOnlyMaterial) modelAndMaterial_;
    private string materialLabel_;
    private MaterialTexturesPanelViewModel materialTexturesPanelViewModel_;
    private MaterialShadersPanelViewModel materialShadersPanelViewModel_;

    public required (IModel, IReadOnlyMaterial?) ModelAndMaterial {
      get => this.modelAndMaterial_;
      set {
        this.RaiseAndSetIfChanged(ref this.modelAndMaterial_, value);
        this.MaterialLabel = $"Material \"{value.Item2?.Name ?? "(null)"}\"";
        if (this.materialShadersPanelViewModel_ == null) {
          this.MaterialTexturesPanel = new() {
              ModelAndMaterial = value,
          };
          this.MaterialShadersPanel = new() {
              ModelAndMaterial = value,
          };
        } else {
          this.MaterialTexturesPanel.ModelAndMaterial = value;
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