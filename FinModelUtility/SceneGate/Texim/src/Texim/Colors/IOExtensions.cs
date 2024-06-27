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
namespace Texim.Colors
{
    using System.Collections.Generic;
    using Yarhl.IO;

    public static class IOExtensions
    {
        public static Rgb ReadColor<T>(this DataReader reader)
            where T : IColorEncoding, new()
        {
            T encoding = new T();
            return encoding.Decode(reader.Stream);
        }

        public static Rgb[] ReadColors<T>(this DataReader reader, int numColors)
            where T : IColorEncoding, new()
        {
            T encoding = new T();
            return encoding.Decode(reader.Stream, numColors);
        }

        public static Rgb[] DecodeColorsAs<T>(this byte[] data)
            where T : IColorEncoding, new()
        {
            T decoder = new T();
            return decoder.Decode(data);
        }

        public static void Write<T>(this DataWriter writer, Rgb color)
            where T : IColorEncoding, new()
        {
            T encoding = new T();
            writer.Write(encoding.Encode(color));
        }

        public static void Write<T>(this DataWriter writer, IEnumerable<Rgb> colors)
            where T : IColorEncoding, new()
        {
            T encoding = new T();
            writer.Write(encoding.Encode(colors));
        }
    }
}
