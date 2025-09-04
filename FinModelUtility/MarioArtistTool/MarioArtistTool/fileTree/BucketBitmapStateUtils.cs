using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MarioArtistTool.fileTree;

public enum BucketBitmapState {
  IDLE,
  WAVE_1_IN,
  WAVE_2_IN,
  WAVE_3_IN,
  WAVE_4_IN,
  WAVE_4_OUT,
  WAVE_2_OUT,
  WAVE_3_OUT,
  WAVE_1_OUT,
  WAVING,
}

public static class BucketBitmapStateExtensions {
  public static bool IsWaving(this BucketBitmapState state)
    => state is BucketBitmapState.WAVE_1_IN
                or BucketBitmapState.WAVE_2_IN
                or BucketBitmapState.WAVE_3_IN
                or BucketBitmapState.WAVE_4_IN
                or BucketBitmapState.WAVE_4_OUT
                or BucketBitmapState.WAVE_3_OUT
                or BucketBitmapState.WAVE_2_OUT
                or BucketBitmapState.WAVE_1_OUT
                or BucketBitmapState.WAVING;
}

public static class BucketBitmapStateUtils {
  private const float STATE_TIME = .1f;

  public static async IAsyncEnumerable<BucketBitmapState> GetPath(
      BucketBitmapState from,
      BucketBitmapState to,
      CancellationToken cancellationToken) {
    while (!cancellationToken.IsCancellationRequested &&
           TryGetNextState_(from, to, out var next)) {
      yield return next;
      await Task.Delay((int) (STATE_TIME * 1000), cancellationToken);
      from = next;
    }
  }

  private static bool TryGetNextState_(BucketBitmapState from,
                                       BucketBitmapState to,
                                       out BucketBitmapState next) {
    if (from == to) {
      next = to;
      return false;
    }

    // Continuously waving
    if (from.IsWaving() && to.IsWaving()) {
      if (from == BucketBitmapState.WAVE_1_OUT) {
        next = BucketBitmapState.WAVE_1_IN;
      } else {
        next = from + 1;
      }
       
      return true;
    }

    if (from.IsWaving()) {
      if (from is BucketBitmapState.WAVE_1_OUT) {
        next = BucketBitmapState.IDLE;
      } else {
        next = from + 1;
      }

      return true;
    }

    if (to.IsWaving()) {
      if (from is BucketBitmapState.IDLE) {
        next = BucketBitmapState.WAVE_1_IN;
      } else {
        // TODO: Handle returning back to idle
        next = from + 1;
      }

      return true;
    }

    throw new NotImplementedException();
  }
}