using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Avalonia.Controls;
using Avalonia.Interactivity;

using fin.model;
using fin.ui;
using fin.ui.avalonia.controls;
using fin.ui.rendering;
using fin.util.strings;

using ReactiveUI;

using uni.ui.avalonia.resources.model;

namespace uni.ui.avalonia.resources.texture;

public sealed class TextureListViewModelForDesigner
    : TextureListViewModel {
  public TextureListViewModelForDesigner() {
    var (model, material) = ModelDesignerUtil.CreateStubModelAndMaterial();
    this.ModelAndTextures = (model, material.Textures.ToArray());
  }
}

public class TextureListViewModel : BViewModel {
  private (IReadOnlyModel, IReadOnlyList<IReadOnlyTexture>)?
      modelAndTextures_;

  public required (IReadOnlyModel, IReadOnlyList<IReadOnlyTexture>)?
      ModelAndTextures {
    get => this.modelAndTextures_;
    set {
      this.RaiseAndSetIfChanged(ref this.modelAndTextures_, value);
      this.Textures = value?.Item2;
    }
  }


  public IReadOnlyList<IReadOnlyTexture>? Textures {
    get;
    private set {
      this.RaiseAndSetIfChanged(ref field, value);
      this.TextureViewModels = new ObservableCollection<TextureViewModel>(
          value?.Select(texture => new TextureViewModel
                            { Texture = texture })
               .OrderBy(
                   t => t.Texture.Name,
                   StringUtil.NaturalSortInstance) ??
          Enumerable.Empty<TextureViewModel>());
    }
  }

  public ObservableCollection<TextureViewModel> TextureViewModels {
    get;
    private set {
      this.RaiseAndSetIfChanged(ref field, value);
      this.SelectedTextureViewModel = this.TextureViewModels.FirstOrDefault();
    }
  }

  public TextureViewModel? SelectedTextureViewModel {
    get;
    set {
      this.RaiseAndSetIfChanged(ref field,
                                value);

      var model = this.modelAndTextures_?.Item1;
      var texture = field?.Texture;
      SelectedTextureService.SelectTexture(
          model != null && texture != null ? (model, texture) : null);
    }
  }
}

public sealed class TextureViewModel : BViewModel {
  public TexturePreviewViewModel texturePreviewViewModel_;

  public required IReadOnlyTexture Texture {
    get;
    set {
      this.RaiseAndSetIfChanged(ref field, value);

      this.TexturePreview = new TexturePreviewViewModel { Texture = value };

      var image = value.Image;
    }
  }

  public TexturePreviewViewModel TexturePreview {
    get => this.texturePreviewViewModel_;
    private set => this.RaiseAndSetIfChanged(
        ref this.texturePreviewViewModel_,
        value);
  }
}

public partial class TextureList : BUserControl<TextureListViewModel> {
  public TextureList() {
    this.InitializeComponent();
  }

  public static readonly RoutedEvent<TextureSelectedEventArgs>
      TextureSelectedEvent =
          RoutedEvent.Register<TextureList, TextureSelectedEventArgs>(
              nameof(TextureSelected),
              RoutingStrategies.Direct);

  public event EventHandler<TextureSelectedEventArgs> TextureSelected {
    add => this.AddHandler(TextureSelectedEvent, value);
    remove => this.RemoveHandler(TextureSelectedEvent, value);
  }

  protected void SelectingItemsControl_OnSelectionChanged(
      object? sender,
      SelectionChangedEventArgs e) {
    if (e.AddedItems.Count == 0 ||
        e.AddedItems[0] is not TextureViewModel selectedTextureViewModel) {
      return;
    }

    this.RaiseEvent(new TextureSelectedEventArgs {
        RoutedEvent = TextureSelectedEvent,
        Texture = selectedTextureViewModel
    });
  }
}

public sealed class TextureSelectedEventArgs : RoutedEventArgs {
  public required TextureViewModel Texture { get; init; }
}