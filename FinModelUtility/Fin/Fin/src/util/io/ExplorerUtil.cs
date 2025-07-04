using System.Diagnostics;

using fin.io;

namespace fin.util.io;

public static class ExplorerUtil {
  public static void OpenInExplorer(IReadOnlyTreeIoObject ioObject)
    => Process.Start("explorer.exe", $"/select,\"{ioObject.FullPath}\"");
}