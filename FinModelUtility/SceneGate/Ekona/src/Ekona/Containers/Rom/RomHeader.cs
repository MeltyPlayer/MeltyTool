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

using Yarhl.FileFormat;

namespace SceneGate.Ekona.Containers.Rom;

/// <summary>
/// Header of the program in a ROM.
/// </summary>
public class RomHeader : IFormat
{
    /// <summary>
    /// Gets or sets the information of the program.
    /// </summary>
    public ProgramInfo ProgramInfo { get; set; } = new ProgramInfo();

    /// <summary>
    /// Gets or sets the information of the sections of the ROM.
    /// </summary>
    public RomSectionInfo SectionInfo { get; set; } = new RomSectionInfo();

    /// <summary>
    /// Gets or sets the compressed copyright logo.
    /// </summary>
    /// <remarks>
    /// <para>The logo is compressed with Huffman with a header present in the BIOS.</para>
    /// <para>The logo must be the original or the device won't boot the game.</para>
    /// </remarks>
    public byte[] CopyrightLogo { get; set; }
}
