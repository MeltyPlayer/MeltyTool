using Avalonia.Controls;

using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.TextMate;

using fin.model;
using fin.model.impl;
using fin.shaders.glsl;

using ReactiveUI;

using uni.ui.avalonia.ViewModels;

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
        var shaderSource = material.ToShaderSource(model, true);
        this.VertexShaderSource
            = new TextDocument(shaderSource.VertexShaderSource);
        this.FragmentShaderSource
            = new TextDocument(shaderSource.FragmentShaderSource);
      }
    }

    public TextDocument VertexShaderSource {
      get;
      private set
        => this.RaiseAndSetIfChanged(ref field, value);
    }

    public TextDocument FragmentShaderSource {
      get;
      private set
        => this.RaiseAndSetIfChanged(ref field, value);
    }
  }

  public partial class MaterialShadersPanel : UserControl {
    public MaterialShadersPanel() {
      this.InitializeComponent();
      this.InitViewer_(this.vertexShaderViewer_);
      this.InitViewer_(this.fragmentShaderViewer_);
    }

    private void InitViewer_(TextEditor textEditor) {
      var registryOptions = new GlslRegistryOptions();
      var textMateInstallation
          = textEditor.InstallTextMate(registryOptions, false);
      textMateInstallation.SetGrammar("source.glsl");

      textEditor.Options = new() {
          AllowScrollBelowDocument = false
      };
    }
  }
}