namespace fin.ui.rendering.gl;

public partial class GlShaderProgram {
  private Dictionary<string, BShaderUniform> cachedUniformsByName_ = new();

  private int GetUniformLocation_(string name)
    => this.cachedShaderProgram_.GetUniformLocation(name);

  private abstract class BShaderUniform(int location) : IShaderUniform {
    public bool IsValid => location != UNDEFINED_ID;

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