using System.Diagnostics.CodeAnalysis;

using Avalonia.Controls;

using fin.model;
using fin.model.impl;
using fin.shaders.glsl;
using fin.ui.avalonia;

using ReactiveUI;

namespace uni.ui.avalonia.resources.model.materials {
  public class MaterialShadersPanelViewModelForDesigner
      : MaterialShadersPanelViewModel {
    public MaterialShadersPanelViewModelForDesigner() {
      this.ModelAndMaterial = (ModelImpl.CreateForViewer(), null);
    }
  }

  public class MaterialShadersPanelViewModel : ViewModelBase {
    public required (IReadOnlyModel, IReadOnlyMaterial?) ModelAndMaterial {
      get;
      set {
        this.RaiseAndSetIfChanged(ref field, value);

        var (model, material) = field;
        var shaderSource
            = material.ToShaderSource(model,
                                      ModelRequirements.FromModel(model));
        this.VertexShaderSource = shaderSource.VertexShaderSource;
        this.FragmentShaderSource = shaderSource.FragmentShaderSource;
      }
    }

    [NotNullIfNotNull(nameof(ModelAndMaterial))]
    public string? VertexShaderSource {
      get;
      private set => this.RaiseAndSetIfChanged(ref field, value);
    }

    [NotNullIfNotNull(nameof(ModelAndMaterial))]
    public string? FragmentShaderSource {
      get;
      private set => this.RaiseAndSetIfChanged(ref field, value);
    }
  }

  public partial class MaterialShadersPanel : UserControl {
    public MaterialShadersPanel() {
      this.InitializeComponent();
    }
  }
}