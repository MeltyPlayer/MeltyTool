using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Avalonia.Controls;
using Avalonia.Interactivity;

using fin.model;
using fin.util.asserts;

using ReactiveUI;

using uni.ui.avalonia.model;
using uni.ui.avalonia.ViewModels;

namespace uni.ui.avalonia.textures {
  public class TextureListViewModelForDesigner
      : TextureListViewModel {
    public TextureListViewModelForDesigner() {
      this.Textures = ModelDesignerUtil.CreateStubMaterial()
                                       .Textures
                                       .ToArray();
    }
  }

  public class TextureListViewModel : ViewModelBase {
    private IReadOnlyList<IReadOnlyTexture>? textures_;
    private ObservableCollection<TextureViewModel> textureViewModels_;
    private TextureViewModel? selectedTextureViewModel_;

    public required IReadOnlyList<IReadOnlyTexture>? Textures {
      get => this.textures_;
      set {
        this.RaiseAndSetIfChanged(ref this.textures_, value);
        this.TextureViewModels = new ObservableCollection<TextureViewModel>(
            value?.Select(texture => new TextureViewModel
                              { Texture = texture }) ??
            Enumerable.Empty<TextureViewModel>());
      }
    }

    public ObservableCollection<TextureViewModel> TextureViewModels {
      get => this.textureViewModels_;
      private set {
        this.RaiseAndSetIfChanged(ref this.textureViewModels_, value);
        this.SelectedTextureViewModel = this.TextureViewModels.FirstOrDefault();
      }
    }

    public TextureViewModel? SelectedTextureViewModel {
      get => this.selectedTextureViewModel_;
      set => this.RaiseAndSetIfChanged(
          ref this.selectedTextureViewModel_,
          value);
    }
  }

  public class TextureViewModel : ViewModelBase {
    private IReadOnlyTexture texture_;
    public TexturePreviewViewModel texturePreviewViewModel_;

    public required IReadOnlyTexture Texture {
      get => this.texture_;
      set {
        this.RaiseAndSetIfChanged(ref this.texture_, value);

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

  public partial class TextureList : UserControl {
    public TextureList() {
      InitializeComponent();
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
      if (e.AddedItems.Count == 0) {
        return;
      }

      var selectedTextureViewModel
          = Asserts.AsA<TextureViewModel>(e.AddedItems[0]);
      this.RaiseEvent(new TextureSelectedEventArgs {
          RoutedEvent = TextureSelectedEvent,
          Texture = selectedTextureViewModel
      });
    }
  }

  public class TextureSelectedEventArgs : RoutedEventArgs {
    public required TextureViewModel Texture { get; init; }
  }
}