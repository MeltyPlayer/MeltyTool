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
namespace Texim.Pixels;

using System;
using System.Drawing;

public static class TileExtensions
{
    public static bool HasEquivalentIndexes(this Memory<IndexedPixel> tile1, Memory<IndexedPixel> tile2)
    {
            return tile1.Span.HasEquivalentIndexes(tile2.Span);
        }

    public static bool HasEquivalentIndexes(this Span<IndexedPixel> tile1, Span<IndexedPixel> tile2)
    {
            if (tile1.Length != tile2.Length) {
                return false;
            }

            for (int i = 0; i < tile1.Length; i++) {
                if (tile1[i].Index != tile2[i].Index) {
                    return false;
                }
            }

            return true;
        }

    public static void FlipHorizontal(this Memory<IndexedPixel> tile, Size tileSize)
    {
            tile.Span.FlipHorizontal(tileSize);
        }

    public static void FlipHorizontal(this Span<IndexedPixel> tile, Size tileSize)
    {
            for (int y = 0; y < tileSize.Height; y++) {
                for (int x = 0; x < tileSize.Width / 2; x++) {
                    int t1 = (y * tileSize.Width) + x;
                    int t2 = (y * tileSize.Width) + (tileSize.Width - 1 - x);

                    IndexedPixel swap = tile[t1];
                    tile[t1] = tile[t2];
                    tile[t2] = swap;
                }
            }
        }

    public static void FlipVertical(this Memory<IndexedPixel> tile, Size tileSize)
    {
            tile.Span.FlipVertical(tileSize);
        }

    public static void FlipVertical(this Span<IndexedPixel> tile, Size tileSize)
    {
            for (int x = 0; x < tileSize.Width; x++) {
                for (int y = 0; y < tileSize.Height / 2; y++) {
                    int t1 = x + (tileSize.Width * y);
                    int t2 = x + (tileSize.Width * (tileSize.Height - 1 - y));

                    IndexedPixel swap = tile[t1];
                    tile[t1] = tile[t2];
                    tile[t2] = swap;
                }
            }
        }
}
