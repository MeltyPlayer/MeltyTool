using System;

using CommunityToolkit.HighPerformance;

using fin.image.formats;

using SixLabors.ImageSharp.PixelFormats;


namespace fin.image.io.pixel;

/// <summary>
///   Helper class for reading 32-bit RGBA pixels.
/// </summary>
public class Rgba32PixelReader : IPixelReader<Rgba32> {
  public IImage<Rgba32> CreateImage(int width, int height)
    => new Rgba32Image(PixelFormat.RGBA8888, width, height);

  public void Decode(ReadOnlySpan<byte> data, Span<Rgba32> scan0, int offset)
    => scan0[offset] = data.Cast<byte, Rgba32>()[0];

  public int BitsPerPixel => 32;
}