using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using static fin.io.sharpfilelister.Interop2;

namespace fin.io.sharpDirLister;

public class RecursiveSharpFileLister : IFileLister {
  public const IntPtr INVALID_HANDLE_VALUE = -1;

  //Code based heavily on https://stackoverflow.com/q/47471744
  public DirectoryInformation FindNextFilePInvoke(string path) {
    var directoryInfo = new DirectoryInformation { AbsoluteSubdirPath = path };
    var fileList = directoryInfo.AbsoluteFilePathsImpl;
    var directoryList = directoryInfo.SubdirsImpl;

    IntPtr fileSearchHandle = INVALID_HANDLE_VALUE;
    try {
      fileSearchHandle = FindFirstFileWInDirectory_(path, out var findData);
      if (fileSearchHandle != INVALID_HANDLE_VALUE) {
        do {
          var fileName = (ReadOnlySpan<char>) findData.cFileName;
          if (fileName is "." or "..") {
            continue;
          }

          var fullPath = @$"{path}\{fileName}";
          if (!findData.dwFileAttributes.CheckFlag(FileAttributes.Directory)) {
            fileList.AddLast(fullPath);
          } else if (!findData.dwFileAttributes.CheckFlag(
                         FileAttributes.ReparsePoint)) {
            directoryList.AddLast(this.FindNextFilePInvoke(fullPath));
          }
        } while (FindNextFile(fileSearchHandle, out findData));
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