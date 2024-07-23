using fin.util.asserts;

using OpenTK.Graphics.OpenGL;


namespace fin.ui.rendering.gl;

public partial class GlShaderProgram {
  public IShaderUniform<int> GetUniformInt(string name) {
      if (!this.cachedUniforms_.TryGetValue(name, out var uniform)) {
        this.cachedUniforms_[name] = uniform =
            new IntShaderUniform(this.GetUniformLocation_(name));
      }

      return Asserts.AsA<IShaderUniform<int>>(uniform);
    }

  private class IntShaderUniform(int location)
      : BShaderUniform, IShaderUniform<int> {
    private int value_;

    public void SetAndMarkDirty(in int value) {
        this.value_ = value;
        this.MarkDirty();
      }

    public void SetAndMaybeMarkDirty(in int value) {
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