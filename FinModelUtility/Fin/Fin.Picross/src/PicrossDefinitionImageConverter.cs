using fin.image;
using fin.image.formats;

using SixLabors.ImageSharp.PixelFormats;

namespace fin.picross;

public class PicrossDefinitionImageConverter {
  public IImage ConvertToImage(IPicrossDefinition picrossDefinition) {
    var width = picrossDefinition.Width;
    var height = picrossDefinition.Height;

    var puzzleImage = new L8Image(PixelFormat.L8, width, height);
    using var imageLock = puzzleImage.Lock();
    var pixels = imageLock.Pixels;

    for (var y = 0; y < height; y++) {
      for (var x = 0; x < width; x++) {
        var cell = picrossDefinition[x, y];
        pixels[y * width + x] = new L8((byte) (cell ? 0 : 255));
      }
    }

    return puzzleImage;
  }
}