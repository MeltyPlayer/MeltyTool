using System.Drawing;
using System.IO;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

using fin.image;
using fin.model;
using fin.util.asserts;

using ReactiveUI;

using uni.ui.avalonia.model;
using uni.ui.avalonia.resources;
using uni.ui.avalonia.ViewModels;

using Bitmap = Avalonia.Media.Imaging.Bitmap;

namespace uni.ui.avalonia.textures;

public class TexturePreviewViewModelForDesigner : TexturePreviewViewModel {
  public TexturePreviewViewModelForDesigner() {
    this.Texture = ModelDesignerUtil.CreateStubTexture(32, 48);
    this.ImageMargin = new Thickness(10);
  }
}

public class TexturePreviewViewModel : ViewModelBase {
  private static readonly Bitmap missingImage_
      = FinImage.Create1x1FromColor(Color.Magenta).AsAvaloniaImage();

  private IReadOnlyTexture? texture_;
  private Bitmap image_;
  private Thickness imageMargin_;

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

  private TexturePreviewViewModel ViewModel_
    => Asserts.AsA<TexturePreviewViewModel>(this.DataContext);

  private async void CopyToClipboard_(object? sender, RoutedEventArgs e) {
    var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
    if (clipboard == null) {
      return;
    }

    var texture = this.ViewModel_.Texture;
    if (texture == null) {
      return;
    }

    var formatName = "image/png";

    using var ms = new MemoryStream();
    texture.WriteToStream(ms);

    var dataObject = new DataObject();
    dataObject.Set(formatName, ms.ToArray());

    await clipboard.SetDataObjectAsync(dataObject);
  }
}