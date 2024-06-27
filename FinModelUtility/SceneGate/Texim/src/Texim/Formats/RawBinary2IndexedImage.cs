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
using System.IO;
using Images;
using Pixels;
using Yarhl.FileFormat;
using Yarhl.IO;

public class RawBinary2IndexedImage :
    IInitializer<RawIndexedImageParams>, IConverter<IBinary, IndexedImage>
{
    private RawIndexedImageParams parameters = RawIndexedImageParams.Default;

    public void Initialize(RawIndexedImageParams parameters)
    {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            this.parameters = parameters;
        }

    public IndexedImage Convert(IBinary source)
    {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            int size = parameters.Size;
            if (size <= 0) {
                size = (int)(source.Stream.Length - parameters.Offset);
            }

            source.Stream.Position = parameters.Offset;
            byte[] data = new byte[size];
            int read = source.Stream.Read(data, 0, size);
            if (read != size) {
                throw new EndOfStreamException();
            }

            IndexedPixel[] pixels = parameters.PixelEncoding.Decode(data);
            if (parameters.Swizzling != null) {
                pixels = parameters.Swizzling.Unswizzle(pixels);
            }

            int width = parameters.Width;
            if (width <= 0) {
                width = 8;
            }

            int height = parameters.Height;
            if (height <= 0) {
                height = (int)Math.Ceiling((double)size / width);
            }

            return new IndexedImage(width, height, pixels);
        }
}
