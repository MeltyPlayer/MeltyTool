using System.Collections.ObjectModel;
using System.Linq;

using Avalonia.Controls;

using fin.model;

using ReactiveUI;

using uni.ui.avalonia.resources;
using uni.ui.avalonia.ViewModels;

using Bitmap = Avalonia.Media.Imaging.Bitmap;

namespace uni.ui.avalonia.materials {
  public class MaterialTexturesPanelViewModelForDesigner
      : MaterialTexturesPanelViewModel {
    public MaterialTexturesPanelViewModelForDesigner() {
      this.Material = MaterialDesignerUtil.CreateStubMaterial();
    }
  }

  public class MaterialTexturesPanelViewModel : ViewModelBase {
    private IReadOnlyMaterial? material_;
    private TextureViewModel? selectedTextureViewModel_;
    private ObservableCollection<TextureViewModel> textureViewModels_;

    public Bitmap TransparentBackgroundImage { get; }
      = AvaloniaImageUtil.Load("checkerboard");

    public int SelectedTextureViewerSize => 150;
    public int SelectedTextureImageSize => this.SelectedTextureViewerSize - 10;

    public required IReadOnlyMaterial? Material {
      get => this.material_;
      set {
        this.RaiseAndSetIfChanged(ref this.material_, value);
        this.Textures = new ObservableCollection<TextureViewModel>(
            this.material_?.Textures
                .Select(texture => new TextureViewModel()
                            { Texture = texture }) ??
            Enumerable.Empty<TextureViewModel>());

        this.SelectedTexture = this.Textures.FirstOrDefault();
      }
    }

    public TextureViewModel? SelectedTexture {
      get => this.selectedTextureViewModel_;
      set => this.RaiseAndSetIfChanged(ref this.selectedTextureViewModel_,
                                       value);
    }

    public ObservableCollection<TextureViewModel> Textures {
      get => this.textureViewModels_;
      private set
        => this.RaiseAndSetIfChanged(ref this.textureViewModels_, value);
    }
  }

  public class TextureViewModel : ViewModelBase {
    private IReadOnlyTexture texture_;
    private Bitmap image_;
    private string caption_;

    public required IReadOnlyTexture Texture {
      get => this.texture_;
      set {
        this.RaiseAndSetIfChanged(ref this.texture_, value);
        this.Image = value.AsAvaloniaImage();

        var image = value.Image;
        this.Caption = $"{image.PixelFormat}, {image.Width}x{image.Height}";
      }
    }

    public Bitmap Image {
      get => this.image_;
      private set => this.RaiseAndSetIfChanged(ref this.image_, value);
    }

    public string Caption {
      get => this.caption_;
      set => this.RaiseAndSetIfChanged(ref this.caption_, value);
    }
  }

  public partial class MaterialTexturesPanel : UserControl {
    public MaterialTexturesPanel() {
      InitializeComponent();
    }
  }
}