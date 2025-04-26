using System.Runtime.CompilerServices;

using fin.animation.interpolation;
using fin.animation.keyframes;
using fin.animation.types.single;
using fin.math.rotations;

namespace fin.animation.types.radians;

public class RadiansKeyframeInterpolator
    : RadiansKeyframeInterpolator<Keyframe<float>> {
  public static RadiansKeyframeInterpolator Instance { get; } = new();
  private RadiansKeyframeInterpolator() { }
}

public class RadiansKeyframeInterpolator<TKeyframe>
    : IRadiansKeyframeInterpolator<TKeyframe>
    where TKeyframe : IKeyframe<float> {
  public float Interpolate(
      TKeyframe from,
      TKeyframe to,
      float frame,
      ISharedInterpolationConfig sharedInterpolationConfig) {
    InterpolationUtil.GetTAndDuration(from,
                                      to,
                                      frame,
                                      sharedInterpolationConfig,
                                      out var t,
                                      out _);

    var fromValue = from.ValueOut;
    var toValue = to.ValueIn;
    if (!sharedInterpolationConfig.DisableNearestRotationFix) {
      toValue = RadiansKeyframeInterpolator.GetAdjustedToValue(
          fromValue,
          to.ValueIn);
    }

    return FloatKeyframeInterpolator.Interpolate(fromValue, toValue, t);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float GetAdjustedToValue(float fromValue, float rawToValue)
    => fromValue + RadiansUtil.CalculateRadiansTowards(fromValue, rawToValue);
}