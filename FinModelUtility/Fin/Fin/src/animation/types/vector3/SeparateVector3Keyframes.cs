using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

using fin.animation.interpolation;
using fin.animation.keyframes;
using fin.math.interpolation;
using fin.util.asserts;

using NoAlloq;

namespace fin.animation.types.vector3;

public sealed class SeparateVector3Keyframes<TKeyframe>(
    ISharedInterpolationConfig sharedConfig,
    IKeyframeInterpolator<TKeyframe, float> interpolator,
    IndividualInterpolationConfig<float>? individualConfigX,
    IndividualInterpolationConfig<float>? individualConfigY,
    IndividualInterpolationConfig<float>? individualConfigZ)
    : ISeparateVector3Keyframes<TKeyframe>
    where TKeyframe : IKeyframe<float> {
  public SeparateVector3Keyframes(
      ISharedInterpolationConfig sharedConfig,
      IKeyframeInterpolator<TKeyframe, float> interpolator,
      IndividualInterpolationConfig<float>? individualConfig = null)
      : this(sharedConfig,
             interpolator,
             individualConfig,
             individualConfig,
             individualConfig) { }

  public int AnimationLength => sharedConfig.AnimationLength;
  public bool Looping => sharedConfig.Looping;

  public IReadOnlyList<IInterpolatableKeyframes<TKeyframe, float>> Axes { get; }
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

  public bool TryGetAtFrameWithBinarySearch(float frame, out Vector3 value) {
    value = default;

    if (!this.Axes[0]
             .TryGetAtFrameOrDefault(frame, individualConfigX, out var x)) {
      return false;
    }

    if (!this.Axes[1]
             .TryGetAtFrameOrDefault(frame, individualConfigY, out var y)) {
      return false;
    }

    if (!this.Axes[2]
             .TryGetAtFrameOrDefault(frame, individualConfigZ, out var z)) {
      return false;
    }

    value = new Vector3(x, y, z);
    return true;
  }

  public void GetAllFrames(Span<Vector3> dst) {
    Span<float> x = stackalloc float[dst.Length];
    Span<float> y = stackalloc float[dst.Length];
    Span<float> z = stackalloc float[dst.Length];

    this.Axes[0].GetAllFrames(x);
    this.Axes[1].GetAllFrames(y);
    this.Axes[2].GetAllFrames(z);

    for (var i = 0; i < dst.Length; ++i) {
      dst[i] = new Vector3(x[i], y[i], z[i]);
    }
  }

  public bool TryGetSimpleKeyframes(
      out IReadOnlyList<(float frame, Vector3 value)> keyframes,
      out IReadOnlyList<(Vector3 tangentIn, Vector3 tangentOut)>?
          tangentKeyframes) {
    var xAxis = this.Axes[0];
    var yAxis = this.Axes[1];
    var zAxis = this.Axes[2];

    if (!xAxis.HasAnyData && !yAxis.HasAnyData && !zAxis.HasAnyData) {
      keyframes = [];
      tangentKeyframes = null;
      return true;
    }

    Span<(IInterpolatableKeyframes<TKeyframe, float> keyframes,
        IInterpolatableKeyframes<KeyframeWithTangents<float>, float>? keyframesWithTangents,
        IndividualInterpolationConfig<float>? config)> axes = [
        (xAxis,
         xAxis as IInterpolatableKeyframes<KeyframeWithTangents<float>, float>,
         individualConfigX),
        (yAxis,
         yAxis as IInterpolatableKeyframes<KeyframeWithTangents<float>, float>,
         individualConfigY),
        (zAxis,
         zAxis as IInterpolatableKeyframes<KeyframeWithTangents<float>, float>,
         individualConfigZ),
    ];

    if (axes.Any(a => !a.keyframes.HasAnyData &&
                      !(a.config?.DefaultValue?.Try(out _) ?? false))) {
      keyframes = null!;
      tangentKeyframes = null;
      return false;
    }

    var unionKeyframes
        = xAxis.Definitions
               .Concat(yAxis.Definitions)
               .Concat(zAxis.Definitions)
               .Select(keyframe => keyframe.Frame)
               .Distinct()
               .Order()
               .ToArray();

    var unionKeyframeCount = unionKeyframes.Length;

    {
      var mutableKeyframes = new (float, Vector3)[unionKeyframeCount];
      for (var k = 0; k < unionKeyframeCount; ++k) {
        var keyframe = unionKeyframes[k];
        var value = new Vector3();

        for (var i = 0; i < axes.Length; ++i) {
          var axis = axes[i];
          Asserts.True(
              axis.keyframes.TryGetAtFrameOrDefault(keyframe,
                                                    axis.config,
                                                    out var axisValue));
          value[i] = axisValue;
        }

        mutableKeyframes[k] = (keyframe, value);
      }

      keyframes = mutableKeyframes;
    }

    if (axes.Any(a => a.keyframesWithTangents == null)) {
      tangentKeyframes = null;
    } else {
      var mutableTangentKeyframes = new (Vector3, Vector3)[unionKeyframeCount];

      for (var k = 0; k < unionKeyframeCount; ++k) {
        var keyframe = unionKeyframes[k];

        var tangentIn = new Vector3();
        var tangentOut = new Vector3();

        for (var i = 0; i < axes.Length; ++i) {
          var axis = axes[i];
          if (!HermiteInterpolationUtil.TryGetTangent(
                  axis.keyframesWithTangents!,
                  keyframe,
                  out var axisTangentIn,
                  out var axisTangentOut)) {
            tangentKeyframes = null;
            return false;
          }

          tangentIn[i] = axisTangentIn;
          tangentOut[i] = axisTangentOut;
        }

        mutableTangentKeyframes[k] = (tangentIn, tangentOut);
      }

      tangentKeyframes = mutableTangentKeyframes;
    }

    return true;
  }
}