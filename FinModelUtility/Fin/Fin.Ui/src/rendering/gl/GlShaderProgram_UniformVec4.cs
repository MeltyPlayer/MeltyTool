using System.Numerics;

using fin.math.matrix.four;
using fin.util.asserts;

using OpenTK.Graphics.OpenGL;


namespace fin.ui.rendering.gl;

public partial class GlShaderProgram {
  public IShaderUniform<Vector4> GetUniformVec4(string name) {
    if (!this.cachedUniforms_.TryGetValue(name, out var uniform)) {
      this.cachedUniforms_[name] = uniform =
          new Vec4ShaderUniform(this.GetUniformLocation_(name));
    }

    return Asserts.AsA<IShaderUniform<Vector4>>(uniform);
  }

  private class Vec4ShaderUniform : BShaderUniform, IShaderUniform<Vector4> {
    private readonly int location_;
    private Vector4 value_;

    public Vec4ShaderUniform(int location) {
      this.location_ = location;
    }

    public void SetAndMarkDirty(in Vector4 value) {
      this.value_ = value;
      this.MarkDirty();
    }

    public void SetAndMaybeMarkDirty(in Vector4 value) {
      if (this.value_.Equals(value)) {
        return;
      }

      this.value_ = value;
      this.MarkDirty();
    }

    protected override unsafe void PassValueToProgram() {
      fixed (float* ptr = &this.value_.X) {
        GL.Uniform4(this.location_, 1, ptr);
      }
    }
  }
}