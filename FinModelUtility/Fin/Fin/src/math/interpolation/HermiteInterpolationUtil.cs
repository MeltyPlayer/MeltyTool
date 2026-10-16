using System.Collections.Generic;
using System.Runtime.CompilerServices;

using CommunityToolkit.HighPerformance;

using fin.animation.keyframes;
using fin.math.floats;
using fin.util.asserts;

namespace fin.math.interpolation;

/// <summary>
///   Hermite was a French guy, it's pronounced "Air-meet".
/// </summary>
public static class HermiteInterpolationUtil {
  /// <summary>
  ///   Calculates the tangent at a given time within an Hermite interpolation.
  ///   Useful for splitting a single interpolation into two.
  /// </summary>
  public static float GetTangent(float fromValue,
                                 float fromTime,
                                 float fromTangent,
                                 float toValue,
                                 float toTime,
                                 float toTangent,
                                 float time) {
    var dt = toTime - fromTime;

    var m0 = fromTangent * dt;
    var m1 = toTangent * dt;

    var t1 = (time - fromTime) / dt;
    var t2 = t1 * t1;

    var m2 = 6 * (fromValue - toValue) * (t1 - 1) * t1 +
           m0 * (3 * t2 - 4 * t1 + 1) +
           m1 * t1 * (-2 + 3 * t1);
    return m2 / dt;
  }

  public static bool TryGetTangent(
      IInterpolatableKeyframes<KeyframeWithTangents<float>, float> keyframes,
      float frame,
      out float tangentIn,
      out float tangentOut) {
    var unsafeList = keyframes.Definitions
                              .AssertAsA<List<KeyframeWithTangents<float>>>();
    if (unsafeList.AsSpan()
                  .TryGetPrecedingAndFollowingKeyframes(
            frame,
            keyframes.SharedConfig,
            keyframes.IndividualConfig!,
            out var precedingKeyframe,
            out var followingKeyframe,
            out var normalizedFrame) !=
        KeyframesUtil.InterpolationDataType.PRECEDING_AND_FOLLOWING) {
      tangentIn = tangentOut = 0;
      return false;
    }

    if (frame.IsRoughly(precedingKeyframe.Frame)) {
      tangentIn = precedingKeyframe.TangentIn ?? 0;
      tangentOut = precedingKeyframe.TangentOut ?? 0;
      return true;
    }

    tangentIn = tangentOut = GetTangent(precedingKeyframe.ValueOut,
                      precedingKeyframe.Frame,
                      precedingKeyframe.TangentOut ?? 0,
                      followingKeyframe.ValueIn,
                      followingKeyframe.Frame,
                      followingKeyframe.TangentIn ?? 0,
                      normalizedFrame);
    return true;
  }

  /// <summary>
  ///   Shamelessly stolen from:
  ///   https://answers.unity.com/questions/464782/t-is-the-math-behind-animationcurveevaluate.html
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void GetCoefficients(float fromTime,
                                     float fromTangent,
                                     float toTime,
                                     float toTangent,
                                     float time,
                                     out float fromCoefficient,
                                     out float toCoefficient,
                                     out float oneCoefficient) {
    var dt = toTime - fromTime;

    var m0 = fromTangent * dt;
    var m1 = toTangent * dt;

    var t1 = (time - fromTime) / (toTime - fromTime);
    var t2 = t1 * t1;
    var t3 = t2 * t1;

    var a = 2 * t3 - 3 * t2 + 1;
    var b = t3 - 2 * t2 + t1;
    var c = t3 - t2;
    var d = -2 * t3 + 3 * t2;

    fromCoefficient = a;
    toCoefficient = d;
    oneCoefficient = b * m0 + c * m1;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float InterpolateFloats(
      float fromTime,
      float fromValue,
      float fromTangent,
      float toTime,
      float toValue,
      float toTangent,
      float time) {
    GetCoefficients(fromTime,
                    fromTangent,
                    toTime,
                    toTangent,
                    time,
                    out var fromCoefficient,
                    out var toCoefficient,
                    out var oneCoefficient);

    return fromValue * fromCoefficient +
           toValue * toCoefficient +
           oneCoefficient;
  }
}