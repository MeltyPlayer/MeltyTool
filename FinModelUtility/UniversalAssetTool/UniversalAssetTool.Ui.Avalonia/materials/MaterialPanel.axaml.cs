using System.Collections.Generic;
using System.IO;
using System.Reflection;

using Avalonia.Controls;

using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.TextMate;

using fin.model;
using fin.model.impl;
using fin.shaders.glsl;

using TextMateSharp.Grammars;
using TextMateSharp.Internal.Grammars.Reader;
using TextMateSharp.Internal.Themes.Reader;
using TextMateSharp.Internal.Types;
using TextMateSharp.Registry;
using TextMateSharp.Themes;

using uni.ui.avalonia.ViewModels;

namespace uni.ui.avalonia.materials {
  public class MaterialPanelViewModel : ViewModelBase {
    public MaterialPanelViewModel(
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

  public class MaterialPanelViewModelForDesigner()
      : MaterialPanelViewModel(new ModelImpl());

  public partial class MaterialPanel : UserControl {
    public MaterialPanel() {
      InitializeComponent();
      this.DataContext = new MaterialPanelViewModelForDesigner();

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