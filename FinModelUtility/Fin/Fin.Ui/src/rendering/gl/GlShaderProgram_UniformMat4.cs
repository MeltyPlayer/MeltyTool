using System.Numerics;

using fin.util.asserts;

using OpenTK.Graphics.OpenGL4;


namespace fin.ui.rendering.gl;

public partial class GlShaderProgram {
  public IShaderUniform<Matrix4x4> GetUniformMat4(string name) {
    if (!this.cachedUniformsByName_.TryGetValue(name, out var uniform)) {
      this.cachedUniformsByName_[name] = uniform =
          new Mat4ShaderUniform(this.GetUniformLocation_(name));
    }

    return Asserts.AsA<IShaderUniform<Matrix4x4>>(uniform);
  }

  private class Mat4ShaderUniform(int location)
      : BShaderUniform(location), IShaderUniform<Matrix4x4> {
    private Matrix4x4 value_;

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
        GL.UniformMatrix4(location, 1, false, ptr);
      }
    }
  }
}