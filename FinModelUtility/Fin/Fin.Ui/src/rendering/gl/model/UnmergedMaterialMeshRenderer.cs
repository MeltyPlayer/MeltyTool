using fin.math;
using fin.model;
using fin.shaders.glsl;
using fin.ui.rendering.gl.material;

namespace fin.ui.rendering.gl.model;

public partial class ModelRenderer {
  private class UnmergedMaterialMeshRenderer(
      IReadOnlyModel model,
      IReadOnlyMesh mesh,
      IModelRequirements modelRequirements,
      IGlBufferManager bufferManager,
      IReadOnlyTextureTransformManager? textureTransformManager = null)
      : IDisposable {
    private List<PrimitivesWithCommonMaterialRenderer>? materialMeshRenderers_;

    // Generates buffer manager and model within the current GL context.
    public void GenerateModelIfNull() {
      if (this.materialMeshRenderers_ != null) {
        return;
      }

      this.materialMeshRenderers_
          = new List<PrimitivesWithCommonMaterialRenderer>();

      var primitiveMerger = new PrimitiveMerger();
      Action<IReadOnlyMaterial?, IEnumerable<IReadOnlyPrimitive>>
          addPrimitivesRenderer =
              (material, primitives) => {
                if (primitiveMerger.TryToMergePrimitives(
                        primitives
                            .OrderBy(primitive => primitive.InversePriority)
                            .ToList(),
                        out var mergedPrimitive)) {
                  this.materialMeshRenderers_.Add(
                      new PrimitivesWithCommonMaterialRenderer(
                          textureTransformManager,
                          bufferManager,
                          model,
                          modelRequirements,
                          material,
                          mergedPrimitive));
                }
              };

      IReadOnlyMaterial? currentMaterial = null;
      var currentPrimitives = new LinkedList<IReadOnlyPrimitive>();

      foreach (var primitive in mesh.Primitives) {
        var material = primitive.Material;

        if (currentMaterial != material) {
          if (currentPrimitives.Count > 0) {
            addPrimitivesRenderer(currentMaterial, currentPrimitives);
            currentPrimitives.Clear();
          }

          currentMaterial = material;
        }

        currentPrimitives.AddLast(primitive);
      }

      if (currentPrimitives.Count > 0) {
        addPrimitivesRenderer(currentMaterial, currentPrimitives);
      }
    }

    ~UnmergedMaterialMeshRenderer() => this.ReleaseUnmanagedResources_();

    public void Dispose() {
      this.ReleaseUnmanagedResources_();
      GC.SuppressFinalize(this);
    }

    private void ReleaseUnmanagedResources_() {
      foreach (var materialMeshRenderer in this.materialMeshRenderers_ ?? []) {
        materialMeshRenderer.Dispose();
      }

      this.materialMeshRenderers_?.Clear();
    }

    public void Render() {
      this.GenerateModelIfNull();

      foreach (var materialMeshRenderer in this.materialMeshRenderers_!) {
        materialMeshRenderer.Render();
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