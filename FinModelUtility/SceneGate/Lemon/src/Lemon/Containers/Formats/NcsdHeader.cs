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
namespace SceneGate.Lemon.Containers.Formats;

using System.Diagnostics.CodeAnalysis;

/// <summary>
/// NCCH header information.
/// </summary>
public class NcsdHeader
{
    /// <summary>
    /// Gets the magic identifier of the format.
    /// </summary>
    /// <value>The magic ID of the format.</value>
    public static string MagicId {
        get { return "NCSD"; }
    }

    /// <summary>
    /// Gets the equivalent in bytes of one unit for the header values.
    /// </summary>
    /// <value>One unit in bytes.</value>
    public static int Unit {
        get { return 0x200; }
    }

    /// <summary>
    /// Gets or sets the RSA-2048 signature of the NCCH header using SHA-256.
    /// </summary>
    /// <value>The signature of the header.</value>
    [SuppressMessage(
        "Microsoft.Performance",
        "CA1819:PropertiesShouldNotReturnArrays",
        Justification="Model or DTO.")]
    public byte[] Signature {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the size of the format.
    /// </summary>
    /// <value>The size of the format in bytes.</value>
    public long Size {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the media ID.
    /// </summary>
    /// <value>The media ID.</value>
    public ulong MediaId {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the type of the firmware for each partition.
    /// </summary>
    /// <value>The type of the partitions firmware.</value>
    [SuppressMessage(
        "Microsoft.Performance",
        "CA1819:PropertiesShouldNotReturnArrays",
        Justification="Model or DTO.")]
    public FirmwareType[] FirmwaresType {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the encryption type of each partition.
    /// </summary>
    /// <value>The encryption type of each partition.</value>
    [SuppressMessage(
        "Microsoft.Performance",
        "CA1819:PropertiesShouldNotReturnArrays",
        Justification="Model or DTO.")]
    public byte[] CryptType {
        get;
        set;
    }
}
