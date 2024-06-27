using System;
using System.Numerics;
using System.Runtime.CompilerServices;

using fin.animation;
using fin.math.interpolation;

namespace fin.model.impl {
  public partial class ModelImpl<TVertex> {
    public class ScaleTrackImpl
        : BScalarAxesTrack<Vector3, float, FloatInterpolator>,
          IScale3dTrack {
      private readonly IReadOnlyBone bone_;

      public ScaleTrackImpl(IAnimation animation,
                            IReadOnlyBone bone,
                            ReadOnlySpan<int> initialCapacityPerAxis) :
          base(animation, 3, initialCapacityPerAxis, new FloatInterpolator()) {
        this.bone_ = bone;
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public override bool TryGetInterpolatedFrame(
          float frame,
          out Vector3 interpolatedValue,
          AnimationInterpolationConfig? config = null) {
        var localScale = this.bone_.LocalScale;

        if (!this.axisTracks[0].TryGetInterpolatedFrame(frame, out var x, config)) {
          x = localScale?.X ?? 1;
        }

        if (!this.axisTracks[1].TryGetInterpolatedFrame(frame, out var y, config)) {
          y = localScale?.Y ?? 1;
        }

        if (!this.axisTracks[2].TryGetInterpolatedFrame(frame, out var z, config)) {
          z = localScale?.Z ?? 1;
        }

        interpolatedValue = new(x, y, z);
        return true;
      }
    }
  }
}