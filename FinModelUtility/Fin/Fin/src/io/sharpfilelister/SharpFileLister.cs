using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using fin.data.queues;

using static fin.io.sharpfilelister.Interop;

namespace fin.io.sharpDirLister;

public class DirectoryInformation : ISubdirPaths {
  public string AbsoluteSubdirPath { get; set; }

  public IReadOnlyCollection<string> AbsoluteFilePaths
    => AbsoluteFilePathsImpl;

  public IReadOnlyCollection<ISubdirPaths> Subdirs => SubdirsImpl;

  public LinkedList<string> AbsoluteFilePathsImpl { get; } = [];
  public LinkedList<DirectoryInformation> SubdirsImpl { get; } = [];
}

public interface IFileLister {
  DirectoryInformation FindNextFilePInvoke(string path);
}

public class SharpFileLister : IFileLister {
  public const IntPtr INVALID_HANDLE_VALUE = -1;

  //Code based heavily on https://stackoverflow.com/q/47471744
  public DirectoryInformation FindNextFilePInvoke(string rootPath) {
    var rootDirectoryInfo = new DirectoryInformation
        { AbsoluteSubdirPath = rootPath };

    var queue = new FinQueue<DirectoryInformation>(rootDirectoryInfo);
    while (queue.TryDequeue(out var directoryInfo)) {
      var path = directoryInfo.AbsoluteSubdirPath;
      var fileList = directoryInfo.AbsoluteFilePathsImpl;
      var directoryList = directoryInfo.SubdirsImpl;

      IntPtr fileSearchHandle = INVALID_HANDLE_VALUE;
      try {
        fileSearchHandle = this.FindFirstFileWInDirectory_(path, out var findData);
        if (fileSearchHandle != INVALID_HANDLE_VALUE) {
          do {
            if (findData.cFileName is "." or "..") {
              continue;
            }

            var fullPath = @$"{path}\{findData.cFileName}";
            if (!findData.dwFileAttributes.HasFlag(
                    FileAttributes.Directory)) {
              fileList.AddLast(fullPath);
            } else if (!findData.dwFileAttributes.HasFlag(
                           FileAttributes.ReparsePoint)) {
              var childDirInfo = new DirectoryInformation
                  { AbsoluteSubdirPath = fullPath };
              directoryList.AddLast(childDirInfo);
              queue.Enqueue((childDirInfo));
            }
          } while (FindNextFile(fileSearchHandle, out findData));
        }
      } finally {
        if (fileSearchHandle != INVALID_HANDLE_VALUE) {
          FindClose(fileSearchHandle);
        }
      }
    }

    return rootDirectoryInfo;
  }

  private unsafe nint FindFirstFileWInDirectory_(
      string directoryPath,
      out WIN32_FIND_DATAW findData) {
    Span<char> pathChars = stackalloc char[directoryPath.Length + 3];
    directoryPath.AsSpan().CopyTo(pathChars);
    pathChars[^3] = '\\';
    pathChars[^2] = '*';

    fixed (char* ptr = &MemoryMarshal.GetReference(pathChars)) {
      return FindFirstFileW((IntPtr) ptr, out findData);
    }
  }
}