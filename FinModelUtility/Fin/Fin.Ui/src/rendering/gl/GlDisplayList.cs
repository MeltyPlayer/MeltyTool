using OpenTK.Graphics.OpenGL;

namespace fin.ui.rendering.gl;

public class GlDisplayList : IDisposable {
  private readonly int displayListId_;
  private bool valid_ = false;
  private Action compile_;

  public GlDisplayList(Action compile) {
      this.displayListId_ = GL.GenLists(1);
      this.compile_ = compile;
    }

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
      this.compile_();
      GL.EndList();
    }
}