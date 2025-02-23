using System;
using System.Runtime.InteropServices;
using System.Security;

namespace fin.io.sharpfilelister;

[SuppressUnmanagedCodeSecurity]
public class Interop2 {
  [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
  public static extern IntPtr FindFirstFileW(IntPtr lpFileName,
                                             IntPtr lpFindFileData);

  [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
  public static extern bool FindNextFile(IntPtr hFindFile,
                                         IntPtr lpFindFileData);

  [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
  public static extern bool FindClose(IntPtr hFindFile);
}