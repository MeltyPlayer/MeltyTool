using fin.scene;

namespace fin.ui.rendering.gl.scene;

public class SceneRenderer : IRenderable, IDisposable {
  public SceneRenderer(ISceneInstance scene) {
      this.AreaRenderers
      = scene.Areas
                 .Select(area => new SceneAreaRenderer(area, scene.Lighting))
                 .ToArray();
    }

  ~SceneRenderer() => this.ReleaseUnmanagedResources_();

  public void Dispose() {
      this.ReleaseUnmanagedResources_();
      GC.SuppressFinalize(this);
    }

  private void ReleaseUnmanagedResources_() {
      foreach (var areaRenderer in this.AreaRenderers) {
        areaRenderer.Dispose();
      }
    }

  public IReadOnlyList<SceneAreaRenderer> AreaRenderers { get; }

  public void Render() {
      foreach (var areaRenderer in this.AreaRenderers) {
        areaRenderer.Render();
      }
    }
}