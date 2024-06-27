// TitleMetadata2Binary.cs
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
namespace SceneGate.Lemon.Titles;

using System;
using Yarhl.FileFormat;
using Yarhl.IO;

/// <summary>
/// Convert a TitleMetadata object into binary format.
/// </summary>
public class TitleMetadata2Binary : IConverter<TitleMetadata, BinaryFormat>
{
    /// <summary>
    /// Converts a title metadata object into a binary format.
    /// </summary>
    /// <param name="source">The source binary.</param>
    /// <returns>The deserializer title metadata.</returns>
    public BinaryFormat Convert(TitleMetadata source)
    {
            var binary = new BinaryFormat();
            var writer = new DataWriter(binary.Stream) {
                Endianness = EndiannessMode.BigEndian,
            };

            writer.Write(source.SignType);
            writer.Write(source.Signature);
            writer.Write(source.SignatureIssuer.PadRight(0x40, '\0'));
            writer.Write(source.Version);
            writer.Write(source.CaCrlVersion);
            writer.Write(source.SignerCrlVersion);
            writer.Write((byte)0x0); // Reserved

            writer.Write(source.SystemVersion);
            writer.Write(source.TitleId);
            writer.Write(source.TitleType);
            writer.Write(source.GroupId);
            writer.Write(source.SaveSize);
            writer.Write(source.SrlPrivateSaveSize);
            writer.Write(new byte[0x4]); // Reserved

            writer.Write(source.SrlFlag);
            writer.Write(new byte[0x31]); // Reserved

            writer.Write(source.AccessRights);
            writer.Write(source.TitleVersion);
            writer.Write((short)source.Chunks.Count);
            writer.Write((short)source.BootContent);
            writer.Write(new byte[0x2]); // Padding

            writer.Write(source.Hash);

            for (int i = 0; i < source.InfoRecords.Count; i++) {
                var infoRecord = source.InfoRecords[i];

                writer.Write(infoRecord.IndexOffset);
                writer.Write(infoRecord.CommandCount);
                writer.Write(infoRecord.Hash);
            }

            for (int i = 0; i < source.Chunks.Count; i++) {
                var chunk = source.Chunks[i];

                writer.Write(chunk.Id);
                writer.Write(chunk.Index);

                int attribute = (int)Enum.Parse(typeof(ContentAttributes), chunk.Attributes.ToString());
                writer.Write((short)attribute);

                writer.Write(chunk.Size);
                writer.Write(chunk.Hash);
            }

            return binary;
        }
}
