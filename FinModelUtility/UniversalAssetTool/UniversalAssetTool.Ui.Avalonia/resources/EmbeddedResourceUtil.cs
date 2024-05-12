using System;
using System.Linq;
using System.Reflection;

using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

using fin.util.asserts;

namespace uni.ui.avalonia.resources {
  public static class EmbeddedResourceUtil {
    public static IImage Load(string embeddedResourceName)
      => Load(Assembly.GetExecutingAssembly(), embeddedResourceName);

    public static IImage Load(Assembly assembly, string embeddedResourceName) {
      var resourceNames = assembly.GetManifestResourceNames();
      Asserts.True(resourceNames.Contains(embeddedResourceName));

      using var stream
          = assembly.GetManifestResourceStream(embeddedResourceName)!;
      return new Bitmap(stream);
    }

    public static string GetAvaloniaImageUri(string imageName)
      => $"avares://UniversalAssetTool.Ui.Avalonia/resources/images/{imageName}.png";


    public static Bitmap LoadAvaloniaImage(string imageName)
      => new(AssetLoader.Open(new Uri(GetAvaloniaImageUri(imageName))));

    public static unsafe Bitmap CreateFromColor(
        int width,
        int height,
        Color color) {
      var bitmap = new WriteableBitmap(new PixelSize(width, height),
                                       new Vector(96, 96),
                                       PixelFormat.Rgba8888,
                                       AlphaFormat.Premul);

      using var frameBuffer = bitmap.Lock();
      var span = new Span<byte>(frameBuffer.Address.ToPointer(),
                                4 * width * height);
      for (var i = 0; i < span.Length; i += 4) {
        span[i + 0] = color.R;
        span[i + 1] = color.G;
        span[i + 2] = color.B;
        span[i + 3] = color.A;
      }

      return bitmap;
    }
  }
}