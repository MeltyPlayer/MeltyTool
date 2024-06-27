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
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Filesystem navigator.
    /// Search for nodes and iterate over them.
    /// </summary>
    public static class Navigator
    {
        /// <summary>
        /// Search a node by path.
        /// </summary>
        /// <param name="rootNode">The root node to start the search.</param>
        /// <param name="path">Path to search.</param>
        /// <returns>Node or null if not found.</returns>
        /// <typeparam name="T">NavigableNode type.</typeparam>
        /// <remarks>
        /// <para>If the path starts with the path separator '/', it is
        /// considered to be a full path. Otherwise, it would be a relative
        /// path starting with the node in the argument.</para>
        /// </remarks>
        public static T? SearchNode<T>(T rootNode, string path)
            where T : NavigableNode<T>
        {
            if (rootNode == null)
                throw new ArgumentNullException(nameof(rootNode));

            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            // Absolute path
            if (path.StartsWith(NodeSystem.PathSeparator, StringComparison.Ordinal)) {
                // Path must start the same way
                if (!path.StartsWith(rootNode.Path, StringComparison.Ordinal)) {
                    return null;
                }

                // And then we remove the initial path and search relative.
                path = path.Remove(0, rootNode.Path.Length);
            }

            string[] paths = path.Split(
                [NodeSystem.PathSeparator],
                StringSplitOptions.RemoveEmptyEntries);

            T? currentNode = rootNode;
            foreach (string segment in paths) {
                currentNode = currentNode.Children[segment];
                if (currentNode == null) {
                    return null;
                }
            }

            return currentNode;
        }

        /// <summary>
        /// Iterates the nodes recursively.
        /// </summary>
        /// <param name="rootNode">The root node to start iterating.</param>
        /// <param name="mode">The navigation mode.</param>
        /// <returns>The nodes.</returns>
        /// <typeparam name="T">NavigableNode type.</typeparam>
        public static IEnumerable<T> IterateNodes<T>(
            T rootNode,
            NavigationMode mode = NavigationMode.BreadthFirst)
            where T : NavigableNode<T>
        {
            if (rootNode == null)
                throw new ArgumentNullException(nameof(rootNode));

            if (mode == NavigationMode.BreadthFirst)
                return IterateBreadthFirst(rootNode);
            if (mode == NavigationMode.DepthFirst)
                return IterateDepthFirst(rootNode);
            throw new ArgumentOutOfRangeException(nameof(mode));
        }

        static IEnumerable<T> IterateBreadthFirst<T>(T rootNode)
            where T : NavigableNode<T>
        {
            var queue = new Queue<T>();
            queue.Enqueue(rootNode);

            while (queue.Count > 0) {
                T currentNode = queue.Dequeue();

                foreach (var child in currentNode.Children) {
                    queue.Enqueue(child);
                    yield return child;
                }
            }
        }

        static IEnumerable<T> IterateDepthFirst<T>(T rootNode)
            where T : NavigableNode<T>
        {
            var stack = new Stack<T>(rootNode.Children.Reverse());

            while (stack.Count > 0) {
                T currentNode = stack.Pop();
                yield return currentNode;

                foreach (var child in currentNode.Children.Reverse())
                    stack.Push(child);
            }
        }
    }
}
