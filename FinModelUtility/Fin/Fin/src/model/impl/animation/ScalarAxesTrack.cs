using System;
using System.Linq;
using System.Runtime.CompilerServices;

using fin.animation;
using fin.animation.keyframes;
using fin.math.interpolation;

namespace fin.model.impl {
  public partial class ModelImpl<TVertex> {
    public abstract class BScalarAxesTrack<TAxes, TAxis, TInterpolator>
        : BScalarAxesTrack<TAxes, TAxis, TAxes, TInterpolator>
        where TInterpolator : IInterpolator<TAxis> {
      public BScalarAxesTrack(
          IAnimation animation,
          int axisCount,
          ReadOnlySpan<int> initialKeyframeCapacitiesPerAxis,
          TInterpolator interpolator)
          : base(animation,
                 axisCount,
                 initialKeyframeCapacitiesPerAxis,
                 interpolator) { }
    }

    public abstract class BScalarAxesTrack<TAxes, TAxis, TInterpolated,
                                           TInterpolator>
        : IAxesTrack<TAxis, TInterpolated>
        where TInterpolator : IInterpolator<TAxis> {
      protected InputOutputTrackImpl<TAxis, TInterpolator>[] axisTracks;

      public BScalarAxesTrack(
          IAnimation animation,
          int axisCount,
          ReadOnlySpan<int> initialKeyframeCapacitiesPerAxis,
          TInterpolator interpolator) {
        this.Animation = animation;
        this.axisTracks =
            new InputOutputTrackImpl<TAxis, TInterpolator>[axisCount];
        for (var i = 0; i < axisCount; ++i) {
          this.axisTracks[i] =
              new InputOutputTrackImpl<TAxis, TInterpolator>(animation,
                initialKeyframeCapacitiesPerAxis[i],
                interpolator);
        }
      }

      public IAnimation Animation { get; }
      public bool HasAtLeastOneKeyframe => this.axisTracks.Any(axis => axis.HasAtLeastOneKeyframe);

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public void Set(
          int frame,
          int axis,
          TAxis incomingValue,
          TAxis outgoingValue,
          float? optionalIncomingTangent,
          float? optionalOutgoingTangent,
          string frameType = "")
        => this.axisTracks[axis]
               .SetKeyframe(frame,
                    incomingValue,
                    outgoingValue,
                    optionalIncomingTangent,
                    optionalOutgoingTangent,
                    frameType);

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public Keyframe<ValueAndTangents<TAxis>>? GetKeyframe(
          int keyframe,
          int axis)
        => this.axisTracks[axis].GetKeyframe(keyframe);

      public abstract bool TryGetInterpolatedFrame(
          float frame,
          out TInterpolated interpolatedValue,
          AnimationInterpolationConfig? config = null
      );
    }
  }
}