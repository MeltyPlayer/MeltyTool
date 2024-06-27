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
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using Serialization.Attributes;
    using SerializableAttribute = Serialization.Attributes.SerializableAttribute;

    /// <summary>
    /// Binary DataReader for DataStreams.
    /// </summary>
    public class DataReader
    {
        static DataReader()
        {
            // Make sure that the shift-jis encoding is initialized.
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Yarhl.IO.DataReader"/> class.
        /// </summary>
        /// <param name="stream">Stream to read from.</param>
        /// <remarks>
        /// <para>By default the endianness is LittleEndian and
        /// the encoding is UTF-8.</para>
        /// </remarks>
        public DataReader(Stream stream)
        {
            if (stream is null)
                throw new ArgumentNullException(nameof(stream));

            Stream = stream as DataStream ?? new DataStream(stream, 0, stream.Length, false);
            Endianness = EndiannessMode.LittleEndian;
            DefaultEncoding = new UTF8Encoding(false, true);
        }

        /// <summary>
        /// Gets the stream.
        /// </summary>
        public DataStream Stream {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the endianness.
        /// </summary>
        public EndiannessMode Endianness {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the default encoding.
        /// </summary>
        public Encoding DefaultEncoding {
            get;
            set;
        }

        /// <summary>
        /// Reads a 8-bit byte number.
        /// </summary>
        /// <returns>The next byte.</returns>
        public byte ReadByte()
        {
            if (Stream.Position >= Stream.Length)
                throw new EndOfStreamException();

            return (byte)Stream.ReadByte();
        }

        /// <summary>
        /// Reads a signed 8-bit byte number.
        /// </summary>
        /// <returns>The next signed byte.</returns>
        [CLSCompliant(false)]
        public sbyte ReadSByte()
        {
            return (sbyte)ReadByte();
        }

        /// <summary>
        /// Reads an unsigned 16-bit number.
        /// </summary>
        /// <returns>The next 16-bit number.</returns>
        [CLSCompliant(false)]
        public ushort ReadUInt16()
        {
            if (Endianness == EndiannessMode.LittleEndian)
                return (ushort)(ReadByte() | (ReadByte() << 8));
            if (Endianness == EndiannessMode.BigEndian)
                return (ushort)((ReadByte() << 8) | ReadByte());

            throw new NotSupportedException($"Endianness not supported: {Endianness}");
        }

        /// <summary>
        /// Reads a signed 16-bit number.
        /// </summary>
        /// <returns>The next signed 16-bit number.</returns>
        public short ReadInt16()
        {
            return (short)ReadUInt16();
        }

        /// <summary>
        /// Reads a 24-bit number.
        /// </summary>
        /// <returns>The next 24-bit number.</returns>
        public int ReadInt24()
        {
            if (Endianness == EndiannessMode.LittleEndian)
                return ReadByte() | (ReadByte() << 8) | (ReadByte() << 16);
            if (Endianness == EndiannessMode.BigEndian)
                return (ReadByte() << 16) | (ReadByte() << 8) | ReadByte();

            throw new NotSupportedException($"Endianness not supported: {Endianness}");
        }

        /// <summary>
        /// Reads an unsigned 32-bit number.
        /// </summary>
        /// <returns>The next unsigned 32-bit number.</returns>
        [CLSCompliant(false)]
        public uint ReadUInt32()
        {
            if (Endianness == EndiannessMode.LittleEndian)
                return (uint)(ReadUInt16() | (ReadUInt16() << 16));
            if (Endianness == EndiannessMode.BigEndian)
                return (uint)((ReadUInt16() << 16) | ReadUInt16());

            throw new NotSupportedException($"Endianness not supported: {Endianness}");
        }

        /// <summary>
        /// Reads a signed 32-bit number.
        /// </summary>
        /// <returns>The next signed 32-bit number.</returns>
        public int ReadInt32()
        {
            return (int)ReadUInt32();
        }

        /// <summary>
        /// Reads an unsigned 64-bit number.
        /// </summary>
        /// <returns>The next unsigned 64-bit number.</returns>
        [CLSCompliant(false)]
        public ulong ReadUInt64()
        {
            if (Endianness == EndiannessMode.LittleEndian)
                return ReadUInt32() | ((ulong)ReadUInt32() << 32);
            if (Endianness == EndiannessMode.BigEndian)
                return ((ulong)ReadUInt32() << 32) | ReadUInt32();

            throw new NotSupportedException($"Endianness not supported: {Endianness}");
        }

        /// <summary>
        /// Reads a signed 64-bit number.
        /// </summary>
        /// <returns>The next signed 64-bit number.</returns>
        public long ReadInt64()
        {
            return (long)ReadUInt64();
        }

        /// <summary>
        /// Reads a 32-bits IEEE 754 single precision floating-point number.
        /// </summary>
        /// <returns>The next float number.</returns>
        public float ReadSingle()
        {
            if (Endianness == EndiannessMode.LittleEndian)
                return BitConverter.ToSingle(ReadBytes(4), 0);
            if (Endianness == EndiannessMode.BigEndian)
                return BitConverter.ToSingle(ReadBytes(4).Reverse().ToArray(), 0);

            throw new NotSupportedException($"Endianness not supported: {Endianness}");
        }

        /// <summary>
        /// Reads a 64-bits IEEE 754 double precision floating-point number.
        /// </summary>
        /// <returns>The next double number.</returns>
        public double ReadDouble()
        {
            if (Endianness == EndiannessMode.LittleEndian)
                return BitConverter.ToDouble(ReadBytes(8), 0);
            if (Endianness == EndiannessMode.BigEndian)
                return BitConverter.ToDouble(ReadBytes(8).Reverse().ToArray(), 0);

            throw new NotSupportedException($"Endianness not supported: {Endianness}");
        }

        /// <summary>
        /// Reads bytes from the stream.
        /// </summary>
        /// <returns>The bytes read.</returns>
        /// <param name="count">Number of bytes to read.</param>
        public byte[] ReadBytes(int count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            if (Stream.Position + count > Stream.Length)
                throw new EndOfStreamException();

            byte[] buffer = new byte[count];
            _ = Stream.Read(buffer, 0, count);
            return buffer;
        }

        /// <summary>
        /// Reads a char.
        /// </summary>
        /// <remarks>
        /// This method read one code units. A code unit may not represent a full
        /// grapheme. This method may return corrupted code units and may
        /// advance a wrong number of bytes if the given number of code units to
        /// read are not enough to represent a grapheme.
        /// </remarks>
        /// <returns>The next char.</returns>
        /// <param name="encoding">
        /// Encoding to use or <c>null</c> to use <see cref="DefaultEncoding" />.
        /// </param>
        public char ReadChar(Encoding? encoding = null)
        {
            return ReadChars(1, encoding)[0];
        }

        /// <summary>
        /// Reads an array of chars.
        /// </summary>
        /// <remarks>
        /// This method reads code units. A code unit may not represent a full
        /// grapheme. This method may return corrupted code units and may
        /// advance a wrong number of bytes if the given number of code units to
        /// read are not enough to represent a grapheme.
        /// </remarks>
        /// <returns>The chars (code-units) read.</returns>
        /// <param name="count">The number of chars (code-units) to read.</param>
        /// <param name="encoding">
        /// Encoding to use or <c>null</c> to use <see cref="DefaultEncoding" />.
        /// </param>
        public char[] ReadChars(int count, Encoding? encoding = null)
        {
            if (encoding == null)
                encoding = DefaultEncoding;

            long startPos = Stream.Position;
            long streamLength = Stream.Length;

            // By cloning the encoding, it becomes non-readonly so we can
            // change the DecoderFallback to one that won't throw exceptions
            // if it finds non-text bytes. Yarhl issue #134
            Encoding encodingNoException = (Encoding)encoding.Clone();
            encodingNoException.DecoderFallback = DecoderFallback.ReplacementFallback;

            // Read as much as bytes as we can to try to decode the given number
            // of chars.
            int maxBytes = encoding.GetMaxByteCount(count);
            if (maxBytes > streamLength - startPos) {
                maxBytes = (int)(streamLength - startPos);
            }

            byte[] buffer = ReadBytes(maxBytes);
            char[] charArray = encodingNoException.GetChars(buffer);

            if (charArray.Length < count) {
                throw new EndOfStreamException();
            }

            // Now that we have the required chars, check how many bytes actually
            // takes to get them and decode again with the original fallbacks
            int exactBytes = encoding.GetByteCount(charArray, 0, count);
            charArray = encoding.GetChars(buffer, 0, exactBytes);
            _ = Stream.Seek(startPos + exactBytes, SeekOrigin.Begin);

            return charArray;
        }

        /// <summary>
        /// Reads a string until a string token is found.
        /// </summary>
        /// <returns>The read string.</returns>
        /// <param name="token">Token to find.</param>
        /// <param name="encoding">
        /// Encoding to use or <c>null</c> to use <see cref="DefaultEncoding" />.
        /// </param>
        public string ReadStringToToken(string token, Encoding? encoding = null)
        {
            if (string.IsNullOrEmpty(token))
                throw new ArgumentNullException(nameof(token));

            if (encoding == null)
                encoding = DefaultEncoding;

            // By cloning the encoding, it becomes non-readonly so we can
            // change the DecoderFallback to one that won't throw exceptions
            // if it finds non-text bytes. Yarhl issue #134
            Encoding encodingNoException = (Encoding)encoding.Clone();
            encodingNoException.DecoderFallback = DecoderFallback.ReplacementFallback;

            long startPos = Stream.Position;
            long streamLength = Stream.Length;

            // Gather bytes from buffer to buffer into a list and try to
            // convert to find the token. This approach is faster since we
            // read blocks and it avoids issues with half-encoded chars.
            const int BufferSize = 128;
            byte[] buffer = new byte[BufferSize];

            List<byte> textBuffer = [];
            string text = string.Empty;
            int matchIndex = -1;

            while (matchIndex == -1) {
                if (Stream.Position >= Stream.Length) {
                    throw new EndOfStreamException();
                }

                // Read buffer size if possible, otherwise remaining bytes
                long currentPosition = Stream.Position;
                int size = currentPosition + BufferSize <= streamLength ?
                    BufferSize :
                    (int)(streamLength - currentPosition);

                int read = Stream.Read(buffer, 0, size);
                textBuffer.AddRange(buffer.Take(read));

                text = encodingNoException.GetString(textBuffer.ToArray());
                matchIndex = text.IndexOf(token, StringComparison.Ordinal);
            }

            // Get the final string and the number exact of bytes to seek
            int endPos = matchIndex + token.Length;
            int exactBytes = encoding.GetByteCount(text.ToCharArray(0, endPos));
            _ = Stream.Seek(startPos + exactBytes, SeekOrigin.Begin);

            // Now we know the number of bytes, decode again with the original
            // encoding so it can apply its original decoder fallback.
            text = encoding.GetString(textBuffer.ToArray(), 0, exactBytes);
            text = text[..^token.Length];

            return text;
        }

        /// <summary>
        /// Reads a string that ends with the null terminator.
        /// </summary>
        /// <returns>The string.</returns>
        /// <param name="encoding">Optional encoding to use.</param>
        public string ReadString(Encoding? encoding = null)
        {
            return ReadStringToToken("\0", encoding);
        }

        /// <summary>
        /// Reads a string with a constant size.
        /// </summary>
        /// <returns>The string.</returns>
        /// <param name="bytesCount">Size of the string in bytes.</param>
        /// <param name="encoding">Optional encoding to use.</param>
        public string ReadString(int bytesCount, Encoding? encoding = null)
        {
            if (encoding == null)
                encoding = DefaultEncoding;

            byte[] buffer = ReadBytes(bytesCount);
            return encoding.GetString(buffer);
        }

        /// <summary>
        /// Reads the size with a size field first.
        /// </summary>
        /// <returns>The string.</returns>
        /// <param name="sizeType">Type of the size field.</param>
        /// <param name="encoding">Optional encoding to use.</param>
        public string ReadString(Type sizeType, Encoding? encoding = null)
        {
            if (encoding == null)
                encoding = DefaultEncoding;

            dynamic size = ReadByType(sizeType);
            size = Convert.ChangeType(size, typeof(int), CultureInfo.InvariantCulture);
            return ReadString(size, encoding);
        }

        /// <summary>
        /// Reads a field by type.
        /// </summary>
        /// <returns>The field.</returns>
        /// <remarks>Nullable types are not supported.</remarks>
        /// <param name="type">Type of the field.</param>
        public dynamic ReadByType(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            bool serializable = Attribute.IsDefined(type, typeof(SerializableAttribute));
            if (serializable)
                return ReadUsingReflection(type);

            if (type == typeof(long))
                return ReadInt64();
            if (type == typeof(ulong))
                return ReadUInt64();
            if (type == typeof(int))
                return ReadInt32();
            if (type == typeof(uint))
                return ReadUInt32();
            if (type == typeof(short))
                return ReadInt16();
            if (type == typeof(ushort))
                return ReadUInt16();
            if (type == typeof(byte))
                return ReadByte();
            if (type == typeof(sbyte))
                return ReadSByte();
            if (type == typeof(char))
                return ReadChar();
            if (type == typeof(string))
                return ReadString();
            if (type == typeof(float))
                return ReadSingle();
            if (type == typeof(double))
                return ReadDouble();

            throw new FormatException("Unsupported type");
        }

        /// <summary>
        /// Read a field by type.
        /// </summary>
        /// <returns>The field.</returns>
        /// <typeparam name="T">The type of the field.</typeparam>
        public dynamic Read<T>()
        {
            return ReadByType(typeof(T));
        }

        /// <summary>
        /// Skip bytes to pad the position in the stream.
        /// </summary>
        /// <param name="padding">Padding value.</param>
        public void SkipPadding(int padding)
        {
            if (padding < 0)
                throw new ArgumentOutOfRangeException(nameof(padding));

            if (padding <= 1) {
                return;
            }

            long remainingBytes = Stream.Position.Pad(padding) - Stream.Position;
            if (remainingBytes > 0) {
                _ = Stream.Seek(remainingBytes, SeekOrigin.Current);
            }
        }

        dynamic ReadUsingReflection(Type type)
        {
            // It returns null for Nullable<T>, but as that is a class and
            // it won't have the serializable attribute, it will throw an
            // unsupported exception before. So this can't be null at this point.
            #pragma warning disable SA1009 // False positive
            object obj = Activator.CreateInstance(type)!;
            #pragma warning restore SA1009

            PropertyInfo[] properties = type.GetProperties(
                BindingFlags.DeclaredOnly |
                BindingFlags.Public |
                BindingFlags.Instance);

            foreach (PropertyInfo property in properties) {
                bool ignore = Attribute.IsDefined(property, typeof(BinaryIgnoreAttribute));
                if (ignore) {
                    continue;
                }

                EndiannessMode currentEndianness = Endianness;
                bool forceEndianness = Attribute.IsDefined(property, typeof(BinaryForceEndiannessAttribute));
                if (forceEndianness) {
                    var attr = Attribute.GetCustomAttribute(property, typeof(BinaryForceEndiannessAttribute)) as BinaryForceEndiannessAttribute;
                    Endianness = attr!.Mode;
                }

                if (property.PropertyType == typeof(bool) && Attribute.IsDefined(property, typeof(BinaryBooleanAttribute))) {
                    // booleans can only be read if they have the attribute.
                    var attr = Attribute.GetCustomAttribute(property, typeof(BinaryBooleanAttribute)) as BinaryBooleanAttribute;
                    dynamic value = ReadByType(attr!.ReadAs);
                    property.SetValue(obj, value == (dynamic)attr.TrueValue);
                } else if (property.PropertyType == typeof(int) && Attribute.IsDefined(property, typeof(BinaryInt24Attribute))) {
                    // read the number as int24.
                    int value = ReadInt24();
                    property.SetValue(obj, value);
                } else if (property.PropertyType.IsEnum && Attribute.IsDefined(property, typeof(BinaryEnumAttribute))) {
                    // enums can only be read if they have the attribute.
                    var attr = Attribute.GetCustomAttribute(property, typeof(BinaryEnumAttribute)) as BinaryEnumAttribute;
                    dynamic value = ReadByType(attr!.ReadAs);
                    property.SetValue(obj, Enum.ToObject(property.PropertyType, value));
                } else if (property.PropertyType == typeof(string) && Attribute.IsDefined(property, typeof(BinaryStringAttribute))) {
                    var attr = Attribute.GetCustomAttribute(property, typeof(BinaryStringAttribute)) as BinaryStringAttribute;
                    Encoding? encoding = null;
                    if (attr!.CodePage != -1) {
                        encoding = Encoding.GetEncoding(attr.CodePage);
                    }

                    dynamic value;
                    if (attr.SizeType == null) {
                        value = attr.FixedSize == -1 ? this.ReadString(encoding) : this.ReadString(attr.FixedSize, encoding);
                    } else {
                        value = ReadString(attr.SizeType, encoding);
                    }

                    property.SetValue(obj, value);
                } else {
                    dynamic value = ReadByType(property.PropertyType);
                    property.SetValue(obj, value);
                }

                // Restore previous endianness
                Endianness = currentEndianness;
            }

            return obj;
        }
    }
}
