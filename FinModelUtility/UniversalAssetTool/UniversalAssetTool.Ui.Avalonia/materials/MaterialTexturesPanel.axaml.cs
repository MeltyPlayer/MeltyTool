using System.Collections.ObjectModel;
using System.Linq;

using Avalonia;
using Avalonia.Controls;

using fin.model;

using ReactiveUI;

using uni.ui.avalonia.resources;
using uni.ui.avalonia.textures;
using uni.ui.avalonia.ViewModels;

namespace uni.ui.avalonia.materials {
  public class MaterialTexturesPanelViewModelForDesigner
      : MaterialTexturesPanelViewModel {
    public MaterialTexturesPanelViewModelForDesigner() {
      this.Material = MaterialDesignerUtil.CreateStubMaterial();
    }
  }

  public class MaterialTexturesPanelViewModel : ViewModelBase {
    private IReadOnlyMaterial? material_;
    private TextureViewModel selectedTextureViewModel_;
    private TexturePreviewViewModel selectedTexturePreviewViewModel_;
    private string? selectedTextureCaption_;
    private ObservableCollection<TextureViewModel> textureViewModels_;

    public required IReadOnlyMaterial? Material {
      get => this.material_;
      set {
        this.RaiseAndSetIfChanged(ref this.material_, value);

        var textures = this.material_?.Textures;
        this.Textures = new ObservableCollection<TextureViewModel>(
            textures?.Select(texture => new TextureViewModel
                                 { Texture = texture }) ??
            Enumerable.Empty<TextureViewModel>());

        this.SelectedTexture = this.Textures.FirstOrDefault();
      }
    }

    public ObservableCollection<TextureViewModel> Textures {
      get => this.textureViewModels_;
      private set
        => this.RaiseAndSetIfChanged(ref this.textureViewModels_, value);
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
        this.SelectedTextureCaption = value?.Caption;
      }
    }

    public TexturePreviewViewModel? SelectedTexturePreview {
      get => this.selectedTexturePreviewViewModel_;
      private set => this.RaiseAndSetIfChanged(
          ref this.selectedTexturePreviewViewModel_,
          value);
    }

    public string? SelectedTextureCaption {
      get => this.selectedTextureCaption_;
      private set
        => this.RaiseAndSetIfChanged(ref this.selectedTextureCaption_, value);
    }
  }

  public class TextureViewModel : ViewModelBase {
    private IReadOnlyTexture texture_;
    private string caption_;
    public TexturePreviewViewModel texturePreviewViewModel_;

    public required IReadOnlyTexture Texture {
      get => this.texture_;
      set {
        this.RaiseAndSetIfChanged(ref this.texture_, value);

        this.TexturePreview = new TexturePreviewViewModel { Texture = value };

        var image = value.Image;
        this.Caption = $"{image.PixelFormat}, {image.Width}x{image.Height}";
      }
    }

    public TexturePreviewViewModel TexturePreview {
      get => this.texturePreviewViewModel_;
      private set => this.RaiseAndSetIfChanged(
          ref this.texturePreviewViewModel_,
          value);
    }

    public string Caption {
      get => this.caption_;
      private set => this.RaiseAndSetIfChanged(ref this.caption_, value);
    }
  }

  public partial class MaterialTexturesPanel : UserControl {
    public MaterialTexturesPanel() {
      InitializeComponent();
    }
  }
}