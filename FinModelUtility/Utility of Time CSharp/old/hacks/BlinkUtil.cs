using System;

namespace UoT.hacks {
  public static class BlinkUtil {
    // TODO: Get the exact right values for this.

    public const int OPEN_FRAMES = 48;
    public const int HALF_OPEN_FRAMES = 1;
    public const int CLOSED_FRAMES = 1;

    public const int TOTAL_FRAMES =
        OPEN_FRAMES +
        HALF_OPEN_FRAMES +
        CLOSED_FRAMES +
        HALF_OPEN_FRAMES;

    public static T Get<T>(T open, T halfOpen, T closed) {
      var frame = (int) Math.Floor(Time.Frame % TOTAL_FRAMES);

      if (frame < OPEN_FRAMES) {
        return open;
      }

      if (frame < OPEN_FRAMES + HALF_OPEN_FRAMES) {
        return halfOpen;
      }

      if (frame <
          OPEN_FRAMES +
          HALF_OPEN_FRAMES +
          CLOSED_FRAMES) {
        return closed;
      }

      return halfOpen;
    }
  }
}