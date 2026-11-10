using fin.scene;

namespace fin.ui.rendering.gl.scene;

public sealed class SceneNodeRenderer : IRenderable {
  private readonly ISceneNodeInstance sceneNode_;
  private readonly SceneNodeRenderer[] childNodeRenderers_;

  private IReadOnlySceneNode? selectedNode_;

  public SceneNodeRenderer(ISceneNodeInstance sceneNode) {
    this.sceneNode_ = sceneNode;
    this.childNodeRenderers_
        = sceneNode
          .ChildNodes
          .Select(n => new SceneNodeRenderer(n))
          .ToArray();

    SelectedNodeService.OnNodeSelected += selectedNode
        => this.selectedNode_ = selectedNode;
  }

  ~SceneNodeRenderer() => this.ReleaseUnmanagedResources_();

  public void Dispose() {
    this.ReleaseUnmanagedResources_();
    GC.SuppressFinalize(this);
  }

  private void ReleaseUnmanagedResources_() {
    foreach (var child in this.ChildNodeRenderers) {
      child.Dispose();
    }
  }

  public IReadOnlyList<SceneNodeRenderer> ChildNodeRenderers
    => this.childNodeRenderers_;

  public void Render() {
    var isSelected = this.selectedNode_ == this.sceneNode_.Definition;

    this.RenderImpl_();

    if (isSelected) {
      GlUtil.RenderHighlight(this.RenderImpl_);
    }
  }

  private void RenderImpl_() {
    GlTransform.PushMatrix();
    GlTransform.MultMatrix(this.sceneNode_.Transform.LocalMatrix);

    foreach (var component in this.sceneNode_.Definition.Components) {
      if (component is ISceneNodeRenderComponent renderComponent) {
        renderComponent.Render(this.sceneNode_);
      }
    }

    foreach (var child in this.ChildNodeRenderers) {
      child.Render();
    }

    GlTransform.PopMatrix();
  }
}