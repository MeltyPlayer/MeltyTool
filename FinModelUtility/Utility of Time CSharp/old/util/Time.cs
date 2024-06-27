using System;

namespace UoT {
  public static class Time {
    private static readonly DateTime ZERO_ = DateTime.UtcNow; //new DateTime(1970, 1, 1);

    public static double Current { get; private set; }
    public static double CurrentFrac => Time.Current % 1;


    // TODO: Base this on the framerate set in the animation tab.
    public static double FrameRate { get; set; } = 20;
    public static double Frame => Time.Current * Time.FrameRate;


    public static double TrueCurrent
      => (DateTime.UtcNow - Time.ZERO_).TotalSeconds;

    public static void UpdateCurrent() => Time.Current = Time.TrueCurrent;
  }
}