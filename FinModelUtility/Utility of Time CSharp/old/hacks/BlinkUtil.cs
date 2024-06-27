using System;

namespace UoT.hacks {
  public static class BlinkUtil {
    // TODO: Get the exact right values for this.

    public const int OPEN_FRAMES = 48;
    public const int HALF_OPEN_FRAMES = 1;
    public const int CLOSED_FRAMES = 1;

    public const int TOTAL_FRAMES =
        BlinkUtil.OPEN_FRAMES +
        BlinkUtil.HALF_OPEN_FRAMES +
        BlinkUtil.CLOSED_FRAMES +
        BlinkUtil.HALF_OPEN_FRAMES;

    public static T Get<T>(T open, T halfOpen, T closed) {
      var frame = (int) Math.Floor(Time.Frame % BlinkUtil.TOTAL_FRAMES);

      if (frame < BlinkUtil.OPEN_FRAMES) {
        return open;
      }

      if (frame < BlinkUtil.OPEN_FRAMES + BlinkUtil.HALF_OPEN_FRAMES) {
        return halfOpen;
      }

      if (frame <
          BlinkUtil.OPEN_FRAMES +
          BlinkUtil.HALF_OPEN_FRAMES +
          BlinkUtil.CLOSED_FRAMES) {
        return closed;
      }

      return halfOpen;
    }
  }
}