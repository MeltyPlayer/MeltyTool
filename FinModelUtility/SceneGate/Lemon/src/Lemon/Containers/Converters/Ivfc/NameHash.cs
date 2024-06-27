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
namespace SceneGate.Lemon.Containers.Converters.Ivfc
{
    /// <summary>
    /// Generate hashes for name and directory lookup.
    /// </summary>
    internal static class NameHash
    {
        /// <summary>
        /// Calculates the size of tokens for the hash table.
        /// </summary>
        /// <param name="numEntries">The number of entries to hash.</param>
        /// <returns>The number of tokens of the hash table.</returns>
        public static int CalculateTableLength(int numEntries)
        {
            if (numEntries < 3) {
                return 3;
            } else if (numEntries < 19) {
                // This will return 15 that it is not prime, but whatever Nin.
                return numEntries | 1;
            }

            // Find the first prime number with a simple algorithm.
            // Really lazy algorithm eh, Nin.
            bool IsMoreOrLessPrime(int x) =>
                (x % 2 != 0) && (x % 3 != 0) && (x % 5 != 0) && (x % 7 != 0) &&
                (x % 11 != 0) && (x % 13 != 0) && (x % 17 != 0);

            int count = numEntries;
            while (!IsMoreOrLessPrime(count))
            {
                count++;
            }

            return count;
        }

        /// <summary>
        /// Calculates the hash for an entry.
        /// </summary>
        /// <param name="seed">The seed value.</param>
        /// <param name="name">The name of the entry.</param>
        /// <returns>The calculated hash.</returns>
        public static uint CalculateHash(uint seed, byte[] name)
        {
            uint hash = seed ^ 123456789;
            for (int i = 0; i < name.Length; i += 2) {
                hash = (hash >> 5) | (hash << 27);
                hash ^= (ushort)(name[i] | (name[i + 1] << 8));
            }

            return hash;
        }
    }
}