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
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using FileFormat;
using StreamFormat;

/// <summary>
/// Virtual <see cref="Stream" /> with substream capabilities and read/write
/// abstraction layer.
/// </summary>
/// <remarks>
/// The type is thread-safe at the level of the substream. For instance, it
/// is safe to use several DataStream over the same base stream in parallel.
/// The type is not thread-safe for its method. For instance, it is NOT safe
/// to use the same DataStream in different threads at the same time.
/// </remarks>
[SuppressMessage(
    "",
    "S3881",
    Justification = "Historical reasons: https://docs.microsoft.com/en-us/dotnet/api/system.io.stream.dispose")]
public class DataStream : Stream
{
    static readonly ConcurrentDictionary<Stream, StreamInfo> Instances = new ConcurrentDictionary<Stream, StreamInfo>();
    readonly Stack<long> positionStack = new Stack<long>();
    readonly bool canExpand;
    readonly bool hasOwnsership;
    readonly StreamInfo streamInfo;
    readonly Stream baseStream;

    bool disposed;
    long offset;
    long position;
    long length;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataStream"/> class.
    /// A new stream is created in memory.
    /// </summary>
    public DataStream()
        : this(new RecyclableMemoryStream())
    {
        }

    /// <summary>
    /// Initializes a new instance of the <see cref="DataStream" /> class.
    /// </summary>
    /// <remarks>
    /// <p>The dispose ownership is transferred to this stream.</p>
    /// </remarks>
    /// <param name="stream">Base stream.</param>
    public DataStream(Stream stream)
        : this(stream, 0, stream?.Length ?? -1, true)
    {
            canExpand = true;
        }

    /// <summary>
    /// Initializes a new instance of the <see cref="DataStream" /> class
    /// from a substream transferring the ownership of the life-cycle. In the
    /// case the stream is another <see cref="DataStream" /> the ownership
    /// is inherited.
    /// </summary>
    /// <param name="stream">Base stream.</param>
    /// <param name="offset">Offset from the base stream.</param>
    /// <param name="length">Length of this substream.</param>
    [SuppressMessage("", "S1125: remove unnecessary boolean", Justification = "Readability")]
    public DataStream(Stream stream, long offset, long length)
        : this(stream, offset, length, (stream is not DataStream dataStream) || dataStream.hasOwnsership)
    {
        }

    /// <summary>
    /// Initializes a new instance of the <see cref="DataStream" /> class.
    /// </summary>
    /// <param name="stream">Base stream.</param>
    /// <param name="offset">Offset from the base stream.</param>
    /// <param name="length">Length of this substream.</param>
    /// <param name="transferOwnership">
    /// Transfer the ownsership of the stream argument to this class so
    /// it can dispose it.
    /// </param>
    public DataStream(Stream stream, long offset, long length, bool transferOwnership)
    {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            if (offset < 0 || offset > stream.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (length < 0 || offset + length > stream.Length)
                throw new ArgumentOutOfRangeException(nameof(length));

            this.length = length;
            canExpand = false;
            hasOwnsership = transferOwnership;

            if (stream is DataStream dataStream) {
                ParentDataStream = dataStream;
                baseStream = dataStream.BaseStream;
                Offset = dataStream.Offset + offset;

                if (transferOwnership && !dataStream.hasOwnsership) {
                    throw new ArgumentException(
                        "Cannot transfer ownership from another datastream that doesn't have ownership",
                        nameof(transferOwnership));
                }
            } else {
                ParentDataStream = null;
                baseStream = stream;
                Offset = offset;
            }

            streamInfo = GetOrCreateStreamInfo();
        }

    /// <summary>
    /// Gets the number of streams in use.
    /// </summary>
    public static int ActiveStreams => Instances.Values.Count(i => i.NumInstances > 0);

    /// <summary>
    /// Gets a value indicating whether this <see cref="DataStream"/> is disposed.
    /// </summary>
    public bool Disposed {
        get => disposed;
        private set => disposed = value;
    }

    /// <summary>
    /// Gets the offset from the BaseStream.
    /// </summary>
    public long Offset {
        get => offset;
        private set => offset = value;
    }

    /// <summary>
    /// Gets or sets the position from the start of this stream.
    /// </summary>
    public override long Position {
        get => position;

        set {
                if (Disposed)
                    throw new ObjectDisposedException(nameof(DataStream));
                if (value < 0 || value > Length)
                    throw new ArgumentOutOfRangeException(nameof(value));

                position = value;
            }
    }

    /// <summary>
    /// Gets the length of this stream.
    /// </summary>
    public override long Length {
        get => length;
    }

    /// <summary>
    /// Gets the parent DataStream only if this stream was initialized from
    /// a DataStream.
    /// </summary>
    public DataStream? ParentDataStream {
        get;
        private set;
    }

    /// <summary>
    /// Gets the base stream.
    /// </summary>
    public Stream BaseStream {
        get => baseStream;
    }

    /// <summary>
    /// Gets a value indicating whether the position is at end of the stream.
    /// </summary>
    public bool EndOfStream {
        get => Position >= Length;
    }

    /// <summary>
    /// Gets the position from the base stream.
    /// </summary>
    public long AbsolutePosition {
        get => Offset + Position;
    }

    /// <summary>
    /// Gets a value indicating whether the current stream supports reading.
    /// </summary>
    public override bool CanRead {
        get => true;
    }

    /// <summary>
    /// Gets a value indicating whether the current stream supports writing.
    /// </summary>
    public override bool CanWrite {
        get => true;
    }

    /// <summary>
    /// Gets a value indicating whether the current stream supports seeking.
    /// </summary>
    public override bool CanSeek {
        get => true;
    }

    /// <summary>
    /// Gets a value indicating whether the current stream support timeouts.
    /// </summary>
    public override bool CanTimeout => false;

    /// <summary>
    /// Gets or sets an invalid value as read time is not supported.
    /// </summary>
    public override int ReadTimeout {
        get => -1;
        set => throw new InvalidOperationException("Read timeout is not supported");
    }

    /// <summary>
    /// Gets or sets an invalid value as write time is not supported.
    /// </summary>
    public override int WriteTimeout {
        get => -1;
        set => throw new InvalidOperationException("Write timeout is not supported");
    }

    /// <summary>
    /// Gets the internal stream information for testing pourpose only.
    /// </summary>
    internal StreamInfo InternalInfo {
        get => streamInfo;
    }

    /// <summary>
    /// Sets the length of the current stream.
    /// </summary>
    /// <param name="value">The new length value.</param>
    public override void SetLength(long value)
    {
            if (Disposed)
                throw new ObjectDisposedException(nameof(DataStream));
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value));

            if (value > length) {
                if (!canExpand) {
                    throw new InvalidOperationException(
                        "Cannot change the size of sub-streams.");
                }

                lock (streamInfo.LockObj) {
                    if (value > BaseStream.Length) {
                        // If we can expand, it's not a substream so forget
                        // about offset (always 0). Increase base stream too.
                        BaseStream.SetLength(value);
                    }
                }
            }

            length = value;
            if (Position > Length) {
                Position = Length;
            }
        }

    /// <summary>
    /// Clears all buffers for this stream and causes any buffered data
    /// to be written to the underlying device.
    /// </summary>
    public override void Flush()
    {
            if (Disposed)
                throw new ObjectDisposedException(nameof(DataStream));

            BaseStream.Flush();
        }

    /// <summary>
    /// Move the position of the Stream.
    /// </summary>
    /// <param name="shift">Distance to move position.</param>
    /// <param name="mode">Mode to move position.</param>
    [Obsolete("Use the overload with SeekOrigin.")]
    public void Seek(long shift, SeekMode mode)
    {
            if (Disposed)
                throw new ObjectDisposedException(nameof(DataStream));

            switch (mode) {
                case SeekMode.Current:
                    _ = Seek(shift, SeekOrigin.Current);
                    break;
                case SeekMode.Start:
                    _ = Seek(shift, SeekOrigin.Begin);
                    break;
                case SeekMode.End:
                    _ = Seek(shift, SeekOrigin.End);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode));
            }
        }

    /// <summary>
    /// Move the position of the stream.
    /// </summary>
    /// <param name="offset">Distance to move position.</param>
    /// <param name="origin">Mode to move position.</param>
    /// <returns>The new position of the stream.</returns>
    [SuppressMessage("", "S1006", Justification = "It's an good improvement")]
    public override long Seek(long offset, SeekOrigin origin = SeekOrigin.Begin)
    {
            if (Disposed)
                throw new ObjectDisposedException(nameof(DataStream));

            switch (origin) {
                case SeekOrigin.Current:
                    Position += offset;
                    break;
                case SeekOrigin.Begin:
                    Position = offset;
                    break;
                case SeekOrigin.End:
                    Position = Length + offset;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(origin));
            }

            return Position;
        }

    /// <summary>
    /// Push the current position into a stack and move to a new one.
    /// </summary>
    /// <param name="shift">Distance to move position.</param>
    /// <param name="mode">Mode to move position.</param>
    [Obsolete("Use the overload with SeekOrigin.")]
    public void PushToPosition(long shift, SeekMode mode)
    {
            if (Disposed)
                throw new ObjectDisposedException(nameof(DataStream));

            positionStack.Push(Position);
            Seek(shift, mode);
        }

    /// <summary>
    /// Push the current position into a stack and move to a new one.
    /// </summary>
    /// <param name="shift">Distance to move position.</param>
    /// <param name="mode">Mode to move position.</param>
    public void PushToPosition(long shift, SeekOrigin mode = SeekOrigin.Begin)
    {
            if (Disposed)
                throw new ObjectDisposedException(nameof(DataStream));

            positionStack.Push(Position);
            _ = Seek(shift, mode);
        }

    /// <summary>
    /// Push the current position into a stack.
    /// </summary>
    public void PushCurrentPosition()
    {
            if (Disposed)
                throw new ObjectDisposedException(nameof(DataStream));

            positionStack.Push(Position);
        }

    /// <summary>
    /// Pop the last position from the stack and move to it.
    /// </summary>
    public void PopPosition()
    {
            if (Disposed)
                throw new ObjectDisposedException(nameof(DataStream));

            if (positionStack.Count == 0)
                throw new InvalidOperationException("Position stack is empty");

            Position = positionStack.Pop();
        }

    /// <summary>
    /// Run a method in a specific position.
    /// This command will move into the position, run the method and return
    /// to the current position.
    /// </summary>
    /// <param name="action">Action to run.</param>
    /// <param name="position">Position to move.</param>
    /// <param name="mode">Mode to move position.</param>
    [Obsolete("Use the overload with SeekOrigin.")]
    public void RunInPosition(Action action, long position, SeekMode mode)
    {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            PushToPosition(position, mode);
            action();
            PopPosition();
        }

    /// <summary>
    /// Run a method in a specific position.
    /// This command will move into the position, run the method and return
    /// to the current position.
    /// </summary>
    /// <param name="action">Action to run.</param>
    /// <param name="position">Position to move.</param>
    /// <param name="mode">Mode to move position.</param>
    public void RunInPosition(Action action, long position, SeekOrigin mode = SeekOrigin.Begin)
    {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            PushToPosition(position, mode);
            action();
            PopPosition();
        }

    /// <summary>
    /// Reads a byte from the stream and advances the position within the
    /// stream by one byte, or returns -1 if at the end of the stream.
    /// </summary>
    /// <returns>The unsigned byte cast to an Int32, or -1 if at the end of the stream.</returns>
    public override int ReadByte()
    {
            if (disposed)
                throw new ObjectDisposedException(nameof(DataStream));

            if (position >= length)
                return -1;

            lock (streamInfo.LockObj) {
                baseStream.Position = AbsolutePosition;
                position++;
                return baseStream.ReadByte();
            }
        }

    /// <summary>
    /// Reads a sequence of bytes from the current stream and advances the
    /// position within the stream by the number of bytes read.
    /// </summary>
    /// <returns>
    /// The total number of bytes read into the buffer. This can be less than
    /// the number of bytes requested if that many bytes are not currently
    /// available, or zero (0) if the end of the stream has been reached.
    /// </returns>
    /// <param name="buffer">Buffer to copy data.</param>
    /// <param name="offset">Index to start copying in buffer.</param>
    /// <param name="count">Maximum number of bytes to read.</param>
    public override int Read(byte[] buffer, int offset, int count)
    {
            if (Disposed)
                throw new ObjectDisposedException(nameof(DataStream));
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));
            if (offset + count > buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));

            if (count == 0)
                return 0;

            long pos = Position;
            long len = Length;
            if (pos + count > len) {
                count = (int)(len - pos);
            }

            int read = 0;
            lock (streamInfo.LockObj) {
                BaseStream.Position = AbsolutePosition;
                read = BaseStream.Read(buffer, offset, count);
            }

            position = pos + read;
            return read;
        }

    /// <summary>
    /// Reads a format from this stream.
    /// </summary>
    /// <returns>The format read.</returns>
    /// <typeparam name="T">The type of the format to read.</typeparam>
    public T ReadFormat<T>()
    {
            if (Disposed)
                throw new ObjectDisposedException(nameof(DataStream));

            T format;
            using (var binary = new BinaryFormat(this))
                format = ConvertFormat.To<T>(binary);
            return format;
        }

    /// <summary>
    /// Writes a byte.
    /// </summary>
    /// <param name="value">Byte value.</param>
    public override void WriteByte(byte value)
    {
            if (Disposed)
                throw new ObjectDisposedException(nameof(DataStream));

            if (Position == Length && !canExpand)
                throw new InvalidOperationException("Cannot expand stream");

            lock (streamInfo.LockObj) {
                BaseStream.Position = AbsolutePosition;
                BaseStream.WriteByte(value);
            }

            if (Position == Length) {
                SetLength(Length + 1);
            }

            Position++;
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
                throw new ObjectDisposedException(nameof(DataStream));
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));
            if (offset + count > buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));

            long pos = Position;
            long len = Length;
            if (pos + count > len && !canExpand)
                throw new InvalidOperationException("Cannot expand stream");

            if (count == 0)
                return;

            lock (streamInfo.LockObj) {
                BaseStream.Position = AbsolutePosition;
                BaseStream.Write(buffer, offset, count);
            }

            if (pos + count > len) {
                SetLength(pos + count);
            }

            position = pos + count;
        }

    /// <summary>
    /// Writes the complete stream into a file.
    /// </summary>
    /// <param name="fileOut">Output file path.</param>
    /// <remarks>
    /// It preserves the current position and creates any required directory.
    /// </remarks>
    public void WriteTo(string fileOut)
    {
            if (Disposed)
                throw new ObjectDisposedException(nameof(DataStream));
            if (string.IsNullOrEmpty(fileOut))
                throw new ArgumentNullException(nameof(fileOut));

            WriteSegmentTo(0, Length, fileOut);
        }

    /// <summary>
    /// Writes the complete stream into another stream preserving the current position.
    /// </summary>
    /// <param name="stream">The stream to write.</param>
    /// <remarks>
    /// This method is similar to <see cref="Stream.CopyTo(Stream)" />.
    /// The difference is that it copies always from the position 0 of the
    /// current stream, and it preserves the current position afterwards.
    /// It writes into the current position of the destination stream.
    /// </remarks>
    public void WriteTo(Stream stream)
    {
            if (Disposed)
                throw new ObjectDisposedException(nameof(DataStream));
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            WriteSegmentTo(0, Length, stream);
        }

    /// <summary>
    /// Writes a segment of the stream into a file from a defined position to the end.
    /// </summary>
    /// <param name="start">Starting position to read from the current stream.</param>
    /// <param name="fileOut">Output file path.</param>
    /// <remarks>
    /// It preserves the current position and creates any required directory.
    /// </remarks>
    public void WriteSegmentTo(long start, string fileOut)
    {
            if (Disposed)
                throw new ObjectDisposedException(nameof(DataStream));
            if (string.IsNullOrEmpty(fileOut))
                throw new ArgumentNullException(nameof(fileOut));
            if (start < 0 || start > Length)
                throw new ArgumentOutOfRangeException(nameof(start), start, "Invalid offset");

            WriteSegmentTo(start, Length - start, fileOut);
        }

    /// <summary>
    /// Writes a segment of the stream into another stream from a defined position to the end.
    /// </summary>
    /// <param name="start">Starting position to read from the current stream.</param>
    /// <param name="stream">Output stream.</param>
    /// <remarks>
    /// It preserves the current position and writes to the current position
    /// of the destination stream.
    /// </remarks>
    public void WriteSegmentTo(long start, Stream stream)
    {
            if (Disposed)
                throw new ObjectDisposedException(nameof(DataStream));
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            if (start < 0 || start > Length)
                throw new ArgumentOutOfRangeException(nameof(start), start, "Invalid offset");

            WriteSegmentTo(start, Length - start, stream);
        }

    /// <summary>
    /// Writes a segment of the stream into a file.
    /// </summary>
    /// <param name="start">Starting position to read from the current stream.</param>
    /// <param name="length">Length of the segment to read.</param>
    /// <param name="fileOut">Output file path.</param>
    /// <remarks>
    /// It preserves the current position and creates any required directory.
    /// </remarks>
    public void WriteSegmentTo(long start, long length, string fileOut)
    {
            if (Disposed)
                throw new ObjectDisposedException(nameof(DataStream));
            if (string.IsNullOrEmpty(fileOut))
                throw new ArgumentNullException(nameof(fileOut));
            if (start < 0 || start > Length)
                throw new ArgumentOutOfRangeException(nameof(start), start, "Invalid offset");
            if (length < 0 || length > Length)
                throw new ArgumentOutOfRangeException(nameof(length), length, "Invalid length");
            if (start + length > Length)
                throw new ArgumentOutOfRangeException(nameof(start), start + length, "Invalid segment boundary");

            // Parent dir can be empty if we just specified the file name.
            // In that case, the folder (cwd) already exists.
            string? parentDir = Path.GetDirectoryName(fileOut);
            if (!string.IsNullOrEmpty(parentDir)) {
                _ = Directory.CreateDirectory(parentDir);
            }

            // We use FileStream so it creates a file even when the length is zero
            using var segment = new FileStream(fileOut, FileMode.OpenOrCreate, FileAccess.Write);
            WriteSegmentTo(start, length, segment);
        }

    /// <summary>
    /// Writes a segment of the stream into another stream.
    /// </summary>
    /// <param name="start">Starting position to read from the current stream.</param>
    /// <param name="length">Length of the segment to read.</param>
    /// <param name="stream">Output stream.</param>
    /// <remarks>
    /// It preserves the current position and writes to the current position
    /// of the destination stream.
    /// </remarks>
    public void WriteSegmentTo(long start, long length, Stream stream)
    {
            if (Disposed)
                throw new ObjectDisposedException(nameof(DataStream));
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            if (start < 0 || start > Length)
                throw new ArgumentOutOfRangeException(nameof(start), start, "Invalid offset");
            if (length < 0 || length > Length)
                throw new ArgumentOutOfRangeException(nameof(length), length, "Invalid length");
            if (start + length > Length)
                throw new ArgumentOutOfRangeException(nameof(start), start + length, "Invalid segment boundary");

            PushToPosition(start);

            const int MaxBufferSize = 70 * 1024;
            int bufferSize = (length > MaxBufferSize) ? MaxBufferSize : (int)length;
            byte[] buffer = ArrayPool<byte>.Shared.Rent(bufferSize);

            try {
                if (length > buffer.Length) {
                    long endPos = start + length;
                    while (Position < endPos) {
                        int read = BlockRead(this, buffer, endPos);
                        stream.Write(buffer, 0, read);
                    }
                } else {
                    int read = Read(buffer, 0, (int)length);
                    stream.Write(buffer, 0, read);
                }
            } finally {
                ArrayPool<byte>.Shared.Return(buffer);
            }

            PopPosition();
        }

    /// <summary>
    /// Compare the content of the stream with another one.
    /// </summary>
    /// <returns>The result of the comparison.</returns>
    /// <param name="otherStream">Stream to compare with.</param>
    public bool Compare(Stream otherStream)
    {
            if (otherStream == null)
                throw new ArgumentNullException(nameof(otherStream));

            // We can't check if the other stream is disposed because Stream
            // doesn't provide the property, so we delay it to the Seek method.
            if (Disposed)
                throw new ObjectDisposedException(nameof(DataStream));

            long startPosition = Position;
            long otherStreamStartPosition = otherStream.Position;
            _ = Seek(0, SeekOrigin.Begin);
            _ = otherStream.Seek(0, SeekOrigin.Begin);

            if (Length != otherStream.Length) {
                return false;
            }

            const int BufferSize = 70 * 1024;
            byte[] buffer1 = new byte[Length > BufferSize ? BufferSize : Length];
            byte[] buffer2 = new byte[buffer1.Length];

            bool result = true;
            while (!EndOfStream && result) {
                // As we have checked the length before, we assume we read the same
                int loopLength = Read(buffer1, 0, buffer1.Length);
                _ = otherStream.Read(buffer2, 0, buffer2.Length);

                for (int i = 0; i < loopLength && result; i++) {
                    if (buffer1[i] != buffer2[i]) {
                        result = false;
                    }
                }
            }

            _ = Seek(startPosition, SeekOrigin.Begin);
            _ = otherStream.Seek(otherStreamStartPosition, SeekOrigin.Begin);

            return result;
        }

    /// <summary>
    /// Releases all resource used by the <see cref="DataStream"/>
    /// object.
    /// </summary>
    /// <param name="disposing">If set to
    /// <see langword="true" /> free managed resources also.</param>
    protected override void Dispose(bool disposing)
    {
            if (Disposed)
                return;

            Disposed = true;

            // We have only managed resources, so if it's a finalizer call stop.
            // If we don't have the ownership no need to decrease counter or dispose
            if (!disposing || !hasOwnsership) {
                return;
            }

            streamInfo.DecreaseInstancesAndRun(
                info => {
                    if (info.NumInstances == 0) {
                        BaseStream.Dispose();
                        _ = Instances.TryRemove(BaseStream, out _);
                    }
                });
        }

    private static int BlockRead(Stream stream, byte[] buffer, long endPosition)
    {
            int read;
            if (stream.Position + buffer.Length > endPosition) {
                read = (int)(endPosition - stream.Position);
            } else {
                read = buffer.Length;
            }

            _ = stream.Read(buffer, 0, read);
            return read;
        }

    private StreamInfo GetOrCreateStreamInfo()
    {
            StreamInfo info = Instances.GetOrAdd(BaseStream, new StreamInfo());

            // If we have ownership, we increase the counter so we take into
            // account for disposing. We create always the entry for the lock.
            if (hasOwnsership) {
                info.IncreaseInstances();
            }

            return info;
        }

    /// <summary>
    /// Information of the stream for the DataStream class.
    /// </summary>
    internal sealed class StreamInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref=" StreamInfo"/> class.
        /// </summary>
        public StreamInfo()
        {
                LockObj = new object();
                NumInstances = 0;
            }

        /// <summary>
        /// Gets the number of DataStream instances for the base stream.
        /// </summary>
        public int NumInstances { get; private set; }

        /// <summary>
        /// Gets the lock object for the base stream operations.
        /// </summary>
        public object LockObj { get; }

        /// <summary>
        /// Increase the number of instances using the base stream.
        /// </summary>
        public void IncreaseInstances()
        {
                lock (LockObj) {
                    NumInstances++;
                }
            }

        /// <summary>
        /// Decrease the number of instances using the base stream.
        /// </summary>
        public void DecreaseInstances()
        {
                lock (LockObj) {
                    NumInstances--;
                }
            }

        /// <summary>
        /// Decrease the number of instances and run the action.
        /// </summary>
        /// <param name="action">The action to run.</param>
        public void DecreaseInstancesAndRun(Action<StreamInfo> action)
        {
                lock (LockObj) {
                    DecreaseInstances();
                    action(this);
                }
            }
    }
}
