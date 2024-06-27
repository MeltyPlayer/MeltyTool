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
namespace Texim.Formats
{
    using System;
    using Colors;
    using Palettes;
    using Yarhl.FileFormat;
    using Yarhl.IO;

    public class RawBinary2PaletteCollection :
        IInitializer<RawPaletteParams>, IConverter<BinaryFormat, PaletteCollection>
    {
        private RawPaletteParams parameters = RawPaletteParams.Default;

        public void Initialize(RawPaletteParams parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            this.parameters = parameters;
        }

        public PaletteCollection Convert(BinaryFormat source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var reader = new DataReader(source.Stream);
            source.Stream.Position = parameters.Offset;

            int size = parameters.Size > 0
                ? parameters.Size
                : (int)(source.Stream.Length - parameters.Offset);
            var data = reader.ReadBytes(size);

            var colors = parameters.ColorEncoding.Decode(data).AsSpan();
            if (parameters.ColorsPerPalette <= 0) {
                return new PaletteCollection(new Palette(colors.ToArray()));
            }

            var collection = new PaletteCollection();
            for (int i = 0; i < colors.Length; i += parameters.ColorsPerPalette) {
                Rgb[] paletteColors;
                if (i + parameters.ColorsPerPalette >= colors.Length) {
                    // Take the rest (there may be more than in the other palettes).
                    paletteColors = colors.Slice(i).ToArray();
                } else {
                    paletteColors = colors.Slice(i, parameters.ColorsPerPalette).ToArray();
                }

                var palette = new Palette(paletteColors);
                collection.Palettes.Add(palette);
            }

            return collection;
        }
    }
}
