using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security;

using fin.util.strings;

namespace fin.io.sharpfilelister;

[SuppressUnmanagedCodeSecurity]
public sealed class Interop {
  [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
  public unsafe struct WIN32_FIND_DATAW {
    public FileAttributes dwFileAttributes;
    internal FILETIME ftCreationTime;
    internal FILETIME ftLastAccessTime;
    internal FILETIME ftLastWriteTime;
    public uint nFileSizeHigh;
    public uint nFileSizeLow;
    public uint dwReserved0;
    public uint dwReserved1;

    private const int FILE_NAME_LENGTH_ = 260;

    private fixed byte cFileName[2 * FILE_NAME_LENGTH_];
    private fixed byte cFileNameAlternativeName[2 * 14];

    public ReadOnlySpan<char> FileName {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get {
        fixed (byte* fileNamePtr = &this.cFileName[0]) {
          return new ReadOnlySpan<char>(fileNamePtr, FILE_NAME_LENGTH_)
              .SubstringUpTo('\0');
        }
      }
    }

    public long FileSize {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => ((long) this.nFileSizeHigh << 32) | (uint) this.nFileSizeLow;
    }
  }

  [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
  public static extern unsafe nint FindFirstFileW(
      nint lpFileName,
      WIN32_FIND_DATAW* lpFindFileData);

  [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
  public static extern unsafe bool FindNextFile(
      nint hFindFile,
      WIN32_FIND_DATAW* lpFindFileData);

  [DllImport("kernel32.dll")]
  public static extern bool FindClose(nint hFindFile);
}