// ContentInfoRecord.cs
//
// Author:
//       Maxwell Ruiz maxwellaquaruiz@gmail.com
//
// Copyright (c) 2022 Maxwell Ruiz
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
namespace SceneGate.Lemon.Titles
{
    /// <summary>
    /// Information about a CIA content info record.
    /// </summary>
    public class ContentInfoRecord
    {
        /// <summary>
        /// Gets or sets the content index offset.
        /// </summary>
        public short IndexOffset { get; set; }

        /// <summary>
        /// Gets or sets the content command count (number of chunks to hash).
        /// </summary>
        public short CommandCount { get; set; }

        /// <summary>
        /// Gets or sets the record hash.
        /// </summary>
        public byte[] Hash { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether if the info record is empty.
        /// </summary>
        public bool IsEmpty { get; set; }
    }
}
