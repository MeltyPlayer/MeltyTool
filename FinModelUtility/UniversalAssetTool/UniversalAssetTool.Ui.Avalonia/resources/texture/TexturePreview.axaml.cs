using System.Drawing;
using System.IO;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;

using fin.image;
using fin.image.formats;
using fin.model;
using fin.ui.avalonia;
using fin.ui.avalonia.images;
using fin.util.asserts;

using ReactiveUI;

using SixLabors.ImageSharp.PixelFormats;

using uni.ui.avalonia.resources.model;

using Bitmap = Avalonia.Media.Imaging.Bitmap;

namespace uni.ui.avalonia.resources.texture;

public class TexturePreviewViewModelForDesigner : TexturePreviewViewModel {
  public TexturePreviewViewModelForDesigner() {
    this.Texture = ModelDesignerUtil.CreateStubTexture(32, 48);
    this.ImageMargin = new Thickness(10);
  }
}

public class TexturePreviewViewModel : ViewModelBase {
  private static readonly Bitmap missingImage_
      = FinImage.Create1x1FromColor(Color.Magenta).AsAvaloniaImage();

  public required IReadOnlyTexture? Texture {
    get;
    set {
      this.RaiseAndSetIfChanged(ref field, value);
      this.Image = null;
      Dispatcher.UIThread.Invoke(() => this.Image
                   = value?.AsMergedMipmapAvaloniaImage() ?? missingImage_);
    }
  }

  protected Bitmap? Image {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
  }

  public Thickness ImageMargin {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
  }
}

public partial class TexturePreview : UserControl {
  public TexturePreview() {
    this.InitializeComponent();
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

    using var ms = new MemoryStream();
    var firstImage = texture.Image;
    var mipmapImages = texture.MipmapImages;

    if (mipmapImages.Length == 1) {
      firstImage.ExportToStream(ms, LocalImageFormat.PNG);
    } else {
      using var mergedMipmapImage = this.GetMergedMipmapImage_(texture);
      mergedMipmapImage.ExportToStream(ms, LocalImageFormat.PNG);
    }

    var dataObject = new DataObject();
    var formatName = "image/png";
    dataObject.Set(formatName, ms.ToArray());

    await clipboard.SetDataObjectAsync(dataObject);
  }

  private unsafe Rgba32Image GetMergedMipmapImage_(IReadOnlyTexture texture) {
    var firstImage = texture.Image;
    var dst = new Rgba32Image(2 * firstImage.Width, firstImage.Height);
    using var dstLock = dst.UnsafeLock();
    var pixels = dstLock.pixelScan0;

    var mipmapImages = texture.MipmapImages;
    var baseDstY = 0;
    for (var i = 0; i < mipmapImages.Length; i++) {
      var baseDstX = i == 0 ? 0 : firstImage.Width;

      var mipmapImage = mipmapImages[i];
      mipmapImage.Access(get => {
        for (var srcY = 0; srcY < mipmapImage.Height; ++srcY) {
          for (var srcX = 0; srcX < mipmapImage.Width; ++srcX) {
            get(srcX, srcY, out var r, out var g, out var b, out var a);

            var dstX = baseDstX + srcX;
            var dstY = baseDstY + srcY;

            pixels[dstY * dst.Width + dstX] = new Rgba32(r, g, b, a);
          }
        }
      });

      if (i >= 1) {
        baseDstY += mipmapImage.Height;
      }
    }

    return dst;
  }
}