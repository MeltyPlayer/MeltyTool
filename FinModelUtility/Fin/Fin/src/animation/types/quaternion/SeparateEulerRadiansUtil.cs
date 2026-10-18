using System;

using fin.animation.keyframes;
using fin.math.floats;

namespace fin.animation.types.quaternion;

public static class SeparateEulerRadiansKeyframesUtil {
  public static bool CanInterpolateWithQuaternions<TKeyframe>(
      ReadOnlySpan<bool> areFromsAndTosNull,
      ReadOnlySpan<TKeyframe?> fromsAndTos,
      ReadOnlySpan<bool> areAxesStatic) where TKeyframe : IKeyframe<float> {
    for (var i = 0; i < 3; ++i) {
      if (areAxesStatic[i % 3]) {
        continue;
      }

      if (areFromsAndTosNull[i] || areFromsAndTosNull[3 + i]) {
        return false;
      }

      if (fromsAndTos[i] is IKeyframeWithTangents<float> from &&
          (from.TangentOut ?? 0) != 0) {
        return false;
      }

      if (fromsAndTos[i] is IKeyframeWithTangents<float> to &&
          (to.TangentIn ?? 0) != 0) {
        return false;
      }
    }

    for (var i = 0; i < 3; ++i) {
      if (areAxesStatic[i]) {
        continue;
      }

      var from = fromsAndTos[i];
      for (var oi = i + 1; oi < 3; ++oi) {
        if (areAxesStatic[oi]) {
          continue;
        }

        var to = fromsAndTos[oi];
        if (!from.Frame.IsRoughly(to.Frame)) {
          return false;
        }
      }
    }

    return true;
  }

  public static void AreAxesStatic<TKeyframe>(
      ReadOnlySpan<bool> areFromsAndTosNull,
      ReadOnlySpan<TKeyframe?> fromsAndTos,
      Span<bool> areAxesStatic) where TKeyframe : IKeyframe<float> {
    for (var i = 0; i < 3; ++i) {
      var fromIsNull = areFromsAndTosNull[i];
      var toIsNull = areFromsAndTosNull[3 + i];

      if (fromIsNull && toIsNull) {
        areAxesStatic[i] = true;
      } else if (!fromIsNull && !toIsNull) {
        var from = fromsAndTos[i];
        var to = fromsAndTos[3 + i];
        if (from.ValueOut.IsRoughly(to.ValueIn)) {
          areAxesStatic[i] = true;
        }
      }
    }
  }

  public static bool GetFromAndToFrameIndex<TKeyframe>(
      ReadOnlySpan<TKeyframe?> fromsAndTos,
      ReadOnlySpan<bool> areAxesStatic,
      out float fromFrameIndex,
      out float toFrameIndex) where TKeyframe : IKeyframe<float> {
    for (var i = 0; i < 3; ++i) {
      if (!areAxesStatic[i]) {
        fromFrameIndex = fromsAndTos[i].Frame;
        toFrameIndex = fromsAndTos[3 + i].Frame;
        return true;
      }
    }

    fromFrameIndex = 0;
    toFrameIndex = 0;
    return false;
  }

  public static float GetFromValueOrDefault<TKeyframe>(
      KeyframesUtil.InterpolationDataType interpolationType,
      TKeyframe? fromKeyframe,
      float defaultValue) where TKeyframe : IKeyframe<float>
    => interpolationType == KeyframesUtil.InterpolationDataType.NONE
        ? defaultValue
        : fromKeyframe.ValueOut;

  public static float GetToValueOrDefault<TKeyframe>(
      KeyframesUtil.InterpolationDataType interpolationType,
      TKeyframe? toKeyframe,
      float defaultValue) where TKeyframe : IKeyframe<float>
    => interpolationType ==
       KeyframesUtil.InterpolationDataType.PRECEDING_AND_FOLLOWING
        ? toKeyframe.ValueIn
        : defaultValue;
}