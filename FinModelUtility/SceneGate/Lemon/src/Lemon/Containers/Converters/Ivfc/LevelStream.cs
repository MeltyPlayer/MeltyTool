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
namespace SceneGate.Lemon.Containers.Converters.Ivfc
{
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using Yarhl.IO;
    using Yarhl.IO.StreamFormat;

    /// <summary>
    /// IVFC level stream.
    /// </summary>
    internal class LevelStream : StreamWrapper
    {
        readonly object lockObj = new object();
        SHA256 sha;

        /// <summary>
        /// Initializes a new instance of the <see cref="LevelStream"/> class
        /// and use a stream in-memory.
        /// </summary>
        /// <param name="blockSize">Block size for padding and hash.</param>
        public LevelStream(int blockSize)
            : this(blockSize, new RecyclableMemoryStream())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LevelStream" /> class.
        /// Changes in position in this stream won't affect the base stream.
         /// </summary>
        /// <param name="blockSize">Block size for padding and hash.</param>
        /// <param name="stream">The underlying stream.</param>
        public LevelStream(int blockSize, Stream stream)
            : base(new DataStream(stream)) // so the position is independent.
        {
            BlockSize = blockSize;
            sha = SHA256.Create();
        }

        /// <summary>
        /// Raises when a new block of data is generated.
        /// </summary>
        public event EventHandler<BlockWrittenEventArgs> BlockWritten;

        /// <summary>
        /// Gets the block size for padding and hash.
        /// </summary>
        public int BlockSize { get; }

        /// <summary>
        /// Gets the stream lock.
        /// </summary>
        public object LockObj => lockObj;

        /// <summary>
        /// Writes a byte.
        /// </summary>
        /// <param name="value">Byte value.</param>
        public override void WriteByte(byte value)
        {
            WriteAndUpdateHash([value], 0, 1);
        }

        /// <summary>
        /// Writes the a portion of the buffer to the stream.
        /// </summary>
        /// <param name="buffer">Buffer to write.</param>
        /// <param name="offset">Index in the buffer.</param>
        /// <param name="count">Bytes to write.</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            WriteAndUpdateHash(buffer, offset, count);
        }

        /// <summary>
        /// Releases all resources used by the object.
        /// </summary>
        /// <param name="disposing">Whether to free the managed resources too.</param>
        protected override void Dispose(bool disposing)
        {
            if (Disposed) {
                return;
            }

            if (disposing) {
                sha.Dispose();
            }

            base.Dispose(disposing);
        }

        void WriteAndUpdateHash(byte[] data, int offset, int count)
        {
            while (true) {
                // Repeat until writing the data does not span into the next block.
                long writtenBlocks = Position / BlockSize;
                long futureBlocks = (Position + count) / BlockSize;
                if (futureBlocks == writtenBlocks) {
                    break;
                }

                int posInBlock = (int)(Position % BlockSize);
                int bytesToWrite = BlockSize - posInBlock;

                // It's mandatory to call at least one time to TransformBlock
                // before calling again TransformFinalBlock, so this should
                // make the trick, calling with size 0.
                if (posInBlock == 0 && bytesToWrite > 0)
                {
                    sha.TransformBlock(data, offset, 1, data, offset);
                    WriteWithoutHash(data, offset, 1);
                    offset += 1;
                    count -= 1;
                    bytesToWrite -= 1;
                }

                // Send bytes to SHA and to the stream so we update our
                // position too.
                sha.TransformFinalBlock(data, offset, bytesToWrite);
                WriteWithoutHash(data, offset, bytesToWrite);

                // Trigger event.
                var eventArgs = new BlockWrittenEventArgs(sha.Hash);
                BlockWritten?.Invoke(this, eventArgs);
                sha.Dispose();
                sha = SHA256.Create();

                // Skip already processed bytes
                offset += bytesToWrite;
                count -= bytesToWrite;
            }

            if (count > 0) {
                sha.TransformBlock(data, offset, count, data, offset);
                WriteWithoutHash(data, offset, count);
            }
        }

        void WriteWithoutHash(byte[] data, int index, int count)
        {
            base.Write(data, index, count);
        }
    }
}
