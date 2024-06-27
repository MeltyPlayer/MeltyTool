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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Serialization.Attributes;
using SerializableAttribute = Serialization.Attributes.SerializableAttribute;

/// <summary>
/// Binary writer for DataStreams.
/// </summary>
public class DataWriter
{
    static DataWriter()
    {
            // Make sure that the shift-jis encoding is initialized.
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

    /// <summary>
    /// Initializes a new instance of the <see cref="Yarhl.IO.DataWriter"/> class.
    /// </summary>
    /// <param name="stream">Stream to write to.</param>
    /// <remarks>
    /// <para>By default the endianess is LittleEndian and
    /// the encoding is UTF-8.</para>
    /// </remarks>
    public DataWriter(Stream stream)
    {
            if (stream is null)
                throw new ArgumentNullException(nameof(stream));

            Stream = stream as DataStream ?? new DataStream(stream, 0, stream.Length, false);
            Endianness = EndiannessMode.LittleEndian;
            DefaultEncoding = Encoding.UTF8;
        }

    /// <summary>
    /// Gets the stream.
    /// </summary>
    /// <value>The stream.</value>
    public DataStream Stream {
        get;
        private set;
    }

    /// <summary>
    /// Gets or sets the endianness.
    /// </summary>
    /// <value>The endianness.</value>
    public EndiannessMode Endianness {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the default encoding.
    /// </summary>
    /// <value>The default encoding.</value>
    public Encoding DefaultEncoding {
        get;
        set;
    }

    /// <summary>
    /// Write the specified 8-bits byte value.
    /// </summary>
    /// <param name="val">Byte value.</param>
    public void Write(byte val)
    {
            Stream.WriteByte(val);
        }

    /// <summary>
    /// Write the specified 8-bits signed byte value.
    /// </summary>
    /// <param name="val">Signed byte value.</param>
    [CLSCompliant(false)]
    public void Write(sbyte val)
    {
            Stream.WriteByte((byte)val);
        }

    /// <summary>
    /// Write the specified 16-bits signed value.
    /// </summary>
    /// <param name="val">16-bits signed value.</param>
    public void Write(short val)
    {
            WriteNumber((ushort)val, 16);
        }

    /// <summary>
    /// Write the specified 16-bits unsigned value.
    /// </summary>
    /// <param name="val">16-bits unsigned value.</param>
    [CLSCompliant(false)]
    public void Write(ushort val)
    {
            WriteNumber(val, 16);
        }

    /// <summary>
    /// Write the specified 32-bits signed value.
    /// </summary>
    /// <param name="val">32-bits signed value.</param>
    public void Write(int val)
    {
            WriteNumber((uint)val, 32);
        }

    /// <summary>
    /// Write the specified 32-bits unsigned value.
    /// </summary>
    /// <param name="val">32-bits unsigned value.</param>
    [CLSCompliant(false)]
    public void Write(uint val)
    {
            WriteNumber(val, 32);
        }

    /// <summary>
    /// Write the specified 64-bits signed value.
    /// </summary>
    /// <param name="val">64-bits signed value.</param>
    public void Write(long val)
    {
            WriteNumber((ulong)val, 64);
        }

    /// <summary>
    /// Write the specified 64-bits unsigned value.
    /// </summary>
    /// <param name="val">64-bits unsigned value.</param>
    [CLSCompliant(false)]
    public void Write(ulong val)
    {
            WriteNumber(val, 64);
        }

    /// <summary>
    /// Write the specified 32-bits IEEE 754 single precision
    /// floating point value.
    /// </summary>
    /// <param name="val">Single precision floating point value.</param>
    public void Write(float val)
    {
            if (Endianness == EndiannessMode.LittleEndian)
                Write(BitConverter.GetBytes(val));
            else if (Endianness == EndiannessMode.BigEndian)
                Write(BitConverter.GetBytes(val).Reverse().ToArray());
            else
                throw new NotSupportedException($"Endianness not supported: {Endianness}");
        }

    /// <summary>
    /// Write the specified 64-bits IEEE 754 double precision
    /// floating point value.
    /// </summary>
    /// <param name="val">Double precision floating point value.</param>
    public void Write(double val)
    {
            if (Endianness == EndiannessMode.LittleEndian)
                Write(BitConverter.GetBytes(val));
            else if (Endianness == EndiannessMode.BigEndian)
                Write(BitConverter.GetBytes(val).Reverse().ToArray());
            else
                throw new NotSupportedException($"Endianness not supported: {Endianness}");
        }

    /// <summary>
    /// Write the specified byte buffer.
    /// </summary>
    /// <param name="buffer">Byte buffer.</param>
    public void Write(byte[] buffer)
    {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            Stream.Write(buffer, 0, buffer.Length);
        }

    /// <summary>
    /// Write the specified char using a text encoding.
    /// </summary>
    /// <param name="ch">Char to write.</param>
    /// <param name="encoding">Optional text encoding to use.</param>
    /// <remarks>
    /// <para>If the encoding is null, it will use the default encoding.</para>
    /// </remarks>
    public void Write(char ch, Encoding? encoding = null)
    {
            if (encoding == null)
                encoding = DefaultEncoding;

            Write(encoding.GetBytes(new[] { ch }));
        }

    /// <summary>
    /// Write the specified chars using a text encoding.
    /// </summary>
    /// <param name="chars">Char array to write.</param>
    /// <param name="encoding">Optional text encoding to use.</param>
    /// <remarks>
    /// <para>If the encoding is null, it will use the default encoding.</para>
    /// </remarks>
    public void Write(char[] chars, Encoding? encoding = null)
    {
            if (chars == null)
                throw new ArgumentNullException(nameof(chars));

            if (encoding == null)
                encoding = DefaultEncoding;

            Write(encoding.GetBytes(chars));
        }

    /// <summary>
    /// Write a text string.
    /// </summary>
    /// <param name="text">Text string to write.</param>
    /// <param name="nullTerminator">
    /// If set to <see langword="true" /> add null terminator.
    /// </param>
    /// <param name="encoding">Text encoding to use.</param>
    /// <param name="maxSize">Maximum size of the encoded string in bytes.</param>
    /// <remarks>
    /// <para>If the encoding is null, it will use the default encoding.</para>
    /// </remarks>
    public void Write(
        string text,
        bool nullTerminator = true,
        Encoding? encoding = null,
        int maxSize = -1)
    {
            Write(text, nullTerminator ? "\0" : null, encoding, maxSize);
        }

    /// <summary>
    /// Write a text string with a fixed size.
    /// </summary>
    /// <param name="text">Text string to write.</param>
    /// <param name="fixedSize">Fixed size of the encoded string in bytes.</param>
    /// <param name="nullTerminator">
    /// If set to <see langword="true" /> add null terminator.
    /// </param>
    /// <param name="encoding">Text encoding to use.</param>
    /// <remarks>
    /// <para>If the encoding is null, it will use the default encoding.</para>
    /// </remarks>
    public void Write(
        string text,
        int fixedSize,
        bool nullTerminator = true,
        Encoding? encoding = null)
    {
            Write(text, fixedSize, nullTerminator ? "\0" : null, encoding);
        }

    /// <summary>
    /// Write a text string and its size.
    /// </summary>
    /// <param name="text">Text string to write.</param>
    /// <param name="sizeType">Type of the string size to write.</param>
    /// <param name="nullTerminator">
    /// If set to <see langword="true" /> add null terminator.
    /// </param>
    /// <param name="encoding">Text encoding to use.</param>
    /// <param name="maxSize">Maximum size of the encoded string in bytes.</param>
    /// <remarks>
    /// <para>If the encoding is null, it will use the default encoding.</para>
    /// </remarks>
    public void Write(
        string text,
        Type sizeType,
        bool nullTerminator = false,
        Encoding? encoding = null,
        int maxSize = -1)
    {
            Write(text, sizeType, nullTerminator ? "\0" : null, encoding, maxSize);
        }

    /// <summary>
    /// Write a text string using a custom terminator.
    /// </summary>
    /// <param name="text">Text string to write.</param>
    /// <param name="terminator">
    /// Token to add as terminator.
    /// <remarks>If null, then no token will be added.</remarks>
    /// </param>
    /// <param name="encoding">Text encoding to use.</param>
    /// <param name="maxSize">Maximum size of the encoded string in bytes.</param>
    /// <remarks>
    /// <para>If the encoding is null, it will use the default encoding.</para>
    /// </remarks>
    public void Write(
        string text,
        string? terminator,
        Encoding? encoding = null,
        int maxSize = -1)
    {
            if (text == null)
                throw new ArgumentNullException(nameof(text));
            if (maxSize < -1)
                throw new ArgumentOutOfRangeException(nameof(maxSize));

            if (encoding == null)
                encoding = DefaultEncoding;

            if (!string.IsNullOrEmpty(terminator) && !text.EndsWith(terminator, StringComparison.Ordinal))
                text = string.Concat(text, terminator);

            int textSize = encoding.GetByteCount(text);
            if (maxSize != -1 && textSize > maxSize)
                textSize = maxSize;

            Write(text, textSize, terminator, encoding);
        }

    /// <summary>
    /// Write a text string with a fixed size and a custom terminator.
    /// </summary>
    /// <param name="text">Text string to write.</param>
    /// <param name="fixedSize">Fixed size of the encoded string in bytes.</param>
    /// <param name="terminator">
    /// Token to add as terminator.
    /// <remarks>If null, then no token will be added.</remarks>
    /// </param>
    /// <param name="encoding">Text encoding to use.</param>
    /// <remarks>
    /// <para>If the encoding is null, it will use the default encoding.</para>
    /// </remarks>
    public void Write(
        string text,
        int fixedSize,
        string? terminator,
        Encoding? encoding = null)
    {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            if (encoding == null)
                encoding = DefaultEncoding;

            byte[] buffer = encoding.GetBytes(text);
            Array.Resize(ref buffer, fixedSize);

            // There is no problem having already the terminator since in that
            // case it will overwrite it.
            if (!string.IsNullOrEmpty(terminator)) {
                byte[] nullChar = encoding.GetBytes(terminator);
                for (int i = 0; i < nullChar.Length; i++)
                    buffer[fixedSize - nullChar.Length + i] = nullChar[i];
            }

            Write(buffer);
        }

    /// <summary>
    /// Write a text string and its size.
    /// </summary>
    /// <param name="text">Text string to write.</param>
    /// <param name="sizeType">Type of the string size to write.</param>
    /// <param name="terminator">
    /// Token to add as terminator.
    /// <remarks>If null, then no token will be added.</remarks>
    /// </param>
    /// <param name="encoding">Text encoding to use.</param>
    /// <param name="maxSize">Maximum size of the encoded string in bytes.</param>
    /// <remarks>
    /// <para>If the encoding is null, it will use the default encoding.</para>
    /// </remarks>
    public void Write(
        string text,
        Type sizeType,
        string? terminator,
        Encoding? encoding = null,
        int maxSize = -1)
    {
            if (text == null)
                throw new ArgumentNullException(nameof(text));
            if (sizeType == null)
                throw new ArgumentNullException(nameof(sizeType));
            if (maxSize < -1)
                throw new ArgumentOutOfRangeException(nameof(maxSize));

            if (encoding == null)
                encoding = DefaultEncoding;

            if (!string.IsNullOrEmpty(terminator) && !text.EndsWith(terminator, StringComparison.Ordinal))
                text = string.Concat(text, terminator);

            int textSize = encoding.GetByteCount(text);
            if (maxSize != -1 && textSize > maxSize)
                textSize = maxSize;

            WriteOfType(sizeType, textSize);
            Write(text, textSize, terminator, encoding);
        }

    /// <summary>
    /// Write the specified value converting to any supported type.
    /// </summary>
    /// <param name="type">Type of the value.</param>
    /// <param name="val">Value to write.</param>
    /// <remarks>
    /// <para>The supported types are: long, ulong, int, uint, short,
    /// ushort, byte, sbyte, char and string.</para>
    /// </remarks>
    public void WriteOfType(Type type, dynamic val)
    {
            if (val == null)
                throw new ArgumentNullException(nameof(val));
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            val = Convert.ChangeType(val, type, CultureInfo.InvariantCulture);

            bool serializable = Attribute.IsDefined(type, typeof(SerializableAttribute));
            if (serializable) {
                WriteUsingReflection(type, val);
            } else {
                switch (val) {
                    case long l:
                        Write(l);
                        break;
                    case ulong ul:
                        Write(ul);
                        break;

                    case int i:
                        Write(i);
                        break;
                    case uint ui:
                        Write(ui);
                        break;

                    case short s:
                        Write(s);
                        break;
                    case ushort us:
                        Write(us);
                        break;

                    case byte b:
                        Write(b);
                        break;
                    case sbyte sb:
                        Write(sb);
                        break;

                    case char ch:
                        Write(ch);
                        break;
                    case string str:
                        Write(str);
                        break;

                    case float f:
                        Write(f);
                        break;

                    case double d:
                        Write(d);
                        break;

                    default:
                        throw new FormatException("Unsupported type");
                }
            }
        }

    /// <summary>
    /// Write the specified value forcing the type in the generic.
    /// </summary>
    /// <param name="val">Value to write.</param>
    /// <typeparam name="T">The type of the value.</typeparam>
    public void WriteOfType<T>(T val)
    {
            if (val == null)
                throw new ArgumentNullException(nameof(val));

            WriteOfType(typeof(T), val);
        }

    /// <summary>
    /// Writes the same byte the specified number of times.
    /// </summary>
    /// <param name="val">Value to write.</param>
    /// <param name="times">Number of times to write the byte.</param>
    public void WriteTimes(byte val, long times)
    {
            const int BufferSize = 5 * 1024;

            if (times < 0)
                throw new ArgumentOutOfRangeException(nameof(times));

            byte[] buffer = new byte[BufferSize];
            for (int i = 0; i < BufferSize; i++)
                buffer[i] = val;

            int written = 0;
            int bytesToWrite;
            do {
                if (written + BufferSize > times) {
                    bytesToWrite = (int)(times - written);
                } else {
                    bytesToWrite = BufferSize;
                }

                written += bytesToWrite;
                Stream.Write(buffer, 0, bytesToWrite);
            } while (written != times);
        }

    /// <summary>
    /// Writes the same byte until the given stream length is reached.
    /// </summary>
    /// <param name="val">Value to repeat.</param>
    /// <param name="length">Stream length to reach.</param>
    public void WriteUntilLength(byte val, long length)
    {
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length));

            if (length <= Stream.Length)
                return;

            // We only increase the size of the stream by writing at the end
            _ = Stream.Seek(0, SeekOrigin.End);
            long times = length - Stream.Length;
            WriteTimes(val, times);
        }

    /// <summary>
    /// Write the same byte to pad the stream.
    /// </summary>
    /// <param name="val">Value to repeat.</param>
    /// <param name="padding">Padding value.</param>
    public void WritePadding(byte val, int padding)
    {
            if (padding < 0)
                throw new ArgumentOutOfRangeException(nameof(padding));

            if (padding <= 1) {
                return;
            }

            WriteTimes(val, Stream.Position.Pad(padding) - Stream.Position);
        }

    void WriteNumber(ulong number, byte numBits)
    {
            byte start;
            byte end;
            int step;

            if (Endianness == EndiannessMode.LittleEndian) {
                start = 0;
                end = numBits;
                step = 8;
            } else if (Endianness == EndiannessMode.BigEndian) {
                start = (byte)(numBits - 8);
                end = 0xF8; // When the counter var reach < 0 it overflows to 0-8=0xF8
                step = -8;
            } else {
                throw new NotSupportedException($"Endianness not supported: {Endianness}");
            }

            for (byte i = start; i < end; i = (byte)(i + step)) {
                byte val = (byte)((number >> i) & 0xFF);
                Stream.WriteByte(val);
            }
        }

    void WriteUsingReflection(Type type, dynamic obj)
    {
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
                var endiannessAttr = property.GetCustomAttribute<BinaryForceEndiannessAttribute>();
                if (endiannessAttr is not null) {
                    Endianness = endiannessAttr.Mode;
                }

                dynamic value = property.GetValue(obj);

                if (property.PropertyType == typeof(bool) && property.GetCustomAttribute<BinaryBooleanAttribute>() is { } boolAttr) {
                    // booleans can only be written if they have the attribute.
                    dynamic typeValue = value ? boolAttr.TrueValue : boolAttr.FalseValue;
                    WriteOfType(boolAttr.WriteAs, typeValue);
                } else if (property.PropertyType == typeof(int) && Attribute.IsDefined(property, typeof(BinaryInt24Attribute))) {
                    // write the number as int24
                    WriteNumber((uint)value, 24);
                } else if (property.PropertyType.IsEnum && property.GetCustomAttribute<BinaryEnumAttribute>() is { } enumAttr) {
                    // enums can only be written if they have the attribute.
                    WriteOfType(enumAttr.WriteAs, value);
                } else if (property.PropertyType == typeof(string) && property.GetCustomAttribute<BinaryStringAttribute>() is { } stringAttr) {
                    Encoding? encoding = null;
                    if (stringAttr.CodePage != -1) {
                        encoding = Encoding.GetEncoding(stringAttr.CodePage);
                    }

                    if (stringAttr.SizeType is null) {
                        if (stringAttr.FixedSize == -1) {
                            Write((string)value, stringAttr.Terminator, encoding, stringAttr.MaxSize);
                        } else {
                            Write((string)value, stringAttr.FixedSize, stringAttr.Terminator, encoding);
                        }
                    } else {
                        Write((string)value, stringAttr.SizeType, stringAttr.Terminator, encoding, stringAttr.MaxSize);
                    }
                } else {
                    WriteOfType(property.PropertyType, value);
                }

                // Restore previous endianness
                Endianness = currentEndianness;
            }
        }
}
