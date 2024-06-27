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
namespace Yarhl.IO.Serialization.Attributes
{
    using System;

    /// <summary>
    /// Define how to read and write a string value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class BinaryStringAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryStringAttribute"/> class.
        /// </summary>
        public BinaryStringAttribute()
        {
            CodePage = -1;
            Terminator = "\0";
            FixedSize = -1;
            MaxSize = -1;
            SizeType = null;
        }

        /// <summary>
        /// Gets or sets the string code page.
        /// <remarks>Set to -1 to use the reader/writer encoding.</remarks>
        /// </summary>
        public int CodePage {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the string terminator.
        /// </summary>
        public string Terminator {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the fixed size (in bytes) of the string.
        /// <remarks>Set to -1 if the string is length variable.</remarks>
        /// </summary>
        public int FixedSize {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the max size (in bytes) of the string.
        /// <remarks>Set to -1 if the string has no max size.</remarks>
        /// </summary>
        public int MaxSize {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the size value type.
        /// <remarks>Set to null if string doesn't have the length serialized.</remarks>
        /// </summary>
        public Type? SizeType {
            get;
            set;
        }
    }
}
