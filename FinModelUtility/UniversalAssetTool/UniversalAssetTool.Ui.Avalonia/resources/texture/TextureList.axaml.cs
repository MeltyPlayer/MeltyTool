using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Avalonia.Controls;
using Avalonia.Interactivity;

using fin.data.dictionaries;
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

    var modelsAndTextures = new ListDictionary<IReadOnlyModel, IReadOnlyTexture>();
    modelsAndTextures.AddRange(model, material.Textures);

    this.ModelsAndTextures = modelsAndTextures;
  }
}

public class TextureListViewModel : BViewModel {
  public IReadOnlyListDictionary<IReadOnlyModel, IReadOnlyTexture>
      ModelsAndTextures {
    get;
    set {
      this.RaiseAndSetIfChanged(ref field, value);
      this.TextureViewModels = [
          .. value.GetPairs()
                  .SelectMany(tuple => tuple.value.Select(t => (tuple.key, t)))
                  .Select(tuple => new TextureViewModel {
                      Model = tuple.key,
                      Texture = tuple.t
                  })
                  .OrderBy(t => t.Texture.Name, StringUtil.NaturalSortInstance)
                  .ThenBy(t => t.Texture.Image.GetHashCode())
      ];
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

      var model = field?.Model;
      var texture = field?.Texture;
      SelectedTextureService.SelectTexture(
          model != null && texture != null ? (model, texture) : null);
    }
  }
}

public sealed class TextureViewModel : BViewModel {
  public TexturePreviewViewModel texturePreviewViewModel_;

  public required IReadOnlyModel Model {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
  }

  public required IReadOnlyTexture Texture {
    get;
    set {
      this.RaiseAndSetIfChanged(ref field, value);
      this.TexturePreview = new TexturePreviewViewModel { Texture = value };
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