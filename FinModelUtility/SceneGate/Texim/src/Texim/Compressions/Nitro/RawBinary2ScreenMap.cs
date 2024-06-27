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
    using Yarhl.FileFormat;
    using Yarhl.IO;

    public class RawBinary2ScreenMap :
        IInitializer<RawScreenMapParams>, IConverter<IBinary, ScreenMap>
    {
        private RawScreenMapParams parameters = RawScreenMapParams.Default;

        public void Initialize(RawScreenMapParams parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            this.parameters = parameters;
        }

        public ScreenMap Convert(IBinary source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            int numMaps = parameters.Size / 2;
            if (numMaps <= 0) {
                numMaps = (parameters.Width > 0 && parameters.Height > 0)
                    ? (parameters.Width * parameters.Height / 64)
                    : (int)(source.Stream.Length - parameters.Offset) / 2;
            }

            source.Stream.Position = parameters.Offset;
            var reader = new DataReader(source.Stream);
            MapInfo[] maps = reader.ReadMapInfos(numMaps);

            int width = parameters.Width;
            if (width <= 0) {
                width = 256;
            }

            int height = parameters.Height;
            if (height <= 0) {
                height = (int)Math.Ceiling((double)numMaps * 64 / width);
            }

            return new ScreenMap {
                Width = width,
                Height = height,
                Maps = maps,
            };
        }
    }
}
