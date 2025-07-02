using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System;

using Avalonia.Controls;

using fin.io.bundles;
using fin.ui.rendering;
using fin.util.progress;

using NaturalSort.Extension;

using uni.ui.avalonia.resources.model;
using uni.ui.avalonia.resources.texture;
using uni.ui.avalonia.ViewModels;

namespace uni.ui.avalonia.io;

/*public class FileBundleGathererProgressViewModel : ViewModelBase {
  public required IPercentageProgress Progress { get; init; }
  public required IFileBundleDirectory Directory { get; init; }
}

public class FileBundleGatherersProgressViewModelForDesigner
    : FileBundleGatherersProgressViewModel {
  public FileBundleGatherersProgressViewModelForDesigner() {
    this.ModelAndTextures = (model, material.Textures.ToArray());
  }
}

public class FileBundleGatherersProgressViewModel : ViewModelBase {
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
                   new NaturalSortComparer(
                       StringComparison.OrdinalIgnoreCase)) ??
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
}*/

public partial class FileBundleGatherersProgressWindow : Window {
  public FileBundleGatherersProgressWindow() {
    InitializeComponent();
  }
}