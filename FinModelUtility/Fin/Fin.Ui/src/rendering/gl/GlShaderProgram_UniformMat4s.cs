using System.Numerics;

using fin.util.asserts;

using OpenTK.Graphics.ES30;


namespace fin.ui.rendering.gl;

public partial class GlShaderProgram {
  public IShaderUniformArray<Matrix4x4> GetUniformMat4s(
      string name,
      int length) {
      if (!this.cachedUniforms_.TryGetValue(name, out var uniform)) {
        this.cachedUniforms_[name] = uniform =
            new Mat4sShaderUniform(this.GetUniformLocation_(name), length);
      }

      return Asserts.AsA<IShaderUniformArray<Matrix4x4>>(uniform);
    }

  private class Mat4sShaderUniform(int location, int length)
      : BShaderUniform, IShaderUniformArray<Matrix4x4> {
    private readonly Matrix4x4[] value_ = new Matrix4x4[length];

    public void SetAndMarkDirty(int index, in Matrix4x4 value) {
        this.value_[index] = value;
        this.MarkDirty();
      }

    public void SetAndMaybeMarkDirty(int index, in Matrix4x4 value) {
        if (!this.IsDirty) {
          var current = this.value_[index];
          if (!current.Equals(value)) {
            this.MarkDirty();
          }
        }

        this.value_[index] = value;
      }

    protected override unsafe void PassValueToProgram() {
        fixed (Matrix4x4* mat = this.value_) {
          GL.UniformMatrix4(location,
                            this.value_.Length,
                            false,
                            (float*) mat);
        }
      }
  }
}