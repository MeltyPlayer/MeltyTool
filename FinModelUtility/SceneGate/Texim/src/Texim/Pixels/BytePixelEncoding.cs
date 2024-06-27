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
    using System.IO;
    using System.Linq;
    using Yarhl.IO;

    public abstract class BytePixelEncoding : IIndexedPixelEncoding
    {
        public abstract int BitsPerPixel { get; }

        public EndiannessMode Endianness { get; set; } = EndiannessMode.LittleEndian;

        public IndexedPixel[] Decode(Stream stream, int numPixels)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            if (numPixels < 0)
                throw new ArgumentOutOfRangeException(nameof(numPixels), numPixels, "Invalid arg");

            int numBytes = numPixels * BitsPerPixel / 8;
            byte[] buffer = new byte[numBytes];
            int read = stream.Read(buffer, 0, buffer.Length);
            if (read != buffer.Length) {
                throw new EndOfStreamException();
            }

            return Decode(buffer);
        }

        public IndexedPixel[] Decode(Span<byte> data)
        {
            int numPixels = data.Length * (8 / BitsPerPixel);
            var pixels = new IndexedPixel[numPixels];

            int mask = (1 << BitsPerPixel) - 1;
            for (int i = 0, bitPos = 0; i < numPixels; i++, bitPos += BitsPerPixel) {
                int endiannessShift = (Endianness == EndiannessMode.LittleEndian)
                    ? (bitPos % 8)
                    : (BitsPerPixel - (bitPos % 8));
                int value = (data[bitPos / 8] >> endiannessShift) & mask;
                pixels[i] = BitsToPixel((byte)value);
            }

            return pixels;
        }

        public byte[] Encode(IEnumerable<IndexedPixel> pixels)
        {
            if (pixels == null)
                throw new ArgumentNullException(nameof(pixels));

            var pixelList = pixels.ToArray();
            int numBytes = pixelList.Length * BitsPerPixel / 8;
            byte[] data = new byte[numBytes];

            for (int i = 0, bitPos = 0; i < pixelList.Length; i++, bitPos += BitsPerPixel) {
                byte pixelData = PixelToBits(pixelList[i]);

                int endiannessShift = (Endianness == EndiannessMode.LittleEndian)
                    ? (bitPos % 8)
                    : (BitsPerPixel - (bitPos % 8));

                byte value = data[bitPos / 8];
                value |= (byte)(pixelData << endiannessShift);
                data[bitPos / 8] = value;
            }

            return data;
        }

        protected abstract IndexedPixel BitsToPixel(byte data);

        protected abstract byte PixelToBits(IndexedPixel pixel);
    }
}
