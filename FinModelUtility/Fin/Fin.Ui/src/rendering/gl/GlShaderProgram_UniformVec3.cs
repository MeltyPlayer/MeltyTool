using System.Numerics;

using fin.util.asserts;

using OpenTK.Graphics.OpenGL;


namespace fin.ui.rendering.gl;

public partial class GlShaderProgram {
  public IShaderUniform<Vector3> GetUniformVec3(string name) {
      if (!this.cachedUniforms_.TryGetValue(name, out var uniform)) {
        this.cachedUniforms_[name] = uniform =
            new Vec3ShaderUniform(this.GetUniformLocation_(name));
      }

      return Asserts.AsA<IShaderUniform<Vector3>>(uniform);
    }

  private class Vec3ShaderUniform : BShaderUniform, IShaderUniform<Vector3> {
    private readonly int location_;
    private Vector3 value_;

    public Vec3ShaderUniform(int location) {
        this.location_ = location;
      }

    public void SetAndMarkDirty(in Vector3 value) {
        this.value_ = value;
        this.MarkDirty();
      }

    public void SetAndMaybeMarkDirty(in Vector3 value) {
        if (this.value_.Equals(value)) {
          return;
        }

        this.value_ = value;
        this.MarkDirty();
      }

    protected override unsafe void PassValueToProgram() {
        fixed (float* ptr = &this.value_.X) {
          GL.Uniform3(this.location_, 1, ptr);
        }
      }
  }
}