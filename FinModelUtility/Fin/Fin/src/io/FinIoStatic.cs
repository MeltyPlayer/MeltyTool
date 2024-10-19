using System.IO;
using System.Runtime.CompilerServices;

namespace fin.io;

public static class FinIoStatic {
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string GetName(string fullName)
    => Path.GetFileName(Path.TrimEndingDirectorySeparator(fullName));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string? GetParentFullName(string fullName) {
    var parentFullName
        = Path.GetDirectoryName(Path.TrimEndingDirectorySeparator(fullName));
    return parentFullName == "" ? null : parentFullName;
  }
}