using System.Collections.Generic;
using System.Linq;

using Avalonia;
using Avalonia.Controls;

using fin.model;
using fin.util.asserts;

using ReactiveUI;

using uni.ui.avalonia.resources.texture;
using uni.ui.avalonia.ViewModels;

namespace uni.ui.avalonia.resources.model.materials {
  public class MaterialTexturesPanelViewModelForDesigner
      : MaterialTexturesPanelViewModel {
    public MaterialTexturesPanelViewModelForDesigner() {
      var (model, material) = ModelDesignerUtil.CreateStubModelAndMaterial();
      this.ModelAndTextures = (model, material.Textures.ToArray());
    }
  }

  public class MaterialTexturesPanelViewModel : ViewModelBase {
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
      }
    }

    public TexturePreviewViewModel? SelectedTexturePreview {
      get;
      private set => this.RaiseAndSetIfChanged(
          ref field,
          value);
    }
  }

  public partial class MaterialTexturesPanel : UserControl {
    public MaterialTexturesPanel() {
      this.InitializeComponent();
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