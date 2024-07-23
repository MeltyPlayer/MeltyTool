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
    using System.Collections.Generic;
    using System.Linq;
    using Colors;

    public class ExhaustiveColorSearch(IEnumerable<Rgb> vertex)
    {
        private readonly Rgb[] vertex = vertex.ToArray();

        public static (int Index, int Distance) Search(IEnumerable<Rgb> vertex, Rgb color)
        {
            var vertexArray = (vertex as Rgb[]) ?? vertex.ToArray();

            // Set the largest distance and a null index
            int minDistance = (255 * 255) + (255 * 255) + (255 * 255) + 1;
            int nearestColor = -1;

            // FUTURE: Implement "Approximate Nearest Neighbors in Non-Euclidean Spaces"
            // algorithm or k-d tree if it's computing CIE76 color difference
            for (int i = 0; i < vertexArray.Length && minDistance > 0; i++) {
                // Since we only want the value to compare,
                // it is faster to not computer the squared root
                int distance = color.GetDistanceSquared(vertexArray[i]);
                if (distance < minDistance) {
                    minDistance = distance;
                    nearestColor = i;
                }
            }

            return (nearestColor, minDistance);
        }

        public (int Index, int Distance) Search(Rgb color)
        {
            return Search(vertex, color);
        }
    }
}
