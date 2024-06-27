using System.Runtime.InteropServices;

namespace uni.cli {
  public static class ConsoleUtil {
    public static void ShowConsole() => AllocConsole();

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool AllocConsole();
  }
}