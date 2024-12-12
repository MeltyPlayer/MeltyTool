using System.Drawing;
using System.Numerics;

using fin.math;
using fin.model;
using fin.model.impl;

using OpenTK.Graphics.OpenGL;

using LogicOp = fin.model.LogicOp;

namespace fin.ui.rendering.gl.model;

public interface ISkeletonRenderer : IRenderable {
  IReadOnlySkeleton Skeleton { get; }
  IReadOnlyBone? SelectedBone { get; set; }
  float Scale { get; set; }
}

/// <summary>
///   A renderer for a Fin model's skeleton.
/// </summary>
public class SkeletonRenderer(
    IReadOnlySkeleton skeleton,
    IReadOnlyBoneTransformManager boneTransformManager)
    : ISkeletonRenderer {
  private static readonly IModelRenderer BONE_RENDERER_;
  private static float BONE_SCALE_ = 5;

  static SkeletonRenderer() {
    var model = ModelImpl.CreateForViewer();

    var material = model.MaterialManager.AddNullMaterial();
    material.DepthMode = DepthMode.WRITE_ONLY;
    material.SetBlending(BlendEquation.ADD,
                         BlendFactor.CONST_COLOR,
                         BlendFactor.SRC_ALPHA,
                         LogicOp.SET);

    var skin = model.Skin;

    var from = skin.AddVertex(new Vector3(0, 0, 0));
    var to = skin.AddVertex(new Vector3(BONE_SCALE_, 0, 0));

    var midpoint = .25f * BONE_SCALE_;
    var radius = .2f * BONE_SCALE_;
    var middle1 = skin.AddVertex(new Vector3(midpoint, -radius, 0));
    var middle2 = skin.AddVertex(new Vector3(midpoint, 0, -radius));
    var middle3 = skin.AddVertex(new Vector3(midpoint, radius, 0));
    var middle4 = skin.AddVertex(new Vector3(midpoint, 0, radius));

    skin.AddMesh()
        .AddLines([
            from, middle1,
            from, middle2,
            from, middle3,
            from, middle4,

            middle1, middle2,
            middle2, middle3,
            middle3, middle4,
            middle4, middle1,

            middle1, to,
            middle2, to,
            middle3, to,
            middle4, to,
        ])
        .SetMaterial(material);
        
    BONE_RENDERER_ = new ModelRendererV2(model);
  }

  public IReadOnlySkeleton Skeleton { get; } = skeleton;
  public IReadOnlyBone? SelectedBone { get; set; }
  public float Scale { get; set; } = 1;

  public void Render() {
    GL.LineWidth(1);
    this.RenderBone_(this.Skeleton.Root, this.Skeleton);
  }

  private void RenderBone_(IReadOnlyBone bone, IReadOnlySkeleton skeleton) {
    GlTransform.PushMatrix();
    GlTransform.MultMatrix(boneTransformManager.GetWorldMatrix(bone).Impl);

    if (skeleton.Root != bone) {
      GlUtil.SetBlendColor(bone == this.SelectedBone
                               ? Color.White
                               : Color.Blue);
      BONE_RENDERER_.Render();
    }
    GlTransform.PopMatrix();

    foreach (var child in bone.Children) {
      this.RenderBone_(child, skeleton);
    }
  }
}