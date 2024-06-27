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
namespace Texim.Pixels
{
    using System.Collections.Generic;
    using Yarhl.IO;

    public static class IOExtensions
    {
        public static IndexedPixel[] ReadPixels<T>(this DataReader reader, int numPixels)
            where T : IIndexedPixelEncoding, new()
        {
            T decoder = new T();
            return decoder.Decode(reader.Stream, numPixels);
        }

        public static IndexedPixel[] DecodePixelsAs<T>(this byte[] data)
            where T : IIndexedPixelEncoding, new()
        {
            T decoder = new T();
            return decoder.Decode(data);
        }

        public static void Write<T>(this DataWriter writer, IEnumerable<IndexedPixel> pixels)
            where T : IIndexedPixelEncoding, new()
        {
            T encoder = new T();
            writer.Write(encoder.Encode(pixels));
        }

        public static byte[] EncodePixelsAs<T>(this IndexedPixel[] pixels)
            where T : IIndexedPixelEncoding, new()
        {
            T encoder = new T();
            return encoder.Encode(pixels);
        }

        public static IndexedPixel[] UnswizzleWith(this IndexedPixel[] pixels, ISwizzling<IndexedPixel> swizzling)
        {
            return swizzling.Unswizzle(pixels);
        }
    }
}
