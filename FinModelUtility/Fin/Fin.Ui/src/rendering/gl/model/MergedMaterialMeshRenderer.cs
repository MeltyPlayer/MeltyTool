using fin.data.dictionaries;
using fin.math;
using fin.model;
using fin.model.util;
using fin.ui.rendering.gl.material;
using fin.image.util;
using fin.shaders.glsl;


namespace fin.ui.rendering.gl.model;

public partial class ModelRenderer {
  private class MergedMaterialMeshRenderer(
      IReadOnlyModel model,
      IReadOnlyMesh mesh,
      IModelRequirements modelRequirements,
      IGlBufferManager bufferManager,
      IReadOnlyTextureTransformManager? textureTransformManager = null)
      : IDisposable {
    private PrimitivesWithCommonMaterialRenderer[]? materialMeshRenderers_;

    private MergedMaterialMeshRenderer[] children_
        = mesh.SubMeshes.Select(subMesh => new MergedMaterialMeshRenderer(
                                    model,
                                    subMesh,
                                    modelRequirements,
                                    bufferManager,
                                    textureTransformManager))
              .ToArray();

    // Generates buffer manager and model within the current GL context.
    private void GenerateModelIfNull_() {
      if (this.materialMeshRenderers_ != null) {
        return;
      }

      var primitiveMerger = new PrimitiveMerger();
      var materialQueue = new RenderPriorityOrderedSet<IReadOnlyMaterial?>();
      var primitivesByMaterial
          = new ListDictionary<IReadOnlyMaterial?, IReadOnlyPrimitive>(
              new NullFriendlyDictionary<IReadOnlyMaterial?,
                  IList<IReadOnlyPrimitive>>());
      foreach (var primitive in mesh.Primitives) {
        primitivesByMaterial.Add(primitive.Material, primitive);
        materialQueue.Add(
            primitive.Material,
            primitive.InversePriority,
            (primitive.Material?.TransparencyType ??
             TransparencyType.OPAQUE) ==
            TransparencyType.TRANSPARENT);
      }

      var materialMeshRenderers
          = new List<PrimitivesWithCommonMaterialRenderer>();
      foreach (var material in materialQueue) {
        var primitives = primitivesByMaterial[material];
        if (!primitiveMerger.TryToMergePrimitives(
                primitives,
                out var mergedPrimitives)) {
          continue;
        }

        materialMeshRenderers.Add(new PrimitivesWithCommonMaterialRenderer(
                                      textureTransformManager,
                                      bufferManager,
                                      model,
                                      modelRequirements,
                                      material,
                                      mergedPrimitives));
      }

      this.materialMeshRenderers_ = materialMeshRenderers.ToArray();
    }

    ~MergedMaterialMeshRenderer() => this.ReleaseUnmanagedResources_();

    public void Dispose() {
      this.ReleaseUnmanagedResources_();
      GC.SuppressFinalize(this);
    }

    private void ReleaseUnmanagedResources_() {
      foreach (var materialMeshRenderer in this.materialMeshRenderers_ ?? []) {
        materialMeshRenderer.Dispose();
      }
    }

    public void Render(IReadOnlyMesh? selectedMesh,
                       IReadOnlySet<IReadOnlyMesh>? hiddenMeshes) {
      if (hiddenMeshes?.Contains(mesh) ?? false) {
        return;
      }
      
      this.GenerateModelIfNull_();

      var isSelected = selectedMesh == mesh;
      foreach (var materialMeshRenderer in this.materialMeshRenderers_!) {
        materialMeshRenderer.Render();

        if (isSelected) {
          GlUtil.RenderHighlight(materialMeshRenderer.Render);
        }
      }

      foreach (var child in this.children_) {
        child.Render(selectedMesh, hiddenMeshes);
      }
    }

    public IEnumerable<IGlMaterialShader> GetMaterialShaders(
        IReadOnlyMaterial material)
      => this.materialMeshRenderers_?
             .Where(r => r.Material == material)
             .Select(r => r.MaterialShader) ??
         [];
  }
}