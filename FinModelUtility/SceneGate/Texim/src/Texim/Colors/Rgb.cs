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

using System.Drawing;

public readonly struct Rgb
{
    public Rgb(Color color)
    {
            Alpha = color.A;
            Red = color.R;
            Green = color.G;
            Blue = color.B;
        }

    public Rgb(SixLabors.ImageSharp.PixelFormats.Rgba32 color)
    {
            Alpha = color.A;
            Red = color.R;
            Green = color.G;
            Blue = color.B;
        }

    public Rgb(Rgb color, byte alpha)
    {
            Alpha = alpha;
            Red = color.Red;
            Green = color.Green;
            Blue = color.Blue;
        }

    public Rgb(byte red, byte green, byte blue)
    {
            Alpha = 255;
            Red = red;
            Green = green;
            Blue = blue;
        }

    public Rgb(byte red, byte green, byte blue, byte alpha)
    {
            Alpha = alpha;
            Red = red;
            Green = green;
            Blue = blue;
        }

    public byte Red { get; init; }

    public byte Green { get; init; }

    public byte Blue { get; init; }

    public byte Alpha { get; init; }

    public readonly Color ToColor() =>
        Color.FromArgb(Alpha, Red, Green, Blue);

    public readonly SixLabors.ImageSharp.PixelFormats.Rgba32 ToImageSharpColor() =>
        new SixLabors.ImageSharp.PixelFormats.Rgba32(Red, Green, Blue, Alpha);

    public readonly int GetDistanceSquared(Rgb other)
    {
            return ((Red - other.Red) * (Red - other.Red))
                + ((Green - other.Green) * (Green - other.Green))
                + ((Blue - other.Blue) * (Blue - other.Blue));
        }
}
