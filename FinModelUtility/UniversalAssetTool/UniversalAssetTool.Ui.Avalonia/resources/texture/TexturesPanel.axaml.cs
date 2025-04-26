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
    public (IReadOnlyModel, IReadOnlyList<IReadOnlyTexture>)? ModelAndTextures {
      get;
      set {
        this.RaiseAndSetIfChanged(ref field, value);
        this.TextureList = new TextureListViewModel
            { ModelAndTextures = value };
      }
    }

    public TextureListViewModel TextureList {
      get;
      private set {
        this.RaiseAndSetIfChanged(ref field, value);
        this.SelectedTexture = value.TextureViewModels.FirstOrDefault();
      }
    }

    public TextureViewModel? SelectedTexture {
      get;
      set {
        this.RaiseAndSetIfChanged(ref field,
                                  value);
        this.SelectedTexturePreview = value != null
            ? new TexturePreviewViewModel {
                Texture = value.Texture,
                ImageMargin = new Thickness(5),
            }
            : null;

        var texture = value?.Texture;
        var transform = texture?.TextureTransform;
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
                 transform?.IsTransform3d ?? false
                     ? transform.Center
                     : transform?.Center?.Xy()),
                ("UV translation",
                 transform?.IsTransform3d ?? false
                     ? transform.Translation
                     : transform?.Translation?.Xy()),
                ("UV rotation (radians)",
                 transform?.IsTransform3d ?? false
                     ? transform.RotationRadians
                     : transform?.RotationRadians?.X),
                ("UV scale",
                 transform?.IsTransform3d ?? false
                     ? transform.Scale
                     : transform?.Scale?.Xy()),
            ]
        };
      }
    }

    public TexturePreviewViewModel? SelectedTexturePreview {
      get;
      private set => this.RaiseAndSetIfChanged(
          ref field,
          value);
    }

    public KeyValueGridViewModel SelectedTextureKeyValueGrid {
      get;
      private set
        => this.RaiseAndSetIfChanged(ref field,
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