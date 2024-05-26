using System;

namespace fin.util.time {
  public static class FrameTime {
    private static readonly DateTime firstFrameStart_ = DateTime.Now;
    private static DateTime previousFrameStart_;

    public static void MarkStartOfFrame() {
      FrameTime.previousFrameStart_ = FrameTime.StartOfFrame;
      FrameTime.StartOfFrame = DateTime.Now;

      FrameTime.ElapsedTime = FrameTime.StartOfFrame - firstFrameStart_;

      var elapsedSeconds =
          (float) (FrameTime.StartOfFrame - FrameTime.previousFrameStart_)
          .TotalSeconds;
      FrameTime.DeltaTime = elapsedSeconds;
    }

    public static DateTime StartOfFrame { get; private set; }
    public static TimeSpan ElapsedTime { get; private set; }
    public static float DeltaTime { get; private set; }
  }
}