using System.Diagnostics.CodeAnalysis;

using Avalonia.Controls;

using fin.model;
using fin.model.impl;
using fin.shaders.glsl;
using fin.ui;
using fin.ui.avalonia.controls;

using ReactiveUI;

namespace uni.ui.avalonia.resources.model.materials;

public sealed class MaterialShadersPanelViewModelForDesigner
    : MaterialShadersPanelViewModel {
  public MaterialShadersPanelViewModelForDesigner() {
    this.ModelAndMaterial = (ModelImpl.CreateForViewer(), null);
  }
}

public class MaterialShadersPanelViewModel : BViewModel {
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

public partial class MaterialShadersPanel
    : BUserControl<MaterialShadersPanelViewModel> {
  public MaterialShadersPanel() {
    this.InitializeComponent();
  }
}