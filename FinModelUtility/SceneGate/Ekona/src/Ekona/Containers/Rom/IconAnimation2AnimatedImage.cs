// Copyright(c) 2022 SceneGate
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Drawing;
using System.Linq;
using Texim.Animations;
using Texim.Images;
using Texim.Palettes;
using Texim.Pixels;
using Yarhl.FileFormat;
using Yarhl.FileSystem;

namespace SceneGate.Ekona.Containers.Rom
{
    /// <summary>
    /// Converter for ROM banner animated icons into an animated image.
    /// </summary>
    public class IconAnimation2AnimatedImage : IConverter<NodeContainerFormat, AnimatedFullImage>
    {
        /// <summary>
        /// Convert the 'animated' node from the ROM banner into an animated image.
        /// </summary>
        /// <param name="source">The animated node of the ROM banner.</param>
        /// <returns>A new animated image.</returns>
        /// <exception cref="ArgumentNullException">The argument is null.</exception>
        public AnimatedFullImage Convert(NodeContainerFormat source)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));

            IndexedImage[] bitmaps = source.Root.Children
                .Where(n => n.Name.StartsWith("bitmap"))
                .Select(n => n.GetFormatAs<IndexedImage>())
                .ToArray();
            if (bitmaps.Length == 0) {
                throw new FormatException("Missing bitmaps");
            }

            IPaletteCollection palette = source.Root.Children["palettes"]?.GetFormatAs<IPaletteCollection>()
                ?? throw new FormatException("Missing palettes or wrong format");
            IconAnimationSequence animation = source.Root.Children["animation"]?.GetFormatAs<IconAnimationSequence>()
                ?? throw new FormatException("Missing animation sequence or wrong format");

            var animatedImage = new AnimatedFullImage();
            animatedImage.Loops = 0; // infinite

            var fullImageConverter = new Indexed2FullImage();
            int width = bitmaps[0].Width;
            int height = bitmaps[0].Height;

            foreach (IconAnimationFrame frameInfo in animation.Frames) {
                var framePixels = ((IndexedPixel[])bitmaps[frameInfo.BitmapIndex].Pixels.Clone()).AsSpan();
                if (frameInfo.FlipHorizontal) {
                    framePixels.FlipHorizontal(new Size(width, height));
                }

                if (frameInfo.FlipVertical) {
                    framePixels.FlipVertical(new Size(width, height));
                }

                var frameIndexed = new IndexedImage(width, height, framePixels.ToArray());
                fullImageConverter.Initialize(palette.Palettes[frameInfo.PaletteIndex]);
                FullImage frameImage = fullImageConverter.Convert(frameIndexed);

                var frame = new FullImageFrame {
                    Image = frameImage,
                    Duration = frameInfo.Duration,
                };
                animatedImage.Frames.Add(frame);
            }

            return animatedImage;
        }
    }
}
