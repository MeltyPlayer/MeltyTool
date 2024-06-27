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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

/// <summary>
/// Text reader for <see cref="Stream" />.
/// </summary>
public class TextDataReader
{
    readonly DataReader reader;
    string newLine;

    static TextDataReader()
    {
            // Make sure that the shift-jis encoding is initialized.
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

    /// <summary>
    /// Initializes a new instance of the <see cref="TextDataReader"/> class.
    /// </summary>
    /// <param name="stream">Stream to read from.</param>
    /// <remarks><para>The default encoding is UTF-8.</para></remarks>
    public TextDataReader(Stream stream)
        : this(stream, Encoding.UTF8)
    {
        }

    /// <summary>
    /// Initializes a new instance of the <see cref="TextDataReader"/> class.
    /// </summary>
    /// <param name="stream">Stream to read from.</param>
    /// <param name="encoding">Encoding to use.</param>
    public TextDataReader(Stream stream, string encoding)
        : this(stream, Encoding.GetEncoding(encoding))
    {
        }

    /// <summary>
    /// Initializes a new instance of the <see cref="TextDataReader"/> class.
    /// </summary>
    /// <param name="stream">Stream to read from.</param>
    /// <param name="encoding">Encoding to use.</param>
    public TextDataReader(Stream stream, Encoding encoding)
    {
            if (stream is null)
                throw new ArgumentNullException(nameof(stream));

            Stream = stream as DataStream ?? new DataStream(stream, 0, stream.Length, false);
            Encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
            newLine = Environment.NewLine;
            AutoNewLine = true;

            reader = new DataReader(stream) {
                DefaultEncoding = Encoding,
            };
        }

    /// <summary>
    /// Gets the stream.
    /// </summary>
    public DataStream Stream {
        get;
        private set;
    }

    /// <summary>
    /// Gets the encoding.
    /// </summary>
    public Encoding Encoding {
        get;
        private set;
    }

    /// <summary>
    /// Gets or sets the new line and set to false AutoNewLine.
    /// </summary>
    /// <value>The new line.</value>
    /// <remarks><para>The default value is OS-dependant.</para></remarks>
    public string NewLine {
        get {
                return newLine;
            }

        set {
                newLine = value;
                AutoNewLine = false;
            }
    }

    /// <summary>
    /// Gets or sets a value indicating whether reads any kind of
    /// NewLine format (\r\n or \n). If true, this ignore the
    /// NewLine field.
    /// </summary>
    /// <returns>If true, it will treat new lines any matching of \r\n or
    /// \n. Otherwise false.
    /// </returns>
    public bool AutoNewLine {
        get;
        set;
    }

    /// <summary>
    /// Read a char from the stream.
    /// </summary>
    /// <returns>The read char.</returns>
    public char Read()
    {
            SkipPreamble();
            return reader.ReadChar();
        }

    /// <summary>
    /// Read the specified number of chars.
    /// </summary>
    /// <returns>The read chars.</returns>
    /// <param name="count">Chars to read.</param>
    public char[] Read(int count)
    {
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            SkipPreamble();
            return reader.ReadChars(count);
        }

    /// <summary>
    /// Reads a string until a string / token is found.
    /// </summary>
    /// <param name="token">Token to find.</param>
    /// <returns>The read string or null.</returns>
    /// <exception cref="EndOfStreamException">
    /// If the stream position is at the end.
    /// </exception>
    public string ReadToToken(string token)
    {
            if (string.IsNullOrEmpty(token))
                throw new ArgumentNullException(nameof(token));

            // If starting is EOF, then throw exception.
            if (Stream.Position >= Stream.Length) {
                throw new EndOfStreamException();
            }

            SkipPreamble();

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
                    break;
                }

                // Read buffer size if possible, otherwise remaining bytes
                long currentPosition = Stream.Position;
                int size = currentPosition + BufferSize <= streamLength ?
                    BufferSize :
                    (int)(streamLength - currentPosition);

                int read = Stream.Read(buffer, 0, size);
                textBuffer.AddRange(buffer.Take(read));

                text = Encoding.GetString(textBuffer.ToArray());
                matchIndex = text.IndexOf(token, StringComparison.Ordinal);
            }

            if (matchIndex != -1) {
                // We skip the bytes of the token too
                string fullResult = text.Substring(0, matchIndex + token.Length);
                Stream.Seek(startPos + Encoding.GetByteCount(fullResult), SeekOrigin.Begin);

                // Result without token
                text = text.Substring(0, matchIndex);
            }

            return text;
        }

    /// <summary>
    /// Reads a line.
    /// </summary>
    /// <returns>The line.</returns>
    public string ReadLine()
    {
            string line;

            // We search for \n new lines.
            if (AutoNewLine) {
                line = ReadToToken("\n");

                // In the case of Windows, the last char will be \r. We remove it.
                if (!string.IsNullOrEmpty(line) && line[line.Length - 1] == '\r')
                    line = line.Remove(line.Length - 1, 1);
            } else {
                line = ReadToToken(NewLine);
            }

            return line;
        }

    /// <summary>
    /// Reads to the end.
    /// </summary>
    /// <returns>The string.</returns>
    public string ReadToEnd()
    {
            SkipPreamble();
            return reader.ReadString((int)(Stream.Length - Stream.Position));
        }

    /// <summary>
    /// Read the next char without changing the position.
    /// </summary>
    /// <returns>The next char.</returns>
    public char Peek()
    {
            long startPos = Stream.Position;
            char ch = Read();
            Stream.Seek(startPos, SeekOrigin.Begin);
            return ch;
        }

    /// <summary>
    /// Read the next count of chars without changing the position.
    /// </summary>
    /// <returns>The next chars.</returns>
    /// <param name="count">Number of chars to read.</param>
    public char[] Peek(int count)
    {
            long startPos = Stream.Position;
            char[] chars = Read(count);
            Stream.Seek(startPos, SeekOrigin.Begin);
            return chars;
        }

    /// <summary>
    /// Read until a string / token is found without changing the position.
    /// </summary>
    /// <returns>The next chars.</returns>
    /// <param name="token">Token to find.</param>
    public string PeekToToken(string token)
    {
            long startPos = Stream.Position;
            string content = ReadToToken(token);
            Stream.Seek(startPos, SeekOrigin.Begin);
            return content;
        }

    /// <summary>
    /// Read the next line without changing the position.
    /// </summary>
    /// <returns>The next line.</returns>
    public string PeekLine()
    {
            long startPos = Stream.Position;
            string line = ReadLine();
            Stream.Seek(startPos, SeekOrigin.Begin);
            return line;
        }

    void SkipPreamble()
    {
            // Preambles can only be at the beginning of the stream.
            if (Stream.Position > 0) {
                return;
            }

            byte[] preamble = Encoding.GetPreamble();
            if (Stream.Length < preamble.Length) {
                return;
            }

            bool match = true;
            for (int i = 0; i < preamble.Length && match; i++) {
                match = preamble[i] == reader.ReadByte();
            }

            // If it didn't fully match it wasn't a preamble, returns to 0
            if (!match) {
                Stream.Seek(0, SeekOrigin.Begin);
            }
        }
}
