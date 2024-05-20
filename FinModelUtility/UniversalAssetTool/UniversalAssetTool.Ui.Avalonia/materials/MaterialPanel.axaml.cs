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
    private MaterialShaderTabsViewModel materialShaderTabsViewModel_;

    public required (IModel, IReadOnlyMaterial?) ModelAndMaterial {
      get => this.modelAndMaterial_;
      set {
        this.RaiseAndSetIfChanged(ref this.modelAndMaterial_, value);
        this.MaterialLabel = $"Material \"{value.Item2?.Name ?? "(null)"}\"";
        if (this.materialShaderTabsViewModel_ == null) {
          this.materialShaderTabsViewModel_ = new() {
              ModelAndMaterial = value,
          };
        } else {
          this.materialShaderTabsViewModel_.ModelAndMaterial = value;
        }
      }
    }

    public string MaterialLabel {
      get => this.materialLabel_;
      set => this.RaiseAndSetIfChanged(ref this.materialLabel_, value);
    }

    public MaterialShaderTabsViewModel MaterialShaderTabs {
      get => this.materialShaderTabsViewModel_;
      set => this.RaiseAndSetIfChanged(ref this.materialShaderTabsViewModel_,
                                       value);
    }
  }

  public partial class MaterialPanel : UserControl {
    public MaterialPanel() {
      InitializeComponent();
    }
  }
}