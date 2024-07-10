using System.Numerics;

using fin.math.matrix.three;

namespace fin.math.transform;

public static class Transform2dExtensions {
  public static void SetMatrix(this ITransform2d transform,
                               IReadOnlyFinMatrix3x2 matrix)
    => transform.SetMatrix(matrix.Impl);

  public static void SetMatrix(this ITransform2d transform,
                               Matrix3x2 matrix) {
    matrix.Decompose(out var scale,
                     out var quaternion,
                     out var translation,
                     out _);

    transform.Translation = translation;
    transform.Rotation = quaternion;
    transform.Scale = scale;
  }


  public static void SetTranslation(this ITransform2d transform,
                                    float x,
                                    float y)
    => transform.Translation = new Vector2(x, y);

  public static void SetScale(this ITransform2d transform,
                              float x,
                              float y)
    => transform.Scale = new Vector2(x, y);
}