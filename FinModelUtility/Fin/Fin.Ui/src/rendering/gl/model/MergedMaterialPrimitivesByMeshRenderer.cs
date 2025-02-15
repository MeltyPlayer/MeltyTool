using fin.math;
using fin.model;
using fin.shaders.glsl;
using fin.ui.rendering.gl.material;

namespace fin.ui.rendering.gl.model;

public partial class ModelRendererV2 {
  private class MergedMaterialPrimitivesByMeshRenderer : IDisposable {
    private readonly IGlBufferRenderer bufferRenderer_;
    private bool isSelected_;

    public IReadOnlyMaterial? Material { get; }
    public IGlMaterialShader MaterialShader { get; }

    public MergedMaterialPrimitivesByMeshRenderer(
        IReadOnlyTextureTransformManager? textureTransformManager,
        IGlBufferManager bufferManager,
        IReadOnlyModel model,
        IModelRequirements modelRequirements,
        IReadOnlyMaterial? material,
        MergedPrimitive mergedPrimitive) {
      this.Material = material;

      this.MaterialShader = GlMaterialShader.FromMaterial(model,
        modelRequirements,
        material,
        textureTransformManager);

      this.bufferRenderer_ = bufferManager.CreateRenderer(mergedPrimitive);

      SelectedMaterialsService.OnMaterialsSelected
          += selectedMaterials =>
              this.isSelected_ = this.Material != null &&
                                 (selectedMaterials?.Contains(this.Material) ??
                                  false);
    }

    ~MergedMaterialPrimitivesByMeshRenderer()
      => this.ReleaseUnmanagedResources_();

    public void Dispose() {
      this.ReleaseUnmanagedResources_();
      GC.SuppressFinalize(this);
    }

    private void ReleaseUnmanagedResources_() {
      this.MaterialShader?.Dispose();
      this.bufferRenderer_.Dispose();
    }

    public bool UseLighting {
      get => this.MaterialShader?.UseLighting ?? false;
      set {
        if (this.MaterialShader != null) {
          this.MaterialShader.UseLighting = value;
        }
      }
    }

    public void Render() {
      this.RenderImpl_();

      if (this.isSelected_) {
        GlUtil.RenderHighlight(this.RenderImpl_);
      }
    }

    private void RenderImpl_() {
      this.MaterialShader?.Use();

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
                                  this.Material?.UpdateAlphaChannel ?? true);

      this.bufferRenderer_.Render();
    }
  }
}