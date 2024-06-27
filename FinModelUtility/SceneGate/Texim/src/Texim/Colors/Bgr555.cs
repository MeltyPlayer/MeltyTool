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

public class Bgr555 : IColorEncoding
{
    public static int BytesPerColor => 2;

    public static Bgr555 Instance { get; } = new Bgr555();

    public static Rgb FromUInt16(ushort value)
    {
            return new Rgb {
                Red = (byte)((value & 0x1F) << 3),
                Green = (byte)(((value >> 5) & 0x1F) << 3),
                Blue = (byte)(((value >> 10) & 0x1F) << 3),
                Alpha = 255,
            };
        }

    public static ushort ToUInt16(Rgb color)
    {
            return (ushort)((color.Red >> 3)
                | ((color.Green >> 3) << 5)
                | ((color.Blue >> 3) << 10));
        }

    public Rgb Decode(Stream stream)
    {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            ushort data = (ushort)(stream.ReadByte() | (stream.ReadByte() << 8));
            return FromUInt16(data);
        }

    public Rgb[] Decode(Stream stream, int numColors)
    {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            if (numColors < 0)
                throw new ArgumentOutOfRangeException(nameof(numColors), numColors, "Invalid arg");

            byte[] buffer = new byte[numColors * BytesPerColor];
            int read = stream.Read(buffer, 0, buffer.Length);
            if (read != buffer.Length) {
                throw new EndOfStreamException();
            }

            return Decode(buffer);
        }

    public Rgb[] Decode(Span<byte> data)
    {
            int numColors = data.Length / BytesPerColor;
            var colors = new Rgb[numColors];
            for (int i = 0; i < numColors; i++) {
                ushort value = (ushort)(data[i * 2] | (data[(i * 2) + 1] << 8));
                colors[i] = FromUInt16(value);
            }

            return colors;
        }

    public byte[] Encode(Rgb color)
    {
            ushort data = ToUInt16(color);
            return [(byte)(data & 0xFF), (byte)(data >> 8)];
        }

    public byte[] Encode(IEnumerable<Rgb> colors)
    {
            if (colors == null)
                throw new ArgumentNullException(nameof(colors));

            var colorList = colors.ToArray();
            byte[] buffer = new byte[colorList.Length * 2];

            for (int i = 0; i < colorList.Length; i++) {
                ushort data = ToUInt16(colorList[i]);
                buffer[i * 2] = (byte)(data & 0xFF);
                buffer[(i * 2) + 1] = (byte)(data >> 8);
            }

            return buffer;
        }
}
