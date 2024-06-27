using System.Numerics;

using fin.util.asserts;

using OpenTK.Graphics.OpenGL;


namespace fin.ui.rendering.gl;

public partial class GlShaderProgram {
  public IShaderUniform<Matrix4x4> GetUniformMat4(string name) {
      if (!this.cachedUniforms_.TryGetValue(name, out var uniform)) {
        this.cachedUniforms_[name] = uniform =
            new Mat4ShaderUniform(this.GetUniformLocation_(name));
      }

      return Asserts.AsA<IShaderUniform<Matrix4x4>>(uniform);
    }

  private class Mat4ShaderUniform
      : BShaderUniform, IShaderUniform<Matrix4x4> {
    private readonly int location_;
    private Matrix4x4 value_;

    public Mat4ShaderUniform(int location) {
        this.location_ = location;
      }

    public void SetAndMarkDirty(in Matrix4x4 value) {
        this.value_ = value;
        this.MarkDirty();
      }

    public void SetAndMaybeMarkDirty(in Matrix4x4 value) {
        if (this.value_.Equals(value)) {
          return;
        }

        this.value_ = value;
        this.MarkDirty();
      }

    protected override unsafe void PassValueToProgram() {
        fixed (float* ptr = &this.value_.M11) {
          GL.UniformMatrix4(this.location_, 1, false, ptr);
        }
      }
  }
}