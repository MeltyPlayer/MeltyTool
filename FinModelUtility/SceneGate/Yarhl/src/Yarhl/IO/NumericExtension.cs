// Copyright (c) 2019 SceneGate

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
namespace Yarhl.IO
{
    using System;

    /// <summary>
    /// Extension methods for numeric types.
    /// </summary>
    public static class NumericExtension
    {
        /// <summary>
        /// Pad the specified address.
        /// </summary>
        /// <returns>The address padded.</returns>
        /// <param name="address">Address to pad.</param>
        /// <param name="padding">Padding target.</param>
        public static short Pad(this short address, int padding)
        {
            return (short)Pad((ulong)address, (ulong)padding);
        }

        /// <summary>
        /// Pad the specified address.
        /// </summary>
        /// <returns>The address padded.</returns>
        /// <param name="address">Address to pad.</param>
        /// <param name="padding">Padding target.</param>
        [CLSCompliant(false)]
        public static ushort Pad(this ushort address, int padding)
        {
            return (ushort)Pad(address, (ulong)padding);
        }

        /// <summary>
        /// Pad the specified address.
        /// </summary>
        /// <returns>The address padded.</returns>
        /// <param name="address">Address to pad.</param>
        /// <param name="padding">Padding target.</param>
        public static int Pad(this int address, int padding)
        {
            return (int)Pad((ulong)address, (ulong)padding);
        }

        /// <summary>
        /// Pad the specified address.
        /// </summary>
        /// <returns>The address padded.</returns>
        /// <param name="address">Address to pad.</param>
        /// <param name="padding">Padding target.</param>
        [CLSCompliant(false)]
        public static uint Pad(this uint address, int padding)
        {
            return (uint)Pad(address, (ulong)padding);
        }

        /// <summary>
        /// Pad the specified address.
        /// </summary>
        /// <returns>The address padded.</returns>
        /// <param name="address">Address to pad.</param>
        /// <param name="padding">Padding target.</param>
        public static long Pad(this long address, long padding)
        {
            return (long)Pad((ulong)address, (ulong)padding);
        }

        /// <summary>
        /// Pad the specified address.
        /// </summary>
        /// <returns>The address padded.</returns>
        /// <param name="address">Address to pad.</param>
        /// <param name="padding">Padding target.</param>
        [CLSCompliant(false)]
        public static ulong Pad(this ulong address, ulong padding)
        {
            return address + ((address % padding == 0) ? 0 : padding - (address % padding));
        }
    }
}
