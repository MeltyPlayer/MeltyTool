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
namespace Texim.Colors;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class Rgba32 : IColorEncoding
{
    private static Rgba32 instance = new Rgba32();
    private readonly bool hasAlpha;

    public Rgba32()
    {
            hasAlpha = true;
        }

    internal Rgba32(bool hasAlpha)
    {
            this.hasAlpha = hasAlpha;
        }

    public static Rgba32 Instance => instance;

    public static int BytesPerColor => 4;

    public Rgb Decode(Stream stream)
    {
            Span<byte> data = stackalloc byte[4];
            int read = stream.Read(data);
            if (read != 4) {
                throw new EndOfStreamException();
            }

            byte alpha = hasAlpha ? data[3] : (byte)255;
            return new Rgb(data[0], data[1], data[2], alpha);
        }

    public Rgb[] Decode(Stream stream, int numColors)
    {
            byte[] data = new byte[numColors * BytesPerColor];
            int read = stream.Read(data, 0, data.Length);
            if (read != data.Length) {
                throw new EndOfStreamException();
            }

            return Decode(data);
        }

    public Rgb[] Decode(Span<byte> data)
    {
            var colors = new Rgb[data.Length / BytesPerColor];
            for (int i = 0; i < colors.Length; i++) {
                colors[i] = new Rgb(
                    data[i * 4],
                    data[(i * 4) + 1],
                    data[(i * 4) + 2],
                    hasAlpha ? data[(i * 4) + 3] : (byte)255);
            }

            return colors;
        }

    public byte[] Encode(Rgb color)
    {
            return [
                color.Red,
                color.Green,
                color.Blue,
                hasAlpha ? color.Alpha : (byte)0
            ];
        }

    public byte[] Encode(IEnumerable<Rgb> colors)
    {
            var colorList = colors.ToArray();
            byte[] data = new byte[colorList.Length];
            for (int i = 0; i < colorList.Length; i++) {
                data[i * 4] = colorList[i].Red;
                data[(i * 4) + 1] = colorList[i].Green;
                data[(i * 4) + 2] = colorList[i].Blue;
                data[(i * 4) + 3] = hasAlpha ? colorList[i].Alpha : (byte)0;
            }

            return data;
        }
}
