using System.IO;
using System.IO.Abstractions;
using System.Runtime.CompilerServices;

using fin.util.strings;

namespace fin.io;

public static class FinFileStatic {
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool Exists(string fullName)
    => FinFileSystem.File.Exists(fullName);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool Delete(string fullName) {
    if (!FinFileStatic.Exists(fullName)) {
      return false;
    }

    FinFileSystem.File.Delete(fullName);
    return true;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string GetExtension(string fullName) {
    int length = fullName.Length;
    for (int i = length; --i >= 0;) {
      char ch = fullName[i];
      if (ch == '.')
        return fullName.Substring(i, length - i).ToLower();
      if (ch == '\\' || ch == '/' || ch == Path.VolumeSeparatorChar)
        break;
    }

    return string.Empty;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string GetNameWithoutExtension(string name)
    => name.SubstringUpTo(".");

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static FileSystemStream OpenRead(string fullName)
    => FileUtil.OpenRead(fullName);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static FileSystemStream OpenWrite(string fullName)
    => FileUtil.OpenWrite(fullName);
}