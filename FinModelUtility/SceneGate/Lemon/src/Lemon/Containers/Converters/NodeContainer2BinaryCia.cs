// Copyright (c) 2020 SceneGate

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
namespace SceneGate.Lemon.Containers.Converters
{
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using Titles;
    using Yarhl.FileFormat;
    using Yarhl.FileSystem;
    using Yarhl.IO;

    /// <summary>
    /// Convert a NodeContainer with CIA sections into binary format.
    /// </summary>
    /// <remarks>
    /// <p>The initialization is optional. If no stream is provided, a new one
    /// will be created on memory. This may consume large RAM memory for big games.</p>
    /// <p>This converter expects to have a node with the following binary
    /// children: certs_chain, ticket, title (TMD), content/program,
    /// content/manual (optional), content/download_play (optional).</p>
    /// <p>This converter expects to receive consistent nodes. This means it expected
    /// that the TMD already has the updated chunks of the content and that the
    /// metadata match the exHeader and ExeFS info.</p>
    /// </remarks>
    public class NodeContainer2BinaryCia :
        IConverter<NodeContainerFormat, BinaryFormat>,
        IInitializer<DataStream>
    {
        const int BlockSize = 64;
        const int ContentIndexSize = 0x2000;
        const int HeaderSize = 0x20 + ContentIndexSize;
        const ushort CiaType = 0; // the type of format, always 0 for 3DS
        const ushort CiaVersion = 0; // the version of the format

        DataStream outputStream;

        /// <summary>
        /// Initializes the converter with the output stream.
        /// </summary>
        /// <param name="parameters">The stream to write the new CIA.</param>
        public void Initialize(DataStream parameters)
        {
            outputStream = parameters;
        }

        /// <summary>
        /// Converts a container into a binary CIA.
        /// </summary>
        /// <param name="source">The container with the CIA files.</param>
        /// <returns>The new binary format with the CIA.</returns>
        public BinaryFormat Convert(NodeContainerFormat source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var binary = (outputStream != null) ? new BinaryFormat(outputStream) : new BinaryFormat();
            binary.Stream.Position = 0;

            var writer = new DataWriter(binary.Stream) {
                Endianness = EndiannessMode.LittleEndian,
            };

            Node root = source.Root;

            if (root.Children["title"].Format is IBinary) {
                root.Children["title"].TransformWith<Binary2TitleMetadata>();
            }

            if (root.Children["title"].Format is not TitleMetadata) {
                throw new FormatException("Unknown format for title node");
            }

            var title = root.Children["title"].GetFormatAs<TitleMetadata>();
            UpdateTitleMetadata(title, root.Children["content"]);

            var binaryConverter = new TitleMetadata2Binary();
            var binaryTitle = binaryConverter.Convert(title);
            root.Children["title"].ChangeFormat(binaryTitle);

            WriteHeader(writer, root);

            WriteFile(writer, root.Children["certs_chain"]);
            WriteFile(writer, root.Children["ticket"]);

            writer.Endianness = EndiannessMode.BigEndian;
            binaryTitle.Stream.WriteTo(writer.Stream);
            writer.Endianness = EndiannessMode.LittleEndian;

            writer.WritePadding(0x00, BlockSize);

            WriteContent(writer, root.Children["content"], root.Children["title"]);
            WriteFile(writer, root.Children["metadata"], false);

            return binary;
        }

        void WriteHeader(DataWriter writer, Node root)
        {
            writer.Write(HeaderSize);
            writer.Write(CiaType);
            writer.Write(CiaVersion);
            writer.Write((uint)(root.Children["certs_chain"]?.Stream?.Length ?? 0));
            writer.Write((uint)(root.Children["ticket"]?.Stream?.Length ?? 0));
            writer.Write((uint)(root.Children["title"]?.Stream?.Length ?? 0));
            writer.Write((uint)(root.Children["metadata"]?.Stream?.Length ?? 0));
            writer.Write(0L); // placeholder for content size
            writer.WriteTimes(0, ContentIndexSize); // placeholder for content index
            writer.WritePadding(0x00, BlockSize);
        }

        void WriteFile(DataWriter writer, Node file, bool isRequired = true)
        {
            if (file == null && isRequired) {
                throw new FileNotFoundException("Missing CIA file");
            }

            if (file.Format is not IBinary) {
                throw new FormatException("Cannot write file as it is not binary");
            }

            file.Stream.WriteTo(writer.Stream);
            writer.WritePadding(0x00, BlockSize);
        }

        void UpdateTitleMetadata(TitleMetadata title, Node content)
        {
            int chunksHashed = 0;
            using var infoRecords = new DataStream();
            var infoRecordsWriter = new DataWriter(infoRecords) {
                Endianness = EndiannessMode.BigEndian,
            };

            for (int i = 0; i < title.Chunks.Count; i++) {
                var chunk = title.Chunks[i];

                string childName = title.Chunks[i].GetChunkName();
                var child = content.Children[childName];
                if (child is null)
                    throw new FormatException($"Missing child: {childName}");
                if (child.Format is not IBinary)
                    throw new FormatException($"Cannot write child {childName} as it is not binary");

                chunk.Size = child.Stream.Length;
                chunk.Hash = SHA256FromStream(child.Stream);
            }

            for (int i = 0; i < title.InfoRecords.Count; i++) {
                if (title.InfoRecords[i].IsEmpty) {
                    infoRecordsWriter.Write(new byte[0x24]);
                    continue;
                }

                var infoRecord = title.InfoRecords[i];

                using var chunksToHash = new DataStream();
                var chunksToHashWriter = new DataWriter(chunksToHash) {
                    Endianness = EndiannessMode.BigEndian,
                };

                for (int k = 0; k < infoRecord.CommandCount; k++) {
                    var chunk = title.Chunks[k + chunksHashed];

                    chunksToHashWriter.Write(chunk.Id);
                    chunksToHashWriter.Write(chunk.Index);

                    int attribute = (int)Enum.Parse(typeof(ContentAttributes), chunk.Attributes.ToString());
                    chunksToHashWriter.Write((short)attribute);

                    chunksToHashWriter.Write(chunk.Size);
                    chunksToHashWriter.Write(chunk.Hash);
                }

                infoRecord.Hash = SHA256FromStream(chunksToHash);
                chunksHashed += infoRecord.CommandCount;

                infoRecordsWriter.Write(infoRecord.IndexOffset);
                infoRecordsWriter.Write(infoRecord.CommandCount);
                infoRecordsWriter.Write(infoRecord.Hash);
            }

            title.Hash = SHA256FromStream(infoRecords);
        }

        byte[] SHA256FromStream(DataStream stream)
        {
            using (SHA256 sha256 = SHA256.Create()) {
                try {
                    stream.Position = 0;
                    return sha256.ComputeHash(stream);
                } catch (Exception ex) {
                    throw new FormatException($"Failed to perform SHA256 during TMD update", ex);
                }
            }
        }

        void WriteContent(DataWriter writer, Node content, Node titleNode)
        {
            if (titleNode == null)
                throw new FormatException("Missing game title");

            var title = (TitleMetadata)ConvertFormat.With<Binary2TitleMetadata>(titleNode.Format);

            int contentBitset = 0;
            long contentSize = 0;
            for (int i = 0; i < title.Chunks.Count; i++) {
                string childName = title.Chunks[i].GetChunkName();
                var child = content.Children[childName];
                if (child is null)
                    throw new FormatException($"Missing child: {childName}");
                if (child.Format is not IBinary)
                    throw new FormatException($"Cannot write child {childName} as it is not binary");

                child.Stream.WriteTo(writer.Stream);
                contentSize += child.Stream.Length;
                contentBitset |= 1 << (7 - i); // one high bit per content
            }

            // Update header values
            writer.Stream.PushToPosition(0x18);
            writer.Write(contentSize);
            writer.Write(contentBitset);
            writer.Stream.PopPosition();
        }
    }
}
