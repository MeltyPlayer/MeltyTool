using Avalonia.Controls;

using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.TextMate;

using fin.model;
using fin.model.impl;
using fin.shaders.glsl;

using uni.ui.avalonia.ViewModels;

namespace uni.ui.avalonia.materials {
  public class MaterialShaderTabsViewModel : ViewModelBase {
    public MaterialShaderTabsViewModel(
        IReadOnlyModel model,
        IReadOnlyMaterial? material = null) {
      this.Material = material;

      var shaderSource = material.ToShaderSource(model, true);
      this.VertexShaderSource
          = new TextDocument(shaderSource.VertexShaderSource);
      this.FragmentShaderSource
          = new TextDocument(shaderSource.FragmentShaderSource);
    }

    public IReadOnlyMaterial? Material { get; }

    public TextDocument VertexShaderSource { get; }
    public TextDocument FragmentShaderSource { get; }
  }

  public class MaterialShaderTabsViewModelForDesigner()
      : MaterialShaderTabsViewModel(new ModelImpl());

  public partial class MaterialShaderTabs : UserControl {
    public MaterialShaderTabs() {
      InitializeComponent();
      this.DataContext = new MaterialShaderTabsViewModelForDesigner();

      this.InitViewer_(this.vertexShaderViewer_);
      this.InitViewer_(this.fragmentShaderViewer_);
    }

    private void InitViewer_(TextEditor textEditor) {
      var registryOptions = new GlslRegistryOptions();
      var textMateInstallation = textEditor.InstallTextMate(registryOptions);
      textMateInstallation.SetGrammar("source.glsl");
    }
  }
}