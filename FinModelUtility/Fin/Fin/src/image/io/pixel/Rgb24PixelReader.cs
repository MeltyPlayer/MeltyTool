using System;

using CommunityToolkit.HighPerformance;

using fin.image.formats;

using SixLabors.ImageSharp.PixelFormats;


namespace fin.image.io.pixel;

/// <summary>
///   Helper class for reading 24-bit RGB pixels.
/// </summary>
public class Rgb24PixelReader : IPixelReader<Rgb24> {
  public IImage<Rgb24> CreateImage(int width, int height)
    => new Rgb24Image(PixelFormat.RGB888, width, height);

  public void Decode(ReadOnlySpan<byte> data, Span<Rgb24> scan0, int offset)
    => scan0[offset] = data.Cast<byte, Rgb24>()[0];

  public int BitsPerPixel => 24;
}