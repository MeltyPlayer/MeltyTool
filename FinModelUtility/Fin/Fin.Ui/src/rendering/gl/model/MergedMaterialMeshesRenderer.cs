using fin.math;
using fin.model;
using fin.model.skin;
using fin.shaders.glsl;
using fin.ui.rendering.gl.material;


namespace fin.ui.rendering.gl.model;

public partial class ModelRenderer {
  /// <summary>
  ///   Model renderer that merges primitives by material.
  /// </summary>
  private sealed class MergedMaterialMeshesRenderer : IDynamicModelRenderer {
    private readonly bool dynamic_;
    private readonly IReadOnlyTextureTransformManager textureTransformManager_;
    private readonly IReadOnlyTextureSwapManager textureSwapManager_;

    private IGlBufferManager? bufferManager_;
    private IDynamicGlBufferManager? dynamicBufferManager_;

    private MergedMaterialPrimitivesRenderer[] materialRenderers_ = [];

    public int VaoId => this.bufferManager_?.VaoId ?? 0;

    public MergedMaterialMeshesRenderer(
        IReadOnlyModel model,
        IReadOnlyTextureTransformManager textureTransformManager,
        IReadOnlyTextureSwapManager? textureSwapManager,
        bool dynamic = false) {
      this.Model = model;
      this.textureTransformManager_ = textureTransformManager;
      this.textureSwapManager_ = textureSwapManager ??
                                         new TextureSwapManager(
                                             model.MaterialManager.Textures);
      this.dynamic_ = dynamic;
    }

    // Generates buffer manager and model within the current GL context.
    public void GenerateModelIfNull() {
      if (this.bufferManager_ != null) {
        return;
      }

      this.textureSwapManager_.GenerateGlTexturesIfNull();

      var modelRequirements = ModelRequirements.FromModel(this.Model);

      if (!this.dynamic_) {
        this.bufferManager_
            = GlBufferManager.CreateStatic(this.Model, modelRequirements);
      } else {
        this.bufferManager_ = this.dynamicBufferManager_
            = GlBufferManager.CreateDynamic(this.Model, modelRequirements);
      }

      this.materialRenderers_
          = MergedMaterialPrimitivesRenderer.CreateFromPrimitives(
              this.bufferManager_,
              this.Model,
              this.textureTransformManager_,
              this.textureSwapManager_,
              modelRequirements,
              this.MeshVisibility);
    }

    ~MergedMaterialMeshesRenderer() => this.ReleaseUnmanagedResources_();

    public void Dispose() {
      this.ReleaseUnmanagedResources_();
      GC.SuppressFinalize(this);
    }

    private void ReleaseUnmanagedResources_() {
      foreach (var materialRenderer in this.materialRenderers_) {
        materialRenderer.Dispose();
      }

      this.bufferManager_?.Dispose();
    }

    public IReadOnlyModel Model { get; }

    public IReadOnlyMeshVisibilityDictionary? MeshVisibility { get; set; }

    public IEnumerable<IMaterialRenderer> MaterialRenderers
      => this.materialRenderers_;

    public void UpdateBuffer() => this.dynamicBufferManager_?.UpdateBuffer();

    public void UpdateMatricesUbo() { }
    public void BindMatricesUbo() { }

    public void Render() {
      this.GenerateModelIfNull();

      foreach (var meshRenderer in this.materialRenderers_) {
        meshRenderer.Render();
      }
    }

    public IEnumerable<IGlMaterialShader> GetMaterialShaders(
        IReadOnlyMaterial material)
      => this.materialRenderers_.Where(p => p.Material == material)
             .Select(p => p.GlMaterialShader)
             .Distinct();
  }
}