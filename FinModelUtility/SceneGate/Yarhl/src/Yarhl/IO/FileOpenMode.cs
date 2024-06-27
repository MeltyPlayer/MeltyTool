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
    using System.Diagnostics.CodeAnalysis;
    using System.IO;

    /// <summary>
    /// Mode to open files.
    /// </summary>
    public enum FileOpenMode
    {
        /// <summary>
        /// Open the file for reading.
        /// If the file doesn't exist it will throw a FileNotFound exception.
        /// Requires reading permissions.
        /// </summary>
        Read,

        /// <summary>
        /// Open a file for writing.
        /// If the file doesn't exist it will be created.
        /// If the file exists it will be truncated and start writing from the beginning.
        /// Requires writing permissions.
        /// </summary>
        Write,

        /// <summary>
        /// Open a file for reading and/or writing.
        /// If the file doesn't exist it wll be created.
        /// If the file exists it will start writing from the beginning but not truncated.
        /// Requires reading and writing permissions.
        /// </summary>
        ReadWrite,

        /// <summary>
        /// Open a file for appending data at the end.
        /// If the file doesn't exist it will throw an exception.
        /// Requires writing permissions.
        /// </summary>
        Append,
    }

    /// <summary>
    /// Extensions for the FileOpenMode enumeration.
    /// </summary>
    [SuppressMessage(
        "Microsoft.StyleCop.CSharp.MaintainabilityRules",
        "SA1649:FileNameMustMatchTypeName",
        Justification = "Extension class for the enum.")]
    static class FileOpenModeExtensions
    {
        /// <summary>
        /// Get the equivalent <see cref="FileMode"/>.
        /// </summary>
        /// <returns>The file mode.</returns>
        /// <param name="openMode">File open mode.</param>
        public static FileMode ToFileMode(this FileOpenMode openMode)
        {
            switch (openMode) {
                case FileOpenMode.Read:
                    return FileMode.Open;
                case FileOpenMode.Write:
                    return FileMode.Create;
                case FileOpenMode.Append:
                    return FileMode.Append;
                case FileOpenMode.ReadWrite:
                    return FileMode.OpenOrCreate;
                default:
                    throw new ArgumentOutOfRangeException(nameof(openMode));
            }
        }

        /// <summary>
        /// Get the equivalent <see cref="FileAccess"/>.
        /// </summary>
        /// <returns>The file access.</returns>
        /// <param name="openMode">File open mode.</param>
        public static FileAccess ToFileAccess(this FileOpenMode openMode)
        {
            switch (openMode) {
                case FileOpenMode.Read:
                    return FileAccess.Read;
                case FileOpenMode.Write:
                    return FileAccess.Write;
                case FileOpenMode.Append:
                    return FileAccess.Write;
                case FileOpenMode.ReadWrite:
                    return FileAccess.ReadWrite;
                default:
                    throw new ArgumentOutOfRangeException(nameof(openMode));
            }
        }
    }
}
