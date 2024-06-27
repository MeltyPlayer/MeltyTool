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

/// <summary>
/// Attributes for a partition (NCCH) from the title metadata.
/// </summary>
[Flags]
public enum ContentAttributes {
    /// <summary>
    /// No special attributes.
    /// </summary>
    None = 0,

    /// <summary>
    /// The content is encrypted.
    /// </summary>
    Encrypted = 1,

    /// <summary>
    /// Unknown. The content comes from a cartridge?
    /// </summary>
    Disc = 2,

    /// <summary>
    /// Unknown.
    /// </summary>
    Cfm = 4,

    /// <summary>
    /// The content is optional.
    /// </summary>
    Optional = 0x4000,

    /// <summary>
    /// The content is shared.
    /// </summary>
    Shared = 0x8000,
}
