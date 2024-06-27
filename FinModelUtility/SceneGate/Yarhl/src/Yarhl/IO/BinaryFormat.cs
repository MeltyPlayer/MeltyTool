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
namespace Yarhl.IO;

using System;
using System.IO;
using FileFormat;

/// <summary>
/// Binary format.
/// </summary>
public class BinaryFormat : IBinary, IDisposable, ICloneableFormat
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BinaryFormat"/> class.
    /// Creates a stream in memory.
    /// </summary>
    public BinaryFormat()
    {
            Stream = new DataStream();
        }

    /// <summary>
    /// Initializes a new instance of the <see cref="BinaryFormat"/> class.
    /// </summary>
    /// <param name="stream">
    /// Stream to wrap as a format. It takes over the ownership of the stream.
    /// You must not dispose it.
    /// </param>
    public BinaryFormat(Stream stream)
    {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            Stream = stream as DataStream ?? DataStreamFactory.FromStream(stream);
        }

    /// <summary>
    /// Initializes a new instance of the <see cref="BinaryFormat"/> class.
    /// </summary>
    /// <remarks>
    /// <para>This format creates an internal <see cref="DataStream" /> from the
    /// provided stream. It will take over the ownership of the stream
    /// argument, you should not dispose this argument, unless you are
    /// providing a <see cref="DataStream" /> that we won't take over in case
    /// you want to create more substreams.</para>
    /// </remarks>
    /// <param name="stream">Binary stream.</param>
    /// <param name="offset">Offset from the DataStream start.</param>
    /// <param name="length">Length of the substream.</param>
    public BinaryFormat(Stream stream, long offset, long length)
    {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            if (offset < 0 || offset > stream.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (length < 0 || offset + length > stream.Length)
                throw new ArgumentOutOfRangeException(nameof(length));

            Stream = DataStreamFactory.FromStream(stream, offset, length);
        }

    /// <summary>
    /// Gets the stream.
    /// </summary>
    public DataStream Stream {
        get;
        private set;
    }

    /// <summary>
    /// Gets a value indicating whether this <see cref="BinaryFormat"/>
    /// is disposed.
    /// </summary>
    public bool Disposed {
        get;
        private set;
    }

    /// <summary>
    /// Makes a copy of the format stream <strong>into memory</strong>
    /// and returns a new <see cref="BinaryFormat"/> object.
    /// </summary>
    /// <remarks><para>The stream is copied into memory, so it is limited to 2GB size.</para></remarks>
    /// <returns>The cloned <see cref="BinaryFormat"/>.</returns>
    public virtual object DeepClone()
    {
            DataStream newStream = DataStreamFactory.FromMemory();
            Stream.WriteTo(newStream);

            return new BinaryFormat(newStream);
        }

    /// <summary>
    /// Releases all resource used by the <see cref="BinaryFormat"/> object.
    /// </summary>
    public void Dispose()
    {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    /// <summary>
    /// Releases all resource used by the <see cref="BinaryFormat"/> object.
    /// </summary>
    /// <param name="disposing">
    /// If set to <see langword="true" /> free managed resources also.
    /// It happens from Dispose() calls.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
            if (Disposed)
                return;

            Disposed = true;
            if (disposing) {
                Stream.Dispose();
            }
        }
}
