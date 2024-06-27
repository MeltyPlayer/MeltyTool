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
namespace Texim.Compressions.Nitro
{
    using System;
    using System.Drawing;
    using Images;
    using Pixels;
    using Yarhl.FileFormat;

    public class MapDecompression :
        IInitializer<MapDecompressionParams>,
        IConverter<IIndexedImage, IndexedImage>,
        IConverter<IndexedPixel[], IndexedPixel[]>
    {
        private Size tileSize;
        private IScreenMap map;
        private int outOfBoundsIndex = -1;

        public void Initialize(MapDecompressionParams parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            tileSize = parameters.TileSize;
            map = parameters.Map;
            outOfBoundsIndex = parameters.OutOfBoundsTileIndex;
        }

        public IndexedImage Convert(IIndexedImage source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (map == null)
                throw new InvalidOperationException("Missing initialization");

            // It's easier if we swizzle again, with the compressed tiles
            var swizzling = new TileSwizzling<IndexedPixel>(tileSize, source.Width);
            IndexedPixel[] tiles = swizzling.Swizzle(source.Pixels);

            IndexedPixel[] decompressed = DecompressTiles(tiles);

            // Unswizzle but this time with the final image size
            var unswizzling = new TileSwizzling<IndexedPixel>(tileSize, map.Width);
            decompressed = unswizzling.Unswizzle(decompressed);

            return new IndexedImage(map.Width, map.Height, decompressed);
        }

        public IndexedPixel[] Convert(IndexedPixel[] source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (map == null)
                throw new InvalidOperationException("Missing initialization");

            return DecompressTiles(source);
        }

        private IndexedPixel[] DecompressTiles(IndexedPixel[] source)
        {
            int pixelsPerTile = tileSize.Width * tileSize.Height;

            var decompressed = new IndexedPixel[map.Width * map.Height];
            Span<IndexedPixel> pixelsOut = decompressed;
            Span<IndexedPixel> pixelsIn = source;

            for (int i = 0; i < map.Maps.Length; i++) {
                MapInfo info = map.Maps[i];

                int inIndex = info.TileIndex * pixelsPerTile;
                if (inIndex + pixelsPerTile > pixelsIn.Length || inIndex < 0) {
                    if (outOfBoundsIndex == -1) {
                        throw new FormatException($"Required tile index {info.TileIndex} is out of bounds");
                    } else {
                        inIndex = outOfBoundsIndex * pixelsPerTile;
                    }
                }

                Span<IndexedPixel> tileIn = pixelsIn.Slice(inIndex, pixelsPerTile);

                int outIndex = i * pixelsPerTile;
                Span<IndexedPixel> tileOut = pixelsOut.Slice(outIndex, pixelsPerTile);

                for (int t = 0; t < pixelsPerTile; t++) {
                    tileOut[t] = new IndexedPixel(
                        tileIn[t].Index,
                        tileIn[t].Alpha,
                        info.PaletteIndex);
                }

                if (info.HorizontalFlip) {
                    tileOut.FlipHorizontal(tileSize);
                }

                if (info.VerticalFlip) {
                    tileOut.FlipVertical(tileSize);
                }
            }

            return decompressed;
        }
    }
}
