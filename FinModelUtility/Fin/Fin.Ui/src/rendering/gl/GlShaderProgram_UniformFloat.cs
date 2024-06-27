using fin.math.floats;
using fin.util.asserts;

using OpenTK.Graphics.OpenGL;


namespace fin.ui.rendering.gl;

public partial class GlShaderProgram {
  public IShaderUniform<float> GetUniformFloat(string name) {
      if (!this.cachedUniforms_.TryGetValue(name, out var uniform)) {
        this.cachedUniforms_[name] = uniform =
            new FloatShaderUniform(this.GetUniformLocation_(name));
      }

      return Asserts.AsA<IShaderUniform<float>>(uniform);
    }

  private class FloatShaderUniform : BShaderUniform, IShaderUniform<float> {
    private readonly int location_;
    private float value_;

    public FloatShaderUniform(int location) {
        this.location_ = location;
      }

    public void SetAndMarkDirty(in float value) {
        this.value_ = value;
        this.MarkDirty();
      }

    public void SetAndMaybeMarkDirty(in float value) {
        if (this.value_ == value) {
          return;
        }

        this.value_ = value;
        this.MarkDirty();
      }

    protected override void PassValueToProgram()
      => GL.Uniform1(this.location_, this.value_);
  }
}