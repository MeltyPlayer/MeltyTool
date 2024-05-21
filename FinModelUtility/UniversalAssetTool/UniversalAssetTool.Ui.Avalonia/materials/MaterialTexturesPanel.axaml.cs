using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;

using Avalonia.Controls;

using DynamicData;
using DynamicData.Binding;

using fin.image;
using fin.model;
using fin.model.impl;

using ReactiveUI;

using uni.ui.avalonia.resources;
using uni.ui.avalonia.ViewModels;

using Bitmap = Avalonia.Media.Imaging.Bitmap;

namespace uni.ui.avalonia.materials {
  public class MaterialTexturesPanelViewModelForDesigner
      : MaterialTexturesPanelViewModel {
    public MaterialTexturesPanelViewModelForDesigner() {
      var model = new ModelImpl();
      var materialManager = model.MaterialManager;
      var material = materialManager.AddStandardMaterial();

      var diffuseTexture = materialManager.CreateTexture(
          FinImage.Create1x1FromColor(Color.Cyan));
      diffuseTexture.Name = "Diffuse (Cyan)";
      material.DiffuseTexture = diffuseTexture;

      var normalTexture = materialManager.CreateTexture(
          FinImage.Create1x1FromColor(Color.Yellow));
      normalTexture.Name = "Normal (Yellow)";
      material.NormalTexture = normalTexture;

      this.Material = material;
    }
  }

  public class MaterialTexturesPanelViewModel : ViewModelBase {
    private IReadOnlyMaterial? material_;
    private ObservableCollection<TextureViewModel> textureViewModels_;

    public required IReadOnlyMaterial? Material {
      get => this.material_;
      set {
        this.RaiseAndSetIfChanged(ref this.material_, value);
        this.Textures = new ObservableCollection<TextureViewModel>(
            this.material_?.Textures
                .Select(texture => new TextureViewModel()
                            { Texture = texture }) ??
            Enumerable.Empty<TextureViewModel>());
      }
    }

    public ObservableCollection<TextureViewModel>? Textures {
      get => this.textureViewModels_;
      private set
        => this.RaiseAndSetIfChanged(ref this.textureViewModels_, value);
    }
  }

  public class TextureViewModel : ViewModelBase {
    private IReadOnlyTexture texture_;
    private Bitmap image_;

    public required IReadOnlyTexture Texture {
      get => this.texture_;
      set {
        this.RaiseAndSetIfChanged(ref this.texture_, value);
        this.Image = this.texture_.AsAvaloniaImage();
      }
    }

    public Bitmap Image {
      get => this.image_;
      private set => this.RaiseAndSetIfChanged(ref this.image_, value);
    }
  }

  public partial class MaterialTexturesPanel : UserControl {
    public MaterialTexturesPanel() {
      InitializeComponent();
    }
  }
}