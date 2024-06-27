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
    /// Define how to read and write a Enum value.
    /// <remarks>Default type is <see cref="int"/></remarks>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class BinaryEnumAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryEnumAttribute"/> class.
        /// </summary>
        public BinaryEnumAttribute()
        {
            ReadAs = typeof(int);
            WriteAs = typeof(int);
        }

        /// <summary>
        /// Gets or sets the equivalent type for reading.
        /// </summary>
        public Type ReadAs {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the equivalent type for writing.
        /// </summary>
        public Type WriteAs {
            get;
            set;
        }
    }
}
