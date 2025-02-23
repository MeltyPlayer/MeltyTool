using System;
using System.Runtime.InteropServices;
using System.Security;

namespace fin.io.sharpfilelister;

[SuppressUnmanagedCodeSecurity]
public class Interop2 {
  [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
  public static extern IntPtr FindFirstFileW(
      IntPtr lpFileName,
      out Interop.WIN32_FIND_DATAW lpFindFileData);

  [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
  public static extern bool FindNextFile(
      IntPtr hFindFile,
      out Interop.WIN32_FIND_DATAW lpFindFileData);

  [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
  public static extern bool FindClose(IntPtr hFindFile);
}