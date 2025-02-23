using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

using fin.io.sharpDirLister;

namespace fin.io;

public static class FinDirectoryStatic {
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool Exists(string fullName)
    => FinFileSystem.Directory.Exists(fullName);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsEmpty(string fullName)
    => !FinFileSystem.Directory.EnumerateFileSystemEntries(fullName).Any();

  public static bool Create(string fullName)
    => FinFileSystem.Directory.CreateDirectory(fullName).Exists;

  public static bool Delete(string fullName, bool recursive = false) {
    if (!Exists(fullName)) {
      return false;
    }

    FinFileSystem.Directory.Delete(fullName, recursive);
    return true;
  }

  public static void MoveTo(string fullName, string path) {
    try {
      FinFileSystem.Directory.Move(fullName, path);
    }
    // Sometimes the first move throws a permission denied error, so we just need to try again.
    catch {
      FinFileSystem.Directory.Move(fullName, path);
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IEnumerable<string> GetExistingSubdirs(string fullName)
    => FinFileSystem.Directory.EnumerateDirectories(fullName);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ReadOnlySpan<char> GetSubdir(
      ReadOnlySpan<char> fullName,
      ReadOnlySpan<char> relativePath,
      bool create = false) {
    var subdirs = relativePath.Split(['/', '\\']);
    var current = fullName;

    foreach (var subdirRange in subdirs) {
      var subdir = relativePath[subdirRange];

      if (subdir.IsEmpty) {
        continue;
      }

      if (subdir is "..") {
        current = Path.GetDirectoryName(current);
        continue;
      }

      current = Path.Join(current, subdir);
    }

    if (create) {
      FinFileSystem.Directory.CreateDirectory(current.ToString());
    }

    return current;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IEnumerable<string> GetExistingFiles(string fullName)
    => FinFileSystem.Directory.EnumerateFiles(fullName);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IEnumerable<string> SearchForFiles(
      string fullName,
      string searchPattern,
      bool includeSubdirs = false)
    => FinFileSystem
       .Directory.GetFiles(
           fullName,
           searchPattern,
           includeSubdirs
               ? SearchOption.AllDirectories
               : SearchOption.TopDirectoryOnly);

  public static bool TryToGetExistingFile(
      ReadOnlySpan<char> fullName,
      ReadOnlySpan<char> path,
      out string file) {
    var fileInfo = Path.Join(fullName, path);
    if (FinFileSystem.File.Exists(fileInfo)) {
      file = fileInfo;
      return true;
    }

    file = default;
    return false;
  }

  public static string GetExistingFile(ReadOnlySpan<char> fullName,
                                       ReadOnlySpan<char> path) {
    if (TryToGetExistingFile(fullName, path, out var file)) {
      return file;
    }

    throw new Exception(
        $"Expected to find file: '{path}' in directory '{fullName}'");
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IEnumerable<string> GetFilesWithExtension(
      string fullName,
      string extension,
      bool includeSubdirs = false)
    => FinFileSystem.Directory.GetFiles(
        fullName,
        $"*{Files.AssertValidExtension(extension)}",
        includeSubdirs
            ? SearchOption.AllDirectories
            : SearchOption.TopDirectoryOnly);

  public static long GetTotalSize(string fullName)
    => new SchemaSharpDirectorySizeMeasurer().MeasureSizeOfDirectory(
        fullName);
}