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
  public class MaterialShadersPanelViewModelForDesigner
      : MaterialShadersPanelViewModel {
    public MaterialShadersPanelViewModelForDesigner() {
      this.ModelAndMaterial = (new ModelImpl(), null);
    }
  }

  public class MaterialShadersPanelViewModel : ViewModelBase {
    private (IReadOnlyModel, IReadOnlyMaterial?) modelAndMaterial_;
    private TextDocument vertexShaderSource_;
    private TextDocument fragmentShaderSource_;

    public required (IReadOnlyModel, IReadOnlyMaterial?) ModelAndMaterial {
      get => this.modelAndMaterial_;
      set {
        this.RaiseAndSetIfChanged(ref this.modelAndMaterial_, value);

        var (model, material) = this.modelAndMaterial_;
        var shaderSource = material.ToShaderSource(model, true);
        this.VertexShaderSource
            = new TextDocument(shaderSource.VertexShaderSource);
        this.FragmentShaderSource
            = new TextDocument(shaderSource.FragmentShaderSource);
      }
    }

    public TextDocument VertexShaderSource {
      get => this.vertexShaderSource_;
      private set
        => this.RaiseAndSetIfChanged(ref this.vertexShaderSource_, value);
    }

    public TextDocument FragmentShaderSource {
      get => this.fragmentShaderSource_;
      private set
        => this.RaiseAndSetIfChanged(ref this.fragmentShaderSource_, value);
    }
  }

  public partial class MaterialShadersPanel : UserControl {
    public MaterialShadersPanel() {
      InitializeComponent();
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