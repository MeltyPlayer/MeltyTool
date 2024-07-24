using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace fin.io.sharpfilelister;

public class Interop2 {
  [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
  public struct WIN32_FIND_DATAW {
    public FileAttributes dwFileAttributes;
    internal FILETIME ftCreationTime;
    internal FILETIME ftLastAccessTime;
    internal FILETIME ftLastWriteTime;
    public int nFileSizeHigh;
    public int nFileSizeLow;
    public int dwReserved0;
    public int dwReserved1;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
    public string cFileName;
  }

  [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
  public static extern IntPtr FindFirstFileW(
      IntPtr lpFileName,
      out WIN32_FIND_DATAW lpFindFileData);

  [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
  public static extern bool FindNextFile(IntPtr hFindFile,
                                         out WIN32_FIND_DATAW lpFindFileData);

  [DllImport("kernel32.dll")]
  public static extern bool FindClose(IntPtr hFindFile);
}