using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Avalonia;
using Avalonia.Controls;

using fin.model;
using fin.util.asserts;

using ReactiveUI;

using uni.ui.avalonia.model.materials;
using uni.ui.avalonia.ViewModels;

namespace uni.ui.avalonia.textures {
  public class TexturesPanelViewModelForDesigner
      : TexturesPanelViewModel {
    public TexturesPanelViewModelForDesigner() {
      this.Textures
          = MaterialDesignerUtil.CreateStubMaterial().Textures.ToArray();
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
    private IReadOnlyList<IReadOnlyTexture>? textures_;
    private TextureListViewModel textureListViewModel_;
    private TextureViewModel? selectedTextureViewModel_;
    private TexturePreviewViewModel? selectedTexturePreviewViewModel_;

    private ObservableCollection<KeyValuePairViewModel>
        selectedTextureKeyValuePairs_ = [];

    public IReadOnlyList<IReadOnlyTexture>? Textures {
      get => this.textures_;
      set {
        this.RaiseAndSetIfChanged(ref this.textures_, value);
        this.TextureList = new TextureListViewModel { Textures = value };
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
        this.SelectedTextureKeyValuePairs = [
            ("Name", texture?.Name),
            ("Pixel Format", texture?.Image.PixelFormat),
            ("Transparency Type", texture?.TransparencyType),
            ("Width", texture?.Image.Width),
            ("Height", texture?.Image.Height),
            ("Horizontal Wrap Mode", texture?.WrapModeU),
            ("Vertical Wrap Mode", texture?.WrapModeV),
            ("UV Index", texture?.UvIndex),
            ("UV Type", texture?.UvType),
        ];
      }
    }

    public TexturePreviewViewModel? SelectedTexturePreview {
      get => this.selectedTexturePreviewViewModel_;
      private set => this.RaiseAndSetIfChanged(
          ref this.selectedTexturePreviewViewModel_,
          value);
    }

    public ObservableCollection<KeyValuePairViewModel>
        SelectedTextureKeyValuePairs {
      get => this.selectedTextureKeyValuePairs_;
      private set
        => this.RaiseAndSetIfChanged(ref this.selectedTextureKeyValuePairs_,
                                     value);
    }
  }

  public partial class TexturesPanel : UserControl {
    public TexturesPanel() {
      InitializeComponent();
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