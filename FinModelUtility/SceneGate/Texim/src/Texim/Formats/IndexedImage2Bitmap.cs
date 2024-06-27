// Copyright (c) 2021 SceneGate

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
namespace Texim.Formats;

using System;
using System.Linq;
using Images;
using Palettes;
using SixLabors.ImageSharp;
using Yarhl.FileFormat;
using Yarhl.IO;
using SixRgb = SixLabors.ImageSharp.PixelFormats.Rgba32;

public class IndexedImage2Bitmap :
    IInitializer<IndexedImageBitmapParams>,
    IConverter<IIndexedImage, BinaryFormat>
{
    private IndexedImageBitmapParams parameters;

    public void Initialize(IndexedImageBitmapParams parameters)
    {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            this.parameters = parameters;
        }

    public BinaryFormat Convert(IIndexedImage source)
    {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            // Preconvert to colors for faster iteration later
            SixRgb[] PaletteToColors(IPalette pal) =>
                pal.Colors.Select(c => c.ToImageSharpColor()).ToArray();

            SixRgb[][] palettes = (parameters.Palettes != null)
                ? parameters.Palettes.Palettes.Select(PaletteToColors).ToArray()
                : [PaletteToColors(parameters.Palette)];

            using var bitmap = new Image<SixRgb>(source.Width, source.Height);

            for (int x = 0; x < source.Width; x++) {
                for (int y = 0; y < source.Height; y++) {
                    var pixel = source.Pixels[(y * source.Width) + x];
                    if (pixel.PaletteIndex >= palettes.Length) {
                        throw new FormatException(
                            $"[x={x},y={y}] Missing palettes. " +
                            $"Need palette #{pixel.PaletteIndex} but it has {palettes.Length}. " +
                            $"First palette length: {palettes[0].Length}");
                    }

                    var palette = palettes[pixel.PaletteIndex];
                    if (pixel.Index >= palette.Length) {
                        throw new FormatException(
                            $"[x={x},y={y}] Missing colors in palette. " +
                            $"Need color #{pixel.Index} but palette " +
                            $"#{pixel.PaletteIndex} has {palette.Length}");
                    }

                    SixRgb color = (pixel.Alpha != 255)
                        ? new SixRgb(palette[pixel.Index].R, palette[pixel.Index].G, palette[pixel.Index].B, pixel.Alpha)
                        : palette[pixel.Index];
                    bitmap[x, y] = color;
                }
            }

            var binary = new BinaryFormat();
            bitmap.Save(binary.Stream, parameters.Encoder);
            return binary;
        }
}
