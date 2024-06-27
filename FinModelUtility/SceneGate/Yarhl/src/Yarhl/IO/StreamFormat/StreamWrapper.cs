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
    /// Wrapper over a streams to override easily specific methods and use the
    /// base implementation for the rest.
    /// </summary>
    public abstract class StreamWrapper : Stream
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StreamWrapper" /> class.
        /// </summary>
        /// <param name="stream">The stream to wrap.</param>
        protected StreamWrapper(Stream stream)
        {
            BaseStream = stream;
        }

        /// <summary>
        /// Gets or sets the base stream.
        /// </summary>
        public Stream BaseStream {
            get;
            protected set;
        }

        /// <summary>
        /// Gets or sets the position from the start of this stream.
        /// </summary>
        public override long Position {
            get => BaseStream.Position;
            set => BaseStream.Position = value;
        }

        /// <summary>
        /// Gets the length of this stream.
        /// </summary>
        public override long Length => BaseStream.Length;

        /// <summary>
        /// Gets a value indicating whether this stream has been disposed.
        /// </summary>
        public bool Disposed {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports reading.
        /// </summary>
        public override bool CanRead => BaseStream?.CanRead ?? true;

        /// <summary>
        /// Gets a value indicating whether the current stream supports seeking.
        /// </summary>
        public override bool CanSeek => BaseStream?.CanSeek ?? true;

        /// <summary>
        /// Gets a value indicating whether the current stream supports writing.
        /// </summary>
        public override bool CanWrite => BaseStream?.CanWrite ?? true;

        /// <summary>
        /// Gets a value indicating whether the current stream supports timeouts.
        /// </summary>
        public override bool CanTimeout => BaseStream?.CanTimeout ?? false;

        /// <summary>
        /// Sets the length of the stream.
        /// </summary>
        /// <param name="value">The new length of the stream.</param>
        public override void SetLength(long value)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(StreamWrapper));

            BaseStream.SetLength(value);
        }

        /// <summary>
        /// Clears all buffers for this stream and causes any buffered data
        /// to be written to the underlying device.
        /// </summary>
        public override void Flush()
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(StreamWrapper));

            BaseStream.Flush();
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
                throw new ObjectDisposedException(nameof(StreamWrapper));

            return BaseStream.Read(buffer, offset, count);
        }

        /// <summary>
        /// Reads the next byte.
        /// </summary>
        /// <returns>The next byte.</returns>
        public override int ReadByte()
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(StreamWrapper));

            return BaseStream.ReadByte();
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
                throw new ObjectDisposedException(nameof(StreamWrapper));

            BaseStream.Write(buffer, offset, count);
        }

        /// <summary>
        /// Writes a byte.
        /// </summary>
        /// <param name="value">Byte value.</param>
        public override void WriteByte(byte value)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(StreamWrapper));

            BaseStream.WriteByte(value);
        }

        /// <summary>
        /// Move the position of the stream.
        /// </summary>
        /// <param name="offset">Distance to move position.</param>
        /// <param name="origin">Mode to move position.</param>
        /// <returns>The new position of the stream.</returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(StreamWrapper));

            return BaseStream.Seek(offset, origin);
        }

        /// <summary>
        /// Releases all resource used by the <see cref="StreamWrapper"/>
        /// object.
        /// </summary>
        /// <param name="disposing">If set to
        /// <see langword="true" /> free managed resources also.</param>
        protected override void Dispose(bool disposing)
        {
            if (Disposed) {
                return;
            }

            Disposed = true;
            if (disposing) {
                BaseStream?.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
