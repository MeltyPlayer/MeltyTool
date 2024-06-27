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
namespace Yarhl.IO.Serialization.Attributes;

using System;

/// <summary>
/// Set to force the endianness in automatic serialization.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class BinaryForceEndiannessAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BinaryForceEndiannessAttribute"/> class.
    /// </summary>
    /// <param name="mode">Endianness mode for the property.</param>
    public BinaryForceEndiannessAttribute(EndiannessMode mode)
    {
            Mode = mode;
        }

    /// <summary>
    /// Gets the endianness mode.
    /// </summary>
    public EndiannessMode Mode {
        get;
    }
}
