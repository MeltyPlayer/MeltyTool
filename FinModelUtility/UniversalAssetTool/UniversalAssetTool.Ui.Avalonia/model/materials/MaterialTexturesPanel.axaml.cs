using System.Collections.Generic;
using System.Linq;

using Avalonia;
using Avalonia.Controls;

using fin.model;
using fin.util.asserts;

using ReactiveUI;

using uni.ui.avalonia.textures;
using uni.ui.avalonia.ViewModels;

namespace uni.ui.avalonia.model.materials {
  public class MaterialTexturesPanelViewModelForDesigner
      : MaterialTexturesPanelViewModel {
    public MaterialTexturesPanelViewModelForDesigner() {
      this.Textures
          = ModelDesignerUtil.CreateStubMaterial().Textures.ToArray();
    }
  }

  public class MaterialTexturesPanelViewModel : ViewModelBase {
    private IReadOnlyList<IReadOnlyTexture>? textures_;
    private TextureListViewModel textureListViewModel_;
    private TextureViewModel? selectedTextureViewModel_;
    private TexturePreviewViewModel? selectedTexturePreviewViewModel_;

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
      }
    }

    public TexturePreviewViewModel? SelectedTexturePreview {
      get => this.selectedTexturePreviewViewModel_;
      private set => this.RaiseAndSetIfChanged(
          ref this.selectedTexturePreviewViewModel_,
          value);
    }
  }

  public partial class MaterialTexturesPanel : UserControl {
    public MaterialTexturesPanel() {
      InitializeComponent();
    }

    protected MaterialTexturesPanelViewModel ViewModel
      => Asserts.AsA<MaterialTexturesPanelViewModel>(this.DataContext);

    protected void TextureList_OnTextureSelected(
        object? sender,
        TextureSelectedEventArgs e) {
      this.ViewModel.SelectedTexture = e.Texture;
    }
  }
}