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
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using Yarhl.IO;

    public class TileSwizzling<T>(Size tileSize, int width) : ISwizzling<T>
    {
        public TileSwizzling() : this(new Size(8, 8), 256)
        { }

        public TileSwizzling(int width) : this(new Size(8, 8), width)
        { }

        public Size TileSize { get; set; } = tileSize;

        public int Width { get; set; } = width;

        public T[] Swizzle(IEnumerable<T> data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            return Codec(data, false);
        }

        public T[] Unswizzle(IEnumerable<T> data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            return Codec(data, true);
        }

        private T[] Codec(IEnumerable<T> data, bool decoding)
        {
            if (Width % TileSize.Width != 0)
                throw new FormatException("Width must be a multiple of tile width");

            T[] input = data.ToArray();

            // If the image is incomplete there may missing tiles but in order
            // to set some pixels we will need a square image so we can skip
            // the missing pixels.
            int height = (input.Length / Width).Pad(TileSize.Height);
            T[] output = new T[Width * height];
            for (int linealIndex = 0; linealIndex < input.Length; linealIndex++) {
                int tiledIndex = GetTiledIndex(linealIndex % Width, linealIndex / Width);
                if (decoding) {
                    output[linealIndex] = tiledIndex < input.Length ? input[tiledIndex] : default;
                } else {
                    output[tiledIndex] = input[linealIndex];
                }
            }

            return output;
        }

        private int GetTiledIndex(int x, int y)
        {
            int tileLength = TileSize.Width * TileSize.Height;
            int numTilesX = Width / TileSize.Width;

            var pixelPos = new Point(x % TileSize.Width, y % TileSize.Height);
            var tilePos = new Point(x / TileSize.Width, y / TileSize.Height);

            return (tilePos.Y * numTilesX * tileLength) // Start row of tile
                + (tilePos.X * tileLength) // plus start column of tile
                + (pixelPos.Y * TileSize.Width) // plus start row inside tile
                + pixelPos.X; // plus start column inside tile
        }
    }
}
