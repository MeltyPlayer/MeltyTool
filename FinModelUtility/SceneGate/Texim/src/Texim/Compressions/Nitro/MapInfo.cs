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
    public readonly struct MapInfo
    {
        public MapInfo(byte value)
        {
            TileIndex = value;
            HorizontalFlip = false;
            VerticalFlip = false;
            PaletteIndex = 0;
        }

        public MapInfo(ushort value)
        {
            TileIndex = (short)(value & 0x3FF);
            HorizontalFlip = ((value >> 10) & 0x01) == 1;
            VerticalFlip = ((value >> 11) & 0x01) == 1;
            PaletteIndex = (byte)(value >> 12);
        }

        public MapInfo(short value)
            : this((ushort)value)
        {
        }

        public short TileIndex { get; init; }

        public bool HorizontalFlip { get; init; }

        public bool VerticalFlip { get; init; }

        public byte PaletteIndex { get; init; }

        public readonly short ToInt16()
        {
            return (short)((TileIndex & 0x3FF)
                | ((HorizontalFlip ? 1 : 0) << 10)
                | ((VerticalFlip ? 1 : 0) << 11)
                | ((PaletteIndex & 0x0F) << 12));
        }
    }
}
