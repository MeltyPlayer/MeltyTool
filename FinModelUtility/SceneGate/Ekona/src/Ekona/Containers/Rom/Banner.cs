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

using System;
using SceneGate.Ekona.Security;
using Yarhl.FileFormat;

namespace SceneGate.Ekona.Containers.Rom;

/// <summary>
/// Banner of a program.
/// </summary>
public class Banner : IFormat
{
    private Version version;

    /// <summary>
    /// Gets or sets the version of the banner model.
    /// </summary>
    /// <remarks>
    /// <para>Only the following version are valid:</para>
    /// <list type="bullet">
    /// <item><description>0.1: Original</description></item>
    /// <item><description>0.2: Support Chinese title</description></item>
    /// <item><description>0.3: Support Chinese and Korean titles</description></item>
    /// <item><description>1.3: Support Chinese and Korean titles and animated DSi icon.</description></item>
    /// </list>
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">Invalid version number.</exception>
    public Version Version {
        get => version;
        set {
                if (value.Minor is < 1 or > 3)
                    throw new ArgumentOutOfRangeException(nameof(value), value.Minor, "Minor must be [1,3]");
                if (value.Major is < 0 or > 1)
                    throw new ArgumentOutOfRangeException(nameof(value), value.Major, "Major must be [0, 1]");
                if (value.Major == 1 && value.Minor != 3)
                    throw new ArgumentOutOfRangeException(nameof(value), value.Minor, "Minor must be 3 for major 1");

                version = value;
            }
    }

    /// <summary>
    /// Gets a value indicating whether the banner supports animated icons (version >= 1.3).
    /// </summary>
    public bool SupportAnimatedIcon => Version is { Major: > 1 } or { Major: 1, Minor: >= 3 };

    /// <summary>
    /// Gets or sets the CRC-16 checksum for the banner binary data of version 0.1.
    /// </summary>
    /// <remarks>This field may be null if the model was not deserialized from binary data.</remarks>
    public HashInfo ChecksumBase { get; set; }

    /// <summary>
    /// Gets or sets the CRC-16 checksum for the banner binary data of version 0.2
    /// that includes Chinese title.
    /// </summary>
    /// <remarks>This field may be null if the model was not deserialized from binary data.</remarks>
    public HashInfo ChecksumChinese { get; set; }

    /// <summary>
    /// Gets or sets the CRC-16 checksum for the banner binary data of version 0.3
    /// that includes Chinese and Korean titles.
    /// </summary>
    /// <remarks>This field may be null if the model was not deserialized from binary data.</remarks>
    public HashInfo ChecksumKorean { get; set; }

    /// <summary>
    /// Gets or sets the CRC-16 checksum for the animated DSi icon.
    /// </summary>
    /// <remarks>This field may be null if the model was not deserialized from binary data.</remarks>
    public HashInfo ChecksumAnimatedIcon { get; set; }

    /// <summary>
    /// Gets or sets the Japenese title.
    /// </summary>
    public string JapaneseTitle { get; set; }

    /// <summary>
    /// Gets or sets the English title.
    /// </summary>
    public string EnglishTitle { get; set; }

    /// <summary>
    /// Gets or sets the French title.
    /// </summary>
    public string FrenchTitle { get; set; }

    /// <summary>
    /// Gets or sets the German title.
    /// </summary>
    public string GermanTitle { get; set; }

    /// <summary>
    /// Gets or sets the Italian title.
    /// </summary>
    public string ItalianTitle { get; set; }

    /// <summary>
    /// Gets or sets the Spanish title.
    /// </summary>
    public string SpanishTitle { get; set; }

    /// <summary>
    /// Gets or sets the Chinese title.
    /// </summary>
    public string ChineseTitle { get; set; }

    /// <summary>
    /// Gets or sets the Korean title.
    /// </summary>
    public string KoreanTitle { get; set; }
}
