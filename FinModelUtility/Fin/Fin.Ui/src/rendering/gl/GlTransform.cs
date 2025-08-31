using System.Numerics;
using System.Runtime.CompilerServices;

using fin.math.matrix.four;
using fin.math.rotations;

using OpenTK.Graphics.OpenGL;

using Vector3 = System.Numerics.Vector3;

namespace fin.ui.rendering.gl;

public enum TransformMatrixMode {
  MODEL,
  VIEW,
  PROJECTION,
}

public static class GlTransform {
  private static readonly Matrix4x4Stack modelMatrix_ = new();
  private static readonly Matrix4x4Stack viewMatrix_ = new();
  private static readonly Matrix4x4Stack projectionMatrix_ = new();

  private static Matrix4x4Stack currentMatrix_ = modelMatrix_;

  public static Matrix4x4 ModelMatrix {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => modelMatrix_.Top;
  }

  public static Matrix4x4 ViewMatrix {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => viewMatrix_.Top;
  }

  public static Matrix4x4 ProjectionMatrix {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => projectionMatrix_.Top;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void UniformMatrix4(int location, in Matrix4x4 matrix) {
    fixed (float* ptr = &matrix.M11) {
      GL.UniformMatrix4(location, 1, false, ptr);
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void UniformMatrix4s(int location,
                                            ReadOnlySpan<Matrix4x4> matrices) {
    fixed (float* ptr = &(matrices[0].M11)) {
      GL.UniformMatrix4(location, matrices.Length, false, ptr);
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void PushMatrix() {
    currentMatrix_.Push();
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void PopMatrix() {
    currentMatrix_.Pop();
  }

  public static void MatrixMode(TransformMatrixMode mode)
    => currentMatrix_ = mode switch {
        TransformMatrixMode.MODEL => modelMatrix_,
        TransformMatrixMode.VIEW => viewMatrix_,
        TransformMatrixMode.PROJECTION => projectionMatrix_,
        _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
    };

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void LoadIdentity() => currentMatrix_.SetIdentity();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Set(in Matrix4x4 matrix) => currentMatrix_.Top = matrix;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void MultMatrix(in Matrix4x4 matrix)
    => currentMatrix_.MultiplyInPlace(matrix);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Translate(double x, double y, double z)
    => Translate((float) x, (float) y, (float) z);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Translate(float x, float y, float z)
    => MultMatrix(SystemMatrix4x4Util.FromTranslation(x, y, z));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Translate(in Vector3 xyz)
    => Translate(xyz.X, xyz.Y, xyz.Z);


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Scale(double x, double y, double z)
    => Scale((float) x, (float) y, (float) z);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Scale(float x, float y, float z)
    => MultMatrix(SystemMatrix4x4Util.FromScale(x, y, z));


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Rotate(double degrees, double x, double y, double z)
    => Rotate((float) degrees, (float) x, (float) y, (float) z);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Rotate(float degrees, float x, float y, float z)
    => MultMatrix(
        Matrix4x4.CreateFromAxisAngle(new Vector3(x, y, z),
                                      degrees * FinTrig.DEG_2_RAD));


  public static void Perspective(double fovYDegrees,
                                 double aspectRatio,
                                 double zNear,
                                 double zFar) {
    var matrix = new Matrix4x4();

    var f = 1.0 / Math.Tan(fovYDegrees * FinTrig.DEG_2_RAD * .5f);

    SetInMatrix(ref matrix, 0, 0, f / aspectRatio);
    SetInMatrix(ref matrix, 1, 1, f);
    SetInMatrix(ref matrix, 2, 2, (zNear + zFar) / (zNear - zFar));
    SetInMatrix(ref matrix, 3, 2, 2 * zNear * zFar / (zNear - zFar));
    SetInMatrix(ref matrix, 2, 3, -1);

    MultMatrix(matrix);
  }

  public static void Ortho2d(int left,
                             int right,
                             int bottom,
                             int top)
    => Ortho2d(left, right, bottom, top, -1, 1);

  public static void Ortho2d(int left,
                             int right,
                             int bottom,
                             int top,
                             float near,
                             float far) {
    var matrix = new Matrix4x4();

    SetInMatrix(ref matrix, 0, 0, 2f / (right - left));
    SetInMatrix(ref matrix, 1, 1, 2f / (top - bottom));
    SetInMatrix(ref matrix, 2, 2, -2f / (far - near));
    SetInMatrix(ref matrix, 3, 0, -(1f * right + left) / (right - left));
    SetInMatrix(ref matrix, 3, 1, -(1f * top + bottom) / (top - bottom));
    SetInMatrix(ref matrix, 3, 2, -(1f * far + near) / (far - near));
    SetInMatrix(ref matrix, 3, 3, 1);

    MultMatrix(matrix);
  }

  public static void LookAt(in Vector3 eye,
                            in Vector3 center,
                            in Vector3 up) {
    var look = Vector3.Normalize(center - eye);
    var side = Vector3.Normalize(Vector3.Cross(look, up));
    var newUp = Vector3.Cross(side, look);

    var matrix = new Matrix4x4();

    SetInMatrix(ref matrix, 0, 0, side.X);
    SetInMatrix(ref matrix, 1, 0, side.Y);
    SetInMatrix(ref matrix, 2, 0, side.Z);

    SetInMatrix(ref matrix, 0, 1, newUp.X);
    SetInMatrix(ref matrix, 1, 1, newUp.Y);
    SetInMatrix(ref matrix, 2, 1, newUp.Z);

    SetInMatrix(ref matrix, 0, 2, -look.X);
    SetInMatrix(ref matrix, 1, 2, -look.Y);
    SetInMatrix(ref matrix, 2, 2, -look.Z);

    SetInMatrix(ref matrix, 3, 3, 1);

    MultMatrix(matrix);
    Translate(-eye);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void SetInMatrix(ref Matrix4x4 matrix,
                                 int r,
                                 int c,
                                 double value)
    => matrix[r, c] = (float) value;
}