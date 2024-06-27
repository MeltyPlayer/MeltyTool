using System.Runtime.CompilerServices;

namespace fin.math.interpolation;

public static class HermiteInterpolationUtil {
  /// <summary>
  ///   Shamelessly copied from:
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