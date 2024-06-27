using fin.data.dictionaries;
using fin.math;
using fin.model;
using fin.model.util;
using fin.util.image;


namespace fin.ui.rendering.gl.model;

public class MergedMaterialMeshesRenderer : IModelRenderer {
  private GlBufferManager? bufferManager_;
  private readonly IReadOnlyLighting? lighting_;
  private readonly IReadOnlyBoneTransformManager? boneTransformManager_;

  private (IReadOnlyMesh, MergedMaterialPrimitivesRenderer[])[]
      materialMeshRenderers_ = [];

  public MergedMaterialMeshesRenderer(
      IReadOnlyModel model,
      IReadOnlyLighting? lighting,
      IReadOnlyBoneTransformManager? boneTransformManager = null) {
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
        new List<(IReadOnlyMesh, MergedMaterialPrimitivesRenderer[])>();

    // TODO: Optimize this with something like a "MinMap"?
    var meshQueue = new RenderPriorityOrderedSet<IReadOnlyMesh>();
    foreach (var mesh in this.Model.Skin.Meshes) {
      foreach (var primitive in mesh.Primitives) {
        meshQueue.Add(mesh,
                      primitive.InversePriority,
                      (primitive.Material?.TransparencyType ??
                       TransparencyType.OPAQUE) ==
                      TransparencyType.TRANSPARENT);
      }
    }

    var primitiveMerger = new PrimitiveMerger();
    foreach (var mesh in meshQueue) {
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

      var materialMeshRenderers =
          new ListDictionary<IReadOnlyMesh,
              MergedMaterialPrimitivesRenderer>();
      foreach (var material in materialQueue) {
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
                UseLighting = this.UseLighting
            });
      }

      allMaterialMeshRenderers.AddRange(
          materialMeshRenderers
              .GetPairs()
              .Select(tuple => (tuple.key, tuple.value.ToArray())));
    }

    this.materialMeshRenderers_ = allMaterialMeshRenderers.ToArray();
  }

  ~MergedMaterialMeshesRenderer() => ReleaseUnmanagedResources_();

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

    this.bufferManager_?.Dispose();
  }

  public IReadOnlyModel Model { get; }

  public ISet<IReadOnlyMesh> HiddenMeshes { get; }
    = new HashSet<IReadOnlyMesh>();

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