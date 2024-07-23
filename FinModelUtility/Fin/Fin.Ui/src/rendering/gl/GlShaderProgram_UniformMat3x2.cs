using System.Numerics;
using System.Runtime.CompilerServices;

using fin.util.asserts;

using OpenTK.Graphics.OpenGL;


namespace fin.ui.rendering.gl;

public partial class GlShaderProgram {
  public IShaderUniform<Matrix3x2> GetUniformMat3x2(string name) {
    if (!this.cachedUniforms_.TryGetValue(name, out var uniform)) {
      this.cachedUniforms_[name] = uniform =
          new Mat3x2ShaderUniform(this.GetUniformLocation_(name));
    }

    return Asserts.AsA<IShaderUniform<Matrix3x2>>(uniform);
  }

  private class Mat3x2ShaderUniform(int location)
      : BShaderUniform, IShaderUniform<Matrix3x2> {
    private Matrix3x2 value_;

    public void SetAndMarkDirty(in Matrix3x2 value) {
      this.value_ = value;
      this.MarkDirty();
    }

    public void SetAndMaybeMarkDirty(in Matrix3x2 value) {
      if (this.value_.Equals(value)) {
        return;
      }

      this.value_ = value;
      this.MarkDirty();
    }

    protected override unsafe void PassValueToProgram() {
      fixed (float* ptr = &this.value_.M11) {
        GL.UniformMatrix3x2(location, 1, false, ptr);
      }
    }
  }
}