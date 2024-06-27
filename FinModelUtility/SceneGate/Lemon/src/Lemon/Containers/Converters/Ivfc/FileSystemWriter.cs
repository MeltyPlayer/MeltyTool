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
namespace SceneGate.Lemon.Containers.Converters.Ivfc;

using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Logging;
    using Yarhl.FileSystem;
    using Yarhl.IO;

    /// <summary>
    /// IVFC file system writer.
    /// </summary>
    internal class FileSystemWriter
    {
        const int HeaderSize = 0x28;
        const int DirMetadataSize = 0x18;
        const int FileMetadataSize = 0x20;
        const int HashSize = 4;
        static readonly ILog Logger = LogProvider.GetCurrentClassLogger();
        static readonly Encoding NameEncoding = Encoding.Unicode;

        readonly Node root;

        FileSystemInfo info;
        Section dirHashTable;
        Section dirInfoTable;
        Section fileHashTable;
        Section fileInfoTable;
        Section fileData;
        List<Node> files;

        DataWriter streamWriter;
        DataWriter writer;
        DataReader reader;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileSystemWriter" />
        /// class.
        /// </summary>
        /// <param name="root">The root node with the file system.</param>
        public FileSystemWriter(Node root)
        {
            this.root = root;

            Analyze();
            PrecalculateSize();
        }

        /// <summary>
        /// Gets the total size of the file system.
        /// </summary>
        public long Size => fileData.Offset + fileData.Size;

        /// <summary>
        /// Gets the size of the metadata.
        /// </summary>
        public long MetadataSize {
            get {
                return dirHashTable.Size + dirInfoTable.Size
                    + fileHashTable.Size + fileInfoTable.Size;
            }
        }

        /// <summary>
        /// Writes the file system into a stream.
        /// </summary>
        /// <param name="stream">The stream to write the file system.</param>
        public void Write(DataStream stream)
        {
            dirHashTable.ResetPosition();
            dirInfoTable.ResetPosition();
            fileHashTable.ResetPosition();
            fileInfoTable.ResetPosition();
            fileData.ResetPosition();
            files = [];

            streamWriter = new DataWriter(stream);
            using (var metadataStream = new DataStream())
            {
                writer = new DataWriter(metadataStream);
                reader = new DataReader(metadataStream);

                WriteHeader();

                // Prefill so we can jump between sections
                // Also, unused fields must be at 0xFF
                writer.WriteTimes(0xFF, MetadataSize);

                WriteDirectoryInfo(root, false, true, (uint)dirInfoTable.Offset);
                WriteNode(root, (uint)dirInfoTable.Offset);

                metadataStream.WriteTo(stream);
                streamWriter.WritePadding(0x00, 0x10);
            }

            WriteFiles();
        }

        void Analyze()
        {
            info = new FileSystemInfo();
            info.Directories = 1; // root, no name.

            foreach (var node in Navigator.IterateNodes(root, NavigationMode.BreadthFirst)) {
                if (node.IsContainer) {
                    info.Directories++;
                    info.DirNamesLength += NameEncoding.GetByteCount(node.Name).Pad(0x04);
                } else if (node.Stream == null) {
                    Logger.Error($"Node '{node.Path}' is not a folder or a binary file.");
                } else {
                    info.Files++;
                    info.FileNamesLength += NameEncoding.GetByteCount(node.Name).Pad(0x04);
                    info.FileDataLength += node.Stream.Length.Pad(0x10);
                }
            }

            info.DirectoryHashes = NameHash.CalculateTableLength(info.Directories);
            info.FileHashes = NameHash.CalculateTableLength(info.Files);
        }

        void PrecalculateSize()
        {
            dirHashTable = new Section {
                Offset = HeaderSize,
                Size = HashSize * info.DirectoryHashes,
            };

            dirInfoTable = new Section {
                Offset = dirHashTable.Offset + dirHashTable.Size,
                Size = (DirMetadataSize * info.Directories) + info.DirNamesLength,
            };

            fileHashTable = new Section {
                Offset = dirInfoTable.Offset + dirInfoTable.Size,
                Size = HashSize * info.FileHashes,
            };

            fileInfoTable = new Section {
                Offset = fileHashTable.Offset + fileHashTable.Size,
                Size = (FileMetadataSize * info.Files) + info.FileNamesLength,
            };

            fileData = new Section {
                Offset = (fileInfoTable.Offset + fileInfoTable.Size).Pad(0x10),
                Size = info.FileDataLength,
            };
        }

        void WriteFiles()
        {
            foreach (var file in files)
            {
                file.Stream.WriteTo(streamWriter.Stream);
                streamWriter.WritePadding(0x00, 0x10);
            }
        }

        void WriteNode(Node current, uint currentOffset)
        {
            var subFiles = current.Children.Where(n => !n.IsContainer).ToList();
            for (int i = 0; i < subFiles.Count; i++)
            {
                WriteFileInfo(subFiles[i], i == 0, i + 1 == subFiles.Count, currentOffset);
            }

            var dirs = current.Children.Where(n => n.IsContainer).ToList();
            uint[] offsets = new uint[dirs.Count];
            for (int i = 0; i < dirs.Count; i++)
            {
                offsets[i] = (uint)dirInfoTable.Position;
                WriteDirectoryInfo(dirs[i], i == 0, i + 1 == dirs.Count, currentOffset);
            }

            for (int i = 0; i < dirs.Count; i++)
            {
                WriteNode(dirs[i], offsets[i]);
            }
        }

        void WriteHeader()
        {
            writer.Write(HeaderSize);
            writer.Write((uint)dirHashTable.Offset);
            writer.Write((uint)dirHashTable.Size);
            writer.Write((uint)dirInfoTable.Offset);
            writer.Write((uint)dirInfoTable.Size);
            writer.Write((uint)fileHashTable.Offset);
            writer.Write((uint)fileHashTable.Size);
            writer.Write((uint)fileInfoTable.Offset);
            writer.Write((uint)fileInfoTable.Size);
            writer.Write((uint)fileData.Offset);
        }

        void WriteDirectoryInfo(Node dir, bool isFirst, bool isLast, uint parentOffset)
        {
            uint currentPosition = (uint)dirInfoTable.Position;
            uint relativeParent = (uint)(parentOffset - dirInfoTable.Offset);
            if (isFirst) {
                writer.Stream.Position = parentOffset + 0x08;
                writer.Write((uint)dirInfoTable.RelativePosition);
            }

            // Calculate the hash from the name and the parent offset.
            // The root node doesn't have name.
            string dirName = dir == root ? string.Empty : dir.Name;
            byte[] name = NameEncoding.GetBytes(dirName);
            uint hash = NameHash.CalculateHash(relativeParent, name);
            int hashIdx = (int)(hash % info.DirectoryHashes);

            // Get the previous hash and overwrite with our hash
            int hashOffset = (int)dirHashTable.Offset + (HashSize * hashIdx);
            reader.Stream.Position = hashOffset;
            uint collisionOffset = reader.ReadUInt32();

            writer.Stream.Position = hashOffset;
            writer.Write((uint)dirInfoTable.RelativePosition);

            // Write our info
            writer.Stream.Position = dirInfoTable.Position;
            writer.Write(relativeParent);
            writer.Write(0xFFFFFFFF); // next sibling later we may refill it
            writer.Write(0xFFFFFFFF); // firsth child later we may refill it
            writer.Write(0xFFFFFFFF); // first file later we may refill it
            writer.Write(collisionOffset);
            writer.Write(name.Length);
            writer.Write(name);
            writer.WritePadding(0x00, 4);

            dirInfoTable.Position = (uint)writer.Stream.Position;

            // If there are siblings, update our offset pointing to the next one.
            if (!isLast) {
                writer.Stream.Position = currentPosition + 0x04;
                writer.Write((uint)dirInfoTable.RelativePosition);
            }
        }

    void WriteFileInfo(Node file, bool isFirst, bool isLast, uint parentOffset)
    {
            uint currentPosition = (uint)fileInfoTable.Position;
            uint relativeParent = (uint)(parentOffset - dirInfoTable.Offset);
            if (isFirst) {
                writer.Stream.Position = parentOffset + 0x0C;
                writer.Write((uint)fileInfoTable.RelativePosition);
            }

            // Calculate the hash from the name and the parent offset.
            byte[] name = NameEncoding.GetBytes(file.Name);
            uint hash = NameHash.CalculateHash(relativeParent, name);
            int hashIdx = (int)(hash % info.FileHashes);

            // Get the previous hash and overwrite with our hash
            int hashOffset = (int)fileHashTable.Offset + (HashSize * hashIdx);
            reader.Stream.Position = hashOffset;
            uint collisionOffset = reader.ReadUInt32();

            writer.Stream.Position = hashOffset;
            writer.Write((uint)fileInfoTable.RelativePosition);

            // Write our info
            writer.Stream.Position = fileInfoTable.Position;
            writer.Write(relativeParent);
            writer.Write(0xFFFFFFFF); // later we may refill it
            writer.Write(fileData.RelativePosition);
            writer.Write(file.Stream.Length);
            writer.Write(collisionOffset);
            writer.Write(name.Length);
            writer.Write(name);
            writer.WritePadding(0x00, 4);

            fileInfoTable.Position = writer.Stream.Position;

            // If there are siblings, update our offset pointing to the next one.
            if (!isLast) {
                writer.Stream.Position = currentPosition + 0x04;
                writer.Write((uint)fileInfoTable.RelativePosition);
            }

            files.Add(file);
            fileData.Position += file.Stream.Length.Pad(0x10);
        }

    class FileSystemInfo
    {
        public int Directories { get; set; }

        public int DirNamesLength { get; set; }

        public int DirectoryHashes { get; set; }

        public int Files { get; set; }

        public int FileNamesLength { get; set; }

        public int FileHashes { get; set; }

        public long FileDataLength { get; set; }
    }

    class Section
    {
        public long Offset { get; set; }

        public long Position { get; set; }

        public long RelativePosition => Position - Offset;

        public long Size { get; set; }

        public void ResetPosition()
        {
                Position = Offset;
            }
    }
}
