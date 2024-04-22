using fin.data.dictionaries;
using fin.math;
using fin.model;


namespace fin.ui.rendering.gl.model {
  public class MergedMaterialMeshesRenderer : IModelRenderer {
    private GlBufferManager? bufferManager_;
    private readonly ILighting? lighting_;
    private readonly IBoneTransformManager? boneTransformManager_;

    private (IMesh, MergedMaterialPrimitivesRenderer[])[]
        materialMeshRenderers_;

    public MergedMaterialMeshesRenderer(
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

      var allMaterialMeshRenderers =
          new List<(IMesh, MergedMaterialPrimitivesRenderer[])>();

      var primitiveMerger = new PrimitiveMerger();
      foreach (var mesh in this.Model.Skin.Meshes) {
        var primitivesByMaterial
            = new ListDictionary<IReadOnlyMaterial, IReadOnlyPrimitive>();
        var prioritiesByMaterial = new SetDictionary<IReadOnlyMaterial, uint>();
        foreach (var primitive in mesh.Primitives) {
          primitivesByMaterial.Add(primitive.Material, primitive);
          prioritiesByMaterial.Add(primitive.Material,
                                   primitive.InversePriority);
        }

        var orderedMaterials =
            prioritiesByMaterial.OrderBy(pair => pair.Value.Order().First())
                                .Select(pair => pair.Key)
                                .ToArray();

        var materialMeshRenderers =
            new ListDictionary<IMesh, MergedMaterialPrimitivesRenderer>();
        foreach (var material in orderedMaterials) {
          var primitives = primitivesByMaterial[material];
          if (!primitiveMerger.TryToMergePrimitives(
                  primitives,
                  out var mergedPrimitives)) {
            continue;
          }

          materialMeshRenderers.Add(
              mesh,
              new MergedMaterialPrimitivesRenderer(
                  this.boneTransformManager_,
                  this.bufferManager_,
                  this.Model,
                  material,
                  this.lighting_,
                  mergedPrimitives) {
                  UseLighting = UseLighting
              });
        }

        allMaterialMeshRenderers.AddRange(
            materialMeshRenderers.Select(
                tuple => (tuple.Key, tuple.Value.ToArray())));
      }

      this.materialMeshRenderers_ = allMaterialMeshRenderers.ToArray();
    }

    ~MergedMaterialMeshesRenderer() => ReleaseUnmanagedResources_();

    public void Dispose() {
      ReleaseUnmanagedResources_();
      GC.SuppressFinalize(this);
    }

    private void ReleaseUnmanagedResources_() {
      foreach (var (_, materialMeshRenderers) in this.materialMeshRenderers_
                   .AsSpan()) {
        foreach (var materialMeshRenderer in materialMeshRenderers.AsSpan()) {
          materialMeshRenderer.Dispose();
        }
      }

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
                 this.materialMeshRenderers_.AsSpan()) {
          foreach (var materialMeshRenderer in materialMeshRenderers.AsSpan()) {
            materialMeshRenderer.UseLighting = value;
          }
        }
      }
    }

    public void Render() {
      this.GenerateModelIfNull_();

      foreach (var (mesh, materialMeshRenderers) in
               this.materialMeshRenderers_.AsSpan()) {
        if (this.HiddenMeshes?.Contains(mesh) ?? false) {
          continue;
        }

        foreach (var materialMeshRenderer in materialMeshRenderers.AsSpan()) {
          materialMeshRenderer.Render();
        }
      }
    }
  }
}