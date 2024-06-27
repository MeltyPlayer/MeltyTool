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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using IO;

/// <summary>
/// Node factory.
/// </summary>
public static class NodeFactory
{
    /// <summary>
    /// Creates a new <see cref="Node"/> with a new NodeContainer format.
    /// </summary>
    /// <returns>The new node.</returns>
    /// <param name="name">Node name.</param>
    [SuppressMessage(
        "Reliability",
        "CA2000:Dispose objects before losing scope",
        Justification = "Ownserhip dispose transferred")]
    public static Node CreateContainer(string name)
    {
            return new Node(name, new NodeContainerFormat());
        }

    /// <summary>
    /// Creates the missing parent nodes to contain the child and add it.
    /// </summary>
    /// <param name="root">The root node that will contain the nodes.</param>
    /// <param name="path">
    /// The path for the child. It doesn't contain the root or child names.</param>
    /// <param name="child">The child to add to root with the path.</param>
    public static void CreateContainersForChild(Node root, string path, Node child)
    {
            if (root == null)
                throw new ArgumentNullException(nameof(root));

            if (child == null)
                throw new ArgumentNullException(nameof(child));

            if (path == null)
                throw new ArgumentNullException(nameof(path));

            // Replace wrong slashes to support native Windows paths
            path = path.Replace("\\", NodeSystem.PathSeparator);

            string[] parentNames = path.Split(
                new[] { NodeSystem.PathSeparator[0] },
                StringSplitOptions.RemoveEmptyEntries);

            Node currentNode = root;
            foreach (string name in parentNames) {
                Node? subParent = currentNode.Children[name];
                if (subParent is null) {
                    subParent = CreateContainer(name);
                    currentNode.Add(subParent);
                }

                currentNode = subParent;
            }

            currentNode.Add(child);
        }

    /// <summary>
    /// Creates a Node with a new stream from memory.
    /// </summary>
    /// <param name="name">The name of the node.</param>
    /// <returns>The new node.</returns>
    [SuppressMessage(
        "Reliability",
        "CA2000:Dispose objects before losing scope",
        Justification = "Ownserhip dispose transferred")]
    public static Node FromMemory(string name)
    {
            return new Node(name, new BinaryFormat());
        }

    /// <summary>
    /// Creates a Node with a binary format containing the array.
    /// </summary>
    /// <param name="name">The name of the node.</param>
    /// <param name="data">The data for the binary format.</param>
    /// <returns>The new node.</returns>
    /// <exception cref="ArgumentNullException">The name is null or empty, the data is null.</exception>
    public static Node FromArray(string name, byte[] data)
    {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));
            if (data is null)
                throw new ArgumentNullException(nameof(data));

            var format = new BinaryFormat(DataStreamFactory.FromArray(data));
            return new Node(name, format);
        }

    /// <summary>
    /// Creates a Node with a binary format containing a part of the array.
    /// </summary>
    /// <param name="name">The name of the node.</param>
    /// <param name="data">The data for the binary format.</param>
    /// <param name="offset">The offset to start the data of the node.</param>
    /// <param name="length">The number of bytes for the node's data.</param>
    /// <returns>The new node.</returns>
    /// <exception cref="ArgumentNullException">The name is null or empty, the data is null.</exception>
    public static Node FromArray(string name, byte[] data, int offset, int length)
    {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));
            if (data is null)
                throw new ArgumentNullException(nameof(data));

            var format = new BinaryFormat(DataStreamFactory.FromArray(data, offset, length));
            return new Node(name, format);
        }

    /// <summary>
    /// Creates a Node from a stream.
    /// </summary>
    /// <param name="name">The name of the node.</param>
    /// <param name="stream">The binary stream.</param>
    /// <remarks>
    /// <para>It will take over the ownership of the stream
    /// argument, you should not dispose this stream argument.</para>
    /// </remarks>
    /// <returns>The new node.</returns>
    public static Node FromStream(string name, Stream stream)
    {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));
            if (stream is null)
                throw new ArgumentNullException(nameof(stream));

            var binary = new BinaryFormat(stream);
            return new Node(name, binary);
        }

    /// <summary>
    /// Creates a Node from a part of a stream.
    /// </summary>
    /// <param name="name">The name of the node.</param>
    /// <param name="source">The source / parent stream.</param>
    /// <param name="offset">
    /// The offset in the source stream where the node starts.
    /// </param>
    /// <param name="length">The length of the data in the node.</param>
    /// <remarks>
    /// <para>This format creates an internal <see cref="DataStream" /> from the
    /// provided stream. It will take over the ownership of the stream
    /// argument, you should not dispose this argument, unless you are
    /// providing a <see cref="DataStream" /> that we won't take over in case
    /// you want to create more substreams.</para>
    /// </remarks>
    /// <returns>The new node.</returns>
    [SuppressMessage(
        "Reliability",
        "CA2000:Dispose objects before losing scope",
        Justification = "Ownserhip dispose transferred")]
    public static Node FromSubstream(
        string name,
        Stream source,
        long offset,
        long length)
    {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            var binary = new BinaryFormat(source, offset, length);
            return new Node(name, binary);
        }

    /// <summary>
    /// Creates a Node from a file.
    /// </summary>
    /// <returns>The node.</returns>
    /// <param name="filePath">File path.</param>
    /// <param name="mode">The mode to open the file.</param>
    public static Node FromFile(string filePath, FileOpenMode mode = FileOpenMode.ReadWrite)
    {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));

            string filename = Path.GetFileName(filePath);
            return FromFile(filePath, filename, mode);
        }

    /// <summary>
    /// Creates a Node from a file.
    /// </summary>
    /// <returns>The node.</returns>
    /// <param name="filePath">File path.</param>
    /// <param name="nodeName">Node name.</param>
    /// <param name="mode">The mode to open the file.</param>
    /// <remarks>
    /// <para>Add the tag "FileInfo" with the file info status at the time it's created.</para>
    /// <para>In the case of Windows Symlinks, it will be the status of the link file, not the target.</para>
    /// </remarks>
    [SuppressMessage(
        "Reliability",
        "CA2000:Dispose objects before losing scope",
        Justification = "Ownserhip dispose transferred")]
    public static Node FromFile(string filePath, string nodeName, FileOpenMode mode = FileOpenMode.ReadWrite)
    {
            // We need to catch if the node creation fails
            // for instance for null names, to dispose the stream.
            var format = new BinaryFormat(DataStreamFactory.FromFile(filePath, mode));
            Node node;
            try {
                node = new Node(nodeName, format) {
                    Tags = { ["FileInfo"] = new FileInfo(filePath) },
                };
            } catch {
                format.Dispose();
                throw;
            }

            return node;
        }

    /// <summary>
    /// Creates a Node containing all the files from the directory.
    /// </summary>
    /// <returns>The container node.</returns>
    /// <param name="dirPath">Directory path.</param>
    /// <param name="filter">Filter for files in directory.</param>
    /// <param name="mode">The mode to open the files.</param>
    public static Node FromDirectory(string dirPath, string filter = "*", FileOpenMode mode = FileOpenMode.ReadWrite)
    {
            if (string.IsNullOrEmpty(dirPath))
                throw new ArgumentNullException(nameof(dirPath));

            // This sanitizes the path and remove double slashes
            dirPath = Path.GetFullPath(dirPath);

            if (dirPath[^1] == Path.DirectorySeparatorChar) {
                dirPath = dirPath.Remove(dirPath.Length - 1);
            }

            string dirName = Path.GetFileName(dirPath);
            return FromDirectory(dirPath, filter, dirName, false, mode);
        }

    /// <summary>
    /// Creates a Node containing all the files from the directory.
    /// </summary>
    /// <returns>The container node.</returns>
    /// <param name="dirPath">Directory path.</param>
    /// <param name="filter">Filter for files in directory.</param>
    /// <param name="nodeName">Node name.</param>
    /// <param name="subDirectories">
    /// If <see langword="true" /> it searchs recursively in subdirectories.
    /// </param>
    /// <param name="mode">The mode to open the files.</param>
    [SuppressMessage(
        "Reliability",
        "CA2000:Dispose objects before losing scope",
        Justification = "Ownserhip dispose transferred")]
    public static Node FromDirectory(
        string dirPath,
        string filter,
        string nodeName,
        bool subDirectories = false,
        FileOpenMode mode = FileOpenMode.ReadWrite)
    {
            if (string.IsNullOrEmpty(dirPath))
                throw new ArgumentNullException(nameof(dirPath));

            if (string.IsNullOrEmpty(filter))
                throw new ArgumentNullException(nameof(filter));

            if (string.IsNullOrEmpty(nodeName))
                throw new ArgumentNullException(nameof(nodeName));

            SearchOption options = subDirectories ?
                SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            // This sanitizes the path and remove double slashes
            dirPath = Path.GetFullPath(dirPath);

            if (dirPath[^1] == Path.DirectorySeparatorChar) {
                dirPath = dirPath.Remove(dirPath.Length - 1);
            }

            string[] fileList = Directory.GetFiles(dirPath, filter, options);
            return FromFileList(dirPath, nodeName, fileList, mode);
        }

    /// <summary>
    /// Creates a Node containing all the files from the directory.
    /// </summary>
    /// <returns>The container node.</returns>
    /// <param name="dirPath">Directory path.</param>
    /// <param name="filter">Filter for files in directory.</param>
    /// <param name="mode">The mode to open the files.</param>
    public static Node FromDirectory(string dirPath, Func<string, bool> filter, FileOpenMode mode = FileOpenMode.ReadWrite)
    {
            if (string.IsNullOrEmpty(dirPath))
                throw new ArgumentNullException(nameof(dirPath));

            if (filter == null)
                throw new ArgumentNullException(nameof(filter));

            // This sanitizes the path and remove double slashes
            dirPath = Path.GetFullPath(dirPath);

            if (dirPath[^1] == Path.DirectorySeparatorChar) {
                dirPath = dirPath.Remove(dirPath.Length - 1);
            }

            string dirName = Path.GetFileName(dirPath);
            return FromDirectory(dirPath, filter, dirName, false, mode);
        }

    /// <summary>
    /// Creates a Node containing all the files from the directory.
    /// </summary>
    /// <returns>The container node.</returns>
    /// <param name="dirPath">Directory path.</param>
    /// <param name="filter">Filter for files in directory.</param>
    /// <param name="nodeName">Node name.</param>
    /// <param name="subDirectories">
    /// If <see langword="true" /> it searchs recursively in subdirectories.
    /// </param>
    /// <param name="mode">The mode to open the files.</param>
    [SuppressMessage(
        "Reliability",
        "CA2000:Dispose objects before losing scope",
        Justification = "Ownserhip dispose transferred")]
    public static Node FromDirectory(
        string dirPath,
        Func<string, bool> filter,
        string nodeName,
        bool subDirectories = false,
        FileOpenMode mode = FileOpenMode.ReadWrite)
    {
            if (string.IsNullOrEmpty(dirPath))
                throw new ArgumentNullException(nameof(dirPath));

            if (filter == null)
                throw new ArgumentNullException(nameof(filter));

            if (string.IsNullOrEmpty(nodeName))
                throw new ArgumentNullException(nameof(nodeName));

            SearchOption options = subDirectories ?
                SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            // This sanitizes the path and remove double slashes
            dirPath = Path.GetFullPath(dirPath);

            if (dirPath[^1] == Path.DirectorySeparatorChar) {
                dirPath = dirPath.Remove(dirPath.Length - 1);
            }

            string[] allFiles = Directory.GetFiles(dirPath, "*", options);
            string[] fileList = Array.FindAll(allFiles, x => filter(x));
            return FromFileList(dirPath, nodeName, fileList, mode);
        }

    private static Node FromFileList(string dirPath, string nodeName, IEnumerable<string> fileList, FileOpenMode mode = FileOpenMode.ReadWrite)
    {
            Node folder = CreateContainer(nodeName);
            folder.Tags["DirectoryInfo"] = new DirectoryInfo(dirPath);

            foreach (string filePath in fileList) {
                string relParent = Path.GetDirectoryName(filePath)?
                                       .Replace(dirPath, string.Empty) ?? string.Empty;
                CreateContainersForChild(folder, relParent, FromFile(filePath, mode));
            }

            foreach (Node node in Navigator.IterateNodes(folder)) {
                if (!node.IsContainer || node.Tags.ContainsKey("DirectoryInfo")) {
                    continue;
                }

                int rootPathLength = $"{NodeSystem.PathSeparator}{nodeName}".Length;
                string nodePath = Path.GetFullPath(string.Concat(dirPath, node.Path[rootPathLength..]));
                node.Tags["DirectoryInfo"] = new DirectoryInfo(nodePath);
            }

            return folder;
        }
}
