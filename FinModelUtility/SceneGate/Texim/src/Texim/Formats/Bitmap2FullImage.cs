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
using Colors;
using Images;
using SixLabors.ImageSharp;
using Yarhl.FileFormat;
using Yarhl.IO;
using Rgba32 = SixLabors.ImageSharp.PixelFormats.Rgba32;

public class Bitmap2FullImage : IConverter<BinaryFormat, FullImage>
{
    public FullImage Convert(BinaryFormat source)
    {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            using var image = Image.Load<Rgba32>(source.Stream);
            var fullImage = new FullImage(image.Width, image.Height);

            for (int x = 0; x < image.Width; x++) {
                for (int y = 0; y < image.Height; y++) {
                    fullImage.Pixels[(y * image.Width) + x] = new Rgb(image[x, y]);
                }
            }

            return fullImage;
        }
}
