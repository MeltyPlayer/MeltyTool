using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using fin.io.sharpfilelister;

using static fin.io.sharpfilelister.Interop;

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
          (uint) fullPath.Length,
          ref totalSize,
          ref findData,
          &findData);
    }

    return totalSize;
  }

  //Code based heavily on https://stackoverflow.com/q/47471744
  private static unsafe void FindNextFilePInvoke_(
      char* pathPtr,
      uint parentLength,
      ref long totalSize,
      ref WIN32_FIND_DATAW findData,
      WIN32_FIND_DATAW* findDataPtr) {
    IntPtr fileSearchHandle = INVALID_HANDLE_VALUE;
    try {
      pathPtr[parentLength] = '\\';
      pathPtr[parentLength + 1] = '*';

      ClearEndOfPath_(pathPtr, parentLength + 2);

      var subFilePtr = pathPtr + parentLength + 1;
      var remainingLength = PATH_SPAN_LENGTH - parentLength;

      fileSearchHandle = FindFirstFileW((IntPtr) pathPtr, findDataPtr);
      if (fileSearchHandle != INVALID_HANDLE_VALUE) {
        do {
          var fileName = findData.FileName;
          if (fileName[0] == '.') {
            continue;
          }

          fixed (char* fileNamePtr = &fileName[0]) {
            var attributes = findData.dwFileAttributes;
            if ((attributes & FileAttributes.Directory) == 0) {
              totalSize += findData.FileSize;
            } else if ((attributes & FileAttributes.ReparsePoint) == 0) {
              Copy_(subFilePtr, fileNamePtr, remainingLength);
              var subDirLength = Length_(fileNamePtr);
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
  private static unsafe void Copy_(char* dstPtr, char* srcPtr, uint length)
    => Unsafe.CopyBlock(dstPtr, srcPtr, 2 * length);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static unsafe void ClearEndOfPath_(char* pathPtr, uint length)
    => Unsafe.InitBlockUnaligned(pathPtr + length,
                                 0,
                                 2 * (PATH_SPAN_LENGTH - length));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static unsafe uint Length_(char* pathPtr) {
    for (uint i = 0; i < PATH_SPAN_LENGTH; i++) {
      if (pathPtr[i] == '\0') {
        return i;
      }
    }

    return PATH_SPAN_LENGTH;
  }
}