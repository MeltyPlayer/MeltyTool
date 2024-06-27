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
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using Images;
    using Pixels;
    using Yarhl.FileFormat;
    using Yarhl.FileSystem;

    public class MapCompression :
        IInitializer<MapCompressionParams>, IConverter<IIndexedImage, NodeContainerFormat>
    {
        private Size tileSize = new Size(8, 8);
        private IIndexedImage mergeImage;

        public void Initialize(MapCompressionParams parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            tileSize = parameters.TileSize;
            mergeImage = parameters.MergeImage;
        }

        public NodeContainerFormat Convert(IIndexedImage source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            // First we need to do tile swizzling
            var swizzling = new TileSwizzling<IndexedPixel>(source.Width);
            var tiles = swizzling.Swizzle(source.Pixels).AsMemory();

            var initialTiles = Memory<IndexedPixel>.Empty;
            if (mergeImage != null) {
                var mergeImageSwizzling = new TileSwizzling<IndexedPixel>(mergeImage.Width);
                initialTiles = mergeImageSwizzling.Swizzle(mergeImage.Pixels).AsMemory();
            }

            int pixelsPerTile = tileSize.Width * tileSize.Height;
            int numTiles = tiles.Length / pixelsPerTile;
            var uniqueTiles = new List<Memory<IndexedPixel>>();

            // Add the initial tiles in case of creating one pixel data + several maps
            for (int i = 0; i < initialTiles.Length; i += pixelsPerTile) {
                var tile = initialTiles.Slice(i, pixelsPerTile);
                uniqueTiles.Add(tile);
            }

            var maps = new MapInfo[numTiles];
            for (int i = 0; i < numTiles; i++) {
                var tile = tiles.Slice(i * pixelsPerTile, pixelsPerTile);

                int repeatedIdx = uniqueTiles.FindIndex(t => t.HasEquivalentIndexes(tile));
                if (repeatedIdx != -1) {
                    maps[i] = new MapInfo {
                        TileIndex = (short)repeatedIdx,
                        HorizontalFlip = false,
                        VerticalFlip = false,
                        PaletteIndex = tile.Span[0].PaletteIndex,
                    };
                    continue;
                }

                tile.FlipHorizontal(tileSize);
                repeatedIdx = uniqueTiles.FindIndex(t => t.HasEquivalentIndexes(tile));
                if (repeatedIdx != -1) {
                    maps[i] = new MapInfo {
                        TileIndex = (short)repeatedIdx,
                        HorizontalFlip = true,
                        VerticalFlip = false,
                        PaletteIndex = tile.Span[0].PaletteIndex,
                    };
                    continue;
                }

                tile.FlipVertical(tileSize);
                repeatedIdx = uniqueTiles.FindIndex(t => t.HasEquivalentIndexes(tile));
                if (repeatedIdx != -1) {
                    maps[i] = new MapInfo {
                        TileIndex = (short)repeatedIdx,
                        HorizontalFlip = true,
                        VerticalFlip = true,
                        PaletteIndex = tile.Span[0].PaletteIndex,
                    };
                    continue;
                }

                tile.FlipHorizontal(tileSize);
                repeatedIdx = uniqueTiles.FindIndex(t => t.HasEquivalentIndexes(tile));
                if (repeatedIdx != -1) {
                    maps[i] = new MapInfo {
                        TileIndex = (short)repeatedIdx,
                        HorizontalFlip = false,
                        VerticalFlip = true,
                        PaletteIndex = tile.Span[0].PaletteIndex,
                    };
                    continue;
                }

                tile.FlipVertical(tileSize);
                maps[i] = new MapInfo {
                    TileIndex = (short)uniqueTiles.Count,
                    HorizontalFlip = false,
                    VerticalFlip = false,
                    PaletteIndex = tile.Span[0].PaletteIndex,
                };
                uniqueTiles.Add(tile);
            }

            // The compressed image now doesn't have any valid size.
            // So we compress with width 8 (one tile) that will work always
            // It also requires to have two types as now the map and image
            // size are different, which is required to swizzle/unswizzle later.
            var uniquePixels = uniqueTiles.SelectMany(t => t.ToArray()).ToArray();
            var unswizzling = new TileSwizzling<IndexedPixel>(tileSize, 8);
            uniquePixels = unswizzling.Unswizzle(uniquePixels);

            var container = new NodeContainerFormat();
            var indexed = new IndexedImage {
                Width = 8,
                Height = uniquePixels.Length / 8,
                Pixels = uniquePixels,
            };
            container.Root.Add(new Node("Pixels", indexed));

            var map = new ScreenMap(source.Width, source.Height) {
                Maps = maps,
            };
            container.Root.Add(new Node("ScreenMap", map));

            return container;
        }
    }
}
