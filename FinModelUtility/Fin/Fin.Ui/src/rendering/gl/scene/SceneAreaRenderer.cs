using fin.model;
using fin.scene;

namespace fin.ui.rendering.gl.scene;

public class SceneAreaRenderer : IRenderable, IDisposable {
  private readonly SceneObjectRenderer[] objectRenderers_;

  public SceneAreaRenderer(ISceneAreaInstance sceneArea,
                           IReadOnlyLighting? lighting) {
    var customSkybox = sceneArea.CustomSkyboxObject;
    this.CustomSkyboxRenderer = customSkybox != null
        ? new SceneObjectRenderer(customSkybox, lighting)
        : null;

    this.objectRenderers_
        = sceneArea
          .Objects
          .Where(obj => obj != customSkybox)
          .Select(obj => new SceneObjectRenderer(obj, lighting))
          .ToArray();
  }

  ~SceneAreaRenderer() => this.ReleaseUnmanagedResources_();

  public void Dispose() {
    this.ReleaseUnmanagedResources_();
    GC.SuppressFinalize(this);
  }

  private void ReleaseUnmanagedResources_() {
    this.CustomSkyboxRenderer?.Dispose();
    foreach (var objRenderer in this.ObjectRenderers) {
      objRenderer.Dispose();
    }
  }

  public IReadOnlyList<SceneObjectRenderer> ObjectRenderers
    => this.objectRenderers_;

  public SceneObjectRenderer? CustomSkyboxRenderer { get; }

  public void Render() {
    foreach (var objRenderer in this.objectRenderers_) {
      objRenderer.Render();
    }
  }
}