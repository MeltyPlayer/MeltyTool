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
using System.Collections.Generic;
using FileFormat;
using IO;

/// <summary>
/// Node in the FileSystem with an associated format.
/// </summary>
public class Node : NavigableNode<Node>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Node"/> class.
    /// </summary>
    /// <param name="name">Node name.</param>
    public Node(string name)
        : base(name)
    {
        }

    /// <summary>
    /// Initializes a new instance of the <see cref="Node"/> class.
    /// </summary>
    /// <param name="name">Node name.</param>
    /// <param name="initialFormat">Node format.</param>
    public Node(string name, IFormat initialFormat)
        : this(name)
    {
            ChangeFormat(initialFormat);
        }

    /// <summary>
    /// Initializes a new instance of the <see cref="Node"/> class.
    /// </summary>
    /// <param name="node">The original node.</param>
    /// <remarks><para>It makes a copy of the original node.
    /// The original format is deep copied. See <see cref="ICloneableFormat"/> for details.</para>
    /// </remarks>
    /// <remarks><para>If the node has children, it must be a <see cref="NodeContainerFormat"/> to clone them.
    /// In other case, the format must implement <see cref="ICloneableFormat"/> and clone the children explicitly.
    /// </para></remarks>
    public Node(Node node)
        : this(node != null ? node.Name : string.Empty)
    {
            if (node!.Format != null && node.Format is not ICloneableFormat)
                throw new InvalidOperationException("Format does not implement ICloneableFormat interface.");

            ICloneableFormat? newFormat = null;
            if (node.Format != null) {
                var oldFormat = node.Format as ICloneableFormat;
                newFormat = (ICloneableFormat)oldFormat!.DeepClone();
            }

            ChangeFormat(newFormat);

            foreach (KeyValuePair<string, dynamic> tag in node.Tags)
            {
                Tags[tag.Key] = tag.Value;
            }
        }

    /// <summary>
    /// Gets the current format of the node.
    /// </summary>
    /// <value>The current format.</value>
    public IFormat? Format {
        get;
        private set;
    }

    /// <summary>
    /// Gets the node associated DataStream if the format is IBinary.
    /// </summary>
    /// <value>
    /// DataStream if the format is IBinary, null otherwise.
    /// </value>
    public DataStream? Stream => GetFormatAs<IBinary>()?.Stream;

    /// <summary>
    /// Gets a value indicating whether the format is a container of nodes.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the format is a container; otherwise,
    /// <see langword="false"/>.
    /// </value>
    public bool IsContainer => Format is NodeContainerFormat;

    /// <summary>
    /// Gets the format as the specified type.
    /// </summary>
    /// <returns>The format casted to the type or null if not possible.</returns>
    /// <typeparam name="T">The format type.</typeparam>
    public T? GetFormatAs<T>()
        where T : class, IFormat
    {
            if (Disposed)
                throw new ObjectDisposedException(nameof(Node));

            return Format as T;
        }

    /// <summary>
    /// Change the format of the current node.
    /// </summary>
    /// <remarks>
    /// <para>If the previous format was a container, this method will
    /// remove the children of the node.
    /// If the new format is a container, this method will add the format
    /// children to the node.</para>
    /// <para>If the new format is the same reference as the current format
    /// the method is a no-op.</para>
    /// </remarks>
    /// <param name="newFormat">The new format to assign.</param>
    /// <param name="disposePreviousFormat">
    /// If <see langword="true" /> the method will dispose the previous
    /// format.
    /// </param>
    public void ChangeFormat(IFormat? newFormat, bool disposePreviousFormat = true)
    {
            if (Disposed)
                throw new ObjectDisposedException(nameof(Node));

            if (newFormat == Format) {
                return;
            }

            // If it was a container, clean children
            if (IsContainer) {
                RemoveChildren();
            }

            if (disposePreviousFormat) {
                (Format as IDisposable)?.Dispose();
            }

            Format = newFormat;

            // If now it's a container, add its children
            if (IsContainer) {
                AddContainerChildren();
            }
        }

    /// <summary>
    /// Transforms the node format to the specified format.
    /// </summary>
    /// <typeparam name="TDst">Format to convert.</typeparam>
    /// <returns>This node.</returns>
    public Node TransformTo<TDst>()
        where TDst : IFormat
    {
            if (Disposed)
                throw new ObjectDisposedException(nameof(Node));

            if (Format == null) {
                throw new InvalidOperationException(
                    "Cannot transform a node without format");
            }

            ChangeFormat(ConvertFormat.To<TDst>(Format));
            return this;
        }

    /// <summary>
    /// Transforms the node format to the specified format.
    /// </summary>
    /// <returns>This node.</returns>
    /// <param name="dst">Format to convert. It must implement IFormat.</param>
    public Node TransformTo(Type dst)
    {
            if (Disposed)
                throw new ObjectDisposedException(nameof(Node));

            if (dst == null)
                throw new ArgumentNullException(nameof(dst));

            if (Format == null) {
                throw new InvalidOperationException(
                    "Cannot transform a node without format");
            }

            object result = ConvertFormat.To(dst, Format);
            CastAndChangeFormat(result);

            return this;
        }

    /// <summary>
    /// Transform the node format to another format with a given converter.
    /// </summary>
    /// <returns>This node.</returns>
    /// <typeparam name="TConv">The type of the converter to use.</typeparam>
    public Node TransformWith<TConv>()
        where TConv : IConverter, new()
    {
            if (Disposed)
                throw new ObjectDisposedException(nameof(Node));

            if (Format == null) {
                throw new InvalidOperationException(
                    "Cannot transform a node without format");
            }

            object result = ConvertFormat.With<TConv>(Format);
            CastAndChangeFormat(result);

            return this;
        }

    /// <summary>
    /// Transform the node format to another format with a given converter
    /// initialized with parameters.
    /// </summary>
    /// <returns>This node.</returns>
    /// <typeparam name="TConv">The type of the converter to use.</typeparam>
    /// <typeparam name="TParam">The type for initializing the converter.</typeparam>
    /// <param name="param">Parameters to initialize the converter.</param>
    public Node TransformWith<TConv, TParam>(TParam param)
        where TConv : IConverter, IInitializer<TParam>, new()
    {
            if (Disposed)
                throw new ObjectDisposedException(nameof(Node));

            if (Format == null) {
                throw new InvalidOperationException(
                    "Cannot transform a node without format");
            }

            object result = ConvertFormat.With<TConv, TParam>(param, Format);
            CastAndChangeFormat(result);

            return this;
        }

    /// <summary>
    /// Transforms the node format with the specified converter.
    /// </summary>
    /// <returns>This node.</returns>
    /// <param name="converterType">The type of the converter to use.</param>
    public Node TransformWith(Type converterType)
    {
            if (Disposed)
                throw new ObjectDisposedException(nameof(Node));

            if (converterType == null)
                throw new ArgumentNullException(nameof(converterType));

            if (Format == null) {
                throw new InvalidOperationException(
                    "Cannot transform a node without format");
            }

            object result = ConvertFormat.With(converterType, Format);
            CastAndChangeFormat(result);

            return this;
        }

    /// <summary>
    /// Transform the node format to another format using a converter.
    /// </summary>
    /// <param name="converter">Convert to use.</param>
    /// <typeparam name="TSrc">The type of the source format.</typeparam>
    /// <typeparam name="TDst">The type of the destination format.</typeparam>
    /// <returns>This node.</returns>
    public Node TransformWith<TSrc, TDst>(IConverter<TSrc, TDst> converter)
        where TSrc : IFormat
        where TDst : IFormat
    {
            if (Disposed)
                throw new ObjectDisposedException(nameof(Node));

            if (converter == null)
                throw new ArgumentNullException(nameof(converter));

            if (Format == null) {
                throw new InvalidOperationException(
                    "Cannot transform a node without format");
            }

            ChangeFormat(converter.Convert((TSrc)Format));
            return this;
        }

    /// <summary>
    /// Releases all resource used by the <see cref="Node"/>
    /// object.
    /// </summary>
    /// <param name="freeManagedResourcesAlso">
    /// If set to <see langword="true"/> free managed resources also.
    /// </param>
    protected override void Dispose(bool freeManagedResourcesAlso)
    {
            base.Dispose(freeManagedResourcesAlso);
            if (freeManagedResourcesAlso) {
                (Format as IDisposable)?.Dispose();
            }
        }

    void AddContainerChildren()
    {
            RemoveChildren();
            GetFormatAs<NodeContainerFormat>()?.MoveChildrenTo(this);
        }

    void CastAndChangeFormat(object newFormat)
    {
            if (newFormat == null) {
                // Null may be an acceptable format, for now.
                ChangeFormat(null);
            } else if (newFormat is IFormat format) {
                ChangeFormat(format);
            } else {
                throw new InvalidOperationException(
                    "Result format does not implement the IFormat interface. " +
                    "Cannot assign to the Format property.");
            }
        }
}
