using fin.math.matrix.four;
using fin.model;
using fin.scene;

namespace fin.ui.rendering.gl.scene;

public class SceneObjectRenderer : IRenderable, IDisposable {
  private readonly ISceneObjectInstance sceneObject_;
  private readonly SceneModelRenderer[] modelRenderers_;

  public SceneObjectRenderer(ISceneObjectInstance sceneObject,
                             IReadOnlyLighting? lighting) {
    this.sceneObject_ = sceneObject;
    this.modelRenderers_
        = sceneObject
          .Models
          .Select(model => new SceneModelRenderer(model, lighting))
          .ToArray();
  }

  ~SceneObjectRenderer() => this.ReleaseUnmanagedResources_();

  public void Dispose() {
    this.ReleaseUnmanagedResources_();
    GC.SuppressFinalize(this);
  }

  private void ReleaseUnmanagedResources_() {
    foreach (var modelRenderer in this.ModelRenderers) {
      modelRenderer.Dispose();
    }
  }

  public IReadOnlyList<SceneModelRenderer> ModelRenderers
    => this.modelRenderers_;

  public void Render() {
    GlTransform.PushMatrix();

    GlTransform.MultMatrix(
        SystemMatrix4x4Util.FromTrs(this.sceneObject_.Position,
                                    this.sceneObject_.Rotation,
                                    this.sceneObject_.Scale));

    foreach (var model in this.modelRenderers_) {
      model.Render();
    }

    foreach (var component in this.sceneObject_.Definition.Components) {
      if (component is ISceneNodeRenderComponent renderComponent) {
        renderComponent.Render(this.sceneObject_);
      }
    }

    GlTransform.PopMatrix();
  }
}