using System.Numerics;

using fin.math;
using fin.model;
using fin.model.util;
using fin.ui.rendering.gl.material;
using fin.ui.rendering.gl.ubo;


namespace fin.ui.rendering.gl.model;

/// <summary>
///   A renderer for a Fin model.
///
///   NOTE: This will only be valid in the GL context this was first rendered in!
/// </summary>
public partial class ModelRenderer(
    IReadOnlyModel model,
    IReadOnlyLighting? lighting = null,
    IReadOnlyBoneTransformManager? boneTransformManager = null,
    IReadOnlyTextureTransformManager? textureTransformManager = null,
    bool dynamic = false)
    : IDynamicModelRenderer {
  private readonly IDynamicModelRenderer impl_ = (model.Skin.AllowMaterialRendererMerging)
      ? new MergedMaterialMeshesRenderer(model,
                                         textureTransformManager,
                                         dynamic)
      : new UnmergedMaterialMeshesRenderer(
          model,
          textureTransformManager,
          dynamic);

  public static IModelRenderer CreateStatic(
      IReadOnlyModel model,
      IReadOnlyLighting? lighting = null,
      IReadOnlyBoneTransformManager? boneTransformManager = null,
      IReadOnlyTextureTransformManager? textureTransformManager = null)
    => new ModelRenderer(model,
                           lighting,
                           boneTransformManager,
                           textureTransformManager);

  public static IDynamicModelRenderer CreateDynamic(
      IReadOnlyModel model,
      IReadOnlyLighting? lighting = null,
      IReadOnlyBoneTransformManager? boneTransformManager = null,
      IReadOnlyTextureTransformManager? textureTransformManager = null)
    => new ModelRenderer(model,
                           lighting,
                           boneTransformManager,
                           textureTransformManager,
                           true);


  private MatricesUbo? matricesUbo_;
  private LightsUbo? lightsUbo_;

  ~ModelRenderer() => this.ReleaseUnmanagedResources_();

  public void Dispose() {
    this.ReleaseUnmanagedResources_();
    GC.SuppressFinalize(this);
  }

  private void ReleaseUnmanagedResources_() {
    this.impl_.Dispose();
    this.matricesUbo_?.Dispose();
    this.lightsUbo_?.Dispose();
  }

  public IReadOnlyModel Model => this.impl_.Model;

  public IReadOnlySet<IReadOnlyMesh>? HiddenMeshes {
    get => this.impl_.HiddenMeshes;
    set => this.impl_.HiddenMeshes = value;
  }

  public bool UseLighting { get; set; }

  public void UpdateBuffer() => this.impl_.UpdateBuffer();

  public void Render() {
    var bonesUsedByVertices = model.Skin.BonesUsedByVertices;
    Span<Matrix4x4> boneMatrices
        = stackalloc Matrix4x4[1 + bonesUsedByVertices.Count];
    boneMatrices[0] = Matrix4x4.Identity;
    var boneIndex = 1;
    foreach (var bone in bonesUsedByVertices) {
      var localToWorldMatrix =
          boneTransformManager?.GetLocalToWorldMatrix(bone).Impl ??
          Matrix4x4.Identity;
      var inverseMatrix =
          boneTransformManager?.GetInverseBindMatrix(bone).Impl ??
          Matrix4x4.Identity;
      boneMatrices[boneIndex++] = inverseMatrix * localToWorldMatrix;
    }

    this.matricesUbo_ ??= new(model.Skin.BonesUsedByVertices.Count);
    this.lightsUbo_ ??= new LightsUbo();

    this.matricesUbo_.UpdateData(GlTransform.ModelMatrix,
                                 GlTransform.ViewMatrix,
                                 GlTransform.ProjectionMatrix,
                                 boneMatrices);
    this.matricesUbo_.Bind();

    this.lightsUbo_.UpdateData(this.UseLighting, lighting);
    this.lightsUbo_.Bind();

    this.impl_.Render();
  }

  public void GenerateModelIfNull() => this.impl_.GenerateModelIfNull();

  public IEnumerable<IGlMaterialShader> GetMaterialShaders(IReadOnlyMaterial material)
    => this.impl_.GetMaterialShaders(material);
}