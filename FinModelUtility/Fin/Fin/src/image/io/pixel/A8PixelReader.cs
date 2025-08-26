﻿using System;

using fin.image.formats;

using SixLabors.ImageSharp.PixelFormats;

namespace fin.image.io.pixel;

/// <summary>
///   Helper class for reading 8-bit alpha pixels.
/// </summary>
public sealed class A8PixelReader : IPixelReader<La16> {
  public IImage<La16> CreateImage(int width, int height)
    => new La16Image(PixelFormat.A8, width, height);

  public void Decode(ReadOnlySpan<byte> data, Span<La16> scan0, int offset)
    => scan0[offset] = new La16(0xFF, data[0]);

  public int BitsPerPixel => 8;
}