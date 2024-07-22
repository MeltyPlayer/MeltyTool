using System.Numerics;

using fin.math;
using fin.model;


namespace fin.ui.rendering.gl.model;

/// <summary>
///   A renderer for a Fin model.
///
///   NOTE: This will only be valid in the GL context this was first rendered in!
/// </summary>
public class ModelRendererV2(
    IReadOnlyModel model,
    IReadOnlyLighting? lighting,
    IReadOnlyBoneTransformManager? boneTransformManager = null,
    IReadOnlyTextureTransformManager? textureTransformManager = null)
    : IModelRenderer {
  // TODO: Require passing in a GL context in the constructor.

  private readonly IModelRenderer impl_
      = (model.Skin.AllowMaterialRendererMerging)
          ? new MergedMaterialByMeshRenderer(
              model,
              lighting,
              textureTransformManager)
          : new UnmergedMaterialMeshesRenderer(
              model,
              lighting,
              textureTransformManager);

  private MatrixUbo matricesAndCameraUbo_;

  ~ModelRendererV2() => ReleaseUnmanagedResources_();

  public void Dispose() {
    ReleaseUnmanagedResources_();
    GC.SuppressFinalize(this);
  }

  private void ReleaseUnmanagedResources_() => this.impl_.Dispose();

  public IReadOnlyModel Model => this.impl_.Model;

  public IReadOnlySet<IReadOnlyMesh>? HiddenMeshes {
    get => this.impl_.HiddenMeshes;
    set => this.impl_.HiddenMeshes = value;
  }

  public bool UseLighting {
    get => this.impl_.UseLighting;
    set => this.impl_.UseLighting = value;
  }

  public void Render() {
    var camera = Camera.Instance;

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

    if (this.matricesAndCameraUbo_ == null) {
      this.matricesAndCameraUbo_ = new(model.Skin.BonesUsedByVertices.Count);
    }

    this.matricesAndCameraUbo_.UpdateData(
        GlTransform.ModelMatrix,
        GlTransform.ModelViewMatrix,
        GlTransform.ProjectionMatrix,
        boneMatrices);

    this.matricesAndCameraUbo_.Bind();

    this.impl_.Render();
  }
}