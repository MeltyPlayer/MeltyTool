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
    using Titles;
    using Yarhl.FileFormat;
    using Yarhl.FileSystem;
    using Yarhl.IO;

    /// <summary>
    /// Converter for binary CIA streams into node containers.
    /// </summary>
    public class BinaryCia2NodeContainer : IConverter<BinaryFormat, NodeContainerFormat>
    {
        const int BlockSize = 64;

        /// <summary>
        /// Converts a binary CIA format into a container.
        /// </summary>
        /// <param name="source">The binary CIA format.</param>
        /// <returns>The container with the CIA content.</returns>
        public NodeContainerFormat Convert(BinaryFormat source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var stream = source.Stream;
            Header header = ReadHeader(stream);

            var container = new NodeContainerFormat();

            // These are for reference, they shouldn't be other than 0 in 3DS.
            container.Root.Tags["version"] = header.Version;
            container.Root.Tags["type"] = header.Type;

            long certsOffset = header.HeaderSize.Pad(BlockSize);
            Node certs = NodeFactory.FromSubstream(
                "certs_chain",
                stream,
                certsOffset,
                header.CertificatesLength);

            long ticketOffset = (certsOffset + header.CertificatesLength).Pad(BlockSize);
            Node ticket = NodeFactory.FromSubstream(
                "ticket",
                stream,
                ticketOffset,
                header.TicketLength);

            long tmdOffset = (ticketOffset + header.TicketLength).Pad(BlockSize);
            Node title = NodeFactory.FromSubstream(
                "title",
                stream,
                tmdOffset,
                header.TitleMetaLength);

            long contentOffset = (tmdOffset + header.TitleMetaLength).Pad(BlockSize);
            Node content = UnpackContent(title, stream, contentOffset);

            long metaOffset = (contentOffset + header.ContentLength).Pad(BlockSize);
            Node metadata = NodeFactory.FromSubstream(
                "metadata",
                stream,
                metaOffset,
                header.MetaLength);

            AddNodes(container.Root, certs, ticket, title, content, metadata);
            return container;
        }

        static Header ReadHeader(DataStream stream)
        {
            // Read header except content index array.
            // We don't need it as it's a bit array with a bit set for each
            // number of content (i.e. 0xC0 means two contents).
            var reader = new DataReader(stream);
            Header header = new Header {
                HeaderSize = reader.ReadUInt32(),
                Type = reader.ReadUInt16(),
                Version = reader.ReadUInt16(),
                CertificatesLength = reader.ReadUInt32(),
                TicketLength = reader.ReadUInt32(),
                TitleMetaLength = reader.ReadUInt32(),
                MetaLength = reader.ReadUInt32(),
                ContentLength = reader.ReadInt64(),
            };

            if (header.Type != 0)
                throw new FormatException($"Invalid type: '{header.Type}'");

            if (header.Version != 0)
                throw new FormatException($"Unsupported version: '{header.Version}'");

            return header;
        }

        static Node UnpackContent(Node titleNode, DataStream stream, long offset)
        {
            Node content = NodeFactory.CreateContainer("content");

            var title = (TitleMetadata)ConvertFormat.With<Binary2TitleMetadata>(titleNode.Format);
            foreach (var chunk in title.Chunks) {
                var chunkNode = NodeFactory.FromSubstream(
                    chunk.GetChunkName(),
                    stream,
                    offset,
                    chunk.Size);

                if (chunk.Attributes.HasFlag(ContentAttributes.Encrypted)) {
                    chunkNode.Tags["LEMON_NCCH_ENCRYPTED"] = true;
                }

                content.Add(chunkNode);
                offset += chunk.Size;
            }

            return content;
        }

        static void AddNodes(Node parent, params Node[] children)
        {
            foreach (var child in children) {
                parent.Add(child);
            }
        }

        private struct Header
        {
            public uint HeaderSize { get; set; }

            public ushort Type { get; set; }

            public ushort Version { get; set; }

            public uint CertificatesLength { get; set; }

            public uint TicketLength { get; set; }

            public uint TitleMetaLength { get; set; }

            public uint MetaLength { get; set; }

            public long ContentLength { get; set; }
        }
    }
}
