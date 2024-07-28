using System;
using System.Collections.Generic;
using System.Linq;

using Avalonia;
using Avalonia.Controls;

using fin.math;
using fin.model;
using fin.util.asserts;

using ReactiveUI;

using uni.ui.avalonia.common;
using uni.ui.avalonia.resources.model;
using uni.ui.avalonia.ViewModels;

namespace uni.ui.avalonia.resources.texture {
  public class NullTexturesPanelViewModelForDesigner
      : TexturesPanelViewModel;

  public class EmptyTexturesPanelViewModelForDesigner
      : TexturesPanelViewModel {
    public EmptyTexturesPanelViewModelForDesigner() {
      this.ModelAndTextures = (ModelDesignerUtil.CreateStubModel(),
                               Array.Empty<IReadOnlyTexture>());
    }
  }

  public class PopulatedTexturesPanelViewModelForDesigner
      : TexturesPanelViewModel {
    public PopulatedTexturesPanelViewModelForDesigner() {
      var (model, material) = ModelDesignerUtil.CreateStubModelAndMaterial();
      this.ModelAndTextures = (model, material.Textures.ToArray());
    }
  }

  public class KeyValuePairViewModel(string key, string? value)
      : ViewModelBase {
    public string Key => key;
    public string? Value => value;

    public static implicit operator KeyValuePairViewModel(
        (string key, object? value) tuple)
      => new(tuple.key, tuple.value?.ToString());
  }

  public class TexturesPanelViewModel : ViewModelBase {
    private (IReadOnlyModel, IReadOnlyList<IReadOnlyTexture>)?
        modelAndTextures_;
    private TextureListViewModel textureListViewModel_;
    private TextureViewModel? selectedTextureViewModel_;
    private TexturePreviewViewModel? selectedTexturePreviewViewModel_;

    private KeyValueGridViewModel selectedTextureKeyValueGrid_;

    public (IReadOnlyModel, IReadOnlyList<IReadOnlyTexture>)? ModelAndTextures {
      get => this.modelAndTextures_;
      set {
        this.RaiseAndSetIfChanged(ref this.modelAndTextures_, value);
        this.TextureList = new TextureListViewModel
            { ModelAndTextures = value };
      }
    }

    public TextureListViewModel TextureList {
      get => this.textureListViewModel_;
      private set
        => this.RaiseAndSetIfChanged(ref this.textureListViewModel_, value);
    }

    public TextureViewModel? SelectedTexture {
      get => this.selectedTextureViewModel_;
      set {
        this.RaiseAndSetIfChanged(ref this.selectedTextureViewModel_,
                                  value);
        this.SelectedTexturePreview = value != null
            ? new TexturePreviewViewModel {
                Texture = value.Texture,
                ImageMargin = new Thickness(5),
            }
            : null;

        var texture = value?.Texture;
        this.SelectedTextureKeyValueGrid = new KeyValueGridViewModel {
            KeyValuePairs = [
                ("Pixel format", texture?.Image.PixelFormat),
                ("Transparency type", texture?.TransparencyType),
                ("Width", texture?.Image.Width),
                ("Height", texture?.Image.Height),
                ("Horizontal wrapping", texture?.WrapModeU),
                ("Vertical wrapping", texture?.WrapModeV),
                ("Horizontal clamp range", texture?.ClampS),
                ("Vertical clamp range", texture?.ClampT),
                ("Min filter", texture?.MinFilter),
                ("Mag filter", texture?.MagFilter),
                ("UV index", texture?.UvIndex),
                ("UV type", texture?.UvType),
                ("UV center",
                 texture?.IsTransform3d ?? false
                     ? texture.Center
                     : texture?.Center?.Xy()),
                ("UV translation",
                 texture?.IsTransform3d ?? false
                     ? texture.Translation
                     : texture?.Translation?.Xy()),
                ("UV rotation (radians)",
                 texture?.IsTransform3d ?? false
                     ? texture.RotationRadians
                     : texture?.RotationRadians?.X),
                ("UV scale",
                 texture?.IsTransform3d ?? false
                     ? texture.Scale
                     : texture?.Scale?.Xy()),
            ]
        };
      }
    }

    public TexturePreviewViewModel? SelectedTexturePreview {
      get => this.selectedTexturePreviewViewModel_;
      private set => this.RaiseAndSetIfChanged(
          ref this.selectedTexturePreviewViewModel_,
          value);
    }

    public KeyValueGridViewModel SelectedTextureKeyValueGrid {
      get => this.selectedTextureKeyValueGrid_;
      private set
        => this.RaiseAndSetIfChanged(ref this.selectedTextureKeyValueGrid_,
                                     value);
    }
  }

  public partial class TexturesPanel : UserControl {
    public TexturesPanel() {
      this.InitializeComponent();
    }

    protected TexturesPanelViewModel ViewModel
      => Asserts.AsA<TexturesPanelViewModel>(this.DataContext);

    protected void TextureList_OnTextureSelected(
        object? sender,
        TextureSelectedEventArgs e) {
      this.ViewModel.SelectedTexture = e.Texture;
    }
  }
}