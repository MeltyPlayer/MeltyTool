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
    using System.Collections.Generic;
    using System.Linq;
    using Colors;
    using Palettes;
    using Pixels;

    public class MedianCutQuantization : IQuantization
    {
        public MedianCutQuantization(int maxColors)
        {
            if (maxColors <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxColors), maxColors, "Cannot be less than 0");
            if ((maxColors & (maxColors - 1)) != 0)
                throw new ArgumentOutOfRangeException(nameof(maxColors), maxColors, "Must be power of 2");

            MaxColors = maxColors;
        }

        public int MaxColors { get; }

        public (IndexedPixel[], IPaletteCollection) Quantize(Rgb[] pixels)
        {
            // Convert to pixel reference to keep track of pixel indexes
            var pixelrefs = new PixelRef[pixels.Length];
            for (int i = 0; i < pixels.Length; i++) {
                pixelrefs[i] = new PixelRef(pixels[i], i);
            }

            // Initialize with all colors in a bucket.
            var buckets = new Queue<PixelRef[]>();
            buckets.Enqueue(pixelrefs);

            // Median cut until we have a bucket per palette color
            while (buckets.Count < MaxColors) {
                PixelRef[] bucket = buckets.Dequeue();
                var (bucket1, bucket2) = MedianCut(bucket);
                buckets.Enqueue(bucket1);
                buckets.Enqueue(bucket2);
            }

            // Average bucket color and create indexes
            var indexed = new IndexedPixel[pixels.Length];
            var palette = new Palette();
            foreach (PixelRef[] bucket in buckets) {
                Rgb color = Average(bucket);

                short paletteIdx = (short)palette.Colors.Count;
                palette.Colors.Add(color);

                foreach (PixelRef pixel in bucket) {
                    indexed[pixel.Index] = new IndexedPixel(paletteIdx);
                }
            }

            return (indexed, new PaletteCollection(palette));
        }

        private (PixelRef[], PixelRef[]) MedianCut(PixelRef[] bucket)
        {
            long rangeRed = bucket.Max(c => c.Color.Red) - bucket.Min(c => c.Color.Red);
            long rangeGreen = bucket.Max(c => c.Color.Green) - bucket.Min(c => c.Color.Green);
            long rangeBlue = bucket.Max(c => c.Color.Blue) - bucket.Min(c => c.Color.Blue);

            PixelRef[] sorted;
            if (rangeRed >= rangeGreen && rangeRed >= rangeBlue) {
                sorted = bucket.OrderBy(c => c.Color.Red).ToArray();
            } else if (rangeGreen >= rangeRed && rangeGreen >= rangeBlue) {
                sorted = bucket.OrderBy(c => c.Color.Green).ToArray();
            } else {
                sorted = bucket.OrderBy(c => c.Color.Blue).ToArray();
            }

            int median = bucket.Length / 2;
            return (sorted[..median], sorted[median..]);
        }

        private Rgb Average(PixelRef[] bucket) =>
            new Rgb(
                (byte)bucket.Average(c => c.Color.Red),
                (byte)bucket.Average(c => c.Color.Green),
                (byte)bucket.Average(c => c.Color.Blue));

        private readonly struct PixelRef
        {
            public PixelRef(Rgb color, int index) => (Color, Index) = (color, index);

            public Rgb Color { get; init; }

            public int Index { get; init; }
        }
    }
}
