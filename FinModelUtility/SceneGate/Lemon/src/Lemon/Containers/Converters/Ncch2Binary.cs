// Ncch2Binary.cs
//
// Author:
//       Maxwell Ruiz maxwellaquaruiz@gmail.com
//
// Copyright (c) 2022 Maxwell Ruiz
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
namespace SceneGate.Lemon.Containers.Converters;

using System;
using System.IO;
using System.Security.Cryptography;
using Formats;
using Yarhl.FileFormat;
using Yarhl.FileSystem;
using Yarhl.IO;

/// <summary>
/// Convert a NCCH instance into binary format.
/// </summary>
/// <remarks>
/// <p>This converter expects to have a node with the following binary
/// children: header_data/partition_id, header_data/maker_code
/// header_data/version, header_data/program_id, header_data/product_code,
/// header_data/flag_(0 through 7),
/// extended_header (optional), access_descriptor (optional), sdk_info.txt (optional),
/// logo.bin (optional), system (optional), rom (optional).</p>
/// </remarks>
public class Ncch2Binary :
    IConverter<Ncch, BinaryFormat>
{
    DataStream stream;

    /// <summary>
    /// Initialize the converter by providing the stream to write to.
    /// </summary>
    /// <param name="parameters">Stream to write to.</param>
    public void Initialize(DataStream parameters)
    {
            this.stream = parameters;
        }

    /// <summary>
    /// Converts a NCCH instance into a binary stream.
    /// </summary>
    /// <param name="source">Ncch to convert.</param>
    /// <returns>The new binary container.</returns>
    public BinaryFormat Convert(Ncch source)
    {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var binary = (stream != null) ? new BinaryFormat(stream) : new BinaryFormat();
            var writer = new DataWriter(binary.Stream);

            Node root = source.Root;

            // Ncch header data has a length of 0x200 before binary data.
            writer.Write(new byte[0x200]);
            writer.Stream.Position = 0;

            // First we write the header signature
            writer.Write(source.Header.Signature);

            byte[] magicId = [0x4E, 0x43, 0x43, 0x48];
            writer.Write(magicId);

            // We'll write the total size at last.
            writer.Stream.Position += 0x4;

            writer.Write(source.Header.PartitionId);
            writer.Write(source.Header.MakerCode);
            writer.Write(source.Header.Version);

            if (source.Header.Flags[7] == 0x20) {
                throw new FormatException("Value 0x20 for Flag 0x07 not supported yet");
            }
            else {
                writer.Stream.Position += 0x4;
            }

            writer.Write(source.Header.ProgramId);

            writer.Stream.Position += 0x10; // Reserved

            WriteSHA256(writer, root.Children["logo.bin"]);
            writer.Write(source.Header.ProductCode.PadRight(0x10, '\0'));
            WriteSHA256(writer, root.Children["extended_header"]);
            writer.Write((int)(root.Children["extended_header"]?.Stream?.Length ?? 0));

            writer.Stream.Position += 0x4; // Reserved

            for (int i = 0; i < source.Header.Flags.Length; i++) {
                byte flag = source.Header.Flags[i];
                writer.Write(flag);
            }

            // Now we begin writing the NCCH content
            writer.Stream.Position = 0x200;
            WriteFile(writer, root.Children["extended_header"], false);
            WriteFile(writer, root.Children["access_descriptor"], false);

            writer.Stream.Position = 0x190;
            WriteOffsetSizeAndData(writer, root.Children["sdk_info.txt"]);
            WriteOffsetSizeAndData(writer, root.Children["logo.bin"]);
            WriteOffsetSizeAndData(writer, root.Children["system"], source.Header.SystemHashSize, true);

            writer.Stream.Position += 0x4; // Reserved
            WriteOffsetSizeAndData(writer, root.Children["rom"], source.Header.RomHashSize, true);

            writer.Stream.Position += 0x4; // Reserved

            WriteSHA256(writer, root.Children["system"], source.Header.SystemHashSize, true);
            WriteSHA256(writer, root.Children["rom"], source.Header.RomHashSize, true);

            // Write padding at the end of the binary to prevent having a wrong size in units
            writer.Stream.Position = writer.Stream.Length;
            writer.WritePadding(0, NcchHeader.Unit);

            // Write the full size of the binary (in units) in 0x104
            writer.Stream.Position = 0x104;
            writer.Write((int)writer.Stream.Length / NcchHeader.Unit);

            return binary;
        }

    void WriteFile(DataWriter writer, Node file, bool isRequired = true)
    {
            if (file == null && isRequired) {
                throw new FileNotFoundException("Missing NCCH file");
            } else if (file == null) {
                return;
            }

            if (file.Format is not IBinary) {
                throw new FormatException("Cannot write file as it is not binary");
            }

            file.Stream.WriteTo(writer.Stream);
        }

    void WriteOffsetSizeAndData(DataWriter writer, Node file, int hashRegion = 0, bool hasHashRegion = false)
    {
            if (file != null) {
                long position = writer.Stream.Position;
                int offsetInUnits = (int)writer.Stream.Length / NcchHeader.Unit;
                int fileLength = (int)file.Stream.Length.Pad(NcchHeader.Unit);
                int fileLengthInUnits = fileLength / NcchHeader.Unit;

                writer.Write(offsetInUnits);
                writer.Write(fileLengthInUnits);

                writer.Stream.Position = writer.Stream.Length;
                file.Stream.WriteTo(writer.Stream);
                writer.WritePadding(0, NcchHeader.Unit);

                writer.Stream.Position = position + 0x8;
            }
            else {
                writer.Write(0);
                writer.Write(0);
            }

            if (hasHashRegion) {
                writer.Write(hashRegion);
            }
        }

    void WriteSHA256(DataWriter writer, Node file, int hashRegion = 0, bool hasHashRegion = false)
    {
            using (SHA256 sha256 = SHA256.Create()) {
                try {
                    if (file == null) {
                        writer.Write(new byte[0x20]);
                        return;
                    }

                    if (hasHashRegion && hashRegion == 0) {
                        writer.Write(sha256.ComputeHash(new byte[0x0]));
                    } else if (hasHashRegion) {
                        int hashSize = hashRegion * 0x200;
                        byte[] buffer = new byte[hashSize];
                        file.Stream.Position = 0;
                        file.Stream.Read(buffer, 0, hashSize);

                        writer.Write(sha256.ComputeHash(buffer));
                    } else {
                        writer.Write(sha256.ComputeHash(file.Stream));
                    }
                } catch (Exception ex) {
                    throw new FormatException($"Failed to perform SHA256 for {file.Name}", ex);
                }
            }
        }
}
