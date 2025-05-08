using fin.image;
using fin.image.formats;

namespace gm.schema.dataWin;

using System;
using System.IO;

/// <summary>
///   A class that converts to and from the GM-custom QOI format.
///
///   Shamelessly stolen from:
///   https://github.com/UnderminersTeam/UndertaleModTool/blob/master/UndertaleModLib/Util/QoiConverter.cs#L33
/// </summary>
/// <remarks>Ported over from DogScepter's QOI converter at <see href="https://github.com/colinator27/dog-scepter/"/>.</remarks>
public static class QoiConverter {
  public const int MaxChunkSize = 5; // according to the QOI spec: https://qoiformat.org/qoi-specification.pdf

  public const int HeaderSize = 12;

  private const byte QOI_INDEX = 0x00;
  private const byte QOI_RUN_8 = 0x40;
  private const byte QOI_RUN_16 = 0x60;
  private const byte QOI_DIFF_8 = 0x80;
  private const byte QOI_DIFF_16 = 0xc0;
  private const byte QOI_DIFF_24 = 0xe0;

  private const byte QOI_COLOR = 0xf0;
  private const byte QOI_MASK_2 = 0xc0;
  private const byte QOI_MASK_3 = 0xe0;
  private const byte QOI_MASK_4 = 0xf0;

  /// <summary>
  /// Creates a raw format <see cref="GMImage"/> from a <see cref="Stream"/>.
  /// </summary>
  /// <param name="s">The stream to create the PNG image from.</param>
  /// <returns>The QOI image as a raw format image.</returns>
  /// <exception cref="Exception">If there is an invalid QOIF magic header or there was an error with stride width.</exception>
  public static IImage GetImageFromStream(Stream s) {
    Span<byte> header = stackalloc byte[12];
    s.ReadExactly(header);
    int length = header[8] +
                 (header[9] << 8) +
                 (header[10] << 16) +
                 (header[11] << 24);
    byte[] bytes = new byte[12 + length];
    s.Position -= 12;
    s.ReadExactly(bytes, 0, bytes.Length);
    return GetImageFromSpan(bytes);
  }

  /// <summary>
  /// Creates a raw format <see cref="GMImage"/> from a <see cref="ReadOnlySpan{TKey}"/> of <see cref="byte"/>s.
  /// </summary>
  /// <param name="bytes">The <see cref="Span{TKey}"/> of <see cref="byte"/>s to create the raw image from.</param>
  /// <returns>The QOI image as a raw format image.</returns>
  /// <exception cref="Exception">If there is an invalid QOIF magic header or there was an error with stride width.</exception>
  public static IImage GetImageFromSpan(ReadOnlySpan<byte> bytes)
    => GetImageFromSpan(bytes, out _);

  /// <summary><inheritdoc cref="GetImageFromSpan(System.ReadOnlySpan{byte})"/></summary>
  /// <param name="bytes"><inheritdoc cref="GetImageFromSpan(System.ReadOnlySpan{byte})"/></param>
  /// <param name="length">The total amount of data read from the <see cref="Span{TKey}"/>.</param>
  /// <returns><inheritdoc cref="GetImageFromSpan(System.ReadOnlySpan{byte})"/></returns>
  /// <exception cref="Exception"><inheritdoc cref="GetImageFromSpan(System.ReadOnlySpan{byte})"/></exception>
  public static IImage GetImageFromSpan(ReadOnlySpan<byte> bytes,
                                        out int length) {
    ReadOnlySpan<byte> header = bytes[..12];
    if (header[0] != (byte) 'f' ||
        header[1] != (byte) 'i' ||
        header[2] != (byte) 'o' ||
        header[3] != (byte) 'q')
      throw new Exception("Invalid little-endian QOIF image magic");

    int width = header[4] + (header[5] << 8);
    int height = header[6] + (header[7] << 8);
    length = header[8] +
             (header[9] << 8) +
             (header[10] << 16) +
             (header[11] << 24);

    ReadOnlySpan<byte> pixelData = bytes.Slice(12, length);

    int pos = 0;
    int run = 0;
    byte r = 0, g = 0, b = 0, a = 255;
    Span<byte> index = stackalloc byte[64 * 4];

    Rgba32Image img = new Rgba32Image(width, height);
    var fastLock = img.Lock();
    Span<byte> rawData = fastLock.Bytes;
    int rawDataLength = rawData.Length;
    for (int rawDataPos = 0; rawDataPos < rawDataLength; rawDataPos += 4) {
      if (run > 0) {
        run--;
      } else if (pos < pixelData.Length) {
        int b1 = pixelData[pos++];

        if ((b1 & QOI_MASK_2) == QOI_INDEX) {
          int indexPos = (b1 ^ QOI_INDEX) << 2;
          r = index[indexPos];
          g = index[indexPos + 1];
          b = index[indexPos + 2];
          a = index[indexPos + 3];
        } else if ((b1 & QOI_MASK_3) == QOI_RUN_8) {
          run = b1 & 0x1f;
        } else if ((b1 & QOI_MASK_3) == QOI_RUN_16) {
          int b2 = pixelData[pos++];
          run = (((b1 & 0x1f) << 8) | b2) + 32;
        } else if ((b1 & QOI_MASK_2) == QOI_DIFF_8) {
          r += (byte) (((b1 & 48) << 26 >> 30) & 0xff);
          g += (byte) (((b1 & 12) << 28 >> 22 >> 8) & 0xff);
          b += (byte) (((b1 & 3) << 30 >> 14 >> 16) & 0xff);
        } else if ((b1 & QOI_MASK_3) == QOI_DIFF_16) {
          int b2 = pixelData[pos++];
          int merged = b1 << 8 | b2;
          r += (byte) (((merged & 7936) << 19 >> 27) & 0xff);
          g += (byte) (((merged & 240) << 24 >> 20 >> 8) & 0xff);
          b += (byte) (((merged & 15) << 28 >> 12 >> 16) & 0xff);
        } else if ((b1 & QOI_MASK_4) == QOI_DIFF_24) {
          int b2 = pixelData[pos++];
          int b3 = pixelData[pos++];
          int merged = b1 << 16 | b2 << 8 | b3;
          r += (byte) (((merged & 1015808) << 12 >> 27) & 0xff);
          g += (byte) (((merged & 31744) << 17 >> 19 >> 8) & 0xff);
          b += (byte) (((merged & 992) << 22 >> 11 >> 16) & 0xff);
          a += (byte) (((merged & 31) << 27 >> 3 >> 24) & 0xff);
        } else if ((b1 & QOI_MASK_4) == QOI_COLOR) {
          if ((b1 & 8) != 0)
            r = pixelData[pos++];
          if ((b1 & 4) != 0)
            g = pixelData[pos++];
          if ((b1 & 2) != 0)
            b = pixelData[pos++];
          if ((b1 & 1) != 0)
            a = pixelData[pos++];
        }

        int indexPos2 = ((r ^ g ^ b ^ a) & 63) << 2;
        index[indexPos2] = r;
        index[indexPos2 + 1] = g;
        index[indexPos2 + 2] = b;
        index[indexPos2 + 3] = a;
      }

      rawData[rawDataPos] = r;
      rawData[rawDataPos + 1] = g;
      rawData[rawDataPos + 2] = b;
      rawData[rawDataPos + 3] = a;
    }

    length += header.Length;
    return img;
  }
}