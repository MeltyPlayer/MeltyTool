using fin.util.asserts;

using OpenTK.Graphics.OpenGL4;


namespace fin.ui.rendering.gl;

public partial class GlShaderProgram {
  public IShaderUniform<long> GetUniformSampler2d(string name) {
    if (!this.cachedUniformsByName_.TryGetValue(name, out var uniform)) {
      this.cachedUniformsByName_[name] = uniform =
          new Sampler2dShaderUniform(this.GetUniformLocation_(name));
    }

    return Asserts.AsA<IShaderUniform<long>>(uniform);
  }

  private class Sampler2dShaderUniform(int location)
      : BShaderUniform(location), IShaderUniform<long> {
    private long value_;

    public void SetAndMarkDirty(in long value) {
      this.value_ = value;
      this.MarkDirty();
    }

    public void SetAndMaybeMarkDirty(in long value) {
      if (this.value_ == value) {
        return;
      }

      this.value_ = value;
      this.MarkDirty();
    }

    protected override void PassValueToProgram()
      => GL.Arb.UniformHandle(location, this.value_);
  }
}