using System.Numerics;

using fin.animation.keyframes;

namespace fin.animation.interpolation;

public class Vector3KeyframeWithTangentsInterpolator
    : Vector3KeyframeWithTangentsInterpolator<KeyframeWithTangents<Vector3>> {
  public static Vector3KeyframeWithTangentsInterpolator Instance { get; }
    = new();

  private Vector3KeyframeWithTangentsInterpolator() { }
}

public class Vector3KeyframeWithTangentsInterpolator<TKeyframe>
    : IKeyframeInterpolator<TKeyframe, Vector3>
    where TKeyframe : IKeyframeWithTangents<Vector3> {
  public Vector3 Interpolate(
      TKeyframe from,
      TKeyframe to,
      float frame,
      ISharedInterpolationConfig sharedInterpolationConfig) {
    InterpolationUtil.GetHermiteCoefficients(
        from,
        to,
        frame,
        sharedInterpolationConfig,
        out var fromCoefficient,
        out var toCoefficient,
        out var oneCoefficient);

    return fromCoefficient * from.Value +
           toCoefficient * to.Value +
           Vector3.One * oneCoefficient;
  }
}