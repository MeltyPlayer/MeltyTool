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
namespace Yarhl.FileSystem
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    /// <summary>
    /// Read-only collection of NavigableNodes.
    /// </summary>
    /// <typeparam name="T">The implementation of NavigableNodes.</typeparam>
    public class NavigableNodeCollection<T> : ReadOnlyCollection<T>
        where T : NavigableNode<T>
    {
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="Yarhl.FileSystem.NavigableNodeCollection{T}"/> class.
        /// </summary>
        /// <param name="list">Original list of nodes.</param>
        public NavigableNodeCollection(IList<T> list)
            : base(list)
        {
        }

        /// <summary>
        /// Gets the <see cref="Yarhl.FileSystem.NavigableNodeCollection{T}"/>
        /// with the specified name.
        /// </summary>
        /// <param name="name">Node name.</param>
        /// <returns>The node with the same name or null if not found.</returns>
        public T? this[string name] => this.FirstOrDefault(node => node?.Name == name);
    }
}
