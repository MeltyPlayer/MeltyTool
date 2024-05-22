using System.Drawing;

using Avalonia;
using Avalonia.Controls;

using fin.image;
using fin.model;

using ReactiveUI;

using uni.ui.avalonia.materials;
using uni.ui.avalonia.resources;
using uni.ui.avalonia.ViewModels;

using Bitmap = Avalonia.Media.Imaging.Bitmap;

namespace uni.ui.avalonia.textures;

public class TexturePreviewViewModelForDesigner : TexturePreviewViewModel {
  public TexturePreviewViewModelForDesigner() {
    this.Texture = MaterialDesignerUtil.CreateStubTexture(32, 48);
    this.ImageMargin = new Thickness(10);
  }
}

public class TexturePreviewViewModel : ViewModelBase {
  private static readonly Bitmap transparentBackgroundImage_
      = AvaloniaImageUtil.Load("checkerboard");

  private static readonly Bitmap missingImage_
      = FinImage.Create1x1FromColor(Color.Magenta).AsAvaloniaImage();

  private IReadOnlyTexture? texture_;
  private Bitmap image_;
  private Thickness imageMargin_;

  public Bitmap TransparentBackgroundImage => transparentBackgroundImage_;

  public required IReadOnlyTexture? Texture {
    get => this.texture_;
    set {
      this.RaiseAndSetIfChanged(ref this.texture_, value);
      this.Image = value?.AsAvaloniaImage() ?? missingImage_;
    }
  }

  public Bitmap Image {
    get => this.image_;
    private set => this.RaiseAndSetIfChanged(ref this.image_, value);
  }

  public Thickness ImageMargin {
    get => this.imageMargin_;
    set => this.RaiseAndSetIfChanged(ref this.imageMargin_, value);
  }
}

public partial class TexturePreview : UserControl {
  public TexturePreview() {
    InitializeComponent();
  }
}