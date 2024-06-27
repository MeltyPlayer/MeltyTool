using System.IO;
using System.Runtime.CompilerServices;

namespace fin.io;

public static class FinIoStatic {
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string GetName(string fullName) => Path.GetFileName(fullName);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string? GetParentFullName(string fullName)
    => Path.GetDirectoryName(fullName);
}