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
using System.Text;
using Logging;
using Yarhl.FileFormat;
using Yarhl.FileSystem;
using Yarhl.IO;

/// <summary>
/// Converter for Binary streams into a file system following the
/// IVFC tree format.
/// </summary>
/// <remarks>
/// <para>This converter does not validate the level 0, 1 and 2 hashes
/// or use the level 3 file and directory tokens.</para>
/// </remarks>
public class BinaryIvfc2NodeContainer : IConverter<BinaryFormat, NodeContainerFormat>
{
    const int Level0Padding = 0x1000;

    static readonly ILog Logger = LogProvider.GetCurrentClassLogger();
    static readonly Encoding Encoding = Encoding.Unicode;

    DataReader levelReader;
    uint dirInfoOffset;
    uint fileInfoOffset;
    uint fileDataOffset;

    /// <summary>
    /// Gets the magic identifier of the format.
    /// </summary>
    /// <value>The magic ID of the format.</value>
    public static string MagicId {
        get { return "IVFC"; }
    }

    /// <summary>
    /// Gets the supported format version.
    /// </summary>
    /// <value>The supported format version.</value>
    public static uint SupportedVersion {
        get { return 0x0001_0000; }
    }

    /// <summary>
    /// Converts a binary stream into a file system with the IVFC format.
    /// </summary>
    /// <param name="source">The binary stream to convert.</param>
    /// <returns>The file system from the IVFC stream.</returns>
    public NodeContainerFormat Convert(BinaryFormat source)
    {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var reader = new DataReader(source.Stream);
            var root = new NodeContainerFormat();

            if (reader.ReadString(4) != MagicId)
                throw new FormatException("Invalid Magic ID");

            uint version = reader.ReadUInt32();
            if (version != SupportedVersion) {
                Logger.Warn($"Unsupported version: {version}, expecting {SupportedVersion}.");
            }

            // Level 0, 1 and 2 only contain SHA-256 hashes. We can skip
            // and go directly to level 3 where the directory and file data is.
            // We don't need either the offset and size from the levels.
            uint level0Size = reader.ReadUInt32();
            reader.Stream.Position = 0x44;
            uint level3Size = reader.ReadUInt32();
            reader.Stream.Position = 0x54;
            uint headerSize = reader.ReadUInt32();

            // Level 3 is right after level 0. Don't ask me why -.-'
            // We know it should be call level 3 and not level 1 because the
            // logical level offset entries from the header follow that order.
            // We create a new sub-stream because all the offset are relative
            // to this section.
            long level3Offset = (headerSize + level0Size).Pad(Level0Padding);
            using (var level3 = DataStreamFactory.FromStream(reader.Stream, level3Offset, level3Size)) {
                levelReader = new DataReader(level3);

                // First we have the header. Since we don't need to search an
                // entry but we read all of them, we can skip the token hashes,
                // and go directly to the information sections.
                level3.Position = 0x0C;
                dirInfoOffset = levelReader.ReadUInt32();
                level3.Position = 0x1C;
                fileInfoOffset = levelReader.ReadUInt32();
                level3.Position = 0x24;
                fileDataOffset = levelReader.ReadUInt32();

                // Start processing directories and from there we will
                // get their files too. First entry is root.
                level3.Position = dirInfoOffset;
                ReadDirectoryInfo(root.Root);
            }

            return root;
        }

    [SuppressMessage("Reliability", "CA2000", Justification = "Transfer ownership")]
    void ReadDirectoryInfo(Node parent)
    {
            levelReader.Stream.Position += 4; // no need parent directory
            uint nextSiblingDir = levelReader.ReadUInt32();
            uint firstChildDir = levelReader.ReadUInt32();
            uint firstChildFile = levelReader.ReadUInt32();
            levelReader.Stream.Position += 4; // we are not using the hash table
            int nameLength = levelReader.ReadInt32();

            // We pass the root node, so detect if it's that to reuse object.
            // In any case, the root node doesn't have name (nameLength == 0).
            Node current;
            if (nameLength == 0) {
                if (parent.Parent != null) {
                    Logger.Error("Directory without name. Ignoring.");
                    return;
                }

                current = parent;
            } else {
                string name = Encoding.GetString(levelReader.ReadBytes(nameLength));
                current = NodeFactory.CreateContainer(name);
                parent.Add(current);
            }

            // Get next sibling or child files / dirs.
            if (nextSiblingDir != 0xFFFFFFFF) {
                levelReader.Stream.Position = dirInfoOffset + nextSiblingDir;
                ReadDirectoryInfo(parent);
            }

            if (firstChildFile != 0xFFFFFFFF) {
                levelReader.Stream.Position = fileInfoOffset + firstChildFile;
                ReadFileInfo(current);
            }

            if (firstChildDir != 0xFFFFFFFF) {
                levelReader.Stream.Position = dirInfoOffset + firstChildDir;
                ReadDirectoryInfo(current);
            }
        }

    void ReadFileInfo(Node parent)
    {
            levelReader.Stream.Position += 4; // no need parent directory
            uint nextSiblingFile = levelReader.ReadUInt32();
            long offset = levelReader.ReadInt64() + fileDataOffset;
            long size = levelReader.ReadInt64();
            levelReader.Stream.Position += 4; // we are not using the hash table
            int nameLength = levelReader.ReadInt32();
            string name = Encoding.GetString(levelReader.ReadBytes(nameLength));

            var file = NodeFactory.FromSubstream(name, levelReader.Stream, offset, size);
            parent.Add(file);

            if (nextSiblingFile != 0xFFFFFFFF) {
                levelReader.Stream.Position = fileInfoOffset + nextSiblingFile;
                ReadFileInfo(parent);
            }
        }
}
