using System;
using System.Collections.Generic;
using System.Numerics;

using fin.animation.interpolation;
using fin.animation.keyframes;
using fin.animation.types.radians;
using fin.animation.types.vector3;
using fin.math;
using fin.math.floats;
using fin.math.rotations;
using fin.util.asserts;
using fin.util.optional;

namespace fin.animation.types.quaternion;

// TODO: Add support for tangents in quaternions
public sealed class SeparateEulerRadiansKeyframes<TKeyframe>(
    ISharedInterpolationConfig sharedConfig,
    IRadiansKeyframeInterpolator<TKeyframe> interpolator,
    IndividualInterpolationConfig<float>? individualConfigX,
    IndividualInterpolationConfig<float>? individualConfigY,
    IndividualInterpolationConfig<float>? individualConfigZ)
    : ISeparateEulerRadiansKeyframes<TKeyframe>
    where TKeyframe : class, IKeyframe<float> {
  private readonly IReadOnlyList<InterpolatedKeyframes<TKeyframe, float>> axes_
      = [
          new (sharedConfig, interpolator, individualConfigX),
          new (sharedConfig, interpolator, individualConfigY),
          new (sharedConfig, interpolator, individualConfigZ),
      ];

  public SeparateEulerRadiansKeyframes(
      ISharedInterpolationConfig sharedConfig,
      IRadiansKeyframeInterpolator<TKeyframe> interpolator,
      IndividualInterpolationConfig<float>? individualConfig = null)
      : this(sharedConfig,
             interpolator,
             individualConfig,
             individualConfig,
             individualConfig) { }

  public int AnimationLength => sharedConfig.AnimationLength;
  public bool Looping => sharedConfig.Looping;

  public IReadOnlyList<IInterpolatableKeyframes<TKeyframe, float>> Axes
    => this.axes_;

  public bool TryGetAtFrameWithBinarySearch(float frame, out Quaternion value) {
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
    SeparateEulerRadiansKeyframesUtil.AreAxesStatic(areFromsAndTosNull, fromsAndTos, areAxesStatic);

    if (!SeparateEulerRadiansKeyframesUtil.CanInterpolateWithQuaternions(
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

    var fromX = SeparateEulerRadiansKeyframesUtil.GetFromValueOrDefault(xInterpolationType, fromXFrame, defaultX);
    var fromY = SeparateEulerRadiansKeyframesUtil.GetFromValueOrDefault(yInterpolationType, fromYFrame, defaultY);
    var fromZ = SeparateEulerRadiansKeyframesUtil.GetFromValueOrDefault(zInterpolationType, fromZFrame, defaultZ);

    if (SeparateEulerRadiansKeyframesUtil.GetFromAndToFrameIndex(
            fromsAndTos,
            areAxesStatic,
            out var fromFrame,
            out var toFrame)) {
      if (toFrame < fromFrame) {
        toFrame += sharedConfig.AnimationLength;
      }

      var frameDelta = (frame - fromFrame) / (toFrame - fromFrame);

      var q1 = this.ConvertRadiansToQuaternionImpl(fromX, fromY, fromZ);

      var toX = SeparateEulerRadiansKeyframesUtil.GetToValueOrDefault(xInterpolationType, toXFrame, defaultX);
      var toY = SeparateEulerRadiansKeyframesUtil.GetToValueOrDefault(yInterpolationType, toYFrame, defaultY);
      var toZ = SeparateEulerRadiansKeyframesUtil.GetToValueOrDefault(zInterpolationType, toZFrame, defaultZ);

      var q2 = this.ConvertRadiansToQuaternionImpl(toX, toY, toZ);

      if (Quaternion.Dot(q1, q2) < 0) {
        q2 = -q2;
      }

      value = Quaternion.Normalize(QuaternionUtil.Slerp(q1, q2, frameDelta));

      return true;
    }

    value = Quaternion.Normalize(
        this.ConvertRadiansToQuaternionImpl(fromX, fromY, fromZ));
    return true;
  }

  public ISeparateEulerRadiansKeyframes<TKeyframe>.ConvertRadiansToQuaternion
      ConvertRadiansToQuaternionImpl { get; set; } =
    QuaternionUtil.CreateZyxRadians;

  public IRadiansKeyframeInterpolator<TKeyframe> Interpolator => interpolator;

  public void GetAllFrames(Span<Quaternion> dst) {
    // TODO: Switch this to use a more optimized approach
    for (var i = 0; i < dst.Length; ++i) {
      this.TryGetAtFrameWithBinarySearch(i, out var rotation);
      dst[i] = rotation;
    }
  }

  // TODO: Implement this
  public bool TryGetSimpleKeyframes(
      out IReadOnlyList<(float frame, Quaternion value)> keyframes,
      out IReadOnlyList<(Quaternion tangentIn, Quaternion tangentOut)>? tangentKeyframes) {
    keyframes = null;
    tangentKeyframes = null;
    return false;
  }

  public IAdvancingInterpolator<Quaternion> GetAdvancingInterpolator()
    => new SeparateEulerRadiansAdvancingInterpolator<TKeyframe>(
        this,
        sharedConfig,
        individualConfigX,
        individualConfigY,
        individualConfigZ,
        this.Axes[0].AssertAsA<InterpolatedKeyframes<TKeyframe, float>>().Impl,
        this.Axes[1].AssertAsA<InterpolatedKeyframes<TKeyframe, float>>().Impl,
        this.Axes[2].AssertAsA<InterpolatedKeyframes<TKeyframe, float>>().Impl,
        interpolator);
}