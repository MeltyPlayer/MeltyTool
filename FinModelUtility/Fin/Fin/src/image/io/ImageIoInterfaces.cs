using System;

using schema.binary;

using SixLabors.ImageSharp.PixelFormats;

namespace fin.image.io;

public interface IPixelReader<TPixel>
    where TPixel : unmanaged, IPixel<TPixel> {
  IImage<TPixel> CreateImage(int width, int height);

  void Decode(ReadOnlySpan<byte> data, Span<TPixel> scan0, int offset);

  int PixelsPerRead => 1;
  int BitsPerPixel { get; }
}

public interface IPixelIndexer {
  void GetPixelCoordinates(int index, out int x, out int y);
}

public interface ITileReader<TPixel>
    where TPixel : unmanaged, IPixel<TPixel> {
  IImage<TPixel> CreateImage(int width, int height);

  int TileWidth { get; }
  int TileHeight { get; }

  void Decode(IBinaryReader br,
              Span<TPixel> scan0,
              int tileX,
              int tileY,
              int imageWidth,
              int imageHeight);
}

public interface IImageReader {
  IImage ReadImage(byte[] srcBytes,
                   Endianness endianness = Endianness.LittleEndian) {
    using var br = new SchemaBinaryReader(srcBytes, endianness);
    return this.ReadImage(br);
  }

  IImage ReadImage(IBinaryReader br);
}

public interface IImageReader<out TImage> : IImageReader
    where TImage : IImage {
  IImage IImageReader.ReadImage(
      byte[] srcBytes,
      Endianness endianness = Endianness.LittleEndian)
    => this.ReadImage(srcBytes, endianness);

  new TImage ReadImage(byte[] srcBytes,
                       Endianness endianness = Endianness.LittleEndian) {
    using var br = new SchemaBinaryReader(srcBytes, endianness);
    return this.ReadImage(br);
  }

  IImage IImageReader.ReadImage(IBinaryReader br) => this.ReadImage(br);

  new TImage ReadImage(IBinaryReader br);
}