using fin.data.dictionaries;
using fin.data.indexable;
using fin.image.util;
using fin.math;
using fin.model;
using fin.model.skin;
using fin.model.util;
using fin.shaders.glsl;
using fin.ui.rendering.gl.material;


namespace fin.ui.rendering.gl.model;

using VisibilityMeshMaterialTuple =
    (IReadOnlyMaterial? material, (IReadOnlyMesh mesh, bool isVisibilityAnimated
    )? meshTuple);

public partial class ModelRenderer {
  /// <summary>
  ///   Renderer for all primitives sharing a material in a single mesh.
  /// </summary>
  private sealed class MergedMaterialPrimitivesRenderer : IMaterialRenderer {
    private readonly IGlBufferRenderer bufferRenderer_;
    private bool isMaterialSelected_;
    private bool isMeshSelected_;
    private readonly bool renderOnlyWhenSelected_;

    public IReadOnlyMesh? Mesh { get; }
    public IReadOnlyMaterial? Material { get; }
    public required int MinPrimitiveIndex { get; init; }
    public required uint InversePriority { get; init; }
    public IGlMaterialShader GlMaterialShader { get; }

    public IReadOnlyMeshVisibilityDictionary? MeshVisibility { get; set; }

    public static MergedMaterialPrimitivesRenderer[] CreateFromPrimitives(
        IGlBufferManager bufferManager,
        IReadOnlyModel model,
        IReadOnlyTextureTransformManager textureTransformManager,
        IReadOnlyTextureFlipbookSwapManager textureFlipbookSwapManager,
        IModelRequirements modelRequirements,
        IReadOnlyMeshVisibilityDictionary? meshVisibility) {
      // Gathers up which meshes have visibility toggled on and off.
      var meshesWithVisibilityAnimations = new IndexableSet<IReadOnlyMesh>();
      foreach (var mesh in model.Skin.Meshes) {
        if (meshesWithVisibilityAnimations.Contains(mesh)) {
          continue;
        }

        foreach (var animation in model.AnimationManager.Animations) {
          if (animation.MeshTracks.ContainsKey(mesh)) {
            foreach (var visibilityMesh in mesh.SelfAndChildren()) {
              meshesWithVisibilityAnimations.Add(visibilityMesh);
            }
            break;
          }
        }
      }

      var primitiveMerger = new PrimitiveMerger();
      var mergedMaterialQueue =
          new RenderPriorityOrderedSet<VisibilityMeshMaterialTuple>();
      var unmergedMaterialQueue =
          new RenderPriorityOrderedSet<VisibilityMeshMaterialTuple>();
      var primitivesByMaterial
          = new ListDictionary<VisibilityMeshMaterialTuple, IReadOnlyPrimitive>(
              new NullFriendlyDictionary<VisibilityMeshMaterialTuple,
                  List<IReadOnlyPrimitive>>());

      foreach (var mesh in model.Skin.Meshes) {
        var isVisibilityAnimated
            = meshesWithVisibilityAnimations.Contains(mesh);

        foreach (var primitive in mesh.Primitives) {
          var isTransparent = (primitive.Material?.GetTransparencyType() ??
                               TransparencyType.OPAQUE) ==
                              TransparencyType.TRANSPARENT;

          if (!isVisibilityAnimated) {
            VisibilityMeshMaterialTuple visibilityMeshMaterialTuple =
                (primitive.Material, null);
            primitivesByMaterial.Add(visibilityMeshMaterialTuple, primitive);
            mergedMaterialQueue.Add(
                visibilityMeshMaterialTuple,
                primitive.Index,
                primitive.InversePriority,
                isTransparent);
          }

          // Need to always include this to support rendering meshes when
          // selected in the viewer.
          if (SceneTypeService.IsASingleModel) {
            var visibilityMeshMaterialTuple
                = (primitive.Material, (mesh, isVisibilityAnimated));
            primitivesByMaterial.Add(visibilityMeshMaterialTuple, primitive);
            unmergedMaterialQueue.Add(
                visibilityMeshMaterialTuple,
                primitive.Index,
                primitive.InversePriority,
                isTransparent);
          }
        }
      }

      var mergedPrimitives = new List<MergedPrimitive>();
      var materialTuples
          = new List<(bool isTransparent, int minPrimitiveIndex, uint
              inversePriority,
              VisibilityMeshMaterialTuple visibilityMeshMaterialTuple)>();
      var mergedPrimitiveByMaterial
          = new NullFriendlyDictionary<IReadOnlyMaterial?, MergedPrimitive>();

      foreach (var meshTuple in
               mergedMaterialQueue.Select(t => (t, true))
                                  .Concat(
                                      unmergedMaterialQueue
                                          .Select(t => (t, false)))) {
        var (t, isMerged) = meshTuple;
        var (isTransparent, minPrimitiveIndex, inversePriority,
            visibilityMeshMaterialTuple) = t;

        var materialPrimitives =
            primitivesByMaterial[visibilityMeshMaterialTuple];
        if (!primitiveMerger.TryToMergePrimitives(
                materialPrimitives,
                out var mergedPrimitive)) {
          continue;
        }

        var material = visibilityMeshMaterialTuple.material;
        if (isMerged) {
          mergedPrimitiveByMaterial[material] = mergedPrimitive;
        } else if (visibilityMeshMaterialTuple.meshTuple
                                              ?.isVisibilityAnimated is
                   false) {
          mergedPrimitive.Base = mergedPrimitiveByMaterial.TryGetValue(
              material,
              out var baseMergedPrimitive)
              ? baseMergedPrimitive
              : null;
        }

        mergedPrimitives.Add(mergedPrimitive);
        materialTuples.Add((isTransparent, minPrimitiveIndex, inversePriority,
                            visibilityMeshMaterialTuple));
      }

      var rendererByMergedPrimitive
          = new Dictionary<MergedPrimitive, MergedMaterialPrimitivesRenderer>();

      return bufferManager
             .CreateRenderers(mergedPrimitives)
             .Select((bufferRenderer, i) => {
               var (isTransparent, minPrimitiveIndex, inversePriority,
                       visibilityMeshMaterialTuple)
                   = materialTuples[i];

               var (material, visibilityMeshTuple) =
                   visibilityMeshMaterialTuple;

               var mergedPrimitive = mergedPrimitives[i];
               var renderer = new MergedMaterialPrimitivesRenderer(
                   textureTransformManager,
                   textureFlipbookSwapManager,
                   model,
                   modelRequirements,
                   visibilityMeshTuple,
                   mergedPrimitive.Base != null
                       ? rendererByMergedPrimitive[mergedPrimitive.Base]
                           .GlMaterialShader
                       : null,
                   material,
                   bufferRenderer) {
                   IsTransparent = isTransparent,
                   MinPrimitiveIndex = minPrimitiveIndex,
                   InversePriority = inversePriority,
                   MeshVisibility = meshVisibility
               };

               rendererByMergedPrimitive[mergedPrimitive] = renderer;

               return renderer;
             })
             .ToArray();
    }

    public MergedMaterialPrimitivesRenderer(
        IReadOnlyTextureTransformManager textureTransformManager,
        IReadOnlyTextureFlipbookSwapManager textureFlipbookSwapManager,
        IReadOnlyModel model,
        IModelRequirements modelRequirements,
        (IReadOnlyMesh mesh, bool isVisibilityAnimated)? meshTuple,
        IGlMaterialShader? existingShader,
        IReadOnlyMaterial? material,
        IGlBufferRenderer bufferRenderer) {
      this.Mesh = meshTuple?.mesh;
      this.renderOnlyWhenSelected_ = meshTuple?.isVisibilityAnimated is false;

      this.Material = material;

      this.GlMaterialShader
          = existingShader ??
            gl.material.GlMaterialShader.FromMaterial(model,
              modelRequirements,
              material,
              textureTransformManager,
              textureFlipbookSwapManager);

      this.bufferRenderer_ = bufferRenderer;

      SelectedMaterialsService.OnMaterialsSelected
          += selectedMaterials =>
              this.isMaterialSelected_ = this.Material != null &&
                                         (selectedMaterials?.Contains(
                                              this.Material) ??
                                          false);
      SelectedMeshService.OnMeshSelected += selectedMesh
          => this.isMeshSelected_
              = selectedMesh != null && this.Mesh == selectedMesh;
    }

    ~MergedMaterialPrimitivesRenderer()
      => this.ReleaseUnmanagedResources_();

    public void Dispose() {
      this.ReleaseUnmanagedResources_();
      GC.SuppressFinalize(this);
    }

    private void ReleaseUnmanagedResources_() {
      this.GlMaterialShader?.Dispose();
      this.bufferRenderer_?.Dispose();
    }

    public required bool IsTransparent { get; init; }
    public bool IsSelected => this.isMaterialSelected_ || this.isMeshSelected_;

    public void Render() {
      var isSelected = this.IsSelected;
      if (this.renderOnlyWhenSelected_ && !isSelected) {
        return;
      }

      var isVisible = this.Mesh == null ||
                      (this.MeshVisibility?[this.Mesh] ?? true);
      if (!isVisible) {
        return;
      }

      this.RenderImpl_();
    }

    private void RenderImpl_() {
      this.GlMaterialShader?.Use();

      if (this.Material != null) {
        GlUtil.SetBlendingSeparate(this.Material.ColorBlendEquation,
                                   this.Material.ColorSrcFactor,
                                   this.Material.ColorDstFactor,
                                   this.Material.AlphaBlendEquation,
                                   this.Material.AlphaSrcFactor,
                                   this.Material.AlphaDstFactor,
                                   this.Material.LogicOp);
      } else {
        GlUtil.ResetBlending();
      }

      GlUtil.SetCulling(this.Material?.CullingMode ?? CullingMode.SHOW_BOTH);
      GlUtil.SetDepth(
          this.Material?.DepthMode ?? DepthMode.READ_AND_WRITE,
          this.Material?.DepthCompareType ??
          DepthCompareType.LEqual);
      GlUtil.SetChannelUpdateMask(this.Material?.UpdateColorChannel ?? true,
                                  this.Material?.UpdateAlphaChannel ?? false);

      this.bufferRenderer_.Render();
    }
  }
}