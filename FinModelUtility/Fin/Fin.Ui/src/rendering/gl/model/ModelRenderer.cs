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
public partial class ModelRenderer
    : IDynamicModelRenderer {
  private readonly IReadOnlyList<IReadOnlyBone> bonesUsedByVertices_;

  private readonly Matrix4x4[] boneMatrices_;

  private readonly IDynamicModelRenderer impl_;

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
  private readonly IReadOnlyModel model_;
  private readonly IReadOnlyLighting? lighting_;
  private readonly IReadOnlyBoneTransformManager? boneTransformManager_;

  /// <summary>
  ///   A renderer for a Fin model.
  ///
  ///   NOTE: This will only be valid in the GL context this was first rendered in!
  /// </summary>
  public ModelRenderer(IReadOnlyModel model,
                       IReadOnlyLighting? lighting = null,
                       IReadOnlyBoneTransformManager? boneTransformManager = null,
                       IReadOnlyTextureTransformManager? textureTransformManager = null,
                       bool dynamic = false) {
    this.model_ = model;
    this.lighting_ = lighting;
    this.boneTransformManager_ = boneTransformManager;
    
    this.bonesUsedByVertices_ = model.Skin.BonesUsedByVertices.ToArray();
    this.boneMatrices_ = new Matrix4x4[1 + this.bonesUsedByVertices_.Count];
    this.boneMatrices_[0] = Matrix4x4.Identity;

    this.impl_ = (model.Skin.AllowMaterialRendererMerging)
        ? new MergedMaterialMeshesRenderer(model,
                                           textureTransformManager,
                                           dynamic)
        : new UnmergedMaterialMeshesRenderer(
            model,
            textureTransformManager,
            dynamic);
  }

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

    this.matricesUbo_ ??= new(this.model_.Skin.BonesUsedByVertices.Count);
    this.lightsUbo_ ??= new LightsUbo();

    this.matricesUbo_.UpdateData(GlTransform.ModelMatrix,
                                 GlTransform.ViewMatrix,
                                 GlTransform.ProjectionMatrix,
                                 this.boneMatrices_);
    this.matricesUbo_.Bind();

    this.lightsUbo_.UpdateData(this.UseLighting, this.lighting_);
    this.lightsUbo_.Bind();

    this.impl_.Render();
  }

  public void GenerateModelIfNull() => this.impl_.GenerateModelIfNull();

  public IEnumerable<IGlMaterialShader> GetMaterialShaders(IReadOnlyMaterial material)
    => this.impl_.GetMaterialShaders(material);
}