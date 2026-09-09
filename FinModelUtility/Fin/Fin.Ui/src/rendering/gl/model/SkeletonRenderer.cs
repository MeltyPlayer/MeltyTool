using System.Drawing;
using System.Numerics;

using fin.data.indexable;
using fin.math.floats;
using fin.math.matrix.four;
using fin.model;
using fin.model.impl;
using fin.model.skeleton;
using fin.util.asserts;

using OpenTK.Graphics.OpenGL4;

using LogicOp = fin.model.LogicOp;


namespace fin.ui.rendering.gl.model;

public interface ISkeletonRenderer : IRenderable {
  IReadOnlySkeleton Skeleton { get; }
  IReadOnlyBone? SelectedBone { get; set; }
}

/// <summary>
///   A renderer for a Fin model's skeleton.
/// </summary>
public sealed class SkeletonRenderer
    : ISkeletonRenderer {
  private static readonly IModelRenderer BONE_RENDERER_= GenerateBoneRenderer();

  private static readonly Color UNSELECTED_BONE = Color.Blue;
  private static readonly Color SELECTED_BONE = Color.White;
  private static readonly Color SELECTED_CHILD = Color.CornflowerBlue;

  public static IModelRenderer GenerateBoneRenderer() {
    var model = ModelImpl.CreateForViewer();

    var material = model.MaterialManager.AddNullMaterial();
    material.DepthMode = DepthMode.WRITE_ONLY;
    material.SetBlending(BlendEquation.ADD,
                         BlendFactor.CONST_COLOR,
                         BlendFactor.ONE_MINUS_SRC_ALPHA,
                         LogicOp.UNDEFINED);

    var skin = model.Skin;

    var from = skin.AddVertex(new Vector3(0, 0, 0));
    var to = skin.AddVertex(new Vector3(1, 0, 0));

    var midpoint = .25f;
    var radius = .2f;
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

    return new ModelRenderer(model);
  }

  public void Dispose() { }

  public IReadOnlySkeleton Skeleton { get; }

  public IReadOnlyBone? SelectedBone {
    get;
    set {
      field = value;

      this.selectedChildren_.Clear();
      if (value != null) {
        AddChildrenToSet_(value, this.selectedChildren_);
      }
    }
  }

  private readonly IndexableSet<IReadOnlyBone> selectedChildren_ = [];
  private readonly IReadOnlyBoneTransformManager boneTransformManager_;

  private readonly IReadOnlyIndexableDictionary<IReadOnlyBone, Vector3>
      scaleByBone_;

  public SkeletonRenderer(IReadOnlyModel model,
                          IReadOnlyBoneTransformManager boneTransformManager) {
    var skeleton = model.Skeleton;

    this.boneTransformManager_ = boneTransformManager;
    this.Skeleton = skeleton;

    var verticesByBone
        = new IndexableDictionary<IReadOnlyBone, HashSet<IReadOnlyVertex>>(
            skeleton.Bones.Count);
    foreach (var vertex in model.Skin.Vertices) {
      var boneWeights = vertex.BoneWeights;
      if (boneWeights == null) {
        continue;
      }

      foreach (var boneWeight in boneWeights.Weights) {
        if (verticesByBone.TryGetValue(boneWeight.Bone, out var vertexSet)) {
          vertexSet.Add(vertex);
        } else {
          verticesByBone[boneWeight.Bone] = [vertex];
        }
      }
    }

    var scaleByBone
        = new IndexableDictionary<IReadOnlyBone, Vector3>(skeleton.Bones.Count);
    this.scaleByBone_ = scaleByBone;
    foreach (var bone in skeleton.Bones) {
      if (bone.Children is [var childBone]) {
        var length = childBone.Transform.LocalTranslation.X;
        scaleByBone[bone] = new Vector3(length);
        continue;
      }

      var maxLength = -1f;
      if (verticesByBone.TryGetValue(bone, out var vertices)) {
        foreach (var vertex in vertices) {
          var localPosition = vertex.LocalPosition;

          var boneWeights = vertex.BoneWeights.AssertNonnull();
          if (boneWeights.VertexSpace == VertexSpace.RELATIVE_TO_WORLD) {
            ProjectionUtil.ProjectPosition(
                boneTransformManager.GetInverseBindMatrix(boneWeights).Impl,
                ref localPosition);
          }

          maxLength = Math.Max(maxLength, localPosition.X);
        }
      }

      if (!maxLength.IsRoughly(-1f)) {
        scaleByBone[bone] = new Vector3(maxLength);
        continue;
      }

      // TODO: What to do in case where there's no vertices??
      scaleByBone[bone] = Vector3.One;
    }
  }

  public void Render() {
    GL.LineWidth(1);
    this.RenderBone_(this.Skeleton.Root, this.Skeleton);
  }

  private void RenderBone_(IReadOnlyBone bone, IReadOnlySkeleton skeleton) {
    GlTransform.PushMatrix();
    GlTransform.MultMatrix(this.boneTransformManager_.GetWorldMatrix(bone)
                               .Impl);

    if (skeleton.Root != bone) {
      GlTransform.Scale(this.scaleByBone_[bone]);
      GlUtil.SetBlendColor(bone == this.SelectedBone
                               ? SELECTED_BONE
                               : this.selectedChildren_.Contains(bone)
                                   ? SELECTED_CHILD
                                   : UNSELECTED_BONE);
      BONE_RENDERER_.Render();
    }

    GlTransform.PopMatrix();

    foreach (var child in bone.Children) {
      this.RenderBone_(child, skeleton);
    }
  }

  private static void AddChildrenToSet_(IReadOnlyBone bone,
                                        IIndexableSet<IReadOnlyBone> set) {
    foreach (var child in bone.Children) {
      set.Add(child);
      AddChildrenToSet_(child, set);
    }
  }
}