using OpenTK.Graphics.OpenGL;

namespace fin.ui.rendering.gl;

public sealed class GlDisplayList(Action compile) : IDisposable {
  private readonly int displayListId_ = GL.GenLists(1);
  private bool valid_ = false;

  ~GlDisplayList() => this.ReleaseUnmanagedResources_();

  public void Dispose() {
      this.ReleaseUnmanagedResources_();
      GC.SuppressFinalize(this);
    }

  private void ReleaseUnmanagedResources_() {
      GL.DeleteLists(this.displayListId_, 1);
    }

  public void Invalidate() {
      this.valid_ = false;
    }

  public void CompileOrRender() {
      if (this.valid_) {
        GL.CallList(this.displayListId_);
        return;
      }

      this.valid_ = true;

      GL.NewList(this.displayListId_, ListMode.CompileAndExecute);
      compile();
      GL.EndList();
    }
}