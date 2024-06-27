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
namespace Yarhl.FileFormat
{
    using System;

    /// <summary>
    /// Metadata associated to a Format class.
    /// </summary>
    public class FormatMetadata : IExportMetadata
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FormatMetadata" /> class.
        /// </summary>
        public FormatMetadata()
        {
            // MEF should always set these properties, so they won't be null.
            // We set some initial values to ensure later they are not set to null.
            Name = "<invalid>";
            Type = typeof(FormatMetadata);
        }

        /// <summary>
        /// Gets or sets the type full name. Shortcut of Type.FullName.
        /// </summary>
        /// <value>The full name of the type.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the type of the format.
        /// </summary>
        /// <value>The type of the format.</value>
        public Type Type { get; set; }
    }
}
