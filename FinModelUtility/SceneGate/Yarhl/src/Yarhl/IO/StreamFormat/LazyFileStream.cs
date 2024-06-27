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
    using System.IO;

    /// <summary>
    /// Open file for reading or writing on the first operation (lazily).
    /// </summary>
    public sealed class LazyFileStream : StreamWrapper
    {
        readonly string path;
        readonly FileOpenMode mode;
        readonly long initialLength;

        bool isInitialized;
        long initialPosition;

        /// <summary>
        /// Initializes a new instance of the <see cref="LazyFileStream"/> class.
        /// </summary>
        /// <param name="path">Path to the file.</param>
        /// <param name="mode">Mode to open the file.</param>
        public LazyFileStream(string path, FileOpenMode mode)
            : base(null!)
        {
            this.path = path;
            this.mode = mode;

            // Before the stream is initialized, we can get the
            // length so we can report it without opening the file.
            // If it's a windows symlink, get the length by opening the stream.
            // Otherwise we would need P/Invoke calls.
            // .NET does the redirection of the symlink in FileStream automatically.
            var info = new FileInfo(path);
            if (info.Exists && info.Attributes.HasFlag(FileAttributes.ReparsePoint)) {
                using var tempStream = new FileStream(path, FileMode.Open);
                initialLength = tempStream.Length;
            } else {
                initialLength = info.Exists ? info.Length : 0;
            }
        }

        /// <inheritdoc/>
        public override long Position
        {
            get => BaseStream?.Position ?? 0;
            set
            {
                if (BaseStream is null) {
                    initialPosition = value;
                } else {
                    BaseStream.Position = value;
                }
            }
        }

        /// <summary>
        /// Gets the length of this stream.
        /// </summary>
        public override long Length => BaseStream?.Length ?? initialLength;

        /// <summary>
        /// Sets the length of the stream.
        /// </summary>
        /// <param name="value">The new length of the stream.</param>
        public override void SetLength(long value)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(LazyFileStream));

            if (!isInitialized) {
                Initialize();
            }

            base.SetLength(value);
        }

        /// <summary>
        /// Reads from the stream to the buffer.
        /// </summary>
        /// <returns>The number of bytes read.</returns>
        /// <param name="buffer">Buffer to copy data.</param>
        /// <param name="offset">Index to start copying in buffer.</param>
        /// <param name="count">Number of bytes to read.</param>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(LazyFileStream));

            if (!isInitialized) {
                Initialize();
            }

            return base.Read(buffer, offset, count);
        }

        /// <summary>
        /// Reads the next byte.
        /// </summary>
        /// <returns>The next byte.</returns>
        public override int ReadByte()
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(LazyFileStream));

            if (!isInitialized) {
                Initialize();
            }

            return base.ReadByte();
        }

        /// <summary>
        /// Writes the a portion of the buffer to the stream.
        /// </summary>
        /// <param name="buffer">Buffer to write.</param>
        /// <param name="offset">Index in the buffer.</param>
        /// <param name="count">Bytes to write.</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(LazyFileStream));

            if (!isInitialized) {
                Initialize();
            }

            base.Write(buffer, offset, count);
        }

        /// <summary>
        /// Writes a byte.
        /// </summary>
        /// <param name="value">Byte value.</param>
        public override void WriteByte(byte value)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(LazyFileStream));

            if (!isInitialized) {
                Initialize();
            }

            base.WriteByte(value);
        }

        void Initialize()
        {
            isInitialized = true;
            BaseStream = new FileStream(path, mode.ToFileMode(), mode.ToFileAccess());
            BaseStream.Position = initialPosition;
        }
    }
}
