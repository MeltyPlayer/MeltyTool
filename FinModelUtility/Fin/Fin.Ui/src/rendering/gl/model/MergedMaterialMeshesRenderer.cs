using fin.math;
using fin.model;
using fin.model.util;
using fin.shaders.glsl;
using fin.ui.rendering.gl.material;
using fin.image.util;


namespace fin.ui.rendering.gl.model;

public partial class ModelRenderer {
  private class MergedMaterialMeshesRenderer : IDynamicModelRenderer {
    private readonly bool dynamic_;
    private readonly IReadOnlyTextureTransformManager? textureTransformManager_;

    private IGlBufferManager? bufferManager_;
    private IDynamicGlBufferManager? dynamicBufferManager_;
    private IReadOnlyMesh selectedMesh_;

    private MergedMaterialMeshRenderer[] materialMeshRenderers_ = [];

    public MergedMaterialMeshesRenderer(
        IReadOnlyModel model,
        IReadOnlyTextureTransformManager? textureTransformManager = null,
        bool dynamic = false) {
      this.Model = model;
      this.textureTransformManager_ = textureTransformManager;
      this.dynamic_ = dynamic;

      SelectedMeshService.OnMeshSelected += selectedMesh
          => this.selectedMesh_ = selectedMesh;
    }

    // Generates buffer manager and model within the current GL context.
    public void GenerateModelIfNull() {
      if (this.bufferManager_ != null) {
        return;
      }

      var modelRequirements = ModelRequirements.FromModel(this.Model);

      if (!this.dynamic_) {
        this.bufferManager_
            = GlBufferManager.CreateStatic(this.Model, modelRequirements);
      } else {
        this.bufferManager_ = this.dynamicBufferManager_
            = GlBufferManager.CreateDynamic(this.Model, modelRequirements);
      }

      var allMaterialMeshRenderers = new List<MergedMaterialMeshRenderer>();

      // TODO: Optimize this with something like a "MinMap"?
      var meshQueue = new RenderPriorityOrderedSet<IReadOnlyMesh>();
      foreach (var mesh in this.Model.Skin.Meshes) {
        if (mesh.IsSubMesh) {
          continue;
        }

        meshQueue.Add(mesh, UInt32.MaxValue, false);
        foreach (var primitive in mesh.Primitives) {
          meshQueue.Add(mesh,
                        primitive.InversePriority,
                        (primitive.Material?.TransparencyType ??
                         TransparencyType.OPAQUE) ==
                        TransparencyType.TRANSPARENT);
        }
      }

      foreach (var mesh in meshQueue) {
        var materialMeshRenderer = new MergedMaterialMeshRenderer(
                                         this.Model,
                                         mesh,
                                         modelRequirements,
                                         this.bufferManager_,
                                         this.textureTransformManager_);
        materialMeshRenderer.GenerateModelIfNull();
        allMaterialMeshRenderers.Add(materialMeshRenderer);
      }

      this.materialMeshRenderers_ = allMaterialMeshRenderers.ToArray();
    }

    ~MergedMaterialMeshesRenderer() => this.ReleaseUnmanagedResources_();

    public void Dispose() {
      this.ReleaseUnmanagedResources_();
      GC.SuppressFinalize(this);
    }

    private void ReleaseUnmanagedResources_() {
      foreach (var meshRenderer in this.materialMeshRenderers_) {
        meshRenderer.Dispose();
      }

      this.bufferManager_?.Dispose();
    }

    public IReadOnlyModel Model { get; }

    public IReadOnlySet<IReadOnlyMesh>? HiddenMeshes { get; set; }

    public void UpdateBuffer() => this.dynamicBufferManager_?.UpdateBuffer();

    public void Render() {
      this.GenerateModelIfNull();

      foreach (var meshRenderer in this.materialMeshRenderers_) {
        meshRenderer.Render(this.selectedMesh_, this.HiddenMeshes);
      }
    }

    public IEnumerable<IGlMaterialShader> GetMaterialShaders(
        IReadOnlyMaterial material)
      => this.materialMeshRenderers_.SelectMany(
          p => p.GetMaterialShaders(material));
  }
}