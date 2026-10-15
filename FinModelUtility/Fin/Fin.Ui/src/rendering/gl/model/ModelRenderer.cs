using System.Numerics;

using fin.math;
using fin.model;
using fin.model.skeleton;
using fin.model.skin;
using fin.ui.rendering.gl.material;
using fin.ui.rendering.gl.ubo;


namespace fin.ui.rendering.gl.model;

/// <summary>
///   A renderer for a Fin model.
///
///   NOTE: This will only be valid in the GL context this was first rendered in!
/// </summary>
public sealed partial class ModelRenderer : IDynamicModelRenderer {
  private readonly IReadOnlyList<IReadOnlyBone> bonesUsedByVertices_;
  private readonly Matrix4x4[] boneMatrices_;
  private readonly MergedMaterialMeshesRenderer impl_;

  public int VaoId => this.impl_.VaoId;

  public static IModelRenderer CreateStatic(
      IReadOnlyModel model,
      IReadOnlyBoneTransformManager? boneTransformManager = null,
      IReadOnlyTextureTransformManager? textureTransformManager = null,
      IReadOnlyTextureSwapManager? textureSwapManager = null)
    => new ModelRenderer(model,
                         boneTransformManager,
                         textureTransformManager,
                         textureSwapManager);

  public static IDynamicModelRenderer CreateDynamic(
      IReadOnlyModel model,
      IReadOnlyBoneTransformManager? boneTransformManager = null,
      IReadOnlyTextureTransformManager? textureTransformManager = null,
      IReadOnlyTextureSwapManager? textureSwapManager = null)
    => new ModelRenderer(model,
                         boneTransformManager,
                         textureTransformManager,
                         textureSwapManager,
                         true);


  private IModelMatricesBo? matricesBo_;
  private readonly IReadOnlyModel model_;
  private readonly IReadOnlyBoneTransformManager? boneTransformManager_;

  public IEnumerable<IMaterialRenderer> MaterialRenderers
    => this.impl_.MaterialRenderers;

  /// <summary>
  ///   A renderer for a Fin model.
  ///
  ///   NOTE: This will only be valid in the GL context this was first rendered in!
  /// </summary>
  public ModelRenderer(
      IReadOnlyModel model,
      IReadOnlyBoneTransformManager? boneTransformManager = null,
      IReadOnlyTextureTransformManager? textureTransformManager = null,
      IReadOnlyTextureSwapManager? textureSwapManager = null,
      bool dynamic = false) {
    this.model_ = model;
    this.boneTransformManager_ = boneTransformManager;

    this.bonesUsedByVertices_ = model.Skin.BonesUsedByVertices.ToArray();
    this.boneMatrices_ = new Matrix4x4[1 + this.bonesUsedByVertices_.Count];
    this.boneMatrices_[0] = Matrix4x4.Identity;

    this.impl_ = new MergedMaterialMeshesRenderer(model,
                                                  textureTransformManager!,
                                                  textureSwapManager!,
                                                  dynamic);
  }

  ~ModelRenderer() => this.ReleaseUnmanagedResources_();

  public void Dispose() {
    this.ReleaseUnmanagedResources_();
    GC.SuppressFinalize(this);
  }

  private void ReleaseUnmanagedResources_() {
    this.impl_.Dispose();
    this.matricesBo_?.Dispose();
    this.LightsUbo?.Dispose();
  }

  public IReadOnlyModel Model => this.impl_.Model;

  public IReadOnlyMeshVisibilityDictionary? MeshVisibility {
    get => this.impl_.MeshVisibility;
    set => this.impl_.MeshVisibility = value;
  }

  public LightsUbo? LightsUbo { get; set; }

  public void UpdateBuffer() => this.impl_.UpdateBuffer();

  public void Render() {
    this.UpdateMatricesUbo();
    this.BindMatricesUbo();
    this.LightsUbo?.Bind();

    this.impl_.Render();
  }

  public void UpdateMatricesUbo() {
    var boneIndex = 1;
    // Intentionally looping by index to avoid allocating an enumerator.
    for (var i = 0; i < this.bonesUsedByVertices_.Count; ++i) {
      var bone = this.bonesUsedByVertices_[i];
      var localToWorldMatrix =
          this.boneTransformManager_?.GetLocalToWorldMatrix(bone).Impl ??
          Matrix4x4.Identity;
      var inverseMatrix =
          this.boneTransformManager_?.GetInverseBindMatrix(bone).Impl ??
          Matrix4x4.Identity;
      this.boneMatrices_[boneIndex++] = inverseMatrix * localToWorldMatrix;
    }

    this.matricesBo_
        ??= ModelMatricesBo.New(this.model_.Skin.BonesUsedByVertices.Count);
    this.matricesBo_.UpdateData(GlTransform.ModelMatrix, this.boneMatrices_);
  }

  public void BindMatricesUbo() => this.matricesBo_.Bind();

  public void GenerateModelIfNull() => this.impl_.GenerateModelIfNull();

  public IEnumerable<IGlMaterialShader> GetMaterialShaders(
      IReadOnlyMaterial material)
    => this.impl_.GetMaterialShaders(material);
}