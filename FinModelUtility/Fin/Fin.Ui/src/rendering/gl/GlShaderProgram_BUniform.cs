namespace fin.ui.rendering.gl;

public partial class GlShaderProgram {
  private Dictionary<string, BShaderUniform> cachedUniforms_ = new();

  private int GetUniformLocation_(string name)
    => this.cachedShaderProgram_.GetUniformLocation(name);

  private abstract class BShaderUniform {
    protected bool IsDirty { get; private set; }

    public void PassValueToProgramIfDirty() {
        if (this.IsDirty) {
          this.IsDirty = false;
          this.PassValueToProgram();
        }
      }

    protected void MarkDirty() => this.IsDirty = true;
    protected abstract void PassValueToProgram();
  }
}