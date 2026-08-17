using System.Drawing;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Threading;

using fin.image;
using fin.model;
using fin.ui;
using fin.ui.avalonia.controls;
using fin.ui.avalonia.images;
using fin.util.asserts;

using ReactiveUI;

using uni.ui.avalonia.resources.model;

using Bitmap = Avalonia.Media.Imaging.Bitmap;

namespace uni.ui.avalonia.resources.texture;

public sealed class TexturePreviewViewModelForDesigner : TexturePreviewViewModel {
  public TexturePreviewViewModelForDesigner() {
    this.Texture = ModelDesignerUtil.CreateStubTexture(32, 48);
    this.ImageMargin = new Thickness(10);
  }
}

public class TexturePreviewViewModel : BViewModel {
  private static readonly Bitmap missingImage_
      = FinImage.Create1x1FromColor(Color.Magenta).AsAvaloniaImage();

  public required IReadOnlyTexture? Texture {
    get;
    set {
      this.RaiseAndSetIfChanged(ref field, value);
      this.Image = null;
      this.ImageAsync = Dispatcher.UIThread.InvokeAsync(
          () => this.Image = value?.AsMergedMipmapAvaloniaImage() ??
                             missingImage_);
    }
  }

  protected Bitmap? Image {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
  }

  public DispatcherOperation<Bitmap>? ImageAsync { get; set; }

  public Thickness ImageMargin {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
  }
}

public partial class TexturePreview : BUserControl<TexturePreviewViewModel> {
  public TexturePreview() {
    this.InitializeComponent();
  }

  private TexturePreviewViewModel ViewModel_
    => Asserts.AsA<TexturePreviewViewModel>(this.ViewModel);

  private async void CopyToClipboard_(object? sender, RoutedEventArgs e) {
    var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
    if (clipboard == null) {
      return;
    }

    var bitmapTask = this.ViewModel_.ImageAsync?.GetTask();
    var bitmap = bitmapTask != null ? await bitmapTask : null;
    await clipboard.SetBitmapAsync(bitmap);
  }
}