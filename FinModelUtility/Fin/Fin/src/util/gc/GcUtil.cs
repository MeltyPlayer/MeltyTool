using System;

namespace fin.util.gc;

public static class GcUtil {
  public static void ForceCollectEverything() {
    GC.Collect();
    GC.WaitForFullGCComplete();
    GC.WaitForPendingFinalizers();
  }
}