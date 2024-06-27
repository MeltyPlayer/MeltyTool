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
namespace Yarhl.IO
{
    using System;
    using System.IO;
    using StreamFormat;

    /// <summary>
    /// Factory of DataStream.
    /// </summary>
    public static class DataStreamFactory
    {
        /// <summary>
        /// Creates a new <see cref="DataStream"/> from a <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The stream to use as a base.</param>
        /// <remarks>
        /// <p>The dispose ownership is transferred to the new DataStream.</p>
        /// </remarks>
        /// <returns>A new <see cref="DataStream"/>.</returns>
        public static DataStream FromStream(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            return new DataStream(stream);
        }

        /// <summary>
        /// Creates a new <see cref="DataStream"/> from a section of a
        /// <see cref="Stream"/>.
        /// </summary>
        /// <remarks>
        /// <p>The life-management of the stream is transferred to the
        /// <see cref="DataStream"/>. This means that disposing the new
        /// <see cref="DataStream"/> will potentially dispose the underlying
        /// stream.</p>
        /// <p>Check <see cref="FromStreamKeepingOwnership" /> if you don't
        /// want this behavior.</p>
        /// </remarks>
        /// <param name="stream">The stream to use as a base.</param>
        /// <param name="offset">Offset of the base stream.</param>
        /// <param name="length">Length of the new substream.</param>
        /// <returns>A new <see cref="DataStream"/>.</returns>
        public static DataStream FromStream(Stream stream, long offset, long length)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            if (offset < 0 || offset > stream.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (length < 0 || offset + length > stream.Length)
                throw new ArgumentOutOfRangeException(nameof(length));

            return new DataStream(stream, offset, length, true);
        }

        /// <summary>
        /// Creates a new <see cref="DataStream"/> from a section of a
        /// <see cref="Stream"/>.
        /// </summary>
        /// <remarks>
        /// <p>The dispose ownership is not transferred to the new
        /// <see cref="DataStream" />. Instead, the caller is still responsible
        /// to dispose according the stream argument.</p>
        /// </remarks>
        /// <param name="stream">The stream to use as a base.</param>
        /// <param name="offset">Offset of the base stream.</param>
        /// <param name="length">Length of the new substream.</param>
        /// <returns>A new <see cref="DataStream"/>.</returns>
        public static DataStream FromStreamKeepingOwnership(Stream stream, long offset, long length)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            if (offset < 0 || offset > stream.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (length < 0 || offset + length > stream.Length)
                throw new ArgumentOutOfRangeException(nameof(length));

            return new DataStream(stream, offset, length, false);
        }

        /// <summary>
        /// Creates a new <see cref="DataStream"/> in memory.
        /// </summary>
        /// <returns>A new <see cref="DataStream"/>.</returns>
        public static DataStream FromMemory()
        {
            var baseStream = new RecyclableMemoryStream();
            return new DataStream(baseStream);
        }

        /// <summary>
        /// Creates a new <see cref="DataStream"/> from an array of data.
        /// </summary>
        /// <param name="data">The array of data to use in the stream.</param>
        /// <returns>A new <see cref="DataStream"/>.</returns>
        public static DataStream FromArray(byte[] data)
        {
            if (data is null)
                throw new ArgumentNullException(nameof(data));

            return FromArray(data, 0, data.Length);
        }

        /// <summary>
        /// Creates a new <see cref="DataStream"/> from an array of data.
        /// </summary>
        /// <param name="data">The array of data to use in the stream.</param>
        /// <param name="offset">Offset in the array of data.</param>
        /// <param name="length">Length of the new stream.</param>
        /// <returns>A new <see cref="DataStream"/>.</returns>
        public static DataStream FromArray(byte[] data, int offset, int length)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            if (offset < 0 || offset > data.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (length < 0 || offset + length > data.Length)
                throw new ArgumentOutOfRangeException(nameof(length));

            var baseStream = new MemoryStream(data, 0, data.Length);
            return new DataStream(baseStream, offset, length, true);
        }

        /// <summary>
        /// Creates a new <see cref="DataStream"/> from a file.
        /// </summary>
        /// <param name="path">The path of the file.</param>
        /// <param name="mode">The mode to open the file.</param>
        /// <returns>A new <see cref="DataStream"/>.</returns>
        public static DataStream FromFile(string path, FileOpenMode mode)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));
            if (mode == FileOpenMode.Read && !File.Exists(path)) {
                throw new FileNotFoundException("File to read does not exist", path);
            }

            var baseStream = new LazyFileStream(path, mode);
            return new DataStream(baseStream);
        }

        /// <summary>
        /// Creates a new <see cref="DataStream"/> from a section of a file.
        /// </summary>
        /// <param name="path">The path of the file.</param>
        /// <param name="mode">The mode to open the file.</param>
        /// <param name="offset">Offset from the start of the file.</param>
        /// <param name="length">Length of the new stream.</param>
        /// <returns>A new <see cref="DataStream"/>.</returns>
        public static DataStream FromFile(string path, FileOpenMode mode, long offset, long length)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));
            if (mode == FileOpenMode.Read && !File.Exists(path)) {
                throw new FileNotFoundException("File to read does not exist: " + path, path);
            }

            var info = new FileInfo(path);
            long fileSize;

            // If it's a windows symlink, get the length by opening the stream.
            // Otherwise we would need P/Invoke calls.
            // .NET does the redirection of the symlink in FileStream automatically.
            if (info.Exists && info.Attributes.HasFlag(FileAttributes.ReparsePoint)) {
                using var tempStream = new FileStream(path, FileMode.Open);
                fileSize = tempStream.Length;
            } else {
                fileSize = info.Exists ? info.Length : 0;
            }

            if (offset < 0 || offset > fileSize)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (length < 0 || offset + length > fileSize)
                throw new ArgumentOutOfRangeException(nameof(length));

            var baseStream = new LazyFileStream(path, mode);
            return new DataStream(baseStream, offset, length, true);
        }
    }
}
