using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using static fin.io.sharpfilelister.Interop2;

namespace fin.io.sharpDirLister;

public sealed class SchemaSharpDirectorySizeMeasurer {
  public const IntPtr INVALID_HANDLE_VALUE = -1;

  private const int PATH_SPAN_LENGTH = 260;

  public unsafe long MeasureSizeOfDirectory(string fullPath) {
    Span<char> pathSpan = stackalloc char[PATH_SPAN_LENGTH];
    fullPath.CopyTo(pathSpan);

    WIN32_FIND_DATAW findData = default;

    var totalSize = 0L;
    fixed (char* pathPtr = &MemoryMarshal.GetReference(pathSpan)) {
      FindNextFilePInvoke_(
          pathPtr,
          fullPath.Length,
          ref totalSize,
          ref findData,
          &findData);
    }

    return totalSize;
  }

  //Code based heavily on https://stackoverflow.com/q/47471744
  private static unsafe void FindNextFilePInvoke_(
      char* pathPtr,
      int parentLength,
      ref long totalSize,
      ref WIN32_FIND_DATAW findData,
      WIN32_FIND_DATAW* findDataPtr) {
    IntPtr fileSearchHandle = INVALID_HANDLE_VALUE;
    try {
      pathPtr[parentLength] = '\\';
      pathPtr[parentLength + 1] = '*';
      pathPtr[parentLength + 2] = '\0';

      var subFilePtr = pathPtr + parentLength + 1;
      var remainingLength = PATH_SPAN_LENGTH - parentLength;

      fileSearchHandle = FindFirstFileExW(
          pathPtr,
          // Bypasses MS-DOS short name query
          FIND_EX_INFO_BASIC,
          findDataPtr,
          FIND_EX_SEARCH_NAME_MATCH,
          0,
          // Uses larger kernel buffer size and case-sensitive search
          FIND_FIRST_EX_FLAGS);
      if (fileSearchHandle != INVALID_HANDLE_VALUE) {
        do {
          var fileName = findData.FileName;
          if (fileName is "." or "..") {
            continue;
          }

          fixed (char* fileNamePtr = &fileName[0]) {
            var attributes = findData.dwFileAttributes;
            if ((attributes & FileAttributes.Directory) == 0) {
              totalSize += findData.FileSize;
            } else if ((attributes & FileAttributes.ReparsePoint) == 0) {
              Copy_(subFilePtr, fileNamePtr, remainingLength);
              var subDirLength = fileName.Length;
              FindNextFilePInvoke_(pathPtr,
                                   parentLength + 1 + subDirLength,
                                   ref totalSize,
                                   ref findData,
                                   findDataPtr);
            }
          }
        } while (FindNextFile(fileSearchHandle, findDataPtr));
      }
    } finally {
      if (fileSearchHandle != INVALID_HANDLE_VALUE) {
        FindClose(fileSearchHandle);
      }
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static unsafe void Copy_(char* dstPtr, char* srcPtr, int length)
    => Unsafe.CopyBlock(dstPtr, srcPtr, (uint) (2 * length));
}