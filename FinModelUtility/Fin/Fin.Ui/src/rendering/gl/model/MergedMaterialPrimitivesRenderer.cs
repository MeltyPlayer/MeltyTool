using fin.math;
using fin.model;
using fin.ui.rendering.gl.material;

namespace fin.ui.rendering.gl.model;

public class MergedMaterialPrimitivesRenderer : IDisposable {
  private readonly GlBufferManager.GlBufferRenderer bufferRenderer_;

  private readonly IReadOnlyMaterial? material_;

  private readonly IGlMaterialShader? materialShader_;

  public MergedMaterialPrimitivesRenderer(
      IReadOnlyBoneTransformManager? boneTransformManager,
      GlBufferManager bufferManager,
      IReadOnlyModel model,
      IReadOnlyMaterial? material,
      IReadOnlyLighting? lighting,
      MergedPrimitive mergedPrimitive) {
      this.material_ = material;

      this.materialShader_ = GlMaterialShader.FromMaterial(model,
        material,
        boneTransformManager,
        lighting);

      this.bufferRenderer_ = bufferManager.CreateRenderer(mergedPrimitive);
    }

  ~MergedMaterialPrimitivesRenderer() => ReleaseUnmanagedResources_();

  public void Dispose() {
      ReleaseUnmanagedResources_();
      GC.SuppressFinalize(this);
    }

  private void ReleaseUnmanagedResources_() {
      this.materialShader_?.Dispose();
      this.bufferRenderer_.Dispose();
    }

  public IMesh Mesh { get; }

  public bool UseLighting {
    get => this.materialShader_?.UseLighting ?? false;
    set {
        if (this.materialShader_ != null) {
          this.materialShader_.UseLighting = value;
        }
      }
  }

  public void Render() {
      this.materialShader_?.Use();

      if (this.material_ is IReadOnlyFixedFunctionMaterial
          fixedFunctionMaterial) {
        GlUtil.SetBlendingSeparate(
            fixedFunctionMaterial.ColorBlendEquation,
            fixedFunctionMaterial.ColorSrcFactor,
            fixedFunctionMaterial.ColorDstFactor,
            fixedFunctionMaterial.AlphaBlendEquation,
            fixedFunctionMaterial.AlphaSrcFactor,
            fixedFunctionMaterial.AlphaDstFactor,
            fixedFunctionMaterial.LogicOp);
      } else {
        GlUtil.ResetBlending();
      }

      GlUtil.SetCulling(this.material_?.CullingMode ?? CullingMode.SHOW_BOTH);
      GlUtil.SetDepth(
          this.material_?.DepthMode ?? DepthMode.USE_DEPTH_BUFFER,
          this.material_?.DepthCompareType ??
          DepthCompareType.LEqual);

      this.bufferRenderer_.Render();
    }
}