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

  private class IntShaderUniform : BShaderUniform, IShaderUniform<int> {
    private readonly int location_;
    private int value_;

    public IntShaderUniform(int location) {
        this.location_ = location;
      }

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
      => GL.Uniform1(this.location_, this.value_);
  }
}