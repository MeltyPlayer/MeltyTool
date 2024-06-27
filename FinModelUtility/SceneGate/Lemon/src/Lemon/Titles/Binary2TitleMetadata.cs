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
namespace SceneGate.Lemon.Titles;

using System;
using Yarhl.FileFormat;
using Yarhl.IO;

/// <summary>
/// Deserializer of binary title metadata.
/// </summary>
public class Binary2TitleMetadata : IConverter<BinaryFormat, TitleMetadata>
{
    const int NumContentInfo = 64;

    /// <summary>
    /// Converts a binary format into a title metadata object.
    /// </summary>
    /// <param name="source">The source binary.</param>
    /// <returns>The deserializer title metadata.</returns>
    public TitleMetadata Convert(BinaryFormat source)
    {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            source.Stream.Position = 0;
            var reader = new DataReader(source.Stream) {
                Endianness = EndiannessMode.BigEndian,
            };

            TitleMetadata metadata = new TitleMetadata();

            // TODO: Validate signature
            uint signType = reader.ReadUInt32();
            metadata.SignType = signType;
            int signSize = GetSignatureSize(signType);
            metadata.Signature = reader.ReadBytes(signSize);

            metadata = ReadHeader(reader, out int contentCount, metadata);

            for (int i = 0; i < NumContentInfo; i++) {
                ContentInfoRecord infoRecord = ReadInfoRecord(reader);
                metadata.InfoRecords.Add(infoRecord);
            }

            for (int i = 0; i < contentCount; i++) {
                ContentChunkRecord chunk = ReadChunkRecord(reader);
                metadata.Chunks.Add(chunk);
            }

            return metadata;
        }

    static TitleMetadata ReadHeader(DataReader reader, out int contentCount, TitleMetadata metadata)
    {
            metadata.SignatureIssuer = reader.ReadString(0x40).Replace("\0", string.Empty);
            metadata.Version = reader.ReadByte();
            metadata.CaCrlVersion = reader.ReadByte();
            metadata.SignerCrlVersion = reader.ReadByte();
            reader.Stream.Position++; // padding

            metadata.SystemVersion = reader.ReadInt64();
            metadata.TitleId = reader.ReadInt64();
            metadata.TitleType = reader.ReadInt32();
            metadata.GroupId = reader.ReadInt16();

            metadata.SaveSize = reader.ReadInt32();
            metadata.SrlPrivateSaveSize = reader.ReadInt32();
            reader.Stream.Position += 4; // reserved

            metadata.SrlFlag = reader.ReadByte();
            reader.Stream.Position += 0x31; // reserved

            metadata.AccessRights = reader.ReadInt32();
            metadata.TitleVersion = reader.ReadInt16();
            contentCount = reader.ReadInt16();
            metadata.BootContent = reader.ReadInt16();
            reader.Stream.Position += 2; // padding

            reader.Stream.Position += 0x20; // SHA-256 hash content info records

            return metadata;
        }

    static ContentChunkRecord ReadChunkRecord(DataReader reader)
    {
            var chunk = new ContentChunkRecord {
                Id = reader.ReadInt32(),
                Index = reader.ReadInt16(),
                Attributes = (ContentAttributes)reader.ReadInt16(),
                Size = reader.ReadInt64(),
                Hash = reader.ReadBytes(0x20),
            };

            return chunk;
        }

    static ContentInfoRecord ReadInfoRecord(DataReader reader)
    {
            var infoRecord = new ContentInfoRecord {
                IndexOffset = reader.ReadInt16(),
                CommandCount = reader.ReadInt16(),
                Hash = reader.ReadBytes(0x20),
                IsEmpty = false, // This exists if for some reason there's a TMD which has an empty record followed by a populated one.
            };

            bool hashIsEmpty = true;
            for (int i = 0; i < infoRecord.Hash.Length; i++) {
                if (infoRecord.Hash[i] != 0) {
                    hashIsEmpty = false;
                }
            }

            if (infoRecord.IndexOffset == 0 && infoRecord.CommandCount == 0 && hashIsEmpty) {
                infoRecord.IsEmpty = true;
            }

            return infoRecord;
        }

    static int GetSignatureSize(uint type)
    {
            // Including padding
            return type switch {
                0x010000 => 0x23C,
                0x010001 => 0x13C,
                0x010002 => 0x7C,
                0x010003 => 0x23C,
                0x010004 => 0x13C,
                0x010005 => 0x7C,
                _ => throw new NotSupportedException(),
            };
        }
}
