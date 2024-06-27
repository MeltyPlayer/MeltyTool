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
namespace SceneGate.Lemon.Containers
{
    using System;
    using Converters;
    using Yarhl.FileSystem;
    using Yarhl.IO;

    /// <summary>
    /// Manage containers for 3DS formats.
    /// </summary>
    public static class ContainerManager
    {
        /// <summary>
        /// Unpack a node with a binary format representing a .3ds file.
        /// </summary>
        /// <param name="gameNode">Node to unpack with .3ds format.</param>
        public static void Unpack3DSNode(Node gameNode)
        {
            if (gameNode == null)
                throw new ArgumentNullException(nameof(gameNode));

            if (!(gameNode.Format is IBinary))
                throw new ArgumentException("Invalid node format", nameof(gameNode));

            // First transform the binary format into NCSD (CCI/CSU/NAND).
            gameNode.TransformWith<Binary2Ncsd>();

            // All the partition have NCCH format.
            foreach (var partition in gameNode.Children) {
                partition.TransformWith<Binary2Ncch>();
            }

            // Unpack each partition until we have the actual file system.
            gameNode.Children["program"].Children["rom"]
                .TransformWith<BinaryIvfc2NodeContainer>();
            gameNode.Children["program"].Children["system"]
                .TransformWith<BinaryExeFs2NodeContainer>();
        }
    }
}
