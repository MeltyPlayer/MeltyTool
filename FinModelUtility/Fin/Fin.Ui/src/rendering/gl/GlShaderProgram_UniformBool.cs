using fin.util.asserts;

using OpenTK.Graphics.ES30;


namespace fin.ui.rendering.gl;

public partial class GlShaderProgram {
  public IShaderUniform<bool> GetUniformBool(string name) {
    if (!this.cachedUniforms_.TryGetValue(name, out var uniform)) {
      this.cachedUniforms_[name] = uniform =
          new BoolShaderUniform(this.GetUniformLocation_(name));
    }

    return Asserts.AsA<IShaderUniform<bool>>(uniform);
  }

  private class BoolShaderUniform(int location)
      : BShaderUniform, IShaderUniform<bool> {
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