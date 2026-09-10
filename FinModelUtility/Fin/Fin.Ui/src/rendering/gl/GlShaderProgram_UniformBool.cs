using fin.util.asserts;

using OpenTK.Graphics.OpenGL4;


namespace fin.ui.rendering.gl;

public partial class GlShaderProgram {
  public IShaderUniform<bool> GetUniformBool(string name) {
    if (!this.cachedUniformsByName_.TryGetValue(name, out var uniform)) {
      this.cachedUniformsByName_[name] = uniform =
          new BoolShaderUniform(this.GetUniformLocation_(name));
    }

    return Asserts.AsA<IShaderUniform<bool>>(uniform);
  }

  private class BoolShaderUniform(int location)
      : BShaderUniform(location), IShaderUniform<bool> {
    private bool value_;

    public void SetAndMarkDirty(in bool value) {
      this.value_ = value;
      this.MarkDirty();
    }

    public void SetAndMaybeMarkDirty(in bool value) {
      if (this.value_ == value) {
        return;
      }

      this.value_ = value;
      this.MarkDirty();
    }

    protected override void PassValueToProgram()
      => GL.Uniform1(location, this.value_ ? 1 : 0);
  }
}