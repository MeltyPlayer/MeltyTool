using System;
using System.Collections.Generic;
using System.Numerics;

using fin.animation.interpolation;
using fin.animation.keyframes;
using fin.animation.types.quaternion;
using fin.math;
using fin.math.rotations;
using fin.util.optional;

namespace fin.animation.types.vector3;

public sealed class SeparateEulerRadiansAdvancingInterpolator<TKeyframe>(
    ISeparateEulerRadiansKeyframes<TKeyframe> parent,
    ISharedInterpolationConfig sharedConfig,
    IndividualInterpolationConfig<float>? individualConfigX,
    IndividualInterpolationConfig<float>? individualConfigY,
    IndividualInterpolationConfig<float>? individualConfigZ,
    IReadOnlyList<TKeyframe> keyframesX,
    IReadOnlyList<TKeyframe> keyframesY,
    IReadOnlyList<TKeyframe> keyframesZ,
    IKeyframeInterpolator<TKeyframe, float> keyframeInterpolator)
    : IAdvancingInterpolator<Quaternion>
    where TKeyframe : class, IKeyframe<float> {
  private readonly AdvancingInterpolator<float, TKeyframe> xImpl_
      = new(sharedConfig, individualConfigX, keyframesX, keyframeInterpolator);

  private readonly AdvancingInterpolator<float, TKeyframe> yImpl_
      = new(sharedConfig, individualConfigY, keyframesY, keyframeInterpolator);

  private readonly AdvancingInterpolator<float, TKeyframe> zImpl_
      = new(sharedConfig, individualConfigZ, keyframesZ, keyframeInterpolator);

  public bool TryGetAtFrame(float frame, out Quaternion value) {
    if (sharedConfig.Looping) {
      frame = frame.ModRange(0, sharedConfig.AnimationLength);
    }

    value = default;

    var xTrack = this.xImpl_;
    var yTrack = this.yImpl_;
    var zTrack = this.zImpl_;

    var xInterpolationType = xTrack.TryGetPrecedingAndFollowingKeyframes(
        frame,
        out var fromXFrame,
        out var toXFrame);
    var yInterpolationType = yTrack.TryGetPrecedingAndFollowingKeyframes(
        frame,
        out var fromYFrame,
        out var toYFrame);
    var zInterpolationType = zTrack.TryGetPrecedingAndFollowingKeyframes(
        frame,
        out var fromZFrame,
        out var toZFrame);

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
    SeparateEulerRadiansKeyframesUtil.AreAxesStatic(areFromsAndTosNull, fromsAndTos, areAxesStatic);

    if (!SeparateEulerRadiansKeyframesUtil.CanInterpolateWithQuaternions(
            areFromsAndTosNull,
            fromsAndTos,
            areAxesStatic)) {
      if (!xTrack.TryGetAtFrame(frame, out var xRadians)) {
        return false;
      }

      if (!yTrack.TryGetAtFrame(frame, out var yRadians)) {
        return false;
      }

      if (!zTrack.TryGetAtFrame(frame, out var zRadians)) {
        return false;
      }

      value = parent.ConvertRadiansToQuaternionImpl(xRadians, yRadians, zRadians);
      return true;
    }

    var defaultX = individualConfigX.DefaultValue.GetOrNull();
    var defaultY = individualConfigY.DefaultValue.GetOrNull();
    var defaultZ = individualConfigZ.DefaultValue.GetOrNull();

    var fromX
        = SeparateEulerRadiansKeyframesUtil.GetFromValueOrDefault(xInterpolationType, fromXFrame, defaultX);
    var fromY
        = SeparateEulerRadiansKeyframesUtil.GetFromValueOrDefault(yInterpolationType, fromYFrame, defaultY);
    var fromZ
        = SeparateEulerRadiansKeyframesUtil.GetFromValueOrDefault(zInterpolationType, fromZFrame, defaultZ);

    if (SeparateEulerRadiansKeyframesUtil.GetFromAndToFrameIndex(
            fromsAndTos,
            areAxesStatic,
            out var fromFrame,
            out var toFrame)) {
      if (toFrame < fromFrame) {
        toFrame += sharedConfig.AnimationLength;
      }

      var frameDelta = (frame - fromFrame) / (toFrame - fromFrame);

      var q1 = parent.ConvertRadiansToQuaternionImpl(fromX, fromY, fromZ);

      var toX = SeparateEulerRadiansKeyframesUtil.GetToValueOrDefault(xInterpolationType, toXFrame, defaultX);
      var toY = SeparateEulerRadiansKeyframesUtil.GetToValueOrDefault(yInterpolationType, toYFrame, defaultY);
      var toZ = SeparateEulerRadiansKeyframesUtil.GetToValueOrDefault(zInterpolationType, toZFrame, defaultZ);

      var q2 = parent.ConvertRadiansToQuaternionImpl(toX, toY, toZ);

      if (Quaternion.Dot(q1, q2) < 0) {
        q2 = -q2;
      }

      value = Quaternion.Normalize(QuaternionUtil.Slerp(q1, q2, frameDelta));

      return true;
    }

    value = Quaternion.Normalize(
        parent.ConvertRadiansToQuaternionImpl(fromX, fromY, fromZ));
    return true;
  }
}