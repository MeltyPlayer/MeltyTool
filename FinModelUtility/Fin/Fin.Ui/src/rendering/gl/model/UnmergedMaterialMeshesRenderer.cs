using fin.math;
using fin.model;
using fin.shaders.glsl;
using fin.ui.rendering.gl.material;

namespace fin.ui.rendering.gl.model;

public partial class ModelRenderer {
  private class UnmergedMaterialMeshesRenderer(
      IReadOnlyModel model,
      IReadOnlyTextureTransformManager? textureTransformManager = null,
      bool dynamic = false)
      : IDynamicModelRenderer {
    private IGlBufferManager? bufferManager_;
    private IDynamicGlBufferManager? dynamicBufferManager_;

    private readonly List<(IReadOnlyMesh, UnmergedMaterialMeshRenderer)>
        meshRenderers_ = new();

    // Generates buffer manager and model within the current GL context.
    public void GenerateModelIfNull() {
      if (this.bufferManager_ != null) {
        return;
      }

      var modelRequirements = ModelRequirements.FromModel(this.Model);

      if (!dynamic) {
        this.bufferManager_
            = GlBufferManager.CreateStatic(this.Model, modelRequirements);
      } else {
        this.bufferManager_ = this.dynamicBufferManager_
            = GlBufferManager.CreateDynamic(this.Model, modelRequirements);
      }

      foreach (var mesh in this.Model.Skin.Meshes) {
        this.meshRenderers_.Add(
            (mesh,
             new UnmergedMaterialMeshRenderer(model,
                                              mesh,
                                              modelRequirements,
                                              this.bufferManager_,
                                              textureTransformManager)));
      }
    }

    ~UnmergedMaterialMeshesRenderer() => this.ReleaseUnmanagedResources_();

    public void Dispose() {
      this.ReleaseUnmanagedResources_();
      GC.SuppressFinalize(this);
    }

    private void ReleaseUnmanagedResources_() {
      foreach (var (_, meshRenderer) in this.meshRenderers_) {
        meshRenderer.Dispose();
      }

      this.meshRenderers_.Clear();
      this.bufferManager_?.Dispose();
    }

    public IReadOnlyModel Model { get; } = model;

    public IReadOnlySet<IReadOnlyMesh>? HiddenMeshes { get; set; }

    public void UpdateBuffer() => this.dynamicBufferManager_?.UpdateBuffer();

    public void Render() {
      this.GenerateModelIfNull();

      foreach (var (mesh, meshRenderer) in
               this.meshRenderers_) {
        if (this.HiddenMeshes?.Contains(mesh) ?? false) {
          continue;
        }

        meshRenderer.Render();
      }
    }

    public IEnumerable<IGlMaterialShader> GetMaterialShaders(
        IReadOnlyMaterial material)
      => this.meshRenderers_.SelectMany(
          p => p.Item2.GetMaterialShaders(material));
  }
}