using fin.data.dictionaries;
using fin.math;
using fin.model;

namespace fin.ui.rendering.gl.model {
  public class UnmergedMaterialMeshesRenderer : IModelRenderer {
    private GlBufferManager? bufferManager_;
    private readonly ILighting? lighting_;
    private readonly IBoneTransformManager? boneTransformManager_;

    private readonly ListDictionary<IMesh, MergedMaterialPrimitivesRenderer>
        materialMeshRenderers_ = new();

    public UnmergedMaterialMeshesRenderer(
        IReadOnlyModel model,
        ILighting? lighting,
        IBoneTransformManager? boneTransformManager = null) {
      this.Model = model;
      this.lighting_ = lighting;
      this.boneTransformManager_ = boneTransformManager;
    }

    // Generates buffer manager and model within the current GL context.
    private void GenerateModelIfNull_() {
      if (this.bufferManager_ != null) {
        return;
      }

      this.bufferManager_ = new GlBufferManager(this.Model);

      var primitiveMerger = new PrimitiveMerger();
      Action<IMesh, IMaterial?, IEnumerable<IReadOnlyPrimitive>>
          addPrimitivesRenderer =
              (mesh, material, primitives) => {
                if (primitiveMerger.TryToMergePrimitives(
                        primitives
                            .OrderBy(primitive => primitive.InversePriority)
                            .ToList(),
                        out var mergedPrimitive)) {
                  this.materialMeshRenderers_.Add(
                      mesh,
                      new MergedMaterialPrimitivesRenderer(
                          this.boneTransformManager_,
                          this.bufferManager_,
                          this.Model,
                          material,
                          this.lighting_,
                          mergedPrimitive) {
                          UseLighting = UseLighting
                      });
                }
              };

      foreach (var mesh in this.Model.Skin.Meshes) {
        IMaterial? currentMaterial = null;
        var currentPrimitives = new LinkedList<IPrimitive>();

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

    ~UnmergedMaterialMeshesRenderer() => ReleaseUnmanagedResources_();

    public void Dispose() {
      ReleaseUnmanagedResources_();
      GC.SuppressFinalize(this);
    }

    private void ReleaseUnmanagedResources_() {
      foreach (var (_, materialMeshRenderers) in this.materialMeshRenderers_) {
        foreach (var materialMeshRenderer in materialMeshRenderers) {
          materialMeshRenderer.Dispose();
        }
      }

      materialMeshRenderers_.Clear();
      this.bufferManager_?.Dispose();
    }

    public IReadOnlyModel Model { get; }
    public ISet<IMesh> HiddenMeshes { get; } = new HashSet<IMesh>();

    private bool useLighting_ = false;

    public bool UseLighting {
      get => this.useLighting_;
      set {
        this.useLighting_ = value;
        foreach (var (_, materialMeshRenderers) in
                 this.materialMeshRenderers_) {
          foreach (var materialMeshRenderer in materialMeshRenderers) {
            materialMeshRenderer.UseLighting = value;
          }
        }
      }
    }

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