using System;
using System.Linq;

namespace fin.util.time {
  public static class FrameTime {
    private static readonly DateTime firstFrameStart_ = DateTime.Now;
    private static DateTime previousFrameStart_;

    private static readonly float[] frameTimesForSmoothedFps_ = new float[60];

    public static void MarkStartOfFrame() {
      FrameTime.previousFrameStart_ = FrameTime.StartOfFrame;
      FrameTime.StartOfFrame = DateTime.Now;

      FrameTime.ElapsedTimeSinceApplicationOpened
          = FrameTime.StartOfFrame - firstFrameStart_;

      var elapsedSeconds =
          (float) (FrameTime.StartOfFrame - FrameTime.previousFrameStart_)
          .TotalSeconds;
      FrameTime.DeltaTime = elapsedSeconds;
    }

    public static void MarkEndOfFrameForFpsDisplay() {
      var elapsedSeconds
          = (float) (DateTime.Now - FrameTime.StartOfFrame).TotalSeconds;

      for (var i = frameTimesForSmoothedFps_.Length - 1; i >= 1; --i) {
        frameTimesForSmoothedFps_[i] = frameTimesForSmoothedFps_[i - 1];
      }

      frameTimesForSmoothedFps_[0] = elapsedSeconds;

      var smoothedFrameTime = frameTimesForSmoothedFps_.Average();

      SmoothedFps = smoothedFrameTime == 0 ? 0 : 1 / smoothedFrameTime;
    }

    public static DateTime StartOfFrame { get; private set; }

    public static TimeSpan ElapsedTimeSinceApplicationOpened {
      get;
      private set;
    }

    public static float DeltaTime { get; private set; }
    public static float SmoothedFps { get; private set; }
  }
}