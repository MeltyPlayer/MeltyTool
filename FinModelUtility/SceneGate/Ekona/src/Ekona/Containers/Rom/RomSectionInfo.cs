// Copyright(c) 2021 SceneGate
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
namespace SceneGate.Ekona.Containers.Rom;

/// <summary>
/// Information about the different sections of the ROM.
/// </summary>
public class RomSectionInfo
{
    /// <summary>
    /// Gets or sets the offset of the ARM-9 program.
    /// </summary>
    public uint Arm9Offset { get; set; }

    /// <summary>
    /// Gets or sets the size of the ARM-9 program.
    /// </summary>
    public uint Arm9Size { get; set; }

    /// <summary>
    /// Gets or sets the offset of the ARM-7 program.
    /// </summary>
    public uint Arm7Offset { get; set; }

    /// <summary>
    /// Gets or sets the size of the ARM-7 program.
    /// </summary>
    public uint Arm7Size { get; set; }

    /// <summary>
    /// Gets or sets the offset to the file name table.
    /// </summary>
    public uint FntOffset { get; set; }

    /// <summary>
    /// Gets or sets the size of the file name table.
    /// </summary>
    public uint FntSize { get; set; }

    /// <summary>
    /// Gets or sets the offset of the file access table.
    /// </summary>
    public uint FatOffset { get; set; }

    /// <summary>
    /// Gets or sets the size of the file access table.
    /// </summary>
    public uint FatSize { get; set; }

    /// <summary>
    /// Gets or sets the offset of the table of overlays for ARM-9 program.
    /// </summary>
    public uint Overlay9TableOffset { get; set; }

    /// <summary>
    /// Gets or sets the size of the table of overlays for ARM-9 program.
    /// </summary>
    public int Overlay9TableSize { get; set; }

    /// <summary>
    /// Gets or sets the offset of the table of overlays for ARM-7 program.
    /// </summary>
    public uint Overlay7TableOffset { get; set; }

    /// <summary>
    /// Gets or sets the size of the table of overlays for ARM-7 program.
    /// </summary>
    public int Overlay7TableSize { get; set; }

    /// <summary>
    /// Gets or sets the offset of the program banner.
    /// </summary>
    public uint BannerOffset { get; set; }

    /// <summary>
    /// Gets or sets the exact size of the ROM.
    /// </summary>
    public uint RomSize { get; set; }

    /// <summary>
    /// Gets or sets the size of the header.
    /// </summary>
    public uint HeaderSize { get; set; }

    /// <summary>
    /// Gets or sets the DS region end.
    /// </summary>
    public ushort NitroRegionEnd { get; set; }

    /// <summary>
    /// Gets or sets the DSi region start.
    /// </summary>
    public ushort TwilightRegionStart { get; set; }

    /// <summary>
    /// Gets or sets the offset for the ARM-9 program for DSi devices.
    /// </summary>
    public uint Arm9iOffset { get; set; }

    /// <summary>
    /// Gets or sets the size of the ARM-9 program for DSi devices.
    /// </summary>
    public uint Arm9iSize { get; set; }

    /// <summary>
    /// Gets or sets the offset for the ARM-7 program for DSi devices.
    /// </summary>
    public uint Arm7iOffset { get; set; }

    /// <summary>
    /// Gets or sets the size of the ARM-7 program for DSi devices.
    /// </summary>
    public uint Arm7iSize { get; set; }

    /// <summary>
    /// Gets or sets the offset for the data to digest (hash) of the DS ROM section.
    /// </summary>
    public uint DigestNitroOffset { get; set; }

    /// <summary>
    /// Gets or sets the length for the data to digest (hash) for the DS ROM section.
    /// </summary>
    public uint DigestNitroLength { get; set; }

    /// <summary>
    /// Gets or sets the offset for the data to digest (hash) of the DSi ROM section.
    /// </summary>
    public uint DigestTwilightOffset { get; set; }

    /// <summary>
    /// Gets or sets the offset for the data to digest (hash) of the DSi ROM section.
    /// </summary>
    public uint DigestTwilightLength { get; set; }

    /// <summary>
    /// Gets or sets the offset for the digest hash table of sectors.
    /// </summary>
    public uint DigestSectorHashtableOffset { get; set; }

    /// <summary>
    /// Gets or sets the length of the digest hash table of sectors.
    /// </summary>
    public uint DigestSectorHashtableLength { get; set; }

    /// <summary>
    /// Gets or sets the offset for the digest hash table of blocks (hashes of sectors).
    /// </summary>
    public uint DigestBlockHashtableOffset { get; set; }

    /// <summary>
    /// Gets or sets the length of the digest hash table of blocks (hashes of sectors).
    /// </summary>
    public uint DigestBlockHashtableLength { get; set; }

    /// <summary>
    /// Gets or sets the size for each sector digest.
    /// </summary>
    public uint DigestSectorSize { get; set; }

    /// <summary>
    /// Gets or sets the number of sectors to hash per block.
    /// </summary>
    public uint DigestBlockSectorCount { get; set; }

    /// <summary>
    /// Gets or sets the banner length.
    /// </summary>
    public uint BannerLength { get; set; }

    /// <summary>
    /// Gets or sets the ROM size including the DSi section.
    /// </summary>
    public uint DsiRomLength { get; set; }

    /// <summary>
    /// Gets or sets the offset for the first area modcrypted in the ROM.
    /// </summary>
    public uint ModcryptArea1Offset { get; set; }

    /// <summary>
    /// Gets or sets the length for the first area modcrypted in the ROM.
    /// </summary>
    public uint ModcryptArea1Length { get; set; }

    /// <summary>
    /// Gets or sets the offset for the second area modcrypted in the ROM.
    /// </summary>
    public uint ModcryptArea2Offset { get; set; }

    /// <summary>
    /// Gets or sets the length for the second area modcrypted in the ROM.
    /// </summary>
    public uint ModcryptArea2Length { get; set; }
}
