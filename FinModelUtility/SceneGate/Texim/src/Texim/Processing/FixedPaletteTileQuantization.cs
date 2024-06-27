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
namespace Texim.Processing
{
    using System;
    using System.Drawing;
    using System.Linq;
    using Colors;
    using Palettes;
    using Pixels;

    public class FixedPaletteTileQuantization : IQuantization
    {
        private readonly IPaletteCollection paletteCollection;
        private readonly Rgb[][] palettes;
        private readonly Size tileSize;
        private readonly int width;

        public FixedPaletteTileQuantization(IPaletteCollection palettes, Size tileSize, int width)
        {
            if (palettes == null)
                throw new ArgumentNullException(nameof(palettes));

            this.paletteCollection = palettes;
            this.palettes = palettes.Palettes.Select(p => p.Colors.ToArray()).ToArray();
            this.width = width;
            this.tileSize = tileSize;
        }

        public bool FirstAsTransparent { get; set; }

        public (IndexedPixel[], IPaletteCollection) Quantize(Rgb[] pixels)
        {
            // Swizzle to work with tiles
            var colorSwizzling = new TileSwizzling<Rgb>(tileSize, width);
            Rgb[] tiles = colorSwizzling.Swizzle(pixels);

            IndexedPixel[] indexed = new IndexedPixel[pixels.Length];
            Span<IndexedPixel> output = indexed;
            ReadOnlySpan<Rgb> input = tiles;

            int tileLength = tileSize.Width * tileSize.Height;
            for (int i = 0; i < pixels.Length; i += tileLength) {
                var tileIn = input.Slice(i, tileLength);
                var tileOut = output.Slice(i, tileLength);
                int paletteIdx = SearchNearestPalette(tileIn);
                ApproximateTile(tileIn, paletteIdx, tileOut);
            }

            // Unswizzle to return
            var indexSwizzling = new TileSwizzling<IndexedPixel>(tileSize, width);
            return (indexSwizzling.Unswizzle(indexed), paletteCollection);
        }

        private void ApproximateTile(ReadOnlySpan<Rgb> tile, int paletteIdx, Span<IndexedPixel> output)
        {
            for (int i = 0; i < tile.Length; i++) {
                int colorIdx = (FirstAsTransparent && tile[i].Alpha <= 128)
                    ? 0
                    : ExhaustiveColorSearch.Search(palettes[paletteIdx], tile[i]).Index;
                output[i] = new IndexedPixel((short)colorIdx, tile[i].Alpha, (byte)paletteIdx);
            }
        }

        private int SearchNearestPalette(ReadOnlySpan<Rgb> tile)
        {
            int minDistance = int.MaxValue;
            int nearestPalette = -1;

            for (int i = 0; i < palettes.Length; i++) {
                int distance = GetTilePaletteDistance(tile, palettes[i]);
                if (distance < minDistance) {
                    minDistance = distance;
                    nearestPalette = i;
                }
            }

            return nearestPalette;
        }

        private int GetTilePaletteDistance(ReadOnlySpan<Rgb> tile, Rgb[] palette)
        {
            int totalDistance = 0;
            for (int i = 0; i < tile.Length; i++) {
                totalDistance += ExhaustiveColorSearch.Search(palette, tile[i]).Distance;
            }

            return totalDistance;
        }
    }
}
