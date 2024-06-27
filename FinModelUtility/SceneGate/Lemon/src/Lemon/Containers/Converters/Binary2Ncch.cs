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
namespace SceneGate.Lemon.Containers.Converters;

using System;
using System.Diagnostics.CodeAnalysis;
using Formats;
using Yarhl.FileFormat;
using Yarhl.FileSystem;
using Yarhl.IO;

/// <summary>
/// Converter for Binary streams into a NCCH instance.
/// </summary>
public class Binary2Ncch : IConverter<BinaryFormat, Ncch>
{
    const int HeaderLength = 0x200;
    const int AccessDescriptorLength = 0x400;

    /// <summary>
    /// Converts a binary stream into a NCCH instance.
    /// </summary>
    /// <param name="source">Binary stream to convert.</param>
    /// <returns>The new NCCH instance.</returns>
    public Ncch Convert(BinaryFormat source)
    {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var ncch = new Ncch();
            var reader = new DataReader(source.Stream);
            reader.Stream.Position = 0;

            // First read the header
            var header = ncch.Header;
            header.Signature = reader.ReadBytes(0x100);
            if (reader.ReadString(4) != NcchHeader.MagicId)
                throw new FormatException("Invalid Magic ID");

            source.Stream.Position = 0x108;
            header.PartitionId = reader.ReadInt64();
            header.MakerCode = reader.ReadInt16();
            header.Version = reader.ReadInt16();

            source.Stream.Position = 0x118;
            header.ProgramId = reader.ReadInt64();

            source.Stream.Position = 0x150;
            header.ProductCode = reader.ReadString(0x10).Replace("\0", string.Empty);

            source.Stream.Position = 0x188;
            byte[] flags = new byte[0x8];
            for (int i = 0; i < flags.Length; i++) {
                flags[i] = reader.ReadByte();
            }

            header.Flags = flags;

            source.Stream.Position = 0x180;
            uint exHeaderLength = reader.ReadUInt32();
            if (exHeaderLength != 0) {
                AddExtendedHeader(ncch.Root, exHeaderLength, source.Stream);
            }

            // Read the subfiles
            source.Stream.Position = 0x190;
            AddChildIfExists("sdk_info.txt", ncch.Root, reader);

            AddChildIfExists("logo.bin", ncch.Root, reader);

            AddChildIfExists("system", ncch.Root, reader);

            header.SystemHashSize = reader.ReadInt32();
            source.Stream.Position += 4; // Reserved

            AddChildIfExists("rom", ncch.Root, reader);

            header.RomHashSize = reader.ReadInt32();

            return ncch;
        }

    static void AddExtendedHeader(Node root, uint length, DataStream baseStream)
    {
            // Extended header is just after the NCCH header
            var extendedHeader = NodeFactory.FromSubstream(
                "extended_header",
                baseStream,
                HeaderLength,
                length);
            root.Add(extendedHeader);

            // Access descriptor is just after the extended header
            var accessDescriptor = NodeFactory.FromSubstream(
                "access_descriptor",
                baseStream,
                HeaderLength + length,
                AccessDescriptorLength);
            root.Add(accessDescriptor);
        }

    [SuppressMessage("Reliability", "CA2000", Justification = "Transfer ownership")]
    static void AddChildIfExists(string name, Node root, DataReader reader)
    {
            BinaryFormat binary = ReadBinaryChild(reader);
            if (binary != null) {
                root.Add(new Node(name, binary));
            }
        }

    static BinaryFormat ReadBinaryChild(DataReader reader)
    {
            long offset = reader.ReadUInt32() * NcchHeader.Unit;
            long size = reader.ReadUInt32() * NcchHeader.Unit;
            if (size == 0) {
                return null;
            }

            return new BinaryFormat(reader.Stream, offset, size);
        }
}
