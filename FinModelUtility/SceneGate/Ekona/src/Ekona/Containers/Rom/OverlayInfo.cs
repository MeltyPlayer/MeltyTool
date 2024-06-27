// Copyright(c) 2021 SceneGate
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
namespace SceneGate.Ekona.Containers.Rom
{
    /// <summary>
    /// Information about a program overlay.
    /// </summary>
    public class OverlayInfo
    {
        /// <summary>
        /// Gets or sets the ID of the overlay.
        /// </summary>
        public uint OverlayId { get; set; }

        /// <summary>
        /// Gets or sets the address where the overlay will be load in the RAM.
        /// </summary>
        public uint RamAddress { get; set; }

        /// <summary>
        /// Gets or sets the amount of bytes to load in RAM of the overlay.
        /// </summary>
        public uint RamSize { get; set; }

        /// <summary>
        /// Gets or sets the size of the BSS data region.
        /// </summary>
        public uint BssSize { get; set; }

        /// <summary>
        /// Gets or sets the static initialization start address.
        /// </summary>
        public uint StaticInitStart { get; set; }

        /// <summary>
        /// Gets or sets the static initialization end address.
        /// </summary>
        public uint StaticInitEnd { get; set; }

        /// <summary>
        /// Gets or sets the size of the overlay compressed or 0 if it is not compressed.
        /// </summary>
        public uint CompressedSize { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the overlay is BLZ compressed.
        /// </summary>
        public bool IsCompressed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this overlay is digitally signed.
        /// </summary>
        /// <remarks>
        /// The signature seems to be validated only when executing a program
        /// recived from other devices.
        /// </remarks>
        public bool IsSigned { get; set; }
    }
}
