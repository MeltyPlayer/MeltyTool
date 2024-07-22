using System.Numerics;

using CommunityToolkit.HighPerformance;

using fin.color;
using fin.math.xyz;

namespace fin.ui.rendering.gl.ubo;

public static class UboUtil {
  public const int SIZE_OF_VECTOR3 = 4 * 3;
  public const int SIZE_OF_VECTOR4 = 4 * 4;
  public const int SIZE_OF_MATRIX4X4 = 4 * 4 * 4;

  public static void AppendBool(Span<byte> buffer,
                                ref int offset,
                                bool value)
    => AppendInt(buffer, ref offset, value ? 1 : 0);

  public static void AppendInt(Span<byte> buffer,
                               ref int offset,
                               int value) {
    buffer.Slice(offset, 4).Cast<byte, int>()[0] = value;
    offset += 4;
  }

  public static void AppendVector3(Span<byte> buffer,
                                   ref int offset,
                                   Vector3? vector3) {
    buffer.Slice(offset, SIZE_OF_VECTOR3).Cast<byte, Vector3>()[0]
        = vector3 ?? Vector3.Zero;
    offset += SIZE_OF_VECTOR3;
  }

  public static void AppendVector3(Span<byte> buffer,
                                   ref int offset,
                                   IReadOnlyXyz? xyz)
    => AppendVector3(
        buffer,
        ref offset,
        xyz != null ? new Vector3(xyz.X, xyz.Y, xyz.Z) : Vector3.Zero);

  public static void AppendVector4(Span<byte> buffer,
                                   ref int offset,
                                   Vector4? vector4) {
    buffer.Slice(offset, SIZE_OF_VECTOR4).Cast<byte, Vector4>()[0]
        = vector4 ?? Vector4.Zero;
    offset += SIZE_OF_VECTOR4;
  }

  public static void AppendMatrix4x4(Span<byte> buffer,
                                     ref int offset,
                                     Matrix4x4 matrix4x4) {
    buffer.Slice(offset, SIZE_OF_MATRIX4X4).Cast<byte, Matrix4x4>()[0]
        = matrix4x4;
    offset += SIZE_OF_MATRIX4X4;
  }

  public static void AppendMatrix4x4s(Span<byte> buffer,
                                      ref int offset,
                                      ReadOnlySpan<Matrix4x4> matrix4x4s) {
    matrix4x4s.CopyTo(
        buffer.Slice(offset, matrix4x4s.Length * SIZE_OF_MATRIX4X4)
              .Cast<byte, Matrix4x4>());
    offset += matrix4x4s.Length * SIZE_OF_MATRIX4X4;
  }
}