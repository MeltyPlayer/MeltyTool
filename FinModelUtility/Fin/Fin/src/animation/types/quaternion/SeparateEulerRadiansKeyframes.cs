using System;
using System.Collections.Generic;
using System.Numerics;

using fin.animation.interpolation;
using fin.animation.keyframes;
using fin.math.floats;
using fin.math.rotations;
using fin.model;
using fin.util.optional;

namespace fin.animation.types.quaternion;

public class SeparateEulerRadiansKeyframes<TKeyframe>(
    ISharedInterpolationConfig sharedConfig,
    IKeyframeInterpolator<TKeyframe, float> interpolator,
    IndividualInterpolationConfig<float> individualConfigX,
    IndividualInterpolationConfig<float> individualConfigY,
    IndividualInterpolationConfig<float> individualConfigZ)
    : ISeparateQuaternionKeyframes<TKeyframe>
    where TKeyframe : IKeyframe<float> {
  private readonly IReadOnlyList<InterpolatedKeyframes<TKeyframe, float>> axes_
      = [
          new InterpolatedKeyframes<TKeyframe, float>(
              sharedConfig,
              interpolator,
              individualConfigX),
          new InterpolatedKeyframes<TKeyframe, float>(
              sharedConfig,
              interpolator,
              individualConfigY),
          new InterpolatedKeyframes<TKeyframe, float>(
              sharedConfig,
              interpolator,
              individualConfigZ),
      ];

  public SeparateEulerRadiansKeyframes(
      ISharedInterpolationConfig sharedConfig,
      IKeyframeInterpolator<TKeyframe, float> interpolator,
      IndividualInterpolationConfig<float> individualConfig = default)
      : this(sharedConfig,
             interpolator,
             individualConfig,
             individualConfig,
             individualConfig) { }

  public IReadOnlyList<IInterpolatableKeyframes<TKeyframe, float>> Axes
    => this.axes_;

  public bool TryGetAtFrame(float frame, out Quaternion value) {
    value = default;

    var xTrack = this.axes_[0];
    var yTrack = this.axes_[1];
    var zTrack = this.axes_[2];

    xTrack.TryGetPrecedingAndFollowingKeyframes(
        frame,
        out var fromXFrame,
        out var toXFrame,
        out _);
    yTrack.TryGetPrecedingAndFollowingKeyframes(
        frame,
        out var fromYFrame,
        out var toYFrame,
        out _);
    zTrack.TryGetPrecedingAndFollowingKeyframes(
        frame,
        out var fromZFrame,
        out var toZFrame,
        out _);

    Span<TKeyframe?> fromsAndTos = [
        fromXFrame,
        fromYFrame,
        fromZFrame,
        toXFrame,
        toYFrame,
        toZFrame,
    ];
    Span<bool> areAxesStatic = stackalloc bool[3];
    AreAxesStatic_(fromsAndTos, areAxesStatic);

    var defaultX = individualConfigX.DefaultValue.GetOrNull();
    var defaultY = individualConfigY.DefaultValue.GetOrNull();
    var defaultZ = individualConfigZ.DefaultValue.GetOrNull();
    if (!CanInterpolateWithQuaternions_(
            fromsAndTos,
            areAxesStatic)) {
      if (!xTrack.TryGetAtFrameOrDefault(frame,
                                         individualConfigX,
                                         out var xRadians)) {
        return false;
      }

      if (!yTrack.TryGetAtFrameOrDefault(frame,
                                         individualConfigY,
                                         out var yRadians)) {
        return false;
      }

      if (!zTrack.TryGetAtFrameOrDefault(frame,
                                         individualConfigZ,
                                         out var zRadians)) {
        return false;
      }

      value = this.ConvertRadiansToQuaternionImpl(xRadians, yRadians, zRadians);
      return true;
    }

    if (GetFromAndToFrameIndex_(fromsAndTos,
                                areAxesStatic,
                                out var fromFrame,
                                out var toFrame)) {
      if (toFrame < fromFrame) {
        toFrame += sharedConfig.AnimationLength;
      }

      var frameDelta = (frame - fromFrame) / (toFrame - fromFrame);

      var q1 = this.ConvertRadiansToQuaternionImpl(
          fromXFrame?.ValueOut ?? defaultX,
          fromYFrame?.ValueOut ?? defaultY,
          fromZFrame?.ValueOut ?? defaultZ);
      var q2 = this.ConvertRadiansToQuaternionImpl(
          toXFrame?.ValueIn ?? defaultX,
          toYFrame?.ValueIn ?? defaultY,
          toZFrame?.ValueIn ?? defaultZ);

      if (Quaternion.Dot(q1, q2) < 0) {
        q2 = -q2;
      }

      var interp = Quaternion.Slerp(q1, q2, frameDelta);
      value = Quaternion.Normalize(interp);
      return true;
    }

    value = Quaternion.Normalize(this.ConvertRadiansToQuaternionImpl(
                                     fromXFrame?.ValueOut ?? defaultX,
                                     fromYFrame?.ValueOut ?? defaultY,
                                     fromZFrame?.ValueOut ?? defaultZ));
    return true;
  }

  private static void AreAxesStatic_(ReadOnlySpan<TKeyframe?> fromsAndTos,
                                     Span<bool> areAxesStatic) {
    for (var i = 0; i < 3; ++i) {
      var from = fromsAndTos[i];
      var to = fromsAndTos[3 + i];

      if (from == null && to == null) {
        areAxesStatic[i] = true;
      } else if (from != null && to != null) {
        if (from.ValueOut.IsRoughly(to.ValueIn)) {
          if (!SUPPORTS_TANGENTS_IN_QUATERNIONS) {
            areAxesStatic[i] = true;
          } else {
            /*var fromTangentOrNull = from.tangent;
            var toTangentOrNull = from.tangent;
            if (fromTangentOrNull == null && toTangentOrNull == null) {
              areAxesStatic[i] = true;
            } else if (fromTangentOrNull != null &&
                       toTangentOrNull != null) {
              var fromTangent = fromTangentOrNull.Value;
              var toTangent = toTangentOrNull.Value;
              areAxesStatic[i] = fromTangent.IsRoughly(toTangent);
            }*/
          }
        }
      }
    }
  }

  private static bool GetFromAndToFrameIndex_(
      ReadOnlySpan<TKeyframe?> fromsAndTos,
      ReadOnlySpan<bool> areAxesStatic,
      out float fromFrameIndex,
      out float toFrameIndex) {
    for (var i = 0; i < 3; ++i) {
      if (!areAxesStatic[i]) {
        fromFrameIndex = fromsAndTos[i].Frame;
        toFrameIndex = fromsAndTos[3 + i].Frame;
        return true;
      }
    }

    fromFrameIndex = default;
    toFrameIndex = default;
    return false;
  }

  private static bool CanInterpolateWithQuaternions_(
      ReadOnlySpan<TKeyframe?> fromsAndTos,
      ReadOnlySpan<bool> areAxesStatic) {
    for (var i = 0; i < 6; ++i) {
      if (areAxesStatic[i % 3]) {
        continue;
      }

      if (fromsAndTos[i] == null) {
        return false;
      }

      if (!SUPPORTS_TANGENTS_IN_QUATERNIONS) {
        /*if ((fromsAndTos[i].Value.tangent ?? 0) != 0) {
          return false;
        }*/
      }
    }

    for (var i = 0; i < 3; ++i) {
      if (areAxesStatic[i]) {
        continue;
      }

      var from = fromsAndTos[i];
      for (var oi = i + 1; oi < 3; ++oi) {
        if (areAxesStatic[oi]) {
          continue;
        }

        var to = fromsAndTos[oi];
        if (!from.Frame.IsRoughly(to.Frame)) {
          return false;
        }

        if (SUPPORTS_TANGENTS_IN_QUATERNIONS) {
          /*var fromTangentOrNull = from.tangent;
          var toTangentOrNull = to.tangent;
          if ((fromTangentOrNull == null) != (toTangentOrNull == null)) {
            return false;
          }

          if (fromTangentOrNull != null &&
              toTangentOrNull != null &&
              !fromTangentOrNull.Value.IsRoughly(toTangentOrNull.Value)) {
            return false;
          }*/
        }
      }
    }

    return true;
  }

  public IEulerRadiansRotationTrack3d.ConvertRadiansToQuaternion
      ConvertRadiansToQuaternionImpl { get; set; } =
    QuaternionUtil.CreateZyx;


  // TODO: Add support for tangents in quaternions
  private const bool SUPPORTS_TANGENTS_IN_QUATERNIONS = false;
}