using System;
using System.Linq;

using fin.ui;

namespace fin.util.time;

public static class FrameTime {
  private static readonly DateTime firstFrameStart_ = DateTime.Now;
  private static DateTime previousFrameStart_;

  private const int SMOOTH_COUNT = UiConstants.FPS;

  private static readonly float[] frameTimesForSmoothedActualFps_
      = new float[SMOOTH_COUNT];

  private static readonly float[] frameTimesForSmoothedTheoreticalFps_
      = new float[SMOOTH_COUNT];

  public static void MarkStartOfFrame() {
    previousFrameStart_ = StartOfFrame;
    StartOfFrame = DateTime.Now;

    ElapsedTimeSinceApplicationOpened
        = StartOfFrame - firstFrameStart_;

    var elapsedSeconds =
        (float) (StartOfFrame - previousFrameStart_)
        .TotalSeconds;
    DeltaTime = elapsedSeconds;

    for (var i = frameTimesForSmoothedActualFps_.Length - 1; i >= 1; --i) {
      frameTimesForSmoothedActualFps_[i]
          = frameTimesForSmoothedActualFps_[i - 1];
    }

    frameTimesForSmoothedActualFps_[0] = elapsedSeconds;

    var smoothedFrameTime = frameTimesForSmoothedActualFps_.Average();

    SmoothedActualFps = smoothedFrameTime == 0 ? 0 : 1 / smoothedFrameTime;
  }

  public static void MarkEndOfFrameForFpsDisplay() {
    var elapsedSeconds
        = (float) (DateTime.Now - StartOfFrame).TotalSeconds;

    for (var i = frameTimesForSmoothedTheoreticalFps_.Length - 1;
         i >= 1;
         --i) {
      frameTimesForSmoothedTheoreticalFps_[i]
          = frameTimesForSmoothedTheoreticalFps_[i - 1];
    }

    frameTimesForSmoothedTheoreticalFps_[0] = elapsedSeconds;

    var smoothedFrameTime = frameTimesForSmoothedTheoreticalFps_.Average();

    SmoothedTheoreticalFps
        = smoothedFrameTime == 0 ? 0 : 1 / smoothedFrameTime;
  }

  public static DateTime StartOfFrame { get; private set; }

  public static TimeSpan ElapsedTimeSinceApplicationOpened {
    get;
    private set;
  }

  public static float DeltaTime { get; private set; }

  public static float SmoothedActualFps { get; private set; }
  public static float SmoothedTheoreticalFps { get; private set; }

  public static string FpsString
    => $"Universal Asset Tool ({SmoothedActualFps:0.0} / {SmoothedTheoreticalFps:0.0} fps)";
}