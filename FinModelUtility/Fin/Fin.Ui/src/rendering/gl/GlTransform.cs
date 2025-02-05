using System.Numerics;
using System.Runtime.CompilerServices;

using fin.math.matrix.four;
using fin.math.rotations;

using OpenTK.Graphics.OpenGL;

using GlMatrixMode = OpenTK.Graphics.OpenGL.MatrixMode;
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
  public static unsafe void UniformMatrix4(int location, Matrix4x4 matrix) {
    var ptr = &(matrix.M11);
    GL.UniformMatrix4(location, 1, false, ptr);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void UniformMatrix4s(int location,
                                            ReadOnlySpan<Matrix4x4>
                                                matrices) {
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
        TransformMatrixMode.MODEL      => modelMatrix_,
        TransformMatrixMode.VIEW       => viewMatrix_,
        TransformMatrixMode.PROJECTION => projectionMatrix_,
    };

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void LoadIdentity() {
    currentMatrix_.SetIdentity();
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void MultMatrix(Matrix4x4 matrix) {
    currentMatrix_.MultiplyInPlace(matrix);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Translate(double x, double y, double z)
    => Translate((float) x, (float) y, (float) z);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Translate(float x, float y, float z)
    => MultMatrix(SystemMatrix4x4Util.FromTranslation(x, y, z));


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

  public static void Ortho2d(int left, int right, int bottom, int top) {
    var near = -1;
    var far = 1;

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

  public static void LookAt(
      double eyeX,
      double eyeY,
      double eyeZ,
      double centerX,
      double centerY,
      double centerZ,
      double upX,
      double upY,
      double upZ) {
    var lookX = centerX - eyeX;
    var lookY = centerY - eyeY;
    var lookZ = centerZ - eyeZ;
    Normalize3(ref lookX, ref lookY, ref lookZ);

    CrossProduct3(
        lookX,
        lookY,
        lookZ,
        upX,
        upY,
        upZ,
        out var sideX,
        out var sideY,
        out var sideZ);
    Normalize3(ref sideX, ref sideY, ref sideZ);

    CrossProduct3(
        sideX,
        sideY,
        sideZ,
        lookX,
        lookY,
        lookZ,
        out upX,
        out upY,
        out upZ);

    var matrix = new Matrix4x4();

    SetInMatrix(ref matrix, 0, 0, sideX);
    SetInMatrix(ref matrix, 1, 0, sideY);
    SetInMatrix(ref matrix, 2, 0, sideZ);

    SetInMatrix(ref matrix, 0, 1, upX);
    SetInMatrix(ref matrix, 1, 1, upY);
    SetInMatrix(ref matrix, 2, 1, upZ);

    SetInMatrix(ref matrix, 0, 2, -lookX);
    SetInMatrix(ref matrix, 1, 2, -lookY);
    SetInMatrix(ref matrix, 2, 2, -lookZ);

    SetInMatrix(ref matrix, 3, 3, 1);

    MultMatrix(matrix);
    Translate(-eyeX, -eyeY, -eyeZ);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void SetInMatrix(ref Matrix4x4 matrix,
                                 int r,
                                 int c,
                                 double value)
    => matrix[r, c] = (float) value;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void CrossProduct3(
      double x1,
      double y1,
      double z1,
      double x2,
      double y2,
      double z2,
      out double outX,
      out double outY,
      out double outZ) {
    outX = y1 * z2 - z1 * y2;
    outY = z1 * x2 - x1 * z2;
    outZ = x1 * y2 - y1 * x2;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Normalize3(ref double x, ref double y, ref double z) {
    var inverseLength = 1 / Math.Sqrt(x * x + y * y + z * z);
    x *= inverseLength;
    y *= inverseLength;
    z *= inverseLength;
  }
}