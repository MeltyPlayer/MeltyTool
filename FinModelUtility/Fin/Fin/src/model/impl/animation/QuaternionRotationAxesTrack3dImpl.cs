using System.Numerics;

using fin.animation;
using fin.math.interpolation;
using fin.math.rotations;
using fin.util.asserts;

namespace fin.model.impl;

public partial class ModelImpl<TVertex> {
  public class QuaternionAxesRotationTrack3dImpl
      : BScalarAxesTrack<Quaternion, float, FloatInterpolator>,
        IQuaternionAxesRotationTrack3d {
    private readonly IReadOnlyBone bone_;

    public QuaternionAxesRotationTrack3dImpl(IAnimation animation, IReadOnlyBone bone)
        : base(
            animation,
            4,
            new[] { 0, 0, 0, 0 },
            new FloatInterpolator()) {
      this.bone_ = bone;
    }

    public override bool TryGetInterpolatedFrame(
        float frame,
        out Quaternion interpolatedValue,
        AnimationInterpolationConfig? config = null) {
      // TODO: Might need to do something fancier here
      var defaultRotation = this.bone_.LocalRotation != null
          ? QuaternionUtil.Create(this.bone_.LocalRotation)
          : Quaternion.Identity;

      if (!this.axisTracks[0]
               .TryGetInterpolatedFrame(frame,
                                        out var x,
                                        config)) {
        x = defaultRotation.X;
      }

      if (!this.axisTracks[1]
               .TryGetInterpolatedFrame(frame,
                                        out var y,
                                        config)) {
        y = defaultRotation.Y;
      }

      if (!this.axisTracks[2]
               .TryGetInterpolatedFrame(frame,
                                        out var z,
                                        config)) {
        z = defaultRotation.Z;
      }

      if (!this.axisTracks[3]
               .TryGetInterpolatedFrame(frame,
                                        out var w,
                                        config)) {
        w = defaultRotation.W;
      }

      interpolatedValue = new(x, y, z, w);
      return true;
    }
  }
}