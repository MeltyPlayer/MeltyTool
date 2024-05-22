using System.Drawing;

using Avalonia.Controls;

using fin.image;
using fin.model;
using fin.model.impl;

using ReactiveUI;

using uni.ui.avalonia.ViewModels;

namespace uni.ui.avalonia.materials {
  public class MaterialPanelViewModelForDesigner
      : MaterialPanelViewModel {
    public MaterialPanelViewModelForDesigner() {
      var model = new ModelImpl();
      var materialManager = model.MaterialManager;
      var material = materialManager.AddStandardMaterial();

      {
        var diffuseTexture = materialManager.CreateTexture(
            FinImage.Create1x1FromColor(Color.Cyan));
        diffuseTexture.Name = "Diffuse (Cyan)";
        material.DiffuseTexture = diffuseTexture;
      }

      {
        var normalTexture = materialManager.CreateTexture(
            FinImage.Create1x1FromColor(Color.Yellow));
        normalTexture.Name = "Normal (Yellow)";
        material.NormalTexture = normalTexture;
      }

      {
        var aoTexture = materialManager.CreateTexture(
            FinImage.Create1x1FromColor(Color.Magenta));
        aoTexture.Name = "Ambient occlusion (Magenta)";
        material.AmbientOcclusionTexture = aoTexture;
      }

      {
        var emissiveTexture = materialManager.CreateTexture(
            FinImage.Create1x1FromColor(Color.Orange));
        emissiveTexture.Name = "Emissive (Orange)";
        material.EmissiveTexture = emissiveTexture;
      }

      {
        var specularTexture = materialManager.CreateTexture(
            FinImage.Create1x1FromColor(Color.Red));
        specularTexture.Name = "Specular (Red)";
        material.SpecularTexture = specularTexture;
      }

      this.ModelAndMaterial = (model, material);
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

        var (_, material) = value;

        this.MaterialLabel = $"Material \"{material?.Name ?? "(null)"}\"";
        if (this.materialShadersPanelViewModel_ == null) {
          this.MaterialTexturesPanel = new() {
              Material = material,
          };
          this.MaterialShadersPanel = new() {
              ModelAndMaterial = value,
          };
        } else {
          this.MaterialTexturesPanel.Material = material;
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