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
namespace Yarhl.FileSystem;

using System;
using System.Linq;
using FileFormat;

/// <summary>
/// Node container format for unpack / pack files.
/// </summary>
public class NodeContainerFormat : IDisposable, ICloneableFormat
{
    bool manageRoot;

    /// <summary>
    /// Initializes a new instance of the <see cref="NodeContainerFormat"/>
    /// class.
    /// </summary>
    public NodeContainerFormat()
    {
            Root = new Node("NodeContainerRoot");
            manageRoot = true;
        }

    /// <summary>
    /// Gets the root node containing the children.
    /// </summary>
    public Node Root {
        get;
        private set;
    }

    /// <summary>
    /// Gets a value indicating whether this <see cref="NodeContainerFormat"/>
    /// is disposed.
    /// </summary>
    public bool Disposed {
        get;
        private set;
    }

    /// <summary>
    /// Moves the children from this format to a <see cref="Node"/>.
    /// </summary>
    /// <remarks>
    /// <para>The node will handle the lifecycle of the children.
    /// Disposing the format won't dispose the children.
    /// It will replace nodes with the same name.</para>
    /// </remarks>
    /// <param name="newNode">Node that will contain the children.</param>
    /// <param name="mergeContainers">If set to <see langword="true" /> it will merge container nodes with the same name.</param>
    public void MoveChildrenTo(Node newNode, bool mergeContainers = false)
    {
            if (Disposed)
                throw new ObjectDisposedException(nameof(NodeContainerFormat));

            if (newNode == null)
                throw new ArgumentNullException(nameof(newNode));

            if (!mergeContainers || !newNode.IsContainer) {
                newNode.Add(Root.Children);
            } else {
                for (int i = Root.Children.Count - 1; i >= 0; i--) {
                    Node child = Root.Children[i];
                    Node? foundNode = newNode.Children.FirstOrDefault(node => node.Name == child.Name);

                    if (foundNode != null && child.Format is NodeContainerFormat childFormat) {
                        childFormat.MoveChildrenTo(foundNode, true);
                    } else {
                        _ = Root.Remove(child);
                        newNode.Add(child);
                    }
                }
            }

            Root.RemoveChildren(false);
            Root = newNode;
            manageRoot = false;
        }

    /// <inheritdoc />
    public virtual object DeepClone()
    {
            var newFormat = new NodeContainerFormat();

            // Just copy the first generation children.
            // The rest will be copied recursively if necessary.
            foreach (Node node in Root.Children) {
                var newNode = new Node(node);
                newFormat.Root.Add(newNode);
            }

            return newFormat;
        }

    /// <summary>
    /// Releases all resource used by the <see cref="NodeContainerFormat"/> object.
    /// </summary>
    public void Dispose()
    {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    /// <summary>
    /// Releases all resource used by the
    /// <see cref="NodeContainerFormat"/> object.
    /// </summary>
    /// <param name="disposing">
    /// If set to <see langword="true" /> free managed resources also.
    /// It happens from Dispose() calls.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
            if (Disposed)
                return;

            Disposed = true;
            if (disposing && manageRoot) {
                Root.Dispose();
            }
        }
}
