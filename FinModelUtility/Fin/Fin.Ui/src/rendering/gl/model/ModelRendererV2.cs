using fin.math;
using fin.model;


namespace fin.ui.rendering.gl.model {
  /// <summary>
  ///   A renderer for a Fin model.
  ///
  ///   NOTE: This will only be valid in the GL context this was first rendered in!
  /// </summary>
  public class ModelRendererV2 : IModelRenderer {
    // TODO: Require passing in a GL context in the constructor.

    private readonly IModelRenderer impl_;

    public ModelRendererV2(
        IReadOnlyModel model,
        IReadOnlyLighting? lighting,
        IBoneTransformManager? boneTransformManager = null) {
      this.impl_ = (model.Skin.AllowMaterialRendererMerging)
          ? new MergedMaterialMeshesRenderer(
              model,
              lighting,
              boneTransformManager)
          : new UnmergedMaterialMeshesRenderer(model,
                                               lighting,
                                               boneTransformManager);
    }

    ~ModelRendererV2() => ReleaseUnmanagedResources_();

    public void Dispose() {
      ReleaseUnmanagedResources_();
      GC.SuppressFinalize(this);
    }

    private void ReleaseUnmanagedResources_() => this.impl_.Dispose();

    public IReadOnlyModel Model => this.impl_.Model;
    public ISet<IReadOnlyMesh> HiddenMeshes => this.impl_.HiddenMeshes;

    public bool UseLighting {
      get => this.impl_.UseLighting;
      set => this.impl_.UseLighting = value;
    }

    public void Render() => this.impl_.Render();
  }
}