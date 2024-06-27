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
using System.Linq;
using Formats;
using Yarhl.FileFormat;
using Yarhl.FileSystem;
using Yarhl.IO;

/// <summary>
/// Converter for Binary streams into a NCSD instance.
/// </summary>
public class Binary2Ncsd : IConverter<BinaryFormat, Ncsd>
{
    /// <summary>
    /// Gets the name of a partition from its index.
    /// </summary>
    /// <param name="index">Index of the partition.</param>
    /// <returns>The associated partition's name.</returns>
    public static string GetPartitionName(int index)
    {
            switch (index) {
                case 0:
                    return "program";
                case 1:
                    return "manual";
                case 2:
                    return "download_play";
                case 6:
                    return "new3ds_update";
                case 7:
                    return "update";
                default:
                    throw new FormatException("Unsupported partition");
            }
        }

    /// <summary>
    /// Converts a binary stream into a NCSD instance.
    /// </summary>
    /// <param name="source">Binary stream to convert.</param>
    /// <returns>The new NCSD instance.</returns>
    [SuppressMessage("Reliability", "CA2000", Justification = "Transfer ownership")]
    public Ncsd Convert(BinaryFormat source)
    {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var ncsd = new Ncsd();
            var reader = new DataReader(source.Stream);

            // First read the header
            var header = ncsd.Header;
            header.Signature = reader.ReadBytes(0x100);
            if (reader.ReadString(4) != NcsdHeader.MagicId)
                throw new FormatException("Invalid Magic ID");

            header.Size = reader.ReadUInt32() * NcsdHeader.Unit;
            header.MediaId = reader.ReadUInt64();
            header.FirmwaresType = reader.ReadBytes(Ncsd.NumPartitions)
                .Select(x => (FirmwareType)x)
                .ToArray();
            header.CryptType = reader.ReadBytes(Ncsd.NumPartitions);

            // Now add the subfiles / partitions
            for (int i = 0; i < Ncsd.NumPartitions; i++) {
                long offset = reader.ReadUInt32() * NcsdHeader.Unit;
                long size = reader.ReadUInt32() * NcsdHeader.Unit;
                if (size == 0) {
                    continue;
                }

                string name = (header.FirmwaresType[i] == FirmwareType.None)
                    ? GetPartitionName(i)
                    : $"firm{i}";
                var childBinary = new BinaryFormat(source.Stream, offset, size);
                var child = new Node(name, childBinary);
                ncsd.Root.Add(child);
            }

            // TODO: Read rest of header
            return ncsd;
        }
}
