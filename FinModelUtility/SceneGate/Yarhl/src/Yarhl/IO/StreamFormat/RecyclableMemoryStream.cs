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
namespace Yarhl.IO.StreamFormat
{
    using System;
    using Microsoft.IO;

    /// <summary>
    /// In-memory stream with a pool of buffers.
    /// </summary>
    public sealed class RecyclableMemoryStream : StreamWrapper
    {
        static readonly RecyclableMemoryStreamManager Manager = CreateManager();

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="RecyclableMemoryStream"/> class.
        /// </summary>
        public RecyclableMemoryStream()
            : base(Manager.GetStream())
        {
        }

        /// <summary>
        /// Sets the length of the stream.
        /// </summary>
        /// <param name="value">The new length of the stream.</param>
        public override void SetLength(long value)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(RecyclableMemoryStream));

            long oldLength = Length;
            int additionalLength = (int)(value - oldLength);
            base.SetLength(value);

            // Since we are reusing buffers from a pool, it's not guarantee
            // that requesting more space will return a clean buffer.
            if (additionalLength > 0) {
                ClearBuffer(oldLength, additionalLength);
            }
        }

        static RecyclableMemoryStreamManager CreateManager()
        {
            return new RecyclableMemoryStreamManager();
        }

        void ClearBuffer(long offset, long size)
        {
            long oldPos = Position;
            Position = offset;

            const int BufferSize = 70 * 1024;
            byte[] buffer = new byte[size > BufferSize ? BufferSize : size];

            int written = 0;
            do {
                int loopLength;
                if (written + buffer.Length > size) {
                    loopLength = (int)(size - written);
                } else {
                    loopLength = buffer.Length;
                }

                Write(buffer, 0, loopLength);
                written += loopLength;
            } while (written != size);

            Position = oldPos;
        }
    }
}
