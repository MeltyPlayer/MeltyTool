using fin.math;
using fin.model;

namespace fin.ui.rendering.gl.model;

public partial class ModelRendererV2 {
  private class UnmergedMaterialMeshesRenderer(
      IReadOnlyModel model,
      IReadOnlyTextureTransformManager? textureTransformManager = null,
      bool dynamic = false)
      : IDynamicModelRenderer {
    private IGlBufferManager? bufferManager_;
    private IDynamicGlBufferManager? dynamicBufferManager_;

    private readonly
        List<(IReadOnlyMesh, List<MergedMaterialPrimitivesByMeshRenderer>)>
        materialMeshRenderers_ = new();

    // Generates buffer manager and model within the current GL context.
    private void GenerateModelIfNull_() {
      if (this.bufferManager_ != null) {
        return;
      }

      if (!dynamic) {
        this.bufferManager_ = GlBufferManager.CreateStatic(this.Model);
      } else {
        this.bufferManager_ = this.dynamicBufferManager_
            = GlBufferManager.CreateDynamic(this.Model);
      }

      List<MergedMaterialPrimitivesByMeshRenderer> currentList = null;
      var primitiveMerger = new PrimitiveMerger();
      Action<IReadOnlyMesh, IReadOnlyMaterial?, IEnumerable<IReadOnlyPrimitive>>
          addPrimitivesRenderer =
              (mesh, material, primitives) => {
                if (primitiveMerger.TryToMergePrimitives(
                        primitives
                            .OrderBy(primitive => primitive.InversePriority)
                            .ToList(),
                        out var mergedPrimitive)) {

                  currentList.Add(new MergedMaterialPrimitivesByMeshRenderer(
                                      textureTransformManager,
                                      this.bufferManager_,
                                      this.Model,
                                      material,
                                      mergedPrimitive));
                }
              };

      foreach (var mesh in this.Model.Skin.Meshes) {
        currentList = new List<MergedMaterialPrimitivesByMeshRenderer>();
        this.materialMeshRenderers_.Add((mesh, currentList));

        IReadOnlyMaterial? currentMaterial = null;
        var currentPrimitives = new LinkedList<IReadOnlyPrimitive>();

        foreach (var primitive in mesh.Primitives) {
          var material = primitive.Material;

          if (currentMaterial != material) {
            if (currentPrimitives.Count > 0) {
              addPrimitivesRenderer(mesh, currentMaterial, currentPrimitives);
              currentPrimitives.Clear();
            }

            currentMaterial = material;
          }

          currentPrimitives.AddLast(primitive);
        }

        if (currentPrimitives.Count > 0) {
          addPrimitivesRenderer(mesh, currentMaterial, currentPrimitives);
        }
      }
    }

    ~UnmergedMaterialMeshesRenderer() => this.ReleaseUnmanagedResources_();

    public void Dispose() {
      this.ReleaseUnmanagedResources_();
      GC.SuppressFinalize(this);
    }

    private void ReleaseUnmanagedResources_() {
      foreach (var (_, materialMeshRenderers) in this.materialMeshRenderers_) {
        foreach (var materialMeshRenderer in materialMeshRenderers) {
          materialMeshRenderer.Dispose();
        }
      }

      this.materialMeshRenderers_.Clear();
      this.bufferManager_?.Dispose();
    }

    public IReadOnlyModel Model { get; } = model;

    public IReadOnlySet<IReadOnlyMesh>? HiddenMeshes { get; set; }

    public void UpdateBuffer() => this.dynamicBufferManager_?.UpdateBuffer();

    public void Render() {
      this.GenerateModelIfNull_();

      foreach (var (mesh, materialMeshRenderers) in
               this.materialMeshRenderers_) {
        if (this.HiddenMeshes?.Contains(mesh) ?? false) {
          continue;
        }

        foreach (var materialMeshRenderer in materialMeshRenderers) {
          materialMeshRenderer.Render();
        }
      }
    }
  }
}