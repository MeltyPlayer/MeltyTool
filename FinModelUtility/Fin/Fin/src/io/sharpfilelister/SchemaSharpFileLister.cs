using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

using schema.binary;
using schema.binary.attributes;

using static fin.io.sharpfilelister.Interop;

namespace fin.io.sharpDirLister;

[BinarySchema]
public sealed partial class SchemaDirectoryInformation : IBinaryConvertible {
  [StringEncoding(StringEncodingType.UTF8)]
  [StringLengthSource(SchemaIntegerType.INT16)]
  public string Name { get; set; }

  [SequenceLengthSource(SchemaIntegerType.INT16)]
  public Uint16SizedString[] FileNames { get; set; } = [];

  [SequenceLengthSource(SchemaIntegerType.INT16)]
  public SchemaDirectoryInformation[] Subdirs { get; set; } = [];
}

[BinarySchema]
public sealed partial class Uint16SizedString : IBinaryConvertible {
  [StringEncoding(StringEncodingType.UTF8)]
  [StringLengthSource(SchemaIntegerType.UINT16)]
  public string Name { get; set; }
}

public sealed class SchemaSharpFileLister {
  public const IntPtr INVALID_HANDLE_VALUE = -1;

  //Code based heavily on https://stackoverflow.com/q/47471744
  public static unsafe SchemaDirectoryInformation FindNextFilePInvoke(
      string path,
      string name) {
    var directoryInfo = new SchemaDirectoryInformation { Name = name };
    var fileList = new List<Uint16SizedString>();
    var directoryList = new List<SchemaDirectoryInformation>();

    WIN32_FIND_DATAW findData;
    WIN32_FIND_DATAW* findDataPtr = &findData;

    IntPtr fileSearchHandle = INVALID_HANDLE_VALUE;
    try {
      fileSearchHandle = FindFirstFileWInDirectory_(path, findDataPtr);
      if (fileSearchHandle != INVALID_HANDLE_VALUE) {
        do {
          var fileNameSpan = findData.FileName;
          if (fileNameSpan is "." or "..") {
            continue;
          }

          var fileName = fileNameSpan.ToString();
          var attributes = findData.dwFileAttributes;
          if ((attributes & FileAttributes.Directory) == 0) {
            fileList.Add(new Uint16SizedString { Name = fileName });
          } else if ((attributes & FileAttributes.ReparsePoint) == 0) {
            var fullPath = @$"{path}\{fileName}";
            directoryList.Add(FindNextFilePInvoke(fullPath, fileName));
          }
        } while (FindNextFile(fileSearchHandle, findDataPtr));
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
      WIN32_FIND_DATAW* findDataPtr) {
    Span<char> pathChars = stackalloc char[directoryPath.Length + 3];
    directoryPath.CopyTo(pathChars);
    pathChars[^3] = '\\';
    pathChars[^2] = '*';

    fixed (char* ptr = pathChars) {
      return FindFirstFileW((nint) ptr, findDataPtr);
    }
  }
}