using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

using static fin.io.sharpfilelister.Interop;

namespace fin.io.sharpDirLister;

public sealed class DirectoryInformation {
  public string AbsoluteSubdirPath { get; set; }
  public List<string> AbsoluteFilePaths { get; } = [];
  public List<DirectoryInformation> Subdirs { get; } = [];
}

public static class SharpFileLister {
  public const IntPtr INVALID_HANDLE_VALUE = -1;

  //Code based heavily on https://stackoverflow.com/q/47471744
  public static unsafe DirectoryInformation FindNextFilePInvoke(string path) {
    var directoryInfo = new DirectoryInformation { AbsoluteSubdirPath = path };
    var fileList = directoryInfo.AbsoluteFilePaths;
    var directoryList = directoryInfo.Subdirs;

    WIN32_FIND_DATAW findData;
    WIN32_FIND_DATAW* findDataPtr = &findData;

    IntPtr fileSearchHandle = INVALID_HANDLE_VALUE;
    try {
      fileSearchHandle = FindFirstFileWInDirectory_(path, findDataPtr);
      if (fileSearchHandle != INVALID_HANDLE_VALUE) {
        do {
          var fileName = findData.FileName;
          if (fileName is "." or "..") {
            continue;
          }

          var attributes = findData.dwFileAttributes;
          var fullPath = @$"{path}\{fileName}";
          if ((attributes & FileAttributes.Directory) == 0) {
            fileList.Add(fullPath);
          } else if ((attributes & FileAttributes.ReparsePoint) == 0) {
            directoryList.Add(FindNextFilePInvoke(fullPath));
          }
        } while (FindNextFile(fileSearchHandle, findDataPtr));
      }
    } finally {
      if (fileSearchHandle != INVALID_HANDLE_VALUE) {
        FindClose(fileSearchHandle);
      }
    }

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
    pathChars[^1] = '\0';

    fixed (char* ptr = &pathChars[0]) {
      return FindFirstFileW((nint) ptr, findDataPtr);
    }
  }
}