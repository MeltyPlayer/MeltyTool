using System.Collections.Generic;
using System.Linq;

using Avalonia;
using fin.model;
using fin.ui;
using fin.ui.avalonia.controls;

using ReactiveUI;

using uni.ui.avalonia.resources.texture;

namespace uni.ui.avalonia.resources.model.materials;

public sealed class MaterialTexturesPanelViewModelForDesigner
    : MaterialTexturesPanelViewModel {
  public MaterialTexturesPanelViewModelForDesigner() {
    var (model, material) = ModelDesignerUtil.CreateStubModelAndMaterial();
    this.ModelAndTextures = (model, material.Textures.ToArray());
  }
}

public class MaterialTexturesPanelViewModel : BViewModel {
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

public partial class MaterialTexturesPanel
    : BUserControl<MaterialTexturesPanelViewModel> {
  public MaterialTexturesPanel() {
    this.InitializeComponent();
  }

  protected void TextureList_OnTextureSelected(
      object? sender,
      TextureSelectedEventArgs e) {
    this.ViewModel.SelectedTexture = e.Texture;
  }
}