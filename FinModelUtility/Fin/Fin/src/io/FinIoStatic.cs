using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace fin.io;

public static class FinIoStatic {
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ReadOnlySpan<char> GetName(ReadOnlySpan<char> fullName)
    => Path.GetFileName(Path.TrimEndingDirectorySeparator(fullName));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ReadOnlySpan<char> GetParentFullName(
      ReadOnlySpan<char> fullName)
    => Path.GetDirectoryName(Path.TrimEndingDirectorySeparator(fullName));
}