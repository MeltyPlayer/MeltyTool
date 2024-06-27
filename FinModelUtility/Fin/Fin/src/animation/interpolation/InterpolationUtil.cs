using System.Runtime.CompilerServices;

using fin.animation.keyframes;
using fin.math.interpolation;

namespace fin.animation.interpolation;

public static class InterpolationUtil {
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void GetTAndDuration<TKeyframe>(
      TKeyframe from,
      TKeyframe to,
      float frame,
      ISharedInterpolationConfig sharedInterpolationConfig,
      out float t,
      out float duration)
      where TKeyframe : IKeyframe {
    var fromFrame = from.Frame;
    var toFrame = to.Frame;

    if (toFrame < fromFrame) {
      var animationLength = sharedInterpolationConfig.AnimationLength;

      if (frame >= fromFrame) {
        toFrame += animationLength;
      } else {
        fromFrame -= animationLength;
      }
    }

    duration = toFrame - fromFrame;
    t = (frame - fromFrame) / duration;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void GetHermiteCoefficients<TKeyframe>(
      TKeyframe from,
      TKeyframe to,
      float frame,
      ISharedInterpolationConfig sharedInterpolationConfig,
      out float fromCoefficient,
      out float toCoefficient,
      out float oneCoefficient)
      where TKeyframe : IKeyframeWithTangents {
    HermiteInterpolationUtil.GetCoefficients(
        from.Frame,
        from.TangentOut,
        to.Frame,
        to.TangentIn,
        frame,
        out fromCoefficient,
        out toCoefficient,
        out oneCoefficient);
  }
}