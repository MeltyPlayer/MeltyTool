using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using schema.binary;
using schema.binary.attributes;

using static fin.io.sharpfilelister.Interop;

namespace fin.io.sharpDirLister;

[BinarySchema]
public partial class SchemaDirectoryInformation : IBinaryConvertible {
  [StringLengthSource(SchemaIntegerType.INT16)]
  public string Name { get; set; }

  [SequenceLengthSource(SchemaIntegerType.INT16)]
  public Uint16SizedString[] FileNames { get; set; } = [];

  [SequenceLengthSource(SchemaIntegerType.INT16)]
  public SchemaDirectoryInformation[] Subdirs { get; set; } = [];
}

[BinarySchema]
public partial class Uint16SizedString : IBinaryConvertible {
  [StringLengthSource(SchemaIntegerType.UINT16)]
  public string Name { get; set; }
}

public class SchemaSharpFileLister {
  public const IntPtr INVALID_HANDLE_VALUE = -1;

  //Code based heavily on https://stackoverflow.com/q/47471744
  public unsafe SchemaDirectoryInformation FindNextFilePInvoke(
      string path,
      string name) {
    var directoryInfo = new SchemaDirectoryInformation { Name = name };
    var fileList = new LinkedList<Uint16SizedString>();
    var directoryList = new LinkedList<SchemaDirectoryInformation>();

    IntPtr fileSearchHandle = INVALID_HANDLE_VALUE;
    try {
      fileSearchHandle = FindFirstFileWInDirectory_(path, out var findData);
      if (fileSearchHandle != INVALID_HANDLE_VALUE) {
        do {
          var fileNameSpan = new ReadOnlySpan<char>(findData.cFileName, 260);
          var fileName = fileNameSpan[..fileNameSpan.IndexOf('\0')].ToString();

          if (fileName is "." or "..") {
            continue;
          }

          var attributes = findData.dwFileAttributes;
          var fullPath = @$"{path}\{fileName}";
          if ((attributes & FileAttributes.Directory) == 0) {
            fileList.AddLast(new Uint16SizedString { Name = fileName });
          } else if ((attributes & FileAttributes.ReparsePoint) == 0) {
            directoryList.AddLast(this.FindNextFilePInvoke(fullPath, fileName));
          }
        } while (FindNextFile(fileSearchHandle, out findData));
      }
    } finally {
      if (fileSearchHandle != INVALID_HANDLE_VALUE) {
        FindClose(fileSearchHandle);
      }
    }

    directoryInfo.FileNames = fileList.ToArray();
    directoryInfo.Subdirs = directoryList.ToArray();

    return directoryInfo;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static unsafe nint FindFirstFileWInDirectory_(
      ReadOnlySpan<char> directoryPath,
      out WIN32_FIND_DATAW findData) {
    Span<char> pathChars = stackalloc char[directoryPath.Length + 3];
    directoryPath.CopyTo(pathChars);
    pathChars[^3] = '\\';
    pathChars[^2] = '*';

    fixed (char* ptr = &MemoryMarshal.GetReference(pathChars)) {
      return FindFirstFileW((IntPtr) ptr, out findData);
    }
  }
}