using fin.util.asserts;

using OpenTK.Graphics.OpenGL4;


namespace fin.ui.rendering.gl;

public partial class GlShaderProgram {
  public IShaderUniform<float> GetUniformFloat(string name) {
      if (!this.cachedUniformsByName_.TryGetValue(name, out var uniform)) {
        this.cachedUniformsByName_[name] = uniform =
            new FloatShaderUniform(this.GetUniformLocation_(name));
      }

      return Asserts.AsA<IShaderUniform<float>>(uniform);
    }

  private class FloatShaderUniform(int location)
      : BShaderUniform(location), IShaderUniform<float> {
    private float value_;

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
      => GL.Uniform1(location, this.value_);
  }
}