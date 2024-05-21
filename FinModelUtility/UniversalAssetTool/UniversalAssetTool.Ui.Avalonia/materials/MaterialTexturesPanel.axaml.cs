using Avalonia.Controls;

using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.TextMate;

using fin.model;
using fin.model.impl;
using fin.shaders.glsl;

using ReactiveUI;

using uni.ui.avalonia.ViewModels;

namespace uni.ui.avalonia.materials {
  public class MaterialTexturesPanelViewModelForDesigner
      : MaterialTexturesPanelViewModel {
    public MaterialTexturesPanelViewModelForDesigner() {
      this.ModelAndMaterial = (new ModelImpl(), null);
    }
  }

  public class MaterialTexturesPanelViewModel : ViewModelBase {
    private (IModel, IReadOnlyMaterial) modelAndMaterial_;

    public required (IModel, IReadOnlyMaterial?) ModelAndMaterial {
      get => this.modelAndMaterial_;
      set {
        this.RaiseAndSetIfChanged(ref this.modelAndMaterial_, value);

        var (model, material) = this.modelAndMaterial_;
      }
    }
  }

  public partial class MaterialTexturesPanel : UserControl {
    public MaterialTexturesPanel() {
      InitializeComponent();
    }
  }
}