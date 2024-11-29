using System;

using fin.image.io.tile;

using schema.binary;

using SixLabors.ImageSharp.PixelFormats;


namespace fin.image.io;

public static class PixelImageReader {
  public static PixelImageReader<TPixel> New<TPixel>(
      int width,
      int height,
      IPixelReader<TPixel> pixelReader)
      where TPixel : unmanaged, IPixel<TPixel>
    => New(width,
           height,
           new BasicPixelIndexer(width),
           pixelReader);

  public static PixelImageReader<TPixel> New<TPixel>(
      int width,
      int height,
      IPixelIndexer pixelIndexer,
      IPixelReader<TPixel> pixelReader)
      where TPixel : unmanaged, IPixel<TPixel>
    => new(width,
           height,
           pixelIndexer,
           pixelReader);
}

public class PixelImageReader<TPixel>(
    int width,
    int height,
    IPixelIndexer pixelIndexer,
    IPixelReader<TPixel> pixelReader)
    : IImageReader<IImage<TPixel>>
    where TPixel : unmanaged, IPixel<TPixel> {
  public IImage<TPixel> ReadImage(
      byte[] srcBytes,
      Endianness endianness = Endianness.LittleEndian) {
    using var br = new SchemaBinaryReader(srcBytes, endianness);
    return this.ReadImage(br);
  }

  public IImage<TPixel> ReadImage(IBinaryReader br) {
    var image = pixelReader.CreateImage(width, height);
    using var imageLock = image.Lock();
    var scan0 = imageLock.Pixels;

    Span<byte> bytes =
        stackalloc byte[width * height * pixelReader.BitsPerPixel / 8];

    var bytesPerRead = pixelReader.PixelsPerRead * pixelReader.BitsPerPixel / 8;
    br.FillBuffer(bytes, bytesPerRead);

    for (int i = 0, b = 0;
         i < width * height;
         i += pixelReader.PixelsPerRead, b += bytesPerRead) {
      pixelIndexer.GetPixelCoordinates(i, out var x, out var y);
      var dstOffs = y * width + x;
      pixelReader.Decode(bytes.Slice(b, bytesPerRead),
                         scan0,
                         dstOffs);
    }

    return image;
  }
}