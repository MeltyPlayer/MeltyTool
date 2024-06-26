using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using fin.animation;
using fin.animation.tracks;
using fin.math.interpolation;

namespace fin.model.impl;

public partial class ModelImpl<TVertex> {
  public class ImplTrackImpl<TValue> : IImplTrack<TValue> {
    protected readonly Keyframes<ValueAndTangents<TValue>> impl;

    public ImplTrackImpl(IAnimation animation, int initialCapacity) {
      this.Animation = animation;
      this.impl = new Keyframes<ValueAndTangents<TValue>>(initialCapacity);
    }

    public IAnimation Animation { get; }

    public IReadOnlyList<Keyframe<ValueAndTangents<TValue>>> Keyframes
      => this.impl.Definitions;

    public bool HasAtLeastOneKeyframe => this.impl.HasAtLeastOneKeyframe;
    public int MaxKeyframe => this.impl.MaxKeyframe;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetKeyframe(
        int frame,
        TValue incomingValue,
        TValue outgoingValue,
        float? optionalIncomingTangent,
        float? optionalOutgoingTangent,
        string frameType = "")
      => this.impl.SetKeyframe(
          frame,
          new ValueAndTangents<TValue>(
              incomingValue,
              outgoingValue,
              optionalIncomingTangent,
              optionalOutgoingTangent),
          frameType);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetAllKeyframes(IEnumerable<TValue> values)
      => this.impl.SetAllKeyframes(
          values.Select(t => new ValueAndTangents<TValue>(t)));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Keyframe<ValueAndTangents<TValue>>? GetKeyframe(int frame)
      => this.impl.GetKeyframeAtFrame(frame);

    public bool TryGetInterpolationData(
        float frame,
        out (float frame, TValue value, float? tangent)? fromData,
        out (float frame, TValue value, float? tangent)? toData,
        AnimationInterpolationConfig? config = null
    ) {
      var useLoopingInterpolation = config?.UseLoopingInterpolation ?? false;

      var keyframeDefined = this.impl.FindIndexOfKeyframe((int) frame,
        out var fromKeyframeIndex,
        out var fromKeyframe,
        out var isLastKeyframe);
      fromData = toData = null;

      if (!keyframeDefined) {
        return false;
      }

      var fromTime = fromKeyframe.Frame;
      var fromValue = fromKeyframe.Value.OutgoingValue;
      var fromOutgoingTangent = fromKeyframe.Value.OutgoingTangent;
      fromData = (fromTime, fromValue, fromOutgoingTangent);

      // TODO: Make this an option?
      if (!keyframeDefined || (isLastKeyframe && !useLoopingInterpolation)) {
        return true;
      }

      var wrapsAround = isLastKeyframe && useLoopingInterpolation;

      var toKeyframe =
          !wrapsAround
              ? this.impl.GetKeyframeAtIndex(fromKeyframeIndex + 1)
              : this.impl.GetKeyframeAtIndex(0);
      var toTime = toKeyframe.Frame;
      var toValue = toKeyframe.Value.IncomingValue;
      var toIncomingTangent = toKeyframe.Value.IncomingTangent;

      if (wrapsAround) {
        if (frame >= fromTime) {
          toTime += this.Animation.FrameCount;
        } else {
          fromTime -= this.Animation.FrameCount;
          fromData = (fromTime, fromValue, fromOutgoingTangent);
        }
      }

      toData = (toTime, toValue, toIncomingTangent);
      return true;
    }
  }

  public class InputOutputTrackImpl<T, TInterpolator>
      : InputOutputTrackImpl<T, T, TInterpolator>,
        IInputOutputTrack<T, TInterpolator>
      where TInterpolator : IInterpolator<T> {
    public InputOutputTrackImpl(
        IAnimation animation,
        int initialCapacity,
        TInterpolator interpolator) :
        base(animation, initialCapacity, interpolator) { }
  }

  public class InputOutputTrackImpl<TValue, TInterpolated, TInterpolator>
      : ImplTrackImpl<TValue>,
        IInputOutputTrack<TValue, TInterpolated, TInterpolator>
      where TInterpolator : IInterpolator<TValue, TInterpolated> {
    public InputOutputTrackImpl(
        IAnimation animation,
        int initialCapacity,
        TInterpolator interpolator) : base(animation, initialCapacity) {
      this.Interpolator = interpolator;
    }

    public TInterpolator Interpolator { get; private set; }

    public void Set(
        IInputOutputTrack<TValue, TInterpolated, TInterpolator> other) {
      this.Interpolator = other.Interpolator;

      foreach (var keyframe in other.Keyframes) {
        this.SetKeyframe(keyframe.Frame,
                         keyframe.Value.IncomingValue,
                         keyframe.Value.OutgoingValue,
                         keyframe.Value.IncomingTangent,
                         keyframe.Value.OutgoingTangent);
      }
    }

    public bool TryGetInterpolatedFrame(
        float frame,
        out TInterpolated interpolatedValue,
        AnimationInterpolationConfig? config = null) {
      var useLoopingInterpolation = config?.UseLoopingInterpolation ?? false;

      var keyframeDefined = this.impl.FindIndexOfKeyframe((int) frame,
        out var fromKeyframeIndex,
        out var fromKeyframe,
        out var isLastKeyframe);

      if (!keyframeDefined) {
        interpolatedValue = default;
        return false;
      }

      var fromValue = fromKeyframe.Value.OutgoingValue;

      // TODO: Make this an option?
      if (isLastKeyframe && !useLoopingInterpolation) {
        interpolatedValue =
            this.Interpolator.Interpolate(fromValue, fromValue, 0);
        return true;
      }

      var fromTime = fromKeyframe.Frame;
      var fromTangent = fromKeyframe.Value.OutgoingTangent;

      var wrapsAround = isLastKeyframe && useLoopingInterpolation;

      var toKeyframe = !wrapsAround
          ? this.impl.GetKeyframeAtIndex(fromKeyframeIndex + 1)
          : this.impl.GetKeyframeAtIndex(0);
      var toValue = toKeyframe.Value.IncomingValue;
      var toTime = toKeyframe.Frame;

      if (wrapsAround) {
        if (frame >= fromTime) {
          toTime += this.Animation.FrameCount;
        } else {
          fromTime -= this.Animation.FrameCount;
        }
      }

      var toTangent = toKeyframe.Value.IncomingTangent;

      var duration = toTime - fromTime;
      var progress = (frame - fromTime) / duration;

      var useTangents = fromTangent != null && toTangent != null;
      interpolatedValue = !useTangents
          ? this.Interpolator.Interpolate(fromValue, toValue, progress)
          : this.Interpolator.Interpolate(
              fromTime,
              fromValue,
              fromTangent.Value,
              toTime,
              toValue,
              toTangent.Value,
              frame);
      return true;
    }
  }
}