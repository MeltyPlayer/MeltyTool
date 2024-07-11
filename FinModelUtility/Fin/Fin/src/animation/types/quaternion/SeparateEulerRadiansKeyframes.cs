using System;
using System.Collections.Generic;
using System.Numerics;

using fin.animation.interpolation;
using fin.animation.keyframes;
using fin.math;
using fin.math.floats;
using fin.math.rotations;
using fin.util.optional;

namespace fin.animation.types.quaternion;

// TODO: Add support for tangents in quaternions
public class SeparateEulerRadiansKeyframes<TKeyframe>(
    ISharedInterpolationConfig sharedConfig,
    IKeyframeInterpolator<TKeyframe, float> interpolator,
    IndividualInterpolationConfig<float> individualConfigX,
    IndividualInterpolationConfig<float> individualConfigY,
    IndividualInterpolationConfig<float> individualConfigZ)
    : ISeparateEulerRadiansKeyframes<TKeyframe>
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
    if (sharedConfig.Looping) {
      frame = frame.ModRange(0, sharedConfig.AnimationLength);
    }

    value = default;

    var xTrack = this.axes_[0];
    var yTrack = this.axes_[1];
    var zTrack = this.axes_[2];

    var xInterpolationType = xTrack.TryGetPrecedingAndFollowingKeyframes(
        frame,
        out var fromXFrame,
        out var toXFrame,
        out _);
    var yInterpolationType = yTrack.TryGetPrecedingAndFollowingKeyframes(
        frame,
        out var fromYFrame,
        out var toYFrame,
        out _);
    var zInterpolationType = zTrack.TryGetPrecedingAndFollowingKeyframes(
        frame,
        out var fromZFrame,
        out var toZFrame,
        out _);

    Span<bool> areFromsAndTosNull = [
        xInterpolationType is KeyframesUtil.InterpolationDataType.NONE,
        yInterpolationType is KeyframesUtil.InterpolationDataType.NONE,
        zInterpolationType is KeyframesUtil.InterpolationDataType.NONE,
        xInterpolationType is not KeyframesUtil.InterpolationDataType
                                               .PRECEDING_AND_FOLLOWING,
        yInterpolationType is not KeyframesUtil.InterpolationDataType
                                               .PRECEDING_AND_FOLLOWING,
        zInterpolationType is not KeyframesUtil.InterpolationDataType
                                               .PRECEDING_AND_FOLLOWING,
    ];

    Span<TKeyframe?> fromsAndTos = [
        fromXFrame,
        fromYFrame,
        fromZFrame,
        toXFrame,
        toYFrame,
        toZFrame,
    ];
    Span<bool> areAxesStatic = stackalloc bool[3];
    AreAxesStatic_(areFromsAndTosNull, fromsAndTos, areAxesStatic);

    if (!CanInterpolateWithQuaternions_(
            areFromsAndTosNull,
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

    var defaultX = individualConfigX.DefaultValue.GetOrNull();
    var defaultY = individualConfigY.DefaultValue.GetOrNull();
    var defaultZ = individualConfigZ.DefaultValue.GetOrNull();

    var fromX
        = GetFromValueOrDefault_(xInterpolationType, fromXFrame, defaultX);
    var fromY
        = GetFromValueOrDefault_(yInterpolationType, fromYFrame, defaultY);
    var fromZ
        = GetFromValueOrDefault_(zInterpolationType, fromZFrame, defaultZ);

    if (GetFromAndToFrameIndex_(fromsAndTos,
                                areAxesStatic,
                                out var fromFrame,
                                out var toFrame)) {
      if (toFrame < fromFrame) {
        toFrame += sharedConfig.AnimationLength;
      }

      var frameDelta = (frame - fromFrame) / (toFrame - fromFrame);

      var q1 = this.ConvertRadiansToQuaternionImpl(fromX, fromY, fromZ);

      var toX = GetToValueOrDefault_(xInterpolationType, toXFrame, defaultX);
      var toY = GetToValueOrDefault_(yInterpolationType, toYFrame, defaultY);
      var toZ = GetToValueOrDefault_(zInterpolationType, toZFrame, defaultZ);

      var q2 = this.ConvertRadiansToQuaternionImpl(toX, toY, toZ);

      if (Quaternion.Dot(q1, q2) < 0) {
        q2 = -q2;
      }

      var interp = Quaternion.Slerp(q1, q2, frameDelta);
      value = Quaternion.Normalize(interp);
      return true;
    }

    value = Quaternion.Normalize(
        this.ConvertRadiansToQuaternionImpl(fromX, fromY, fromZ));
    return true;
  }

  private static void AreAxesStatic_(
      ReadOnlySpan<bool> areFromsAndTosNull,
      ReadOnlySpan<TKeyframe?> fromsAndTos,
      Span<bool> areAxesStatic) {
    for (var i = 0; i < 3; ++i) {
      var fromIsNull = areFromsAndTosNull[i];
      var toIsNull = areFromsAndTosNull[3 + i];

      if (fromIsNull && toIsNull) {
        areAxesStatic[i] = true;
      } else if (!fromIsNull && !toIsNull) {
        var from = fromsAndTos[i];
        var to = fromsAndTos[3 + i];
        if (from.ValueOut.IsRoughly(to.ValueIn)) {
          areAxesStatic[i] = true;
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
      ReadOnlySpan<bool> areFromsAndTosNull,
      ReadOnlySpan<TKeyframe?> fromsAndTos,
      ReadOnlySpan<bool> areAxesStatic) {
    for (var i = 0; i < 3; ++i) {
      if (areAxesStatic[i % 3]) {
        continue;
      }

      if (areFromsAndTosNull[i] || areFromsAndTosNull[3 + i]) {
        return false;
      }

      if (fromsAndTos[i] is IKeyframeWithTangents<float> from &&
          (from.TangentOut ?? 0) != 0) {
        return false;
      }

      if (fromsAndTos[i] is IKeyframeWithTangents<float> to &&
          (to.TangentIn ?? 0) != 0) {
        return false;
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
      }
    }

    return true;
  }

  public ISeparateEulerRadiansKeyframes<TKeyframe>.ConvertRadiansToQuaternion
      ConvertRadiansToQuaternionImpl { get; set; } =
    QuaternionUtil.CreateZyx;

  private static float GetFromValueOrDefault_(
      KeyframesUtil.InterpolationDataType interpolationType,
      TKeyframe? fromKeyframe,
      float defaultValue)
    => interpolationType == KeyframesUtil.InterpolationDataType.NONE
        ? defaultValue
        : fromKeyframe.ValueOut;

  private static float GetToValueOrDefault_(
      KeyframesUtil.InterpolationDataType interpolationType,
      TKeyframe? toKeyframe,
      float defaultValue)
    => interpolationType ==
       KeyframesUtil.InterpolationDataType.PRECEDING_AND_FOLLOWING
        ? toKeyframe.ValueIn
        : defaultValue;
}