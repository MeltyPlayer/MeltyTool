using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace fin.io.sharpfilelister;

public class Interop {
  [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
  public unsafe struct WIN32_FIND_DATAW {
    public FileAttributes dwFileAttributes;
    internal FILETIME ftCreationTime;
    internal FILETIME ftLastAccessTime;
    internal FILETIME ftLastWriteTime;
    public int nFileSizeHigh;
    public int nFileSizeLow;
    public int dwReserved0;
    public int dwReserved1;

    public fixed char cFileName[260];
    public fixed char cFileNameAlternativeName[14];
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